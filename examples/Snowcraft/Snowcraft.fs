######################################################
##
##  Facility:
##
##  A basic tower defense game, demonstrating
##  further use of Entities and basic pathfinding.
##
##  John Earnest
##
######################################################

:include "../Print.fs"
:include "../Grid.fs"
:include "../Util.fs"
:include "../Sprites.fs"
:include "../Deep/Entities.fs"

:image sprite-tiles "snowcraft.png" 16 24
:image grid-tiles   "sokotiles.png"  8  8
:data grid

	-1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 
	-1 12 13 13 13 13 13 13 13 13 13 13 13 13 13 13 13 13 13 13 13 13 13 13 13 13 13 13 13 13 14 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 
	-1 15  4  1  0  1  0  1  0  1  0  1  0  1  0  1  0  1  0  1  0  1  0  1  0  1  0  1  0  1 31 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 
	-1 15 16 17 16 17 16 17 16 17 16 17 16 17 16 17 16 17 16 17 16 17 16 17 16 17 16 17 16 17 31 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 
	-1 15  0  1 10 29 29 29 29 11  0  1 10 11  0  1 10 29 29 29 29 29 29 29 29 29 29 11  0  1 31 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 
	-1 15 16 17 31 -1 -1 -1 -1 15 16 17 31 15 16 17 31 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 15 16 17 31 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 
	-1 15  0  1 31 -1 -1 -1 -1 15  0  1 31 15  0  1 31 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 15  0  1 31 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 
	-1 15 16 17 31 12 13 13 13 27 16 17 31 15 16 17 31 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 15 16 17 31 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 
	-1 15  0  1 31 15  4  1  0  1  0  1 31 15  0  1 31 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 15  0  1 31 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 
	-1 15 16 17 31 15 16 17 16 17 16 17 31 15 16 17 26 13 13 13 13 13 13 13 13 13 13 27 16 17 26 13 13 13 13 13 14 -1 -1 -1 -1 
	-1 15  0  1 31 28 29 29 29 29 29 29 30 15  0  1  0  1  0  1  0  1  0  1  0  1  0  1  0  1  0  1  0  1  0  1 31 -1 -1 -1 -1 
	-1 15 16 17 31 -1 -1 -1 -1 -1 -1 -1 -1 15 16 17 16 17 16 17 16 17 16 17 16 17 16 17 16 17 16 17 16 17 16 17 31 -1 -1 -1 -1 
	-1 15  0  1 31 -1 -1 -1 -1 -1 -1 -1 -1 28 29 29 29 29 29 29 29 29 29 11  0  1 10 29 29 29 29 29 29 11  0  1 31 -1 -1 -1 -1 
	-1 15 16 17 31 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 15 16 17 31 -1 -1 -1 -1 -1 -1 15 16 17 26 13 14 -1 -1 
	-1 15  0  1 31 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 15  0  1 31 -1 -1 -1 -1 -1 -1 15  0  1  2  3 31 -1 -1 
	-1 15 16 17 26 13 13 13 13 13 13 13 13 13 13 13 13 13 14 -1 -1 -1 -1 15 16 17 31 -1 -1 -1 -1 -1 -1 15 16 17 18 19 31 -1 -1 
	-1 15  0  1  0  1  0  1  0  1  0  1  0  1  0  1  0  1 31 -1 -1 -1 -1 15  0  1 31 -1 -1 -1 -1 -1 -1 15  0  1 10 29 30 -1 -1 
	-1 15 16 17 16 17 16 17 16 17 16 17 16 17 16 17 16 17 31 -1 -1 12 13 27 16 17 26 13 13 13 13 13 13 27 16 17 31 -1 -1 -1 -1 
	-1 15  0  1 10 29 29 29 29 29 29 29 29 29 29 11  0  1 31 -1 -1 15  0  1  0  1  0  1  0  1  0  1  0  1  0  1 31 -1 -1 -1 -1 
	-1 15 16 17 31 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 15 16 17 26 13 13 27 16 17 16 17 16 17 16 17 16 17 16 17 16 17 31 -1 -1 -1 -1 
	-1 15  0  1 31 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 15  0  1  0  1  0  1  0  1 10 29 29 29 29 29 29 29 29 11  0  1 31 -1 -1 -1 -1 
	-1 15 16 17 26 13 13 13 13 13 13 13 14 -1 -1 15 16 17 16 17 16 17 16 17 31 -1 -1 -1 -1 -1 -1 -1 -1 15 16 17 31 -1 -1 -1 -1 
	-1 15  4  1  0  1  0  1  0  1  0  1 31 -1 -1 28 29 29 29 29 29 29 29 29 30 -1 -1 -1 -1 -1 -1 -1 -1 15  0  1 31 -1 -1 -1 -1 
	-1 15 16 17 16 17 16 17 16 17 16 17 31 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 15 16 17 31 -1 -1 -1 -1 
	-1 28 29 29 29 29 29 29 29 11  0  1 31 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 15  0  1 31 -1 -1 -1 -1 
	-1 -1 -1 -1 -1 -1 -1 -1 -1 15 16 17 26 13 13 13 13 13 13 13 13 13 13 13 13 13 13 13 13 13 13 13 13 27 16 17 31 -1 -1 -1 -1 
	-1 -1 -1 -1 -1 -1 -1 -1 -1 15  0  1  0  1  0  1  0  1  0  1  0  1  0  1  0  1  0  1  0  1  0  1  0  1  0  1 31 -1 -1 -1 -1 
	-1 -1 -1 -1 -1 -1 -1 -1 -1 15 16 17 16 17 16 17 16 17 16 17 16 17 16 17 16 17 16 17 16 17 16 17 16 17 16 17 31 -1 -1 -1 -1 
	-1 -1 -1 -1 -1 -1 -1 -1 -1 28 29 29 29 29 29 29 29 29 29 29 29 29 29 29 29 29 29 29 29 29 29 29 29 29 29 29 30 -1 -1 -1 -1 
	-1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 
	-1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 

:const floor    0
:const goalpad  2
:const spawnpad 4
:const +infinity  10000000

######################################################
##
## Pathfinding Logic:
##
######################################################

# since we'll be doing recursion,
# we need to beef these up a bit.
:array data-stack   800 0
:array return-stack 800 0
:array path 300 0

:data xd   0  1  0 -1
:data yd  -1  0  1  0

: path@ ( x y -- addr )
	20 * + path +
;

: clear-path
	14 for
		19 for
			-1 i j path@ !
			i 2 * j 2 * tile-grid@ @
			dup floor    = if +infinity i j path@ ! then
			dup spawnpad = if +infinity i j path@ ! then
			    goalpad  = if  0        i j path@ ! then
		next
	next
;

: min-path ( sx sy -- )
	2dup path@ @ 1 + >r
	3 for
		over xd i + @ + # nx
		over yd i + @ + # ny
		2dup path@ @ j > if
			(sx sy nx ny | index path[sx,sy]+1)
			2dup path@ j swap !
			min-path
		else
			2drop
		then
	next
	r> drop 2drop
;

: find-path
	clear-path
	14 for 19 for
		i j path@ @ -if i j min-path then
	next next
;

: print-path
	# note that this will be flipped
	# both vertically and horizontally.
	14 for
		19 for
			i j path@ @
			dup 0 < if "   " type drop
			else dup 10 < if space then . then
		next
		cr
	next
;

######################################################
##
## Sprite Sorting
##
######################################################

# Any entity state should go here
# so I remember to sort it, too.
:array dirs      257 0
:array sprite-id 257 0

: init-sprites
	255 for
		i i sprite-id + !
	next
;

: sort-sprites ( -- )
	255 for
		i
		i for
			dup py i py <
			if drop i then
		next

		# swap mappings in the id table:
		dup sprite-id indexof
		  i sprite-id indexof swap@

		# swap entries in the dirs table:
		dup dirs +
		  i dirs + swap@

		# swap entries in the types table:
		dup types +
		  i types + swap@

		# swap entries in the timers table:
		dup timers +
		  i timers + swap@

		# swap sprite registers:
		sprite@ i sprite@
		3 for
			over i +
			over i +
			swap@
		next
		2drop
	next
;

######################################################
##
## Entity Logic:
##
######################################################

: spawn (frame rel-x rel-y src-id -- id)
	dup py swap px
	>r + swap r> +
	alloc >r
	i sprite@ .sprite-x !
	i sprite@ .sprite-y !
	i sprite@ .sprite-t !
	16x24 i sprite@ !
	r>
;

: at-goal ( id -- flag )
	dup px     8 / swap
	    py 8 + 8 / tile-grid@ @
	goalpad =
;

: delta-dist ( id delta -- dist )
	>r >r
	i  px     16 / j  xd + @ + #x
	r> py 8 + 16 / r> yd + @ + #y
	path@ @ dup 0 < if drop +infinity then
;

#:data face-frame     4     0     2     0
:data face-frame    10     6     8     6
:data face-stat  16x24 73985 16x24 16x24

: face-dir ( id -- )
	>r
	i dirs + @
	dup face-frame + @  i tile!
	    face-stat  + @ r> sprite@ !
;

: pick-dir ( id -- )
	>r
	3 for
		j j dirs + @ delta-dist
		j i            delta-dist >
		if i j dirs + ! then
	next
	i face-dir
	10 i timers + !
	r> drop
;

: move-dir ( id -- )
	>r
	i dirs + @ xd + @ i sprite@ .sprite-x +@
	i dirs + @ yd + @ i sprite@ .sprite-y +@
	r> drop
;

: crawler
	>r
	i at-goal if i free r> drop exit then

	i px     16 mod 0 =
	i py 8 - 16 mod 0 = and
	if i pick-dir then

	i move-dir

	i timers + dup dec@ @ -if
		i tile 1 xor i tile!
		10 i timers + !
	then
	
	r> drop
;

: spawner
	255 random if drop exit then
	>r 0 0 0 r> spawn
	' crawler type!
;

: create-spawners
	14 for 19 for
		i 2 * j 2 * tile-grid@ @ spawnpad = if
			alloc dup ' spawner type!
			i 16 *     over px!
			j 16 * 8 - over py!
		then
	next next
;

######################################################
##
## Main Game Logic:
##
######################################################

: main
	init-sprites
	find-path
	print-path
	create-spawners

	loop
		think
		sort-sprites
		sync
	again
;