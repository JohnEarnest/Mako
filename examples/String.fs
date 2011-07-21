######################################################
##
##  String:
##
##  A lexicon for manipulating Strings and reading
##  in data from the debug port.
##
##  John Earnest
##
######################################################

: fill   >r 1 - for j over i + ! next r> 2drop ; (addr len n  --)
: >move  1 - for over i + @ over i + ! next    ; (src dst len --) # copies high to low

: <move (src dst len --) # copies low to high
	>r swap dup r> + >r
	loop
		dup i >= if r> 2drop drop exit then
		dup >r @ over ! r>
		1 + swap 1 + swap		
	again
;

: size (addr x -- len)
	>r 0
	loop
		over @ i xor -if swap r> 2drop exit then
		1 + swap 1 + swap
	again
;

# compare two null-terminated strings.
# positive if a>b, negative if a<b.
: -text (a b -- flag)
	loop
		over @ over @
		2dup - if - >r 2drop r> exit then
		and -if 2drop 0 exit then
		1 + swap 1 + swap
	again
;

: number (addr -- n)
	0
	over @ 45 xor -if swap 1 + swap -1 else 1 then >r
	loop
		over @ -if swap drop r> * exit then
		10 * over @ 48 - + swap 1 + swap
	again
;

:vector key   CO @ ;

:const pad-size 255
:array pad      256 0

# read in n chars or until return and
# store them to the pad, null-terminated.
: expect (n -- )
	pad + 1 - >r pad 1 -
	loop
		1 + key
		dup 10 xor -if drop 1 - break then
		over ! dup i <
	while
	0 swap 1 + ! r> drop
;

# read in chars until we hit a delimiter,
# storing a null-terminated string in the pad.
: word (c -- )
	>r pad 1 -
	loop
		1 + key
		dup i xor -if drop 1 - break then
		over !
	again
	0 swap 1 + ! r> 2drop
;

(
# usage examples:
:include "Print.fs"

:string str1 "---hello---"
:string str2 "A sentence."

:string a "aardvark"
:string b "aa"
:string c "blue"

:string n1 "123"
:string n2 "-2748"
:string n3 "0"
:string n4 "2"

: main
	str1 typeln                # should be '---hello---'
	str1 3 + str1 6 + 5 >move
	str1 typeln                # should be '---helhello'
	str1 6 + str1     5 <move
	str1 typeln                # should be 'hellolhello'

	str2 0 size . # should be 11
	cr

	a b -text . #  1 / pos
	b a -text . # -1 / neg
	a a -text . #  0 / 0
	b c -text . # -1 / neg
	cr

	n1 number .
	n2 number .
	n3 number .
	n4 number .
	cr

	halt
;
)