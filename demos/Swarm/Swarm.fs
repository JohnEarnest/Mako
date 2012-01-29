######################################################
##
##  Swarm:
##
##  A demonstration of using the Entities library
##  to easily produce game simulations
##  with complex behaviors.
##
##  John Earnest
##
######################################################

:image sprite-tiles "swarmy.png" 16 16
:const clear-color 0xFFFFFFFF
:const ent-max 32
:include <Sprites.fs>
:include <Game/Entities.fs>
: ent-clear    drop  ;
: ent-swap     2drop ;
: ent-blocked  drop false ;

: toroid ( id -- )
	>r
	i px -16 < if  336 i +px then
	i py -16 < if  256 i +py then
	i px 319 > if -320 i +px then
	i py 239 > if -240 i +py then
	rdrop
;

: player ( id -- )
	>r
	keys key-up and if n i dir ! then
	keys key-dn and if s i dir ! then
	keys key-lf and if w i dir ! then
	keys key-rt and if e i dir ! then
	keys key-up key-lf or = if nw i dir ! then
	keys key-up key-rt or = if ne i dir ! then
	keys key-dn key-lf or = if sw i dir ! then
	keys key-dn key-rt or = if se i dir ! then
	r> 2 over c-move
	toroid
;

: follower ( id -- )
	>r
	i {
		kind @ dup
		' player   = swap
		' follower = or
	} nearest
	1 i tile!
	i over distance 16 dup * < if i over seek opp i dir ! 0 i tile! then
	i over distance 48 dup * > if i over seek     i dir ! 2 i tile! then
	drop
	r> 1 over c-move
	toroid
;

: block drop ;
: pos   RN @ 40 mod RN @ 30 mod ;

: main
	20 15 3 ' player spawn drop
	12 for
		pos 1 ' follower spawn drop
		pos 7 ' block    spawn solid true swap !
	next

	loop
		think
		sync
	again
;