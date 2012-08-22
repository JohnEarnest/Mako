######################################################
##
##  Lazy generators with Curry:
##
##  These demonstrate the power and flexibility
##  of currying by producing a set of composable
##  lazy generating functions, transformers and
##  reducers. Generators follow the protocol of
##  having a stack effect of ( -- value? flag )
##  where if the flag is true, the generator has
##  yielded a value and advanced its state.
##  If the flag is false, no value has been produced
##  and the generator is done producing values.
##
##  This system requires <Algorithms/Curry.fs>
##  which in turn requires <Algorithms/Garbage.fs>.
##
##  John Earnest
##
######################################################

: pinc   dup p@ 1 + swap p!       ;
: pdec   dup p@ 1 - swap p!       ;
: even?  2 mod 0 =                ;
: odd?   2 mod 1 =                ;
: max    2dup < if swap then drop ;
: min    2dup > if swap then drop ;

######################################################
##
##  Core Generators and Transformers:
##
######################################################

: count ( -- gen )
	{ dup pinc p@ true } -1 with
;

: take ( gen n -- gen )
	{ dup p@ -if 2drop false else pdec exec then }
	swap with curry
;

: map ( gen func -- gen )
	{ >r exec -if rdrop false else r> exec true then }
	curry curry
;

: range ( min max -- gen )
	over - 1 + count swap take swap { + } curry map
;

: chain ( gen1 gen2 -- gen )
	{ >r exec if rdrop true else r> exec then }
	curry curry
;

: filter ( gen pred -- gen )
	{
		>r >r
		loop
			i exec -if false break then dup
			j exec  if true  break then drop
		again rdrop rdrop
	} curry curry
;

: take-while ( gen pred -- gen )
	{
		( gen pred flag@ )
		dup p@      -if 2drop drop        false exit then >r
		    >r exec -if rdrop false r> p! false exit then
		dup r> exec -if  drop false r> p! false exit then
		rdrop true
	} true with curry curry
;

: take-until ( gen pred -- gen )
	{ not } compose take-while
;

######################################################
##
##  Reducers:
##
######################################################

: apply ( gen proc -- )
	>r >r loop
		i exec -if break then
		j exec
	again rdrop rdrop
;

: last       0 swap { swap drop } apply ; ( gen -- val )
: length     0 swap { drop 1 +  } apply ; ( gen -- val )
: reduce     >r swap r>           apply ; ( gen start bin-op -- end )

: sum        0 { + }                    reduce ; ( gen -- sum )
: product    1 { * }                    reduce ; ( gen -- product )
: minimum    +infinity ' min            reduce ; ( gen -- minimum )
: maximum    -infinity ' max            reduce ; ( gen -- maximum )
: all?       true  swap { and } compose reduce ; ( gen pred -- flag )
: any?       false swap { or  } compose reduce ; ( gen pred -- flag )

######################################################
##
##  Misc:
##
######################################################

: factorial  1 swap range product ; ( n -- n! )

: digits ( n -- gen )
	{
		dup p@ -if drop false exit then
		dup p@ 10 /mod >r swap p! r> true
	} swap with
;

: array>gen ( addr len -- gen )
	over + 1 - range { @ } map
;

: gen>array ( gen addr -- )
	{ dup pinc p@ + ! } -1 with curry apply
;