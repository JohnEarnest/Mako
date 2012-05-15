######################################################
##
##  Dialog:
##
##  Routines for drawing dialog boxes and
##  yes/no prompts, with animation. Assumes a
##  previous definition of a 'tick' word, which
##  will be called as an alternative to directly
##  invoking 'sync', allowing for audio kernels
##  and for background animation to take place.
##  Requires Grid.fs, Util.fs and String.fs be loaded.
##
##  The system is designed to support up to 10 lines
##  of dialog text, or 8 for a prompt, but this can
##  be increased easily by changing the size of
##  'message' and 'buffer'.
##
##  John Earnest
##
######################################################

:var          timer
:var    show-action
:var    action-tile
:var     solid-tile
:var          lines
:var     last-lines
:array      message  10 0
:array       buffer 400 0
:const   size-delay 2

: action ( -- addr )
	38 28 tile-grid@
;

: action! ( flag -- )
	40 timer !
	dup show-action !
	action-tile @ and
	action !
;

: pulse ( -- )
	show-action @ if
		timer dec@
		timer @ -40 < if 40 timer ! then
		timer @   0 > if action-tile else solid-tile then
		@ action !
	then
;

: ticks       for  tick pulse                next  ;
: tick-while  loop tick pulse keys key-a and while ;
: tick-until  loop tick pulse keys key-a and until ;

: dialog-begin ( marker string... -- )
	loop
		dup -if drop break then
		lines @ message + !
		lines inc@
	again

	lines @ 1 + for
		0 29 i - tile-grid@
		i 40 * buffer + 40 >move
	next

	last-lines @ lines @ < if
		0 loop
			0 over 29 swap - tile-grid@
			40 solid-tile @ fill
			dup last-lines @ 1 + > if size-delay ticks then
			1 + dup lines @ 1 + >
		until drop
	else
		0 loop
			0 over 29 swap - tile-grid@
			40 solid-tile @ fill
			1 + dup last-lines @ 1 + >
		until drop
		loop
			size-delay ticks
			last-lines @ 1 + 40 * buffer +
			0 29 last-lines @ 1 + - tile-grid@ 40 >move
			last-lines dec@
			last-lines @ lines @ >
		while
	then

	lines @ 1 - for
		i message + @
		2 28 i - tile-grid@
		loop
			over @ ascii @ + grid-z or over !
			keys key-a and -if tick then
			2inc over @
		while 2drop
		8 ticks
	next

	true action! tick-while
;

: dialog-end ( -- )
	tick-while false action!
	
	lines @ 1 + for
		0 i message + !
		i 40 * buffer +
		0 29 i - tile-grid@ 40 >move
	next
;

######################################################
##
##  Public routines:
##
######################################################

: dialog[ ( -- marker )
	lines @ last-lines !
	0 dup lines !
;

: ]-text ( marker string... -- )
	dialog-begin
	tick-until
	dialog-end
;

: ]-ask ( marker string... -- flag )
	" " dup message ! message 1 + ! 2 lines !
	dialog-begin
	true loop
		keys key-lf key-rt or and if not 10 ticks then
		dup if 10 28 "> YES <      no  "
		else   10 28 "  yes      > NO <" then
		grid-type tick pulse keys key-a and
	until
	dialog-end
;

: dialog-open ( -- )
	0 lines !
;

: dialog-close ( -- )
	lines @ 1 + for
		0 29 i - tile-grid@
		40 solid-tile @ fill
	next
	lines @ 1 + for
		size-delay ticks
		i 40 * buffer +
		0 29 i - tile-grid@ 40 >move
	next
;

######################################################
##
##  Convenience wrappers:
##
######################################################

: alert ( string -- )
	dialog-open dialog[ swap ]-text dialog-close
;

: ask? ( string -- flag )
	dialog-open dialog[ swap ]-ask dialog-close
;