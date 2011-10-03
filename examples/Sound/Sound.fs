######################################################
##
##  Sound:
##
##  A number of small experiments in audio hardware
##  and software synthesis for Mako.
##
##  John Earnest
##
######################################################

: >> for 2 / next ;
: << for 2 * next ;
:var t

# (t>>7|t|t>>6)*10+4*(t&t>>13|t>>6)

: s0  t @  6 >> or         ;
: s1  t @  7 >> t @ or  s0 ;
: s2  t @ 13 >> t @ and s0 ;
: s3  s1 10 * s2 4 * +     ;

:include "../Print.fs"

: main
	0 t !
	loop
		s3 AU !
		t @ 1 + t !
		sync
	again
;