######################################################
##
##  Bitmap:
##
##  Routines for performing arbitrary per-pixel
##  animation by tiling sprites. This technique requires
##  20 sprite slots, and it is still possible to overlay
##  additional conventional sprites or use transparent
##  pixels to reveal the grid underneath.
##
##  The 'Sprites.fs' standard library must
##  be loaded before this lexicon.
##
##  John Earnest
##
######################################################

:array screenbuffer 81920 0 (320x256 pixel buffer)

: pixel ( x y -- address )
	2dup 64 / 5 * swap 64 / + 4096 *
	swap 64 mod 64 * +
	swap 64 mod +
	ST @ +
;

: init-fullscreen ( -- )
	screenbuffer ST !
	19 for
		64x64 i
		i 5 mod 64 * # x
		i 5 /   64 * # y
		i >sprite
	next
;
