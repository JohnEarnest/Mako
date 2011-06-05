######################################################
##
##  TileEngine:
##
##  A demonstration of using the Mako grid for
##  tile-based top-down games with collision.
##
##  The words c-px! and c-py! only update the position
##  of a sprite if doing so does not cause the sprite
##  to collide with the terrain. The word c-actor?
##  returns a flag to indicate if a 16x24 sprite is
##  currently colliding with the terrain, calibrated
##  for 3:4 perspective.
##  More generally, c-sprite? returns true if a point
##  lies within a given sprite, respecting its current
##  location and size.
##
##  John Earnest
##
######################################################

:image grid-tiles "protoTiles.png" 8 8
:data  grid

	9 9 9 9 9 9 9 9 9 9 9 9 9 9 9 9 9 9 9 9 9 9 9 9 9 9 9 9 9 9 9 9 9 9 9 9 9 9 9 9	0
	9 9 9 9 9 9 9 9 9 9 9 9 9 9 9 9 9 9 9 9 9 9 9 9 9 9 9 9 9 9 9 9 9 9 9 9 9 9 9 9	0
	9 9 9 9 9 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 9 9 9 9 9	0
	9 9 9 9 9 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 9 9 9 9 9	0
	9 9 9 9 9 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 9 9 9 9 9	0
	9 9 9 9 9 0 0 0 9 0 0 0 0 9 9 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 9 9 9 9 9	0
	9 9 9 9 9 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 9 9 9 9 9	0
	9 9 9 9 9 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 9 9 9 9 9	0
	9 9 9 9 9 0 0 0 16 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 9 9 9 9 9	0
	9 9 9 9 9 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 9 9 9 9 9	0
	9 9 9 9 9 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 9 9 9 9 9	0
	9 9 9 9 9 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 9 9 9 9 9	0
	9 9 9 9 9 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 9 9 9 9 9	0
	9 9 9 9 9 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 9 9 9 9 9	0
	9 9 9 9 9 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 9 9 9 9 9	0
	9 9 9 9 9 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 9 9 9 9 9	0
	9 9 9 9 9 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 9 9 9 9 9	0
	9 9 9 9 9 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 9 9 9 9 9	0
	9 9 9 9 9 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 9 9 9 9 9	0
	9 9 9 9 9 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 9 9 9 9 9	0
	9 9 9 9 9 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 9 9 9 9 9	0
	9 9 9 9 9 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 9 9 9 9 9	0
	9 9 9 9 9 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 9 9 9 9 9	0
	9 9 9 9 9 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 9 9 9 9 9	0
	9 9 9 9 9 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 9 9 9 9 9	0
	9 9 9 9 9 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 9 9 9 9 9	0
	9 9 9 9 9 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 9 9 9 9 9	0
	9 9 9 9 9 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 9 9 9 9 9	0
	9 9 9 9 9 9 9 9 9 9 9 9 9 9 9 9 9 9 9 9 9 9 9 9 9 9 9 9 9 9 9 9 9 9 9 9 9 9 9 9	0
	9 9 9 9 9 9 9 9 9 9 9 9 9 9 9 9 9 9 9 9 9 9 9 9 9 9 9 9 9 9 9 9 9 9 9 9 9 9 9 9	0
	0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0

:image sprite-tiles "spriteTemplate.png" 16 24
:array sprites 1024

: sprite@    4 * sprites + ;
: .sprite-t  1 + ;
: .sprite-x  2 + ;
: .sprite-y  3 + ;
: .sprite-w  @ 0x0F00 and  256 / 1 + 8 * ;
: .sprite-h  @ 0xF000 and 4096 / 1 + 8 * ;
: px         sprite@ .sprite-x @ ;
: py         sprite@ .sprite-y @ ;
: px!        sprite@ .sprite-x ! ;
: py!        sprite@ .sprite-y ! ;

:  rot   >r swap r> swap ; # (a b c -- b c a)
: -rot   swap >r swap r> ; # (a b c -- c a b)

: c-sprite? ( x y sprite-id -- flag )

	rot >r sprite@ >r
	i .sprite-y @
	2dup i .sprite-h +
	<= -rot >= and
	
	r> r> swap >r
	i .sprite-x @
	2dup r> .sprite-w +
	<= -rot >= and

	and
;

: tile@ ( x y -- tile-index )
	# tile-id = m[((y/8) * (41 + m[GS])) + (x/8) + m[GP]]
	8 / GS @ 41 + * swap 8 / + GP @ + @
;

: c-tile? ( x y -- flag )
	# By following the simple rule that tiles
	# on the left side of the tile sheet are
	# passable and the tiles on the right side
	# are impassible, there's no need to
	# store collision data separately or
	# use a complex lookup table:

	tile@ 16 mod 7 >
;

: c-actor? ( sprite-id -- flag )
	sprite@ dup .sprite-x @ swap .sprite-y @
	swap  1 + swap 16 +
	over 13 + over  7 + c-tile? >r
	over 13 + over      c-tile? r> or >r
	over  7 + over  7 + c-tile? r> or >r
	over  7 + over      c-tile? r> or >r
	over      over  7 + c-tile? r> or >r
	                    c-tile? r> or
;

: c-px! ( x sprite-id )
	dup px >r dup >r px!
	r> r> swap dup c-actor?
	if px! else 2drop then
;

: c-py! ( x sprite-id )
	dup py >r dup >r py!
	r> r> swap dup c-actor?
	if py! else 2drop then
;

:const player 0
:const face-dn 0
:const face-up 1
:const face-lf 2
:const face-rt 3

# pairs of x/y coordinates relative to the sprite-
# the offset of the 'trigger point' when the player
# is facing in various directions.
:data face-offset
	 8 28 # down
	 8 12 # up
	-4 20 # left
	20 20 # right

:var face

: main

(
	# test c-sprite?
	# you must include "../Print.fs"
	32x32 1 sprite@ !
	32  1 px!
	64  1 py!
	 32  64 1 c-sprite? . # -1 expected
	 35  68 1 c-sprite? . # -1 expected
	 68  35 1 c-sprite? . #  0 expected
	100 100 1 c-sprite? . #  0 expected
	cr
)

	# init sprite
	16x24 player sprite@ !
	160 player px!
	120 player py!

	loop

		keys key-lf and if player px 1 - player c-px! face-lf face ! then
		keys key-rt and if player px 1 + player c-px! face-rt face ! then
		keys key-up and if player py 1 - player c-py! face-up face ! then
		keys key-dn and if player py 1 + player c-py! face-dn face ! then

		face @ player sprite@ .sprite-t !

		sync
	again
;