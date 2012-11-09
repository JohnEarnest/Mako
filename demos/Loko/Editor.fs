######################################################
##
##  Editor:
##
##  A collapsing and expanding text editor panel
##  which can be used in a variety of REPL/
##  command line applications. Depends on Grid.fs,
##  Sprites.fs and String.fs.
##
##  John Earnest
##
######################################################

:array grid-buffer 2400 0

: save-grid ( -- )
	grid-buffer
	28 for
		39 for
			39 i -
			28 j -
			tile-grid@ @ over !
			1 +
		next
	next
	drop
;

: restore-grid ( offset -- )
	40 * grid-buffer +
	28 for
		39 for
			dup @
			39 i -
			28 j -
			tile-grid@ !
			1 +
		next
	next
	drop
;

######################################################
##
##  Editor Support Words:
##
##  For the sake of simplicity I perform a number of
##  repeated calculations here which limit the state
##  of the text editor to the active buffer ('text')
##  and a cursor position within that buffer.
##
######################################################

:const cursor-tile   1792
:const cursor-sprite  129
:const text-offset     64

:const backspace  8
:const return    10
:const space     32
:const maxlines  28

:const text-size 1202
:array text text-size 0
:var cursor

: linestart    0 over 30 swap - tile-grid@ ; ( height -- height addr )
: keydelay     4 for sync next             ; ( -- )
: max          2dup < if swap then drop    ; ( a b -- max )
: min          2dup > if swap then drop    ; ( a b -- min )

: height ( -- lines )
	1 0 text >r
	loop
		( lines charcount | text-ptr )
		i @ -if break then
		i @ return = if
			drop 1 + 0
		else
			1 +
			dup 40 = if drop 1 + 0 then
		then
		r> 1 + >r
	again
	rdrop drop
;

: draw-buffer ( height -- )
	linestart over 41 * space text-offset + fill
	linestart text >r
	loop
		( height grid-ptr | text-ptr )
		i @ -if break then
		i @ return = if
			drop 1 - linestart
		else
			i @ text-offset + over !
			1 +
			dup GP @ - 41 mod 40 = if drop 1 - linestart then
		then
		r> 1 + >r
	again
	rdrop 2drop
;

: cursor-pos ( height -- tx ty )
	0 swap 30 swap - cursor @ text >r
	loop
		( tx ty charsleft | text-ptr )
		dup -if break then
		i @ return = if
			>r
			1 + swap drop 0 swap
			r>
		else
			>r
			swap 1 + swap
			over 40 = if 1 + swap drop 0 swap then
			r>
		then
		r> 1 + >r 1 -
	again
	drop rdrop
;

: cursor-set ( tx ty -- )
	30 height - -
	dup 0      <  if 2drop 0         cursor ! exit then
	dup height >= if 2drop text size cursor ! exit then
	0 text >r loop
		( ty charcount | text-ptr )
		over 0 = if 2drop break then
		i @ return = if
			drop 1 - 0
		else
			1 +
			dup 40 = if drop 1 - 0 then
		then
		r> 1 + >r
	again
	loop
		( tx | text-ptr )
		dup 0 =      if drop break then
		i @         -if drop break then
		i @ return = if drop break then
		1 - r> 1 + >r
	again
	r> text - cursor !
;

: trydelete ( -- )
	cursor @ 1 < if exit then
	cursor @ text + dup 1 - dup size <move
	cursor @ 1 - cursor !
;

: tryinsert ( char -- )
	dup backspace = if drop trydelete exit then
	cursor @ text + dup 1 + over size >move
	cursor @ text + !
	cursor @ 1 + cursor !
	height maxlines > if trydelete then
;

: cursor-line  height cursor-pos swap drop ; ( -- num )

######################################################
##
##  The Editor:
##
##  Automatically preserves any previously existing
##  grid contents and returns the updated string
##  when 'return' is pressed with the cursor on the
##  last line of text.
##
######################################################

: edit ( str -- str' )
	text text-size 0 fill
	text over size 1 + >move
	0 28 cursor-set
	save-grid

	loop
		# drain the keyboard buffer
		loop
			KB @ dup -1 = if drop break then
			dup 3 = if
				cursor-sprite hide
				0 restore-grid
				drop "BREAK" abort
			then
			dup return = if
				drop cursor-line 29 = if
					cursor-sprite hide
					0 restore-grid
					text exit
				else
					return tryinsert
				then
			else
				tryinsert
			then
		again

		# handle cursor keys
		keys key-lf and if
			cursor @ 1 - 0 max
			cursor ! keydelay
		then
		keys key-rt and if
			cursor @ 1 + text size min
			cursor ! keydelay
		then
		keys key-up and if
			height cursor-pos 1 -
			cursor-set keydelay
		then
		keys key-dn and if
			height cursor-pos 1 +
			cursor-set keydelay
		then

		height
		dup restore-grid
		dup draw-buffer

		# update the cursor
		8x8         swap
		cursor-tile swap
		cursor-pos 8 * swap 8 * swap
		cursor-sprite >sprite

		sync
	again
;