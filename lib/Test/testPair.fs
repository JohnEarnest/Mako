:include <Test/Tap.fs>
:include <Algorithms/Pair.fs>

: managed-begin ;
:var A
:var B
: managed-end ;
:var C

: main
	35 plan 666

	# this routine must be called before
	# using any list-manipulation operations-
	# it caches the base positions of the stacks
	# so they can be scanned later.
	gc-init

	1 2 pair pair? true  = "pair1" ok
	nil      pair? false = "pair2" ok
	true     pair? false = "pair3" ok
	false    pair? false = "pair4" ok

	[ 1 2 3 ] [ 1 2 ]   list-equal? false = "equal1" ok
	[ 1 2 3 ] [ 1 5 3 ] list-equal? false = "equal2" ok
	[ 1 2 3 ] [ 1 2 3 ] list-equal? true  = "equal3" ok

	[ 1 2 3 pair ] [ 1 2 4 pair ] list-dequal? false = "dequal1" ok
	[ 1 2 3 pair ] [ 1 2 3 pair ] list-dequal? true  = "dequal2" ok

	[ 3 4 5 ] A !
	[ 5 4 3 ] B !
	[ A @ B @ A @ B @ ]
	[ [ 3 4 5 ] [ 5 4 3 ] [ 3 4 5 ] [ 5 4 3 ] ] list-dequal? "equal4" ok

	[ 9 8 7 ] [ 6 5 4 ]   list-join    [ 9 8 7 6 5 4 ] list-equal? "join" ok
	[ 9 8 7 6 5 ]         list-flatten [ 9 8 7 6 5 ]   list-equal? "flatten1" ok
	[ 9 [ [ 8 ] 7 ] 6 5 ] list-flatten [ 9 8 7 6 5 ]   list-equal? "flatten2" ok
	[ 1 2 pair 3 4 pair ] dup list-flatten             list-dequal? "flatten3" ok

	[ 1 2 3 4 5 ] list-reverse [ 5 4 3 2 1 ] list-equal? "reverse" ok

	[ 1 ]     list-length 1 = "length1" ok
	[ 1 2 ]   list-length 2 = "length2" ok
	[ 1 2 3 ] list-length 3 = "length3" ok

	[ 4 5 6 ]
	dup 0 list-nth 4 = "nth1" ok
	dup 1 list-nth 5 = "nth2" ok
	    2 list-nth 6 = "nth3" ok

	[ 23 45 99 ] list-last                     99 = "last"    ok
	[ 23 45 99 ] list-butlast [ 23 45 ] list-equal? "butlast" ok

	[ 1 1 2 3 5 8 13 ] { C @ + C ! } list-apply C @      33 = "apply"  ok
	[ 1 2 3 4 ] { 3 * } list-map    [ 3 6 9 12 ] list-equal?  "map"    ok
	[ 1 2 3 4 ] 0 { + } list-reduce                      10 = "reduce" ok

	[ 5 4 3 2 1 ] { drop true }  list-filter [ 5 4 3 2 1 ] list-equal? "filter1" ok
	[ 5 4 3 2 1 ] { drop false } list-filter nil           list-equal? "filter2" ok
	[ 1 2 3 4 5 ] { 2 mod }      list-filter [ 1 3 5 ]     list-equal? "filter3" ok
	[ 1 2 3 4 5 ] { 1 + 2 mod }  list-filter [ 2 4 ]       list-equal? "filter4" ok

	[ 1 2 3 ] [ 4 5 6 ] list-zip [ 1 4 pair 2 5 pair 3 6 pair ]    list-dequal? "zip1" ok
	[ 1 2 ] [ 3 4 5 6 ] list-zip [ 1 3 pair 2 4 pair ]             list-dequal? "zip2" ok
	[ 9 8 7 ] 88 list-zipwith    [ 9 88 pair 8 88 pair 7 88 pair ] list-dequal? "zip3" ok

	[ 1 2 3 ] [ 4 5 ] list-cross 
	[ [ 1 4 pair 2 4 pair 3 4 pair ] [ 1 5 pair 2 5 pair 3 5 pair ] ] list-dequal? "cross" ok

	666 = "canary" ok
;
