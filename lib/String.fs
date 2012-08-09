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

: to-num   48 -                              ; ( char -- n )
: to-lower 32 or                             ; ( char -- char )
: to-upper 32 not and                        ; ( char -- char )
: white?   dup 9 = over 10 = or swap 32 = or ; ( char -- flag )
: letter?  to-lower dup 96 > swap 123 < and  ; ( char -- flag )
: digit?            dup 47 > swap  58 < and  ; ( char -- flag )