######################################################
##
##  Parse:
##
##  A lexicon for parsing input from stdin or from
##  pre-existing strings. Includes an error handler
##  intended for reading from a file via stdin.
##
##  Parse requires routines from Print.fs and
##  String.fs in order to function.
##
##  John Earnest
##
######################################################

:const qs       16 # input queue size
:const pad-size 32 # identifier input buffer size

: revector  1 + !            ; ( 'new-word 'word -- )
: inc       dup @ 1 + swap ! ; ( addr -- )
: dec       dup @ 1 - swap ! ; ( addr -- )

######################################################
##
##  Error Handler and Input Subsystem:
##
##  By default, the parser will read from stdin.
##  using '>read' will redirect the input system
##  to read from a string.
##
######################################################

:data line 1
:data char 1

: advance ( char -- )
	10 = if
		line inc
		1 char !
	else
		char inc
	then
;

: fail ( string -- )
	type
	" (line " type line @ .num
	", char " type char @ .num
	")" typeln
	halt
;

:vector read  CO @ ; ( -- char )

:var text-src
: read-text ( -- char )
	text-src @ @ dup if
		text-src inc
	else
		drop -1 # EOF
	then
;

:proto clear-q
: >read ( string -- )
	text-src !
	clear-q
	' read-text ' read revector
;

######################################################
##
##  Input Buffering:
##
##  This buffer provides a fixed-size lookahead
##  window on the input stream, which is necessary
##  for some high-level parsing words. xq can fetch
##  an arbitrary index into the lookahead buffer.
##  Take care when using words other than 'xq',
##  'pull', 'skip', 'curr' and 'getc' explicitly that
##  you advance the input buffer as necessary.
##
######################################################

:array q qs 0
:var a # head
:var b # tail
:var s # size

: q+   dup @ 1 + qs mod swap !            ; ( addr -- )
: >q   a @ q + !  a q+  s inc             ; ( char -- )
: q>   b @ q + @  b q+  s dec dup advance ; ( -- char )
: clear-q   0 a ! 0 b ! 0 s !             ; ( -- )

: xq ( index -- char )
	s @ over <= if
		dup s @ - for
			read >q
		next
	then
	b @ + qs mod q + @
;

: pull   s @ 1 < if read >q then ; ( -- )
: skip   pull q> drop            ; ( -- )
: curr   pull b @ q + @          ; ( -- char )
: getc   pull q>                 ; ( -- char )

######################################################
##
##  Basic Parsing Words:
##
##  A series of handy predicates which operate on
##  the head of the input stream.
##
######################################################

: space?   curr white?  ; ( -- flag )
: name?    curr letter? ; ( -- flag )
: numeral? curr digit?  ; ( -- flag )
: eof?     curr -1 =    ; ( -- flag )
: newline? curr 10 =    ; ( -- flag )

######################################################
##
##  High-Level Parsing Words:
##
##  trim    - advance past any whitespace.
##  starts? - return true if input begins with a string.
##  match?  - like starts? but accept the string on a match.
##  expect  - like match? but errors on failure.
##  number> - read an unsigned integer.
##  signed> - read a signed integer.
##  input>  - read into a specified buffer as long as pred is satisfied.
##  accept> - read into pad as long as a pred is satisfied and trim.
##  token>  - accept> (letter)(letter|digit)*.
##  line>   - read into a specified buffer until a newline.
##  
######################################################

: trim ( -- )
	space? if
		loop skip space? while
	then
;

: starts? ( string -- flag )
	dup 0 loop
		( string char* queue-index )
		over @ -if
			2drop drop true exit
		then
		over @ over xq != if
			2drop drop false exit
		then
		1 + swap 1 + swap
	again
;

: match? ( string -- flag )
	dup starts? if
		size 
		1 - for skip next
		trim true
	else
		drop false
	then
;

: expect ( string -- )
	dup match? -if "Expected '" type type "'." fail then
	drop
;

: number> ( -- n )
	numeral? -if "Numeral expected." fail then
	0 loop
		10 * getc to-num +
		numeral?
	while trim
;

: signed> ( -- n )
	"-" match? if -1 else 1 then
	number> *
;

: input> ( addr len pred -- )
	>r loop
		dup 1 > -if break then
		i exec  -if break then
		over getc swap !
		1 - swap 1 + swap
	again
	r> 2drop
	0 swap !
;

:array pad pad-size 0

: accept> ( pred -- )
	>r pad pad-size r> input> trim pad
;

: token> ( -- string )
	name? -if "Name expected." fail then
	{ name? numeral? or } accept>
;

: line> ( addr len -- )
	{ newline? not } input> skip
;