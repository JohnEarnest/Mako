######################################################
##
##  Voters:
##
##  An implementation of the "voters" simulation
##  described in "The Armchair Universe".
##  Voters in a two-party system randomly take on
##  the party affiliation of one of their
##  toroidal neighbors.
##
######################################################

:const clear-color   0xFFFFCC00
:array grid-tiles 64 0xFF0000AA
:data delta  0 1 1 1 0 -1 -1 -1

: dx        delta + @                               ; ( dir -- dy )
: dy        2 - 8 mod dx                            ; ( dir -- dx )
: >xy       dup 41 mod swap 41 /                    ; ( index -- x y )
: xy>       31 mod 41 * swap 41 mod +               ; ( x y -- index )
: offset    >xy swap >r + swap r> + swap xy>        ; ( dx dy index -- )
: neighbor  >r RN @ 8 mod dup dx swap dy r> offset  ; ( index -- index' )
: voter     RN @ 1271 mod                           ; ( addr -- )
: index!    GP @ + !                                ; ( index -- val )
: index@    GP @ + @                                ; ( val index -- )
: change    voter dup neighbor index@ swap index!   ; ( -- )
: fill      1270 for RN @ 2 mod 1 - i index! next   ; ( -- )
: main      fill loop 31 for change next sync again ; ( -- )