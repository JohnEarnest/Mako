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

:const +infinity  2147483647
:const -infinity -2147483647

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

(
:include "Print.fs"
: test   65536 * fast-sqrt 256 / . cr ;
: test10 10000 * fast-sqrt       . cr ;

: main
	# sqrt usage examples
	   4 test # should be  2
	  25 test # should be  5
	  58 test # should be  7
	  96 test # should be  9
	 125 test # should be 11
	 200 test # should be 14
	 371 test # should be 19
	5738 test # should be 75

	 96 test10 # should be  979
	200 test10 # should be 1414

	# gcd / lcm
	 3  6 gcd . cr # should be 3
	 3  4 gcd . cr # should be 1
	 9 37 gcd . cr # should be 1
	49 21 gcd . cr # should be 7

	 3  6 lcm . cr # should be 6
	 3  4 lcm . cr # should be 12
     9 37 lcm . cr # should be 333
	49 21 lcm . cr # should be 147

	#3 2 nPr . cr # should be 9
	#3 4 nPr . cr # should be 81
	#7 2 nPr . cr # should be 49
	#7 5 nPr . cr # should be 16807

	halt
;
)