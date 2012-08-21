:include <Test/Tap.fs>

:const heap-size 500
:include <Algorithms/Garbage.fs>
:include <Algorithms/Curry.fs>

: managed-begin ;
:var A
: managed-end   ;

: pinc     ptr> dup @ 1 + swap !   ; ( ptr -- )
: counter  { dup pinc p@ } -1 with ; ( -- q )

: main
	gc-init
	9 plan
	666 

	374 value exec 374 = "value 1" ok
	-96 value exec -96 = "value 2" ok

	{ 2 } { 3 } { + } compose compose exec 5 = "compose" ok

	9 { 2 + } curry exec 11 = "curry" ok

	counter
	dup exec 0 = "with 1" ok
	dup exec 1 = "with 2" ok
	dup exec 2 = "with 3" ok

	A ! 1000 for "bogus" value drop next
	A @ exec 3 = "GC pressure" ok

	# check canary
	666 = "canary" ok
;