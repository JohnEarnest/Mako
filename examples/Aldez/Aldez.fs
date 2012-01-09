######################################################
##
##  Aldez:
##
##  The Legendary Armor of Aldez.
##  a game of puzzle-solving, exploration and adventure!
##
##  John Earnest
##
######################################################

:include "../Print.fs"
:include "../Grid.fs"
:include "../Sprites.fs"
:include "../String.fs"
:include "../Util.fs"
:include "../Math.fs"

:const char-offset 224
:const line-offset 25

:image   tile-bank1 "dungeonTiles.png"     8  8
:image   font-tiles "ambertext.png"        8  8
:image sprite-bank1 "dungeonSprites.png"  16 16
:image    syd-heads "sydheads.png"        64 64
:image    syd-more  "syddungeonheads.png" 64 64
:image   hank-heads "hankheads.png"       64 64
:image  lopez-heads "lopezheads.png"      64 64

######################################################
##
##  Global game state
##
######################################################

:array bogus-sprite 4 0
:array sprites 1024 0

:var room-x
:var room-y

:data room-visited
	0 0 0 0 0
	0 0 0 0 0
	0 0 0 0 0
	0 0 0 0 0
	0 0 0 0 0

:data room-secret
	0 0 0 0 0
	0 0 0 0 0
	0 0 0 0 0
	0 0 0 0 0
	0 0 0 0 0

:data item-flags
:data has-helm    true
:data has-shield  true
:data has-spear   true
:data has-mitten  true
:data has-boots   false
:data has-map     true
:data has-compass true
:data has-???     false

:data max-health  6
:data health      5

:var from-x
:var from-y
:var blink-timer
:var throwing

######################################################
##
##  External components
##
######################################################

:proto player
:proto dungeon
:proto rooms
:proto respawn-player

:include "Dialog.fs"
:include "Inventory.fs"
:include "Cutscenes.fs"
:include "Entities.fs"
:include "Engine.fs"
:include "Behaviors.fs"
:include "Level.fs"

######################################################
##
##  Main Loop
##
######################################################

: respawn-player
	{ kind @ ' dummy = } ' free whoever
	0 blink-timer !
	max-health @ give-health

	2 0 set-room
	112 16 spawn-player
	think
;

: main
	  tile-bank1 GT !
	sprite-bank1 ST !
	char-offset ascii !

	2 0 set-room
	112 16 spawn-player

	think think

	syd-normal
	dialog[
		"Well, I guess I'd better"
		" get started..."
	]-text

	loop
		think
		frame
	again
;