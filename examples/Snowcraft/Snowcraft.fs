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
:image grid-tiles   "snowtiles.png"  8  8
:data grid

	 4 74  4  4 97  4  4 97 97  4 75 76  4 97 97 97  4 74 45 46 46 46 46 47 46 46 46 46 47 46 46 46 46 47 46 46 46 46 47 46 -1 
	 4 11 42 42 42 42 42 42 42 42 42 42 42 42 42 42  9 97 61 62 62 62 62 63 62 62 62 62 63 62 62 62 62 63 62 62 62 62 63 62 -1 
	97 27  7  7  1  3  1  3  1  3  1  3  1  3  1  3 25 97 77 78 78 78 78 79 78 78 78 78 79 78 78 78 78 79 78 78 78 78 79 78 -1 
	97 27 23 23  4  2  4  2  4  2  4  2  4  2  4  2 25  4 97 26 97 26 97 97 97 11 42 42 42 42 42 42 42 42 42 42 42 42  9 97 -1 
	 4 26 26 28  1  3 12 10 10 28  1  3 12 28  1  3 25 97  4 97 97 97 97 97 97 27  1  3  1  3  1  3  1  3  1  3  1  3 25 97 -1 
	97  4 97 27  4  2 25 96 18 27  4  2 25 27  4  2 41 42 42 42 42 42 42 42 42 43  4  2  4  2  4  2  4  2  4  2  4  2 25 59 -1 
	74 97 97 27  1  3 25 18 96 27  1  3 25 27  1  3  1  3  1  3  1  3  1  3  1  3  1  3 12 26 26 26 26 10 10 28  1  3 25 75 -1 
	97 97 97 27  4  2 41 42 42 43  4  2 25 27  4  2  4  2  4  2  4  2  4  2  4  2  4  2 25 97 74 97 19 19 18 27  4  2 25 97 -1 
	97 18 97 27  1  3  1  3  1  3  1  3 25 26 26 26 26 26 26 26 10 10 10 10 10 28  1  3 25 97 18 19 97 19 19 27  1  3 25 96 -1 
	97 97 97 27  4  2  4  2  4  2  4  2 25 97 97 97 97 74 97 18 18 19 19 96 18 27  4  2 41 42 42 42 42 42  9 27  4  2 25 19 -1 
	97 59 60 26 26 26 26 26 26 28  1  3 25 19 97 19 97 97 18 19 19 96 18 19 18 27  1  3  1  3  1  3  1  3 25 27  1  3 25 18 -1 
	97 75 76 97  3 97  3 74 97 27  4  2 41 42 42 42 42 42 42 42 42 42 42 42  9 27  4  2  4  2  4  2  4  2 41 43  4  2 25 18 -1 
	14 14 14 14 14 14 14 15  3 27  1  3  1  3  1  3  1  3  1  3  1  3  1  3 25 10 10 10 10 10 10 28  1  3  1  3  1  3 25 96 -1 
	30 30 30 30 30 30 30 31 97 27  4  2  4  2  4  2  4  2  4  2  4  2  4  2 25 18 19 18 18 96 18 27  4  2  4  2  4  2 25 18 -1 
	 3 74  3  3  3  3  3 97 18 27  1  3 12 10 10 10 10 10 10 10 10 28  1  3 25 18 18 96 19 19 18 27  1  3 12 10 10 10 10 19 -1 
	97  3  3 97 97 11 42 42 42 43  4  2 41 42 42 42 42 42  9 19 96 27  4  2 41 42 42 42  9 18 96 27  4  2 25 18 19 19 96 19 -1 
	19 97 97 19 18 27  1  3  1  3  1  3  1  3  1  3  1  3 25 18 18 27  7  7  1  3  1  3 25 18 19 27  1  3 25 18 96 18 19 18 -1 
	18 19 19 18 19 27  4  2  4  2  4  2  4  2  4  2  4  2 25 96 18 27 23 23  4  2  4  2 25 18 19 27  4  2 41 42 42 42  9 18 -1 
	18 19 18 18 18 27  1  3 12 10 10 10 10 10 10 28  1  3 25 18 19 10 10 10 10 28  1  3 25 19 96 27  1  3  1  3  0  0 25 18 -1 
	18 18 18 19 18 27  4  2 25 18 96 18 19 96 18 27  4  2 41 42 42 42 42 42 42 43  4  2 25 18 18 27  4  2  4  2  0  0 25 18 -1 
	18 19 19 19 18 27  1  3 25 19 19 18 18 19 18 27  1  3  1  3  1  3  1  3  1  3  1  3 25 18 19 10 10 28  1  3 12 10 10 18 -1 
	18 19 19 18 18 27  4  2 25 18 19 19 96 19 18 27  4  2  4  2  4  2  4  2  4  2  4  2 25 96 19 19 18 27  4  2 25 18 18 18 -1 
	19 18 19 18 19 27  1  3 25 19 18 18 18 18 19 27  1  3 12 10 10 10 10 10 10 10 10 10 10 18 18 96 18 27  1  3 25 19 19 96 -1 
	18 96 19 18 18 27  4  2 41 42 42 42  9 19 19 27  4  2 25 18 18 96 18 18 18 11 42 42 42 42 42 42 18 43  4  2 25 18 19 18 -1 
	19 19 18 19 18 27  1  3  1  3  7  7 25 19 18 27  1  3 25 18 18 18 18 96 18 27  1  3  1  3  1  3  1  3  1  3 25 18 19 19 -1 
	18 19 19 18 18 27  4  2  4  2 23 23 25 18 18 27  4  2 41 42 42 42 42 42 42 43  4  2  4  2  4  2  4  2  4  2 25 18 58 18 -1 
	18 58 18 18 18 10 10 10 10 10 10 10 10 18 19 27  1  3  1  3  1  3  1  3  1  3  1  3 12 10 10 10 10 10 10 10 10 96 18 19 -1 
	18 19 19 19 18 18 19 18 19 18 18 19 19 19 18 27  4  2  4  2  4  2  4  2  4  2  4  2 25 18 18 18 19 18 18 18 18 19 18 19 -1 
	18 18 19 19 19 96 19 18 19 19 96 18 18 18 18 10 10 10 10 10 10 10 10 10 10 10 10 10 10 18 18 96 19 58 19 19 19 19 19 19 -1 
	18 19 19 58 19 18 18 18 18 18 19 18 18 18 18 18 18 18 18 18 18 18 18 18 18 18 18 18 18 18 19 19 19 19 18 96 18 18 18 18 -1 
	-1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1

:const floor    1
:const goalpad  0
:const spawnpad 7
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

:array pathd 300 0
:array path  300 0

: path@ ( x y -- addr )
	20 * + path +
;

: build-path
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

:include "Pathing.fs"

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

:data face-frame     4     0     2     0
#:data face-frame    10     6     8     6
:data face-stat  16x24 73985 16x24 16x24

: face-dir ( id -- )
	>r
	i dirs + @
	dup face-frame + @  i tile!
	    face-stat  + @ r> sprite@ !
;

: pick-dir ( id -- )
	>r
	i px     16 /
	i py 8 + 16 / 20 * + pathd + @
	i dirs + !
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
	build-path
	pathd path find-path
	pathd print-path
	create-spawners

	loop
		think
		sort-sprites
		sync
	again
;