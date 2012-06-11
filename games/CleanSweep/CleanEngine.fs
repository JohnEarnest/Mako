######################################################
##
##  CleanEngine:
##
##  A moderately flexible top-down adventure game
##  engine for "Mr. Scrubby's Clean Sweep".
##
##  John Earnest
##
######################################################

:include <Print.fs>
:include <Grid.fs>
:include <Util.fs>
:include <Sprites.fs>
:include <String.fs>
:include <Vector.fs>
:include <Math.fs>

:proto tick
:include <Game/Dialog.fs>
:include <Game/Transitions.fs>

:const ent-max 32
:include <Game/Entities.fs>

# Various routines will refer to a specific
# sprite index designated as the player.
# this variable must keep an up-to-date index:
:data player -1

# Thuds will be used to keep track of bit
# vectors representing collisions with an entity.
# Triggers will store callbacks for context-sensitive
# action routines the player can activate.
0 :array thuds     ent-max 0 : thud     thuds     + ;
0 :array triggers  ent-max 0 : trigger  triggers  + ;

: ent-clear ( id -- )
	0 over thud    !
	0 swap trigger !
;

: ent-swap ( a b -- )
	over thud    over thud    swap@
	over trigger over trigger swap@
	2drop
;

# This allows us to have small (8x8) sprites
# coexist with large (16x32) sprites and still
# give them the correct y-ordering.
: compare-sprites ( a b -- )
	dup sprite@ @ 8x8 xor -if py 23 - else py then swap
	dup sprite@ @ 8x8 xor -if py 23 - else py then swap -
;

: ent-blocked ( id -- flag )
	drop false
;

: cap-mag ( a cap -- b )
	over sgn 0 < if
		-1 * max
	else
		min
	then
;

: goto   r> drop >r ; ( addr -- )
: dummy  drop       ; ( id -- )

######################################################
##
##  Action indicator
##
######################################################

:const actioni-sprite 128
:const actioni-tile   512

: show-actioni ( -- )
	8x8 actioni-tile 304 224 actioni-sprite >sprite
;

: hide-actioni ( -- )
	actioni-sprite hide
;

######################################################
##
##  Audio kernel
##
######################################################

:vector bfx 0 ; # background sound function

:var sfx  # sound effect function pointer
:var sft  # sound effect timer
:var sfa  # sound effect register A
:var sfb  # sound effect register B

:data atimer 17
: tick ( -- )
	(
	atimer dec@
	atimer @ -if 17 atimer ! sync exit then
	136 for
		bfx
		sfx @ if
			sft dec@
			sfx @ exec 2 * + 3 /
			sft @ -if 0 sfx ! then
		then
		AU !
	next
	)
	
	sync
;

######################################################
##
##  Code for managing rooms and room transitions.
##
##  goto-room    go directly to a room- no animation
##  push-room    transition to room and return later
##  scroll-room  transition to a neighboring room
##
######################################################

:const /room 6
: .room-init      ;
: .room-main  1 + ;
: .room-n     2 + ;
: .room-e     3 + ;
: .room-s     4 + ;
: .room-w     5 + ;

: room-adj ( room* dir -- adjacent* )
	2 / swap .room-n + @
	dup -if "no room to scroll to!" typeln halt then
;

: goto-room ( this-room* room* -- room* )
	hide-actioni
	rdrop     # discard calling function
	swap drop # discard old room

	clear-entities
	dup .room-init @ exec
	dup .room-main @ goto
;

: init-room ( room* -- )
	blinds-out
	clear-entities
	-1 player !
	.room-init @ exec
;

:proto spawn-scrubby
: push-room ( this-room* room* -- this-room* )
	hide-actioni
	player @ py >r
	player @ px >r

	dup init-room
	sort-sprites
	blinds-in

	dup .room-main @ exec
	drop

	blinds-out
	clear-entities
	-1 player !
	0 0 spawn-scrubby
	r> player @ px!
	r> player @ py!
	dup .room-init @ exec
	
	sort-sprites
	blinds-in
;

:data pan-dx  0 -320 0 320
:data pan-dy  240 0 -240 0
: scroll-sprites ( dir -- )
	2 / ent-max 1 - for
		dup pan-dx + @ i +px
		dup pan-dy + @ i +py
	next drop
;

:var pan-dir
: scroll-room ( this-room* dir -- new-room* )
	hide-actioni
	rdrop
	dup pan-dir ! room-adj

	pan-dir @ scroll-sprites
	{ player @ != } { ' dummy swap kind ! } whoever

	GP @ >r GS @ >r
	dup .room-init @ exec
	pan-dir @ 4 + 8 mod scroll-sprites
	GP @ GS @ r> GS ! r> GP !

	sort-sprites
	pan-dir @ pan-room
	{ kind @ ' dummy = } ' free whoever

	dup .room-main @ goto
;

######################################################
##
##  Common entities and entity utilities
##
######################################################

: spawn-npc ( tile-x tile-y frame 'logic 'trigger -- id )
	>r spawn
	r>   over trigger !
	true over solid   !
;

: face-player ( id -- )
	dup px player @ px >
	if face-left else face-right then
;

: c-npc? ( a b -- flag )
	over py over py  6 - >=        >r
	over py over py  6 + <  r> and >r
	over px over px 14 - >= r> and >r
	over px over px 14 + <  r> and >r
	2drop r>
;

: c-npcs? ( sprite-id -- flag )
	ent-max 1 - for
		dup i !=      # not itself
		i solid @ and # collidable
		i valid   and # valid
		if
			dup i c-npc?
			if rdrop drop true exit then
		then
	next
	drop false
;

:vector c-ground? ( id -- flag )
	>r
	i px  1 + i py 22 + c-tile?
	i px  8 + i py 22 + c-tile? or
	i px 14 + i py 22 + c-tile? or
	i px  1 + i py 31 + c-tile? or
	i px  8 + i py 31 + c-tile? or
	i px 14 + i py 31 + c-tile? or
	rdrop
;

: c-small-ground? ( id -- flag )
	>r
	i px     i py     c-tile?
	i px 7 + i py     c-tile? or
	i px     i py 7 + c-tile? or
	i px 7 + i py 7 + c-tile? or

	i px   0 < or
	i py   9 < or
	i px 312 > or
	i py 232 > or
	rdrop
;

: c-small ' c-small-ground? ' c-ground? revector ; ( -- )
: c-big                     ' c-ground? devector ; ( -- )

: blocked? ( id -- flag )
	>r
	i c-ground?
	i c-npcs? or
	dup if i thud on then
	rdrop
;

: c-walk-x ( x id -- )
	2dup +px dup blocked? if
		over -1 * over +px
	then
	2drop
;

: c-walk-y ( x id -- )
	2dup +py dup blocked? if
		over -1 * over +py
	then
	2drop
;

: discarding ( id -- )
	>r
	# fly into a trashcan and then despawn!
	# possibly some sort of victory particle effect?
	"discarding" typeln
	i free
	rdrop
;

: c-player-small ( id -- flag )
	>r
	i  px player @ px  8 - >
	i  px player @ px 16 + < and
	i  py player @ py 16 + > and
	r> py player @ py 32 + < and
;

:ref p-dir
:proto scrub
: litter ( id -- )
	>r c-small
	i timer @ if
		i timer @ 3 mod -if
			RN @ 4 mod 516 + i tile!
			8x8 i sprite@ !
		then

		i timer dec@
		i dir @
		RN @ 7 mod -if brownian + 8 mod then
		i timer @ 10 > if
			dup delta-x i c-walk-x
			dup delta-y i c-walk-y
		then
		i timer @ 5 > if
			dup delta-x i c-walk-x
			dup delta-y i c-walk-y
		then
		dup delta-x i c-walk-x
		    delta-y i c-walk-y

		# if we're stuck in the player,
		# occasionally impart a large
		# impulse to 'buck out' randomly:
		i timer @ 10 > i timer @ 2 mod and if
			i c-player-small if
				RN @ 8 mod
				dup delta-x 4 * i c-walk-x
				dup delta-y 4 * i c-walk-y
				i dir !
			then
		then
	else
		i c-player-small if
			p-dir @ i dir !
			player @ kind @ ' scrub = if 8 else 15 then
			i timer !
		then
	then
	rdrop c-big
;

: spawn-litter ( can-id -- i )
	>r 8 34 r>
	RN @ 4 mod 516 + ' litter spawn-rel
	8x8 over sprite@ !
;

: spawn-all-litter ( can-id -- )
	dup prev @ @
	dup 1 < if 2drop exit then
	c-small
	1 - for
		RN @ 8 mod
		over spawn-litter
		30 for
			dup thud off
			over delta-x 4 * over c-walk-x
			over delta-y 4 * over c-walk-y
			RN @ 5 mod -if
				swap brownian + 8 mod swap
			then
			dup thud @ if
				swap brownian + 8 mod swap
			then

			# here's an example of how debugging
			# *rules* when you invert rendering control:
			#10 for sync next
		next
		2drop
	next
	drop c-big
;

:proto garbage-can

: near-garbage? ( can-id id -- flag )
	dup valid             -if 2drop false exit then
	dup kind @ ' litter = -if 2drop false exit then
	c-sprites?
;

: garbage-can ( id -- )
	>r
	ent-max 1 - for
		j i near-garbage? if
			' discarding i kind !
			20 j timer !
			j prev @ dec@
		then
	next

	i timer @ if
		i timer dec@
		i timer @ if 58 else 57 then i tile!
	then
	rdrop
;

: spawn-garbage-can ( counter* tile-x tile-y -- )
	3 - 57 ' garbage-can spawn >r
	true i solid !
	i prev !
	i spawn-all-litter
	rdrop
;

: suds ( id -- )
	>r
	i timer @ if
		i timer @ 10 / 524 + i tile!
		i timer @ 5 mod -if
			-1       i +py
			brownian i +px
		then
		i timer dec@
	else
		i free
	then
	rdrop
;

: spawn-suds ( id -- )
	>r
	RN @ 8 mod 3 -   8 +
	RN @ 4 mod 1 -  23 +
	i 524 ' suds spawn-rel

	>r
	8x8 sprite-mirror-horiz or i sprite@ !
	RN @ 10 mod 5 + i timer !
	rdrop

	rdrop
;

# an outer routine driving
# these actions (think) leaves
# a value on the stack- we
# must account for that
# before calling push-room:
: use-door ( id -- )
	>r
	>r j dir @ push-room r>
	rdrop
;

# note that the logical y-position of
# doors is stored in their prev field-
# find-trigger is special cased for this:
: door-sensor? ( id -- flag )
	>r
	i px 16 - player @ px over 40 + within
	i prev @  player @ py over 32 + within and
	rdrop
;

: door ( id -- )
	>r
	# idle
	i timer @ -if
		i door-sensor?
		i trigger @ if
			# door is open, close if player not near
			-if -8 i timer ! then
		else
			# door is closed, open if player near
			 if  8 i timer ! then
		then
	then
	
	# opening
	i timer @ 0 > if
		-4 i +py
		i timer dec@
		i timer @ -if ' use-door i trigger ! then
	then

	# closing
	i timer @ 0 < if
		4 i +py
		i timer inc@
		i timer @ -if 0 i trigger ! then
	then
	rdrop
;

: spawn-door ( room* tx ty -- )
	56 ' door spawn >r
	i dir !
	i py i prev !
	i door-sensor? if
		i py 32 - i py!
		' use-door i trigger !
	then
	rdrop
;

: exit-door ( id -- )
	drop
	rdrop # exit this method
	rdrop # discard player index
	rdrop # exit player routine
	rdrop # exit think dispatch routines:
	rdrop # ...
	rdrop # ...
	rdrop # exit main-common
	rdrop # exit room main loop
	drop  # discard entity index
;

: spawn-exit-door ( size tx ty -- )
	1 - 0 { drop } spawn >r
	1 not and   i sprite@ !
	' exit-door i trigger !
	rdrop
;

######################################################
##
##  Player logic
##
######################################################

:var found-npc
:var found-trigger

: find-trigger ( -- )
	0 found-npc     !
	0 found-trigger !
	ent-max 1 - for
		i trigger @
		i valid and if
			i px player @ px - abs 24 <
			i kind @ ' door = if i prev @ else i py then
			player @ py - abs 16 < and
			if
				i found-npc !
				i trigger @ found-trigger !
			then
		then
	next
;

: dirty? ( tile -- flag )
	dup  16 mod 8 <
	over 16 /   3 < and
	swap 1 and      and
;

: >dirty ( tile -- tile )
	dup  16 mod 8 <
	over 16 /   3 < and if
		dup 8 < if
			drop RN @ 4 mod 2 *
		then
		1 or
	then
;

:var soil-x
:var soil-y
:var soil-w
:var soil-h
: soil ( tx ty w h -- )
	soil-h ! soil-w !
	soil-y ! soil-x !

	soil-h @ 1 - for
		soil-w @ 1 - for
			RN @ 4 mod -if
				i soil-x @ +
				j soil-y @ +
				tile-grid@ dup @ >dirty swap !
			then
		next 
	next
;

: try-clean ( px py -- )
	pixel-grid@
	dup @ dirty? if
		3 for player @ spawn-suds next
		dup @ 1 not and swap !
	else
		drop
	then
;

: clean-floor ( -- )
	player @ px
	player @ py
	over  1 + over 22 + try-clean
	over  8 + over 22 + try-clean
	over 14 + over 22 + try-clean
	over  1 + over 31 + try-clean
	over  8 + over 31 + try-clean
	swap 14 + swap 31 + try-clean
;

:var activated
:var p-acc
:var p-vel
:var p-dir
:var p-bounce

:data keys-dir
	8 0 2 1 4 8 3 2
	6 7 8 0 5 6 4 8

: transform ( id -- )
	>r
	i timer @ if
		i timer @ 5 = if 5 i tile! then
		i timer dec@
	else
		i prev @ i kind !
	then
	rdrop
;

:proto scrub
:proto scrubby

: do-transform ( id -- )
	>r
	0 p-vel    !
	0 p-bounce !
	10 i timer !
	i kind @ ' scrub =
	if ' scrubby else ' scrub then i prev !
	' transform i kind !
	rdrop
;

: scrub ( id -- )
	>r
	p-bounce inc@
	p-bounce @ 10 / 2 mod -if 7 else 6 then
	i tile!

	keys 15 and if
		keys 15 and keys-dir + @
		dup 7 > if drop p-dir @ then p-dir !
		keys key-rt and if i face-right then
		keys key-lf and if i face-left  then
		5 p-acc !
		1 p-vel !
	else
		p-acc @ if
			p-acc dec@
		else
			0 p-vel !
		then
	then

	p-dir @ delta-x p-vel @ * i c-walk-x
	p-dir @ delta-y p-vel @ * i c-walk-y
	clean-floor
	keys key-a and -if i do-transform then

	rdrop
;

: scrubby ( id -- )
	>r
	0 i tile!

	keys 15 and if
		keys 15 and keys-dir + @
		dup 7 > if drop p-dir @ then p-dir !

		p-bounce dec@
		p-bounce @ -10 < if 10 p-bounce ! then
		p-bounce @ 0 > if 1 else 0 then i tile!
		keys key-rt and if i face-right then
		keys key-lf and if i face-left  then

		5 p-acc !
		2 p-vel !
	else
		0 p-bounce !
		p-acc @ if
			p-acc dec@
			1 p-vel !
		else
			0 p-vel !
		then
	then

	i thud off
	p-dir @ delta-x p-vel @ * i c-walk-x
	p-dir @ delta-y p-vel @ * i c-walk-y
	i thud @ if 0 p-acc ! 0 p-bounce ! then

	find-trigger
	found-trigger @ if
		show-actioni
	else
		hide-actioni
	then

	activated @ if
		keys key-a and -if
			activated off
		then
	else	
		keys key-a and if
			found-trigger @ if
				found-npc @
				found-trigger @ exec
				activated on
			else
				i do-transform
			then
		then
	then
	rdrop
;

: player? ( kind -- flag )
	dup  ' scrubby   =
	over ' transform = or
	swap ' scrub     = or
;

: spawn-scrubby ( tile-x tile-y -- )
	# only one player can exist at any time:
	player @ -1 > if 2drop exit then
	0 ' scrubby spawn player !
;

######################################################
##
##  Main game
##
######################################################

:image   grid-tiles "LabTiles.png"      8  8
:image         font "Text.png"          8  8
:image sprite-tiles "Scrubby.png"      16 32
:image smallsprites "SmallSprites.png"  8  8

: main-common ( -- )
	sort-sprites -1 player !
	{ kind @ player? } { player ! } whoever

	player @ py   0 < if -32 player @ +py 0 scroll-room then
	player @ px 304 > if  16 player @ +px 2 scroll-room then
	player @ py 208 > if  32 player @ +py 4 scroll-room then
	player @ px   0 < if -16 player @ +px 6 scroll-room then

	think tick
;

:include "TestLevels.fs"

: main
	16x32 spawn-size !
	224 ascii !
	40 blank !
	40 grid-z or solid-tile !
	261 grid-z or action-tile !
	' compare-sprites ' -sprites revector
	init-testlevels

	false >r
	false center-hallway goto-room
;