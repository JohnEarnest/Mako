######################################################
##
##  Abort:
##
##  A simple stack-smashing mechanism which can be
##  used to create exception-like error handling
##  functionality as well as other kinds of
##  nonlinear control flow.
##
##  John Earnest
##
######################################################

:array tries 20 0
:data  try-ptr tries
:const /try 2

: try-rp   try-ptr @     ; ( -- addr )
: try-dp   try-ptr @ 1 + ; ( -- addr )
: try-src  try-ptr @ 2 + ; ( -- addr )
: try+     try-ptr @ /try + try-ptr ! ; ( -- )
: try-     try-ptr @ /try - try-ptr ! ; ( -- )

: try ( proc -- flag )
	try+
	DP @ 1 - try-dp  !
	RP @     try-rp  !
	exec
	try-
	true
;

: abort ( -- )
	try-rp  @ RP !
	try-dp  @ DP !
	try-
	false
;

######################################################
##
##  Usage example:
##
######################################################

(
:include <Print.fs>

: doomed
	5 6 abort "Shouldn't be here." typeln
;

: blessed
	drop drop drop
;

: choose
	if doomed "Shouldn't be here either." typeln else blessed then
	"Made a choice." typeln
;

: main
	666
	{ 2 3 4 1 choose } try if "Succeeded." else "Failed." then typeln
	{ 2 3 4 0 choose } try if "Awesome."   else "Shoot."  then typeln
	. cr
;
)
