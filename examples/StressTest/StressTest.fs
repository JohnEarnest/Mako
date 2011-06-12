######################################################
##
##  StressTest:
##
##  Essentially a benchmark utility. StressTest
##  uses the full gamut of simultaneous sprites and
##  animates every tile in the background every frame.
##  The idea is to simulate a complex game that
##  pushes Mako to the limit.
##
##  John Earnest
##
######################################################

:image grid-tiles "text.png" 8 8
:array grid 1271 0
:const sprite-tiles grid-tiles
:array sprites 1024 0

: sprite@    4 * sprites + ;
: .sprite-t  1 + ;
: .sprite-x  2 + ;
: .sprite-y  3 + ;
: px         sprite@ .sprite-x @ ;
: py         sprite@ .sprite-y @ ;
: px!        sprite@ .sprite-x ! ;
: py!        sprite@ .sprite-y ! ;

:array velocities 512 0

: vx         2 * velocities +     @ ;
: vy         2 * velocities + 1 + @ ;
: vx!        2 * velocities + ! ;
: vy!        2 * velocities + 1 + ! ;
: rand2      RN @ 2 mod ;
: rand-mag   rand2 2 * 1 - rand2 1 + * ;

:var counter

: main

	# initialize sprites
	255 for
		8x8 i sprite@ !
		i 48 mod i sprite@ .sprite-t !
		RN @ 320 mod i px!
		RN @ 240 mod i py!
		rand-mag i vx!
		rand-mag i vy!
	next

	loop
		# update sprite positions
		255 for
			i px i vx + i px!
			i py i vy + i py!

			i px   0 < if   0 i px! i vx -1 * i vx! then
			i py   0 < if   0 i py! i vy -1 * i vy! then
			i px 320 > if 320 i px! i vx -1 * i vx! then
			i py 240 > if 240 i py! i vy -1 * i vy! then
		next

		# update background tiles
		1270 for
			counter @ i + 48 mod
			i grid + !
		next

		sync
		counter @ 1 + 48 mod counter !
	again
;