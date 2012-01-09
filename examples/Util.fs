######################################################
##
##  Util:
##
##  A lexicon of common utility words.
##
##  John Earnest
##
######################################################

: =        xor -if true  else false then ;
: !=       xor -if false else true  then ;
: 0=            if false else true  then ;
: 0!            if true  else false then ;
: inc@     dup @ 1 + swap !              ;
: dec@     dup @ 1 - swap !              ;
: neg@     dup @ -1 * swap !             ;
: swap@    2dup @ >r @ swap ! r> swap !  ;
: inc-r    r> r> 1 + >r >r               ;
: dec-r    r> r> 1 - >r >r               ;
: +@       swap over @ swap + swap !     ;
: -@       swap over @ swap - swap !     ;
: random   RN @ swap mod                 ;
: brownian RN @ 3 mod 1 -                ;
: abs      dup 0 < if -1 * then          ;
: later    r> r> swap >r >r              ;
: exitif   if rdrop exit then            ;
: tuck     swap over                     ;
: nip      swap drop                     ;
: ?dup     dup if dup then               ;
: within   over >= >r <= r> and          ; ( min v max )


: indexof (value array -- address)
	loop
		2dup @ =
		if swap drop exit then
		1 +
	again
;