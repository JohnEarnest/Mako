######################################################
##
##  BitTrip:
##
##  A demonstration of using the Bitmap.fs library
##  routines to display a complex animated bitwise
##  expression
##
##  John Earnest
##
######################################################

:include <Sprites.fs>
:include <Bitmap.fs>

: main
	init-fullscreen

	0 loop
		# Based on the example of figure 11 on page 4 of 
		# The Art of Computer Programming, volume 4, fascile 1A.
		239 for
			319 for
				dup dup i + j swap not over xor >r + 8 / r> and dup * 
				dup dup 256 * or 256 * or 0xFF000000 or
				i j pixel !
			next
		next
		4 +
		sync
	again
;