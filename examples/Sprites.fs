######################################################
##
##  Sprites:
##
##  A lexicon for manipulating Mako sprites.
##
##  John Earnest
##
######################################################

: sprite@    4 * SP @ + ;
: .sprite-t  1 + ;
: .sprite-x  2 + ;
: .sprite-y  3 + ;
: .sprite-w  @ 0x0F00 and  256 / 1 + 8 * ;
: .sprite-h  @ 0xF000 and 4096 / 1 + 8 * ;

: px         sprite@ .sprite-x @ ;
: py         sprite@ .sprite-y @ ;
: px!        sprite@ .sprite-x ! ;
: py!        sprite@ .sprite-y ! ;

: face-left  dup @ sprite-mirror-horiz not and swap ! ;
: face-right dup @ sprite-mirror-horiz or      swap ! ;
: flip-h     dup @ sprite-mirror-horiz xor     swap ! ;
