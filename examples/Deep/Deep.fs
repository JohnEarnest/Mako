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
:include "../String.fs"
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


:const player       0
:const bomb         8
:const explosion   12
:const bubbles     16
:const monsters    20
:const player-gibs 28

:const blastradius 32
:const killradius  16

:var dropped
:var gameover
:var direction
:var dirtimer

######################################################
##
## A rolling waves animation
## 
## Much of the rest of the game sort of crusted
## around the wave system as a global timer. hm.
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

	0  5 tile-grid@ 40 wave @ wave-tiles  + @ fill
	0 15 tile-grid@ 40 wave @ wave-tiles2 + @ fill
	0 22 tile-grid@ 40 wave @ wave-tiles3 + @ fill
	wave @ wave-height + @ player py!

	direction @ dirtimer +@
	dirtimer @ dup 1 < swap 6 > or if
		direction neg@
	then
;

: wave-time
	wave-timer @ swap mod if r> 2drop then
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
## Game over animations
##
######################################################

: sink-player
	dup py 240 > if
		free
		16 1 "THE  END" grid-type exit
	then
	8 wave-time
	dup sprite@ .sprite-y inc@
	32 wave-time
	sprite@ .sprite-x brownian swap +@
;

: sink-hat
	16 wave-time
	dup sprite@ .sprite-y inc@
	32 wave-time
	sprite@ .sprite-x brownian swap +@
;

: can-capsize
	dup  px player px - dup *
	swap py player py - dup *
	+ killradius killradius * <
;

: disperse
	32 wave-time
	dup px player px >
	if   dup sprite@ .sprite-x inc@
	else dup sprite@ .sprite-x dec@ then
	dup sprite@ .sprite-y inc@
	sprite@ .sprite-t dup @ 1 xor swap !
;

:proto seek-player
:proto swim
: is-monster
	dup  types + @ ' seek-player =
	swap types + @ ' swim        = or
;

: disperse-monster
	' disperse type!
;

: capsize
	gameover @ if exit then
	true gameover !
	' is-monster ' disperse-monster whoever
	1 1 tile-grid@ 38 0 fill
	2 player tile!
	20 for waves think sync next
	1 player tile!
	20 for waves think sync next
	2 player tile!
	20 for waves think sync next
	3 player tile!
	player-gibs     12 5 player spawn ' sink-player type!
	player-gibs 1 + 12 5 player spawn ' sink-hat    type!
;

######################################################
##
## Entity scripts
##
######################################################

: bubble
	dup py 48 < if free exit then
	4 wave-time
	dup sprite@ .sprite-y dec@
	16 wave-time
	dup sprite@ .sprite-x brownian swap +@
	32 wave-time
	sprite@ .sprite-t 4 random bubbles + swap !
;

: menace
	32 wave-time
	sprite@ .sprite-t dup @ 1 xor swap !
;

: seek-player
	dup can-capsize if capsize then
	8 wave-time
	dup py 40 > if dup sprite@ .sprite-y dec@ then
	dup px player px < if 2 else -2 then
	over sprite@ .sprite-x +@
	menace
;

: swim
	dup py 64 < if ' seek-player type! exit then
	4 wave-time
	direction @ over sprite@ .sprite-x +@
	16 wave-time
	dup sprite@ .sprite-y dec@
	menace
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

######################################################
##
## Core game logic
##
######################################################

: move-player
	gameover @ if exit then
	keys dup
	key-lf and if player px 2 - player px! then
	key-rt and if player px 2 + player px! then
	player px 0 max 288 min player px!
;

: drop-charge
	bomb 0 12 player spawn
	dup ' sink type!
	dup 0 timer!
	dropped !
;

: in-radius
	dup  px dropped @ px - dup *
	swap py dropped @ py - dup *
	+ blastradius blastradius * <
;

: kill-monster
	' bubble type!
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
	' in-radius ' kill-monster whoever
	0 dropped !
;

:var pressed
: fire
	gameover @ if exit then
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

: spawn-wave
	7 for
		10 for
			16x16
			monsters j 6 mod +
			i 22 *
			j 18 * 240 +
			alloc dup >r >sprite
			r> ' swim type!
		next
	next
;

: main
	32x16 0 144 32 player >sprite
	spawn-wave
	0 dirtimer  !
	1 direction !

	loop
		move-player
		fire
		waves
		think
		sync
	again
;