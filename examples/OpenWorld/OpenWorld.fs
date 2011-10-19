######################################################
##
##  OpenWorld:
##
##  A demonstration of using the Mako grid for
##  a free-scrolling map larger than a single screen.
##  Note that the GS register (grid-skip) must always
##  be initialized to the width of the grid minus 41.
##
##  John Earnest
##
######################################################

:image sprite-tiles "protoTiles.png" 8 8
:array sprites 1024 0
:image grid-tiles "protoTiles.png" 8 8
:array grid 2500 1

:const grid-skip    9  # width - 41
:const clear-color  0xFFFFFFAA

:include "../Grid.fs"
:include "../Sprites.fs"
:include "../Util.fs"

:const player 0
:const speed  2

: main
	
	# init player sprite and grid state
	8x8 93 156 116 player >sprite
	grid grid-start  !
	50   grid-height !
	50   grid-width  !

	# generate the level
	grid-height @ 1 - for
		grid-width @ 1 - for
			i 0 =
			j 0 =
			i grid-width  @ 1 - =
			j grid-height @ 1 - =
			or or or
			if    2 random 2 +
			else
				10 random 2 < if 10 grid-z or
				else 4 random 5 random 16 * + 4 + then
			then
			i j tile-grid@ !
		next
	next

	loop		
		keys key-lf and if player px speed - player px! speed -1 * scroll-grid-x then
		keys key-rt and if player px speed + player px! speed      scroll-grid-x then
		keys key-up and if player py speed - player py! speed -1 * scroll-grid-y then
		keys key-dn and if player py speed + player py! speed      scroll-grid-y then
		sync
	again
;