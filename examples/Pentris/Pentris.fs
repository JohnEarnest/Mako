######################################################
##
##  Pentris:
##
##  A variation on 'Tetris' using pentominoes
##  intead of tetrominoes. The fact that this
##  simplifies several aspects of the game logic
##  is purely incidental. Honest.
##
##  John Earnest
##
######################################################

:include "../Grid.fs"
#:include "../Math.fs"
:include "../String.fs"
:include "../Print.fs"
:image grid-tiles "pentris.png" 8 8
:data grid

	 0  0 48 37 46 52 50 41 51  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0 -1 
	 2 10 11 11 11 11 11 11 11 11 11 11 11 11 11 11 11 11 11 11 11 11 11 11 11 11 11 11 12  2  2  2  2  2  2  2  2  2  2  2 -1 
	 2 29  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3 13 10 11 11 11 11 11 11 11 11 11 12 -1 
	 1 30  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3 14 29  1  1 51 35 47 50 37  1  1 14 -1 
	 1 30  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3 14 29 32 32 32 32 32 32 32 32 32 14 -1 
	 1 30  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3 14 29  0  0  0  0  0  0  0  3 32 14 -1 
	 2 29  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3 13 26 27 27 27 27 27 27 27 27 27 28 -1 
	 2 29  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3 13  2  2  2  2  2  2  2  2  2  2  2 -1 
	 2 29  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3 13  2 10 11 11 11 11 11 11 11 12  2 -1 
	 1 30  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3 14  1 29  1 44 37 54 37 44  1 14  1 -1 
	 1 30  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3 14  1 29 32 32 32 32 32 32 32 14  1 -1 
	 1 30  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3 14  1 29 32 32 32 32 32  3 32 14  1 -1 
	 2 29  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3 13  2 26 27 27 27 27 27 27 27 28  2 -1 
	 2 29  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3 13  2 10 11 11 11 11 11 11 11 12  2 -1 
	 1 30  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3 14  1 29  1 44 41 46 37 51  1 14  1 -1 
	 1 30  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3 14  1 29 32 32 32 32 32 32 32 14  1 -1 
	 1 30  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3 14  1 29 32 32 32 32  0  3 32 14  1 -1 
	 2 29  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3 13  2 26 27 27 27 27 27 27 27 28  2 -1 
	 2 29  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3 13  2  2  2  2  2  2  2  2  2  2  2 -1 
	 2 29  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3 13  2  2 10 11 11 11 11 11 12  2  2 -1 
	 1 30  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3 14  1  1 29  0  0  0  0  0 14  1  1 -1 
	 1 30  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3 14  1  1 29  0  0  0  0  0 14  1  1 -1 
	 1 30  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3 14  1  1 29  0  0  0  0  0 14  1  1 -1 
	 1 30  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3 14  1  1 29  0  0  0  0  0 14  1  1 -1 
	 1 30  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3 14  1  1 29  0  0  0  0  0 14  1  1 -1 
	 2 29  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3 13  2  2 26 27 27 27 27 27 28  2  2 -1 
	 2 29  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3 13  2  2  2  2  2  2  2  2  2  2  2 -1 
	 2 29  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3 13  2  2  2  2  2  2  2  2  2  2  2 -1 
	 2 26 27 27 27 27 27 27 27 27 27 27 27 27 27 27 27 27 27 27 27 27 27 27 27 27 27 27 28  2  2  2  2  2  2  2  2  2  2  2 -1 
	 0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0 -1 
	-1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1

# each piece is defined as a 3x3 matrix of
# boolean flags, with 4 possible rotations:
:data piece
	-1  0 -1 -1  1 -1  0  0  0 # u
	 0 -1 -1  0 -1  0  0 -1 -1
	 0  0  0 -1 -1 -1 -1  0 -1
	-1 -1  0  0 -1  0 -1 -1  0

	 0 -1  0 -1 -1 -1  0 -1  0 # plus
	 0 -1  0 -1 -1 -1  0 -1  0
	 0 -1  0 -1 -1 -1  0 -1  0
	 0 -1  0 -1 -1 -1  0 -1  0

	 0 -1 -1 -1 -1  0 -1  0  0 # w
	-1 -1  0  0 -1 -1  0  0 -1
	 0  0 -1  0 -1 -1 -1 -1  0
	-1  0  0 -1 -1  0  0 -1 -1

	-1 -1  0  0 -1  0  0 -1 -1 # z
	 0  0 -1 -1 -1 -1 -1  0  0
	-1 -1  0  0 -1  0  0 -1 -1
	 0  0 -1 -1 -1 -1 -1  0  0

	 0 -1 -1  0 -1  0 -1 -1  0 # s
	-1  0  0 -1 -1 -1  0  0 -1
	 0 -1 -1  0 -1  0 -1 -1  0
	-1  0  0 -1 -1 -1  0  0 -1

	 0 -1  0 -1 -1  0 -1 -1  0 # d
	-1 -1  0 -1 -1 -1  0  0  0
	 0 -1 -1  0 -1 -1  0 -1  0
	 0  0  0 -1 -1 -1  0 -1 -1

	 0 -1  0  0 -1 -1  0 -1 -1 # b
	 0  0  0 -1 -1 -1 -1 -1  0
	-1 -1  0 -1 -1  0  0 -1  0
	 0 -1 -1 -1 -1 -1  0  0  0

	-1 -1 -1  0 -1  0  0 -1  0 # t
	 0  0 -1 -1 -1 -1  0  0 -1
	 0 -1  0  0 -1  0 -1 -1 -1
	-1  0  0 -1 -1 -1 -1  0  0

	-1 -1 -1  0  0 -1  0  0 -1 # L
	 0  0 -1  0  0 -1 -1 -1 -1
	-1  0  0 -1  0  0 -1 -1 -1
	-1 -1 -1 -1  0  0 -1  0  0
 
:const /piece 36
:const /phase 9
:const pieces 9

# game state:
:var score
:var level
:var lines
:var nextpiece
:var nextcolor
:var currpiece
:var currcolor
:var px
:var py
:var pr

: draw-number (x y n -- )
	>r
	loop
		2dup tile-grid@ i 10 mod 16 + swap ! # draw digit
		swap 1 - swap                        # move cursor
		r> 10 / >r i                         # cast out digits
	while
	2drop r> drop
;

: draw-hud
	37  5 score @ draw-number
	36 11 level @ draw-number
	36 16 lines @ draw-number
;

: piece@ (rot piece -- addr )
	/piece * swap /phase * + piece +
;

:var draw-color
: draw-piece (x y rot piece -- )
	piece@ >r
	/phase 1 - for
		j i + @ if
			over i 3 mod + # x
			over i 3 /   + # y
			tile-grid@ draw-color @ swap !
		then
	next
	2drop r> drop
;

: select
	nextpiece @ currpiece !
	nextcolor @ currcolor !
	0 draw-color !
	33 21 0 nextpiece @ draw-piece
	RN @ pieces mod nextpiece !
	RN @ 4 mod 4 + nextcolor !
	nextcolor @ draw-color !
	33 21 0 nextpiece @ draw-piece
	13 px !
	2  py !
	0  pr !
;

: colliding ( -- flag )
	pr @ currpiece @ piece@
	/phase 1 - for
		dup i + @
		px @ i 3 mod + # x
		py @ i 3 /   + # y
		tile-grid@ @ and
		if r> 2drop true exit then
	next
	drop false
;

:var rflag
: rotate	
	rflag @ if exit then
	pr @ 1 + 4 mod pr !
	colliding if pr @ 1 - 4 mod pr ! then
	true rflag !
;

: move-h ( dx -- )
	px @ over + px !
	colliding if px @ swap - px ! else drop then
;

: check-row ( y -- flag )
	2 + true
	25 for
		over i 2 + swap
		tile-grid@ @ -if drop false then
	next
	swap drop
;

: shift-down ( y -- )
	1 - for
		2 i 2 + tile-grid@
		2 i 3 + tile-grid@
		26 >move
	next
;

: clear-rows
	0
	25 for
		i check-row if 1 + then
	next
	dup -if drop exit then
	
	dup 100 * over * score @ + score !
	lines @ + lines !
	draw-hud sync

	25 for
		i check-row if i shift-down then
	next
;

: reset-game
	0 score !
	1 level !
	0 lines !
	draw-hud
	select
	select

	25 for
		25 for
			i 2 +
			j 2 + tile-grid@
			0 swap !
		next
	next
;

: gameover
	8 for
		"GAME OVER" i + @ 32 -
		i 10 + 12 tile-grid@ !
	next

	loop keys key-a and -if break then sync again
	loop keys key-a and  if break then sync again
	loop keys key-a and -if break then sync again
	reset-game
;

: move-v
	py @ 1 + py !
	colliding if
		py @ 1 - py !
		currcolor @ draw-color !
		px @ py @ pr @ currpiece @ draw-piece
		clear-rows
		select
		10 for sync next
		colliding if gameover then
	then
;

:var timer
: main
	reset-game
	loop
		0 draw-color !
		px @ py @ pr @ currpiece @ draw-piece

		KY @
		dup key-a  and if rotate else false rflag ! then
		dup key-lf and if -1 move-h  then
		dup key-rt and if  1 move-h  then
		    key-dn and if  0 timer ! then

		timer @ -if
			move-v
			8 timer !
		else
			timer @ 1 - timer !
		then

		currcolor @ draw-color !
		px @ py @ pr @ currpiece @ draw-piece

		5 for sync next
	again
;
