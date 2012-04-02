######################################################
##
##  Tapeworm
##
######################################################

:array grid 1240 -1
:array grid-tiles 64 0xFF000000
:const grid-skip  -1
:const clear-color 0xFF00CCFF

:data key-v key-up key-rt key-dn key-lf
:data key-d    -40      1     40     -1
:data dir -40

:const  /worm 32
:array   worm /worm 0
:var    +worm
:var    -worm

: +%  dup @ 1 + /worm mod swap ! ;
:  H  +worm @ worm +             ;

: >worm
	0 H @ dir @ +
	+worm +% dup H !
	dup @ over grid > and over grid-tiles < and -if
		0xFFFFDD00 CL !
		loop sync again
	then
	!
;

: main
	9 820 grid + H !
	7 for >worm next

	loop
		RN @ 25 mod if
			-1 -worm @ worm + @ !
			-worm +%
		else
			dup 1 > if 1 - then
		then

		>worm
		dup for
			3 for
				keys key-v i + @ and if
					 key-d i + @ dir !
				then
			next
			sync
		next
	again
;