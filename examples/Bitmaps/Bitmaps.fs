######################################################
##
##  Bitmaps:
##
##  A demonstration of performing arbitrary per-pixel
##  animation by tiling sprites. This technique requires
##  20 sprite slots, and it is still possible to overlay
##  additional conventional sprites or use transparent
##  pixels to reveal the grid underneath.
##
##  John Earnest
##
######################################################

:array sprites       1024 0
:array sprite-tiles 81920 0 (320x256 pixel buffer)

:include "../Sprites.fs"

: pixel ( x y -- address )
	2dup 64 / 5 * swap 64 / + 4096 *
	swap 64 mod 64 * +
	swap 64 mod +
	ST @ +
;

: init-fullscreen ( -- )
	19 for
		64x64 i
		i 5 mod 64 * # x
		i 5 /   64 * # y
		i >sprite
	next
;

: main

	init-fullscreen

	0 # rolling animation counter

	loop

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