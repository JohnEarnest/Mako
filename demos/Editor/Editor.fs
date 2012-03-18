:include <Print.fs>
:include <Grid.fs>
:include <Sprites.fs>
:include <String.fs>
:include <Math.fs>

:data  sprite-tiles
:image   grid-tiles "text.png" 8 8

:const cursor-s  0
:const tab-width 3
:const size  1200
:array block size 1200
:var   used
:var   cursor

:var dx
:var dy

: newline ( -- )
	0 dx ! dy inc@
;

: specials? ( char -- t | char f )
	dup 10 = if
		newline
		drop true exit
	then
	dup 9 = if
		tab-width dx +@
		drop true exit
	then
	false
;

: paint ( -- )
	GP @ 1271 0 fill
	0 dx ! 0 dy !
	0 loop
		dup cursor @ = if
			dx @ 8 * cursor-s px!
			dy @ 8 * cursor-s py!
		then
		dup used @ 1 - > if drop break then
		dup block + @ specials? -if
			32 - dx @ dy @ tile-grid@ !
			dx inc@ dx @ 39 > if newline then
		then
		1 +
	again
;

: -newline
	-1 loop
		dup cursor @ + block + @ 10 = if break then
		dup cursor @ + 0 <            if break then
		1 -
	again
	-1 * 1 - 
;

: +newline
	0 loop
		dup cursor @ + block + @ 10 = if break then
		dup cursor @ + used @ >=      if break then
		1 +
	again
;

: main ( -- )
	8x8 123 32 - 0 0 cursor-s >sprite

	loop
		KB @ dup -1 = if drop else
			dup 8 = if
				drop
				# backspace
				cursor @ block + dup 1 -
				used @ cursor @ - 1 + <move
				cursor dec@ used dec@
			else
				# insert
				used @ size >= if drop exit then
				cursor @ block + dup dup 1 +
				used @ cursor @ - >move !
				cursor inc@ used inc@
			then
		then

		keys key-rt and if cursor inc@ then
		keys key-lf and if cursor dec@ then
		cursor @      0 < if      0 cursor ! then
		cursor @ used @ > if used @ cursor ! then
		keys key-up and if
			-newline
			dup 1 + cursor -@
			-newline cursor -@
			+newline min cursor +@
		then
		keys key-dn and if
			-newline
			+newline 1 + cursor +@
			+newline min cursor +@
		then

		paint
		3 for sync next
	again
;