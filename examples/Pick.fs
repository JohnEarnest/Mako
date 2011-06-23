######################################################
##
##  Pick:
##
##  Sometimes it's handy to be able to fetch
##  an element at an arbitrary depth in the stack.
##  The supplied index is 0-based- that is,
##  an index of 0 is the topmost stack element
##  ignoring the argument to this word.
##  'pick' also comes in a flavor for the return stack.
##
##  John Earnest
##
######################################################

: pick ( index -- element )
	DP @ 2 - swap - @
;

: rpick ( index -- element )
	RP @ 2 - swap - @
;

(
# a use example:

:include "Print.fs"

: main

	55 99 66 33
	1 pick . cr # 66 expected
	0 pick . cr # 33 expected
	3 pick . cr # 55 expected
	2 pick . cr # 99 expected

	43 >r 98 >r 41 >r
	1 rpick . cr # 98 expected
	0 rpick . cr # 41 expected
	2 rpick . cr # 43 expected
;
)