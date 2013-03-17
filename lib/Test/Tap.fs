######################################################
##
##  Tap:
##
##  A testing framework compatible with the
##  Test Anything Protocol, TAP.
##
##  Test fixtures should always begin by calling
##  'plan' to indicate the number of tests you wish
##  to run. The optional 'bail' method can be used
##  to halt testing of a fixture for any reason.
##
##  John Earnest
##
######################################################

:include <Print.fs>
:var test-id

: plan ( n -- )
	"1.." type . cr
;

: ok ( flag desc -- )
	swap -if "not " type then "ok " type
	test-id @ 1 + dup . test-id ! type cr
;

# bonus goodies:

: pass  true  "" ok           ; ( -- )
: fail  false "" ok           ; ( -- )
: bail  "Bail out!" type halt ; ( -- )