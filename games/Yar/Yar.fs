######################################################
##
##  Yar:
##
##  A semi-faithful clone of the classic Atari 2600
##  game, Yar's Revenge.
##
##  John Earnest
##
######################################################

:include <Grid.fs>
:include <Util.fs>
:include <Sprites.fs>
:include <Print.fs>

:image   grid-tiles "neutral.png"  8  8
:image      numbers "numbers.png"  8  8
:image sprite-tiles "sprites.png" 16 16

:data lives 4
:var  score
:var  levelno
:var  timer
:var  charge
:var  zfire
:var  swirl

:var  rotate
:var  sindex
:var  scenter
:var  sdir

:const ysprite 0 # yar
:const bsprite 1 # yar's bullet
:const qsprite 2 # the dreaded qotile/swirl
:const msprite 3 # destroyer missile
:const zsprite 4 # the zarlon cannon

: points ( n -- )
	score @ + score !
;

######################################################
##
##  Audio kernel and sound effects:
##
######################################################

:var ia   # registers for background noise
:var ib
:var it
:var sfx  # sound effect function pointer
:var sft  # sound effect timer
:var sf1  # sound effect register 1

: period ( n -- )
	it @ -if
		ib @ ia ! dup it !
		RN @ 64 mod ib !
	then
	drop it dec@
;

:proto not-neutral
: tick ( -- )
	140 for
		not-neutral if
			8 period ib @ ia @ - 7 it @ - * 8 / ia @ +
		else
			4 period ib @ ia @ - 3 it @ - * 4 / ia @ +
		then
		sfx @ if
			sft dec@
			sfx @ exec 2 * + 3 /
			sft @ -if 0 sfx ! then
		then
		AU !
	next
	sync
;

: play-buzz ( -- )
	{
		RN @ 32 mod if RN @ 2 mod sf1 +@ then
		sf1 @ 45 mod
	} sfx ! 2000 sft !
;

: play-beep ( -- )
	{
		sft @ 100 mod -if sf1 dec@ then
		sft @ sf1 @ swap over mod swap 64 swap / *
	} sfx ! 2000 sft ! 30 sf1 !
;

: play-bang ( -- )
	{
		sft @ 100 mod -if sf1 dec@ then
		62 sft @ over mod swap 64 swap / *
		RN @ sf1 @ mod + 2 /
	} sfx ! 4000 sft ! 50 sf1 !
;

: play-laser ( -- )
	{
		sft @ 100 mod -if sf1 inc@ then
		sft @ 8 / 3 *
		sft @ 2 / 4 * xor 64 mod
		sf1 @ - dup 0 < if drop 0 then
	} sfx ! 6000 sft ! 0 sf1 !
;

######################################################
##
##  The neutral zone and the Qotile's shield:
##
######################################################

: neutral ( -- )
	29 for
		6 for
			RN @ 6 mod j + 31 mod
			RN @ 2 mod if grid-z or then
			i 14 + j tile-grid@ !
		next
	next
;

: not-neutral ( -- flag )
	112 ysprite px 152 within not
;

:array buffer 16  0
:array scells 128 0
:array buffer 16  0

:data  barrier
	0 0 0 1 1 1 1 1
	0 0 1 1 1 1 1 1
	0 1 1 1 1 1 1 1
	1 1 1 1 1 1 1 1
	1 1 1 1 1 0 0 0
	1 1 1 1 0 0 0 0
	1 1 1 1 0 0 0 0
	1 1 1 1 0 0 0 0
	1 1 1 1 0 0 0 0
	1 1 1 1 0 0 0 0
	1 1 1 1 1 0 0 0
	1 1 1 1 1 1 1 1
	0 1 1 1 1 1 1 1
	0 0 1 1 1 1 1 1
	0 0 0 1 1 1 1 1
	0 0 0 0 0 0 0 0

: shield@ ( x y -- addr )
	8 * + sindex @ + 128 mod scells +
;

: shield ( -- )
	timer @ 12 mod -if
		sdir @ scenter +@
		scenter @  0 = if  1 sdir ! then
		scenter @ 15 = if -1 sdir ! then
	then

	15 for
		7 for
			i j shield@ @
			31 RN @ 2 mod if grid-z or then and
			i 32 + j scenter @ + tile-grid@ !
		next
	next

	rotate @ if
		timer @ 6 mod -if sindex dec@ then
	then
;

: init-shield ( -- )
	rotate @ if
		127 for -1                i scells + ! next
	else
		127 for i barrier + @ 1 = i scells + ! next
	then
	RN @ 2 mod if 1 else -1 then sdir !
	7 scenter !
;

: in-shield? ( sprite -- flag )
	>r
	32        i px 8 / 1 + 39             within
	scenter @ i py 8 / 1 + scenter @ 15 + within and
	rdrop
;

: hit-cell@ ( sprite -- addr )
	>r
	i px 8 / 1 + 32 -
	i py 8 / 1 + scenter @ - shield@
	rdrop
;

: hit-shield? ( sprite -- flag )
	dup in-shield? -if drop false exit then
	hit-cell@ dup @ if
		play-bang
		dup 1 - off
		dup 1 + off
		dup 8 - off
		dup 8 + off
		off  true
	else
		drop false
	then
;

: bite-shield? ( -- flag )
	ysprite in-shield? -if false exit then
	ysprite hit-cell@ dup @ if
		off  true
	else
		drop false
	then
;

: bounce-shield ( -- )
	loop
		ysprite in-shield?  -if exit then
		ysprite hit-cell@ @ -if exit then
		play-buzz
		-8 ysprite +px
	again
;

######################################################
##
##  Game initialization and level advancement
##
######################################################

: level ( -- )
	    0 0   0   0 bsprite >sprite
	    0 0   0   0 zsprite >sprite
	16x16 4  32 108 ysprite >sprite
	16x16 0 304 108 qsprite >sprite
	16x16 2 304 108 msprite >sprite
	levelno @ 2 mod rotate ! init-shield
	#true rotate ! init-shield
	0 sindex !
	0 timer  !
	0 charge !
	0 swirl  !
	zfire off
;

: cls ( -- )
	1270 for 0 i GP @ + ! next
;

: boomscreen ( count -- )
	0 qsprite sprite@ !
	0 msprite sprite@ !
	0 zsprite sprite@ !
	for
		39 for
			RN @ 8 mod j + 32 mod dup
			i 14 j - tile-grid@ !
			i j 14 + tile-grid@ !
		next
	next
	5 for tick next cls
;

: score-screen ( -- )
	cls
	4 for i hide next
	-16 ascii !
	30 10 score @ draw-number
	30 12 lives @ draw-number
	loop tick keys key-a and while
	loop tick keys key-a and until
	cls level
;

: boom ( -- )
	10 for tick next
	1
	loop dup boomscreen 1 + dup 14 < while
	loop dup boomscreen 1 - dup      while
	drop
	30 for tick next
	levelno inc@
	score-screen
;

: die ( -- )
	play-bang
	lives dec@
	lives @ 0 < if
		4 lives   !
		0 score   !
		0 levelno !
	then
	 8 ysprite tile! 10 for tick next
	12 ysprite tile! 10 for tick next
	13 ysprite tile! 10 for tick next
	   ysprite hide  30 for tick next
	score-screen
;

######################################################
##
##  Logic for Yar and its projectile
##
######################################################

:var yflap
:var ychew
: yar ( -- )
	keys key-up and if
		-2 ysprite +py
		8 ysprite tile!
		16x16 sprite-mirror-vert or ysprite sprite@ !
	then
	keys key-dn and if
		2 ysprite +py
		8 ysprite tile!
		16x16 ysprite sprite@ !
	then
	keys key-lf and if
		-2 ysprite +px
		4 ysprite tile!
		16x16 sprite-mirror-horiz or ysprite sprite@ !
	then
	keys key-rt and if
		 2 ysprite +px
		4 ysprite tile!
		16x16 ysprite sprite@ !
	then
	
	keys key-lf key-up or =
	keys key-lf key-dn or = or
	keys key-rt key-up or = or
	keys key-rt key-dn or = or if
		6 ysprite tile!
	then

	keys 15 and if
		yflap @ 16 mod 8 > if
			ysprite tile 1 xor ysprite tile!
		then
		yflap inc@
	else
		ysprite tile 1 not and ysprite tile!
		0 yflap !
	then

	ysprite py  -8 < if 232 ysprite py! then
	ysprite py 232 > if  -8 ysprite py! then
	ysprite px   0 < if   0 ysprite px! then
	ysprite px 304 > if 304 ysprite px! then

	ychew @ if
		ychew @ 4 / 2 mod if 10 else 4 then
		ysprite tile!
		ychew dec@
	else
		bite-shield? if
			169 points
			16 ychew !
			charge inc@
		then
	then
	bounce-shield
;

: bullet ( -- )
	bsprite sprite@ @ if
		4 bsprite +px
		bsprite hit-shield? if 0 bsprite sprite@ ! 69 points exit then
		bsprite px 320 >    if 0 bsprite sprite@ !                then
	else
		keys key-a and not-neutral and if
			play-beep
			16x16 11
			ysprite px 8 +
			ysprite py
			bsprite >sprite
		then
	then
;

######################################################
##
##  Logic for the Zorlon cannon
##
######################################################

: zorlon ( -- )
	zfire @ if
		8 zsprite +px
	
		zsprite ysprite c-sprites? if
			zsprite hide
			zfire off
			die
			exit
		then

		zsprite qsprite c-sprites? if
			zfire off

			swirl @ if
				swirl @ 1 = if
					2000 points
				else
					6000 points
					lives inc@
				then
			else
				1000 points
			then

			boom
			exit
		then

		zsprite hit-shield? if zsprite hide zfire off then
		zsprite px 320 >    if zsprite hide zfire off then
	else
		charge @ 3 > if
			16x16 3
			0
			ysprite py
			zsprite >sprite

			keys key-a and if
				play-laser
				zfire on
				0 charge !
			then
		else
			zsprite hide
		then
	then
;

######################################################
##
##  The Qotile and its projectile
##
######################################################

:var stimer
:var sdir

: sgn   dup if 0 < if -1 else 1 then then ;

: qotile ( -- )
	swirl @ if
		swirl @ 1 = if
			stimer dec@
			stimer @ 0 < if 2 swirl ! play-laser then
			ysprite py qsprite py - sgn sdir !
		else
			    -6 qsprite +px
			sdir @ qsprite +py

			qsprite px -16 < if
				0 swirl !
				16x16 0 304 108 qsprite >sprite
			then
		then

		qsprite ysprite c-sprites? if die then
	else
		RN @ 1000 mod -if
			1 swirl !
			1 qsprite tile!
			RN @ 200 mod 100 + stimer !
		then
	then
	swirl @ 2 = -if
		scenter @ 8 * 52 + qsprite py!
	then
;

: missile ( -- )
	timer @ 3 mod -if
		ysprite px msprite px - sgn msprite +px
		ysprite py msprite py - sgn msprite +py
	then
	msprite px  4 + msprite py 7 + ysprite c-sprite?
	msprite px 11 + msprite py 8 + ysprite c-sprite? or
	not-neutral and
	if die then
;

######################################################
##
##  Main game loop
##
######################################################

: main
	level
	loop
		timer inc@
		cls
		shield
		neutral
		yar
		bullet
		zorlon
		qotile
		missile
		tick
	again
;