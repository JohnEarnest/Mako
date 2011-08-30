######################################################
##
##  Base ROM:
##
##  This rom is used for bootstrapping FIVETRAN memory
##  images with a handful of support routines.
##  Note that any updates to this code may require
##  updates to the FIVETRAN compiler to reflect the
##  new locations of subroutines.
##
##  John Earnest
##
######################################################

# Support routines for 'print':
: emit    CO ! ;
: .number 10 2dup mod >r / r> 48 + swap dup if .number else drop then emit ;
: print   dup 0 < if 45 emit -1 * then .number 32 emit ;

# Support routines for 'read':
# (This doesn't currently handle negative numbers.
# I have other parts I want to get working.)
: key     CO @ ;
: digit?  dup 47 > over 58 < and swap drop ;
: read
	0 loop
		drop key dup digit?
	until 48 -
	loop
		key dup digit?
		if   swap 10 * swap 48 - +
		else drop break
		then
	again
;

# Stub necessary to compile with Maker-
# FIVETRAN will build a new entrypoint.
: main halt ;