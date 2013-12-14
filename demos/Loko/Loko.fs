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

:const stack-size    200
:array data-stack    stack-size 0
:array data-padding          10 0
:array return-stack  stack-size 0
:array return-padding        10 0

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

: /0? ( n -- n )
	dup 0 = if "Cannot divide by zero." abort then
;

: void ( val? flag -- )
	if
		"I don't know what to do with " type
		logo-print "." abort
	then
;

: noargs ( word -- )
	"Not enough arguments for " type 
	logo-print "!" abort
;

: checkstacks ( -- )
	RP @ return-stack stack-size + 10 - >=
	DP @   data-stack stack-size + 10 - >=
	or if "Stack overflow!" abort then
;

: heap-empty ( -- )
	"Heap memory exhausted!" abort
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
: first!  ptr>     !                               ; ( first pair -- )
: rest!   ptr> 1 + !                               ; ( rest pair -- )
:  split  dup first swap  rest                     ; ( pair -- first rest )
: -split  dup  rest swap first                     ; ( pair -- rest first )

: free-space ( -- words )
	from @ heap-size + head @ -
;

: managed-begin ;
:var   global-env
:var   env
:var   logo-t # true
:var   logo-f # false
:var   A
:var   B
:data  last-text nil # pointer to word containing last entered proc def
:var   last-word     # used in eval
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

: list-reverse ( list -- tsil )
	nil swap loop
		dup nil? if drop break then
		split >r swap pair r>
	again
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
##  the list parser to identify : and non-' prefixes,
##  respectively.
##
######################################################

:const KIND_VAR  1 # variable reference
:const KIND_CALL 2 # function invocation
:const KIND_LIST 3 # stored as a cons-pair chain
:const KIND_WORD 4 # stored as a flat null-terminated string
:const KIND_NUM  5 # stored as a tuple of word halves to avoid gc collisions

: kind!  p!               ; ( kind ptr -- )
: kind   p@               ; ( ptr -- kind )
: var?   kind KIND_VAR  = ; ( ptr -- flag )
: call?  kind KIND_CALL = ; ( ptr -- flag )
: list?  kind KIND_LIST = ; ( ptr -- flag )
: word?  kind KIND_WORD = ; ( ptr -- flag )
: num?   kind KIND_NUM  = ; ( ptr -- flag )

: >word ( str -- ptr )
	dup size 2 + alloc dup >r
	ptr> 1 + over size 1 + >move
	r> KIND_WORD over kind!
;

: >call  >word KIND_CALL over kind! ; ( str -- ptr )
: >var   >word KIND_VAR  over kind! ; ( str -- ptr )

: num> ( ptr -- n )
	dup num? -if logo-print " is not a number." abort then
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

: .list-data ptr> 1 + ; ( ptr -- addr ) # a cons-list of tokens
: .list-args ptr> 2 + ; ( ptr -- addr ) # a cons-list of word names
: .list-text ptr> 3 + ; ( ptr -- addr ) # nil or a source string

: prim? ( func -- flag )
	.list-data @ dup
	nil? if drop false exit then
	p? not
;

: >list ( cons-list -- ptr )
	4 alloc >r
	KIND_LIST i kind!
	nil i .list-args !
	nil i .list-text !
	    i .list-data !
	r>
;

: >func ( code args -- ptr )
	swap >list
	swap        over .list-args !
	last-text @ over .list-text !
;

: word> ( ptr -- str )
	#dup word? -if logo-print " is not a word." abort then
	ptr> 1 +
;

: list> ( ptr -- cons-list )
	dup list? -if logo-print " is not a list." abort then
	.list-data @
;

: args> ( ptr -- list )
	dup list? -if logo-print " is not a list." abort then
	.list-args @ list-reverse >list
;

: fixed-list? ( v count -- v )
	over list> list-size =
	-if "Supplied list is not the correct size." abort then
;

: word= ( p1 p2 -- flag )
	2dup = if 2drop true exit then
	word> swap word> -text 0 =
;

:proto logo=

: list= ( list list -- flag )
	list> swap list>
	loop
		over nil?  over nil?  and    if 2drop true  break then
		over nil?  over nil?  or     if 2drop false break then
		over first over first logo= -if 2drop false break then
		rest swap rest
	again
;

: logo= ( ptr ptr -- flag )
	2dup =    if 2drop true exit then # exit early on the cheap case
	dup list? if over list? -if 2drop false exit then list=            exit then
	dup num?  if over num?  -if 2drop false exit then num> swap num> = exit then
	word=
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

: env-get ( word -- val )
	env @ loop
		dup nil? if break then
		2dup envlist-find
		dup nil? -if nip rest break then
		drop rest
	again
	dup nil? if drop logo-print " has no value." abort then
	nip
;

: env-set ( val word -- )
	env @ loop
		( val word env-head )
		dup nil? if drop break then
		2dup envlist-find dup nil? -if
			( val word env-head entry )
			nip nip rest! exit
		then
		drop rest
	again
	global-env @ envlist-add
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

: tail-call? ( list tail-flag -- env? flag )
	# if we aren't eligible for a tail call, quit:
	dup -if nip exit then drop

	# never try to short-circuit tail calls in primitives:
	dup prim? if drop false exit then

	# the current statement must be fully evaluated:
	env @ .env-cursor @ nil? -if drop false exit then

	# we must find an identical func record
	# higher on the environment chain:
	env @ 2dup .env-func @ = if nip true else 2drop false then
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
:proto eval-token

: call ( ... a2 a1 a0 list -- val? flag )
	brk
	dup .list-args @ loop
		dup nil? if drop break then
		dup first >r >r swap r> swap r>
		env-make
		rest
	again
	dup prim? if
		list> exec
	else
		eval
	then
;

: eval-func ( list tail-flag -- val? flag )
	>r checkstacks
	dup list? -if logo-print " is not a procedure." abort then
	dup >r .list-args @ list-size {
		cursor-next
		dup nil?    if last-word @ noargs then

		# if the previous word is 'output',
		# this call to eval-token could be tail:
		last-word @ word> "output" -text 0 =

		eval-token -if last-word @ noargs then
	} repeat
	r> dup r> tail-call? if
		dup .env-continue @ RP !
		env !
	else
		env-push
		dup  env @ .env-func     !
		RP @ env @ .env-continue !
	then
	call env-pop
;

: eval-token ( ptr tail-flag -- val? flag )
	over var?  if
		swap env-get swap
		over call? if drop true exit then
		over var?  if drop true exit then
	then
	over num?  if drop true exit then
	over word? if drop true exit then
	over list? if drop true exit then
	last-word @ >r
	over last-word !
	swap env-get swap
	eval-func
	r> last-word !
;

: eval ( list -- val? flag )
	list> env @ .env-cursor !
	loop
		cursor-next
		dup nil? if drop break then
		true eval-token void
	again
	false
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

:const quote  34 # "
:const tick   39 # '
:const minus  45 # -
:const colon  58 # :
:const open   91 # [
:const close  93 # ]
:const period 46 # .
:const comma  44 # ,
:const what   63 # ?
:const bang   33 # !

: tokenchar? ( -- flag )
	name?
	curr period = or
	curr comma  = or
	curr bang   = or
	curr what   = or
;

: token> ( -- string )
	tokenchar? -if "Name expected." fail then
	{ tokenchar? numeral? or } accept>
;

: parse-comment ( -- )
	";" match? if
		loop skip eof? newline? or until
		skip trim
	then
;

:proto parse-in

: parse-token ( -- token )
	parse-comment
	open  curr = if skip trim parse-in exit then
	tick  curr = if skip token> >word  exit then
	colon curr = if skip token> >var   exit then
	tokenchar?   if      token> >call  exit then
	numeral?     if number> >num       exit then
	eof? if "Missing ')' ?" abort then
	"The character '" type curr emit "' is not valid." abort
;

:proto infix

: infix-unary ( tail -- tail' )
	signed?    if signed>  >num  list-cat             exit then
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
	">" match? if r> "greater?"   >call list-insert infix     exit then
	"<" match? if r> "less?"      >call list-insert infix     exit then
	"=" match? if r> "equal?"     >call list-insert infix     exit then
	rdrop
;

: parse-in ( -- list )
	nil >list dup
	loop
		parse-comment
		eof?       if break then
		"]" match? if break then
		infix-unary
	again
	drop
;

: parse ( -- list )
	trim "to " match? if
		token> >word
		nil loop
			colon curr = -if break then
			skip token> >word swap pair
		again
		nil >list dup loop
			parse-comment
			eof? if "'end' expected!" abort then
			"end" match? if break then
			infix-unary
		again drop
		list> swap >func swap global-make
		nil >list
		exit
	else
		parse-in
	then
;

######################################################
##
##  Import/Export:
##
######################################################

: xo-typeln ( str -- )
	loop
		dup @ dup
		if XO ! else 2drop 10 XO ! exit then
		1 +
	again
;

:proto logo-printraw
: logo-export ( filename -- )
	word> XA ! x-open-write XS !
	global-env @ first loop
		dup nil? if drop break then
		dup rest rest .list-text @
		dup nil? -if
			"exporting " type
			over rest first logo-printraw
			"..." typeln
			word> xo-typeln 10 XO !
		else
			drop
		then
		first
	again
	x-close XS !
	"export successful!" typeln
;

: xo-read ( -- )
	clear-q
	{ XO @ } ' read revector
;

:array import-buffer 2048 0

: xo-readto ( -- )
	import-buffer loop
		( dest )
		"to" starts? if break then
		skip
	again
	1 for getc over ! 1 + next
	0 loop
		( dest brackets )
		dup -if "end" starts? if break then then
		curr open  = if 1 + then
		curr close = if 1 - then
		swap getc over ! 1 + swap
	again
	drop
	( dest )
	2 for getc over ! 1 + next
	0 swap !
;

: logo-import ( filename -- )
	# parse and load pass:
	word> dup XA !
	x-open-read XS !

	XA @ -1 = if
		"Couldn't find input file!" typeln
		drop exit
	then
	xo-read
	loop
		parse eval void
		eof?
	until
	x-close XS !

	# pseudoparse and stash source pass:
	XA ! x-open-read XS ! xo-read
	loop
		xo-readto
		import-buffer >word                           # copy buffer to the heap
		import-buffer >read "to" expect token> >word  # parse the buffer for a word name
		"importing " type dup word> type "..." typeln # print word name
		env-get .list-text !                          # stash the heap copy as the word's source text
		xo-read trim eof?
	until
	x-close XS !
	"import successful!" typeln
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
	>r swap r> >func
	swap >word global-make
;
 
: n      @ env-get num>               ; ( addr -- int )
: v      @ env-get                    ; ( addr -- val )
: true?  logo-t @ word=               ; ( word -- flag )
: >bool  if logo-t else logo-f then @ ; ( flag -- ptr )

: logo-print ( val -- )
	dup num?  if          num>  .num exit then
	dup call? if          word> type exit then
	dup var?  if ":" type word> type exit then
	dup word? if "'" type word> type exit then
	dup list? if
		dup prim? if
			"primitive @" type list> .
		else
			"[" type list> loop
				dup nil? if drop break then
				-split logo-print
				dup nil? if drop break then
				space
			again
			"]" type
		then exit
	then
	dup p? if
		"@" type ptr> .num
	else
		.num "?" type
	then
;

: logo-flatten ( list -- 'list )
	list> nil >list dup >r
	loop
		over nil? if 2drop break then
		over first dup list? if
			logo-flatten list>
			dup nil? -if
				swap over swap rest!
				list-last
			else
				drop
			then
		else
			nil pair
			swap over swap rest!
		then
		swap rest swap
	again
	r>
;

: logo-printraw ( val -- )
	dup num?  if num> .num exit then
	dup call? if word> type exit then
	dup var?  if word> type exit then
	dup word? if word> type exit then
	dup prim? if "p" type list> . exit then
	logo-flatten
	list> loop
		dup nil? if drop break then
		-split logo-printraw space
	again
;

: logo-word? ( ptr -- flag )
	dup word? over call? or swap var? or
;

: logo-item ( list index -- val )
	dup 0 < if 2drop nil >list exit then
	dup 0 > if
		swap list> swap
		1 - for
			dup nil? if rdrop >list exit then
			rest
		next
	else
		drop list>
	then
	dup nil? if >list else first then
;

: logo-member ( val list -- list' )
	list> loop
		dup nil? if 2drop nil break then
		2dup first logo= if nip break then
		rest
	again >list
;

: logo-last ( list -- val )
	dup list> nil? if drop nil >list exit then
	list> loop
		dup rest nil? if first break then
		rest
	again
;

: logo-butlast ( list -- list' )
	dup list> nil? if drop nil >list exit then
	list> nil >list dup >r loop
		over rest nil? if 2drop break then
		swap split >r nil pair
		over rest! rest r> swap
	again
	r>
;

: logo-lput ( val list -- list' )
	list> nil >list dup >r loop
		over nil? if nip swap nil pair swap rest! break then
		swap split >r nil pair
		over rest! rest r> swap
	again
	r>
;

: logo-first ( list -- val )
	list> dup nil? if >list exit then
	first
;

: logo-butfirst ( list -- val )
	list> dup nil? if >list exit then
	rest >list
;

: logo-stop ( -- )
	# escape to the parent environment
	# of the closest synthetic function:
	env @ loop
		dup nil? if
			"I'm not running a procedure!" abort
		then
		dup .env-func @ nil? -if
			dup .env-func @ prim? -if
				dup .env-continue @ RP !
				rest env !
				break
			then
		then
		rest
	again
;

: logo-bind ( arglist body -- list )
	list> swap list> list-reverse >func
;

:proto plain-text
:proto code-text

: logo-words ( -- )
	"global definitions: " typeln cr
	global-env @ first loop
		dup nil? if drop break then
		dup rest split
		dup list? if
			.list-text @ nil? -if code-text then
		else drop then
		logo-printraw plain-text
		space
		first
	again
	cr
;

: logo-local ( name value -- )
	env @ >r
	env-pop
	env-make
	r> env !
;

:proto edit
:proto showturtle
:proto hideturtle
:ref turtlemode

: def-stub ( string -- )
	# create a global stub linked to the string.
	# this ensures that definitions containing
	# syntax errors don't have to be totally re-entered.
	dup >read "to" expect >word
	nil >list dup >r .list-text ! r>
	token> >word global-make
;

: logo-edit ( word -- )
	dup word? -if "Only words can be edited." abort then
	dup env-get
	dup .list-text @
	dup nil? if
		2drop logo-print " cannot be edited." abort
	else
		>r 2drop r>
	then
	turtlemode @ dup >r
	if hideturtle then
	word> edit
	dup def-stub
	dup typeln
	dup >word last-text !
	>read parse eval void
	nil       last-text !
	r> if showturtle then
;

:include <Sprites.fs>
:include "Turtle.fs"

: wait      sync sync                               ; ( -- )
: pos       posx >num posy >num nil pair pair >list ; ( -- list )

: setpos ( list -- )
	2 fixed-list? list>
	split first
	num> posy! num> posx!
;

: setcolor ( list -- )
	3 fixed-list?
	list> split split first
	num> 0xFF and            swap # b
	num> 0xFF and   256 * or swap # g
	num> 0xFF and 65536 * or      # r
	0xFF000000 or linecolor !
;

:proto readline

: prims-init ( -- )
	"a1"    >word A !
	"a2"    >word B !
	"true"  >word logo-t !
	"false" >word logo-f !

	# misc
	{ A v logo-printraw cr             false } "print"      [ A   ]-prim
	{ A v logo-print    cr             false } "printlist"  [ A   ]-prim
	{ false readline >read parse-in    true  } "readlist"   [     ]-prim
	{ B v A v env-set                  false } "make"       [ A B ]-prim
	{ B v A v logo-local               false } "local"      [ A B ]-prim
	{     false logo-stop                    } "stop"       [     ]-prim
	{ A v true  logo-stop                    } "output"     [ A   ]-prim
	{ A v B v logo-bind                true  } "bind"       [ A B ]-prim
	{ A v args>                        true  } "args"       [ A   ]-prim
	{ A v env-get                      true  } "thing"      [ A   ]-prim
	{ logo-words                       false } "words"      [     ]-prim
	{ gc free-space . "cells" typeln   false } "free"       [     ]-prim
	{ A v logo-export                  false } "export"     [ A   ]-prim
	{ A v logo-import                  false } "import"     [ A   ]-prim
	{ :proto showversion showversion   false } "version"    [     ]-prim
	{ A v logo-edit                    false } "edit"       [ A   ]-prim
	{ stacktrace                       false } "trace"      [     ]-prim
	{ A v true?  if B v eval void then false } "if"         [ A B ]-prim
	{ A v true? -if B v eval void then false } "unless"     [ A B ]-prim
	{ A n { B v eval void } repeat     false } "repeat"     [ A B ]-prim
	{ A v env-pop false eval-func env-push   } "run"        [ A   ]-prim

	# math
	{ A n B n +        >num       true } "sum"        [ A B ]-prim
	{ A n B n -        >num       true } "difference" [ A B ]-prim
	{ A n B n *        >num       true } "product"    [ A B ]-prim
	{ A n B n  /0? /   >num       true } "quotient"   [ A B ]-prim
	{ A n B n  /0? mod >num       true } "remainder"  [ A B ]-prim
	{ RN @ A n /0? mod >num       true } "random"     [ A   ]-prim
	{ A v B v logo= >bool         true } "equal?"     [ A B ]-prim
	{ A n B n < >bool             true } "less?"      [ A B ]-prim
	{ A n B n > >bool             true } "greater?"   [ A B ]-prim
	{ A n -1 * >num               true } "negate"     [ A   ]-prim

	# type predicates
	{ A v logo-word?      >bool   true } "word?"      [ A   ]-prim
	{ A v list?           >bool   true } "list?"      [ A   ]-prim
	{ A v  num?           >bool   true } "num?"       [ A   ]-prim
	{ A v nil >list logo= >bool   true } "empty?"     [ A   ]-prim

	# list manipulation
	{ A v logo-first              true } "first"      [ A   ]-prim
	{ A v logo-butfirst           true } "butfirst"   [ A   ]-prim
	{ A v B v list> pair >list    true } "fput"       [ A B ]-prim
	{ A v B v nil pair pair >list true } "list"       [ A B ]-prim
	{ B v A n logo-item           true } "item"       [ A B ]-prim
	{ A v B v logo-member         true } "member"     [ A B ]-prim
	{ A v list> list-size >num    true } "size"       [ A   ]-prim
	{ A v logo-last               true } "last"       [ A   ]-prim
	{ A v logo-butlast            true } "butlast"    [ A   ]-prim
	{ A v B v logo-lput           true } "lput"       [ A B ]-prim
	{ A v logo-flatten            true } "flatten"    [ A   ]-prim

	# turtle graphics
	{ A n      draw   wait       false } "forward"    [ A   ]-prim
	{ A n -1 * draw   wait       false } "back"       [ A   ]-prim
	{ A n      angle+            false } "right"      [ A   ]-prim
	{ A n -1 * angle+            false } "left"       [ A   ]-prim
	{ home            wait       false } "home"       [     ]-prim
	{ clearscreen                false } "clear"      [     ]-prim
	{ A n angle!                 false } "setheading" [ A   ]-prim
	{ A n posx!       wait       false } "setx"       [ A   ]-prim
	{ A n posy!       wait       false } "sety"       [ A   ]-prim
	{ angle >num                 true  } "heading"    [     ]-prim
	{ posx  >num                 true  } "xcor"       [     ]-prim
	{ posy  >num                 true  } "ycor"       [     ]-prim
	{ pos                        true  } "pos"        [     ]-prim
	{ A v setpos      wait       false } "setpos"     [ A   ]-prim
	{ showturtle                 false } "showturtle" [     ]-prim
	{ hideturtle                 false } "hideturtle" [     ]-prim
	{ true  pen !                false } "pendown"    [     ]-prim
	{ false pen !                false } "penup"      [     ]-prim
	{ A v setcolor               false } "setcolor"   [ A   ]-prim
	{ A n B n lineto             false } "lineto"     [ A B ]-prim
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
		96 + i 29 tile-grid@ !
	next
	8x8 1792 cursor @ 8 * 232 cursor-s >sprite
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


# A much nicer multiline editor for working
# with larger procedure definitions:

:include "Editor.fs"

######################################################
##
##  Stack Trace Utility:
##
######################################################

: envlist-findval ( val root -- entry | nil )
	first loop
		dup nil? if nip break then
		2dup rest rest = if
			nip rest break
		then
		first
	again
;

: func-name ( func -- word | nil )
	env @ loop
		dup nil? if break then
		2dup envlist-findval
		dup nil? -if nip first break then
		drop rest
	again
	nip
;

: .args ( env func -- env )
	.list-args @ loop
		dup nil? if drop break then
		space emit
		dup first dup
		word> type "=" type
		>r over r> swap
		envlist-find rest logo-print
		rest
	again
;

: stacktrace ( -- )
	"stack trace:" typeln
	env @ loop
		dup nil? if drop break then
		dup .env-func @ nil? -if
			dup .env-func @ dup func-name
			dup nil? if
				drop " (?)"
			else
				space emit word>
			then
			type .args cr
		else
			tab "[]" typeln
		then
		rest
	again
	cr
;

######################################################
##
##  Title Screen
##
######################################################

:image title-tiles "tiles.png" 8 8
:data title-grid
	 0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0 
	 0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0 
	 0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0 
	 0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0 
	 0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0 
	 0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0 
	 0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0 
	 0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0 
	 0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0 
	 0  0  0  0  0  1  0  0  0  0  0  0  0  2  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0 
	 0  0  0  0  0  3  4  0  0  0  0  0  5  6  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0 
	 0  0  0  0  0  3  7  4  0  0  0  5  8  6  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0 
	 0  0  0  0  0  3  7  7  4  0  5  8  8  6  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0 
	 0  0  0  0  0  3  7  7  9  0 10  8  8  6  0  0 11 12 13 14  7  0 15 16 16 16 16 17  0 11 12 18 19  0  0  0  0  0  0  0 
	 0  0  0  0  0  3  7  7  9  0 10  8  8  6  0 20  8  8  8  8 21  0 10  8  8  8 22  0 20  8  8  8  8 23  0  0  0  0  0  0 
	 0  0  0  0  0  3  7  7  9  0 10  8  8  6  0 24  8 25 26  8 27  0 10  8  8 22  0  0 24  8 25 26  8 28  0  0  0  0  0  0 
	 0  0  0  0  0  3  7  7  9  0 10  8  8  6  0 29  8 30 31  8 32  0 10  8  8 33  0  0 29  8 30 31  8 34  0  0  0  0  0  0 
	 0  0  0  0  0  3  7  7  9  0 10  8  8  6  0 35  8  8  8  8 36  0 10  8  8  8 33  0 35  8  8  8  8 37  0  0  0  0  0  0 
	 0  0  0  0  0 38 39 39 40  0 41 42 42 43  0  0 44 45 46 47  7  0 41 42 42 42 42 48  0 44 45 49 50  0  0  0  0  0  0  0 
	 0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0 
	 0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0 
	 0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0 
	 0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0 
	 0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0 
	 0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0 
	 0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0 
	 0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0 
	 0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0 
	 0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0 
	 0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0 
	 0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0 

: intro ( -- )
	CL @ GT @ GP @ -1 GS !
	title-tiles GT !
	title-grid  GP !
	0xFF00FF00  CL !
	56 for
		i 4 * 26 + 256 *
		0xFF000000 or CL ! sync		
	next
	59 for sync next
	0 GS ! GP ! GT ! CL !
;

######################################################
##
##  Entrypoint and REPL:
##
######################################################

:array glue-buffer 80 0

: repl ( -- )
	global-env @ env !
	loop
		false readline
		dup >read
		"to " starts? if
			turtlemode @ dup >r
			if hideturtle then
			dup glue-buffer over size >move
			size glue-buffer +
			return over !  1 +
			return over !  1 +
			"e" @  over !  1 +
			"n" @  over !  1 +
			"d" @  over !  1 +
			0      swap !
			glue-buffer
			edit
			dup def-stub
			dup typeln
			dup >word last-text !
			>read
			r> if showturtle then
		else
			drop
		then
		parse eval void
		nil last-text !
	again
;

: run  >read parse eval void ; ( str -- )

: showversion ( -- )
	0 0 "                      " grid-type
	0 1 " Welcome to Loko 0.4  " grid-type
	0 2 " 64k OK               " grid-type
	0 3 "                      " grid-type
;

: main ( -- )
	gc-init
	env-init
	prims-init
	clearcolor CL !
	showturtle
	home
	hideturtle
	clearscreen
	intro

	# set up shorthand aliases for prims:
	"make 'pr   :print"      run
	"make 'pl   :printlist"  run
	"make 'bk   :back"       run
	"make 'fd   :forward"    run
	"make 'rt   :right"      run
	"make 'lt   :left"       run
	"make 'cs   :clear"      run
	"make 'seth :setheading" run
	"make 'pu   :penup"      run
	"make 'pd   :pendown"    run
	"make 'bf   :butfirst"   run
	"make 'bl   :butlast"    run

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

	showversion

	' heap-empty   ' gc-fail revector
	' console-emit ' emit    revector
	' abort        ' fail    revector
	' repl restart-vector !
	repl
;