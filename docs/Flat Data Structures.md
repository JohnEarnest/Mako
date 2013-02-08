Flat Data Structures
====================

One of the first things programmers may miss when working on the Mako platform, or programming in Forth generally, is a readily-available set of general-purpose data structures. The lack of dynamic allocation in MakoVM means that any data structures must be statically-allocated and sized reasonably for their purpose. Thus, our structures will always begin with a flat array. This document is a cookbook of common solutions and reusable code fragments.

All the examples demonstrate implementations meant for operating on a single global structure. If you want to have multiple queues, for example, you can either rewrite the methods below to take a queue struct as an argument or replicate the example code with more specific word names. The correct solution will be dictated by your application.

To begin with, we'll assume a constant `size` has already been declared to indicate the number of elements our structures will contain. Additionally, we define a few common utility words:

	: inc    dup @ 1 + swap ! ; ( addr -- )
	: dec    dup @ 1 - swap ! ; ( addr -- )
	: on     true      swap ! ; ( addr -- )
	: off    false     swap ! ; ( addr -- )

Stack
-----
We already have two stacks, but a third can always be useful. Stacks are a handy and efficient way of managing a variable-sized collection of values. Here we use the index pointer to indicate the first free cell of the stack rather than the top element as it simplifies initialization and checking for emptiness. Adding bounds checks to `push` and `pop` are left as an exercise- for correct code they are often unnecessary overhead.

	:array   stack size 0
	:data    index stack
	: push   index @ ! index inc ; ( val -- )
	: pop    index dec index @ @ ; ( -- val )
	: peek   index @ 1 - @       ; ( -- val )
	: empty? index @ stack =     ; ( -- flag )

Obstack
-------
One place stacks are useful is a type of memory allocator called an "obstack". Chunks of memory can be allocated in slabs separated by calls to `mark`, and `free` will recover all the memory allocated since the last `mark`.

	:array   memory size 0
	:data    here   memory
	: alloc  here @ swap over + here ! ; ( size -- ptr )
	: mark   here @ push               ; ( -- )
	: free   pop here !                ; ( -- )

If we wanted a very simple way of performing list operations, we could implement `cons`, `car` and `cdr` using this allocator as follows: 

	: cons 2 alloc dup >r 1 + ! i ! r> ; ( car cdr -- pair )
	: car      @                       ; ( pair -- car )
	: cdr  1 + @                       ; ( pair -- cdr )

Then it's simply a matter of enclosing code which operates on these pairs between calls to `mark` and `free`. It's important to ensure there will be sufficient free space in the obstack for the entire sequence of operations, though, as consumed pairs will not be automatically freed.

ArrayList
---------
A common twist on stacks is the ArrayList, which allows arbitrary removal of elements, compacting the structure as elements are removed. Compaction is somewhat expensive though, and most of the time this generality is not necessary. Much like in the stack, we use the `index` pointer to track the address of the first available cell. The implementation of `add` is actually identical to that of `push`.

	:array    list size 0
	:data     index list
	: length  index @   list -    ; ( -- n )
	: add     index @ ! index inc ; ( val -- )
	: get     list + @            ; ( pos -- val )
	: set     list + !            ; ( val pos -- )
	
	: remove ( pos -- )
		loop
			dup length = if break then
			dup 1 + get over set
			1 +
		again drop
		index dec
	;

If we're frequently iterating over the elements of the list we can construct a combinator for abstracting the associated loop pattern. Note that in this example I've gone the extra mile of making the combinator walk over elements first-to-last, rather than the slightly simpler last-to-first.

	: foreach ( proc -- )
		length 1 < if drop exit then
		>r length 1 - for
			length 1 - i - get j exec
		next rdrop
	;

Free List
---------
An alternative to the ArrayList is a Free List, where cells of the array which are not 'valid' contain a chain of indices to unoccupied cells, providing constant-time insertions and constant-time removals, given some storage overhead. The main tricky bit here is that valid elements are no longer guaranteed to be contiguous, so iterating over them requires extra logic. As written this will also fail if the list becomes completely full. If you know something about the values you'll be storing in the list it is possible to avoid the "used" array entirely and encode valid/invalid status into flag bits of values. `get` and `set` can be defined as in the ArrayList, but they should always be used in combination with a check to `valid?`.

	:array    list size 0
	:array    used size false
	:var      free
	: valid?  used + @ ; ( pos -- flag )
	
	: init ( -- )
		size 2 - for
			i dup 1 + swap list + !
		next
	;

	: add ( val -- )
		free @ dup list + @ free !
		dup used + on
		list + !
	;
	
	: remove ( pos -- )
		dup used + off
		free @ over list + !
		free !
	;

For comparison, here's a `foreach` combinator that works with a Free List- as noted, we can't ensure any particular iteration order:

	: foreach ( proc -- )
		>r size 1 - for
			i valid? if i get j exec then
		next rdrop
	;

Queue
-----
Augmenting an array with two counters modulo the array size allows us to construct a simple circular buffer. We use indices rather than pointers for tracking the first and last element of the queue as we will always need to increment these counters modulo the queue size- see `wrap`. Keeping track of the number of elements in the queue seperately is necessary if we are to distinguish between a completely full queue and a completely empty queue. `push` and `pop` can be simplified slightly if this is not necessary.

	:array   queue size 0
	:var     head
	:var     tail
	:var     length
	: wrap   dup @ 1 + size mod swap !             ; ( addr -- )
	: push   head @ queue + ! head wrap length inc ; ( val -- )
	: pop    tail @ queue + @ tail wrap length dec ; ( val -- )
	: empty? length @ 0 =                          ; ( -- flag )

Binary Trees
------------
Given a 1-indexed position in an array with a power-of-two size, we can apply a classic trick to compute the positions of relative left/right/parent positions as if the array were a complete binary tree. This approach is often applied to implementations of HeapSort.

	: left    2 *     ; ( index -- index' )
	: right   2 * 1 + ; ( index -- index' )
	: parent  2 /     ; ( index -- index' )
	: root?   1 =     ; ( index -- flag )

Linear Map
----------
For small associative collections, don't be afraid to use a simple linear lookup strategy. A tight search loop can be fast enough for many applications. Dropping the "values" array can easily convert this map implementation into a set. In this implementation `put` assumes you have already checked that the set does not contain your key. Note the use of a variable stack effect in `get`- this "success flag" strategy is a good example of how variable stack effects can be gainfully employed.

	:array   names  size 0
	:array   values size 0
	:var     length

	: put ( name val -- )
		length @ values + !
		length @ names  + !
		length inc
	;

	: get ( name -- value true | false )
		length @ -if drop false exit then
		length @ 1 - for
			dup i names + @ =
			if drop r> values + @ true exit then
		next false
	;

	: contains?  get dup if nip then ; ( name -- flag )

And an iteration combinator:

	: foreach-entry ( proc -- )
		length @ -if drop exit then
		>r length @ 1 - for
			i names  + @
			i values + @
			j exec
		next rdrop
	;

Bitset:
-------
Simple lookup tables containing `true` or `false` are another straightforward way to represent sets. This can be a little wasteful, since each 32-bit cell stores only 1 bit of useful information. When space is more important than time we can bit-pack values:

	:array  bitset size 0

	: bit        1 over if for 2 * next else + then ; ( n -- mask )
	: bit-index  dup 32 / bitset + swap 32 mod bit  ; ( index -- addr mask ) 
	: >flag      if true else false then            ; ( n -- flag )

	: bit@ ( index -- flag )
		bit-index swap @ and >flag
	;

	: bit! ( flag index -- )
		bit-index >r swap i and over @
		r> not and or swap !
	;

`bit` as given here is rather slow due to the fact that Mako lacks bitshift operators. A compromise between speed and space may be obtained by replacing `bit` as given with a lookup table of bitmasks. If there are 32 or fewer possible set elements it can be even more convenient to apply this approach to values on the stack and simply pass them around.

