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

:const   max-e 127
:const bogus-e -1

0 :array kinds    max-e 0
0 :array readies  max-e 0
0 :array timers   max-e 0
0 :array solids   max-e 0
0 :array triggers max-e 0
0 :array dirs     max-e 0
0 :array hits     max-e 0

: kind     kinds    + ;
: ready    readies  + ;
: timer    timers   + ;
: solid    solids   + ;
: trigger  triggers + ;
: dir      dirs     + ;
: hit      hits     + ;

: valid ( id -- flag )
	kind @ if true else false then
;

: alloc ( -- n )
	0 bogus-e kind !
	max-e 1 - loop
		dup valid -if exit then
		1 -
	again
;

: free ( id -- )
	0 over kind    !
	0 over ready   !
	0 over timer   !
	0 over solid   !
	0 over trigger !
	0 over dir     !
	0 over hit     !
	0 swap sprite@ !
;

: whoever ( 'filter 'func -- )
	>r >r
	0 loop
		dup i exec over valid and
		if dup j exec then
		1 + dup max-e <
	while
	drop r> r> 2drop
;

: apply-kind  
	dup ready @ -if true swap ready ! exit then
	dup kind @ exec
;

: count+   drop swap 1 + swap            ;
: count    0 swap ' count+ whoever       ; ( 'filter -- )
: always   drop true                     ;
: think    ' always ' apply-kind whoever ;

######################################################
##
##  High-level entity management
##
######################################################

: spawn ( tile tile-x tile-y 'func -- id )
	alloc >r
	i kind !
	16x16 i sprite@ !
	room-y @ 30 * - 8 *  i py!
	room-x @ 40 * - 8 *  i px!
	i tile!
	r>
;

: spawn-rel ( tile delta-x delta-y id 'func -- id )
	alloc >r
	i kind !
	16x16 i sprite@ ! >r
	i  py + j py!
	r> px + i px!
	i tile!
	r>
;

: sort-sprites ( -- )
	# I may or may not need to do this for the
	# Aldez engine. The perspective seems like
	# it might make it unnecessary.
;

: find-entity ( 'pred -- id? flag )
	max-e 1 - for
		i over exec if drop r> true exit then
	next
	drop false
;
