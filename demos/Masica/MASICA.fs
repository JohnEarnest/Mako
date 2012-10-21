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
##    + - * / % and or not == != > < >= <=
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

:var showline
:var lineno

: abort ( string -- )
	showline @ if
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
	KB @ 3 = if
		false showline !
		"BREAK" abort
	then
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

:const heap-size 65536
:include <Algorithms/Garbage.fs>

: managed-begin ;
:array var-dynamic 26 0
:array jit-heap heap-size 0
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

:const max-line 500000

: insert-line ( string line-no -- )

	dup 0 < over max-line >= or if
		2drop
		"Invalid line number." abort
	then

	# no root
	program-lines @ -if
		swap pair
		max-line nil pair
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

: remove-line ( number -- )
	program-lines @ -if
		drop "No such line number." abort
	then
	
	# = first element
	program-lines @ first first over = if
		drop
		program-lines @ rest
		program-lines !

		# clear the chain entirely if we
		# are removing the first element:
		program-lines @ first first max-line = if
			0 program-lines !
		then
		exit
	then

	# otherwise, find predecessor
	program-lines @ loop
		( line root )
		2dup rest first first = if
			dup rest rest swap rest!
			drop exit
		then
		dup rest first first max-line = if
			2drop "No such line number." abort
		then
		rest
	again
;

: get-line ( number -- str )
	program-lines @ -if
		drop "No such line number." abort
	then
	program-lines @ loop
		2dup first first = if
			swap drop
			first rest ptr>
			exit
		then
		dup first first max-line = if
			2drop "No such line number." abort
		then
	again
;

: stralloc ( str -- ptr )
	dup size 1 +
	dup alloc
	dup >r ptr> swap >move r>
;

######################################################
##
##  Dynamic primitive wrappers:
##
##  This is how we apply different definitions based
##  on the operand types at play.
##
######################################################

:array var-types   26 0
:array var-values  26 0

:const TYPE_BOOL 0
:const TYPE_INT  1
:const TYPE_STR  2

: bool?  TYPE_BOOL = -if "Boolean expected." abort then ; ( type -- )
: int?   TYPE_INT  = -if "Integer expected." abort then ; ( type -- )
: str?   TYPE_STR  = -if "String expected."  abort then ; ( type -- )

: /0?    dup 0 = if "Cannot divide by zero." abort then ; ( n -- n )

: basic-@ ( varno -- val type )
	dup var-types + @ dup TYPE_STR = if
		swap var-dynamic
	else
		swap var-values
	then
	+ @ swap
;

: basic-! ( val type varno -- )
	2dup var-types + !
	swap TYPE_STR = if
		var-dynamic
	else
		var-values
	then
	+ !
;

: basic-== ( a ta b tb -- flag BOOL )
	>r over r> = -if
		# different types can be compared without
		# a type error, but they cannot be equal.
		drop 2drop
		false TYPE_BOOL
		exit
	then
	swap TYPE_STR = if
		ptr> swap ptr> -text 0 =
	else
		=
	then
	TYPE_BOOL
;

: basic-!= ( a ta b tb -- flag BOOL )
	basic-== swap not swap
;

: basic-+  int?  swap int?      +   TYPE_INT  ; ( a ta b ta -- a+b  INT  )
: basic--  int?  swap int?      -   TYPE_INT  ; ( a ta b tb -- a-b  INT  )
: basic-*  int?  swap int?      *   TYPE_INT  ; ( a ta b tb -- a*b  INT  )
: basic-/  int?  swap int?  /0? /   TYPE_INT  ; ( a ta b tb -- a/b  INT  )
: basic-%  int?  swap int?  /0? mod TYPE_INT  ; ( a ta b tb -- a%b  INT  )
: basic-<  int?  swap int?      <   TYPE_BOOL ; ( a ta b tb -- a<b  BOOL )
: basic->  int?  swap int?      >   TYPE_BOOL ; ( a ta b tb -- a>b  BOOL )
: basic-<= int?  swap int?      <=  TYPE_BOOL ; ( a ta b tb -- a<=b BOOL )
: basic->= int?  swap int?      >=  TYPE_BOOL ; ( a ta b tb -- a>=b BOOL )
: basic-&  bool? swap bool?     and TYPE_BOOL ; ( a ta b tb -- a&b  BOOL )
: basic-|  bool? swap bool?     or  TYPE_BOOL ; ( a ta b tb -- a|b  BOOL )

: basic-not  bool? not              TYPE_BOOL ; ( val type -- ~val BOOL )
: basic-neg  int? -1 *              TYPE_INT  ; ( val type -- -val INT  )
: basic-rnd  int? RN @ swap /0? mod TYPE_INT  ; ( max+1 type -- num INT )

: basic-num  swap drop TYPE_INT =   TYPE_BOOL ; ( val type -- flag BOOL )

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

: basic-input ( -- val type )
	eof? if false readline >read trim then
	numeral? "-" @ curr = or if
		signed> TYPE_INT
	else
		{ eof? not } accept> skip trim
		stralloc TYPE_STR
	then
;

: basic-print ( val type -- )
	dup TYPE_STR  = if drop ptr> type                          exit then
	dup TYPE_BOOL = if drop if "true " else "false " then type exit then
	drop .
;

: basic-goto ( addr -- )
	brk r> drop >r
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

: finish ( -- )
	trim eof? -if "Syntax Error?" abort then
;

: jit-var ( -- )
	name? -if "Syntax Error?" abort then
	getc trim to-lower "a" @ -
	jit-const
;

:proto jit-expr
:const quote 34

:vector primary ( -- )
	"true"  match? if true  jit-const TYPE_BOOL jit-const exit then
	"false" match? if false jit-const TYPE_BOOL jit-const exit then
	"rnd("  match? if jit-expr ")" expect ' basic-rnd jit-call exit then
	"num("  match? if jit-expr ")" expect ' basic-num jit-call exit then
	"(" match?     if jit-expr ")" expect exit then
	numeral?       if number> jit-const TYPE_INT jit-const exit then
	name?          if jit-var     ' basic-@ jit-call       exit then
	curr quote =   if
		skip
		{ curr quote != eof? not and }
		accept> stralloc jit-const
		TYPE_STR jit-const
		skip trim
		exit
	then
	"Syntax Error?" abort
;

: unary ( -- )
	"-"   match? if unary ' basic-neg jit-call exit then
	"not" match? if unary ' basic-not jit-call exit then
	primary
;

: multiplicative ( -- )
	unary
	"*"   match? if multiplicative ' basic-* jit-call exit then
	"/"   match? if multiplicative ' basic-/ jit-call exit then
	"%"   match? if multiplicative ' basic-% jit-call exit then
	"and" match? if multiplicative ' basic-& jit-call exit then
;

: additive ( -- )
	multiplicative
	"+"  match? if additive ' basic-+ jit-call exit then
	"-"  match? if additive ' basic-- jit-call exit then
	"or" match? if additive ' basic-| jit-call exit then
;

: jit-expr ( -- )
	additive
	"!=" match? if jit-expr ' basic-!= jit-call exit then
	"==" match? if jit-expr ' basic-== jit-call exit then
	"<=" match? if jit-expr ' basic-<= jit-call exit then
	">=" match? if jit-expr ' basic->= jit-call exit then
	"<"  match? if jit-expr ' basic-<  jit-call exit then
	">"  match? if jit-expr ' basic->  jit-call exit then
;

: jit-print ( -- )
	loop
		jit-expr
		' basic-print jit-call
		"," match?
	while
	";" match? -if
		' cr jit-call
	then
	finish
;

: jit-input ( -- )
	loop
		' basic-input jit-call
		jit-var
		' basic-! jit-call
		"," match?
	while
	finish
;

: jit-let ( -- )
	jit-var
	STR >code
	"=" expect
	jit-expr
	RTS >code
	' basic-! jit-call
	finish
;

: jit-line ( -- )
	"print" match? if
		jit-print
		exit
	then
	"input" match? if
		jit-input
		exit
	then
	"goto" match? if
		number>        >gotos
		jit-head @ 1 + >gotos
		-1 jit-const
		' basic-goto jit-call
		finish
		exit
	then
	"if" match? if
		jit-expr
		"then" expect
		' bool? jit-call
		JUMPZ >code jit-head @ -1 >code
		jit-line
		jit-head @ swap !
		exit
	then
	"end" match? if
		RETURN >code
		finish
		exit
	then

	"let" starts? if "let" match? drop then
	jit-let
;

:array jit-lines  512 0
:array jit-addrs  512 0
:var   lines
: >lines ( line addr -- )
	lines @ jit-addrs + !
	lines @ jit-lines + !
	lines inc
;

: goto-addr ( line-no -- addr )
	lines @ 1 - for
		i jit-lines + @ over = if
			drop r> jit-addrs + @ exit
		then
	next
	drop "No such line number." abort
;

: jit ( -- entrypoint )
	program-lines @ -if "No program entered." abort then
	true showline !
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

		# update line number while
		# executing code for error messages:
		lineno @ jit-const
		lineno   jit-const
		STOR >code

		jit-line
		rest
	again
	loop
		gotos> dup -if drop break then
		gotos> goto-addr swap !
	again
	RETURN >code
	jit-heap
	false showline !
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

		keys key-rt and if cursor @ 1 + used @ min cursor ! sync sync then
		keys key-lf and if cursor @ 1 -      0 max cursor ! sync sync then

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

: eval ( proc -- )
	false showline !
	jit-heap jit-head !
	exec
	RETURN >code
	jit-heap exec
;

: statement ( -- )
	"print" match? if
		' jit-print eval
		exit
	then
	"input" match? if
		' jit-input eval
		exit
	then
	"goto" match? if
		number>
		finish
		" " >read trim
		jit drop
		goto-addr
		true showline !
		exec
		false showline !
		exit
	then
	"if" match? if
		' jit-expr eval
		"then" expect
		bool?
		if
			statement
		then
		exit
	then
	"end" match? if
		# do nothing
		finish
		exit
	then
	"let" starts? if "let" match? drop then
	' jit-let eval
;

: init ( -- )
	25 for
		TYPE_INT i var-types   + !
		       0 i var-values  + !
		       0 i var-dynamic + !
	next
	0 program-lines !
;

:proto repl-line

: command ( -- )
	eof? if
		exit
	then
	"new" match? if
		finish
		init
		"ready." typeln
		exit
	then
	"list" match? if
		finish
		program-lines @ -if
			"No program entered." abort
		then
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
		finish
		jit
		" " >read trim
		true showline !
		exec
		false showline !
		exit
	then
	"edit" match? if
		number> finish
		get-line true readline
		repl-line
		exit
	then
	"erase" match? if
		number> finish remove-line
		exit
	then
	"help" match? if
		finish
		"help  - list available commands"      typeln
		"new   - reset the interpreter"        typeln
		"list  - display program code"         typeln
		"run   - execute the program"          typeln
		"edit  - modify a line of code"        typeln
		"erase - delete a line of code"        typeln
		cr
		"print - write values to display"      typeln
		"input - read values into variables"   typeln
		"goto  - jump to a specified line"     typeln
		"if    - conditional branch"           typeln
		"end   - stop the program"             typeln
		"let   - assign a value to a variable" typeln
		cr
		exit
	then
	statement
;

: repl-line ( str -- )
	dup >read trim
	numeral? if
		dup stralloc
		number> insert-line
	else
		command
	then
	drop
;

: repl ( -- )
	loop false readline repl-line again
;

: main ( -- )
	gc-init
	init
	intro

	1 1 "MASICA BIOS 0.1" grid-type
	1 2 "128k OK"         grid-type

	' console-emit ' emit revector
	' abort        ' fail revector
	' repl          restart-vector !
	repl
;