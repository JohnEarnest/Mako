######################################################
##
##  Zalgo:
##
##  The Zero-overhead ALGOrithms collection, my
##  library of odd utility routines for project Euler
##  problems, ported to Forth.
##
##  John Earnest
##
######################################################

: prime? (n -- flag)
	dup 4 <    if drop true  exit then
	dup 2 mod -if drop false exit then
	1 loop
		2 +
		2dup mod -if 2drop false exit then
		2dup dup * >
	while
	2drop true
;

:const base 10

: int-concat ( a b -- ab )
	dup >r
	loop
		base / swap
		base * swap
		dup
	while
	drop r> +
;

: palindrome? ( n -- flag )
	dup >r 0
	loop
		base * over base mod + swap
		base / swap
		over
	while
	swap drop r>
	xor -if true else false then
;