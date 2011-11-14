######################################################
##
##  BASIC Lib:
##
## Support words for the MASIC-BASIC implementation,
## supplying various intrinsic functions and internals.
## 
######################################################

: goto r> drop >r ;
: abs dup 0 < if -1 * then ;
: sgn dup -if -1 else 1 then ;
: rnd RN @ swap mod ;
: max 2dup < if swap then drop ;
: min 2dup > if swap then drop ;
: >= < not ;
: <= > not ;
: =  xor -if true else false then ;
: <> xor -if false else true then ;

# print:
: emit    CO ! ;
: cr      10 emit ;
: .number 10 2dup mod >r / r> 48 + swap dup if .number else drop then emit ;
: print   dup 0 < if 45 emit -1 * then .number 32 emit ;
: prints  loop dup @ dup if emit else 2drop exit then 1 + again ;

# input:
: key     CO @ ;
: digit?  dup 47 > over 58 < and swap drop ;
: input
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