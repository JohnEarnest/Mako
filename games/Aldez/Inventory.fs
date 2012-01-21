######################################################
##
##  Inventory:
##
##  Code for displaying and browsing the map and
##  inventory screen in Aldez.
##
##  John Earnest
##
######################################################

:data inventory
	 -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1 
	 -1 144 148 148 148 148 148 148 148 148 148 148 148 148 145  -1 144 148 148 148 148 148 148 148 148 148 148 148 148 148 148 148 148 148 148 148 148 148 145  -1  -1 
	 -1 150  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1 151  -1 150  19  19  19  19  19  19  19  19  19  19  19  19  19  19  19  19  19  19  19  19  19 151  -1  -1 
	 -1 150  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1 151  -1 150  19 144 148 145  19 144 148 145  19 144 148 145  19 144 148 145  19 144 148 145  19 151  -1  -1 
	 -1 150  -1  -1  19  19  -1  -1  -1  -1  19  19  -1  -1 151  -1 150  19 150 134 180 162 182 134 151  19 150 134 151  19 150 134 180 162 182 134 151  19 151  -1  -1 
	 -1 150  -1  -1  19  19  -1  -1  -1  -1  19  19  -1  -1 151  -1 150  19 146 149 147  19 146 176 147  19 146 176 147  19 146 149 147  19 146 176 147  19 151  -1  -1 
	 -1 150  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1 151  -1 150  19  19  19  19  19  19 160  19  19  19 160  19  19  19  19  19  19  19 160  19  19 151  -1  -1 
	 -1 150  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1 151  -1 150  19 144 148 145  19 144 178 145  19 144 178 145  19 144 148 145  19 144 178 145  19 151  -1  -1 
	 -1 150  -1  -1  19  19  -1  -1  -1  -1  19  19  -1  -1 151  -1 150  19 150 134 151  19 150 134 180 162 182 134 180 162 182 134 180 162 182 134 151  19 151  -1  -1 
	 -1 150  -1  -1  19  19  -1  -1  -1  -1  19  19  -1  -1 151  -1 150  19 146 176 147  19 146 176 147  19 146 176 147  19 146 149 147  19 146 176 147  19 151  -1  -1 
	 -1 150  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1 151  -1 150  19  19 160  19  19  19 160  19  19  19 160  19  19  19  19  19  19  19 160  19  19 151  -1  -1 
	 -1 150  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1 151  -1 150  19 144 178 145  19 144 178 145  19 144 178 145  19 144 148 145  19 144 178 145  19 151  -1  -1 
	 -1 150  -1  -1  19  19  -1  -1  -1  -1  19  19  -1  -1 151  -1 150  19 150 134 180 162 182 134 151  19 150 134 151  19 150 134 180 162 182 134 151  19 151  -1  -1 
	 -1 150  -1  -1  19  19  -1  -1  -1  -1  19  19  -1  -1 151  -1 150  19 146 149 147  19 146 176 147  19 146 176 147  19 146 176 147  19 146 149 147  19 151  -1  -1 
	 -1 150  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1 151  -1 150  19  19  19  19  19  19 160  19  19  19 160  19  19  19 160  19  19  19  19  19  19 151  -1  -1 
	 -1 150  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1 151  -1 150  19 144 148 145  19 144 178 145  19 144 178 145  19 144 178 145  19 144 148 145  19 151  -1  -1 
	 -1 150  -1  -1  19  19  -1  -1  -1  -1  19  19  -1  -1 151  -1 150  19 150 134 151  19 150 134 180 162 182 134 180 162 182 134 180 162 182 134 151  19 151  -1  -1 
	 -1 150  -1  -1  19  19  -1  -1  -1  -1  19  19  -1  -1 151  -1 150  19 146 176 147  19 146 176 147  19 146 176 147  19 146 149 147  19 146 176 147  19 151  -1  -1 
	 -1 150  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1 151  -1 150  19  19 160  19  19  19 160  19  19  19 160  19  19  19  19  19  19  19 160  19  19 151  -1  -1 
	 -1 150  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1 151  -1 150  19 144 178 145  19 144 178 145  19 144 178 145  19 144 148 145  19 144 178 145  19 151  -1  -1 
	 -1 146 149 149 149 149 149 149 149 149 149 149 149 149 147  -1 150  19 150 134 180 162 182 134 151  19 150 134 151  19 150 134 180 162 182 134 151  19 151  -1  -1 
	 -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1 150  19 146 149 147  19 146 149 147  19 146 149 147  19 146 149 147  19 146 149 147  19 151  -1  -1 
	 -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1 150  19  19  19  19  19  19  19  19  19  19  19  19  19  19  19  19  19  19  19  19  19 151  -1  -1 
	 -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1 146 149 149 149 149 149 149 149 149 149 149 149 149 149 149 149 149 149 149 149 149 149 147  -1  -1 
	 -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1 
	 -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1 
	 -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1 
	 -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1 
	 -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1 
	 -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1 
	 -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1  -1 

######################################################
##  
##  Cursor
##  
######################################################

:var cursor-x
:var cursor-y

: cursor-base ( tile deltax deltay -- )
	cursor-y @ 4 * + 3 + swap
	cursor-x @ 6 * + 3 + swap
	tile-grid@ !
;

: draw-cursor ( tl tr bl br -- )
	3 3 cursor-base
	0 3 cursor-base
	3 0 cursor-base
	0 0 cursor-base
;

: move-cursor ( deltax deltay -- )
	 -1  -1  -1  -1 draw-cursor
	cursor-y @ + 4 mod cursor-y !
	cursor-x @ + 2 mod cursor-x !
	128 129 130 131 draw-cursor
	10 for sync next
;

: cursor-item ( -- index )
	cursor-x @ 4 * cursor-y @ +
;

######################################################
##  
##  Item icons
##  
######################################################

:const item-sprites 239
: show-item ( index -- )
	>r
	16x16
	i 48 +
	i 4 /   48 * 32 +
	i 4 mod 32 * 32 +
	r> item-sprites +
	>sprite
;

: show-items ( -- )
	7 for
		i item-flags + @
		if i show-item then
	next
;

: hide-items ( -- )
	7 for
		0 0 0 0 i item-sprites + >sprite
	next
;

######################################################
##  
##  Item descriptions
##  
######################################################

:data d-helm 2
	"The Helm of Searing Light."
	"Emits an eye-watering glow."
:data d-shield 2
	"The Shield of Impenetrability."
	"Can deflect fireballs and impacts."
:data d-spear 2
	"The Spear of Stabbing."
	"Does what it says on the packet."
:data d-mitten 3
	"The Hot-Pot Mitten of Baking."
	"It's warm and fuzzy,"
	"but not... you know, TOO warm."
:data d-boots 2
	"The Boots of Cloud-Hopping."
	"With apologies to George Lucas."
:data d-map 2
	"The Map of... This Dungeon."
	"Do I really need to explain this?"
:data d-compass 3
	"The Compass of Finding."
	"If a room contains a hidden item,"
	"it will be indicated on the map."
:data d-??? 2
	"How the hell did you pick THAT up?"
	"I'm officially calling shenanigans."
	#------------------------------------#

:data descriptions
	d-helm
	d-shield
	d-spear
	d-mitten
	d-boots
	d-map
	d-compass
	d-???

:var desc-timer
:var desc-index
:var desc-lines
:var desc-line
:var desc-char

: animate-desc ( -- )
	cursor-item item-flags + @ -if exit then

	desc-timer @ if desc-timer dec@ exit then
	desc-line  @ desc-lines @ >= if exit then
	desc-index @ @ char-offset +
	desc-char  @ desc-line @ line-offset + tile-grid@ !
	
	desc-index inc@
	desc-char  inc@
	desc-index @ @ -if
		desc-line  inc@
		desc-index inc@
		2 desc-char !
	then
;

: reset-desc ( -- )
	10 desc-timer !
	 0 desc-line !
	 2 desc-char !
	cursor-item descriptions + @
	dup @ desc-lines !
	1 +   desc-index !

	29 line-offset - for
		0 i line-offset + tile-grid@
		40 -1 fill
	next
;

######################################################
##  
##  Map updates
##  
######################################################

: visited?  5 * + room-visited + @ ; ( x y -- flag )
: secret?   5 * + room-secret  + @ ; ( x y -- flag )

: for-map ( proc[ tile* -- ] -- )
	19 for
		19 for
			j 17 +
			i  2 +
			tile-grid@
			over exec
		next
	next
	drop
;

: hide-map  { dup @ abs -1 * swap ! } for-map ; ( -- )
: show-map  { dup @ abs      swap ! } for-map ; ( -- )

: room-tile ( dx rx dy ry -- addr )
	4 * +  2 + >r
	4 * + 17 + r>
	tile-grid@
;

: passage? ( tile -- flag )
	dup  160 =
	over 161 = or
	over 162 = or
	swap 163 = or
;

: update-indicator ( x y -- )
	>r >r
	has-map @ if
		134 i j visited? 1 and +
		has-compass @ if
			i j secret? 1 and 2 * -
		then
	else
		has-compass @ i j secret? and
		if 132 else 19 then
	then
	2 r> 2 r> room-tile !
;

: update-room ( x y -- )
	>r >r

	# passage right
	4 i 2 j room-tile @ passage? if
		i 1 + j visited? 
		i j visited? and if
			183 163 181
		else
			182 162 180
		then
		3 i 2 j room-tile !
		4 i 2 j room-tile !
		5 i 2 j room-tile !
	then

	# passage down
	2 i 4 j room-tile @ passage? if
		i j 1 + visited?
		i j visited? and if
			179 161 177
		else
			178 160 176
		then
		2 i 3 j room-tile !
		2 i 4 j room-tile !
		2 i 5 j room-tile !
	then

	# erase room selectors
	19 19 19
	0  i 4  j room-tile !
	4  i 0  j room-tile !
	0 r> 0 r> room-tile !
;

: update-map
	has-map @ if show-map else hide-map then
	4 for
		4 for
			i j update-indicator
			has-map @ if i j update-room then
		next
	next

	# draw room selector
	room-y @ >r
	room-x @ >r
	128 129 130 131
	4  i 4  j room-tile !
	0  i 4  j room-tile !
	4  i 0  j room-tile !
	0 r> 0 r> room-tile !
;

######################################################
##  
##  Main routine
##  
######################################################

:array sprite-buffer 1024 0

: show-inventory
	GS @ GP @ SP @
	inventory     GP ! 0 GS !
	sprite-buffer SP !

	 -1 -1 -1 -1 draw-cursor
	0 cursor-x !
	0 cursor-y !
	0 0 move-cursor
	reset-desc
	show-items
	update-map

	10 for sync next

	loop
		keys dup key-lf and if -1 0 move-cursor reset-desc then
		     dup key-rt and if  1 0 move-cursor reset-desc then
		     dup key-up and if 0 -1 move-cursor reset-desc then
		     dup key-dn and if 0  1 move-cursor reset-desc then
		     dup key-a and swap key-b and or if break then
		animate-desc
		sync
	again

	loop sync keys key-a and keys key-b and or until
	loop sync keys key-a and keys key-b and or while

	hide-items
	SP ! GP ! GS !
;