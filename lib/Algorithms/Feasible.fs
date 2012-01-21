######################################################
##
##  Feasible:
##
##  A lexicon of lazy generators and higher-order
##  functions for spiffy functional programming.
##
##  John Earnest
##
######################################################

:include "../Print.fs"

:const gen-size 5
: .iter 0 + ;
: .prev 1 + ;
: .x    2 + ;
: .y    3 + ;
: .z    4 + ;

:const gen-count  100
:array generators 500 0

: nth-gen ( index -- gen* )
	gen-size * generators +
;

: alloc-gen ( -- gen* )
	gen-count 1 - for
		i nth-gen .iter @
		-if r> nth-gen exit then
	next
	"failed to allocate generator!" typeln halt
;

: free-gen ( gen* -- )
	#"freed: " type dup . cr
	dup .prev @ dup
	if free-gen else drop then
	gen-size 1 - for 0 over i + ! next drop
;

# take should be used whenever you wish to extract
# the next element from a generator.
# This word will automatically deallocate generators
# when they have completed.

: take ( gen* -- val? flag )
	dup -if drop false exit then
	>r i .iter @ dup
	-if drop rdrop false exit then
	i swap exec
	dup if rdrop else r> free-gen then
;

######################################################
##
##  Base generators
##
##  step: count by step, stopping when element = end
##  range: generate an inclusive range of numbers
##  count: infinite sequence of natural numbers
##  iterate: enumerate elements of an array
##
######################################################

# all these bracketed names are the internal
# guts of the corresponding generator.
# you never need to call them explicitly-
# 'take' takes care of that.

: [step] ( gen* -- val? flag )
	# x: index
	# y: end value
	# z: step value
	>r i .x @ i .y @ xor -if rdrop false exit then
	i .x @ dup i .z @ + r> .x ! true
;

: step ( start end step -- gen* )
	alloc-gen >r
	' [step] i .iter !
	i .z ! i .y ! i .x !
	r>
;

: range  1 + 1 step ; ( min max -- gen* )
: count  1 0 1 step ; ( -- gen* )

: [iterate] ( gen* -- val? flag )
	>r i .x @ i .y @ xor -if rdrop false exit then
	i .x @ dup 1 + r> .x ! @ true
;

: iterate ( array* length -- gen* )
	alloc-gen >r
	' [iterate] i .iter !
	over + i .y ! i .x !
	r>
;

######################################################
##
##  Transforming generators
##
##  map: apply a unary op to elements of a sequence
##  take-while: halt sequence if element lacks property
##  take-until: halt sequence if element has property
##  filter: skip elements that fail a predicate
##  chain: concatenate two sequences
##  slice: take the first n elements of a sequence
##
######################################################

: [map] ( gen* -- val? flag )
	>r i .prev @ take
	if i .x @ exec true else false then
	rdrop
;

: [take-while] ( gen* -- val? flag )
	>r i .prev @ take
	if dup i .x @ exec -if drop false else true then
	else false then
	rdrop
;

: [take-until] ( gen* -- val? flag )
	>r i .prev @ take
	if dup i .x @ exec if drop false else true then
	else false then
	rdrop
;

: [filter] ( gen* -- val? flag )
	>r loop
		i .prev @ take -if rdrop false exit then
		dup i .x @ exec if rdrop  true exit then
		drop
	again
;

: [chain] ( gen* -- val? flag )
	>r i .prev @ take
	if rdrop true exit then
	i .prev @ -if rdrop false exit then
	i .prev @ free-gen
	i .x @ i .prev ! 0 i .x !
	r> .prev @ take
;

: [slice] ( gen* -- val? flag )
	>r i .x @ -if rdrop false exit then
	i .x @ 1 - i .x !
	r> .prev @ take
;

# a handy word for assembling generators with two args
: build-bin ( gen* word* iter* -- gen* )
	alloc-gen >r i .iter ! i .x ! i .prev ! r>
;

# public words:
: map         ' [map]        build-bin ; ( gen* unary-op* -- gen* )
: take-while  ' [take-while] build-bin ; ( gen* pred*     -- gen* )
: take-until  ' [take-until] build-bin ; ( gen* pred*     -- gen* )
: filter      ' [filter]     build-bin ; ( gen* pred*     -- gen* )
: chain       ' [chain]      build-bin ; ( gen1* gen2*    -- gen* )
: slice       ' [slice]      build-bin ; ( gen* n         -- gen* )

######################################################
##
##  Reducing functions
##
##  length: count the elements of a sequence
##  last: final element of sequence or -1 if no elements
##  apply: apply a word to each element of a sequence
##  reduce: curry binary op over elements of a sequence
##  >array: save sequence in memory (be careful of bounds!)
##
######################################################

: length ( gen* -- count )
	0 loop
		over take
		-if swap drop exit then
		drop 1 +
	again
;

: last ( gen* -- count )
	-1 loop
		over take
		-if swap drop exit then
		swap drop
	again
;

: apply ( gen* word* -- )
	loop
		over take
		-if 2drop exit then
		over exec
	again
;

: reduce ( gen* start bin-op* -- end )
	>r loop
		over take
		-if swap r> 2drop exit then
		i exec
	again
;

: >array ( gen* addr -- )
	loop
		over take
		-if 2drop exit then
		over ! 1 +
	again
;

######################################################
##
##  Optional Goodies
##
######################################################

: show     ' . apply cr ; ( gen* -- )

:include "../Math.fs"

: upto     1 swap range            ; ( max -- gen* )
: sum      0 { + } reduce          ; ( gen* -- val )
: product  1 { * } reduce          ; ( gen* -- val )
: maximum  -infinity ' max reduce  ; ( gen* -- val )
: minimum  +infinity ' min reduce  ; ( gen* -- val )
: even?    2 mod if 0 else -1 then ; ( val -- flag )
: odd?     2 mod if -1 else 0 then ; ( val -- flag )

: [fibs] ( gen* -- val? flag )
	>r i .y @ i .x @ over + i .y ! i .x !
	r> .x @ true
;

: fibs ( -- gen* )
	alloc-gen >r
	' [fibs] i .iter !
	0 i .x ! 1 i .y ! r>
;

: [digits] ( gen* -- val? flag )
	>r i .x @ dup -if rdrop drop false exit then
	10 /mod swap r> .x ! true
;

# generates digits from least to most significant.
# this implementation only handles positive numbers
# because I am as lazy as these generating functions.
: digits ( n -- gen* )
	alloc-gen >r
	' [digits] i .iter !
	i .x ! r>
;

:include "Zalgo.fs"

: primes   2 0 range ' prime? filter ; ( -- gen* )


######################################################
##
##  Tests and examples
##
######################################################

# handy debugging functions
(
: .res
	"result: " type
	if   "true  " type .
	else "false " type then
	cr
;

: .gen
	"generator [ " type dup . "]:" typeln
	"  iter: " type dup .iter @ . cr
	"  prev: " type dup .prev @ . cr
	"     x: " type dup .x @ . cr
	"     y: " type dup .y @ . cr
	"     z: " type     .z @ . cr cr
;
)

:data testarr 12 34 255 89 -37 100
:array destarr 8 777

:include "../Util.fs"

: main
	
	# examples/functional tests:

	#5 8 range length . cr
	#5 9 range { . cr } apply
	#1 10 range 0 { + } reduce . cr # should be 55
	#1 10 range { 2 * } map show
	#count { 10 < } take-while show
	#count { 10 > } take-until show
	#1 10 range ' even? filter show
	#1 3 range 7 10 range chain show
	#count 5 slice length . cr # should be 5
	#testarr 6 iterate show
	#testarr 6 iterate destarr 1 + >array
	#destarr 8 iterate show
	#testarr 6 iterate maximum . cr # should be 255
	#testarr 6 iterate minimum . cr # should be -37
	#primes 20 slice show
	#fibs 20 slice show
	#1337 digits show

	# some project euler problems!

	# problem 1:
	999 upto { dup 3 mod 0 = swap 5 mod 0 = or } filter sum . cr

	# problem 2:
	fibs { 4000000 <= } take-while ' even? filter sum . cr

	# problem 5:
	# (can't go up to 20 due to overflow)
	10 upto 1 ' lcm reduce . cr

	# problem 6:
	100 upto sum dup *
	100 upto { dup * } map sum - . cr

	# problem 7:
	primes 10001 slice last . cr

	halt
;
