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
##  returns a flag to indicate if a 16x32 sprite is
##  currently colliding with the terrain, calibrated
##  for 3:4 perspective.
##  More generally, c-sprite? returns true if a point
##  lies within a given sprite, respecting its current
##  location and size.
##
##  John Earnest
##
######################################################

:image grid-tiles "LabTiles.png" 8 8
:data  grid

	-1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 
	-1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 
	-1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 15 15 15 15 15 15 15 15 15 15 15 15 15 15 15 15 15 15 15 15 15 15 15 15 -1 -1 -1 -1 -1 
	-1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 15 10  9 11 29 11 10 28 11 14 11 11 15 15 30 30  8  8  8  8  8  8  8 15 -1 -1 -1 -1 -1 
	-1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 15 12 10 28 10 14 11 11 12 10 14 28 15 15 30 30  8  8  8  8  8  8  8 15 -1 -1 -1 -1 -1 
	-1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 15 10 28 13 10 29 12 10 12 13 28 12 15 15 30 30  8  8  8  8  8  8  8 15 -1 -1 -1 -1 -1 
	-1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 15  9 10 28 12 11 11  8 13 12 29 12 15 15 30 30  8  8  8  8  8  8  8 15 -1 -1 -1 -1 -1 
	-1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 15  5  5  1  1  1  1  1  1  1  1  1 15 15  5  5  1  1  1  1  1  1  1 15 -1 -1 -1 -1 -1 
	-1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 15  5  5  1  1  1  1  1  1  1  1  1 15 15  5  5  1  1  1  1  1  1  1 15 -1 -1 -1 -1 -1 
	-1 -1 -1 -1 15 15 15 15 15 15 15 15 15  5  5  1  1  1  1  1  1  1  1  1 15 15  5  5  1  1  1  1  1  1  1 15 -1 -1 -1 -1 -1 
	-1 -1 -1 -1 15 13 11 11 29 28 11 15 15  5  5  1  1  1  1  1  1  1  1  1 28 10  5  5  1  1  1  1  1  1  1 15 -1 -1 -1 -1 -1 
	-1 -1 -1 -1 15 28 29 13 11 11 28 15 15  5  5  1  1  1  1  1  1  1  1  1 11 11  5  5  1  1  1  1  1  1  1 15 -1 -1 -1 -1 -1 
	-1 -1 -1 -1 15 28 12 28 28 12 11 28 12  5  5  1  1  1  1  1  1  1  1  1 28 11  5  6  1  1  1  1  1  1  1 15 -1 -1 -1 -1 -1 
	-1 -1 -1 -1 15 12 13 11 12 11 10 28 12  5  5  1  1  1  1  1  1  1  1  1 11 10  6  1  1  1  1  1  1  1  1 15 -1 -1 -1 -1 -1 
	-1 -1 -1 -1 15  5  5  1  1  1  1 13 11  5  6  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1 15 -1 -1 -1 -1 -1 
	-1 -1 -1 -1 15  5  5  1  1  1  1 12 28  6  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1 15 -1 -1 -1 -1 -1 
	-1 -1 -1 -1 15  5  5  1  1  1  1  7  7  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1 15 -1 -1 -1 -1 -1 
	-1 -1 -1 -1 15  5  5  1  1  1  1  7  7  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1 15 -1 -1 -1 -1 -1 
	-1 -1 -1 -1 15  5  5  1  1  1  1  7  7  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1 15 -1 -1 -1 -1 -1 
	-1 -1 -1 -1 15  5  5  1  1  1  1  7  7  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1 15 -1 -1 -1 -1 -1 
	-1 -1 -1 -1 15  5  5  1  1  1  1 15 15  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1 15 -1 -1 -1 -1 -1 
	-1 -1 -1 -1 15 15 15 15 15 15 15 15 15 15 15 15 15 15 15 15 15 15 15 15 15 15 15 15 15 15 15 15 15 15 15 15 -1 -1 -1 -1 -1 
	-1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 
	-1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1
	-1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 
	-1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 
	-1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1  
	-1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 
	-1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 
	-1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 
	-1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 

:image sprite-tiles "Scrubby.png" 16 32
:array sprites 1024 

:include "../Sprites.fs"
:include "../Print.fs"

:  rot   >r swap r> swap ; # (a b c -- b c a)
: -rot   swap >r swap r> ; # (a b c -- c a b)
: swap@  2dup @ >r @ swap ! r> swap ! ;

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
	swap  1 + swap 24 +
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

:data sprite-id
	 0  1  2  3  4  5  6  7
	 8  9 10 11 12 13 14 15

: indexof (value array)
	loop
		2dup @ xor
		-if swap drop exit then
		1 +
	again
;

# sort sprite drawing orders by their
# y-coordinates for simulated perspective.
# sprite 'ids' should be fetched through
# the sprite-id table, which is sorted
# in lockstep with sprite registers.
: sort-sprites

	15 for
		i
		i for
			dup py i py <
			if drop i then
		next

		# swap mappings in the id table:
		dup sprite-id indexof
		i sprite-id indexof swap@

		# swap a pair of sprite entries:
		sprite@ i sprite@
		3 for
			over i +
			over i +
			swap@
		next
		2drop

	next
;

# What offset needs to be added to an
# ASCII character to get the appropriate
# grid tile index?
:const text-offset 48

# Draw a series of strings to the display.
# The address provided should point to a
# line count followed by a series of
# null-terminated strings.
: showtext (msg* -- )
	(msg* --)
	dup @
	(msg* lines)
	swap 1 + swap
	(string* lines)
	for
		29 i - 41 * 8 + GP @ +
		(string* [41*[i-29]]+4+m[GP])
		>r
		loop
			(string* | grid*)
			dup @ dup
			(string* char char | grid)
			-if drop break then
			
			text-offset + i !
			1 + r> 1 + >r
		again
		r> drop 1 +
	next
;

: player 0 sprite-id + @ ;
: janet  1 sprite-id + @ ;
: bill   2 sprite-id + @ ;
: meg    3 sprite-id + @ ;

:var   flipcnt
:var   clipcnt

:data   helo 4
:string $ "Shouldn't you be, like,"
:string $ "cleaning or something?"
:string $ " "
:string $ " "

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
	16x32 player sprite@ !
	  0 player sprite@ .sprite-t !
	160 player px!
	120 player py!

	# init npc1
	16x32 janet sprite@ !
	 10 janet sprite@ .sprite-t !
	120 janet px!
	 60 janet py!
	200 flipcnt !

	# init npc2
	16x32 bill sprite@ !
	 16 bill sprite@ .sprite-t !
	250 bill px!
	 70 bill py!
	300 clipcnt !

	# init npc3
	16x32 meg sprite@ !
	 20 meg sprite@ .sprite-t !
	 60 meg px!
	130 meg py!

	helo showtext

	loop

		keys key-lf and if player px 1 - player c-px! player sprite@ face-left  then
		keys key-rt and if player px 1 + player c-px! player sprite@ face-right then
		keys key-up and if player py 1 - player c-py! then
		keys key-dn and if player py 1 + player c-py! then

		# animate npc1
		flipcnt @
		if
			flipcnt @ 1 - flipcnt !
		else
			janet sprite@ flip-h
			RN @ 600 mod 200 + flipcnt !
		then

		# animate npc2
		clipcnt @
		if
			clipcnt @ 1 - clipcnt !
		else
			bill sprite@ .sprite-t @ 16 xor
			-if  # clipboard is currently up
				17 bill sprite@ .sprite-t !
				50 clipcnt !
			else # clipboard is currently down
				16 bill sprite@ .sprite-t !
				RN @ 600 mod 200 + clipcnt !
			then
		then

		keys key-a and if
			#65 CO !
			1 player sprite@ .sprite-t ! 10 for sync next
			2 player sprite@ .sprite-t ! 10 for sync next
			3 player sprite@ .sprite-t ! 10 for sync next
			2 player sprite@ .sprite-t ! 10 for sync next
			1 player sprite@ .sprite-t ! 10 for sync next
			0 player sprite@ .sprite-t !
		then

		sort-sprites

		sync
	again
;