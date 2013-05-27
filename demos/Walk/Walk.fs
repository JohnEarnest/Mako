######################################################
##
##  Walk:
##
##  An adaptation of the "MSP430 Launchpad GamingPack"
##  sprite walking example by bear24rw.
##  Several aspects of the logic have been simplified
##  by rearranging the sprite sheet.
##
######################################################

:include <Sprites.fs>
:image sprite-tiles "link.png" 16 16

:const LINK   0 # link sprite index
:const SWORD  1 # sword sprite index
:data  dir    2 # direction link is facing
:data  foot   0 # which walking sprite to use
:data  count  0 # keep track of how many pixels since last walking switch

:data deltas 0 1 0 -1
: dx  deltas + @   ; ( dir -- mag )
: dy  1 - 4 mod dx ; ( dir -- mag )

: attack ( -- )
	SWORD show
	dir @ dup 2 *
	dup 8 + LINK  tile!
	    9 + SWORD tile!
	dup dx 16 * LINK px + SWORD px!
	    dy 16 * LINK py + SWORD py!
;

: step ( dir -- )
	dup dir !
	dup dx 4 * LINK +px
	    dy 4 * LINK +py
	count @ 1 + count !
	count @ 3 > if
		foot @ 1 xor foot !
		0 count !
	then
;

: walk ( -- )
	SWORD hide
	keys key-up and if 0 step exit then
	keys key-rt and if 1 step exit then
	keys key-dn and if 2 step exit then
	keys key-lf and if 3 step      then
;

: main ( -- )
	16x16 0 152 120 LINK  >sprite
	16x16 0   0   0 SWORD >sprite
	loop
		keys key-a and if attack else walk then
		2 for sync next
		dir @ 2 * foot @ + LINK tile!
	again
;