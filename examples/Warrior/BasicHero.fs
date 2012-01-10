######################################################
##
##  BasicHero
##
##  A crude example of a hero controller for
##  playing Forth Warrior.
##
##  John Earnest
##
######################################################

:const start-level 0
:include "Warrior.fs"

: level ( n -- )
	"Look, Ma- I made it to level " type . "!" typeln
	"I hear " type listen . "monsters..." typeln
;

:var going-down
:var going-up

: try ( dir -- success? )
	dup look
	dup gem    = if drop take   true exit then
	dup slime  = if drop attack true exit then
	    solid != if      walk   true exit then
	drop false
;

: tick ( -- )
	going-up @ if
		north try
		if exit else false going-up ! then
	then

	going-down @ if
		south try
		if exit else false going-down ! then
	then

	east try
	-if
		north look empty  = if north walk true going-up   ! exit then
		south look empty  = if south walk true going-down ! exit then
	then
;
