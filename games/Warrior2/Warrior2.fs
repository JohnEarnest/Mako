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
	0 emit,
	
	# fill with clear color
	0 29 tile-grid@ 40 0 fill
	0 29 emit-buff grid-type
	emit-buff emit-ptr !
	
	# wait for key press?
	89 for sync next

	0 29 tile-grid@ 40 192 fill
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

: check-dir ( dir -- dir )
	dup 0 < over 4 > or if
		terminal
		. "invalid direction!" abort
	then
	dup E = if player-sprite face-left  then
	dup W = if player-sprite face-right then
;

:var single  # test on just one level?
:var fast    # display animations in 'fast mode'?
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
	dup @ SPIKES = if
		dup paint-wall
		>r 32 33 48 49 r> draw-block
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
	dup @ START = if
		>r
		16x16 0
		i boardx
		i boardy
		player-sprite >sprite
		 0  0 50 51 i N relative draw-block
		36 37 52 53 i            draw-block
		FLOOR r> !
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
	1 28 level @ level-names + @ grid-type

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

: delay ( -- )
	fast @ if 5 else 20 then
	for sync next
;

: move-or-attack ( id -- )
	3 for
		dup ent-pos i relative player-pos = if
			12 player-pos verb!
			4 player-sprite tile!
			delay
			health dec draw-hud
			verb-sprite hide
			health @ 1 < if
				player-sprite hide
				60 for sync next
				"Melted by a slime!" abort
			then
			delay
			0 player-sprite tile!
			rdrop drop exit
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
		delay
		single @ if game-over then
		level inc
		level @ level-count >= if game-over then
		copy-level draw-hud
	then
	delay
;

: pithy-saying ( -- )
	17 player-sprite tile!
	RN @ 3 mod
		#XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
	dup 0 = if
		"'It's dangerous to go alone...'" typeln
		2 player-sprite tile!
		"Hmph, what does that old geezer know." typeln
		0 player-sprite tile!
		drop exit
	then
	dup 1 = if
		"Heeere's Lizzy!" typeln
		0 player-sprite tile!
		drop exit
	then
	dup 2 = if
		"Sweet- ancient ruins!" typeln
		"Just like it said in the brochures!" typeln
		drop exit
	then
	"Fame and riches, here I come!" typeln
	0 player-sprite tile!
	drop
;

: start-game ( level -- )
	name> dict-find swap
	game
	init-game
	copy-level
	59 for sync next
	single @ -if pithy-saying then
	draw-hud
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
	"7 const SPIKES" run
	
	{ health @ PUSH finish } "health" primitive ( -- n )
	{ level  @ PUSH finish } "level"  primitive ( -- n )
	{ gems   @ PUSH finish } "gems"   primitive ( -- n )
	{ keys   @ PUSH finish } "keys"   primitive ( -- n )
	{ game-step     finish } "wait"   primitive ( -- )

	{
		3 player-sprite tile!
		delay
		0 {
			dup slime? if
				9 over ent-pos N relative verb!
				delay
				swap 1 + swap
			then drop
		} ent-for PUSH
		0 player-sprite tile!
		verb-sprite hide
		delay
		finish
	} "listen" primitive ( -- n )

	{
		POP check-dir >r
		player-pos i relative at-pos
		dup SLIME = if
			player-pos r> relative player-sprite set-pos
			20 for sync next
			4 player-sprite tile!
			20 for sync next
			12 player-sprite tile!
			60 for sync next
			player-sprite hide
			60 for sync next
			"Stepped on a slime and dissolved!" abort
		then
		dup dup FLOOR = over SPIKES = or swap STAIRS = or -if
			4 player-sprite tile!
			delay
			0 player-sprite tile!
			delay
			terminal
			"Unable to walk " type
			i N = if "north" type then
			i E = if "east"  type then
			i S = if "south" type then
			i W = if "west"  type then
			"!" abort
		then
		player-pos r> relative player-sprite set-pos
		SPIKES = if
			health dec
			draw-hud
			4 player-sprite tile!
			delay
			0 player-sprite tile!
			health @ 1 < if
				"Impaled on spikes! Yeouch!" abort
			then
		then
		delay
		game-step finish
	} "walk" primitive ( dir -- )

	{
		POP check-dir >r
		3 player-sprite tile!
		5 player-pos i relative verb!
		delay
		verb-sprite hide
		player-pos i relative ent-at if
			dup slime? if
				12 over tile!
				delay
				dup ent-free
				kills inc
			then drop
		then
		r> drop
		0 player-sprite tile!
		game-step finish
	} "attack" primitive ( dir -- )

	{
		POP check-dir >r
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
		delay
		0 player-sprite tile!
		verb-sprite hide

		game-step finish
	} "take" primitive ( dir -- )

	{
		POP check-dir >r
		player-pos i relative at-pos DOOR = -if
			"No door to open!" abort
		then
		gkeys @ 1 < if
			"Can't open a door without a key!" abort
		then
		gkeys dec

		3 player-sprite tile!
		7 player-pos i relative verb!
		delay

		0 player-sprite tile!
		verb-sprite hide
		FLOOR       player-pos i relative !
		11 11 11 11 player-pos i relative draw-block
		 0  1 20 20 player-pos i relative N relative draw-block
		rdrop

		game-step finish
	} "open" primitive ( dir -- )

	{
		POP check-dir >r
		2 player-sprite tile!
		6 player-pos i relative verb!
		delay

		0 player-sprite tile!
		verb-sprite hide
		delay

		player-pos r> relative at-pos PUSH finish
	} "look" primitive ( dir -- kind )

	{ false single ! 0   start-game finish } "begin" primitive ( -- )
	{ true  single ! POP start-game finish } "test"  primitive ( level -- )
	{ true  fast !                  finish } "fast"  primitive ( -- )
	{ false fast !                  finish } "slow"  primitive ( -- )

	{
		"warrior.fs" XA !
		x-open-read  XS !
		XA @ -1 = if "Couldn't find 'warrior.fs'!" abort then
		{ XO @ } ' read revector clear-q trim
		loop eof? if break then token again
		"OK!" abort
	} "load" primitive ( -- )
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
##  Title Screen
##
######################################################

:image title-tiles "titleTiles.png" 8 8
:data title-grid
	 0  1  2  2  2  2  2  2  2  2  2  2  3  4  5  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  6  7  8  2  2 
	 0  9  2 10 11  2  2  2  2  2  2  3 12 13 14  5  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2  2 15 16  0  0 17 18 
	19  2 20 21 22 23 24 25 26 27 28 29 30 31 32 33 34 35 36 37 38 39 40 41 42 43 44 45  2  2  2  2  2 46 47 48  0  0 49 50 
	 0  2 51 52 53 54 55 56 57 58 59 60 61 62 63 64 65 66 67 68 69 70 71 72 73 74 75 76  2  2  2  2 77 78  0 79 80 81 82 83 
	 0 84 85 86 87  0  0 88 89 90 91 92 93 94 95 64 65 96 97 98  2 73 68  2 73 99 100 101  2  2  2  2 102 103  0 104 83 83 83 83 
	 0 105  0  0  0  0  0 88 106 107 108 92 109 110 111 64 65 112 113  2  2 73 68  2 73 114 115 76  2  2  2  2  2 116  0 117 118 83 83 83 
	119 120 121  0  0  0  0 122 123 124  2 125 126 127 128 129 130 131 132 133  2 134 135  2 134 135 136 137 138 139 140  2  2 141  0 142 83 83 83 83 
	143 144 145 146 147 148 149  0 150 151 152  2 153 154 155  2 156 157 158 159 160 161 162 163 164 165 166 167 168 169 169 170 171 172  0 142 83 83 83 83 
	 0 173 50 174  0  0  0  0 175 176 177  2  2 178  2  2 179 180 181 182 183 184 185 186 187 188 189 190  2  2  2 191 192 193 194 195 83 83 83 83 
	83 83 83 196 197 198  0  0 199  2  2  2  2  2  2  2  2 200 201 202 203 204  2  2  2  2  2  2  2  2  2  2  2 205  0 206 83 83 83 83 
	83 83 83 207 83 208  0  0 209  2  2  2  2  2  2  2 210 211 212 213 214 215 216 217  2  2  2  2 218 219  2  2 220 221  0 222 83 83 83 83 
	83 83 83 83 83 83 83 83 223 224 225 226 227  2  2  2 228 229 230 231 232 233 234 235  2  2  2 236 237 238  2 239 240 241  0 242 83 83 83 83 
	83 83 83 83 83 83 83 243 244 245  2 246 247 248 249 250 251 252 253 254 255 256 257 258  2  2 259 246  0 238  2  2  2 260  0 242 83 83 83 83 
	83 83 83 83 83 83 83 261 262 263  2 246  0 264 265 266 267 268 269 270 271 272 273 274 275 276 277 278  0 279 280 281 282  2 283 284 83 83 83 83 
	83 83 83 83 83 83 285 286 287 288 289 290  0 291  2 292 293 294 295 296 297 298 299 300 301 302 303  0 304 81 81 81 305 306 307 83 83 83 83 308 
	83 83 83 83 83 309 310 311  2  2  2 312  0 313 314 315 316 317 318 319 320 321 322 323  2 324 325  0 326 327 83 83 83 83 328 329 330 331 332 333 
	83 83 83 83 83 334 52 335 336  2 337 338  0 339 340  2  2 341 342 343 344 345 346 347  2  2 348 349  0  0 83 83 350 351 352 353 353 353 354 355 
	83 83 83 83 83 356 52 141 357  2 358 359 360 361 362 363 364 365 366 367 368 369 370 371 372  2 373 374  0  0 375 83 83 83 83 83 83 83 376 377 
	83 83 83 83 83 356 52 378 199  2 379 380 381 382 383 384 384 385 386 387 388 389 390 391 392  2 393 394 395 396 397 83 83 83 83 83 83 398 399 400 
	83 83 83 83 83 356 401 402 199  2 403  0  0  0 238 404 405 406 407 408 409 410 411 412 413 414 415 416 417 417 418 83 83 83 83 83 83 419 400 400 
	83 83 83 83 83 420  0 421 422 423 424  0  0  0 425 426 427 428 429 430 431 432 433 434 435 436 437 438  0  0  0 439 83 83 83 83 440 400 400 400 
	83 83 83 83 441 442 443  0  0  0  0  0  0  0 444 445 446 447 448 449 450 448 451  1  2 452 238  0  0  0  0 453 454 455 456 457 458 400 459 460 
	83 83 83 83 461 462 462 463 464  0  0  0  0  0 465  2  2 466 467 468 469 470 471 472 473 474 475 476 477 478 479 480 400 400 400 481 482 483 484 485 
	83 83 83 83 486  0  0  0  0  0  0  0 487 488  2 489 490 491 492 493 494 495 496 497 498 499 500 501 502 503 400 504 400 505 506 507 508 509 510 511 
	83 83 83 83 512  0  0  0  0  0  0 513 514  2 515 516 517 518 519 520 521 522 523 524 525 526 527 528 529 530 531 532 533 534 400 400 535 536 537 538 
	539 83 83 540  0  0  0  0  0  0  0 541 542  2 543 544 545 546 400 547 548 549 550 551 552 553 554 555 556 557 558 559 560 561 400 400 400 400 400 400 
	562 563 564 565  0  0  0  0  0  0  0  0 566  2  2  2 567 568 569 570 571 572 573 574 575 576 400 400 400 400 400 400 400 400 400 400 400 400 400 400 
	577 577 577 578 579 580 581  0  0  0  0 582 583 584 437  2 585 586 587 588 589 590 591 592 593 594 595 596 400 400 400 400 400 400 400 597 598 599 400 400 
	600 600 600 600 601 602 603 604  0 605 606 607 608 606 609 610 611 612 613 614 615 616 617 618 619 620 621 622 623 400 400 459 624 625 626 627 628 629 630 631 
	83 83 83 83 632  0  0  0  0 633 634 83 83 83 83 83 635 636 637 638 639 640 641 642 643 644 645 646 647 648 649 650 651 652 653 654 655 656 657 658 
:array title-padding 41 0

: intro ( -- )
	CL @ GT @ GP @ -1 GS !
	title-tiles GT !
	title-grid  GP !
	59 2 * for
		sync
	next
	0 GS ! GP ! GT ! CL !
;

######################################################
##
##  In-Game Help
##
######################################################

: help! ( help-str stack-str name -- )
	dict-find
	dup >r
	.dict-stack !
	r> .dict-help !
;

: init-help ( -- )
	{
		name> dict-find
		dup .dict-stack @ -if
			"No help is available for this word." typeln
			finish exit
		then
		dup .dict-stack @  code-text type
		dup .dict-flag @ if "*" type then
		dup .dict-help  @ plain-text " " type typeln
		drop
		finish
	} "help" primitive

	"Add b to a"                  "( a b -- n )"      "+"         help!
	"Subtract b from a"           "( a b -- n )"      "-"         help!
	"Multiply a by b"             "( a b -- n )"      "*"         help!
	"Divide a by b"               "( a b -- n )"      "/"         help!
	"Modulo a by b"               "( a b -- n )"      "mod"       help!
	"Bitwise AND of a and b"      "( a b -- n )"      "and"       help!
	"Bitwise OR of a and b"       "( a b -- n )"      "or"        help!
	"Bitwise XOR of a and b"      "( a b -- n )"      "xor"       help!
	"Is A less than B?"           "( a b -- f )"      "<"         help!
	"Is A greater than B?"        "( a b -- f )"      ">"         help!
	"Store n to address"          "( n addr -- )"     "!"         help!
	"Load n from address"         "( addr -- n )"     "@"         help!
	"Bitwise NOT of n"            "( n -- n' )"       "not"       help!
	"Copy top stack element"      "( n -- n n )"      "dup"       help!
	"Copy second stack element"   "( a b -- a b a )"  "over"      help!
	"Discard top stack element"   "( n -- )"          "drop"      help!
	"Switch top stack elements"   "( a b -- b a )"    "swap"      help!
	"Move n to rstack"            "( n -- )"          ">r"        help!
	"Move top rstack to stack"    "( -- n )"          "r>"        help!
	"Fetch top rstack element"    "( -- n )"          "i"         help!
	"Fetch second rstack element" "( -- n )"          "j"         help!
	"Get addr of dictionary tail" "( -- addr )"       "here"      help!
	"Are we compiling?"           "( -- flag )"       "mode"      help!
	"Append n to dictionary"      "( n -- )"          ","         help!
	"Switch to compiling mode"    "( -- )"            "]"         help!
	"Switch to interpreting mode" "( -- )"            "["         help!
	"Make last word immediate"    "( -- )"            "immediate" help!
	"Compile a literal number"    "( n -- )"          "literal"   help!
	"Start a new def"             "( -- )"            "create"    help!
	"Start compiling a new def"   "( -- )"            ":"         help!
	"Terminate a def"             "( -- )"            ";"         help!
	"Compile exit from word"      "( -- )"            "exit"      help!
	"Begin a conditional"         "( -- )"            "if"        help!
	"Optional for if...then"      "( -- )"            "else"      help!
	"Terminate an if"             "( -- )"            "then"      help!
	"Exit a loop early"           "( -- )"            "break"     help!
	"Begin a loop"                "( -- )"            "loop"      help!
	"Close an unconditional loop" "( -- )"            "again"     help!
	"Loop until true"             "( -- )"            "until"     help!
	"Loop while true"             "( -- )"            "while"     help!
	"Provide runtime semantics for a def" "( -- )"    "does>"     help!
	"Obtain an xt for a word name"        "( -- n )"  "'"         help!

	"Print a number"              "( n -- )"          "."         help!
	"Print a space"               "( -- )"            "space"     help!
	"Print a newline"             "( -- )"            "cr"        help!
	"Print a string (0-term)"     "( str -- )"        "type"      help!
	"Print a string and newline"  "( str -- )"        "typeln"    help!
	"Erase defs after name"       "( -- )"            "forget"    help!
	"Print remaining dict space"  "( -- )"            "free"      help!
	"List def's threaded code"    "( -- )"            "see"       help!
	"Single line comment"         "( -- )"            "#"         help!
	"Multiline comment"           "( -- )"            "("         help!
	"Print stack contents"        "( -- )"            "stack"     help!
	"List available words"        "( -- )"            "words"     help!
	"Define a variable"           "( -- )"            "var"       help!
	"Define a constant"           "( n -- )"          "const"     help!
	"Are two numbers equal?"      "( a b -- f )"      "="         help!
	"Invoke an xt (as from ')"    "( xt -- )"         "exec"      help!
	
	"How many hearts remain?"     "( -- n )"          "health"    help!
	"What level are we on?"       "( -- n )"          "level"     help!
	"How many gems do we have?"   "( -- n )"          "gems"      help!
	"How many keys do we have?"   "( -- n )"          "keys"      help!
	"Do nothing for a timestep"   "( -- )"            "wait"      help!
	"How many enemies are here?"  "( -- n )"          "listen"    help!
	"Move to adjacent tile"       "( dir -- )"        "walk"      help!
	"Attack adjacent tile"        "( dir -- )"        "attack"    help!
	"Pick up adjacent gem or key" "( dir -- )"        "take"      help!
	"Unlock adjacent door"        "( dir -- )"        "open"      help!
	"Get type of adjacent tile"   "( dir -- type )"   "look"      help!
	"Given word name, start game" "( -- )"            "begin"     help!
	"Given word name, try level"  "( level -- )"      "test"      help!
	"Faster game animations"      "( -- )"            "fast"      help!
	"Normal game animation speed" "( -- )"            "slow"      help!
	"Gee, I wonder?"              "( -- )"            "help"      help!
	"Load code from 'warrior.fs'" "( -- )"            "load"      help!
;

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
	init-help

	terminal
	' abort ' fail revector
	' repl restart-vector !

	(
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
	)
	
	intro

	1 1 "Forth Warrior v0.2" grid-type
	1 2 "MakoForth BIOS OK"  grid-type
	1 4 "(Type words for available commands.)"  grid-type
	1 5 "(Type help <word> for documentation.)" grid-type
	1 6 "(Type begin <word> to play.)"          grid-type

	code-text cc @ ascii !
	7 4 "words"        grid-type
	7 5 "help <word>"  grid-type
	7 6 "begin <word>" grid-type
	plain-text cc @ ascii !

	repl
;