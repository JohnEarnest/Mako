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

:const dict-size 9147
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

: fj ( -- )
	{ "Unresolved forward branch!" abort } ,
;

: .dict-link                  ; ( entry -- addr )
: .dict-flag  1 +             ; ( entry -- addr )
: .dict-body  2 +             ; ( entry -- addr )
: .dict-stack 3 +             ; ( entry -- addr )
: .dict-help  4 +             ; ( entry -- addr )
: .dict-name  5 +             ; ( entry -- addr )
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
	0 ,                        # space for stack effect ptr
	0 ,                        # space for help ptr
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

: PUSH   pptr @ ! pptr inc  pptr @ par stack-size + >= if "stack overflow!"   abort then ; ( n -- )
: RPUSH  rptr @ ! rptr inc  rptr @ ret stack-size + >= if "rstack overflow!"  abort then ; ( n -- )
: POP    pptr dec pptr @ @  pptr @ par              <  if "stack underflow!"  abort then ; ( -- n )
: RPOP   rptr dec rptr @ @  rptr @ ret              <  if "rstack underflow!" abort then ; ( -- n )

: VAL    pc @ 1 + @        ; ( -- n )
: DATA   pc @ 2 +          ; ( -- addr )

:var runwhile
: interpret ( entry -- )
	true runwhile !
	.dict-code pc ! 0 RPUSH
	loop
		pc @ @
		exec pc inc
		kbrk runwhile @
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
	{ ' branch0 , here @ fj                PUSH finish } "if"        immediate
	{ ' branch  , here @ fj here @ POP !   PUSH finish } "else"      immediate
	{             here @           POP !        finish } "then"      immediate
	{ ' branch  , here @ bpush 0 ,              finish } "break"     immediate
	{ 0 bpush     here @ PUSH                   finish } "loop"      immediate
	{ ' branch  , POP , resolve-break           finish } "again"     immediate
	{ ' branch0 , POP , resolve-break           finish } "until"     immediate
	{ ' branchi , POP , resolve-break           finish } "while"     immediate
	{ RPOP dup RPUSH 1 + head @ .dict-data ! leave leave } "does>"    primitive
	{ name> dict-find .dict-code 1 - mode @ if ' lit , , else PUSH then finish } "'" immediate

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

	{ pad pad-size line> trim                        finish } "#"      immediate
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
		again
		"<" type
		pptr @ par - .num
		">" typeln
		finish
	} "stack" primitive
	{
		head @ loop
			dup -if drop break then
			dup .dict-name type space
			.dict-link @
		again cr cr finish
	} "words" primitive
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
	dup @ ' leave = if "exit " type 1 + exit then
	"???? " type dup @ . 1 +
;

: see-range ( end start -- )
	loop
		dup . ": " type .token cr 2dup =
	until 2drop
;

: see  code-range see-range cr ; ( str -- )