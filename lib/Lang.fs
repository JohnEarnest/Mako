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

:const true  -1
:const false  0

: =   xor -if true else false then ;

: foobar 23 ;