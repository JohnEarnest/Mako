######################################################
##
##  Console:
##
##  A grid-based virtual console, using the scroll
##  registers to achieve pixel-wise animated scrolling
##  rather than character-based scrolling. Note the
##  use of revectoring to leverage the existing
##  infrastructure of the Print lexicon.
##
##  John Earnest
##
######################################################

:include <Print.fs>
:include <Vector.fs>
:include <Util.fs>

:image grid-tiles "transparentFont.png" 8 8
:array console-display 1271 0
:var   cursor 0

: console-emit ( char -- )
	dup 10 = if
		cursor @ 40 / 1 + 40 * cursor !
		drop
	else
		32 - # our font's chars start at ASCII 32
		GP @ cursor @ + !
		cursor inc@
	then

	cursor @ 1199 > if
		# animate scrolling the grid
		6 for SY inc@ sync next
		# reset the scroll registers
		console-display GP ! 0 SY !
		# shift the data on the grid
		1159 for
			console-display 1159 + i - 40 + @
			console-display 1159 + i -     !
		next
		# reposition the cursor
		cursor @ 40 - cursor !
	then
;

: init-console ( -- )
	' console-emit ' emit revector
	-1 GS !
	console-display GP !
	0 cursor !
	1270 for -1 console-display i + ! next
;

: exit-console ( -- )
	' emit devector
;

: main
	init-console

	"ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz" typeln
	0
	loop
		"Hello, World!" type space
		dup . cr
		1 +
		sync
	again
;