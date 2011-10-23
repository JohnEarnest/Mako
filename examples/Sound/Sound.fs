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

:const tempo 850
: note tempo for i over / 2 mod 255 * AU ! next drop ;
: rst  tempo for 0 AU ! next ;

: C-1 61 note ;
: C$1 58 note ;
: D-1 55 note ;
: D$1 51 note ;
: G$1 39 note ;
: A$1 34 note ;
: B-1 32 note ;

: cs1  D$1 rst G$1 rst D$1 rst G$1 rst ;
: cs2  D-1 rst G$1 rst D-1 rst G$1 rst ;
: cs3  C$1 rst G$1 rst C$1 rst G$1 rst ;
: cs4  C-1 rst G$1 rst C-1 rst C$1 D-1 ;
: cs5  B-1 rst A$1 rst G$1 rst rst rst ;
: cavestory cs1 cs2 cs3 cs4
            cs1 cs2 cs3 cs5 ;

: main
	0 t !
	loop
		# 100 for s3 AU ! t @ 1 + t ! next
		cavestory
		sync
	again
;