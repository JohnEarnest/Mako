######################################################
##
##  Math:
##
##  A lexicon of useful arithmetic words.
##
##  John Earnest
##
######################################################

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

(
# sqrt usage examples
:include "Print.fs"
: test   65536 * fast-sqrt 256 / . cr ;
: test10 10000 * fast-sqrt       . cr ;

: main
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
;
)