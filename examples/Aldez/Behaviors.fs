######################################################
##
##  Behaviors:
##
##  Entity scripts, including movement logic for
##  the player, enemies and interactive objects.
##
##  John Earnest
##
######################################################

: illuminate ( id -- )
	>r i dark?
	if   i tile 1 not and
	else i tile 1 or then
	r> tile!
;

: retile1 ( t x y )
	pixel-grid@ >r i @ 1 and or r> !
;

: retile ( tl tr bl br id -- )
	>r
	i px 8 +  i py 8 + retile1
	i px      i py 8 + retile1
	i px 8 +  i py     retile1
	i px     r> py     retile1
;

:data dir-x    0  1  0 -1
:data dir-y   -1  0  1  0

: daemon ( id -- )
	trigger @ exec
;

: spawn-daemon ( 'proc -- )
	0 0 0 ' daemon spawn
	dup hide trigger !
;

: fall ( id -- )
	>r
	i  timer @    -if r> free exit then
	i  timer @ 5 > if 16 else 43 then i tile!
	r> timer dec@
;

: make-fall ( id -- )
	' fall over kind !
	10 swap timer !
;

: die ( id -- )
	>r
	i timer @ -if r> free exit then
	21 i tile!
	i timer @ 2 > if 20 i tile! then
	i timer @ 4 > if 19 i tile! then
	r> timer dec@
;

: make-die ( id -- )
	dup show
	' die over kind !
	6 swap timer !
;

: poof ( x y -- )
	>r >r 0 r> r> ' die spawn
	6 swap timer !
;

######################################################
##
##  Spear
##
######################################################

:proto player-touching?

:proto scorpion
:proto tatter

:var src-spear
: spear-hit ( id -- flag )
	src-spear !
	{
		>r
		i src-spear @ c-sprites?
		-if rdrop false exit then
		i solid @
		i  kind @ ' scorpion = or
		i  kind @ ' tatter   = or
		rdrop
	}
	find-entity
	if hit true swap ! true else false then
;

: spear-return ( id -- )
	>r
	i dir @ dir-x + @ -16 * i px + i px!
	i dir @ dir-y + @ -16 * i py + i py!
	i spear-hit drop
	i player-touching? if
		i free
		false throwing !
	then
	rdrop
;

: [spear-fly]
	>r
	i dir @ dir-x + @ 8 * i px + i px!
	i dir @ dir-y + @ 8 * i py + i py!
	i px 0 < i px 320 > or
	i py 0 < i py 240 > or or if
		' spear-return i kind !
	then
	i { 16 mod 7 > } any-corners
	i spear-hit or if
		' spear-return i kind !
	then
	rdrop
;

: spear-fly ( id -- )
	dup [spear-fly]
	dup kind @ ' spear-fly =
	if dup [spear-fly] then
	drop
;

: spear-stab ( id -- )
	dup spear-hit drop free
;

:data spear-t     28    29     28    29
:data spear-s  16x16 69889 135425 16x16

: spawn-spear ( player-id -- )
	>r
	has-spear @ -if rdrop exit then
	i dir @ spear-t + @   # tile
	i dir @ dir-x + @ 8 * # delta-x
	i dir @ dir-y + @ 8 * # delta-y
	i                     # src-id
	has-mitten @ if
		' spear-fly
		true throwing !
	else
		' spear-stab
		false throwing !
	then
	spawn-rel
	i dir @ over dir !
	i dir @ spear-s + @ over sprite@ !
	drop rdrop
;

######################################################
##
##  Player
##
######################################################

: player-die ( id -- )
	>r
	i timer @ if
		i timer dec@
	else
		fade-out
		reset-light
		' dummy i kind !
		respawn-player
		fade-in
	then
	rdrop
;

: player-fall ( id -- )
	>r
	has-helm @ if i unlight then
	i timer @ if
		i timer @ 5 > if 17 else 18 then i tile!
		i timer dec@
	else
		' player i kind !
		from-x @ i px!
		from-y @ i py!
		0 i tile!
		1 take-health
	then
	has-helm @ if i light then
	rdrop
;

:proto push-block
:proto move-block
:proto can-push?

:var pusher
: try-push? ( player-id -- succeeded? )
	pusher !
	{
		>r
		i kind @ ' push-block = -if rdrop false exit then
		pusher @ px pusher @ dir @ dir-x + @ 16 * + i px =
		pusher @ py pusher @ dir @ dir-y + @ 16 * + i py = and
		rdrop
	} find-entity -if false exit then

	>r
	i pusher @ can-push? -if rdrop false exit then
	5 i timer !
	' move-block i kind !
	i move-block
	rdrop true
;

:data player-face       4  2  0  2
:data player-shield    10  9  8  9
:data player-throw     13 12 11 12
:data player-stat   16x16 69889 16x16 16x16

: player ( id -- )
	>r
	has-helm @ if i unlight then

	throwing @ if
		i dir @ player-throw + @ i tile!
	else
		keys key-up and if
			0 i dir !
			i try-push? if i py 8 - i py! else -8 i move-y then
			i py 0 < if
				-32 i py!
				0 -1 scroll-room
				i py from-y !
				i px from-x !
			then
		then
		keys key-dn and if
			2 i dir !
			i try-push? if i py 8 + i py! else  8 i move-y then
			i py 224 > if
				256 i py!
				0 1 scroll-room
				i py from-y !
				i px from-x !
			then
		then
		keys key-lf and if
			3 i dir !
			i try-push? if i px 8 - i px! else -8 i move-x then
			i px 0 < if
				-32 i px!
				-1 0 scroll-room
				i py from-y !
				i px from-x !
			then
		then
		keys key-rt and if
			1 i dir !
			i try-push? if i px 8 + i px! else  8 i move-x then
			i px 304 > if
				336 i px!
				1 0 scroll-room
				i py from-y !
				i px from-x !
			then
		then
		i dir @ player-face + @ i tile!
	then
	i dir @ player-stat + @ i sprite@ !

	keys key-a and throwing @ not and if i spawn-spear then
	keys key-b and if show-inventory then

	i over-pit? has-boots @ not and if
		10 i timer !
		0 blink-timer !
		' player-fall i kind !
	then

	blink-timer @ if
		blink-timer @ 2 and
		if i show else i hide then
		blink-timer dec@
	else
		i show
	then

	health @ 1 < if
		20 i timer !
		' always { ' dummy swap kind ! } whoever
		' player-die i kind !
		7 i tile!
		i show
	then

	has-helm @ if i light then
	rdrop
;

:var src-ent
: player-touching? ( id -- flag )
	src-ent !
	{
		>r i kind @ ' player =
		if r> src-ent @ c-sprites?
		else rdrop false then
	} count 0 >
;

: spawn-player ( x y -- )
	>r >r 0 r> r> ' player spawn >r
	i px from-x !
	i py from-y !
	2 i dir !
	rdrop
;

######################################################
##
##  Pickups
##
######################################################

:data bounce 1 -1 -1 1 1 1

: health ( id -- )
	>r
	i timer @ -if r> free exit then
	i timer @ 64 > if
		i timer @ 65 - bounce + @
		i py + i py!
	then
	i timer @ 40 < if
		i timer @ 2 / 4 mod 2 mod
		if i show else i hide then
	then
	i timer dec@

	i player-touching? if
		1 give-health
		r> free exit
	then
	rdrop
;

: spawn-health ( id -- )
	>r 46 0 -2 r> ' health spawn-rel
	70 swap timer !
;

######################################################
##
##  Obstacles
##
######################################################

: block ( id -- )
	>r
	i illuminate
	rdrop
;

: skull ( id -- )
	>r
	i illuminate
	i hit @ if
		i make-die
		6 random -if i spawn-health then
	then
	rdrop
;

: spawn-block ( x y -- )
	>r >r 62 r> r> ' block spawn
	true swap solid !
;

: spawn-skull ( x y -- )
	>r >r 60 r> r> ' skull spawn
	true swap solid !
;

######################################################
##
##  Pushable Blocks
##
######################################################

: push-block ( id -- )
	>r
	i illuminate
	i over-pit? if i make-fall then
	rdrop
;

: can-push? ( pushed src -- flag )
	dir @ swap >r i dir !
	i dir @ dir-x + @  16 * i px + i px!
	i dir @ dir-y + @  16 * i py + i py!
	i colliding? not
	i dir @ dir-x + @ -16 * i px + i px!
	i dir @ dir-y + @ -16 * i py + i py!
	rdrop
;

: move-block ( id -- )
	>r
	has-helm @ if 63 else 62 then i tile!
	i timer @ -if ' push-block r> kind ! exit then
	i timer @ 5 mod -if
		i dir @ dir-x + @ 8 * i px + i px!
		i dir @ dir-y + @ 8 * i py + i py!
	then
	i timer dec@
	rdrop
;

: spawn-push-block ( x y -- )
	>r >r 62 r> r> ' push-block spawn
	true swap solid !
;

######################################################
##
##  Doors
##
######################################################

:data door-t  56 58 56 58
:data door-s  16x16 69889 135425 16x16

:proto opened-door
: open-door ( id -- )
	29 over timer !
	' opened-door over kind !
	false swap solid !
;

:proto closed-door
: close-door ( id -- )
	29 over timer !
	' closed-door over kind !
	true over solid !
	show
;

: closed-door ( id -- )
	>r
	i timer @ if
		i timer @ 10 mod -if
			i dir @ dir-x + @ -8 * i px + i px!
			i dir @ dir-y + @ -8 * i py + i py!
		then
		i timer dec@
	else
		i trigger @ @ if i open-door then
	then
	i illuminate
	rdrop
;

: opened-door ( id -- )
	>r
	i timer @ if
		i timer @ 10 mod -if
			i dir @ dir-x + @ 8 * i px + i px!
			i dir @ dir-y + @ 8 * i py + i py!
		then
		i timer dec@
		i timer @ -if i hide then
	else
		i trigger @ @ -if i close-door then
	then
	i illuminate
	rdrop
;

: spawn-door ( 'flag dir x y -- )
	>r >r 54 r> r> ' closed-door spawn >r
	true i solid !
	dup i dir !
	dup door-t + @ i tile!
	    door-s + @ i sprite@ !
	dup i trigger !
	@ if
		# if the flag is true, spawn
		# the door in the 'open' position.
		i dir @ dir-x + @ 16 * i px + i px!
		i dir @ dir-y + @ 16 * i py + i py!
		' opened-door i kind !
		i hide
		false i solid !
	then
	rdrop
;

######################################################
##
##  Enemies
##
######################################################

:data scorp-crawl 26 24 24 24
:data scorp-s     16x16 69889 16x16 16x16

: scorpion ( id -- )
	>r
	i hit @ if
		i make-die
		6 random -if i spawn-health then
	then
	10 random -if 4 random i dir ! then
	10 random -if
		i dir @ dir-x + @ 8 * i move-x
		i dir @ dir-y + @ 8 * i move-y
	then
	i dir @ scorp-crawl + @ i tile!
	i dir @ scorp-s     + @ i sprite@ !
	i player-touching? if
		i tile 1 + i tile!
		1 take-health
	then
	i over-pit? if i make-fall then
	rdrop
;

: tatter   ;
: ghost    ;
: shade    ;

: spawn-scorpion ( id -- )
	>r >r 24 r> r> ' scorpion spawn >r
	#true i solid !
	rdrop
;

######################################################
##
##  Switches
##
##  tripwire- procedure fires once on player entrance
##  and then this entity is destroyed.
##  plate- procedure fires when something steps on
##  the plate, and again when something steps off.
##  spear switch- fires when the switch is attacked.
##  light switch- fires when the lighting of the
##  switch changes.
##
######################################################

: tripwire ( id -- )
	>r
	i player-touching? if
		i trigger @ exec
		r> free exit
	then
	rdrop
;

: heavy-object? ( kind -- flag )
	dup  ' player      =
	over ' push-block  = or
	over ' scorpion    = or
	swap ' tatter      = or
;

:var cov-src
: covered? ( id -- flag )
	cov-src !
	{
		>r
		i cov-src @ !=
		i kind @ heavy-object? and
		r> cov-src @ c-sprites? and
	} count 0 >
;

: hide-plate >r  7  7  7  7 r> retile ;
: show-plate >r 32 34 36 38 r> retile ;

: plate ( id -- )
	>r
	i timer @ if
		i covered? -if
			i trigger @ exec
			false i timer !
			i show-plate
		then
	else
		i covered? if
			i dir @ exec
			true i timer !
			i hide-plate
		then
	then
	rdrop
;

: light-switch ( id -- )
	>r
	i timer @ if
		i dark? if
			i trigger @ exec
			false i timer !
			22 i tile!
		then
	else
		i dark? -if
			i dir @ exec
			true i timer !
			23 i tile!
		then
	then
	rdrop
;

: spear-switch ( id -- )
	>r
	i timer @ if
		31 i tile!
		i timer dec@
		false i hit !
	else
		30 i tile!
		i hit @ if
			i trigger @ exec
			40 i timer !
		then
	then
	rdrop
;

: spawn-tripwire ( 'proc x y -- )
	>r >r 0 r> r> ' tripwire spawn >r
	i hide
	i trigger !
	rdrop
;

: spawn-plate ( 'proc-on 'proc-off x y -- )
	>r >r 0 r> r> ' plate spawn >r
	i hide
	i trigger !
	i dir !
	i covered?
	if true i timer ! i hide-plate
	else i show-plate then
	rdrop
;

: spawn-light-switch ( 'proc-on 'proc-off x y -- )
	>r >r 22 r> r> ' light-switch spawn >r
	i trigger !
	i dir !
	i dark? -if true i timer ! 23 i tile! then
	rdrop
;

: spawn-spear-switch ( 'proc x y -- )
	>r >r 30 r> r> ' spear-switch spawn >r
	i trigger !
	true i solid !
	rdrop
;

######################################################
##
##  Chests
##
##  When a chest is activated, it executes its
##  trigger routine and provides its own id.
##
######################################################

: close-chest >r 216 218 52 54 r> retile ;
:  open-chest >r 220 222 52 54 r> retile ;
:  hide-chest >r   5   5  5  5 r> retile ;

: chest-open? ( id -- flag )
	dup px swap py pixel-grid@ @
	1 not and 220 =
;

: chest ( id -- )
	>r
	i player-touching?
	i timer @ -if
		if
			true i timer !
			i i trigger @ exec
		then
	else
		-if false i timer ! then
	then
	rdrop
;

: spawn-chest ( 'proc x y -- )
	>r >r 0 r> r> ' chest spawn >r
	i hide
	i open-chest
	i trigger !
	rdrop
;

: destroy-chest ( id -- )
	dup hide-chest
	make-die
;