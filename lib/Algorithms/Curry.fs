######################################################
##
##  Curry:
##
##  A mechanism which uses dynamically allocated
##  code fragments to allow currying of procedures
##  as well as some closure-like functionality.
##
##  This system requires <Algorithms/Garbage.fs>
##
##  John Earnest 
##
######################################################

: p>?         dup p? if ptr> then ; ( ptr? | value -- value )
: >code       over ! 1 +          ; ( addr val -- addr+1 )
:vector exec  p>?       >r        ; # call an address
:vector goto  p>? rdrop >r        ; # jump to an address

######################################################
##
##  Value converts a value on the stack into a
##  passable, executable code fragment which pushes
##  that value onto the stack. Compose takes two
##  procedures and yields a procedure which has the
##  effect of executing the source procedures in
##  sequence. Curry takes a value from the stack
##  and a procedure and yields a procedure which
##  pushes the value to the stack and then executes
##  the original procedure.
##
######################################################

:include <Assembly.fs>

: value ( n -- q )
	5 alloc dup >r ptr>
	CONST  >code
	swap   >code ( n )
	CALL   >code
	' p>?  >code
	RETURN >code
	drop r>
;

: compose ( q1 q2 -- compose )
	swap
	8 alloc dup >r ptr>
	CONST  >code
	swap   >code ( p1 )
	CALL   >code
	' exec >code
	CONST  >code
	swap   >code ( p2 )
	CALL   >code
	' goto >code
	drop r>
;

: curry ( value q -- curry )
	swap value swap compose
;

######################################################
##
##  A Shard is an executable code fragment with
##  an associated cell of memory. Executing a shard
##  yields a pointer to this cell. By composing
##  shards with procedures you in effect form a 
##  closure around the variable the shard represents.
##  Byref is a mechanism for creating a shard pointing
##  to the top element of the stack (before calling
##  byref), but can only be used when you are passing
##  a closure/procedure to "deeper" procedures.
##
######################################################

: shard       1 alloc swap over p! value ; ( n -- q )
: with        shard swap compose         ; ( q n -- compose )
: byref       DP @ 1 - value             ; ( n -- n q )

######################################################
##
##  Looping combinators
##
##  These are written such that, from the perspective
##  of a quotation, the combinator arguments are
##  removed from the stack when the combinator is
##  invoked. This is consistent with how combinators
##  function in Factor, for example.
##
######################################################

: repeat ( q count -- )
	dup -if 2drop exit then
	1 - for dup >r exec r> next drop
;

: each ( q [ addr ] base count -- )
	1 - for
		dup >r j +
		swap dup >r
		exec r> r>
	next 2drop
;