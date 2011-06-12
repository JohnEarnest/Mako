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
:array grid 2601 1

:const grid-height  50
:const grid-width   50
:const grid-skip    9  # width - 41
:const clear-color  0xFFFFFFAA

:include "../Sprites.fs"
:include "../Util.fs"
:include "../Print.fs"

: grid-tile@  GS @ 41 + * swap + GP @ +  ;
: cam-x       GP @ grid - grid-width mod ;
: cam-y       GP @ grid - grid-width /   ;

: scroll-grid-x
	SX @ +
	dup 8 > cam-x grid-width 41 - < and
	if GP inc@ player px 8 - player px! 8 - then
	dup 0 < cam-x 0 > and
	if GP dec@ player px 8 + player px! 8 + then
	SX !
;

: scroll-grid-y
	SY @ +
	dup 8 > cam-y grid-height 31 - < and
	if grid-width GP +@ player py 8 - player py! 8 - then	
	dup 0 < cam-y 0 > and
	if grid-width GP -@ player py 8 + player py! 8 + then
	SY !
;

:const player 0
:const speed  2

: main
	
	# init player sprite
	8x8 93 156 116 player >sprite

	# generate the level
	grid-height 1 - for
		grid-width 1 - for
			i 0 =
			j 0 =
			i grid-width 1 - =
			j grid-height 1 - =
			or or or
			if    2 random 2 +
			else  4 random 5 random 16 * + 4 + then
			i j tile@ !
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