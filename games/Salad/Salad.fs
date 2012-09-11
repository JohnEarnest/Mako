######################################################
##
##  Salad:
##
##  A "Bejeweled"-style pattern matching puzzle
##  game, "Metal Gear Salad".
##
##  John Earnest
##
######################################################

:include <Grid.fs>
:include <Sprites.fs>
:include <String.fs>
:include <Print.fs>

: inc   dup @ 1 + swap ! ;
: dec   dup @ 1 - swap ! ;
: on    true  swap !     ;
: off   false swap !     ;

: pos!  dup >r py! r> px! ; ( x y sprite -- )

:array   data-stack 500 0
:array return-stack 500 0

######################################################
##
##  Audio Kernel
##
######################################################

:var sfx  # sound effect function pointer
:var sft  # sound effect timer
:var sfa  # sound effect register A
:var sfb  # sound effect register B

: tick ( -- )
	140 for
		sfx @ if
			sft dec
			sfx @ exec
			sft @ -if 0 sfx ! then
		else
			0
		then
		AU !
	next
	sync
;

: s/      dup -if 2drop 0 else / then ;
: square            4 / / 2 mod 32 * ;

: play-spin ( -- ) # spin a metal gear?
	{
		sft @ 50 mod -if sfa inc@ then
		sft @ 4 / 3 *
		sft @ 4 / 4 * xor 32 mod
		sfa @ - dup 0 < if drop 0 then

	} sfx ! 4000 sft ! 0 sfa !
;

: play-nope ( -- ) # unable to spin
	{
		sft @ 64 mod 2 /
	} sfx ! 2000 sft !
;

: play-small ( -- ) # move the cursor
	{
		sft @ 2 * 110 mod -if sfa dec@ then
		sft @ sfa @ swap over mod swap 20 swap / *
	} sfx ! 1000 sft ! 30 sfa !
;

: play-big ( -- ) # slide a row
	{
		sft @ 80 mod -if sfa dec@ then
		sft @ sfa @ swap over mod swap 25 swap / *
	} sfx ! 1000 sft ! 20 sfa !
;

: play-buzz ( -- )
	{
		sft @ 2 / dup 23 / 50 + square
	} sfx ! 1000 sft !
;

: play-hum ( -- )
	{
		sft @ 4 / 120 square
		sft @ 4 / 126 square +
		sft @ 4 / 149 square + 3 /
	} sfx ! 1000 sft !
;

: play-on ( -- ) # activate/end codec
	{
		sft @ 7 and  sft @ 64 / 7 and *
	} sfx ! 4000 sft !
;

######################################################
##
##  Title Screen
##
######################################################

:image title-tiles "title.png" 8 8
:data title-board
	  0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0 0
	  0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0 0
	  0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0 0
	  0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0 0
	  0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0 0
	  0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0 0
	  0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0 0
	  0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0 0
	  0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0 0
	  0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0 0
	  0   0   0   0   0   0   0   0   0   0   1   2   3   4   5   6   7   8   0   9  10  11  11  12  13   3   4   5  14  15   0   0   0   0   0   0   0   0   0   0 0
	  0   0   0   0   0   0  16  17  18  19  20  21  22  23  21  24  25  26   0  27   0   0  28  21  29  30  21  31  32  33  34  35  21  21  36   0   0   0   0   0 0
	  0   0   0   0   0   0  37  38  39  40  41  42  43  44  45  46  47  48  49  50   0  51  52  53  54  55  42  56  57  42  58  59  42  60  61   0   0   0   0   0 0
	  0   0   0   0   0  62  63  64  65  66  67  68  69  70  71  72  73  74  75  76   0  77  78  79  80  81  82  83  84  85  86  87  88  89  90   0   0   0   0   0 0
	  0   0   0   0   0  91  71  92  93  94  95  96  97  98  99 100 101 102 103 104 105 106 107 108 109 110 111 112 113 114 115 116 117 118   0   0   0   0   0   0 0
	  0   0   0   0   0 119 120 121 121 121 121 121 121 122 123 121 124 125 121 126 121 127 128 121 129 130 121 121 121 121 121 121 131 132 133   0   0   0   0   0 0
	  0   0   0   0   0 134 135  68  68  68  68  68  68 136 137  68 138 139  68 140 141 142 143  68 144 145  68  68  68  68  68  68 146 147 148   0   0   0   0   0 0
	  0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0 0
	  0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0 0
	  0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0 0
	  0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0 0
	  0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0 0
	  0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0 0
	  0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0 0
	  0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0 0
	  0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0 0
	  0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0 0
	  0   0   0   0   0   0   0   0   0 149   0 150   0 151   0 152   0 152   0   0   0   0 152   0 149   0 153   0 154   0 151   0   0   0   0   0   0   0   0   0 0
	  0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0 0
	  0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0 0
	  0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0 0

:data prompt
	149 0 150 0 151 0 152 0 152 0 0 0 0 152 0 149 0 153 0 154 0 151

: title ( -- )
	title-board GP !
	title-tiles GT !
	255 for i hide next
	loop
		60 for tick keys if rdrop break then next
		9 27 tile-grid@ 22 0 fill
		60 for tick keys if rdrop break then next
		prompt 9 27 tile-grid@ 22 >move
	again
	loop tick keys while

	(
	loop
		keys key-a and if play-on then
		tick
	again
	)
;

######################################################
##
##  Codec Screen
##
######################################################

:array tgrid 2542 -1
:data  blank 0

: show-t  GP @ GS @ tgrid GP ! 0 GS ! ; ( -- GP GS )
: hide-t  GS ! GP !                   ; ( GP GS -- )

: save-grid ( stride base -- )
	29 for
		over 41 + i * over +
		0 i tile-grid@ swap
		40 >move
	next 2drop
;

: blinds-out ( -- )
	0 tgrid save-grid show-t
	40 for
		30 for
			blank @ grid-z or
			i 2 mod if j else 40 j - then i tile-grid@ !
		next
		tick
	next
	40 for tick next
	hide-t
;

: blinds-in ( -- )
	40 for
		30 for
			i 2 mod if j else 40 j - then i 2dup
			tile-grid@ @ >r 41 * + tgrid + r> swap !
		next
		show-t tick hide-t
	next
;


:image game-tiles "salad.png"  8  8
:image text       "text.png"   8  8
:image heads      "heads.png" 48 64

:data codec-board
	-1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 
	-1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 
	-1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 
	-1 -1 -1 -1 26 26 26 26 26 26 26 26 26 26 26 26 26 26 26 26 26 26 26 26 26 26 26 26 26 26 26 26 26 26 26 26 -1 -1 -1 -1 -1 
	-1 -1 -1 -1 26  9 10 10 10 10 10 10 10 10 14 10 10 10 10 10 10 10 10 10 10 14 10 10 10 10 10 10 10 10 11 26 -1 -1 -1 -1 -1 
	-1 -1 -1 -1 26 25 26 -1 -1 -1 -1 -1 -1 26 27 58 59 59 26 26 26 26 26 26 26 27 26 -1 -1 -1 -1 -1 -1 26 27 26 -1 -1 -1 -1 -1 
	-1 -1 -1 -1 26 25 26 -1 -1 -1 -1 -1 -1 26 27 37 39 39 39 39 39 39 39 39 39 27 26 -1 -1 -1 -1 -1 -1 26 27 26 -1 -1 -1 -1 -1 
	-1 -1 -1 -1 26 25 26 -1 -1 -1 -1 -1 -1 26 27 37 39 39 39 57 17 17 17 17 17 27 26 -1 -1 -1 -1 -1 -1 26 27 26 -1 -1 -1 -1 -1 
	-1 -1 -1 -1 26 25 26 -1 -1 -1 -1 -1 -1 26 27 37 39 39 55 17 17 17 17 17 17 27 26 -1 -1 -1 -1 -1 -1 26 27 26 -1 -1 -1 -1 -1 
	-1 -1 -1 -1 26 25 26 -1 -1 -1 -1 -1 -1 26 27 37 39 53 17 17 17 17 17 17 17 27 26 -1 -1 -1 -1 -1 -1 26 27 26 -1 -1 -1 -1 -1 
	-1 -1 -1 -1 26 25 26 -1 -1 -1 -1 -1 -1 26 27 37 39 17 17 17 17 17 17 17 17 27 26 -1 -1 -1 -1 -1 -1 26 27 26 -1 -1 -1 -1 -1 
	-1 -1 -1 -1 26 25 26 -1 -1 -1 -1 -1 -1 26 15 10 10 10 10 10 10 10 10 10 10 31 26 -1 -1 -1 -1 -1 -1 26 27 26 -1 -1 -1 -1 -1 
	-1 -1 -1 -1 26 25 26 -1 -1 -1 -1 -1 -1 26 27 44 45 46 47 26 26 60 61 62 63 27 26 -1 -1 -1 -1 -1 -1 26 27 26 -1 -1 -1 -1 -1 
	-1 -1 -1 -1 26 41 42 42 42 42 42 42 42 42 30 42 42 42 42 42 42 42 42 42 42 30 42 42 42 42 42 42 42 42 43 26 -1 -1 -1 -1 -1 
	-1 -1 -1 -1 26 26 26 26 26 26 26 26 26 26 26 26 26 26 26 26 26 26 26 26 26 26 26 26 26 26 26 26 26 26 26 26 -1 -1 -1 -1 -1 
	-1 -1 -1 -1 26 26 26 26 26 26 26 26 26 26 26 26 26 26 26 26 26 26 26 26 26 26 26 26 26 26 26 26 26 26 26 26 -1 -1 -1 -1 -1 
	-1 -1 -1 -1 26 12 10 10 10 10 10 10 10 10 10 10 10 10 10 10 10 10 10 10 10 10 10 10 10 10 10 10 10 10 13 26 -1 -1 -1 -1 -1 
	-1 -1 -1 -1 26 25 26 26 26 26 26 26 26 26 26 26 26 26 26 26 26 26 26 26 26 26 26 26 26 26 26 26 26 26 27 26 -1 -1 -1 -1 -1 
	-1 -1 -1 -1 26 25 26 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 26 27 26 -1 -1 -1 -1 -1 
	-1 -1 -1 -1 26 25 26 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 26 27 26 -1 -1 -1 -1 -1 
	-1 -1 -1 -1 26 25 26 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 26 27 26 -1 -1 -1 -1 -1 
	-1 -1 -1 -1 26 25 26 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 26 27 26 -1 -1 -1 -1 -1 
	-1 -1 -1 -1 26 25 26 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 26 27 26 -1 -1 -1 -1 -1 
	-1 -1 -1 -1 26 25 26 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 26 27 26 -1 -1 -1 -1 -1 
	-1 -1 -1 -1 26 25 26 26 26 26 26 26 26 26 26 26 26 26 26 26 26 26 26 26 26 26 26 26 26 26 26 26 26 26 27 26 -1 -1 -1 -1 -1 
	-1 -1 -1 -1 26 28 42 42 42 42 42 42 42 42 42 42 42 42 42 42 42 42 42 42 42 42 42 42 42 42 42 42 42 42 29 26 -1 -1 -1 -1 -1 
	-1 -1 -1 -1 26 26 26 26 26 26 26 26 26 26 26 26 26 26 26 26 26 26 26 26 26 26 26 26 26 26 26 26 26 26 26 26 -1 -1 -1 -1 -1 
	-1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 
	-1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 
	-1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 
	-1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 

:const snake   0
:const colonel 1
:const mei     2
:const left    0
:const right   1

: zipin ( face side -- )
	dup show
	over 6 + over tile!
	2 for tick next
	over 3 + over tile!
	5 for tick next
	tile!
;

: zipout ( face side -- )
	over 3 + over tile!
	5 for tick next
	over 6 + over tile!
	2 for tick next
	0 over tile!
	hide drop
;

#: flicker ( face -- )
#	dup tile 3 mod
#	RN @ 50 mod -if 3 + then
#	swap tile!
#;

: light ( face flag -- )
	if
		dup tile 3 mod 9 + swap tile!
	else
		dup tile 3 mod     swap tile!
	then
;

:var lf
:var rf
:var vol

: dialog-tick ( -- )
	left  lf @ light
	right rf @ light
	tick
;

: fleft   lf off rf on ;
: fright  rf off lf on ;

: clear-dialog ( -- )
	5 for
		7 18 i + tile-grid@ 26 blank @ fill
	next
;

: dialog[ ( -- marker )
	clear-dialog 0
;

:array  msg-buffer 7 0

: [dialog-begin]
	5 msg-buffer + loop
		over -if break then
		swap over ! 1 -
	again
	swap drop 1 +
	
	18 loop
		over @ over 7 swap tile-grid@
		loop
			over @ ascii @ + over !
			keys key-a and -if dialog-tick dialog-tick play-hum then
			1 + swap 1 + swap over @
		while 2drop
		20 for dialog-tick next
		1 + swap 1 + swap over @
	while 2drop
	loop dialog-tick keys key-a and while
;

: [dialog-end]
	loop dialog-tick keys key-a and while
	clear-dialog
;

: ]-text ( msg* -- )
	[dialog-begin]
	loop dialog-tick keys key-a and until
	[dialog-end]
;

: ]-ask ( msg* -- flag )
	[dialog-begin]
	true loop
		keys key-lf key-rt or and if
			not 10 for dialog-tick next
		then
		dup if 11 23 "> YES <      NO  "
		else   11 23 "  YES      > NO <" then
		grid-type dialog-tick keys key-a and
	until
	[dialog-end]
;

: setup-codec ( -- )
	play-on
	codec-board GP !
	game-tiles  GT !
	heads       ST !
	32 ascii !
	17 blank !

	255 for i hide next
	48x64 invisible 0  56 40 0 >sprite
	48x64 invisible 0 216 40 1 >sprite
	blinds-in

	4 for
		9 for
			i 15 + j 6 +
			tile-grid@ dup @
			dup 36 >= over 39 <= and swap
			dup 52 >= swap 57 <= and or
			if dup @ 1 xor swap ! else drop then
		next
		6 for tick next
	next
	19 9 "140.85" grid-type
;

: leave-codec ( -- )
	play-on
	left  tile 3 mod left  zipout
	right tile 3 mod right zipout

	4 for
		9 for
			i 15 + 4 j - 6 +
			tile-grid@ dup @
			dup 36 >= over 39 <= and swap
			dup 52 >= swap 57 <= and or
			if dup @ 1 xor swap ! else drop then
		next
		6 for tick next
	next
	19 9 "      " grid-type

	60 for tick next
	blinds-out
;

:data played-before false

: long-intro
	colonel left zipin
	fleft
	dialog[ "Snake?" ]-text
	snake   right zipin
	fright
	dialog[ "Colonel." ]-text
	fleft
	dialog[
		"That's right, Snake."
		"Are you prepared for your"
		"mission?"
	]-text
	fright
	dialog[ "Mission?" ]-text
	fleft
	dialog[
		"This is going to be a"
		"snacking mission."
		"Pork rinds, bacon-"
	]-text
	colonel left zipout
	mei     left zipin
	fleft
	dialog[
		"Colonel?"
		"Have you forgotten about"
		"your diet?"
	]-text
	fright
	dialog[ "Mei Ling..." ]-text
	mei     left zipout
	colonel left zipin
	fleft
	dialog[ "Er, I mean..." ]-text
	dialog[
		"Salad."
		" "
		"...But it has to be mixed"
		"just right."
	]-text
	dialog[
		"All ingredients will be"
		"procure-on-site."
	]-text
	dialog[ "Oh, and Snake-" ]-text
	fright
	dialog[ "Colonel?"       ]-text
	fleft
	dialog[
		"You'll need to use..."
		"Metal Gear."
	]-text
	fright
	dialog[ "Metal Gear...?" ]-text
	fleft
	dialog[
		"That's right, Snake-"
		"Metal Gear is the key to"
		"your snacking mission."
	]-text
	dialog[
		"Good luck, Snake!"
	]-text
	played-before on
;

: short-intro ( -- )
	colonel left zipin
	fleft
	dialog[
		"Snake..."
		"You know what to do!"
	]-text
;

: intro	( -- )
	setup-codec

	played-before @ if
		short-intro
	else
		long-intro
	then

	leave-codec
;

:ref score
: game-over ( -- )
	setup-codec
	mei left zipin
	dialog[
		"Great job, Snake!"
		"Here's your final score:"
		24 22 score @ draw-number
	]-text
	leave-codec
;

######################################################
##
##  Game State
##
######################################################

:image game-sprites "toppings.png" 16 16

:data  game-board
	 -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1 
	 -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1 
	 -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1 
	 -1  -1  -1  -1  -1  -1  -1  -1 1073741841 1073741841 1073741841 1073741841 1073741841 1073741841 1073741841 1073741841 1073741841 1073741841 1073741841 1073741841 1073741841 1073741841 1073741841 1073741841 1073741841 1073741841 1073741841 1073741841 1073741841 1073741841 1073741841 1073741841  -1  -1  -1  -1  -1  -1  -1  -1  -1 
	 -1  -1  -1  -1  -1  -1  -1  -1 1073741841 1073741824 1073741825 1073741825 1073741830 1073741825 1073741825 1073741825 1073741829 1073741825 1073741825 1073741825 1073741825 1073741830 1073741825 1073741825 1073741825 1073741825 1073741825 1073741825 1073741825 1073741829 1073741826 1073741841  -1  -1  -1  -1  -1  -1  -1  -1  -1 
	 -1  -1  -1  -1  -1  -1  -1  -1 1073741841 1073741840   3   4   3   4   3   4   3   3   4   3   3   4   3   4   3   4   4   4   3   4 1073741842 1073741841  -1  -1  -1  -1  -1  -1  -1  -1  -1 
	 -1  -1  -1  -1  -1  -1  -1  -1 1073741841 1073741840  19   3  19  20  19  20  19  19  20  19  19  20  19  20  19  20  20  20  19  20 1073741848 1073741841  -1  -1  -1  -1  -1  -1  -1  -1  -1 
	 -1  -1  -1  -1  -1  -1  -1  -1 1073741841 1073741840   3   4   3   4   3   4  19  20  19  20   3   4  19  20   3  20   3   4   3   4 1073741848 1073741841  -1  -1  -1  -1  -1  -1  -1  -1  -1 
	 -1  -1  -1  -1  -1  -1  -1  -1 1073741841 1073741840  19  20  19  20  19  20   3   3   4  20  19   3   4   3   4   3  19  20   4   4 1073741842 1073741841  -1  -1  -1  -1  -1  -1  -1  -1  -1 
	 -1  -1  -1  -1  -1  -1  -1  -1 1073741841 1073741831  19  20   4  20  19   3   4  19  20   3   4  19  20  19  20   3   4  19  20  20 1073741842 1073741841  -1  -1  -1  -1  -1  -1  -1  -1  -1 
	 -1  -1  -1  -1  -1  -1  -1  -1 1073741841 1073741840   3   4   3   4   4  19  20   3   3  19  20   3   4  19   3  19  20  19   3   4 1073741842 1073741841  -1  -1  -1  -1  -1  -1  -1  -1  -1 
	 -1  -1  -1  -1  -1  -1  -1  -1 1073741841 1073741840  19  20  19  20   3   4   3   4   3  19  20   3   4   4  19   4   4   4  19  20 1073741842 1073741841  -1  -1  -1  -1  -1  -1  -1  -1  -1 
	 -1  -1  -1  -1  -1  -1  -1  -1 1073741841 1073741847   3   4  19  20  19   3   4  20  19  20  19  19  20   3   3   4   3   3   4   4 1073741842 1073741841  -1  -1  -1  -1  -1  -1  -1  -1  -1 
	 -1  -1  -1  -1  -1  -1  -1  -1 1073741841 1073741840  19  20   4   3   3  19   3   4   4   3   4  19  20  19  19  20  19   3   4  20 1073741842 1073741841  -1  -1  -1  -1  -1  -1  -1  -1  -1 
	 -1  -1  -1  -1  -1  -1  -1  -1 1073741841 1073741840   3   4  20   3   4  20   3   4  20  19  20  19  20   4  19  20   3   4  20   4 1073741842 1073741841  -1  -1  -1  -1  -1  -1  -1  -1  -1 
	 -1  -1  -1  -1  -1  -1  -1  -1 1073741841 1073741840   3   4   4  19  20  19  19  20   3   4  19   3   3   4  19  20   3   4  19  20 1073741842 1073741841  -1  -1  -1  -1  -1  -1  -1  -1  -1 
	 -1  -1  -1  -1  -1  -1  -1  -1 1073741841 1073741840  19  20   3   4   4   3   4   4  19  20   3  19  19  20   3   4  19  20   3   4 1073741842 1073741841  -1  -1  -1  -1  -1  -1  -1  -1  -1 
	 -1  -1  -1  -1  -1  -1  -1  -1 1073741841 1073741840  19  20   3   4   4  19  20  20  19  20   3   4   3   4   4   3   4  20  19  20 1073741832 1073741841  -1  -1  -1  -1  -1  -1  -1  -1  -1 
	 -1  -1  -1  -1  -1  -1  -1  -1 1073741841 1073741840  19  20  19  20   3   4   3   4   4  20   3   4  19  20  20  19  20   4  19  20 1073741842 1073741841  -1  -1  -1  -1  -1  -1  -1  -1  -1 
	 -1  -1  -1  -1  -1  -1  -1  -1 1073741841 1073741840  19  20   3   4   3   4  19  20  20  20  19  20  19  20   3  20  19  20   3   4 1073741842 1073741841  -1  -1  -1  -1  -1  -1  -1  -1  -1 
	 -1  -1  -1  -1  -1  -1  -1  -1 1073741841 1073741840  19  20   3   4  19   3   3   4   3   3   4  20   3   4   3  20   3   4  19  20 1073741848 1073741841  -1  -1  -1  -1  -1  -1  -1  -1  -1 
	 -1  -1  -1  -1  -1  -1  -1  -1 1073741841 1073741840   3   4  19  20   3   4  19  20  19  19  20   4  19  20   4   4  19  20   4  20 1073741842 1073741841  -1  -1  -1  -1  -1  -1  -1  -1  -1 
	 -1  -1  -1  -1  -1  -1  -1  -1 1073741841 1073741831  19   3   4   4  19  20  20   3   4  20   3   3  19  20   3   3   4  20   3   4 1073741842 1073741841  -1  -1  -1  -1  -1  -1  -1  -1  -1 
	 -1  -1  -1  -1  -1  -1  -1  -1 1073741841 1073741840   3  19  20  20  20   3   4  19  20   3   4   3   4   3  19   3   4   3  19  20 1073741842 1073741841  -1  -1  -1  -1  -1  -1  -1  -1  -1 
	 -1  -1  -1  -1  -1  -1  -1  -1 1073741841 1073741840  19  19  20  19  20  19  20  19  20  19  20  19  20  19  19  19  20  19  19  20 1073741842 1073741841  -1  -1  -1  -1  -1  -1  -1  -1  -1 
	 -1  -1  -1  -1  -1  -1  -1  -1 1073741841 1073741856 1073741857 1073741857 1073741857 1073741845 1073741857 1073741857 1073741857 1073741846 1073741857 1073741857 1073741857 1073741857 1073741857 1073741857 1073741857 1073741857 1073741845 1073741857 1073741857 1073741857 1073741858 1073741841  -1  -1  -1  -1  -1  -1  -1  -1  -1 
	 -1  -1  -1  -1  -1  -1  -1  -1 1073741841 1073741872 1073741873 1073741873 1073741873 1073741873 1073741873 1073741873 1073741873 1073741873 1073741873 1073741873 1073741873 1073741873 1073741873 1073741873 1073741873 1073741873 1073741873 1073741873 1073741873 1073741873 1073741874 1073741841  -1  -1  -1  -1  -1  -1  -1  -1  -1 
	 -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1 
	 -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1 
	 -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1 
	 -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1 

:const gear         0
:const spin        13 
:const top-base     1
:const icon-base   14
:const cursor-base 26
:const toppings     0 # board tile sprites
:const fake       101 # fake sprite for slides
:const cursors    102 # cursor outline sprites
:const icons      112 # board tile icons

:var   score
:var   time
:var   cursor
:var   direction

: random-salad ( -- id )
	RN @ 13 mod
;

: double? ( id -- flag )
	1 - 2 mod if true else false then
;

: icon? ( id -- flag )
	icon-base >=
;

: valid? ( id -- flag )
	dup 0 > swap 13 < and
;

: match? ( a b -- flag )
	over gear = over gear = or if 2drop false exit then
	top-base - 2 / swap top-base - 2 / =
;

: tile>pixel ( x y -- x' y' )
	16 * 40 + swap
	16 * 80 + swap
;

: topping ( x y -- sprite )
	10 * + toppings +
;

: salad ( x y -- addr )
	topping sprite@ .sprite-t
;

######################################################
##
##  Board Manipulation
##
######################################################

: show-board ( -- )
	-1 -1 tile>pixel fake pos!
	99 for
		i 10 mod i 10 / tile>pixel i toppings + pos!
		i toppings + show
	next
;

: slide-horiz ( n -- )
	15 for
		9 for dup i cursor @ topping +px next
		dup fake +px
		tick
	next
	drop
;

: slide-vert ( n -- )
	15 for
		9 for dup cursor @ i topping +py next
		dup fake +py
		tick
	next
	drop
;

: roll-rt ( -- )
	play-big
	-1 cursor @ tile>pixel fake pos!
	 9 cursor @ salad @    fake tile!
	1 slide-horiz
	9 cursor @ salad @
	8 for
		i     cursor @ salad @
		i 1 + cursor @ salad !
	next
	0 cursor @ salad !
	show-board
;

: roll-lf ( -- )
	play-big
	10 cursor @ tile>pixel fake pos!
	 0 cursor @ salad @    fake tile!
	-1 slide-horiz
	0 cursor @ salad @
	8 for
		9 i - cursor @ salad @
		8 i - cursor @ salad !
	next
	9 cursor @ salad !
	show-board
;

: roll-dn ( -- )
	play-big
	cursor @ -1 tile>pixel fake pos!
	cursor @  9 salad @    fake tile!
	1 slide-vert
	cursor @ 9 salad @
	8 for
		cursor @ i     salad @
		cursor @ i 1 + salad !
	next
	cursor @ 0 salad !
	show-board
;

: roll-up ( -- )
	play-big
	cursor @ 10 tile>pixel fake pos!
	cursor @  0 salad @    fake tile!
	-1 slide-vert
	cursor @ 0 salad @
	8 for
		cursor @ 9 i - salad @
		cursor @ 8 i - salad !
	next
	cursor @ 9 salad !
	show-board
;

######################################################
##
##  Cursor Manipulation
##
######################################################

: show-cursor ( -- )
	direction @ if
		9 for
			16x16
			cursor-base 3 +
			i 0 > if 1 + then
			i 9 = if 1 + then
			cursor @ i tile>pixel
			cursors i + >sprite
		next
	else
		9 for
			16x16
			cursor-base
			i 0 > if 1 + then
			i 9 = if 1 + then
			i cursor @ tile>pixel
			cursors i + >sprite
		next
	then
;

: cursor+ ( -- )
	play-small
	cursor @ + 10 mod cursor !
	show-cursor
	7 for tick next
;

: selected ( index -- tile-addr )
	direction @ if
		10 * cursor @ +
	else
		cursor @ 10 * +
	then
	toppings + sprite@ .sprite-t
;

: actually-swap ( index tile-addr -- )
	spin over !
	play-spin
	9 for tick next
	random-salad swap !
	direction @ not direction !
	cursor ! show-cursor
	9 for tick next
;

: swap-dir ( -- )
	9 for
		9 i - selected
		dup @ gear = if
			9 r> - swap actually-swap exit
		else
			drop play-nope
		then
	next
;

######################################################
##
##  Board Clearing Logic
##
######################################################

:array visited 100 false
: clear-visited  99 for i visited + off next ; ( -- )

: here ( index -- addr )
	toppings + sprite@ .sprite-t
;

: ico ( index -- addr )
	icons    + sprite@ .sprite-t
;

: region-size ( type loc -- size )
	dup here @ valid?  -if 2drop 0 exit then # non-salad tile
	dup visited + @     if 2drop 0 exit then # visited tile
	2dup here @ match? -if 2drop 0 exit then # non-matching tile
	dup visited + on
	1 >r
	dup         9 > if 2dup 10 - region-size r> + >r then # up
	dup        90 < if 2dup 10 + region-size r> + >r then # down
	dup 10 mod  0 > if 2dup  1 - region-size r> + >r then # left
	dup 10 mod  9 < if 2dup  1 + region-size r> + >r then # right
	2drop r>
;

: region-clear ( type loc -- )
	dup visited + @     if 2drop exit then # visited tile
	2dup here @ match? -if 2drop exit then # non-matching tile

	dup visited + on
	dup here @ 1 - icon-base + over icons + tile!
	
	dup         9 > if 2dup 10 - region-clear then # up
	dup        90 < if 2dup 10 + region-clear then # down
	dup 10 mod  0 > if 2dup  1 - region-clear then # left
	dup 10 mod  9 < if 2dup  1 + region-clear then # right
	2drop
;

: find-regions ( -- flag )
	false
	99 for
		clear-visited

		i dup here @ swap region-size 3 > if
			play-buzz
			clear-visited
			i dup here @ swap region-clear
			true or
		then
	next
;

:proto hud
: total-points ( -- )
	99 for
		i ico @ 32 = -if
			10 i here @ double? if 2 * then dup
			score @ + score !
			time  @ + time  !
			32 i ico  !
			32 i here !
		then
	next
	hud
;

:var row
:var index
: shift-in ( row index -- )
	play-hum
	index ! row !
	10 row @ tile>pixel fake pos!
	random-salad        fake tile!
	
	15 for
		9 for
			i index @ > if -1 i row @ topping +px then
		next
		-1 fake +px
		tick
	next

	8 for
		9 i - index @ > if
			9 i - row @ salad @
			8 i - row @ salad !
		then
	next

	fake tile 9 row @ salad !
	show-board
;

: insert-salad ( -- )
	99 for
		i here @ 32 = if
			i 10 / i 10 mod shift-in
		then
	next
;

: clear ( -- )
	loop
		find-regions -if break then
		19 for tick next
		total-points
		19 for tick next
		insert-salad
	again
;

######################################################
##
##  High-Level Game Logic
##
######################################################

: setup ( -- )
	game-board   GP !
	game-tiles   GT !
	game-sprites ST !

	99 for
		16x16 random-salad
		i 10 mod i 10 / tile>pixel
		i toppings + >sprite

		16x16 32
		i 10 mod i 10 / tile>pixel
		i icons + >sprite
	next
	show-cursor

	16x16 0 -1 -1 tile>pixel fake >sprite

	  32 ascii !
	   0 score !
	3600 time  !
	#60 time !
;

: hud ( -- )
	 2 1 " TIME"      grid-type
	 1 2 "00:00:00"   grid-type
	 2 2 time @ 3600 /          draw-number
	 5 2 time @ 3600 mod 60 /   draw-number
	 8 2 time @          60 mod draw-number

	32 1 "SCORE"      grid-type	
	30 2 "          " grid-type
	37 2 score @     draw-number
;

: cursor ( -- )
	keys key-rt and if direction @  if  1 cursor+ else roll-rt clear then then
	keys key-lf and if direction @  if -1 cursor+ else roll-lf clear then then
	keys key-dn and if direction @ -if  1 cursor+ else roll-dn clear then then
	keys key-up and if direction @ -if -1 cursor+ else roll-up clear then then
	keys key-a  and if swap-dir clear then
;

: time? ( -- flag )
	tick
	time dec
	time @
;

: game ( -- )
	setup
	59 for tick next
	hud clear
	loop
		hud
		cursor
		time?
	while
;

: main ( -- )
	loop
		title
		intro
		game
		game-over
	again
;