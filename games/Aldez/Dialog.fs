######################################################
##
##  Dialog:
##
##  Routines for drawing dialog boxes and yes/no
##  prompts, both with support for character portraits.
##
##  John Earnest
##
######################################################



######################################################
##  
##  Portraits
##  
######################################################

:const  portrait 254

: lf-char! ( id -- )
	64x64 invisible swap   0 128 portrait >sprite
;

: rt-char! ( id -- )
	64x64 invisible swap 256 128 portrait >sprite
;

:  syd-cnormal 4 lf-char! ;
:  syd-chappy  5 lf-char! ;
:  syd-cshock  6 lf-char! ;
:  syd-cangry  7 lf-char! ;
:  syd-normal  8 lf-char! ;
:  syd-happy   9 lf-char! ;
:  syd-shock  10 lf-char! ;
:  syd-angry  11 lf-char! ;
: hank-normal 12 rt-char! ;
: hank-happy  13 rt-char! ;
: hank-shock  14 rt-char! ;
: hank-angry  15 rt-char! ;
: lopez       16 rt-char! ;

: clear-chars ( -- )
	0 portrait sprite@ !
;

######################################################
##  
##  Dialogs
##  
######################################################

:array grid-buffer 240 0
:array  msg-buffer   7 0

: dialog[
	portrait sprite@ @ if portrait show then
	5 for
		0 i 24 + tile-grid@ dup
		i 40 * grid-buffer + 40 >move
		40 0 grid-z or fill
	next
	0
;

: [dialog-begin]
	5 msg-buffer + loop
		over -if break then
		swap over ! 1 -
	again
	swap drop 1 +
	
	line-offset loop
		over @ over 1 swap tile-grid@
		loop
			over @ ascii @ + grid-z or over !
			keys key-a and -if sync then
			1 + swap 1 + swap over @
		while 2drop
		8 for sync next
		1 + swap 1 + swap over @
	while 2drop
	loop sync keys key-a and while
;

: [dialog-end]
	loop sync keys key-a and while
	5 for
		i 40 * grid-buffer +
		0 i 24 + tile-grid@ 40 >move
	next
	portrait hide
	clear-chars
;

: ]-text ( msg* -- )
	[dialog-begin]
	loop sync keys key-a and until
	[dialog-end]
;

: ]-ask ( msg* -- flag )
	[dialog-begin]
	true loop
		keys key-lf key-rt or and if
			not 10 for sync next
		then
		dup if 10 28 "> YES <      NO  "
		else   10 28 "  YES      > NO <" then
		grid-type sync keys key-a and
	until
	[dialog-end]
;