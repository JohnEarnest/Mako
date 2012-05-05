######################################################
##
##  RuleR:
##
##  A flexible implementation of the various
##  'wolfram rule' 1D cellular automata.
##  Fun choices include 30 and 110.
##
######################################################

:const               RULE 30
:const clear-color   0xFFAA99FF
:array grid-tiles 64 0xFF0000AA

: >>      dup if 1 - for 2 / next else drop then   ; ( a bits -- b )
: pat     0 2 for 2 * over i + @ or next swap drop ; ( addr -- pattern )
: rule    pat RULE swap >> 1 and                   ; ( addr -- new-value )
: cell    41 * + GP @ +                            ; ( y x -- addr )
: get     1 - swap 1 + cell                        ; ( y x -- addr )
: put         swap     cell !                      ; ( val y x -- )
: row     39 for dup i get rule over i put next    ; ( y -- y )
: draw    28 for r> row >r next                    ;
: main    1 29 19 put draw loop sync again         ;