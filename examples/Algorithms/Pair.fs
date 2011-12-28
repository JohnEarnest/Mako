######################################################
##
##  Pair:
##
##  An implementation of Lisp-style cons-pairs for
##  Forth, based around a mark-and-sweep garbage
##  collection system, and list manipulation utility
##  words.
##
##  This system uses two high-order bits to identify
##  pointers to pairs for list traversal and garbage
##  collection purposes. The constant 'true' and
##  any fully-opaque or fully-transparent colors will
##  not be falsely identified as a pair, but caution
##  should be taken with having other potential
##  collisions on the stacks or in managed memory
##  during list operations.
##
##  John Earnest
##
######################################################

:include "../Print.fs"

:const nil       -2147483648
:const pair-mask 0x60000000
:const pair-flag 0x40000000
:const pair-hole 0x20000000

: nil? ( val -- flag )
	nil xor -if true else false then
;

: pair? ( addr -- flag )
	dup pair-flag and swap
	not pair-hole and or
	    pair-mask xor
	if false else true then
;

: atom? ( addr -- flag )
	dup nil? not swap pair? not and
;

: .reach pair-mask not and     ;
: .first pair-mask not and 1 + ;
: .rest  pair-mask not and 2 + ;

: raw? ( addr -- flag )
	dup pair? if .rest @ atom?
	else drop false then
;

######################################################
##
##  Garbage Collector:
##  
##  Scans the data and return stacks as well as
##  a specially declared region of 'managed memory'
##  to find pair pointers. Can handle cyclic references.
##
##  Garbage collection will occur automatically when
##  pair operations need to allocate a pair and no
##  heap cells are free. Only gc-init and free-cells
##  are meant to be called explicitly.
##
######################################################

:const heap-size 500
:array heap 1500 0

:proto managed-begin
:proto managed-end

:var data-min
:var data-max
:var return-min
:var return-max

: gc-init ( -- )
	DP @       data-min !
	RP @ 1 - return-min !
;

: free-cells ( -- count )
	0 heap-size 1 - for
		i 3 * heap + @ -if 1 + then
	next
;

: walk ( pair* -- )
	dup .reach @ if drop exit then
	dup .reach true swap !
	dup .first @ dup pair? if walk else drop then
	    .rest  @ dup pair? if walk else drop then
;

: scan ( min max -- )
	over -
	for
		dup i + @ dup pair? if walk
		else drop then
	next
	drop
;

: collect ( -- )
	#"collecting garbage..." typeln
	DP @   data-max !
	RP @ return-max !
	free-cells
	heap-size 1 - for 0 i 3 * heap + ! next
	data-min   @   data-max @ scan
	return-min @ return-max @ scan
	' managed-begin ' managed-end scan
	free-cells swap - 1 <
	if "failed to free space." typeln halt then
;

: new-pair ( -- pair* )
	heap-size 1 - for
		i 3 * heap + dup @ -if
			pair-flag or
			1 over .reach ! rdrop exit
		then
		drop
	next
	collect new-pair
;

######################################################
##
##  Pair Operations:
##  
##  Equivalent to cons, car, cdr, set-car! and set-cdr!,
##  respectively, except with names that make sense.
##
######################################################

: pair ( first rest -- pair* )
	new-pair >r
	i .rest  ! i .first ! r>
;

: first   .first @             ; ( pair* -- first )
: rest     .rest @             ; ( pair* --  rest )
: first!  .first !             ; ( value pair* -- )
: rest!    .rest !             ; ( value pair* -- )
:  split  dup first swap  rest ; ( pair* -- first rest )
: -split  dup  rest swap first ; ( pair* -- rest first )

######################################################
##
##  List utilities:
##
##  These are mostly recursive, so if you
##  intend to use them for more than toy examples
##  it would be a good idea to crank up the size
##  of the data and return stacks.
##
######################################################

: list-build ( nil ... -- pair* )
	nil loop
		pair over nil?
	until
	swap drop
;

: [.atom] ( val -- )
	dup nil? if drop "nil " type else . then
;

: list-print ( pair* -- )
	dup pair? -if [.atom] exit then
	"[ " type
	dup raw? if
		-split [.atom] ". " type .
	else
		loop
			-split
			dup pair? if list-print else [.atom] then
			dup raw?  if list-print break then
			dup nil?  if drop       break then
		again
	then
	"] " type
;

: list-join ( first* second* -- pair* )
	over nil? if swap drop exit then
	>r split r> list-join pair
;

: list-flatten ( pair* -- pair* )
	dup nil?  if exit then
	dup raw?  if nil pair exit then
	dup atom? if nil pair exit then
	split >r list-flatten
	r> list-flatten list-join
;

: list-reverse ( pair* -- pair* )
	dup nil? if exit then
	split list-reverse swap nil pair list-join
;

: list-length ( pair* -- count )
	1 swap rest dup nil? if drop exit then
	list-length +
;

: list-nth ( pair* n -- value )
	over nil? if "bad list index!" typeln halt then
	dup -if drop first exit then
	1 - swap rest swap list-nth
;

: list-last ( pair* -- value )
	dup rest nil? if first exit then
	rest list-last
;

: list-butlast ( pair* -- pair* )
	dup rest nil? if drop nil exit then
	split list-butlast pair
;

: list-equal? ( pair* pair* -- flag )
	over nil? over nil? or
	if over nil? over nil? and >r 2drop r> exit then
	over first over first xor if 2drop false exit then
	rest swap rest list-equal?
;

: list-apply ( pair* func* -- )
	over nil? if 2drop exit then
	>r -split i exec r> list-apply
;

: list-map ( pair* func* -- pair* )
	over nil? if drop exit then
	>r -split i exec swap r>
	list-map pair
;

: list-reduce ( pair* start bin-op* -- end )
	>r over nil? if swap r> 2drop exit then
	>r -split r> i exec r> list-reduce
;

: list-filter ( pair* pred* -- pair* )
	over nil? if drop exit then
	over first over exec
	if   >r split r> list-filter pair
	else >r rest  r> list-filter then
;

: list-zip ( a* b* -- pair* )
	over nil? over nil? or if 2drop nil exit then
	over first over first pair
	swap rest >r swap rest r>
	list-zip pair
;

: list-zipwith ( pair* n -- pair* )
	over nil? if drop exit then
	>r -split i pair swap r>
	list-zipwith pair
;

: list-cross ( a* b* -- pair* )
	dup nil? if swap drop exit then
	-split >r over r> list-zipwith
	swap >r swap r>
	list-cross pair
;

######################################################
##
##  Tests and examples
##
######################################################

# a little sugar for mitigating
# any remaining Lisp envy:
: [ nil        ;
: ] list-build ;
: . list-print ;

# managed memory- any persistent storage
# which might reference a pair must be wrapped
# in these markers.
: managed-begin ;

:var A
:var B

: managed-end ;

: main
	# this routine must be called before
	# using any list-manipulation operations-
	# it caches the base positions of the stacks
	# so they can be scanned later.
	gc-init

	666 # stack 'canary'

	1 2 pair pair? .
	nil      pair? .
	true     pair? .
	false    pair? . cr

	[ 1 [ 2 3 ] 4 5 pair 6 ]             . cr
	77 33 44 nil pair pair pair          . cr
	5 4 3 pair pair                      . cr
	nil 1 pair                           . cr
	[ 1 2 3 [ 4 5 6 ] 7 [ 8 [ 9 10 ] ] ] . cr
	[ 3 4 5 ] A !
	[ 5 4 3 ] B !
	[ A @ B @ A @ B @ ] . cr

	[ 9 8 7 ] [ 6 5 4 ] list-join . cr

	[ 9 8 7 6 5 ]         list-flatten . cr
	[ 9 [ [ 8 ] 7 ] 6 5 ] list-flatten . cr
	[ 1 2 pair 3 4 pair ] list-flatten . cr

	[ 1 2 3 4 5 ] list-reverse . cr

	[ 1 ]     list-length .
	[ 1 2 ]   list-length .
	[ 1 2 3 ] list-length . cr

	[ 4 5 6 ]
	dup 0 list-nth .
	dup 1 list-nth .
	    2 list-nth . cr

	[ 23 45 99 ] list-last . cr
	[ 23 45 99 ] list-butlast . cr

	[ 1 2 3 ] [ 1 2 ]   list-equal? .
	[ 1 2 3 ] [ 1 5 3 ] list-equal? .
	[ 1 2 3 ] [ 1 2 3 ] list-equal? . cr

	[ 1 1 2 3 5 8 13 ] { . ", " type } list-apply cr
	[ 1 2 3 4 ] { 3 * } list-map . cr
	[ 1 2 3 4 ] 0 { + } list-reduce . cr

	[ 5 4 3 2 1 ] { drop true }  list-filter . cr
	[ 5 4 3 2 1 ] { drop false } list-filter . cr 
	[ 1 2 3 4 5 ] { 2 mod }      list-filter . cr
	[ 1 2 3 4 5 ] { 1 + 2 mod }  list-filter . cr

	[ 1 2 3 ] [ 4 5 6 ] list-zip . cr
	[ 1 2 ] [ 3 4 5 6 ] list-zip . cr
	[ 9 8 7 ] 88 list-zipwith    . cr
	[ 1 2 3 ] [ 4 5 ] list-cross . cr

	. cr # print the canary

	halt
;