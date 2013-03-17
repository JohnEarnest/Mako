######################################################
##
##  TileEngine:
##
##  A demonstration of using the Mako grid for
##  tile-based top-down games with collision.
##
##  John Earnest
##
######################################################

:array solid-tiles 512 0xFF000000
:image sprite-tiles "Scrubby.png"  16 32
:image grid-tiles   "LabTiles.png"  8  8
:image basic-font   "text.png"      8  8
:array sprites 1024 0

:include <Grid.fs>
:include <Sprites.fs>
:include <Util.fs>
:include <Print.fs>
:include <String.fs>

# How many actors do we need to deal with
# on the screen at once? (counts from zero)
:const actor-limit 15

:data sprite-id
	 0  1  2  3  4  5  6  7
	 8  9 10 11 12 13 14 15

: player 0 sprite-id + @ ;

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

: c-px! ( x sprite-id -- )
	dup px >r dup >r px! r> r> swap
	dup c-ground? over c-npcs? or
	if px! else 2drop then
;

: c-py! ( x sprite-id -- )
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
	dup player = if drop false exit then
	>r
	player sprite@ @ sprite-mirror-horiz and
	if   # facing right
		player px 24 +
		player py 24 +
	else # facing left
		player px  8 -
		player py 24 +
	then
	i c-sprite?
	player px  8 +
	player py 24 +
	r> c-sprite? or
;

: prompt-cell? 38 28 tile-grid@ @ ; ( -- tile )
: prompt-cell! 38 28 tile-grid@ ! ; ( tile -- )
:var prompt-buffer

: use-prompt ( -- )
	false
	actor-limit for
		i use-object i trigger? and
		if drop true then
	next

	if
		prompt-cell?
		dup 85 xor -if drop else prompt-buffer ! then
		85 prompt-cell!
	else
		prompt-cell?
		85 xor -if prompt-buffer @ prompt-cell! then
	then
;

# sort sprite drawing orders by their
# y-coordinates for simulated perspective.
# sprite 'ids' should be fetched through
# the sprite-id table, which is sorted
# in lockstep with sprite registers.
: sort-sprites ( -- )

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
	prompt-cell?
	85 prompt-cell!
	loop keys key-a and -if break then sync again
	loop keys key-a and  if break then sync again
	loop keys key-a and -if break then sync again
	prompt-cell!
;

# What offset needs to be added to an
# ASCII character to get the appropriate
# grid tile index?
:const text-offset  224
:const text-start-x   3
:const text-start-y  25
:array text-buffer  240 6

# Draw a series of strings to the display.
# The address provided should point to a
# line count followed by a series of
# null-terminated strings.
: show-text (msg* -- )

	5 for
		39 for
			i j text-start-y + tile-grid@ @
			i j 40 * + text-buffer + !
			-1 i j text-start-y + tile-grid@ !
		next
	next

	dup @ 1 - swap 1 + swap
	for
		text-start-x 28 i - tile-grid@ >r
		loop
			dup @ dup
			-if drop break then
			text-offset + i !

			# sync while the text is drawn
			# to create a 'typewriter' effect:
			sync
			1 + inc-r
		again
		r> drop 1 +
	next
	drop
	wait
	
	5 for
		39 for
			i j 40 * + text-buffer + @
			i j text-start-y + tile-grid@ !
		next
	next
;

# Using the highest 150 sprites, create
# a venetian blinds-like transition effect.
# The solid-color sprites are produced
# by padding memory before the sprite sheet.
: init-blinds
	149 for
		64x8 -1
		i 2 / 5 mod 64 * i 2 mod if -320 else 320 then + # x-position 
		i 2 / 5 / 16 * i 2 mod if 8 + then               # y-position
		i 106 + >sprite
	next
;

: animate-blinds
	80 for
		149 for
			i 106 + sprite@ .sprite-x
			dup @ 4 i 2 mod if + else - then swap !
		next
		sync
	next
;

:var room-width
:var room-height
:var room-start

: >room ( w h grid -- )
	dup GP !
	room-start !
	room-height !
	dup room-width !
	1 - 40 * GS !
;

: +sprites-x ( x -- )
	actor-limit for
		dup i sprite@ .sprite-x +@
	next
	drop
;

: +sprites-y ( y -- )
	actor-limit for
		dup i sprite@ .sprite-y +@
	next
	drop
;

# When the player walks off the edge of the board,
# animate a transition to the adjacent board.
: scroll-room ( -- )
	player px 312 > if
		player px 16 + player px!
		39 for
			GP inc@
			-8 +sprites-x
			sync
		next
	then
	player px 4 < if
		player px 28 - player px!
		39 for
			GP dec@
			8 +sprites-x
			sync
		next
	then
	player py 212 > if
		player py 20 + player py!
		29 for
			room-width @ 40 * 1 + GP +@
			-8 +sprites-y
			sync
		next
	then
	player py -8 < if
		player py 32 - player py!
		29 for
			room-width @ 40 * 1 + GP -@
			8 +sprites-y
			sync
		next
	then
;

# Update the positions of sprites
# to reflect the room the player is in.
# Call this if the player teleports
# into a room larger than a single board.
: fix-pos ( -- )
	GP @ room-start @ -   # index into map
	room-width @ 40 * 1 + # width of map in cells
	/mod
	40 / -320 * +sprites-x
	30 / -240 * +sprites-y
;

# Move the GP to a specific sub-board of a room.
: room-board (x y -- )
	30 * room-width @ 40 * 1 + *
	swap 40 * +
	room-start @ + GP !
	fix-pos
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
	GP @ >r          # store the grid pointer
	dup exec         # call the next map's init
	animate-blinds   # fade in next map
	swap exec        # call the next map's main
	drop             # discard the next map's init addr
	init-blinds
	animate-blinds   # fade to black
	dup exec         # call the original map's init
	r> GP !          # restore the grid pointer
	fix-pos          # reposition all the sprites
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

: animate-clean ( -- )
	1 player tile! 10 for sync next
	2 player tile! 10 for sync next
	3 player tile! 10 for sync next
	2 player tile! 10 for sync next
	1 player tile! 10 for sync next
	0 player tile!
;

: use-logic
	keys key-a and if
		0
		actor-limit for
			dup 0 = i use-object and if drop i trigger? then
		next
		dup if exec else drop animate-clean then
	then
;

# break out of a map's main loop when invoked
# as a use-action trigger. This will be used
# most frequently as the trigger code for
# doors that return to a source room.
: use-return r> r> 2drop ;

: >actor        swap over solid! >sprite ; (status tile x y solid? sprite-id -- )
: actor>        dup >r sprite> r> solid? ; (sprite-id -- status tile x y solid? )
: clear-actors  actor-limit for 0 0 0 0 0 i >actor 0 i trigger! next ;

# Every map is defined in a separate file,
# with two entrypoints- one 'load' routine and
# a main loop. Map files may also contain various
# one-off scripts and special purpose logic.

:include "StorageCloset.fs"
:include "StartingRoom.fs"
:include "BigRoom.fs"
:include "Lair.fs"

: main
	#' load-starting-room dup exec
	#main-starting-room
	' load-big-room dup exec
	main-big-room
	#' load-lair dup exec
	#main-lair
;