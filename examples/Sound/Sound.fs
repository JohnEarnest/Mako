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

# t*9&(t*1>>4) | t*5&(t>>7) | t*3&(t*4>>12)
: m0  t @ 4      >> t @ 9 * and ;
: m1  t @ 7      >> t @ 5 * and ;
: m2  t @ 4 * 12 >> t @ 3 * and ;
: m4  m0 m1 m2 or or ;

:include "../Print.fs"

:const tempo 1000
: square   ( i n -- ) / 2 mod 128 * ;
: sawtooth ( i n -- )     mod  32 * ;
: note
	tempo for
		#i over sawtooth
		#i over square
		i over 2dup square >r sawtooth r> or
		AU !
	next
	drop
;
: rst  tempo for 0 AU ! next ;

: C-1 31 note ; # 61
: C$1 29 note ; # 58
: D-1 27 note ; # 55
: D$1 26 note ; # 51
: G$1 19 note ; # 39
: A$1 17 note ; # 34
: B-1 16 note ; # 32

: cs1  D$1 rst G$1 rst D$1 rst G$1 rst
       D-1 rst G$1 rst D-1 rst G$1 rst
       C$1 rst G$1 rst C$1 rst G$1 rst ;
: cs2  C-1 rst G$1 rst C-1 rst C$1 D-1 ;
: cs3  B-1 rst A$1 rst G$1 rst rst rst ;
: cavestory cs1 cs2 cs1 cs3 ;

: main
	0 t !
	#cavestory
	loop
		100 for m4  AU ! t @ 1 + t ! next
		sync
	again
;

(# rolling white noise
: main
	loop
		16 for 8000 for RN @ 16 j + mod AU ! next next
		16 for 8000 for RN @ 32 j - mod AU ! next next
	again
;
)