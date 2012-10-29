######################################################
##
##  Loko:
##
##  An implementation of an interpreter for a small
##  Logo dialect for the Mako VM.
##
##  John Earnest
##
######################################################

:include <Print.fs>
:include <String.fs>

:proto logo-print
:proto stacktrace

:array data-stack   200 0
:array return-stack 200 0

: nip  swap drop                ; ( a b -- b )
: min  2dup > if swap then drop ; ( a b -- min )
: max  2dup < if swap then drop ; ( a b -- max )

: repeat ( n proc -- )
	swap dup -if 2drop exit then 1 -
	for dup >r exec r> next drop
;

######################################################
##
##  Error Handling:
##
##  This is a global error handler which will be
##  patched into the Parser error handler and fired
##  by the BREAK keyboard interrupt, in addition
##  to syntax and runtime errors.
##
######################################################

:var restart-vector

: abort ( string -- )
	typeln
	data-stack   DP !
	return-stack RP !
	restart-vector @ exec
	halt
;

: brk ( -- )
	KB @ 3 = if "BREAK" abort then
;

######################################################
##
##  GC and List utilities:
##
######################################################

:const heap-size 65536
:include <Algorithms/Garbage.fs>

:const nil -2147483648
: nil?    nil xor -if true else false then         ; ( val -- flag )
: pair    2 alloc dup >r ptr> swap over 1 + ! ! r> ; ( first rest -- pair )
: first   ptr>     @                               ; ( pair -- first )
: rest    ptr> 1 + @                               ; ( pair --  rest )
: first!  ptr>     !                               ; ( pair -- first )
: rest!   ptr> 1 + !                               ; ( pair --  rest )
:  split  dup first swap  rest                     ; ( pair -- first rest )
: -split  dup  rest swap first                     ; ( pair -- rest first )

: managed-begin ;
:var   global-env
:var   env
:var   logo-t # true
:var   logo-f # false
:var   A
:var   B
: managed-end ;

: list-size ( list -- 0count )
	0 loop
		over nil? if nip break then
		swap rest swap 1 +
	again
;

: list-last ( ptr -- ptr )
	loop
		dup rest nil? if break then
		rest
	again
;

: list-cat ( tail val -- tail' )
	nil pair dup >r swap rest! r>
;

: list-insert ( prev val -- )
	over rest pair swap rest!
;

######################################################
##
##  Type System:
##
##  Values in Loko are always stored beginning with
##  a cell indicating the type. 'Normal' Forth values
##  can be boxed into Loko values and extracted
##  with the utility routines below.
##
##  The types VAR and CALL are not datatypes per se,
##  but rather alternate forms of WORD emitted by
##  the list parser to identify : and non-" prefixes,
##  respectively.
##
######################################################

:const KIND_FUNC 0 # function definition
:const KIND_VAR  1 # variable reference
:const KIND_CALL 2 # function invocation
:const KIND_LIST 3 # stored as a cons-pair chain
:const KIND_WORD 4 # stored as a flat null-terminated string
:const KIND_NUM  5 # stored as a tuple of word halves to avoid gc collisions

: kind!  p!               ; ( kind ptr -- )
: kind   p@               ; ( ptr -- kind )
: func?  kind KIND_FUNC = ; ( ptr -- flag )
: var?   kind KIND_VAR  = ; ( ptr -- flag )
: call?  kind KIND_CALL = ; ( ptr -- flag )
: list?  kind KIND_LIST = ; ( ptr -- flag )
: word?  kind KIND_WORD = ; ( ptr -- flag )
: num?   kind KIND_NUM  = ; ( ptr -- flag )

: word>  ptr> 1 + ; ( ptr -- str )

: >word ( str -- ptr )
	dup size 2 + alloc dup >r
	ptr> 1 + over size 1 + >move
	r> KIND_WORD over kind!
;

: >call  >word KIND_CALL over kind! ; ( str -- ptr )
: >var   >word KIND_VAR  over kind! ; ( str -- ptr )

: num> ( ptr -- n )
	ptr> dup
	1 + @ 4 *  swap
	2 + @      or
;

: >num ( n -- ptr )
	3 alloc >r dup
	ptr-mask and 4 /  i ptr> 1 + !
	ptr-bits and      i ptr> 2 + !
	r> KIND_NUM over kind!
;

: >list ( cons-list -- ptr )
	KIND_LIST swap pair
;

: word= ( p1 p2 -- flag )
	2dup = if 2drop true exit then
	word> swap word> -text 0 =
;

:proto logo=

: list= ( list list -- flag )
	loop
		rest swap rest
		over nil?  over nil?  and    if 2drop true  break then
		over nil?  over nil?  or     if 2drop false break then
		over first over first logo= -if 2drop false break then
	again
;

: logo= ( ptr ptr -- flag )
	2dup =                 if 2drop true  exit then
	over kind over kind = -if 2drop false exit then
	dup num?  if num> swap num> =         exit then
	dup list? if list=                    exit then
	word=
;

: .func-prim ptr> 1 + ; ( ptr -- addr ) # a boolean flag
: .func-code ptr> 2 + ; ( ptr -- addr ) # an address or a list of tokens
: .func-src  ptr> 3 + ; ( ptr -- addr ) # a cons-list of source text
: .func-args ptr> 4 + ; ( ptr -- addr ) # a cons-list of word names

: check-func ( word func -- func )
	dup nil? if
		drop "I don't know how to " type
		word> type "." abort
	then
	dup func? -if
		drop word> type
		" is not a procedure." abort
		halt
	then
	nip
;

: >prim ( proc arghead -- func )
	5 alloc >r
	KIND_FUNC i kind!
	true i .func-prim !
	i .func-args !
	i .func-code !
	r>
;

: >synth ( codehead arghead -- func )
	>prim false over .func-prim !
;

######################################################
##
##  Environment Structure:
##
##  A linked-list based data structure used to
##  represent variable scope and execution state.
##
######################################################

: .env-map       ptr>     ; ( env -- addr )
: .env-next      ptr> 1 + ; ( env -- addr )
: .env-cursor    ptr> 2 + ; ( env -- addr )
: .env-func      ptr> 3 + ; ( env -- addr )
: .env-continue  ptr> 4 + ; ( env -- addr )

: >env ( first rest -- ptr )
	5 alloc >r
	i .env-next !
	i .env-map  !
	nil i .env-cursor   !
	nil i .env-func     !
	nil i .env-continue !
	r>
;

: env-init ( -- )
	nil nil >env global-env !
	global-env @ env !
;

: envlist-find ( word root -- entry | nil )
	first loop
		dup nil? if nip break then
		2dup rest first word= if
			nip rest break
		then
		first
	again
;

: envlist-add ( val word root --  )
	>r swap pair
	i first swap pair
	r> first!
;

: env-push  nil env @ >env env ! ; ( -- )
: env-pop   env @ rest     env ! ; ( -- )

: env-get ( word -- val | nil )
	env @ loop
		dup nil? if nip break then
		2dup envlist-find
		dup nil? -if nip nip rest break then
		drop rest
	again
;

: env-make ( val word -- )
	dup env @ envlist-find dup nil? if
		drop env @ envlist-add
	else
		nip rest!
	then
;

: global-make ( val word -- )
	env @ >r global-env @ env !
	env-make r> env !
;

: cursor-next ( -- token | nil )
	env @ .env-cursor @
	dup nil? if exit then split
	env @ .env-cursor !
;

: tail-call? ( func -- env? flag )
	# never try to short-circuit tail calls in primitives:
	dup .func-prim @ if drop false exit then

	# we must be the last call in the current procedure:
	env @ .env-cursor @ nil? -if drop false exit then

	# we must find an identical func record
	# higher on the environment chain:
	env @ loop
		dup nil?           if 2drop false break then
		2dup .env-func @ = if nip   true  break then
		rest
	again
;

######################################################
##
##  Interpreter:
##
##  Walk along a token list and evaluate them as
##  prefix expressions and operands.
##
######################################################

:proto eval

: call ( ... a2 a1 a0 func -- ret? )
	brk
	dup .func-args @ loop
		dup nil? if drop break then
		dup first >r >r swap r> swap r>
		env-make
		rest
	again
	dup .func-prim @ if
		.func-code @ exec
	else
		.func-code @ eval
	then
;

: eval-token ( ptr -- )
	dup var?  if env-get then
	dup num?  if    exit then
	dup word? if    exit then
	dup list? if    exit then
	dup env-get check-func

	dup >r .func-args @ list-size {
		cursor-next
		eval-token
	} repeat

	r> dup tail-call? if
		dup .env-continue @ RP !
		env !
	else
		env-push
		dup  env @ .env-func     !
		RP @ env @ .env-continue !
	then
	call env-pop
;

: eval ( list -- )
	rest env @ .env-cursor !
	loop
		cursor-next
		dup nil? if drop break then
		eval-token
	again
;

######################################################
##
##  Parser:
##
##  Convert a source stream into a cons-structured
##  list of tokens, which will be largely comprised
##  of words, calls, var references and numbers.
##
######################################################

:include <Parse.fs>

:const quote 34 # "
:const tick  39 # '
:const minus 45 # -
:const colon 58 # :
:const open  91 # [
:const close 93 # ]

:proto parse

: parse-token ( -- token )
	open  curr = if skip trim parse   exit then
	tick  curr = if skip token> >word exit then
	colon curr = if skip token> >var  exit then
	name?        if      token> >call exit then
	numeral?     if number> >num      exit then
	"invalid token" typeln halt
;

:proto infix

: infix-unary ( tail -- tail' )
	"-" match? if "negate" >call list-cat infix-unary exit then
	"(" match? if loop infix ")" match? until         exit then
	parse-token list-cat
;

: infix-mul ( tail -- tail' )
	dup >r infix-unary
	"*" match? if r> "product"    >call list-insert infix-mul exit then
	"/" match? if r> "quotient"   >call list-insert infix-mul exit then
	"%" match? if r> "remainder"  >call list-insert infix-mul exit then
	rdrop
;

: infix-add ( tail -- tail' )
	dup >r infix-mul
	"+" match? if r> "sum"        >call list-insert infix-add exit then
	"-" match? if r> "difference" >call list-insert infix-add exit then
	rdrop
;

: infix ( tail -- tail' )
	dup >r infix-add
	">" match? if r> "greater"    >call list-insert infix     exit then
	"<" match? if r> "less"       >call list-insert infix     exit then
	"=" match? if r> "equal"      >call list-insert infix     exit then
	rdrop
;

: parse-to ( -- )
	token> >word
	nil loop
		colon curr = -if break then
		skip token> >word swap pair
	again
	parse swap >synth
	swap global-make
;

: parse ( -- list )
	KIND_LIST nil pair dup
	loop
		( head tail )
		"to"  match? if parse-to        then
		"end" match? if           break then
		eof?         if           break then
		close curr = if skip trim break then
		infix-unary
	again
	drop
;

######################################################
##
##  Primitives:
##
##  All primitive functions must have function
##  records with arglists loaded into the global
##  environment. The following are routines for
##  building function records in Forth and the
##  Logo language primitives themselves.
##
######################################################

: [ nil ;
: ]-prim ( proc name nil ... args -- )
	nil nil pair dup >r
	loop
		over nil? if 2drop r> rest break then
		swap @ nil pair swap over swap rest!
	again
	>r swap r> >prim
	swap >word global-make
;
 
: n      @ env-get num>               ; ( addr -- int )
: v      @ env-get                    ; ( addr -- val )
: true?  logo-t @ word=               ; ( word -- flag )
: >bool  if logo-t else logo-f then @ ; ( flag -- ptr )

: logo-print ( val -- )
	dup num?  if          num>  .num exit then
	dup call? if          word> type exit then
	dup word? if "'" type word> type exit then
	dup var?  if ":" type word> type exit then
	dup list? if
		"[ " type rest loop
			dup nil? if drop break then
			-split logo-print space
		again
		"]" type exit
	then
	dup func? if
		"lambda " type
		dup .func-args @ >list logo-print space
		dup .func-prim @ if
			"primitive @" type .func-code @ .
		else
			.func-code @ logo-print
		then
		exit
	then
	.num "?" type
;

: logo-member ( val list -- list' )
	rest loop
		dup nil? if 2drop nil break then
		2dup first logo= if nip break then
		rest
	again >list
;

: logo-last ( list -- val )
	dup rest nil? if drop nil >list exit then
	rest loop
		dup rest nil? if first break then
		rest
	again
;

: logo-butlast ( list -- list' )
	dup rest nil? if drop nil >list exit then
	rest nil >list dup >r loop
		over rest nil? if 2drop break then
		swap split >r nil pair
		over rest! rest r> swap
	again
	r>
;

: logo-lput ( val list -- list' )
	rest nil >list dup >r loop
		over nil? if nip swap nil pair swap rest! break then
		swap split >r nil pair
		over rest! rest r> swap
	again
	r>
;

: logo-stop ( -- )
	# escape to the parent environment
	# of the closest synthetic function:
	env @ loop
		dup .env-func @ nil? -if
			dup .env-func @ .func-prim @ -if
				dup .env-continue @ RP !
				rest env !
				break
			then
		then
		rest
	again
;

:include <Sprites.fs>
:include "Turtle.fs"

: wait ( -- )
	sync sync
;

: pos ( -- list )
	posx >num posy >num nil pair pair >list
;

: setpos ( list -- )
	rest split first num> posy! num> posx!
;

: setcolor ( int -- )
	256 mod 256 * 0xFF000000 or linecolor !
;

: prims-init ( -- )
	"arg1"  >word A !
	"arg2"  >word B !
	"true"  >word logo-t !
	"false" >word logo-f !

	# input and output
	{ A v logo-print cr           } "print"      [ A   ]-prim
	{ B v A v global-make         } "make"       [ A B ]-prim
	{ B v A v env-make            } "local"      [ A B ]-prim
	{     logo-stop               } "stop"       [     ]-prim
	{ A v logo-stop               } "output"     [ A   ]-prim

	# math and flow control
	{ A n B n +   >num            } "sum"        [ A B ]-prim
	{ A n B n -   >num            } "difference" [ A B ]-prim
	{ A n B n *   >num            } "product"    [ A B ]-prim
	{ A n B n /   >num            } "quotient"   [ A B ]-prim
	{ A n B n mod >num            } "remainder"  [ A B ]-prim
	{ RN @ A n mod >num           } "random"     [ A   ]-prim
	{ A v true?  if B v eval then } "if"         [ A B ]-prim
	{ A v true? -if B v eval then } "unless"     [ A B ]-prim
	{ A n { B v eval } repeat     } "repeat"     [ A B ]-prim
	{ A v eval                    } "run"        [ A   ]-prim
	{ A n B n < >bool             } "less"       [ A B ]-prim
	{ A n B n > >bool             } "greater"    [ A B ]-prim
	{ A n -1 * >num               } "negate"     [ A   ]-prim

	# list manipulation
	{ A v rest first              } "first"      [ A   ]-prim
	{ A v rest rest >list         } "butfirst"   [ A   ]-prim
	{ A v B v rest pair >list     } "fput"       [ A B ]-prim
	{ A v B v nil pair pair >list } "list"       [ A B ]-prim
	{ B v A n for rest next first } "item"       [ A B ]-prim
	{ A v B v logo= >bool         } "equal"      [ A B ]-prim
	{ A v B v logo-member         } "member"     [ A B ]-prim
	{ A v rest list-size >num     } "size"       [ A   ]-prim
	{ A v logo-last               } "last"       [ A   ]-prim
	{ A v logo-butlast            } "butlast"    [ A   ]-prim
	{ A v B v logo-lput           } "lput"       [ A B ]-prim

	# turtle graphics
	{ A n      draw   wait        } "forward"    [ A   ]-prim
	{ A n -1 * draw   wait        } "back"       [ A   ]-prim
	{ A n      angle+             } "right"      [ A   ]-prim
	{ A n -1 * angle+             } "left"       [ A   ]-prim
	{ home            wait        } "home"       [     ]-prim
	{ clearscreen                 } "clear"      [     ]-prim
	{ A n angle!                  } "setheading" [ A   ]-prim
	{ A n posx!       wait        } "setx"       [ A   ]-prim
	{ A n posy!       wait        } "sety"       [ A   ]-prim
	{ angle >num                  } "heading"    [     ]-prim
	{ posx  >num                  } "xcor"       [     ]-prim
	{ posy  >num                  } "ycor"       [     ]-prim
	{ pos                         } "pos"        [     ]-prim
	{ A v setpos      wait        } "setpos"     [ A   ]-prim
	{ showturtle                  } "showturtle" [     ]-prim
	{ hideturtle                  } "hideturtle" [     ]-prim
	{ false pen !                 } "pendown"    [     ]-prim
	{ true  pen !                 } "penup"      [     ]-prim
	{ A n setcolor                } "setcolor"   [ A   ]-prim
;

######################################################
##
##  Editor/Virtual Console
##
######################################################

:include <Grid.fs>
:image grid-tiles "text.png" 8 8

:const cursor-s 129
:var   used
:var   cursor
:var   cx
:var   lines
:data  cc -32
:array input 41 0

: plain-text -32 cc ! ;
: code-text   64 cc ! ;

: console-newline
	lines inc
	lines @ 28 > if
		0 lines !
		64 ascii !
		0 29 "[More...]" grid-type
		loop keys key-a and brk sync until
		loop keys key-a and brk sync while
		loop KB @ -1 = until
		0 29 "         " grid-type
	then
	0 cx !
	27 for
		0 28 i - tile-grid@
		0 27 i - tile-grid@
		40 >move
	next
	0 28 tile-grid@ 40 0 fill
	sync
;

: console-emit ( char -- )
	dup 10 = if
		console-newline
		drop exit
	else
		# capture ludicrous characters
		# and turn 'em into happy little boxes.
		dup 0 < over 128 > or if drop 94 then

		cc @ + cx @ 28 tile-grid@ !
		cx inc cx @ 39 >
		if console-newline then
	then
;

: input-clear ( -- )
	input 41 0 fill
	0 cursor !
	0 used   !
;

: refresh-grid ( -- )
	28 for
		39 for
			i j tile-grid@ dup @
			grid-z not and 27 j - lines @ <= if
				grid-z or
			then
			swap !
		next
	next
	39 for
		i used @ >= if 0 else i input + @ 32 - then
		96 + grid-z or i 29 tile-grid@ !
	next
	8x8 191 cursor @ 8 * 232 cursor-s >sprite
;

: readline ( string? flag -- str )
	input-clear
	if
		dup size used !
		input over size >move
	then
	cursor-s show
	loop
		loop
			KB @ dup -1 = if drop break then
			dup 3 = if
				drop
				cursor-s hide
				input-clear
				"BREAK" abort
			then
			dup 10 = if
				# return
				drop
				code-text input typeln plain-text
				0 lines !
				input >word word>
				input-clear
				refresh-grid
				cursor-s hide
				exit
			else
				dup 8 = if
					drop
					# backspace
					cursor @ 0 > if
						cursor @ input + dup 1 -
						used @ cursor @ - 1 + <move
						cursor dec@ used dec@
					then
				else
					# insert
					used @ 40 >= if drop else
						cursor @ input + dup dup 1 +
						used @ cursor @ - >move !
						cursor inc@ used inc@
					then
				then
			then
		again

		keys key-rt and if cursor @ 1 + used @ min cursor ! sync sync then
		keys key-lf and if cursor @ 1 -      0 max cursor ! sync sync then

		refresh-grid

		sync sync
	again
;

######################################################
##
##  Stack Trace Utility:
##
######################################################

: envlist-findval ( word root -- entry | nil )
	first loop
		dup nil? if nip break then
		2dup rest rest = if
			nip rest break
		then
		first
	again
;

: func-name ( func -- word | nil )
	global-env @ loop
		dup nil? if nip break then
		2dup envlist-findval
		dup nil? -if nip nip first break then
		drop rest
	again
;

: .args ( env func -- env )
	.func-args @ loop
		dup nil? if drop break then
		dup first dup
		logo-print " -> " type
		>r over r> swap
		envlist-find rest logo-print space 
		rest
	again
;

: stacktrace ( -- )
	"stack trace:" typeln
	env @ loop
		dup nil? if drop break then
		dup .env-func @ nil? -if
			dup .env-func @ dup func-name
			( func word? )
			dup nil? if drop dup then
			tab logo-print
			# print the arglist
			tab .args cr
		else
			tab "[]" typeln
		then
		rest
	again
	cr
;

######################################################
##
##  Entrypoint and REPL:
##
######################################################

: run  >read parse eval ; ( str -- )

: repl ( -- )
	global-env @ env !
	loop
		false readline run
	again
;

: main ( -- )
	gc-init
	env-init
	prims-init
	clearcolor CL !
	showturtle
	hideturtle

	1 1 "Welcome to Loko" grid-type
	1 2 "128k OK"         grid-type

	' console-emit ' emit revector
	' abort        ' fail revector
	' repl          restart-vector !
	#repl

	"to f :x
		if (:x = 0) [ forward 7 stop ]
		f (:x - 1) left  120
		g (:x - 1) right 120
		f (:x - 1) right 120
		g (:x - 1) left  120
		f (:x - 1)
	end" run

	"to g :x
		if (:x = 0) [ forward 7 stop ]
		g (:x - 1)
		g (:x - 1)
	end" run

	"to sierpinski
		showturtle
		repeat 6 [
			f 4 left 120
			g 4 left 120
			g 4 left  60
		]
	end" run

	"to sq repeat 4 [ forward 100 right 90 ] end" run
	"to spin sq right 10 end"                     run
	"showturtle repeat 36 [spin]"                 run
	"sierpinski" run
	
	"" abort
;