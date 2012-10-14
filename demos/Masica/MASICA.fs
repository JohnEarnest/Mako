######################################################
##
##  Masica:
##
##  An educational interactive TinyBASIC interpreter.
##  The following commands are supported:
##
##    print <expr>, <expr>, ... <expr>
##    input <var>, <var>, ... <var>
##    goto  <line>
##    if <expr> then <statement>
##    [let] <var> = <expr>
##    end
##
##  Expressions can be composed from the following
##  operators, which perform integer arithmetic:
##
##    + - * / % and or not = <> > < >= <=
##
##  John Earnest
##
######################################################

:include <Print.fs>
:include <String.fs>
:include <Parse.fs>

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

:array data-stack   200 0
:array return-stack 200 0

:var restart-vector

:var compiling
:var lineno

: abort ( string -- )
	compiling @ if
		"line " type
		lineno @ .num
		": " type
	then

	typeln
	data-stack   DP !
	return-stack RP !
	restart-vector @ exec
	halt
;

: brk ( -- )
	KB @ 3 = if "BREAK" abort then
;

: expect ( string -- )
	match? -if
		"Unexpected token?" abort
	then
;

######################################################
##
##  Source Text:
##
##  Store the lines of the program as a linked-list
##  backed associative array, keyed by line number.
##  Lines are sorted as they are inserted.
##
######################################################

:const heap-size 4096
:include <Algorithms/Garbage.fs>

: managed-begin ;
:array jit-vars   26 0
:array jit-heap 4096 0
:var   program-lines # ((line-no . string) . next)
: managed-end ;

:const nil -2147483648
: nil?    nil xor -if true else false then         ; ( val -- flag )
: pair    2 alloc dup >r ptr> swap over 1 + ! ! r> ; ( first rest -- pair )
: first   ptr>     @                               ; ( pair -- first )
: rest    ptr> 1 + @                               ; ( pair --  rest )
: first!  ptr>     !                               ; ( pair -- first )
: rest!   ptr> 1 + !                               ; ( pair --  rest )
:  split  dup first swap  rest                     ; ( pair -- first rest )
: -split  dup  rest swap first                     ; ( pair -- rest first )

: insert-line ( string line-no -- )

	# no root
	program-lines @ -if
		swap pair
		500000 nil pair
		nil pair
		pair
		program-lines !
		exit
	then

	# < first element
	program-lines @ first first over > if
		swap pair
		program-lines @ pair
		program-lines !
		exit
	then

	# = first element
	program-lines @ first first over = if
		swap pair
		program-lines @ rest pair
		program-lines !
		exit
	then

	# otherwise, find successor
	program-lines @ loop
		( string line root )
		2dup rest first first < if
			>r swap pair
			i  rest pair
			r> rest!
			break
		then
		2dup rest first first = if
			swap drop
			rest first rest!
			break
		then
		rest
	again
;

: stralloc ( str -- ptr )
	dup size 1 +
	dup alloc
	dup >r ptr> swap >move r>
;

######################################################
##
##  BASIC Library:
##
##  These are a number of routines that are used
##  by BASIC at runtime to perform I/O and flow control.
##  Note that the 'goto' support routine is where
##  we query for interrupt signals.
##
######################################################

:proto readline

: basic-input ( -- value )
	eof? if readline >read trim then
	numeral? if
		signed>
	else
		{ eof? not } accept> skip trim
		stralloc
	then
;

: basic-print ( value -- )
	dup p? if
		ptr> type
	else
		.
	then
;

: basic-goto ( addr -- )
	brk r> drop >r
;

: basic= ( a b -- flag )
	over p? over p? and if
		ptr> swap ptr> -text 0 =
	else
		=
	then
;

######################################################
##
##  Compiler:
##
##  BASIC programs are run through a two-pass compiler
##  before execution. In the first pass, we walk over
##  program lines and emit machine code. The second
##  pass patches the addresses of any GOTO instructions
##  to reflect the machine addresses to which lines
##  were compiled.
##
######################################################

:include <Assembly.fs>

:array jit-gotos 512 0
:var   jit-head
:var   gotos

: gotos>     gotos dec gotos @ @             ; ( -- val )
: >gotos     gotos @ ! gotos inc             ; ( val -- )
: >code      jit-head @ !       jit-head inc ; ( val -- )
: jit-call   CALL  >code >code               ; ( addr -- )
: jit-const  CONST >code >code               ; ( val -- )

: jit-var ( -- )
	name? -if "Syntax Error?" abort then
	getc trim to-lower "a" @ -
	jit-vars + jit-const
;

:proto jit-expr
:const quote 34

:vector primary ( -- )
	"(" match?   if jit-expr ")" expect exit then
	numeral?     if number> jit-const   exit then
	name?        if jit-var LOAD >code  exit then
	curr quote = if
		skip
		{ curr quote != eof? not and }
		accept> stralloc jit-const
		skip trim
		exit
	then
	"Syntax Error?" abort
;

: unary ( -- )
	"-"   match? if unary -1 jit-const MUL >code exit then
	"not" match? if unary              NOT >code exit then
	primary
;

: multiplicative ( -- )
	unary
	"*"   match? if multiplicative MUL >code exit then
	"/"   match? if multiplicative DIV >code exit then
	"%"   match? if multiplicative MOD >code exit then
	"and" match? if multiplicative AND >code exit then
;

: additive ( -- )
	multiplicative
	"+"  match? if additive ADD >code exit then
	"-"  match? if additive SUB >code exit then
	"or" match? if additive OR  >code exit then
;

: jit-expr ( -- )
	additive
	"<>" match? if jit-expr ' basic= jit-call NOT >code exit then
	"="  match? if jit-expr ' basic= jit-call           exit then
	"<=" match? if jit-expr SGT >code         NOT >code exit then
	">=" match? if jit-expr SLT >code         NOT >code exit then
	"<"  match? if jit-expr SLT >code                   exit then
	">"  match? if jit-expr SGT >code                   exit then
;

: jit-line ( -- )
	"print" match? if
		loop
			jit-expr
			' basic-print jit-call
			"," match?
		while
		";" match? -if
			' cr jit-call
		then
		exit
	then
	"input" match? if
		loop
			' basic-input jit-call
			jit-var
			STOR >code
			"," match?
		while
		exit
	then
	"goto" match? if
		number>        >gotos
		jit-head @ 1 + >gotos
		-1 jit-const
		' basic-goto jit-call
		exit
	then
	"if" match? if
		jit-expr
		"then" expect
		JUMPZ >code jit-head @ -1 >code
		jit-line
		jit-head @ swap !
		exit
	then
	"end" match? if
		RETURN >code
		exit
	then

	"let" starts? if "let" match? drop then
	# implicitly a 'let'
	jit-var
	"=" expect
	jit-expr
	SWAP >code
	STOR >code
;

:array jit-lines  512 0
:array jit-addrs  512 0
:var   lines
: >lines ( line addr -- )
	lines @ jit-addrs + !
	lines @ jit-lines + !
	lines inc
;

: patch-goto ( patch-addr line-no -- )
	lines @ 1 - for
		i jit-lines + @ over = if
			drop
			i jit-addrs + @ swap !
			rdrop exit
		then
	next
	"No line numbered " type . "" abort
;

: jit ( -- entrypoint )
	true compiling !
	jit-heap  jit-head !
	jit-gotos    gotos !
	0 >gotos
	0 lines !
	program-lines @ loop
		dup first rest nil? if
			drop break
		then
		dup first rest ptr> >read
		number> dup lineno !
		jit-head @ >lines
		jit-line
		rest
	again
	loop
		gotos> dup -if drop break then
		gotos> patch-goto
	again
	RETURN >code
	jit-heap
	false compiling !
;

######################################################
##
##  UI/Frontend:
##
##  A graphical editor and terminal editor which
##  functions by patching Mako stdio libraries.
##
######################################################

:include <Grid.fs>
:include <Sprites.fs>

:data grid
	98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98
	98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98
	98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98
	98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98
	 0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0 
	 0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0 
	 0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0 
	 0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0 
	 0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0 
	 0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0 
	 0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0 
	 0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0 
	 0  0  0  0  1  1  2  0  3  1  1  0  4  1  1  5  0  6  7  1  1  8  1  1  1  9 10  7  1  1  0 11  1  1 12  0  0  0  0  0 
	 0  0  0  0 13 14 15 16 17 14 18  0 19 20 21 22  0 23 24 25 14 26 27 14 28 29 30 31 13 14  0 32 14 33 34  0  0  0  0  0 
	 0  0  0  0 35 36 37 38 39 36 40  0 41 42 43 44  0 45 36 36 46  0 47 36 48 49 36 50  0  0  0 51 52 53 54  0  0  0  0  0 
	 0  0  0  0 55  1 56  1 57  1  8  0 58  1  1 59  0 60 61 62  1 63 64  1 65 66  1 67 68 69  0 70  1  1  1 71  0  0  0  0 
	 0  0  0  0 72 14 73 74 75 14 76 77 78 28 79 14 80 81 82 72 14 83 84 14 85 86 87 82 72 14 88 89 31 13 14 90  0  0  0  0 
	 0  0  0  0 36 36 91 92 47 36 36 93 36 94 35 36 54 93 36 36 95  0 36 36 36 94 96 97 36 36 41 36 48 47 36 36  0  0  0  0 
	 0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0 
	 0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0 
	 0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0 
	 0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0 
	 0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0 
	 0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0 
	 0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0 
	 0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0 
	98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98
	98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98
	98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98
	98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98 98
:image grid-tiles "tiles.png" 8 8

:data   sprite-tiles
:image console-tiles "text.png" 8 8
:array console-grid 1271 0

:const cursor-s 0
:var   used
:var   cursor
:var   cx
:var   lines
:data  cc -32
:array input 41 0

: min      2dup > if swap then drop ;
: max      2dup < if swap then drop ;

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

: intro ( -- )
	-1 GS !
	117 for
		0xFF000000
		117 i -       65536 * +
		117 i -   5 +   256 * +
		117 i - 118 +         +
		CL !
		sync
	next
	#loop sync keys until
	0 GS !
	console-tiles GT !
	console-grid  GP !
;

: readline ( -- )
	input-clear
	cursor-s show
	loop
		loop
			KB @ dup -1 = if drop break then
			dup 10 = if
				# return
				drop
				code-text input typeln plain-text
				0 lines !
				cursor-s hide
				input
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

		keys key-rt and if cursor @ 1 + used @ min cursor ! then
		keys key-lf and if cursor @ 1 -      0 max cursor ! then

		39 for
			i used @ >= if 0 else i input + @ 32 - then
			96 + i 29 tile-grid@ !
		next
		8x8 191 cursor @ 8 * 232 cursor-s >sprite

		sync sync
	again
;

######################################################
##
##  REPL:
##
##  Repeatedly query for new input and allow users
##  to enter numbered lines of code.
##
######################################################

: command ( -- )
	eof? if
		exit
	then
	"new" match? if
		25 for 0 i jit-vars + ! next
		0 program-lines !
		exit
	then
	"list" match? if
		program-lines @ -if exit then
		program-lines @ loop
			dup first rest nil? if
				drop break
			then
			dup first rest ptr> typeln
			rest
		again
		exit
	then
	"run" match? if
		jit
		" " >read trim
		exec
		exit
	then
	"bye" match? if
		halt
	then
	"Unknown Command?" abort
;

: repl ( -- )
	loop
		readline
		dup >read trim
		numeral? if
			dup stralloc
			number> insert-line
		else
			command
		then
		drop
	again
;

: main ( -- )
	gc-init
	intro

	1 1 "MASICA BIOS 0.1" grid-type
	1 2 "4096k OK"        grid-type

	' console-emit ' emit revector
	' abort        ' fail revector
	' repl          restart-vector !
	repl
;