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
:var  char

: advance ( char -- )
	10 = if
		line inc
		0 char !
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

:vector read  CO @ dup advance ; ( -- char )

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

: q+   dup @ 1 + qs mod swap ! ; ( addr -- )
: >q   a @ q + !  a q+  s inc  ; ( char -- )
: q>   b @ q + @  b q+  s dec  ; ( -- char )
: clear-q   0 a ! 0 b ! 0 s !  ; ( -- )

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
##  A series of handy predicates and equivalents
##  which operate on the head of the input stream
##  for brevity.
##
######################################################

: d>num    48 -                              ; ( char -- n )
: to-lower 32 or                             ; ( char -- char )
: to-upper 32 not and                        ; ( char -- char )
: white?   dup 9 = over 10 = or swap 32 = or ; ( char -- flag )
: letter?  to-lower dup 96 > swap 123 < and  ; ( char -- flag )
: digit?            dup 47 > swap  58 < and  ; ( char -- flag )

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
##  number> - read an unsigned integer
##  signed> - read a signed integer
##  accept> - read as long as a pred is satisfied (stored in pad)
##  token>  - accept> (letter)(letter|digit)* (stored in pad)
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
		10 * getc d>num +
		numeral?
	while trim
;

: signed> ( -- n )
	"-" match? if -1 else 1 then
	number> *
;

:var   padi
:array pad pad-size 0
0 # note the null padding

# >pad ensures that we never overflow
# the size of the input pad as we read.
# accept> will ensure the pad always ends
# with a proper null-terminator:

: >pad ( char -- )
	padi @ pad + !
	padi @ 1 + dup
	pad-size > if drop pad-size then
	padi !
;

: accept> ( pred -- string )
	0 padi !
	loop
		getc >pad
		dup exec
	while trim
	drop 0 >pad pad
;

: token> ( -- string )
	name? -if "Name expected." fail then
	{ name? numeral? or } accept>
;