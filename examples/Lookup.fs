######################################################
##
##  Lookup:
##
##  A pair of words that produce a very simple Map
##  implementation via linear search. For most kinds
##  of small dispatch table this approach is still
##  plenty fast.
##
##  John Earnest
##
######################################################

: scan (max min target -- found*)
	>r
	loop
		dup @ i xor
		-if r> drop swap drop 1 + exit then
		2 + 2dup >
	while
	r> drop 2drop -1
;

: lookup (target count* -- found*)
	swap >r dup @ 2 *
	swap 1 + dup >r + r> r>
	scan
;

(
# a use example:

:data table
	5        # number of entries
	243   3
	112   7
	890  45
	871  20
	100   2
	666  11

:include "Print.fs"

: main
	871 table lookup ? # 20 expected
	112 table lookup ? #  7 expected
	243 table lookup ? #  3 expected
	100 table lookup ? #  2 expected
	666 table lookup . # -1 expected
	999 table lookup . # -1 expected
	cr
;
)