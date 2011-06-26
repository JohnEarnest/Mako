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
: inc@     dup @ 1 + swap !              ;
: dec@     dup @ 1 - swap !              ;
: neg@     dup @ -1 * swap !             ;
: inc-r    r> 1 + >r                     ;
: dec-r    r> 1 - >r                     ;
: +@       swap over @ swap + swap !     ;
: -@       swap over @ swap - swap !     ;
: random   RN @ swap mod                 ;
: brownian RN @ 3 mod 1 -                ;

: abs     dup 0 < if -1 * then      ;
: later   r> r> swap >r >r          ;

: swap@   2dup @ >r @ swap ! r> swap ! ;