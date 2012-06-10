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
##  To use the entity system you must also first
##  define 'ent-max', the maximum sprite index that
##  will be used and a few prototypes as noted below.
##
##  Finally, a number of vectored words and
##  configurable defaults are available for
##  customizing the behavior of this system:
##
##  - spawn-size (default size of a spawned entity)
##  - -sprites (control sorting order)
##  - seek
##  - distance
##
##  John Earnest
##
######################################################

:proto ent-clear   ( id -- )
:proto ent-swap    ( a b -- )
:proto ent-blocked ( id -- flag )

######################################################
##
##  Allocation system
##
######################################################

:const bogus-ent -1
0 :array kinds    ent-max 0 : kind   kinds    + ;
0 :array readies  ent-max 0 : ready  readies  + ;
0 :array timers   ent-max 0 : timer  timers   + ;
0 :array dirs     ent-max 0 : dir    dirs     + ;
0 :array prevs    ent-max 0 : prev   prevs    + ;
0 :array solids   ent-max 0 : solid  solids   + ;

: valid  kind @ 0 != ; ( id -- flag )

: reset ( id -- )
	dup ent-clear
	0 over ready   !
	0 over timer   !
	0 over dir     !
	0 over prev    !
	0 over solid   !
	0 swap sprite@ !
;

: free ( id -- )
	0 over kind    !
	0 swap sprite@ !
;

: alloc ( -- n )
	0 bogus-ent kind !
	ent-max 1 - loop
		dup valid -if dup reset break then
		1 -
	again
;

:data spawn-size 16x16

: spawn ( tile-x tile-y tile 'func -- id )
	alloc >r
	i kind ! i tile!
	spawn-size @ i sprite@ !
	8 * i py!
	8 * i px!
	r>
;

: spawn-rel ( delta-x delta-y id tile 'func -- id )
	alloc >r
	i kind ! i tile!
	spawn-size @ i sprite@ !
	>r
	i  py + j py!
	r> px + i px!
	r>
;

######################################################
##
##  High-level entity management
##
######################################################

: whoever ( 'filter 'func -- )
	>r >r
	0 loop
		dup i exec over valid and
		if dup j exec then
		1 + dup ent-max <
	while
	drop r> r> 2drop
;

: apply-kind ( id -- )
	dup ready @ -if true swap ready ! exit then
	dup kind @ exec
;

: count+          drop swap 1 + swap            ;
: count           0 swap ' count+ whoever       ; ( 'filter -- )
: always          drop true                     ;
: think           ' always ' apply-kind whoever ; ( -- )
: clear-entities  ' always ' free whoever       ; ( -- )

: find-entity ( 'pred -- id? flag )
	ent-max 1 - for
		i over exec if drop r> true exit then
	next
	drop false
;

# positive if a>b, negative if a<b.
:vector -sprites ( a b -- )
	py swap py swap -
;

: swap@  2dup @ >r @ swap ! r> swap ! ;

# sort sprites from smallest to largest,
# and thus from back to front.
: sort-sprites ( -- )
	ent-max 1 - for
		i
		i for
			dup i -sprites 0 <
			if drop i then
		next

		# don't swap with ourselves:
		dup i xor if
			# swap entity data
			dup kind  i kind  swap@
			dup ready i ready swap@
			dup timer i timer swap@
			dup dir   i dir   swap@
			dup prev  i prev  swap@
			dup solid i solid swap@
			dup i ent-swap
	
			# swap sprite registers:
			sprite@ i sprite@
			3 for
				over i +
				over i +
				swap@
			next
			drop
		then
		drop
	next
;

######################################################
##
##  Movement and pathfinding logic
##
######################################################

: opp  4 + 8 mod ; ( dir -- dir )
: lf   2 - 8 mod ; ( dir -- dir )
: rt   2 + 8 mod ; ( dir -- dir )

:const n 0 :const ne 1
:const e 2 :const se 3
:const s 4 :const sw 5
:const w 6 :const nw 7

:data dir-x   0  1  1  1  0 -1 -1 -1 : delta-x dir-x + @ ;
:data dir-y  -1 -1  0  1  1  1  0 -1 : delta-y dir-y + @ ;

: change ( src target -- dx dy )
	2dup py swap py - >r
	     px swap px - r>
;

# find the closest direction
# for moving from src to target
:vector seek ( src target -- dir )
	change
	over  2 * over >= 2 and 1 - >r
	over over  2 * < 1 and >r
	over over -2 * > 1 and r> + >r
	over -2 * over < 1 and r> + r> * >r
	swap 2 *       > 7 and r> +
;

# seek only in orthogonal directions
: ortho ( src target -- dir )
	change
	# n^2 works as well as abs(n):
	over dup * over dup * > if
	drop      0 > -4 and 6 + else
	swap drop 0 >  4 and     then
;

# seek only in diagonal directions
: diago ( src target -- dir )
	change 0 > if
	0 > -2 and 5 + else
	0 > -6 and 7 + then
;

:vector distance ( a b -- dist )
	# yields distance^2 by default
	over px over px - dup * >r
	     py swap py - dup * r> +
;

: nearest ( src ' filter -- )
	>r >r bogus-ent +infinity
	0 loop
		dup valid over i != and if dup j exec if
			dup i distance >r over r> swap over >
			if >r >r 2drop r> r> over else drop then
		then then
		1 + dup ent-max <
	while
	2drop rdrop rdrop
;

: offscreen? ( id -- flag )
	>r
	i  px i sprite@ .sprite-w -1 * <
	i  py i sprite@ .sprite-h -1 * < or
	i  px 320 > or
	r> py 240 > or
;

: move ( magnitude id -- )
	over over dir @ delta-x * over +px
	swap over dir @ delta-y * swap +py
;

: blocked ( id -- flag )
	dup ent-blocked if drop true then
	0 loop
		2dup != over valid and over solid @ and if
			2dup c-sprites? if 2drop true exit then
		then
		1 + dup ent-max <
	while
	2drop false
;

: c-move ( magnitude id -- )
	2dup move
	dup blocked if over -1 * over move then
	2drop
;

######################################################
##
##  Reusable behaviors
##
######################################################

: [waiting] ( id -- )
	>r
	i timer @ 1 - i timer !
	i timer @ -if dup prev @ 0 i prev ! i kind ! then
	rdrop
;

: make-wait ( ticks id -- )
	>r
	i timer !
	i kind @ i prev !
	' [waiting] r> kind !
;

: [walking] ( id -- )
	>r
	i [waiting]
	1 i move
	rdrop
;

: make-walk ( dir id -- )
	>r
	i dir !
	8 i timer !
	i kind @ i prev !
	' [walking] r> kind !
;