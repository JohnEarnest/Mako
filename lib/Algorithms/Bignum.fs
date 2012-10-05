######################################################
##
##  Bignum:
##
##  A system based on Garbage.fs which provides
##  support for arbitrary-precision arithmetic.
##  Depends on Garbage.fs and Print.fs.
##
##  John Earnest
##
######################################################

# A simplified version of the system
# decribed in Pair.fs:

:const nil -2147483648
: nil?    nil xor -if true else false then         ; ( val -- flag )
: pair    2 alloc dup >r ptr> swap over 1 + ! ! r> ; ( first rest -- pair )
: first   ptr>     @                               ; ( pair -- first )
: rest    ptr> 1 + @                               ; ( pair --  rest )
:  split  dup first swap  rest                     ; ( pair -- first rest )
: -split  dup  rest swap first                     ; ( pair -- rest first )

:const base 10

######################################################
##
##  I/O:
##
##  Bignums are represented with a linked-list of
##  base-10 digits, starting with the least significant.
##  'b>' will obviously only work properly if the
##  bignum fits in a machine word- this is primarily
##  for testing purposes. 
##
######################################################

: >b ( n -- big )
	dup -if drop nil exit then
	base /mod swap >b pair
;

: b> ( big -- n )
	dup nil? if drop 0 exit then
	split b> base * +
;

: .bn ( big -- )
	dup nil? if drop exit then
	split .bn .num
;

: .b ( big -- )
	dup nil? if drop 0 . space exit then
	.bn space
;

######################################################
##
##  Counting:
##
######################################################

: binc ( big -- big+1 )
	dup nil? if
		1 swap pair exit
	then
	-split
	1 + dup base = if
		drop binc 0
	then
	swap pair
;

: bdec ( big -- big-1 )
	-split
	1 - dup 0 < if
		drop bdec base 1 -
	then
	over nil? over 0 = and if
		drop exit
	then
	swap pair
;

######################################################
##
##  Arithmetic:
##
######################################################

: +b ( big n -- big+n )
	dup -if drop exit then
	dup 0 < if
		-1 *
		1 - for bdec next
	else
		1 - for binc next
	then
;

: [b+b] ( bigA bigB carryin -- A+B )
	>r
	over nil? over nil? and if
		2drop r>
		dup if nil pair else drop nil then
		exit
	then
	over nil? if 0 else over first then
	over nil? if 0 else over first then
	r> + + base /mod >r >r
	dup nil? -if rest then swap
	dup nil? -if rest then swap
	r> [b+b] r> swap pair
;

: b+b ( bigA bigB -- A+B )
	0 [b+b]
;

: *b ( big n -- big*n )
	dup    -if 2drop nil exit then
	dup 1 = if drop      exit then
	over swap
	2 - for over b+b next
	swap drop
;