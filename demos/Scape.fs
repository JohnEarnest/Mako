:include <Util.fs>
:include <Grid.fs>
:include <Sprites.fs>
:include <Print.fs>

:data  sprite-tiles
:array blue   64 0xFF0000FF
:data  grid-tiles
:array yellow 64 0xFFFFFF00 :array orange 64 0xFFFF9900

: blocked? ( x y -- flag )
	dup 28 >          if 2drop true exit then
	2dup tile-grid@ @ if 2drop true exit then
	1 + tile-grid@ @
;
: left   1 - 40 mod ;
: right  1 + 40 mod ;

: slide ( x y -- x' y flag )
	RN @ 3 mod -if false exit then
	over left  over blocked? -if swap left  swap true exit then
	over right over blocked? -if swap right swap true exit then
	false
;

: fall ( -- )
	RN @ 40 mod # x coord
	0           # y coord
	loop
		2dup blocked? if slide -if break then then
		>r >r
		8x8 0 i 8 * j 8 * 0 >sprite
		r> r> 1 +
		RN @ 10 mod -if sync then
	again
	tile-grid@ 1 swap !
;

: main ( -- )
	500 for fall next
	0 hide
	loop sync again
;