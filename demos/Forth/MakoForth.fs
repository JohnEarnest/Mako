######################################################
##
##  MakoForth:
##
##  A tiny, highly embeddable threaded Forth
##  environment for the MakoVM platform. Contains
##  a minimal base vocabulary with all the important
##  functionality of a true Forth- defining words,
##  interpreted/compiled modes, etc.
##
##  John Earnest
##
######################################################

:include <Print.fs>
:include <String.fs>
:include <Parse.fs>

:vector abort   typeln halt ; ( str -- )

######################################################
##
##  Dictionary
##
######################################################

:const dict-size 4096
:array dictionary dict-size 0
:data  here dictionary
:data  head 0

: free-cells  dict-size here @ dictionary - - ; ( -- n )

: , ( n -- )
	free-cells 1 < if
		"dictionary space exhausted!" abort
	then
	here @ ! here inc
;

: .dict-link                  ; ( entry -- addr )
: .dict-flag 1 +              ; ( entry -- addr )
: .dict-body 2 +              ; ( entry -- addr )
: .dict-name 3 +              ; ( entry -- addr )
: .dict-code .dict-body @     ; ( entry -- addr )
: .dict-data .dict-body @ 1 + ; ( entry -- addr )

: dict-find ( str -- entry )
	head @ loop
		dup -if
			drop "unknown word '" type
			type "'!" abort
		then
		2dup .dict-name -text -if
			nip exit
		then
		.dict-link @
	again
;

: dict-add ( str -- )
	head @ ,                   # link to old head
	here @ 1 - head !          # update head to new entry
	0 ,                        # init flag
	0 ,                        # space for body ptr
	loop                       # copy in name
		dup @ -if @ , break then
		dup @ , 1 +
	again
	here @ head @ .dict-body ! # fixup body ptr
;

: check-addr ( addr -- addr )
	dup dictionary < over dictionary dict-size + > or if
		. "bad address!" abort
	then
;

######################################################
##
##  Inner Interpreter
##
######################################################

:const stack-size 50
:array par stack-size 0 :data pptr par
:array ret stack-size 0 :data rptr ret
:var   pc

: PUSH   pptr @ ! pptr inc ; ( n -- )
: RPUSH  rptr @ ! rptr inc ; ( n -- )
: POP    pptr dec pptr @ @ ; ( -- n )
: RPOP   rptr dec rptr @ @ ; ( -- n )
: VAL    pc @ 1 + @        ; ( -- n )
: DATA   pc @ 2 +          ; ( -- addr )

: check-stacks ( -- )
	pptr @ par              <  if "stack underflow!"  abort then
	pptr @ par stack-size + >= if "stack overflow!"   abort then
	rptr @ ret              <  if "rstack underflow!" abort then
	rptr @ ret stack-size + >= if "rstack overflow!"  abort then
;

:var runwhile
: interpret ( entry -- )
	true runwhile !
	.dict-code pc ! 0 RPUSH
	loop
		#"exec " type pc @ .
		#"[ " type  pc @ @ . "]" typeln
		pc @ @
		exec
		pc inc
		check-stacks
		runwhile @
	while
;

: lit      VAL PUSH pc inc                                 ; ( -- )
: branch0  POP  if pc inc else VAL 1 - pc ! then           ; ( -- )
: branchi  POP -if pc inc else VAL 1 - pc ! then           ; ( -- )
: branch                       VAL 1 - pc !                ; ( -- )
: leave    RPOP pc ! pc @ -if false runwhile ! then        ; ( -- )
: finish   leave                                           ; ( -- )
: call     pc @ 1 + RPUSH VAL 1 - pc !                     ; ( -- )
: dodoes   DATA PUSH VAL if call RPOP drop else leave then ; ( -- )

######################################################
##
##  Compiler Entrails
##
######################################################

: /0         dup -if "divide by zero!" abort then ; ( n -- n )
: name>      { space? not eof? not and } accept>  ; ( -- str )
: primitive  dict-add ,                           ; ( code' str -- )
: immediate  primitive true head @ .dict-flag !   ; ( code' str -- )

:array brk 100 0
:data bptr brk
: bpush bptr @ ! bptr inc ; ( n -- )
: bpop  bptr dec bptr @ @ ; ( -- n )
: resolve-break ( -- )
	loop
		bpop dup -if drop break then
		here @ swap !
	again
;

######################################################
##
##  Primitives
##
######################################################

:var mode

: init-dictionary ( -- )
	# core vocabulary:
	{ POP POP         +                    PUSH finish } "+"         primitive
	{ POP POP swap    -                    PUSH finish } "-"         primitive
	{ POP POP         *                    PUSH finish } "*"         primitive
	{ POP POP swap /0 /                    PUSH finish } "/"         primitive
	{ POP POP swap /0 mod                  PUSH finish } "mod"       primitive
	{ POP POP         and                  PUSH finish } "and"       primitive
	{ POP POP         or                   PUSH finish } "or"        primitive
	{ POP POP         xor                  PUSH finish } "xor"       primitive
	{ POP POP swap    <                    PUSH finish } "<"         primitive
	{ POP POP swap    >                    PUSH finish } ">"         primitive
	{ POP POP swap    check-addr !              finish } "!"         primitive
	{ POP             check-addr @         PUSH finish } "@"         primitive
	{ POP not                              PUSH finish } "not"       primitive
	{ POP dup PUSH PUSH                         finish } "dup"       primitive
	{ POP POP dup swap PUSH swap PUSH PUSH      finish } "over"      primitive
	{ POP drop                                  finish } "drop"      primitive
	{ POP POP swap                    PUSH PUSH finish } "swap"      primitive
	{ RPOP POP RPUSH RPUSH                      finish } ">r"        primitive
	{ RPOP RPOP PUSH RPUSH                      finish } "r>"        primitive
	{ RPOP RPOP      dup PUSH RPUSH       RPUSH finish } "i"         primitive
	{ RPOP RPOP RPOP dup PUSH RPUSH RPUSH RPUSH finish } "j"         primitive
	{ here                                 PUSH finish } "here"      primitive
	{ mode @                               PUSH finish } "mode"      primitive
	{ POP ,                                     finish } ","         primitive
	{ true  mode !                              finish } "]"         primitive
	{ false mode !                              finish } "["         immediate
	{ true head @ .dict-flag !                  finish } "immediate" primitive
	{ ' lit , POP ,                             finish } "literal"   immediate
	{ name> dict-add ' dodoes , 0 ,             finish } "create"    primitive
	{ name> dict-add true  mode !               finish } ":"         primitive
	{ ' leave ,      false mode !               finish } ";"         immediate
	{ ' leave ,                                 finish } "exit"      immediate
	{ ' branch0 , here @ 0 ,               PUSH finish } "if"        immediate
	{ ' branch  , here @ 0 , here @ POP !  PUSH finish } "else"      immediate
	{             here @            POP !       finish } "then"      immediate
	{ ' branch  , here @ bpush 0 ,              finish } "break"     immediate
	{ 0 bpush     here @ PUSH                   finish } "loop"      immediate
	{ ' branch  , POP , resolve-break           finish } "again"     immediate
	{ ' branch0 , POP , resolve-break           finish } "until"     immediate
	{ ' branchi , POP , resolve-break           finish } "while"     immediate
	{ ' lit     , name> dict-find ,             finish } "'"         immediate
	{ RPOP dup RPUSH 1 + head @ .dict-data ! leave leave } "does>"   primitive

	# optional text output vocabulary:
	{ POP .      finish } "."      primitive
	{ space      finish } "space"  primitive
	{ cr         finish } "cr"     primitive
	{ POP type   finish } "type"   primitive
	{ POP typeln finish } "typeln" primitive

	# optional program maintenance vocabulary:
	{ name> dict-find dup here ! .dict-link @ head ! finish } "forget" primitive
	{ free-cells . "cells free" typeln               finish } "free"   primitive
	{ :proto see name> see                           finish } "see"    primitive
	{ line> drop                                     finish } "#"      immediate
	{
		loop
			eof? if break then
			")" match? if break then
			skip
		again finish
	} "(" immediate
	{
		par loop
			dup pptr @ >= if drop break then
			dup @ . 1 +
		again cr finish
	} "stack" primitive
	{
		head @ loop
			dup -if drop break then
			dup .dict-name type space
			.dict-link @
		again cr cr finish
	} "words" primitive

	# optional dictionary manipulation vocabulary:
	(
	{ head           PUSH finish } "head"    primitive
	{ name>          PUSH finish } "name>"   primitive
	{ POP .dict-link PUSH finish } "link"    primitive
	{ POP .dict-flag PUSH finish } "flag"    primitive
	{ POP .dict-body PUSH finish } "name"    primitive
	{ POP .dict-code PUSH finish } "code"    primitive
	{ POP .dict-data PUSH finish } "data"    primitive
	{ ' branch0 , POP ,   finish } "branch0" primitive
	{ ' branch  , POP ,   finish } "branch"  primitive
	)
;

######################################################
##
##  Outer Interpreter / Compiler
##
######################################################

: word ( -- )
	name> dict-find
	dup .dict-flag @ mode @ 0 = or
	if
		interpret
	else
		.dict-code ' call , ,
	then
;

: num ( -- )
	signed>
	mode @ if
		' lit , ,
	else
		PUSH
	then
;

: token ( -- )
	curr 34 = if
		skip
		' branch , here @ 0 ,
		loop
			eof?      if      break then
			curr 34 = if skip break then
			getc ,
		again
		0 , here @ over ! 1 + ' lit , ,
		trim
		exit
	then
	signed? if num exit then
	word
;

: run ( str -- )
	>read trim loop eof? if break then token again
;

######################################################
##
##  Disassembler (Optional)
##
######################################################

: code-range ( str -- start end )
	here @ swap head @ loop
		dup -if
			drop "unknown word '" type type "'!" abort
		then
		2dup .dict-name -text -if nip break then
		>r nip i swap r>
		.dict-link @
	again .dict-code
;

: code>name ( addr -- str? flag )
	head @ loop
		( addr entry )
		dup -if 2drop false exit then
		2dup .dict-code = if nip .dict-name true exit then
		.dict-link @
	again
;

: .arg ( ptr addr str -- ptr' flag )
	>r over @ = if
		r> type 1 + dup @ . 1 + true
	else
		rdrop false
	then
;

: .token  ( ptr -- ptr' )
	' dodoes  "dodoes "  .arg if exit then
	' branch0 "branch0 " .arg if exit then
	' branchi "branchi " .arg if exit then
	' branch  "branch "  .arg if exit then
	' lit     "literal " .arg if exit then
	dup @ ' call  = if
		"call "  type 1 +
		dup @ code>name if type else dup @ . then
		1 + exit
	then
	dup @ ' leave = if "leave " type 1 + exit then
	"???? " type dup @ . 1 +
;

: see-range ( end start -- )
	loop
		tab dup . ": " type .token cr 2dup =
	until 2drop
;

: see  code-range see-range cr ; ( str -- )

######################################################
##
##  Usage Examples
##
######################################################

: main
	init-dictionary

	#"3 2 - dup + . cr" run
	#": rtest  34 97 >r . 5 >r i . j . r> . r> . cr ; rtest" run 
	": body   dup . 1 - dup ;" run
	": ltest  5 loop body while drop cr ; ltest" run
	#": baz  if 7 . else 5 . then cr ; 0 baz 1 baz cr" run
	#": var create 0 , does> ;" run
	#"var a var b 47 a ! 39 b ! a @ . b @ . cr" run
	#"create baz 0 ,  55 baz ! 47 baz @ . . cr" run
	#": thing create 9 , does> 99999 . ;" run
	#"777777 . thing quux 88888 . quux . cr" run

	#": const  create , does> @ ;" run
	#"42 const life-universe" run
	#"1 life-universe + . cr" run

	#": foo 1 2 3 ;" run
	#"words" run
	#"forget foo" run
	#"words" run 

	"see body see ltest" run
;