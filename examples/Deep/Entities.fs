######################################################
##
##  Entities:
##
##  A system for dynamically allocating game entities
##  with an associated hardware sprite. Every entity
##  can be assigned a word which carries out game logic
##  and also serves as an identifier. The 'whoever'
##  word makes it easy to perform some action upon
##  valid entities which pass a predicate.
##
##  Note that the 'Sprites.fs' standard library must
##  be loaded before this lexicon.
##
##  John Earnest
##
######################################################

:const first-e 127
:const last-e  255
:const bogus-e 256

:array types  257 0
:array timers 257 0

: type!   swap types  + ! ; ( id 'func -- )
: timer!  swap timers + ! ; ( id count -- )

: valid ( id -- flag )
	types + @ if true else false then
;

: alloc ( -- n )
	0 bogus-e types + !
	first-e loop
		dup valid -if exit then
		1 +
	again
;

: free ( id -- )
	dup types  + 0 swap !
	dup timers + 0 swap !
	sprite@      0 swap !
;

: whoever ( 'filter 'func -- )
	>r >r first-e
	loop
		dup i exec over valid and
		if dup j exec then
		1 + dup last-e <=
	while
	drop r> r> 2drop
;

: count+      drop swap 1 + swap            ;
: count       0 swap ' count+ whoever       ; ( 'filter -- )
: always      drop true                     ;
: apply-type  dup types + @ exec            ;
: think       ' always ' apply-type whoever ;