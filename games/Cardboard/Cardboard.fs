######################################################
##
##  We Love Cardboard Kombat
##
##  A frenetic action-packing game.
##  Second CogNation Game Jam- "What's in a Name?"
##
##  John Earnest
##
######################################################

:include <Math.fs>
:include <Util.fs>
:include <Grid.fs>
:include <Sprites.fs>
:include <String.fs>
:include "Entities.fs"

:image sprite-tiles "boxSprites.png" 16 16
:image   grid-tiles "boxTiles.png"    8  8
:image         font "text.png"        8  8

:const clear-color 0xFFFFE26F

:data grid
:data title-screen
	 34   4   4   4   4   4   4   4   4  20   4   4   4   4   4  20  37  33   0   0   0  32   2   2  34  36   4  36   4   4  37   4  37   2  82   2  34   0   0   0  -1 
	  2  34   4   4  36  37  85   4  35  34   4   4  85   4  37  37  33   0   0   0   0   0   0   0  16  34  36  20  36  37  36  37   4  19   2   2   0   0   0   0  -1 
	  2   2  34   4   4   4   4  35   2   2  34   4   4  37  37  33   0   0   0   0   0   0   0  16   2   2  34   4   4   4   4   4   4  35  17   0   0   0   0   0  -1 
	  2   2   2  34   4   4  35   2   2   2   2  34  37  37  33   0   0   0   0   0   0   0   0  32   2  80   2  36   4   4   4   4  35   2   2  17   0   0   0   0  -1 
	 19   2   2   2  34  35   2   2   2   2   2  33   0   0   0   0   0   0   0   0   0   0   0   0  32   2   2   4  36   4   4  35   2  80  50  33   0   0   0   0  -1 
	  0  32   2   2  33  32   2   2   2   2  33   0   0   0   0   0   0   0   0   0   0   0   0   0   0  32   2   2   2  34  35   2   2   2  33   0   0   0   0   0  -1 
	  0   0  32  33  16  17  32   2   2  33   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0  32   2   2  33  32   2   2  33   0   0   0   0   0   0  -1 
	  0   0   0   0  32  33   0  32  33   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0  32  33   0   0  32  33   0   0   0   0   0   0   0  -1 
	  0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0  -1 
	  0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0  -1 
	  0   0   0   0   0   0   0   0   0   0   0  96  97  98  99 100 101   0   0 102 103 104 105 106 107 108 109 110 111   0   0   0   0   0   0   0   0   0   0   0  -1 
	  0   0   0   0   0   0   0   0   0   0   0 112 113 114 115 116 117   0   0 118 119 120 121 122 123 124 125 126 127   0   0   0   0   0   0   0   0   0   0   0  -1 
	  0   0   0   0   0   0   0   0   0   0   0 128 129 130 131 132 133   0   0 134 135 136 137 138 139 140 141 142 143   0   0   0   0   0   0   0   0   0   0   0  -1 
	  0   0   0   0   0   0   0   0   0   0   0 144 145 146 147 148 149   0   0 150 151 152 153 154 155 156 157 158 159   0   0   0   0   0   0   0   0   0   0  16  -1 
	  0   0   0   0   0   0   0   0   0   0   0 160 161 162 163 164 165   0   0 166 167 168 169 170 171 172 173 174 175   0   0   0   0   0   0   0   0   0  16   2  -1 
	  0   0   0   0   0   0   0   0   0   0   0   0 176 177 178 179 180 181 177 178 179   0 183 181 182 180 177 184 185   0   0   0   0   0   0   0   0  16   2   2  -1 
	  0   0   0   0   0   0   0   0   0   0   0   0 192 193 194 195 196 197 193 194 195   0 199 197 198 196 193 200 201   0   0   0   0   0   0   0   0  32   2   2  -1 
	  0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0  32   2  -1 
	  0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0  32  -1 
	  0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0  -1 
	  0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0  -1 
	  0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0  -1 
	  0   0  16  17   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0  -1 
	  0  16   2   2  17   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0  -1 
	 16   2  49   2   2  17   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0  -1 
	 19   2   2  49   2   2  17   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0  16  18  19  17  16  17  16  -1 
	  4  19   2   2  18  19   2  17   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0  16  17   0  16  18   4  83  19   2  18  19  -1 
	  4   4  19  18   4   4  19  33   0   0  16  17   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0  16   2   2  17  18   4  83   4   4  19  36   4  -1 
	  4   4  20   4   4   4   4  19   0  16   2   2  17   0   0   0   0   0   0   0   0   0   0   0   0   0   0  16   2   2   2   2  34   4   4   4   4   4   4  36  -1 
	  4  20   4   4  20  21  20  21  19   2  18  19   2  17   0   0   0   0   0   0   0   0   0   0   0   0  16   2   2  48  50   2   2  34   4   4   4   4  20   4  -1 
	 -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1 

:data background
	 34   4   4   4   4   4   4   4   4  20   4   4   4   4   4  20  37  33   0   0   0  32   2   2  34  36   4  36   4   4  37   4  37   2  82   2  34   0   0   0  -1 
	  2 1073741834 1073741835   4   4   4   4   4  35  34   4   4   4   4  37  37   0   0   0   0   0   0   0   0  16  34  36  20  36  37  36  37   4  19   2   2   0   0   0   0  -1 
	  2 1073741850 1073741851   4   4   4   4  35   2   2  34   4   4  37  37  33   0   0   0   0   0   0   0  16   2   2  34   4   4   4   4   4   4  35  17   0   0   0   0   0  -1 
	  2   2   2  34   4   4  35   2   2   2   2  34  37  37  33   0   0   0   0   0   0   0   0  32   2  80   2  36   4   4   4   4  35   2   2  17   0   0   0   0  -1 
	 19   2   2   2  34  35   2   2   2   2   2  33   0   0   0   0   0   0   0   0   0   0   0   0  32   2   2   4  36   4   4  35   2  80  50  33   0   0   0   0  -1 
	  0  32   2   2  33  32   2   2   2   2  33   0   0   0   0   0   0   0   0   0   0   0   0   0   0  32   2   2   2  34  35   2   2   2  33   0   0   0   0   0  -1 
	  0   0  32  33  16  17  32   2   2  33   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0  32   2   2  33  32   2   2  33   0   0   0   0   0   0  -1 
	  0   0   0   0  32  33   0  32  33   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0  32  33   0   0  32  33   0   0   0   0   0   0   0  -1 
	  0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0  -1 
	  0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0  -1 
	  0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0  -1 
	  0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0  -1 
	  0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0  -1 
	  0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0  16  -1 
	  0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0  16   2  -1 
	  0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0  16   2   2  -1 
	  0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0  32   2   2  -1 
	  0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0  32   2  -1 
	  0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0  32  -1 
	  0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0  -1 
	  0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0  -1 
	  0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0  -1 
	  0   0  16  17   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0  -1 
	  0  16   2   2  17   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0  -1 
	 16   2  49   2   2  17   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0  -1 
	 19   2   2  49   2   2  17   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0  16  18  19  17  16  17  16  -1 
	  4  19   2   2  18  19   2  17   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0  16  17   0  16  18   4  83  19   2  18  19  -1 
	  4   4  19  18   4   4  19  33   0   0  16  17   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0  16   2   2  17  18   4  83   4   4  19  36   4  -1 
	  4   4  20   4   4   4   4  19   0  16   2   2  17   0   0  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1   0   0  16   2   2   2   2  34   4   4   4   4   4   4  36  -1 
	  4  20   4   4  20  21  20  21  19   2  18  19   2  17   0   0   0   0   0   0   0   0   0   0   0   0  16   2   2  48  50   2   2  34   4   4   4   4  20   4  -1 
	 -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1

######################################################
##
##  Game State
##
######################################################

:const max-health 20
:var score
:var health
:var you

:const weapon-star  1
:const weapon-blade 0
:const weapon-mine  2
:var   weapon

######################################################
##
##  UI Stuff
##
######################################################

:data hel  4 36 37 85 4 35 34 4 4 85 4 37 37 33 0 0 0 0 0 0 0 

: draw-hud
	max-health for
		i health @ < if 12 grid-z or else i hel + @ then
		i 3 + 1 tile-grid@ !
	next
	13 grid-z or health @ 3 + 1 tile-grid@ !
	23 28 score @ draw-number
	16x16 weapon @ 8 + 296 216 253 >sprite
;

: wait
	loop think sync keys key-a and until
	loop think sync keys key-a and while
;

: title
	clear-entities
	title-screen GP !
	wait
;

:data go1	1073742010 1073742001 1073742006 1073742011  -1  -1
			1073742005 1073742012 1073742011 1073742002 1073742009 
:data go2	1073742026 1073742017 1073742022 1073742027  -1  -1
			1073742021 1073742028 1073742027 1073742018 1073742025

: end-game
	0 0 0 0 253 >sprite
	go1 15 13 tile-grid@ 11 >move
	go2 15 14 tile-grid@ 11 >move
	wait
	15 13 tile-grid@ 11 -1 fill
	15 14 tile-grid@ 11 -1 fill
;

######################################################
##
##  Entity Utils
##
######################################################

:data dx  0  1  0 -1   -1  1  1 -1
:data dy -1  0  1  0   -1 -1  1  1
: delta-x dx + @ ;
: delta-y dy + @ ;

: offscreen? ( id -- flag )
	>r
	i  px -16 <
	i  px 320 > or
	i  py -16 < or
	r> py 240 > or
;

: move-in-dir ( mag id -- )
	>r
	i dir @ delta-x over * i  +px
	i dir @ delta-y swap * r> +py
;

: player-dir ( i -- dir )

	dup  px you @ px <
	over py you @ py > and if drop 5 exit then
	dup  px you @ px <
	over py you @ py < and if drop 6 exit then
	dup  px you @ px >
	over py you @ py > and if drop 4 exit then
	dup  px you @ px >
	over py you @ py < and if drop 7 exit then

	dup px you @ px < if drop 1 exit then
	dup px you @ px > if drop 3 exit then
	dup py you @ py < if drop 2 exit then
	drop 0
;

: time-out ( id )
	timer @ 1 < if rdrop r> free then
;

######################################################
##
##  Pickups and effects
##
######################################################

: poof ( id -- )
	>r
	i time-out
	i timer @ 5 > if 19 else 20 then i tile!
	r> timer dec@
;

: make-poof ( id -- )
	dup show
	' poof over kind !
	10 swap timer !
;

: love
	>r
	i time-out
	21 i tile!
	i timer @ 10 > if 22 i tile! then
	i timer dec@
	i py 2 - i py!
	rdrop
;

: spawn-love ( id -- )
	>r
	6 for
		15 random 10 +
		27
		20 random 10 -
		20 random 15 -
		j ' love spawn-rel
		timer !
	next
	rdrop
;

: shadow
	>r
	i time-out
	i timer @ 8 - i timer !
	rdrop
;

:data  bounce     -1 -1 -1 1 1 1 -1 -1 1 1
:const bounce-len 10

: pickup-common ( 'func id -- )
	>r
	i timer @ 0 > if
		i timer @ 8 - i timer !
		i py 8 + i py!
	else
		i timer @ -1 bounce-len * > if
			i timer @ abs bounce + @ i py + i py!
		else
			i you @ c-sprites? if
				dup exec
				i spawn-love
				i make-poof
			then
		then
		i timer dec@
	then
	rdrop
	drop
;

: health-pickup
	{
		health @ 10 + max-health min health !
		1000 score +@
	} swap pickup-common
;

: blade-pickup
	{
		weapon-blade weapon !
		2000 score +@
	} swap pickup-common
;

: star-pickup
	{
		weapon-star weapon !
		2500 score +@
	} swap pickup-common
;

: mine-pickup
	{
		weapon-mine weapon !
		3000 score +@
	} swap pickup-common
;

:data pickup-types
	health-pickup
	blade-pickup
	star-pickup
	mine-pickup
:data pickup-icons
	11 8 9 10

:var r-type
: spawn-pickup ( -- )
	32 random 4 + >r
	24 random 3 + >r
	4 random r-type !
	r-type @ pickup-icons + @ j i 40 - r-type @ pickup-types + @ spawn 320 swap timer !
	14                        j i                       ' shadow spawn 320 swap timer !
	rdrop rdrop
;

######################################################
##
##  Player weapons
##
######################################################

:proto stockboy
:var hitter
:var hitted
:var damage
: hit-enemies ( damage id -- flag )
	hitter !
	damage !
	false hitted !
	{
		dup kind   @ ' stockboy =
		-if drop false exit then
		hitter @ c-sprites?
	}
	{
		dup hp @ damage @ - over hp !
		true swap hit !
		true hitted !
	} whoever
	hitted @
;

: star
	>r
	i offscreen? 1 i hit-enemies or if
		i parent @ bullet dec@
		r> free exit
	then
	6 i move-in-dir
	i timer inc@
	i timer @ 2 / 2 mod if 24 else 25 then i tile!
	rdrop
;

: spawn-star ( parent -- )
	>r
	i bullet @ 8 > if rdrop exit then
	24 0 -8  i ' star spawn-rel
	i over parent !
	i dir @ swap dir !
	i bullet inc@
	rdrop
;

:data blade-frame   31 29 31 29                30 30 30 30
:data blade-status  16x16 69889 135425 16x16   16x16 69889 200961 135425

: blade
	>r
	i offscreen? 6 i hit-enemies or if
		i parent @ bullet dec@ 
		r> free
		exit
	then
	3 i move-in-dir
	i dir @ blade-frame  + @ i tile!
	i dir @ blade-status + @ i sprite@ !
	rdrop
;

:data o1 4 5 6 7
:data o2 5 6 7 4

: spawn-blade ( parent -- )
	>r
	i bullet @ 3 > if rdrop exit then
	
	0 0 0 i ' blade spawn-rel
	i over parent !
	i dir @        swap dir !

	0 0 0 i ' blade spawn-rel
	i over parent !
	i dir @ o1 + @ swap dir !

	0 0 0 i ' blade spawn-rel
	i over parent !
	i dir @ o2 + @ swap dir !
	
	i bullet @ 3 + i bullet !
	rdrop
;


: exploder
	>r
	999 i hit-enemies drop
	i time-out
	16 i tile!
	i timer @ 10 > if 17 i tile! then
	i timer @ 15 > if 18 i tile! then
	i timer dec@
	i py brownian 2 * + i py!
	i px brownian 2 * + i px!
	rdrop
;

: explode ( id -- )
	>r
	6 for
		15 random 10 +
		27
		20 random 10 -
		20 random 15 -
		j ' exploder spawn-rel
		timer !
	next
	rdrop
;

: mine
	>r
	i timer @ -if
		i parent @ bullet dec@
		i explode
		r> free exit
	then
	i timer dec@
	rdrop
;

:var mine-cook
: spawn-mine ( parent -- )
	>r
	mine-cook @ if mine-cook dec@ rdrop exit then
	i bullet @ 3 > if rdrop exit then
	23 0 0 i ' mine spawn-rel
	i over parent !
	50 swap timer !
	i bullet inc@
	4 mine-cook !
	rdrop
;

######################################################
##
##  Player
##
######################################################

:data  player-anim    0 1 2 1
:data  player-anim-dy 0 3 -6 3
:const player-anim-len 4

: player ( -- )
	>r
	health @ 1 < if rdrop exit then
	
	keys key-up and if -2 i +py   0 i dir ! then
	keys key-dn and if  2 i +py   2 i dir ! then
	keys key-lf and if -2 i +px   3 i dir ! then
	keys key-rt and if  2 i +px   1 i dir ! then
	
	keys key-up key-dn key-lf key-rt or or or and if
		i timer inc@
		i timer @ 8 / player-anim-len mod
		player-anim + @ i tile!
		
		i timer @ 8 mod -if
			i timer @ 8 / player-anim-len mod
			player-anim-dy + @ i +py
		then
	else
		0 i timer !
		0 i tile!
	then

	keys key-a and if
		weapon @ weapon-star  = if i spawn-star  then
		weapon @ weapon-blade = if i spawn-blade then
		weapon @ weapon-mine  = if i spawn-mine  then
		5 i tile!
	then
	
	rdrop
;

: spawn-player ( -- )
	0 >r
	' player i kind !
	16x16 i sprite@ !
	160 8 - i px!
	120 8 - i py!
	0 i tile!
	r> you !
;

######################################################
##
##  Enemies
##
######################################################

:data box-frame   28 26 28 26                27 27 27 27
:data box-status  16x16 69889 135425 16x16   16x16 69889 200961 135425

: boxcutter
	>r
	i offscreen? if
		r> free exit
	then
	i you @ c-sprites? if
		10 random 1 + score @ + score !
		health dec@
		i spawn-love
		r> free exit
	then
	2 i move-in-dir
	i dir @ box-frame  + @ i tile!
	i dir @ box-status + @ i sprite@ !
	rdrop
;

: spawn-boxcutter ( parent -- )
	>r 0 0 0 i ' boxcutter spawn-rel
	r> dir @ swap dir !
;

: hurt-stockboy
	>r
	34 i tile!
	i timer @ 4 / 2 mod if i hide else i show then
	i timer dec@
	i timer @ -if
		' stockboy i kind !
		false i hit !
	then
	rdrop
;

: stockboy
	>r
	i timer inc@
	i timer @ 4 / 2 mod 32 + i tile!
	
	i player-dir i dir !

	i hit @ if
		i spawn-love
		i hp dec@
		i hp @ 1 < if
			20 random 500 + score +@
			r> make-poof exit
		else
			10 random 10 + score +@
			' hurt-stockboy i kind !
			20 i timer !
			-4 i move-in-dir
		then
	then
	i dir @ dup 1 = if i face-left  then
	            3 = if i face-right then
	
	i timer @ 2 mod health @ 0 > and if
		1 i move-in-dir
	then
	
	i timer @ 20 > if
		0 i timer !
		i spawn-boxcutter
	then
	rdrop
;

: spawn-stockboy
	32
	2 random 42 * 1 -
	2 random 32 * 1 -
	' stockboy spawn
	hp 3 swap !
;

######################################################
##
##  Game Logic
##
######################################################

: init-game
	clear-entities
	background GP !
	15 28 tile-grid@ 10 -1 fill
	         0  score  !
	max-health  health !
	weapon-blade weapon !
	spawn-player
;

: toroid
	you @ px -15 < if 319 you @ px! then
	you @ py -15 < if 239 you @ py! then
	you @ px 319 > if -15 you @ px! then
	you @ py 239 > if -15 you @ py! then
;

: main
	224    ascii     !
	grid-z grid-mask !

	loop
		title
		init-game
		loop
			200 random -if spawn-pickup  then
			100 random -if spawn-stockboy then
			think toroid draw-hud sync
			health @ 1 <
		until
		end-game
	again
;