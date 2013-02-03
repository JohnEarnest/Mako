######################################################
##
##  Lang:
##
##  A series of routines that will *always* be
##  loaded by Maker before compiling a user program.
##  They can be thought of as compiler intrinsics.
##
##  John Earnest
##
######################################################

:const key-up 0x01
:const key-rt 0x02
:const key-dn 0x04
:const key-lf 0x08
:const key-a  0x10
:const key-b  0x20

:const sprite-mirror-horiz  0x10000
:const sprite-mirror-vert   0x20000
:const grid-z               0x40000000

:const grid-skip   0
:const scroll-x    0
:const scroll-y    0
:const clear-color 0xFF000000

:const x-close      0
:const x-open-read  1
:const x-open-write 2

:const true  -1
:const false  0

:const +infinity  2147483647
:const -infinity -2147483647

: =     xor -if true  else false then ;
: !=    xor -if false else true  then ;
: exec  >r ;