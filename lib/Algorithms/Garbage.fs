######################################################
##
##  Garbage:
##
##  An implementation of a general-purpose garbage
##  collector based on Cheney's algorithm. Pointers
##  within allocated memory regions must be swizzled
##  and manipulated using the provided 'p@', 'p!'
##  and 'p?' words. User code should avoid using
##  '>ptr' and 'ptr>' conversion words explicitly
##  as they may allow memory to be accidentally
##  reclaimed.
##
##  John Earnest
##
######################################################

:include <Print.fs>

:const ptr-mask 0x60000000
:const ptr-bits 0x9fffffff
:const ptr-flag 0x40000000

: >ptr  ptr-flag or             ; ( addr -- ptr )
: ptr>  ptr-bits and            ; ( ptr -- addr )
: p?    ptr-mask and ptr-flag = ; ( n -- flag )
: p!    ptr> !                  ; ( n ptr -- )
: p@    ptr> @                  ; ( ptr -- n )
: ps    ptr> 1 - @              ; ( ptr -- size )

######################################################
##
##  The GC proper
##
######################################################

:const heap-size 500
:array heap1 heap-size 0
:array heap2 heap-size 0
:proto managed-begin
:proto managed-end

:var data-min
:var data-max
:var return-min
:var return-max

:data from heap1
:data   to heap2
:data head heap1

: broken?  ps 0 < ; ( ptr -- flag )

: >move ( src dst len -- )
	"move src: " type >r over . r>
	     "dst: " type over .
	     "len: " type dup  . cr

	1 - for
		over i + @ over i + !
	next 2drop
;

: gc-copy ( ptr -- )
	dup broken? if drop exit then
	dup ptr> 1 - head @ over @ 1 + >move # copy the object
	    head @ 1 + >ptr over p!          # leave behind link to new location
	dup ps 1 + head @ + head !           # increment the free pointer
	ptr> 1 - dup @ -1 * swap !           # break a heart
;

: gc-walk ( -- )
	to @ head @ >= if exit then
	to @ loop
		dup @ p? if
			dup @ gc-copy
			dup @ p@ over !
		then
		1 + dup head @ <
	while drop
;

: gc-scan ( min max -- )
	over - for
		dup i + @ dup p? if
			dup gc-copy
			p@ over i + !
		else drop then
	next drop
;

: gc ( -- )
	"collecting garbage..." typeln

	DP @   data-max !
	RP @ return-max !
	to @ head !

	  data-min      @   data-max    @ gc-scan
	  return-min    @   return-max  @ gc-scan
	' managed-begin   ' managed-end   gc-scan
	
	gc-walk
	from @ to @ from ! to !
;

: gc-init ( -- )
	DP @       data-min !
	RP @ 1 - return-min !
;

: enough?  head @ + 1 + from @ heap-size + <= ; ( size -- flag )

: alloc ( size -- ptr )
	dup enough? -if gc then
	dup enough? -if "heap exhausted!" typeln halt then

	>r
	  head @ 1 + >ptr       # calculate new pointer
	i head @ !              # store block size
	  head @ 1 + i + head ! # update free index
	rdrop
;

######################################################
##
##  Tests
##
######################################################

(
: managed-begin ;
:var A
:var B
: managed-end   ;

:var array-size
: [ 0 array-size ! ;
: , array-size @ 1 + array-size ! ;
: ]array
	,
	array-size @ alloc ptr>
	array-size @ 1 - + >r
	array-size @ 1 - for
		j ! r> r> 1 - >r >r
	next r> 1 + >ptr
;

: .array
	"[ " type
	dup ps 1 - for
		dup p@ .
		1 +
	next drop
	"]" type
;

: main
	gc-init
	
	[ 666 ]array B !
	[ B @ , 60 ]array A !
	0 B !
	A @ ptr> . cr

	[ 1 , 2 , 3     ]array .array cr
	[ 9 , 8 , 7 , 6 ]array .array cr
	[ 4 , 5 , 4 , 5 ]array .array cr	
	[ 4 , 5 , 4 , 5 ]array .array cr

	A @ .array cr
	A @ p@ .array cr
	A @ ptr> . cr
;
)