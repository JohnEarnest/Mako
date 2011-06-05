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
		2 + over over >
	while
	r> drop drop drop -1
;

: lookup (target count* -- found*)
	swap >r dup @ 2 *
	swap 1 + dup >r + r> r>
	scan
;

:data table
	5        # number of entries
	243   3
	112   7
	890  45
	871  20
	100   2
	666  11

: main
	871 table lookup @ NO ! # 20 expected
	112 table lookup @ NO ! #  7 expected
	243 table lookup @ NO ! #  3 expected
	100 table lookup @ NO ! #  2 expected
	666 table lookup   NO ! # -1 expected (out of range)
	999 table lookup   NO ! # -1 expected (not found)
	10 CO !
	
	loop
		sync
	again
;