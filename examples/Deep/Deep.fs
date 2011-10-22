######################################################
##
##  Deep:
##
##  Yet another vaguely original game. Attempts to
##  fuse together mechanics from Space Invaders and
##  Missile Command in a nautically-themed
##  depth-charge-'em-up.
##
##  John Earnest
##
######################################################

:include "../Grid.fs"
:include "../Sprites.fs"
:include "../Print.fs"
:include "../Util.fs"
:include "../Math.fs"
:include "Entities.fs"

:image  grid-tiles  "deeptiles.png"  8  8
:image sprite-tiles "boat.png"      32 16
:image others       "sprites.png"   16 16

:const clear-color 0xFF90C6C8

:array sprites 1024 0
:array bogus      4 0 # this is where 'bogus' entities will map

:data grid

	 0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0 -1 
	 0 36 37 37 48  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0 -1 
	 0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0 -1 
	 0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0 -1 
	 0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0 -1 
	10 10 10 10 10 10 10 10 10 10 10 10 10 10 10 10 10 10 10 10 10 10 10 10 10 10 10 10 10 10 10 10 10 10 10 10 10 10 10 10 -1 
	 1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1 -1 
	 1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1 -1 
	 1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1 -1 
	 1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1 -1 
	 1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1 -1 
	 1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1 -1 
	 1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1 -1 
	 1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1 -1 
	 1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1  1 -1 
	 9  9  9  9  9  9  9  9  9  9  9  9  9  9  9  9  9  9  9  9  9  9  9  9  9  9  9  9  9  9  9  9  9  9  9  9  9  9  9  9 -1 
	 2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2 -1 
	 2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2 -1 
	 2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2 -1 
	 2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2 -1 
	 2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2 -1 
	 2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2 -1 
	 8  8  8  8  8  8  8  8  8  8  8  8  8  8  8  8  8  8  8  8  8  8  8  8  8  8  8  8  8  8  8  8  8  8  8  8  8  8  8  8 -1 
	 3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3 -1 
	 3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3 -1 
	 3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3 -1 
	 7  7  7  7  7  7  7  7  7  7  7  7  7  7  7  7  7  7  7  7  7  7  7  7  7  7  7  7  7  7  7  7  7  7  7  7  7  7  7  7 -1 
	 4  4  4  4  4  4  4  4  4  4  4  4  4  4  4  4  4  4  4  4  4  4  4  4  4  4  4  4  4  4  4  4  4  4  4  4  4  4  4  4 -1 
	 6  6  6  6  6  6  6  6  6  6  6  6  6  6  6  6  6  6  6  6  6  6  6  6  6  6  6  6  6  6  6  6  6  6  6  6  6  6  6  6 -1 
	 5  5  5  5  5  5  5  5  5  5  5  5  5  5  5  5  5  5  5  5  5  5  5  5  5  5  5  5  5  5  5  5  5  5  5  5  5  5  5  5 -1 
	-1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 


:const player     0
:const bomb       8
:const explosion 12
:const bubbles   16

:var dropped

######################################################
##
## A rolling waves animation
##
######################################################

:data wave-tiles  1073741836 1073741837 1073741838 1073741839 1073741836 1073741837 1073741838 1073741839
:data wave-tiles2  9  9 11 11  9  9 11 11
:data wave-tiles3 10 10  8  8  8  8 10 10
:data wave-height 32 31 30 31 32 31 30 31
:var wave
:var wave-timer

: waves
	wave-timer @ if wave-timer dec@ exit then
	50 wave-timer ! wave @ 1 + 8 mod wave !
	39 for
		wave @ wave-tiles + @
		i 5 tile-grid@ !
		wave @ wave-tiles2 + @
		i 15 tile-grid@ !
		wave @ wave-tiles3 + @
		i 22 tile-grid@ !
	next
	wave @ wave-height + @ player py!
;

######################################################
##
## Potentially reusable bits
##
######################################################

: spawn (frame rel-x rel-y src-id -- id)
	dup py swap px
	>r + swap r> +
	alloc >r
	i sprite@ .sprite-x !
	i sprite@ .sprite-y !
	i sprite@ .sprite-t !
	16x16 i sprite@ !
	r>
;

######################################################
##
## Core game logic
##
######################################################

: move-player
	keys dup
	key-lf and if player px 2 - player px! then
	key-rt and if player px 2 + player px! then
	player px 0 max 288 min player px!
;

: bubble
	dup py 48 < if free exit then
	wave-timer @ 4 mod if drop exit then
	dup sprite@ .sprite-y dec@
	wave-timer @ 16 mod if drop exit then
	dup sprite@ .sprite-x brownian swap +@
	wave-timer @ 32 mod if drop exit then
	sprite@ .sprite-t 4 random bubbles + swap !
;

: sink
	dup py 240 > if free 0 dropped ! exit then
	dup sprite@ .sprite-y inc@
	dup timers + @ 1 + 80 mod
	2dup 20 / bomb + swap tile!
	over timers + !
	drop
;

: explode
	dup timers + @ if
		dup timers + dec@
		dup timers + @ 10 / explosion +
		swap tile!
		exit
	then
	free
;

: drop-charge
	bomb 0 12 player spawn
	dup ' sink type!
	dup 0 timer!
	dropped !
;

: detonate
	dropped @ ' explode type!
	dropped @ 40 timer!
	explosion dropped @ tile!
	2 for
		bubbles 4 random +
		48 random 24 -
		48 random 24 -
		dropped @ spawn ' bubble type!
	next
	0 dropped !
;

:var pressed
: fire
	keys key-a and if
		pressed @ -if
			dropped @
			if   detonate
			else drop-charge 1 player tile! then
			true pressed !
		then
	else
		0 player tile!
		false pressed !
	then
;

: main
	32x16 0 50 32 player >sprite

	loop
		move-player
		fire
		waves
		think
		sync
	again
;