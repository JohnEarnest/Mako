######################################################
##
##  Pick:
##
##  Sometimes it's handy to be able to fetch
##  an element at an arbitrary depth in the stack.
##  The supplied index is 0-based- that is,
##  an index of 0 is the topmost stack element
##  ignoring the argument to this word.
##  'pick' also comes in a flavor for the return stack.
##
##  John Earnest
##
######################################################

: pick ( index -- element )
	DP @ 2 - swap - @
;

: rpick ( index -- element )
	RP @ 2 - swap - @
;