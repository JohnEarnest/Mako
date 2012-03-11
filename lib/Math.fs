######################################################
##
##  Math:
##
##  A lexicon of useful arithmetic words.
##
##  John Earnest
##
######################################################

: max   2dup < if swap then drop       ;
: min   2dup > if swap then drop       ;
: neg   -1 *                           ;
: abs   dup 0 < if neg then            ;
: ?dup  dup if dup then                ;
: gcd   ?dup if swap over mod gcd then ; ( a b -- gcd )
: lcm   2dup gcd >r * abs r> /         ; ( a b -- lcm )

: n-digits ( n -- count )
	0 swap
	loop
		10 / swap 1 + swap
		dup
	while
	drop
;

: fact ( n -- n! )
	dup 1 < if drop 1 exit then
	1 loop
		over *
		swap 1 - swap
		over 1 >
	while
;

# Calculate a fast, fixed-point approximation
# of the square root of a number. Input numbers
# should be multiplied by the squared fixed-base.

: fast-sqrt ( n -- n )
	1073741824 # 1 << 30
	loop 4 / 2dup < while
	dup -if 2drop 0 exit then

	0 >r loop
		2dup i + >= if
			swap over i + - swap
			r> over 2 * + >r
		then
		r> 2 / >r 4 / dup
	while
	2drop r>
;

# saturating addition
: +s ( a b -- a+b ) 
	over 0 > over 0 > >r >r
	+
	dup 0 < i j and and     if drop +infinity then
	dup 0 > i j and not and if drop -infinity then
	rdrop rdrop
;

# saturating subtraction
: -s ( a b -- a-b )
	over 0 > over 0 > >r >r
	-
	dup 0 < i j not and and if drop +infinity then
	dup 0 > i not j and and if drop -infinity then
	rdrop rdrop
;