######################################################
##
##  Forth Warrior 2:
##
##  An educational game about programming in Forth,
##  solving puzzles and poking things with swords.
##
##  John Earnest
##
######################################################

# known things that can crash/lock up the system:
# - a bad value on the rstack during return
# - overwriting chunks of the core dictionary

:include <Print.fs>
:include <String.fs>
:include <Parse.fs>

:const stack-size    200
:array data-stack    stack-size 0
:array data-padding          10 0
:array return-stack  stack-size 0
:array return-padding        10 0

:array      grid    1271 0
:array      sprites 1024 0
:array game-grid    1271 0
:array game-sprites 1024 0

:proto console-emit
: terminal ( -- )
	' console-emit ' emit revector
	grid    GP !
	sprites SP !
;

:var restart-vector

:ref par :ref pptr
:ref ret :ref rptr
:ref brk :ref bptr
:ref mode

: abort ( str -- )
	terminal
	typeln

	par pptr !
	ret rptr !
	brk bptr !
	0   mode !

	data-stack   DP !
	return-stack RP !
	restart-vector @ exec
	halt
;

: kbrk ( -- )
	KB @ 3 = if "BREAK" abort then
;

: min  2dup > if swap then drop ; ( a b -- min )
: max  2dup < if swap then drop ; ( a b -- max )

:include "MakoForth.fs"


######################################################
##
##  Status Line:
##
######################################################

:include <Grid.fs>

:array emit-buff 80 0
:var   emit-ptr

: emit, emit-ptr @ ! emit-ptr inc ; ( n -- )

: game-cr ( -- )
	drop 0 emit,
	
	# fill with clear color
	0 29 tile-grid@ 40 0 fill
	0 29 emit-buff grid-type
	emit-buff emit-ptr !
	
	# wait for key press?
	59 for sync next

	0 29 tile-grid@ 40 196 fill
;

: game-emit ( char -- )
	dup 10 = if
		drop game-cr
	else
		dup 0 < over 128 > or if drop 94 then
		emit,
		emit-ptr @ emit-buff - 39 > if game-cr then
	then
;

: game ( -- )
	emit-buff emit-ptr !
	' game-emit ' emit revector
	game-grid    GP !
	game-sprites SP !
;

######################################################
##
##  Game State and Resources:
##
######################################################

:include <Sprites.fs>
:const player-sprite 200
:const verb-sprite   201

:image sprite-tiles "liz.png"   16 16
:image grid-tiles   "text.png"   8  8
:image floor-tiles  "floor.png"  8  8

:const N 0
:const E 1
:const S 2
:const W 3

:var single  # test on just one level?
:var actions
:var kills
:var health
:var level
:var gems
:var gkeys

:include "Levels.fs"

: init-game ( level -- )
	dup level-count >= if "invalid level!" abort then
	  level   !
	0 actions !
	0 kills   !
	5 health  !
	0 gems    !
	0 gkeys   !
;

######################################################
##
##  Entities:
##
######################################################

:const max-ent 16

: valid? ( id -- )
	sprite@ @
;

: ent-free ( id -- )
	0 swap sprite@ !
;

: ent-alloc ( -- id )
	0 loop
		dup valid? -if exit then
		1 + dup max-ent >
	until drop
	"Entity limit exceeded!" abort
;

: ent-for ( proc -- )
	0 loop
		dup valid? if
			2dup >r >r swap exec r> r>
		then
		1 + dup max-ent >
	until 2drop
;

######################################################
##
##  Board Management:
##
######################################################

:array board-buff  20 0
:array board      300 0

:data rel    -20 1 20 -1
: relative   rel + @ +          ; ( index dir -- index' )
: board-pos  20 * + board +     ; ( x y -- index )
: boardtx    board - 20 mod 2 * ; ( index -- tilex )
: boardty    board - 20 /   2 * ; ( index -- tiley )
: boardx     boardtx 8 *        ; ( index -- px )
: boardy     boardty 8 *        ; ( index -- py )

: ent-pos ( id -- index )
	dup  px 16 /
	swap py 16 /
	board-pos
;

: set-pos ( pos id -- )
	>r
	dup boardx i px!
	    boardy i py!
	rdrop
;

: player-pos ( -- index )
	player-sprite ent-pos
;

: ent-at ( index -- id? flag )
	-1 swap
	{
		>r dup i ent-pos = if
			swap drop i swap
		then
		rdrop
	} ent-for drop
	dup -1 = if drop false else true then
;

: slime?  tile dup 10 = swap 11 = or ; ( id -- flag )
: key?    tile 15 =                  ; ( id -- flag )
: gem?    tile 16 =                  ; ( id -- flag )

: at-pos ( index -- kind )
	dup ent-at if
		dup slime? if drop SLIME exit then
		dup key?   if drop KEY   exit then
		dup gem?   if drop GEM   exit then
		drop
	then
	@
;

: verb! ( tile index -- )
	>r 16x16 swap
	i  boardx
	r> boardy
	verb-sprite >sprite
;

: draw-block ( a b c d index -- )
	>r
	288 + i boardtx 1 + i boardty 1 + tile-grid@ !
	288 + i boardtx     i boardty 1 + tile-grid@ !
	288 + i boardtx 1 + i boardty     tile-grid@ !
	288 + i boardtx     i boardty     tile-grid@ !
	rdrop
;

: paint-wall ( index -- )
	dup N relative @ WALL = if
		dup N relative >r
		RN @ 4 mod
		RN @ 4 mod
		RN @ 4 mod 16 +
		RN @ 4 mod 16 +
		r> draw-block
	then
	drop
;

: draw-tile ( index -- )
	# handle entity spawners:
	dup @ KEY = if
		>r
		16x16 15 i boardx i boardy ent-alloc >sprite
		r>
		FLOOR over !
	then
	dup @ GEM = if
		>r
		16x16 16 i boardx i boardy ent-alloc >sprite
		r>
		FLOOR over !
	then
	dup @ SLIME = if
		>r
		16x16 10 RN @ 2 mod + i boardx i boardy ent-alloc >sprite
		r>
		FLOOR over !
	then
	dup @ START = if
		>r
		16x16 0
		i boardx
		i boardy
		player-sprite >sprite
		r>
		FLOOR over !
	then

	# handle static geometry:
	dup @ WALL = if
		>r 4 4 4 4 r> draw-block
		exit
	then
	dup @ STAIRS = if
		dup paint-wall
		>r 12 13 28 29 r> draw-block
		exit
	then
	dup @ FLOOR = if
		dup paint-wall
		>r
		3 for
			RN @ 4 mod 8 +
			RN @ 2 mod 16 * +
		next
		r> draw-block
		exit
	then
	dup @ DOOR = if
		>r
		 0  1 22 23 i N relative draw-block
		14 15 30 31 r>           draw-block
		exit
	then
	"invalid level data!" abort
;

######################################################
##
##  Core Game Logic:
##
######################################################

: draw-hud ( -- )
	ascii @	dup 96 + 96 + ascii !
	1 1 "LEVEL  0" grid-type
	8 1 level @ draw-number
	ascii !
	4 for
		4 i - health @ < if 7 else 6 then 288 +
		38 i - 1 tile-grid@ !
	next
	10 for
		i gkeys @ < if 38 else 4 then
		288 + 32 i - 1 tile-grid@ !
	next
;

: copy-level ( -- )
	' ent-free ent-for
	level @ levels + @ board 300 >move
	299 for
		299 i - board + draw-tile
	next
	draw-hud
;

: game-over ( -- )
	terminal
	"level   : " type level   @ . cr
	"actions : " type actions @ . cr
	"gems    : " type gems    @ . cr
	"kills   : " type kills   @ . cr
	cr
	"complete." abort
;

: move-or-attack ( id -- )
	3 for
		dup ent-pos i relative player-pos = if
			12 player-pos verb!
			4 player-sprite tile!
			20 for sync next
			health dec draw-hud
			verb-sprite hide
			health @ 1 < if
				player-sprite hide
				60 for sync next
				"Melted by a slime!" abort
			then
			20 for sync next
			0 player-sprite tile!
			drop exit
		then
	next
	dup ent-pos RN @ 4 mod relative
	dup at-pos FLOOR = if
		swap set-pos
	else
		2drop
	then
;

: game-step ( -- )
	draw-hud actions inc

	{
		dup slime? if
			>r
			i tile 10 = if
				i move-or-attack
				11 i tile!
			else
				10 i tile!
			then
			r>
		then drop
	} ent-for

	player-pos at-pos STAIRS = if
		20 for sync next
		single @ if game-over then
		level inc
		level @ level-count >= if game-over then
		copy-level
	then

	10 for sync next
;

: start-game ( level -- )
	name> dict-find swap
	game
	init-game
	copy-level
	59 for sync next
	interpret
	"Program ended." abort
;

: init-game-vocab ( -- )
	": var   create 0 , does>   ;" run
	": const create   , does> @ ;" run
	": = xor if 0 else -1 then  ;" run
	": exec >r                  ;" run
	"0 const N" run
	"1 const E" run
	"2 const S" run
	"3 const W" run

	"0 const FLOOR"  run
	"1 const WALL"   run
	"2 const STAIRS" run
	"3 const DOOR"   run
	"4 const KEY"    run
	"5 const GEM"    run
	"6 const SLIME"  run
	
	{ health @ PUSH finish } "health" primitive ( -- n )
	{ level  @ PUSH finish } "level"  primitive ( -- n )
	{ gems   @ PUSH finish } "gems"   primitive ( -- n )
	{ keys   @ PUSH finish } "keys"   primitive ( -- n )
	{ game-step     finish } "wait"   primitive ( -- )

	{
		3 player-sprite tile!
		10 for sync next
		0 {
			dup slime? if
				9 over ent-pos N relative verb!
				20 for sync next
				swap 1 + swap
			then drop
		} ent-for PUSH
		0 player-sprite tile!
		verb-sprite hide
		20 for sync next
		finish
	} "listen" primitive ( -- n )

	{
		POP >r
		player-pos i relative at-pos
		dup SLIME = if
			player-pos r> relative
			dup boardx player-sprite px!
		    	boardy player-sprite py!
			20 for sync next
			4 player-sprite tile!
			20 for sync next
			12 player-sprite tile!
			60 for sync next
			player-sprite hide
			60 for sync next
			"Stepped on a slime and dissolved!" abort
		then
		dup FLOOR = swap STAIRS = or -if
			4 player-sprite tile!
			30 for sync next
			0 player-sprite tile!
			30 for sync next
			terminal
			"Unable to walk " type
			i N = if "north" type then
			i E = if "east"  type then
			i S = if "south" type then
			i W = if "west"  type then
			"!" abort
		then
		player-pos r> relative
		dup boardx player-sprite px!
		    boardy player-sprite py!
		20 for sync next
		game-step finish
	} "walk" primitive ( dir -- )

	{
		POP >r
		3 player-sprite tile!
		5 player-pos i relative verb!
		20 for sync next
		verb-sprite hide
		player-pos i relative ent-at if
			dup slime? if
				12 over tile!
				20 for sync next
				dup ent-free
				kills inc
			then drop
		then
		r> drop
		0 player-sprite tile!
		game-step finish
	} "attack" primitive ( dir -- )

	{
		POP >r
		player-pos i relative at-pos
		dup KEY = swap GEM = or -if
			"Nothing to pick up!" abort
		then

		player-pos r> relative ent-at drop
		dup key? if
			gkeys @ 1 + 10 min gkeys !
		else
			health @ 1 + 5 min health !
			gems inc
		then

		16x16 over tile
		player-sprite px
		player-sprite py 14 -
		verb-sprite >sprite
		ent-free
		1 player-sprite tile!
		3 for
			5 for sync next
			-1 verb-sprite +py
		next
		10 for sync next
		0 player-sprite tile!
		verb-sprite hide

		game-step finish
	} "take" primitive ( dir -- )

	{
		POP >r
		player-pos i relative at-pos DOOR = -if
			"No door to open!" abort
		then
		gkeys @ 1 < if
			"Can't open a door without a key!" abort
		then
		gkeys dec

		3 player-sprite tile!
		7 player-pos i relative verb!
		30 for sync next

		0 player-sprite tile!
		verb-sprite hide
		FLOOR       player-pos i relative !
		11 11 11 11 player-pos i relative draw-block
		 0  1 20 20 player-pos i relative N relative draw-block
		rdrop

		game-step finish
	} "open" primitive ( dir -- )

	{
		POP >r
		2 player-sprite tile!
		6 player-pos i relative verb!
		20 for sync next

		0 player-sprite tile!
		verb-sprite hide
		10 for sync next

		player-pos r> relative at-pos PUSH finish
	} "look" primitive ( dir -- kind )

	{ false single ! 0   start-game finish } "begin"  primitive ( -- )
	{ true  single ! POP start-game finish } "test"   primitive ( level -- )
;

######################################################
##
##  Terminal and Editor:
##
######################################################

:const cursor-s 129
:var   used
:var   cursor
:var   cx
:var   lines
:data  cc -32
:array input 41 0

: plain-text -32 cc ! ;
: code-text   64 cc ! ;

: console-newline
	lines inc
	lines @ 28 > if
		0 lines !
		64 ascii !
		0 29 "[More...]" grid-type
		loop keys key-a and kbrk sync until
		loop keys key-a and kbrk sync while
		loop KB @ -1 = until
		0 29 "         " grid-type
	then
	0 cx !
	27 for
		0 28 i - tile-grid@
		0 27 i - tile-grid@
		40 >move
	next
	0 28 tile-grid@ 40 0 fill
	sync
;

: console-emit ( char -- )
	dup 10 = if
		console-newline
		drop exit
	else
		# capture ludicrous characters
		# and turn 'em into happy little boxes.
		dup 0 < over 128 > or if drop 94 then
		cc @ + cx @ 28 tile-grid@ !
		cx inc cx @ 39 >
		if console-newline then
	then
;

: input-clear ( -- )
	input 41 0 fill
	0 cursor !
	0 used   !
;

: refresh-grid ( -- )
	28 for
		39 for
			i j tile-grid@ dup @
			grid-z not and 27 j - lines @ <= if
				grid-z or
			then
			swap !
		next
	next
	39 for
		i used @ >= if 0 else i input + @ 32 - then
		96 + i 29 tile-grid@ !
	next
	8x8
	grid-tiles sprite-tiles - 64 / 191 +
	cursor @ 8 * 232 cursor-s >sprite
;

: open-def ( str -- flag )
	>read trim ": " starts? -if false exit then
	loop
		eof? if true exit then
		# ignore comments and strings?
		";" match? if false exit then
		skip
	again
;

:proto edit
:array glue-buffer 80 0
: splice-and-edit ( str -- str' )
	dup glue-buffer over size >move
	size glue-buffer +
	10    over ! 1 +
	10    over ! 1 +
	";" @ over ! 1 +
	0     swap !
	glue-buffer
	edit
;

: readline ( string? flag -- str )
	input-clear
	if
		dup size used !
		input over size >move
	then
	cursor-s show
	loop
		loop
			KB @ dup -1 = if drop break then
			dup 3 = if
				drop
				cursor-s hide
				input-clear
				"BREAK" abort
			then
			dup 10 = if
				# return
				drop input				
				mode @ -if cr then
				dup open-def if splice-and-edit then
				code-text dup typeln plain-text
				0 lines !
				cursor-s hide
				exit
			else
				dup 8 = if
					drop
					# backspace
					cursor @ 0 > if
						cursor @ input + dup 1 -
						used @ cursor @ - 1 + <move
						cursor dec@ used dec@
					then
				else
					# insert
					used @ 40 >= if drop else
						cursor @ input + dup dup 1 +
						used @ cursor @ - >move !
						cursor inc@ used inc@
					then
				then
			then
		again

		keys key-rt and if cursor @ 1 + used @ min cursor ! sync sync then
		keys key-lf and if cursor @ 1 -      0 max cursor ! sync sync then
		refresh-grid
		sync sync
	again
;

:include "Editor.fs"

######################################################
##
##  Main
##
######################################################

: parse  loop eof? if break then token again ; ( -- )

: repl ( -- )
	refresh-grid
	loop false readline >read trim parse again
;

: main ( -- )
	init-dictionary
	init-game-vocab

	terminal
	' abort ' fail revector
	' repl restart-vector !

	#": foo 0 loop dup attack 1 + 4 mod again ;"   run
	#": foo 0 loop dup look drop 1 + 4 mod again ;"   run
	#": foo loop E walk again ; " run

	": example
		loop
			E look
			dup DOOR = if
				E open
				N attack
				S attack
				W attack
				E attack
				listen . cr
			then
			dup SLIME = if E attack then
			dup GEM = swap KEY = or if
				E take
			then
			E walk
		again
	;" run

	1 1 "Forth Warrior v0.1" grid-type
	1 2 "MakoForth BIOS OK"  grid-type
	1 4 "(Type words for available commands.)" grid-type
	1 5 "(Type begin example to play.)"        grid-type

	code-text cc @ ascii !
	7 4 "words"         grid-type
	7 5 "begin example" grid-type
	plain-text cc @ ascii !

	repl
;