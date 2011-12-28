######################################################
##
##  Blip:
##
##  A set of reusable sound effects for video games
##  based around generating raw 8-bit samples for
##  the Audio port.
##
##  John Earnest
##
######################################################

: square       4 / / 2 mod 128 * ;
: noise        4 / / 2 mod RN @ 96 mod 32 + * ;
: saw          4 / swap over mod swap 127 swap / * ;
: square-note  2000 for i over square AU ! next drop ;
:  noise-note  2000 for i over noise  AU ! next drop ;
:    saw-note  2000 for i over saw    AU ! next drop ;

:const C-3 245 :const C$3 231 :const D-3 218
:const D$3 206 :const E-3 194 :const F-3 183
:const F$3 173 :const G-3 163 :const G$3 154
:const A-3 145 :const A$3 137 :const B-3 130

:data scale-3  C-3 C$3 D-3 D$3 E-3 F-3 F$3 G-3 G$3 A-3 A$3 B-3

#######################################################
##
##  Effects
##
#######################################################

: scratch
	 250 for RN @ 96 mod AU ! next
	1750 for 0 AU ! next
;

: bounce
	2000 for
		i dup 23 / 50 + square AU !
	next
;

: dunk
	2000 for
		i 2000 over - 23 / 50 + square AU !
	next
;

#######################################################
##
##  Demos
##
#######################################################

:data instr square-note noise-note saw-note
: note
	RN @ 12 mod scale-3 + @
	RN @  3 mod instr   + @ exec
;

:data effects scratch bounce dunk note
: effect
	RN @ 4 mod effects + @ exec
;

:include "../Print.fs"

: main
	loop
		keys key-a and if
			effect
			loop sync keys key-a and while
		then
		sync
	again
;