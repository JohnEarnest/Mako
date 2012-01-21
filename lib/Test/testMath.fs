:include <Math.fs>
:include <Test/Tap.fs>

: test   65536 * fast-sqrt 256 / ;
: test10 10000 * fast-sqrt       ;

: main

	19 plan
	666

	# sqrt usage examples
	   4 test  2 = "sqrt1" ok
	  25 test  5 = "sqrt2" ok
	  58 test  7 = "sqrt3" ok
	  96 test  9 = "sqrt4" ok
	 125 test 11 = "sqrt5" ok
	 200 test 14 = "sqrt6" ok
	 371 test 19 = "sqrt7" ok
	5738 test 75 = "sqrt8" ok

	 96 test10  979 = "sqrt9"  ok
	200 test10 1414 = "sqrt10" ok


	# gcd / lcm
	 3  6 gcd 3 = "gcd1" ok
	 3  4 gcd 1 = "gcd2" ok
	 9 37 gcd 1 = "gcd3" ok
	49 21 gcd 7 = "gcd4" ok

	 3  6 lcm   6 = "lcm1" ok
	 3  4 lcm  12 = "lcm2" ok
	 9 37 lcm 333 = "lcm3" ok
	49 21 lcm 147 = "lcm4" ok

	# permutations

	#3 2 nPr     9 = "nPr1" ok
	#3 4 nPr    81 = "nPr2" ok
	#7 2 nPr    49 = "nPr3" ok
	#7 5 nPr 16807 = "nPr4" ok

	# check canary
	666 = "canary" ok

	halt
;