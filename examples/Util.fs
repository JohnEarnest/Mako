######################################################
##
##  Util:
##
##  A lexicon of common utility words.
##
##  John Earnest
##
######################################################

: =       xor -if -1 else  0 then   ;
: !=      xor -if  0 else -1 then   ;
: inc@    dup @ 1 + swap !          ;
: dec@    dup @ 1 - swap !          ;
: neg@    dup @ -1 * swap !         ;
: +@      swap over @ swap + swap ! ;
: -@      swap over @ swap - swap ! ;
: random  RN @ swap mod             ;