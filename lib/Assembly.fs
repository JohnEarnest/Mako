######################################################
##
##  Assembly:
##
##  Support words and constants for using Maker
##  in the manner of a conventional assembler and/or
##  for making self-modifying code.
##
##  John Earnest
##
######################################################

:const CONST   0
:const CALL    1
:const JUMP    2
:const JUMPZ   3
:const JUMPIF  4

:const LOAD   10
:const STOR   11
:const RETURN 12
:const DROP   13
:const SWAP   14
:const DUP    15
:const OVER   16
:const STR    17
:const RTS    18

:const ADD    19
:const SUB    20
:const MUL    21
:const DIV    22
:const MOD    23
:const AND    24
:const OR     25
:const XOR    26
:const NOT    27
:const SGT    28
:const SLT    29
:const SYNC   30
:const NEXT   31

######################################################
##
##  A horrifying real-world example:
##
######################################################

(
:data hello "Hello, World!"

:data main
	CONST	hello

:data print
	DUP
	LOAD
	DUP
	if
		CONST	CO
		STOR
	else
		DROP
		DROP
		CONST	10
		CONST	CO
		STOR
		JUMP	-1
	then
	CONST	1
	ADD
	JUMP	print
)