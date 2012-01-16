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

:const   max-e 128
:const bogus-e -1

0 :array kinds    max-e 0
0 :array readies  max-e 0
0 :array timers   max-e 0
0 :array dirs     max-e 0
0 :array bullets  max-e 0
0 :array parents  max-e 0
0 :array hits     max-e 0
0 :array hps      max-e 0

: kind     kinds    + ;
: ready    readies  + ;
: timer    timers   + ;
: dir      dirs     + ;
: bullet   bullets  + ;
: parent   parents  + ;
: hit      hits     + ;
: hp       hps      + ;

: valid ( id -- flag )
	kind @ if true else false then
;

: alloc ( -- n )
	0 bogus-e kind !
	max-e 1 - loop
		dup valid -if break then
		1 -
	again
	(
	dup  kind    @ 0!
	over ready   @ 0! or
	over timer   @ 0! or
	over dir     @ 0! or
	over bullet  @ 0! or
	over parent  @ 0! or
	over hit     @ 0! or
	over hp      @ 0! or
	over sprite@ @ 0! or
	if
		"Stale record: " type dup . cr
		"	kind:    " type dup kind    @ . cr
		"	ready:   " type dup ready   @ . cr
		"	timer:   " type dup timer   @ . cr
		"	dir:     " type dup dir     @ . cr
		"	bullet:  " type dup bullet  @ . cr
		"	parent:  " type dup parent  @ . cr
		"	hit:     " type dup hit     @ . cr
		"	hp:      " type dup hp      @ . cr
		"   sprite@: " type dup sprite@ @ . cr
	then
	)
;

: free ( id -- )
	0 over kind    !
	0 over ready   !
	0 over timer   !
	0 over dir     !
	0 over bullet  !
	0 over parent  !
	0 over hit     !
	0 over hp      !
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
	8 * i py!
	8 * i px!
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

: find-entity ( 'pred -- id? flag )
	max-e 1 - for
		i over exec if drop r> true exit then
	next
	drop false
;

: clear-entities ( -- )
	' always ' free whoever
;
