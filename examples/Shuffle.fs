######################################################
##
##  Shuffle:
##
##  An implementation of a Fisher-Yates shuffle,
##  for randomly reordering elements of an array.
##
##  John Earnest
##
######################################################

: swap@   dup @ >r over @ swap ! r> swap ! ;

: shuffle (addr length -- )
	1 - for
		RN @ i 1 + mod over +
		over i + swap@
	next
;

(
# a use example:

:include "Print.fs"

:data arr 1 2 3 4 5 6

: main
	arr 6 shuffle

	arr     ?
	arr 1 + ?
	arr 2 + ?
	arr 3 + ?
	arr 4 + ?
	arr 5 + ?
	cr
;
)