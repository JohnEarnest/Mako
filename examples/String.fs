######################################################
##
##  String:
##
##  A lexicon for manipulating Strings and reading
##  in data from the debug port.
##
##  John Earnest
##
######################################################

: fill   >r 1 - for j over i + ! next r> 2drop    ; (addr len n  --)
: >move  1 - for over i + @ over i + ! next 2drop ; (src dst len --)

: <move (src dst len --) # copies low to high
	>r swap dup r> + >r
	loop
		dup i >= if r> 2drop drop exit then
		dup >r @ over ! r>
		1 + swap 1 + swap		
	again
;

: size ( addr -- len )
	0 loop
		over @ -if swap drop break then
		1 + swap 1 + swap
	again
;

# compare two null-terminated strings.
# positive if a>b, negative if a<b.
: -text (a b -- flag)
	loop
		over @ over @
		2dup - if - >r 2drop r> exit then
		and -if 2drop 0 exit then
		1 + swap 1 + swap
	again
;

: = xor -if true else false then ;
: digit?  dup 48 >= swap 57 <= and ;
: white?  dup 9 = over 10 = or swap 32 = or ;

:vector key CO @ ;

# read first non-whitespace char from stdin.
: >char ( -- char )
	key loop
		dup white? -if break then
		drop key
	again
;

# read a signed/unsigned integer from stdin.
: >number ( -- n )
	>char dup 45 xor
	-if drop -1 0 key else 1 swap 0 swap then
	loop
		dup digit? -if drop break then
		48 - swap 10 * + key
	again *
;

# read a whitespace-terminated word from stdin.
: >word ( addr len -- )
	>char swap
	2 - for
		dup white?
		if r> 2drop 0 swap ! exit then
		over ! 1 + key
	next
	drop 0 swap !
;

# read a line from stdin.
: >line ( addr len -- )
	2 - for
		key dup 10 xor
		-if r> 2drop 0 swap ! exit then
		over ! 1 +
	next
	0 swap !
;