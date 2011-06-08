######################################################
##
##  Sort:
##
##  An implementation of an in-place selection sort.
##  Sorts elements from smallest to largest.
##
##  John Earnest
##
######################################################

: swap@  2dup @ >r @ swap ! r> swap ! ;

: sort (addr length -- )

	1 - for
		dup i +
		i for
			over i + @ over @ >
			if drop dup i + then
		next
		over i + swap@
	next
	drop
;