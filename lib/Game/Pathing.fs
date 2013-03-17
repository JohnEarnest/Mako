######################################################
##
##  Pathing:
##
##  A system for performing pathfinding on a grid.
##  These routines are designed to calculate a
##  distance field for a 20x15 tile grid based
##  on a series of path costs.
##
##  In the input to 'find-path', mark cells which
##  are impassible with a negative number, cells
##  which are 'goals' with zero and cells to path
##  through with the '+i' constant provided.
##  If the input grid contains smaller positive
##  numbers they will override otherwise superior
##  paths, which may be useful for some applications.
##
##  John Earnest
##
######################################################

:data xd    0   1   0  -1
:data yd   -1   0   1   0
:data rd  -20   1  20  -1
:const +i 100000

:var dist-field
:var  dir-field

: dist@  dist-field @ + ; ( loc -- addr )
:  dir@   dir-field @ + ; ( loc -- addr )
: -dir   2 + 4 mod      ; ( dir -- dir  )    

: min-path ( loc -- )
	dup dist@ @ 1 + >r
	3 for
		dup rd i + @ +
		dup dist@ @ j > if
			j      over dist@ !
			i -dir over  dir@ !
			min-path
		else
			drop
		then
	next
	r> 2drop
;

: find-path ( dir-buff dist-buff -- )
	dist-field !
	 dir-field !
	299 for -1 i dir@ ! next
	299 for i dist@ @ -if i min-path then next
;

:include <Print.fs>
: print-path ( addr -- )
	299 for
		299 i - dup 20 mod -if cr then
		over + @ dup 0 < if "   " type drop
		else dup 10 < if space then . then
	next
	cr drop
;

(
# since we'll be doing recursion,
# we need to beef these up a bit.
:array data-stack   350 0
:array return-stack 350 0

:array dirs 300 0
:data  dist
	-1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1
	-1 -1 -1 -1 -1 +i +i +i +i +i +i +i -1 -1 -1 -1 -1 -1 -1 -1
	-1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 +i -1 -1 -1 -1 -1 -1 -1 -1
	-1 -1  0 -1 -1 -1 +i +i +i +i +i +i -1 -1 -1 -1 -1 -1 -1 -1
	-1 -1 +i -1 -1 -1 +i -1 -1 +i -1 +i -1 -1 -1 -1 -1 -1 -1 -1
	-1 -1 +i -1 -1 -1 +i -1 -1 +i -1 +i -1 -1 -1 -1 -1 -1 -1 -1
	-1 -1 +i +i +i +i +i -1 -1 +i -1 -1 -1 -1 -1 -1 -1 -1 -1 -1
	-1 -1 +i -1 -1 -1 -1 -1 -1 +i -1 -1 -1 -1 -1 -1 -1 -1 -1 -1
	-1 -1 +i -1 +i +i +i +i +i +i -1 -1 -1 -1 -1 -1 -1 -1 -1 -1
	-1 -1 +i -1 +i -1 -1 +i -1 -1 -1 +i -1 -1 -1 -1 -1 -1 -1 -1
	-1 -1 +i -1 +i -1 -1 +i -1 +i +i +i -1 -1 -1 -1 -1 -1 -1 -1
	-1 -1 +i -1 +i -1 -1 +i -1 -1 -1 +i -1 -1 -1 -1 -1 -1 -1 -1
	-1 -1 +i +i +i +i +i +i +i +i +i +i +i +i -1 -1 -1 -1 -1 -1
	-1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1
	-1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1

: main
	dirs dist find-path
	dist print-path
	dirs print-path
	halt
;
)