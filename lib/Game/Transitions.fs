######################################################
##
##  Transitions:
##
##  A set of routines for performing grid-based
##  transition effects such as wipes and pans.
##  Requires that String.fs, Grid.fs and Sprites.fs
##  be loaded.
##
##  John Earnest
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

: clear-grid ( stride base -- )
	29 for
		over 41 + i * over +
		40 blank @ fill
	next 2drop
;

# Indicates a direction in which a sprite is offscreen,
# or -1 if the sprite is still onscreen. Directions
# provided are suitable for indicating a wipe or pan:
: off-dir ( sprite-id -- dir )
	dup px                  320 > if drop 2 exit then
	dup py                  240 > if drop 4 exit then
	dup px over .sprite-w -   0 < if drop 6 exit then
	dup py swap .sprite-h -   0 < if drop 0 exit then
	-1
;

######################################################
##
##  Horizontal blinds
##
######################################################

: blinds-out ( -- )
	0 tgrid save-grid show-t
	40 for
		30 for
			blank @ grid-z or
			i 2 mod if j else 40 j - then i tile-grid@ !
		next
		sync
	next
	40 for sync next
	hide-t
;

: blinds-in ( -- )
	40 for
		30 for
			i 2 mod if j else 40 j - then i 2dup
			tile-grid@ @ >r 41 * + tgrid + r> swap !
		next
		show-t sync hide-t
	next
;

######################################################
##
##  Panning
##
##  Directions are 0=N, 2=E, 4=S, 6=W to match
##  directions in Entities.fs
##
######################################################

:const pan-steps 10
:var   pan-dir
:data  pan-table
	 0 -1  0 1230    0
	 1  0 40    0   40
	 0  1  0    0 1230
	-1  0 40   40    0

: pan-dir!  2 / 5 * pan-table + pan-dir ! ;
: pan-dx                pan-dir @     @   ;
: pan-dy                pan-dir @ 1 + @   ;
: pan-gs                pan-dir @ 2 + @   ;
: pan-src         tgrid pan-dir @ 3 + @ + ;
: pan-dst         tgrid pan-dir @ 4 + @ + ;

: pan-anim ( -- )
	pan-steps 1 - for
		  40 pan-steps / pan-dx *
		1230 pan-steps / pan-dy * +
		GP @ + GP !

		-320 pan-steps / pan-dx *
		-240 pan-steps / pan-dy *
		255 for
			over i +px
			dup  i +py
		next
		2drop

		sync sync
	next
;

: pan-room ( dest-GP dest-GS dir -- )
	pan-dir!
	pan-gs pan-src save-grid
	over    GP ! dup    GS !
	pan-gs pan-dst save-grid
	pan-src GP ! pan-gs GS !
	pan-anim
	hide-t
;

######################################################
##
##  Simple wipes
##
######################################################

: wipe-out ( dir -- )
	pan-dir! GP @ GS @
	pan-gs pan-src save-grid
	pan-gs pan-dst clear-grid
	pan-src GP ! pan-gs GS !
	pan-anim
	hide-t
;

: wipe-in ( dir -- )
	4 + 8 mod
	pan-dir! GP @ GS @
	pan-gs pan-src clear-grid
	pan-gs pan-dst save-grid
	pan-src GP ! pan-gs GS !
	pan-anim
	hide-t
;