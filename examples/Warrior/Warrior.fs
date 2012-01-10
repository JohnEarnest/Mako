######################################################
##
##  Forth Warrior:
##
##  An educational game about introductory Forth
##  programming, basic AI concepts and stabbing
##  things with swords.
##
##  John Earnest
##
######################################################

:const start-level 0

:include "../Print.fs"
:include "../Util.fs"
:include "../Sprites.fs"
:include "../Grid.fs"

:image   grid-tiles   "warriorTiles.png"  8  8
:image sprite-tiles "warriorSprites.png" 16 16

# directions
:const north 0
:const east  1
:const south 2
:const west  3

:data dir-x    0  1  0 -1
:data dir-y   -1  0  1  0

:data n "north"
:data e "east"
:data s "south"
:data w "west"
:data  dir-names n e s w
: .dir dir-names + @ type ;

# terrain types
:const empty  0
:const solid  1
:const stairs 2

######################################################
##
##  Level Data
##
######################################################

:data grid
:data l1
	-1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 
	-1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 
	-1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 
	-1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 
	-1 -1 -1 -1  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 
	-1 -1 -1 -1  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 
	-1 -1 -1 -1  2  2  6  7  6  7  6  7  6  7  6  7  2  2  2  2  6  7  6  7  6  7  6  7  2  2 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 
	-1 -1 -1 -1  2  2 22 23 22 23 22 23 22 23 22 23  2  2  2  2 22 23 22 23 22 23 22 23  2  2 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 
	-1 -1 -1 -1  2  2  8  9  0  1  0  1  0  1  0  1  2  2  2  2  0  1  0  1  0  1  0  1  2  2 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 
	-1 -1 -1 -1  2  2 24 25 16 17 16 17 16 17 16 17  2  2  2  2 16 17 16 17 16 17 16 17  2  2 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 
	-1 -1 -1 -1  2  2  2  2  2  2  2  2  2  2  0  1  2  2  2  2  0  1  2  2  2  2  0  1  2  2  2  2  2  2  2  2 -1 -1 -1 -1 -1 
	-1 -1 -1 -1  2  2  2  2  2  2  2  2  2  2 16 17  2  2  2  2 16 17  2  2  2  2 16 17  2  2  2  2  2  2  2  2 -1 -1 -1 -1 -1 
	-1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1  2  2  0  1  6  7  6  7  0  1  2  2  2  2  0  1  6  7  6  7  6  7  2  2 -1 -1 -1 -1 -1 
	-1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1  2  2 16 17 22 23 22 23 16 17  2  2  2  2 16 17 22 23 22 23 22 23  2  2 -1 -1 -1 -1 -1 
	-1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1  2  2  0  1  0  1  0  1  0  1  2  2  2  2  0  1  0  1  0  1  4  5  2  2 -1 -1 -1 -1 -1 
	-1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1  2  2 16 17 16 17 16 17 16 17  2  2  2  2 16 17 16 17 16 17 20 21  2  2 -1 -1 -1 -1 -1 
	-1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2 -1 -1 -1 -1 -1 
	-1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2 -1 -1 -1 -1 -1 
	-1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 
	-1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 
	-1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 
	-1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 
	-1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 
	-1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 
	-1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 
	-1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 
	-1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 
	-1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 
	-1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 
	-1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 
	-1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 

:data levels l1

######################################################
##
##  Game Simulation
##
######################################################

:proto tick
:proto level

:var player
:var move-dir
:var move-action

: stop   loop keys if halt then sync again ;
: wait   29 for sync next ;

: in-dir ( dir -- tile )
	0 over 3 within -if
		"Invalid direction: " type . cr stop
	then
	dup  dir-x + @ 16 * player @ px +
	swap dir-y + @ 16 * player @ py +
	pixel-grid@ @
;

: no-action
	"No action specified." typeln stop
;

: make-walk
	move-dir @ in-dir
	dup 0 = if
		drop
		move-dir @ dir-x + @ 16 * player @ px + player @ px!
		move-dir @ dir-y + @ 16 * player @ py + player @ py!
		"Walked " type move-dir @ .dir "." typeln
		exit
	then
	dup 4 = if
		drop
		"Completed the dungeon." typeln
		stop
	then
	"Cannot walk " type move-dir @ .dir "." typeln stop
;

: make-take
	"Taking objects is not implemented yet." typeln stop
;

: make-attack
	"Attacking enemies is not implemented yet." typeln stop
;

: init-level ( level -- )
	dup level
	levels + @ GP !
	16x16 0 0 0 200 >sprite 200 player !
	20 for
		15 for
			j 2 * i 2 * tile-grid@ @
			dup 8 = if
				j 16 * player @ px!
				i 16 * player @ py!
			then
			# spawn any other entities here.
			drop
		next
	next
;

: main
	start-level init-level
	loop
		wait
		' no-action move-action !
		tick move-action @ exec
	again
;

######################################################
##
##  Controller API
##
######################################################

: walk   move-dir ! ' make-walk   move-action ! ; ( dir -- )
: take   move-dir ! ' make-take   move-action ! ; ( dir -- )
: attack move-dir ! ' make-attack move-action ! ; ( dir -- )

: look ( dir -- type )
	>r 16x16 4
	i dir-x + @ 16 * player @ px +
	i dir-y + @ 16 * player @ py +
	201 >sprite wait 0 201 sprite@ ! r>
	in-dir
	dup 0 = if drop empty  exit then
	dup 4 = if drop stairs exit then
	drop solid
;

# fill these in:
: level ( n -- ) drop ;

:var going-down
:var going-up

: tick
	going-up @ if
		north look empty = if north walk exit then
		false going-up !
	then

	going-down @ if
		south look empty = if south walk exit then
		false going-down !
	then

	east  look solid != if east  walk exit then
	north look empty  = if north walk true going-up !   exit then
	south look empty  = if south walk true going-down ! exit then
;







