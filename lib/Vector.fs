######################################################
##
##  Vector:
##
##  A series of utility words for dealing with
##  vectored execution. Vectoring is an extremely
##  lightweight mechanism for redefining the behavior
##  of a word at runtime.
##
##  John Earnest
##
######################################################

: vectored? ('word -- flag)
	dup 1 + @ swap 2 + xor -if false else true then
;

: revector  ('new-word 'word --)
	1 + !
;

: devector  ('word --)
	dup 2 + swap 1 + !
;

: default   ('word -- 'word)
	2 + exec
;

(
# use examples:
:include "Print.fs"

# To create a vectored word, use ':vector'.
# Vectored definitions can satisfy prototypes like
# normal colon definitions.
:vector a 5 . ;
:       b 8 . ;

: main
	# Initially, a vectored word will carry out
	# the code specified in its definition as usual:
	a               # 5 expected
	' a vectored? . # 0 expected

	# By 'revectoring' we redirect all calls to a word
	# to a new word's body. Note that vectoring can
	# be chained arbitrarily.
	' b ' a revector

	a               #  8 expected
	' a vectored? . # -1 expected

	# 'default' allows us to access the original
	# implementation of a vectored word, disregarding
	# revectoring:
	' a default     #  5 expected

	# 'devector' rather obviously restores the
	# original behavior.
	' a devector
	a               #  5 expected
	cr
;
)