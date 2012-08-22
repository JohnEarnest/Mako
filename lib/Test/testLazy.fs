:include <Test/Tap.fs>

:const heap-size 500
:include <Algorithms/Garbage.fs>
:include <Algorithms/Curry.fs>
:include <Algorithms/Lazy.fs>

: managed-begin ;
: managed-end   ;

: .all     ' . apply cr ; ( gen -- )
: .bool    if "true" else "false" then type ; ( flag -- )

:data  stuff 34 78 0 50 9
:array more 5 0

: main
	gc-init
	9 plan
	666

	1 3 range 6 7 range chain sum      19 = "sum of chained ranges" ok
	1 10 range ' even? filter length    5 = "length of filtered range" ok
	5 18 range ' odd?  filter last     17 = "last of filtered range" ok
	379810 digits maximum               9 = "maximum of digits" ok
	7 factorial                      5040 = "factorial" ok
	stuff 5 array>gen sum             171 = "sum of array" ok
	count { 5 > } take-until last       5 = "last of taken until" ok

	2 5 range { 2 * } map more gen>array
	more 0 + @  4 =
	more 1 + @  6 = and
	more 2 + @  8 = and
	more 3 + @ 10 = and
	more 4 + @  0 = and "generator to array" ok

	# check canary
	666 = "canary" ok
;