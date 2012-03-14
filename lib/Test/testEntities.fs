:include <Sprites.fs>
:include <Test/Tap.fs>

# providing entity prototypes:

:const ent-max 128
:include <Game/Entities.fs>
: ent-clear    drop  ;
: ent-swap     2drop ;
: ent-blocked  drop false ;

# test code follows:

:const a 0
:const b 1
:const c 2
: pos! swap over py! px! ;

: main
	34 plan 666

	# tests for seeking:
	0 0 a pos!
	  0 -10 b pos!  a b seek n = "north" ok
	  0  20 b pos!  a b seek s = "south" ok
	-30   0 b pos!  a b seek w = "west"  ok
	 29   0 b pos!  a b seek e = "east"  ok
	 10 -10 b pos!  a b seek ne = "northEast" ok
	-20 -20 b pos!  a b seek nw = "northWest" ok
	 30  30 b pos!  a b seek se = "southEast" ok
	-29  29 b pos!  a b seek sw = "southWest" ok
	  1 -99 b pos!  a b seek n = "almostNorth" ok
	  1  99 b pos!  a b seek s = "almostSouth" ok
	 99   1 b pos!  a b seek e = "almostEast"  ok
	-99  -1 b pos!  a b seek w = "almostWest"  ok

	  0 -10 b pos!  a b ortho n = "north-o" ok
	  0  20 b pos!  a b ortho s = "south-o" ok
	-30   0 b pos!  a b ortho w = "west-o"  ok
	 29   0 b pos!  a b ortho e = "east-o"  ok

	 10 -10 b pos!  a b diago ne = "northEast-d" ok
	-20 -20 b pos!  a b diago nw = "northWest-d" ok
	 30  30 b pos!  a b diago se = "southEast-d" ok
	-29  29 b pos!  a b diago sw = "southWest-d" ok

	# tests for movement:
	32x32 a sprite@ !
	 10  10 a pos! a offscreen? false = "offscreen-neg1" ok
	-30 -30 a pos! a offscreen? false = "offscreen-neg2" ok
	100 319 a pos! a offscreen? true  = "offscreen-pos1" ok
	321  10 a pos! a offscreen? true  = "offscreen-pos1" ok

	0 0 a pos! ne a dir ! 7 a move
	a px 7 = a py -7 = and "move" ok
	
	100 100 a pos! true a kind !
	110 110 b pos! true b kind !
	 20  20 c pos! true c kind !
	a b distance   200 = "dist1" ok
	a c distance 12800 = "dist2" ok
	a ' always nearest b = "nearest1" ok
	40 60 a pos!
	a ' always nearest c = "nearest2" ok

	# tests for sorting:
	10 90 a pos!
	30 50 b pos!
	40 50 c pos!
	b c -sprites 0 = "-sprites1" ok
	a c -sprites 0 > "-sprites2" ok
	c a -sprites 0 < "-sprites3" ok
	sort-sprites
	sort-sprites
	ent-max 1 - py 90 =
	ent-max 2 - py 50 = and
	ent-max 3 - py 50 = and
	ent-max 4 - py 0  = and "sorted" ok

	666 = "canary" ok
;