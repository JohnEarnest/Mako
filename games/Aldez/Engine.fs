######################################################
##
##  Engine:
##
##  Core logic for the lighting system,
##  damage, ingame HUD and other subsystems.
##
##  John Earnest
##
######################################################

: dummy  drop ; ( id -- )

:const up 0
:const rt 1
:const dn 2
:const lf 3

######################################################
##
##  Lighting
##
##  The 'light-buff' array tracks the tiles that were
##  originally illuminated in the map data.
##  It is important to call 'init-light' whenever
##  GP is relocated.
##
######################################################

:array light-buff 1271 false

: init-light ( -- )
	29 for
		39 for
			i j tile-grid@ @ 1 and 0!
			i j 41 * + light-buff + !
		next
	next
;

: reset-light ( -- )
	29 for
		39 for
			i j tile-grid@ dup @ 1 not and
			i j 41 * + light-buff + @ 1 and or
			swap !
		next
	next
;

:const light-size 10
:data  light-mask
	 0  0  0 -1 -1 -1 -1  0  0  0
	 0 -1 -1 -1 -1 -1 -1 -1 -1  0
	 0 -1 -1 -1 -1 -1 -1 -1 -1  0
	-1 -1 -1 -1 -1 -1 -1 -1 -1 -1
	-1 -1 -1 -1 -1 -1 -1 -1 -1 -1
	-1 -1 -1 -1 -1 -1 -1 -1 -1 -1
	-1 -1 -1 -1 -1 -1 -1 -1 -1 -1
	 0 -1 -1 -1 -1 -1 -1 -1 -1  0
	 0 -1 -1 -1 -1 -1 -1 -1 -1  0
	 0  0  0 -1 -1 -1 -1  0  0  0

: light-tile ( pixels tile-delta -- pixels )
	light-size 2 / - 8 * +
;

:var light-sprite
:var light-func

: for-light ( sprite* func* -- )
	light-func !
	light-sprite !

	light-size 1 - for
		light-size 1 - for
			i light-size * j + light-mask + @
			0 light-sprite @ px 8 + i light-tile 319 within and
			0 light-sprite @ py 8 + j light-tile 239 within and
			if
				light-sprite @ px 8 + i light-tile
				light-sprite @ py 8 + j light-tile
				pixel-grid@
				
				# this is fairly clumsy. The gist of it is that
				# since the light buffer always has the same stride
				# but the stride of the grid can vary (for large maps),
				# the light-buff index is not a simple offset
				# from the grid index:

				light-sprite @ px 8 + 8 / light-size 2 / - i +
				light-sprite @ py 8 + 8 / light-size 2 / - j +
				41 * + light-buff +
				light-func @ exec
			then
		next
	next
;

: light ( sprite* -- )
	{ drop dup @ 1 or swap ! } for-light
;

: unlight ( sprite* -- )
	{ @ 1 and over @ 1 not and or swap ! } for-light
;

######################################################
##
##  Health Meter / Gold Meter
##
######################################################

:const health-cap 10
:array health-buff health-cap 0
:var   health-timer

: give-health ( n -- )
	health @ + max-health @ min health !
	70 health-timer !
;

: take-health ( n -- )
	blink-timer @ if drop exit then
	health @ swap - 0 max health !
	70 health-timer !
	20 blink-timer !
;

: show-health ( -- )
	1 1 tile-grid@ health-buff health-cap >move
	max-health @ 1 - for
		i health @ >= if 164 else 165 then
		1 i + 1 tile-grid@ !
	next
;

: hide-health ( -- )
	health-buff 1 1 tile-grid@ health-cap >move
;

: take-gold ( n -- flag )
	drop true
;

: frame ( -- )
	(
	health-timer @ if
		health-timer @ 20 > if
			health-timer @ 2 / 4 mod 2 and
		else
			true
		then
		health-timer dec@
	else
		health @ 1 <=
	then
	)
	true

	dup if show-health then
	3 for sync next
	if hide-health then
;

######################################################
##
##  Entity utils
##
######################################################

: all-corners ( id 'pred -- flag )
	>r >r
	i px       i py      pixel-grid@ @  j exec
	i px 15 +  i py      pixel-grid@ @  j exec and
	i px       i py 15 + pixel-grid@ @  j exec and
	i px 15 + r> py 15 + pixel-grid@ @ r> exec and
;

: any-corners ( id 'pred -- flag )
	>r >r
	i px       i py      pixel-grid@ @  j exec
	i px 15 +  i py      pixel-grid@ @  j exec or
	i px       i py 15 + pixel-grid@ @  j exec or
	i px 15 + r> py 15 + pixel-grid@ @ r> exec or
;

: dark? ( id -- flag )
	{ 1 and 0= } all-corners
;

: over-pit? ( id -- flag )
	{ 16 swap 21 within } all-corners
;

:var src-ent
: ent-colliding? ( id -- flag )
	src-ent !
	{
		>r i src-ent @ !=
		   i solid   @ and
		if r> src-ent @ c-sprites?
		else rdrop false then
	} count 0 >
;

: colliding? ( id -- flag )
	dup { 16 mod 7 > } any-corners
	if drop true exit then
	ent-colliding?
;

: move-x ( delta id -- )
	>r >r j px i + j px!
	j colliding? if j px i - j px! then
	rdrop rdrop
;

: move-y ( delta id -- )
	>r >r j py i + j py!
	j colliding? if j py i - j py! then
	rdrop rdrop
;

: not-player? ( id -- flag )
	kind @ ' player !=
;

:const item-sprite 250
: give-item ( id -- )
	# I'm just going to assume the search succeeds,
	# because if we can't find a player things
	# are already pretty screwed up.
	{ kind @ ' player = } find-entity drop >r
	
	 6 i tile!
	dn i dir !
	16x16 swap 48 + i px i py 16 - item-sprite >sprite
	7 for
		item-sprite py 1 - item-sprite py!
		3 for sync next
	next
	rdrop
;

: finish-give
	item-sprite hide
;

######################################################
##
##  Scrolling and teleporting
##
######################################################

: init-room ( x y -- )
	2dup
	5 * + rooms + @ exec
	5 * + room-visited + true swap !
;

: set-room ( x y -- )
	room-y !
	room-x !
	reset-light
	  40 room-x @ *
	6030 room-y @ * +
	dungeon + GP !
	160 GS !
	init-light
	room-x @ room-y @ init-room
;

:const scroll-steps 10
:var sx
:var sy

: scroll-room ( dx dy i -- )
	sy ! sx !
	reset-light
	' not-player? { ' dummy swap kind ! } whoever
	room-x @ sx @ +
	room-y @ sy @ + init-room
	
	# animate moving GP and moving all the sprites.
	scroll-steps 1 - for
		  40 scroll-steps / sx @ *
		6030 scroll-steps / sy @ * +
		GP @ + GP !
		' always {
			>r
			sx @ -320 scroll-steps / * i px + i px!
			sy @ -240 scroll-steps / * i py + i py!
			rdrop
		} whoever

		#init-light
		' not-player? ' apply-kind whoever
		sync sync
		#reset-light
	next

	init-light
	{ kind @ ' dummy = } { free } whoever
	room-x @ sx @ + room-x !
	room-y @ sy @ + room-y !
;