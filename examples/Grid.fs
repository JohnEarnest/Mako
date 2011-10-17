######################################################
##
##  Grid:
##
##  A lexicon for manipulating the Mako grid.
##
##  John Earnest
##
######################################################

:var grid-start
:var grid-width
:var grid-height

:  tile-grid@  GS @ 41 + * swap + GP @ +    ; ( x y -- tile-address )
: pixel-grid@  8 / swap 8 / swap tile-grid@ ; ( x y -- tile-address )

: cam-x        GP @ grid-start @ - grid-width @ mod ;
: cam-y        GP @ grid-start @ - grid-width @ /   ;

# we need a few basics from util and
# the sprite lexicon. Here are pared-down
# versions to avoid cross-dependencies:
: inc@        dup @ 1 + swap !          ;
: dec@        dup @ 1 - swap !          ;
: +@          swap over @ swap + swap ! ;
: -@          swap over @ swap - swap ! ;
: px@         4 * SP @ + 2 +            ;
: py@         4 * SP @ + 3 +            ;

: scroll-grid-x ( x-pixels -- )
	SX @ +
	dup 8 > cam-x grid-width @ 41 - < and
	if GP inc@ 255 for 8 i px@ -@ next 8 - then
	dup 0 < cam-x 0 > and
	if GP dec@ 255 for 8 i px@ +@ next 8 + then
	SX !
;

: scroll-grid-y ( y-pixels -- )
	SY @ +
	dup 8 > cam-y grid-height @ 31 - < and
	if grid-width @ GP +@ 255 for 8 i py@ -@ next 8 - then	
	dup 0 < cam-y 0 > and
	if grid-width @ GP -@ 255 for 8 i py@ +@ next 8 + then
	SY !
;

: c-tile? ( x y -- flag )
	# By following the simple rule that tiles
	# on the left side of the tile sheet are
	# passable and the tiles on the right side
	# are impassible, there's no need to
	# store collision data separately or
	# use a complex lookup table:

	pixel-grid@ @ 16 mod 7 >
;

:data ascii -32

# draw a right-aligned number to the grid
# starting with the rightmost digit at the specified x/y.
# does not clear space, so consider first using 'fill'.
: draw-number (x y n -- )
	>r
	loop
		2dup tile-grid@ i 10 mod
		ascii @ + 48 + swap !                # draw digit
		swap 1 - swap                        # move cursor
		r> 10 / >r i                         # cast out digits
	while
	2drop r> drop
;

# draw a null-terminated string to the grid.
: grid-type (x y addr -- )
	>r tile-grid@ r> swap
	loop
		(string grid)
		over @ ascii @ + over !
		swap 1 + swap 1 +
		over @
	while
	2drop
;