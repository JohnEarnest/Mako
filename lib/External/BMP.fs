######################################################
##
##  BMP.fs
##
##  A lexicon for reading and writing files in the
##  BMP image format via the XO port. Depends on
##  'Print.fs'.
##
##  John Earnest
##
######################################################

:vector file-abort ( str -- )
	typeln halt
;

: >file ( path -- port )
	dup >r
	XA ! x-open-read XS !
	XA @ -1 = if
		"unable to open '" type i type "'!" file-abort
	then rdrop
	XA @
;

: file> ( path -- port )
	dup >r
	XA ! x-open-write XS !
	XA @ -1 = if
		"unable to create '" type i type "'!" file-abort
	then rdrop
	XA @
;

: file-close  XA ! x-close XS ! ; ( port -- )

######################################################
##
##  Little-Endian Byte-Level IO and Codec State:
##
######################################################

:var offset
:var buffer
:var bmp-width
:var bmp-height

: inc  dup @ 1 + swap ! ; ( addr -- )

: int8>   XO @ offset inc           ; ( -- n )
: int16>  int8>  int8>  256   * or  ; ( -- n )
: int32>  int16> int16> 65536 * or  ; ( -- n )

: >int8   255 and XO ! offset inc   ; ( n -- )
: >int16  dup >int8  256   / >int8  ; ( n -- )
: >int32  dup >int16 65536 / >int16 ; ( n -- )

: scan-to ( goal-offset -- )
	offset @ over >= if drop exit then
	loop
		int8> drop
		offset @ over =
	until drop
;

######################################################
##
##  BMP Decoder:
##
######################################################

: >header ( -- )
	0 offset !
	int8> "B" @ = int8> "M" @ = and
	-if "unknown header type." file-abort then

	int32> drop         # file size in bytes
	int32> drop         # reserved header
	int32>              # offset to pixel data
	int32> drop         # dib-size
	int32> bmp-width  ! # width in pixels
	int32> bmp-height ! # may be negative (!)
	int16> drop         # color planes (always 1)

	int16> 32 = -if "only 32 bits per pixel is supported!" file-abort then
	scan-to
;

: >row ( -- )
	bmp-width @ 1 - for
		# horizontal unpadded 32-bit ARGB packed pixels
		# mask in alpha to make bitmap usable by Mako.
		int32> 0xFF000000 or buffer @ ! buffer inc
	next
;

: >bmp ( buffer path -- w h )
	>file >r buffer !
	>header
	bmp-height @ 0 < if
		# negative height means rows
		# are in top-to-bottom order:
		bmp-height @ -1 * dup bmp-height !
		1 - for >row next
	else
		# otherwise they're bottom to top:
		bmp-height @ 1 - bmp-width @ *
		buffer @ + buffer !
		bmp-height @ 1 - for
			>row
			buffer @ bmp-width @ 2 * - buffer !
		next
	then
	r> file-close
;

######################################################
##
##  BMP Encoder:
##
######################################################

: header> ( -- )
	0 offset !
	"B" @ >int8 "M" @ >int8
	bmp-width @ bmp-height @ *
	54 +         >int32 # file size
	0            >int32 # (unused) reserved header
	54           >int32 # offset to pixels
	40           >int32 # BITMAPINFOHEADER size
	bmp-width  @ >int32 # width
	bmp-height @ >int32 # height (bottom-to-top)
	1            >int16 # color planes
	32           >int16 # bits per pixel
	0            >int32 # BI_RGB, no compression
	0            >int32 # raw pixel data size (ignored for BI_RGB)
	2835         >int32 # horizontal resolution
	2835         >int32 # vertical   resolution
	0            >int32 # colors in the palette
	0            >int32 # all colors are "important"
;

: row> ( -- )
	bmp-width @ 1 - for
		# BMP expects the alpha channel to be 00:
		buffer @ @ 0x00FFFFFF and >int32 buffer inc
	next
	buffer @ bmp-width @ 2 * - buffer !
;

: bmp> ( buffer path -- )
	file> >r buffer !
	header>
	bmp-height @ 1 - bmp-width @ * buffer @ + buffer !
	bmp-height @ 1 - for row> next
	r> file-close
;

######################################################
##
##  Usage Example:
##
######################################################

(
:array storage 4096 0
:include <Sprites.fs>

: main
	storage "test.bmp" >bmp
	storage "copy.bmp" bmp>

	storage ST !
	64x64 0 10 10 0 >sprite
	loop sync again
;
)