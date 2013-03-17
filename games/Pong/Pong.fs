######################################################
##
##  Pong:
##
##  The classic game of Pong, implemented with Maker.
##  Demonstrates basic sprite, collision and
##  key input techniques as well as use of the
##  random number generator.
##
##  John Earnest
##
######################################################

:image grid-tiles "text.png" 8 8
:data  grid

	-1 48 47 46 39 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1	0
	-1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1	0
	 0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  3  3  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0	0 
	 0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  3  3  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0	0 
	 0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  3  3  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0	0 
	 0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  3  3  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0	0 
	 0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  3  3  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0	0 
	 0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  3  3  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0	0 
	 0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  3  3  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0	0 
	 0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  3  3  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0	0 
	 0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  3  3  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0	0 
	 0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  3  3  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0	0 
	 0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  3  3  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0	0 
	 0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  3  3  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0	0 
	 0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  3  3  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0	0 
	 0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  3  3  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0	0 
	 0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  3  3  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0	0 
	 0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  3  3  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0	0 
	 0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  3  3  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0	0 
	 0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  3  3  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0	0 
	 0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  3  3  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0	0 
	 0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  3  3  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0	0 
	 0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  3  3  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0	0 
	 0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  3  3  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0	0 
	 0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  3  3  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0	0 
	 0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  3  3  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0	0 
	 0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  3  3  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0	0 
	 0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  3  3  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0	0 
	-1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1	0
	-1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1	0
	 0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0	0

:const sprite-tiles grid-tiles
:data  sprites

	8x8 16 0 0    8x8 16 0 0    8x8 16 0 0    8x8 16 0 0    8x8 16 0 0    8x8 16 0 0    8x8 16 0 0    8x8 16 0 0    # trail and ball
	8x8 9   0 0   8x8 9   0 0   8x8 9   0 0   8x8 9   0 0   8x8 9   0 0   8x8 9   0 0   8x8 9   0 0   8x8 9   0 0   # left  paddle
	8x8 8 312 0   8x8 8 312 0   8x8 8 312 0   8x8 8 312 0   8x8 8 312 0   8x8 8 312 0   8x8 8 312 0   8x8 8 312 0   # right paddle

	0 0 0 0   0 0 0 0   0 0 0 0   0 0 0 0   0 0 0 0   0 0 0 0   0 0 0 0   0 0 0 0
	0 0 0 0   0 0 0 0   0 0 0 0   0 0 0 0   0 0 0 0   0 0 0 0   0 0 0 0   0 0 0 0
	0 0 0 0   0 0 0 0   0 0 0 0   0 0 0 0   0 0 0 0   0 0 0 0   0 0 0 0   0 0 0 0
	0 0 0 0   0 0 0 0   0 0 0 0   0 0 0 0   0 0 0 0   0 0 0 0   0 0 0 0   0 0 0 0
	0 0 0 0   0 0 0 0   0 0 0 0   0 0 0 0   0 0 0 0   0 0 0 0   0 0 0 0   0 0 0 0

	0 0 0 0   0 0 0 0   0 0 0 0   0 0 0 0   0 0 0 0   0 0 0 0   0 0 0 0   0 0 0 0
	0 0 0 0   0 0 0 0   0 0 0 0   0 0 0 0   0 0 0 0   0 0 0 0   0 0 0 0   0 0 0 0
	0 0 0 0   0 0 0 0   0 0 0 0   0 0 0 0   0 0 0 0   0 0 0 0   0 0 0 0   0 0 0 0
	0 0 0 0   0 0 0 0   0 0 0 0   0 0 0 0   0 0 0 0   0 0 0 0   0 0 0 0   0 0 0 0
	0 0 0 0   0 0 0 0   0 0 0 0   0 0 0 0   0 0 0 0   0 0 0 0   0 0 0 0   0 0 0 0
	0 0 0 0   0 0 0 0   0 0 0 0   0 0 0 0   0 0 0 0   0 0 0 0   0 0 0 0   0 0 0 0
	0 0 0 0   0 0 0 0   0 0 0 0   0 0 0 0   0 0 0 0   0 0 0 0   0 0 0 0   0 0 0 0
	0 0 0 0   0 0 0 0   0 0 0 0   0 0 0 0   0 0 0 0   0 0 0 0   0 0 0 0   0 0 0 0

	0 0 0 0   0 0 0 0   0 0 0 0   0 0 0 0   0 0 0 0   0 0 0 0   0 0 0 0   0 0 0 0
	0 0 0 0   0 0 0 0   0 0 0 0   0 0 0 0   0 0 0 0   0 0 0 0   0 0 0 0   0 0 0 0
	0 0 0 0   0 0 0 0   0 0 0 0   0 0 0 0   0 0 0 0   0 0 0 0   0 0 0 0   0 0 0 0
	0 0 0 0   0 0 0 0   0 0 0 0   0 0 0 0   0 0 0 0   0 0 0 0   0 0 0 0   0 0 0 0
	0 0 0 0   0 0 0 0   0 0 0 0   0 0 0 0   0 0 0 0   0 0 0 0   0 0 0 0   0 0 0 0
	0 0 0 0   0 0 0 0   0 0 0 0   0 0 0 0   0 0 0 0   0 0 0 0   0 0 0 0   0 0 0 0
	0 0 0 0   0 0 0 0   0 0 0 0   0 0 0 0   0 0 0 0   0 0 0 0   0 0 0 0   0 0 0 0
	0 0 0 0   0 0 0 0   0 0 0 0   0 0 0 0   0 0 0 0   0 0 0 0   0 0 0 0   0 0 0 0

	0 0 0 0   0 0 0 0   0 0 0 0   0 0 0 0   0 0 0 0   0 0 0 0   0 0 0 0   0 0 0 0
	0 0 0 0   0 0 0 0   0 0 0 0   0 0 0 0   0 0 0 0   0 0 0 0   0 0 0 0   0 0 0 0
	0 0 0 0   0 0 0 0   0 0 0 0   0 0 0 0   0 0 0 0   0 0 0 0   0 0 0 0   0 0 0 0
	0 0 0 0   0 0 0 0   0 0 0 0   0 0 0 0   0 0 0 0   0 0 0 0   0 0 0 0   0 0 0 0
	0 0 0 0   0 0 0 0   0 0 0 0   0 0 0 0   0 0 0 0   0 0 0 0   0 0 0 0   0 0 0 0
	0 0 0 0   0 0 0 0   0 0 0 0   0 0 0 0   0 0 0 0   0 0 0 0   0 0 0 0   0 0 0 0
	0 0 0 0   0 0 0 0   0 0 0 0   0 0 0 0   0 0 0 0   0 0 0 0   0 0 0 0   0 0 0 0
	0 0 0 0   0 0 0 0   0 0 0 0   0 0 0 0   0 0 0 0   0 0 0 0   0 0 0 0   0 0 0 0

:var score-lf
:var score-rt
:var vx
:var vy

:const  ball-sprite  7
:const  left-paddle  8
:const right-paddle 16

: neg@       dup @ -1 * swap ! ;
: inc@       dup @  1 + swap ! ;
: dec@       dup @  1 - swap ! ;
: brownian   RN @ 3 mod 1 - +  ;
: rand-mag   RN @ 2 mod 2 * 1 - RN @ 2 mod 1 + * ;
: sprite@    4 * sprites + ;
: .sprite-x  2 + ;
: .sprite-y  3 + ;
: up         keys key-up and ;
: down       keys key-dn and ;

: paddle! ( paddle-id y -- )
	dup 16 < over 160 > or if drop drop exit then

	7 for                  # move all 8 segments together:
		over i + sprite@   # address of segment sprite
		over i 8 * +       # segment y position
		swap .sprite-y !   # set y-field to position
	next
	drop drop
;

: px   ball-sprite sprite@ .sprite-x @ ;
: py   ball-sprite sprite@ .sprite-y @ ;
: px!  ball-sprite sprite@ .sprite-x ! ;
: py!  ball-sprite sprite@ .sprite-y ! ;
: ly   left-paddle  sprite@ .sprite-y @ ;  
: ry   right-paddle sprite@ .sprite-y @ ;
: ly!  left-paddle  swap paddle! ;
: ry!  right-paddle swap paddle! ;

: reset-game
	# initialize the ball
	rand-mag vx !
	rand-mag vy !
	156 px!
	116 py!

	# reset the trail
	6 for
		i sprite@ dup
		.sprite-x px swap !
		.sprite-y py swap !
	next

	# reset the paddles
	88 ly!
	88 ry!

	# update the scoreboard
	score-lf @ 10 mod 16 + grid  97 + !
	score-rt @ 10 mod 16 + grid 106 + !

	40 for sync next
;

: main
	reset-game
	loop
		# shift the trail
		6 for
			6 i - sprite@ dup
			.sprite-x dup 4 + @ brownian swap !
			.sprite-y dup 4 + @ brownian swap !
		next

		# move the ball
		vx @ px + px!
		vy @ py + py!
		py  16 < if  16 py! vy neg@ then
		py 216 > if 216 py! vy neg@ then

		# left paddle (Human player)
		up   if ly 4 - ly! then
		down if ly 4 + ly! then
		px 8 < if
			py ly  8 - >
			py ly 64 + <
			and if
				8 px! vx neg@
				# add english:
				up   if vy dec@ then
				down if vy inc@ then
			then
		then

		# right player (AI opponent)
		px 160 > if
			py ry  8 - < if ry 3 - ry! then
			py ry 64 + > if ry 3 + ry! then
		then
		px 304 > if
			py ry  8 - >
			py ry 64 + <
			and if 304 px! vx neg@ then
		then

		px   0 < if score-rt inc@ reset-game then
		px 316 > if score-lf inc@ reset-game then

		sync
	again
;