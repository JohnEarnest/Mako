######################################################
##
##  TileEngine:
##
##  A demonstration of using the Mako grid for
##  tile-based top-down games with collision.
##
##  The words c-px! and c-py! only update the position
##  of a sprite if doing so does not cause the sprite
##  to collide with the terrain. The word c-ground?
##  returns a flag to indicate if a 16x32 sprite is
##  currently colliding with the terrain, calibrated
##  for 3:4 perspective.
##
##  John Earnest
##
######################################################

:array solid-tiles 512 0xFF000000
:image sprite-tiles "Scrubby.png"  16 32
:image grid-tiles   "LabTiles.png"  8  8
:array sprites 1024 0

:include "../Sprites.fs"
:include "../Util.fs"
:include "../Print.fs"

# How many actors do we need to deal with
# on the screen at once? (counts from zero)
:const actor-limit 15

:data sprite-id
	 0  1  2  3  4  5  6  7
	 8  9 10 11 12 13 14 15

: player 0 sprite-id + @ ;

: tile@ ( x y -- tile-address )
	# tile-addr = ((y/8) * (41 + m[GS])) + (x/8) + m[GP]
	8 / GS @ 41 + * swap 8 / + GP @ +
;

: c-tile? ( x y -- flag )
	# By following the simple rule that tiles
	# on the left side of the tile sheet are
	# passable and the tiles on the right side
	# are impassible, there's no need to
	# store collision data separately or
	# use a complex lookup table:

	tile@ @ 16 mod 7 >
;

: c-ground? ( sprite-id -- flag )
	sprite@ dup .sprite-x @ swap .sprite-y @
	swap  1 + swap 24 +
	over 13 + over  7 + c-tile?       >r
	over 13 + over      c-tile? r> or >r
	over  7 + over  7 + c-tile? r> or >r
	over  7 + over      c-tile? r> or >r
	over      over  7 + c-tile? r> or >r
	                    c-tile? r> or
;

:array sprite-solid 256 0
: solid? sprite-solid + @ ; ( sprite-id -- flag )
: solid! sprite-solid + ! ; ( flag sprite-id -- )

: c-npc? ( id-a id-b -- flag )
	over py over py  6 - >=        >r
	over py over py  6 + <  r> and >r
	over px over px 14 - >= r> and >r
	over px over px 14 + <  r> and >r
	2drop r>
;

: c-npcs? ( sprite-id -- flag )
	actor-limit for
		# do not check if a sprite
		# collides with itself or
		# is not defined as 'solid'
		dup i xor i solid? and if
			dup i c-npc?
			if r> 2drop true exit then
		then
	next
	drop false
;

: c-px! ( x sprite-id )
	dup px >r dup >r px! r> r> swap
	dup c-ground? over c-npcs? or
	if px! else 2drop then
;

: c-py! ( x sprite-id )
	dup py >r dup >r py! r> r> swap
	dup c-ground? over c-npcs? or
	if py! else 2drop then
;

: move-player ( -- )
	keys key-lf and if player px 1 - player c-px! player face-left  then
	keys key-rt and if player px 1 + player c-px! player face-right then
	keys key-up and if player py 1 - player c-py! then
	keys key-dn and if player py 1 + player c-py! then
;

:array sprite-trigger 256 0
: trigger? sprite-trigger + @ ; ( sprite-id -- addr )
: trigger! sprite-trigger + ! ; ( addr sprite-id -- )

# return true if the player is in
# the right position and facing direction
# for a 'use action' to activate an npc:
: use-object ( sprite -- flag )
	>r
	player sprite@ @ sprite-mirror-horiz and
	if   # facing right
		player px 24 +
		player py 24 +
	else # facing left
		player px  8 -
		player py 24 +
	then
	r> c-sprite?
;

: use-prompt
	false actor-limit for
		i use-object i trigger? and
		if drop true then
	next
	if 85 else -1 then
	GP @ 1186 + !
;

: indexof (value array -- address)
	loop
		2dup @ =
		if swap drop exit then
		1 +
	again
;

# sort sprite drawing orders by their
# y-coordinates for simulated perspective.
# sprite 'ids' should be fetched through
# the sprite-id table, which is sorted
# in lockstep with sprite registers.
: sort-sprites

	actor-limit for
		i
		i for
			dup py i py <
			if drop i then
		next

		# swap mappings in the id table:
		dup sprite-id indexof
		  i sprite-id indexof swap@

		# swap entries in the solid table:
		dup sprite-solid +
		  i sprite-solid + swap@

		# swap entries in the trigger table:
		dup sprite-trigger +
		  i sprite-trigger + swap@

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

# wait for key-a to be pressed
# and then released before continuing.
: wait ( -- )
	85 GP @ 1186 + !
	loop keys key-a and  if break then sync again
	loop keys key-a and -if break then sync again
	-1 GP @ 1186 + !
;

# What offset needs to be added to an
# ASCII character to get the appropriate
# grid tile index?
:const text-offset  48
:const text-start-x  3
:const text-start-y 25

# Draw a series of strings to the display.
# The address provided should point to a
# line count followed by a series of
# null-terminated strings.
: show-text (msg* -- )

	dup @ swap 1 + swap 1 -
	for
		28 i - 41 * text-start-x + GP @ + >r
		loop
			dup @ dup
			-if drop break then			
			text-offset + i !

			# sync while the text is drawn
			# to create a 'typewriter' effect:
			sync
			1 + r> 1 + >r
		again
		r> drop 1 +
	next
	drop
	
	wait

	# clear the text area
	163 for
		-1 GP @ text-start-y 41 * + i + !
	next
;

# Using the highest 120 sprites, create
# a venetian blinds-like transition effect.
# The solid-color sprites are produced
# by padding memory before the sprite sheet.
: init-blinds
	119 for
		64x8 -1
		i 2 / 5 mod 64 * i 2 mod if -320 else 320 then + # x-position 
		i 2 / 5 / 16 * i 2 mod if 8 + then               # y-position
		i 136 + >sprite
	next
;

: animate-blinds
	80 for
		119 for
			i 136 + sprite@ .sprite-x
			dup @ 4 i 2 mod if + else - then swap !
		next
		sync
	next
;

# Load a new map. Every map has an init routine
# that sets up any sprites and local state before
# the map can be 'run' and a main loop. The map
# transition is itself a subroutine call, so this
# method both transitions to the new map and handles
# restoring everything for the return trip.
# Before calling this routine, the init address
# of the current map should be on the stack, and
# this routine will do the same for subsequent calls.
: load-map ( 'current-init 'next-main 'next-init -- 'current-init)
	init-blinds
	animate-blinds   # fade to black
	player py >r     # store the player's position
	player px >r
	dup exec         # call the next map's init
	animate-blinds   # fade in next map
	swap exec        # call the next map's main
	drop             # discard the next map's init addr
	init-blinds
	animate-blinds   # fade to black
	dup exec         # call the original map's init
	r> player px!    # restore player position
	r> player py!
	animate-blinds   # fade back to original map
;

: face-player ( sprite -- )
	dup px player px >
	if face-left else face-right then
;

: player-center ( -- x y )
	player px 8 +
	player py 28 +
;

: use-logic
	keys key-a and if
		0
		15 for
			i use-object if i . cr drop i trigger? then
		next
		dup if exec
		else
			drop
			1 player tile! 10 for sync next
			2 player tile! 10 for sync next
			3 player tile! 10 for sync next
			2 player tile! 10 for sync next
			1 player tile! 10 for sync next
			0 player tile!
		then
	then
;

# break out of a map's main loop when invoked
# as a use-action trigger. This will be used
# most frequently as the trigger code for
# doors that return to a source room.
: use-return r> r> 2drop ;

: >actor   swap over solid! >sprite ; (status tile x y solid? sprite-id -- )
: actor>   dup >r sprite> r> solid? ; (sprite-id -- status tile x y solid? )
: clear-actors   actor-limit for 0 0 0 0 0 i >actor 0 i trigger! next ;

# Every map is defined in a separate file,
# with two entrypoints- one 'load' routine and
# a main loop. Map files may also contain various
# one-off scripts and special purpose logic.

:include "StorageCloset.fs"
:include "StartingRoom.fs"

:string hello "Hello, World!"

: main
	hello typeln

	' load-starting-room dup exec
	main-starting-room

	#load-storage-closet
	#main-storage-closet
;