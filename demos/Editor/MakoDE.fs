######################################################
##
##  MakoDE:
##
##  An experiment in bootstrapping an interactive
##  Forth compiler from Maker.
##
##  John Earnest
##
######################################################

:include <Print.fs>
:include <String.fs>
:include <Vector.fs>
:include <Grid.fs>
:include <Sprites.fs>

:array data-stack   200 0
:array return-stack 200 0

:var restart-vector
: abort ( -- )
	data-stack   DP !
	return-stack RP !
	restart-vector @ exec
	halt
;

: 1arg  DP @ data-stack       > if exit then "Stack underflow!"  typeln abort ;
: 2arg  DP @ data-stack   1 + > if exit then "Stack underflow!"  typeln abort ;
: 3arg  DP @ data-stack   2 + > if exit then "Stack underflow!"  typeln abort ;
: 1ret  RP @ return-stack 1 + > if exit then "RStack underflow!" typeln abort ;
: 2ret  RP @ return-stack 2 + > if exit then "RStack underflow!" typeln abort ;

: abs      1arg  dup 0 < if -1 * then      ;
: inc      1arg  dup @ 1 + swap !          ;
: dec      1arg  dup @ 1 - swap !          ;
: +!       2arg  swap over @ swap + swap ! ;
: -!       2arg  swap over @ swap - swap ! ;
: min      2arg  2dup > if swap then drop  ;
: max      2arg  2dup < if swap then drop  ;

######################################################
##
##  Compiler:
##
######################################################

:const imm  0 # immediate word
:const code 1 # code word
:const def  2 # defined word (has pfa)

:var   here
:var   head
:var   end
:var   mode
:var   input-index    # index into input buffer (private)
:array input     80 0 # input buffer
:array word-buff 80 0 # buffer for current word (private)

: :? mode @ def = ; # are we compiling?

: .prev     ; # previous entry
: .type 1 + ; # entry type
: .code 2 + ; # code address
: .args 3 + ; # argument field/address
: .name 4 + ; # name (null-terminated)

: bogus-jump   "unresolved branch!" typeln abort ;

: ,          1arg here @ ! here inc        ; ( val -- )
: [const]    0 , ,                         ; ( addr -- )
: [call]     1 , ,                         ; ( addr -- )
: [return]  12 ,                           ; ( -- )
: [jump]     2 , ,                         ; ( addr -- )
: [jumpz]    3 , ' bogus-jump , here @ 1 - ; ( -- patch-addr )
: [jumpif]   4 , ' bogus-jump , here @ 1 - ; ( -- patch-addr )


: find ( str-addr -- entry-addr? flag ) 1arg
	head @ loop
		(str entry -- )
		2dup .name -text
		-if swap drop true exit then
		.prev @ dup
	while
	2drop false
;

: word ( -- str-addr )
	word-buff 80 0 fill
	loop
		input-index @ input + @
		dup    -if drop break then
		white? -if      break then
		input-index inc
	again
	word-buff loop
		input-index @ input + @
		dup white? if drop break then
		dup       -if drop break then
		over !
		input-index inc
		1 +
	again
	0 swap ! word-buff
;

: >number? ( -- number? flag )
	1 0 word-buff
	dup @ 45 = if
		1 + >r 2drop -1 0 r>
	then
	loop
		( sgn val addr )
		swap 10 * over @ 48 - + swap
		dup @ digit? -if 2drop drop false exit then
		1 + dup @
	while
	drop * true
;

:var old-here

: bail-def ( -- )
	:? if
		head @ .prev @ head !
		old-here @ here !
		imm mode !
	then
;

: interpret-word ( -- )
	# skip whitespace:
	loop
		input-index @ input + @
		dup    -if drop exit  then
		white? -if      break then
		input-index inc
	again

	# quoted string:
	input-index @ input + @ 34 = if
		input-index inc
		' bogus-jump [jump] here @ 1 -
		loop
			input-index @ input + @
			dup 34 = if drop break then
			, input-index inc
		again
		0 , input-index inc
		here @ over ! 1 + [const]
		exit
	then

	# a word in the dictionary:
	word dup size 1 < if drop exit then
	find if
		dup .type @ imm = if
			.code @ exec
		else
			dup .type @ def = if
				dup .code @ swap .args @ exec
			else
				.code @ :? if [call] else exec then
			then
		then
		exit
	then

	# a number:
	>number? if
		:? if [const] then
		exit
	then
	
	# word not found:
	"'"  type word-buff type "' ?" typeln
	bail-def abort
;

: interpret ( -- )
	0 input-index !
	loop
		interpret-word
		input-index @ input + @
	while
	:? -if cr then
;

: [create] ( name -- )
	here @ old-here !
	head @ here @ head ! , # prev
	code , 0 , 0 ,         # type, code, args
	1 - loop               # name
		1 + dup @ , dup @
	while drop
;

: create ( -- )
	word [create]
	here @ head @ .code !
;

: does> ( -- )
	def head @ .type !
	r>  head @ .args !
;

: lookup ( -- entry-addr )
	word head @ loop
		(str entry)
		2dup .name -text
		-if swap drop exit then
		.prev @ dup
	while drop
	"'"  type type "' ?" typeln
	bail-def abort
;

: forget ( -- )
	lookup
	"forgot how to '" type dup .name type "'" typeln
	dup here ! .prev @ head !
;

: p_' ( -- )
	lookup .code @ :? if [const] then
;

: p_char ( -- char )
	word @ :? if [const] then
;

######################################################
##
##  Flow control primitives:
##
######################################################

:const    if-flag -1
:const  loop-flag -2
:const   for-flag -3
:const curly-flag -4

: check-flow
	:? if exit then
	"can't use flow words in direct mode!" typeln
	bail-def abort
;

: p_if    check-flow ' 1arg [call] [jumpz]             if-flag ;
: p_-if   check-flow ' 1arg [call] [jumpif]            if-flag ;
: p_loop  check-flow               here @            loop-flag ;
: p_for   check-flow ' 1arg [call] 17 , here @        for-flag ;
: p_{     check-flow ' bogus-jump [jump] here @ 1 - curly-flag ;

: p_then
	check-flow
	if-flag = if here @ swap ! exit then
	"'then' without 'if'!" typeln
	bail-def abort
;

: p_else
	check-flow
	if-flag = if
		' bogus-jump [jump]
		here @ 1 - swap here @ swap !
		if-flag exit
	then
	 "'else' without 'if'!" typeln
	bail-def abort
;

: p_again
	check-flow
	loop-flag = if [jump] exit then
	"'again' without 'loop'!" typeln
	bail-def abort
;

: p_until
	check-flow
	loop-flag = if ' 1arg [call] [jumpz] ! exit then
	"'until' without 'loop'!" typeln
	bail-def abort
;

: p_while
	check-flow
	loop-flag = if ' 1arg [call] [jumpif] ! exit then
	 "'while' without 'loop'!" typeln
	bail-def abort
;

: p_next
	check-flow
	for-flag = if 31 , , 18 , 13 , exit then
	"'next' without 'for'!" typeln
	bail-def abort
;

: p_}
	check-flow
	curly-flag = if [return] here @ over ! [const] exit then
	"'}' without '{'!" typeln
	bail-def abort
;

######################################################
##
##  Base dictionary:
##
######################################################

# core primitives:
:data d_+    0      code :proto p_+    p_+    0 "+"    : p_+    2arg +    ;
:data d_-    d_+    code :proto p_-    p_-    0 "-"    : p_-    2arg -    ;
:data d_*    d_-    code :proto p_*    p_*    0 "*"    : p_*    2arg *    ;
:data d_/    d_*    code :proto p_/    p_/    0 "/"    : p_/    2arg /    ;
:data d_mod  d_/    code :proto p_mod  p_mod  0 "mod"  : p_mod  2arg mod  ;
:data d_and  d_mod  code :proto p_and  p_and  0 "and"  : p_and  2arg and  ;
:data d_or   d_and  code :proto p_or   p_or   0 "or"   : p_or   2arg or   ;
:data d_xor  d_or   code :proto p_xor  p_xor  0 "xor"  : p_xor  2arg xor  ;
:data d_not  d_xor  code :proto p_not  p_not  0 "not"  : p_not  1arg not  ;
:data d_>    d_not  code :proto p_>    p_>    0 ">"    : p_>    2arg >    ;
:data d_<    d_>    code :proto p_<    p_<    0 "<"    : p_<    2arg <    ;
:data d_@    d_<    code :proto p_@    p_@    0 "@"    : p_@    1arg @    ;
:data d_!    d_@    code :proto p_!    p_!    0 "!"    : p_!    2arg !    ;
:data d_dup  d_!    code :proto p_dup  p_dup  0 "dup"  : p_dup  1arg dup  ;
:data d_drop d_dup  code :proto p_drop p_drop 0 "drop" : p_drop 1arg drop ;
:data d_swap d_drop code :proto p_swap p_swap 0 "swap" : p_swap 2arg swap ;
:data d_over d_swap code :proto p_over p_over 0 "over" : p_over 2arg over ;
:data d_>r   d_over code :proto p_>r   p_>r   0 ">r"   : p_>r   1arg r> swap >r >r ;
:data d_r>   d_>r   code :proto p_r>   p_r>   0 "r>"   : p_r>   2ret r> r> swap >r ;

# extended 'primitives':
:data d_rdrop d_r>    code :proto p_rdrop p_rdrop 0 "rdrop" : p_rdrop 2ret r> r> drop r> ;
:data d_halt  d_rdrop code :proto p_halt  p_halt  0 "halt"  : p_halt  halt          ;
:data d_exit  d_halt  code :proto p_exit  p_exit  0 "exit"  : p_exit  1ret rdrop    ;
:data d_sync  d_exit  code :proto p_sync  p_sync  0 "sync"  : p_sync  sync          ;
:data d_keys  d_sync  code :proto p_keys  p_keys  0 "keys"  : p_keys  keys          ;
:data d_i     d_keys  code :proto p_i     p_i     0 "i"     : p_i     j             ;
:data d_j     d_i     code :proto p_j     p_j     0 "j"     : p_j     k             ;

# core immediate words
:data d_if    d_j     imm p_if    0 "if"
:data d_-if   d_if    imm p_-if   0 "-if"
:data d_then  d_-if   imm p_then  0 "then"
:data d_else  d_then  imm p_else  0 "else"
:data d_loop  d_else  imm p_loop  0 "loop"
:data d_again d_loop  imm p_again 0 "again"
:data d_until d_again imm p_until 0 "until"
:data d_while d_until imm p_while 0 "while"
:data d_for   d_while imm p_for   0 "for"
:data d_next  d_for   imm p_next  0 "next"
:data d_{     d_next  imm p_{     0 "{"
:data d_}     d_{     imm p_}     0 "}"
:data d_'     d_}     imm p_'     0 "'"
:data d_char  d_'     imm p_char  0 "char"
:data d_[     d_char  imm :proto  [       [       0 "["     : [       imm mode !  ;
:data d_]     d_[     imm :proto  ]       ]       0 "]"     : ]       def mode !  ;
:data d_:     d_]     imm :proto  p_:     p_:     0 ":"     : p_:     create ]    ;
:data d_;     d_:     imm :proto  p_;     p_;     0 ";"     : p_;     [return] [  ;

:array free-space 4096 0
:data  last-cell  0
:const last-def   d_;

######################################################
##
##  Initialization:
##  (Wherein the compiler compiles itself)
##
######################################################

: direct ( -- )
	input over size 1 + >move interpret
;

: constant ( arg-addr -- val? )
	@ :? if [const] then
;

: build-constant ( value name -- )
	[create]
	' constant head @ .args !
	def        head @ .type !
	here @     head @ .code ! ,
;

: build-word ( addr name -- )
	[create] head @ .code !
;

: foreach ( 0 ... 'word -- )
	>r loop i exec dup while r> 2drop
;

: init ( -- )
	last-def   head !
	last-cell  end  !
	free-space here !

	0
	here  "here"
	head  "head"
	end   "end"
	mode  "mode"
	input "input"
	' build-constant foreach

	0
	' =        "="        ' !=       "!="
	' +!       "+!"       ' -!       "-!"
	' inc      "inc"      ' dec      "dec"
	' abs      "abs"      ' min      "min"
	' max      "max"      ' abort    "abort"
	' ,        ","        ' .prev    ".prev"
	' .type    ".type"    ' .code    ".code"
	' .args    ".args"    ' .name    ".name"
	' find     "find"     ' create   "create"
	' does>    "does>"    ' cr       "cr"
	' space    "space"    ' lookup   "lookup"
	' forget   "forget"

	{ 3arg >move  } ">move"
	{ 3arg <move  } "<move"
	{ 3arg fill   } "fill"
	{ 2arg -text  } "-text"
	{ 1arg digit? } "digit?"
	{ 1arg white? } "white?"
	{ 1arg emit   } "emit"
	{ 1arg .      } "."
	{ 1arg type   } "type"
	{ 1arg typeln } "typeln"

	' build-word foreach

	{ drop } ' emit revector
	": >=         < not              ;" direct
	": <=         > not              ;" direct
	": 0=         0 =                ;" direct
	": 0!         0 !=               ;" direct
	": 1+         1 +                ;" direct
	": 1-         1 -                ;" direct
	": neg        -1 *               ;" direct
	": 2dup       over over          ;" direct
	": 2drop      drop drop          ;" direct
	": :var       create 0 , does>   ;" direct
	": immediate  0 head @ .type !   ;" direct
	": exec       >r                 ;" direct
	": words  head @ loop"              direct
	"  dup .name type space .prev @"    direct
	"  dup while drop cr ;"             direct
	": allot      1 - for 0 , next   ;" direct
	": free       end @ here @ -     ;" direct
	' emit devector
;

######################################################
##
##  Main loop and I/O routines:
##
######################################################

:data  sprite-tiles
:image   grid-tiles "text.png" 8 8

:const cursor-s 0
:var   used
:var   cursor
:var   cx
:var   lines
:data  cc -32

: plain-text -32 cc ! ;
: code-text   64 cc ! ;

: console-newline
	lines inc
	lines @ 28 > if
		0 lines !
		64 ascii !
		0 29 "[More...]" grid-type
		loop keys key-a and sync until
		loop keys key-a and sync while
		loop KB @ -1 = until
		0 29 "         " grid-type
	then
	0 cx !
	27 for
		0 28 i - tile-grid@
		0 27 i - tile-grid@
		40 >move
	next
	0 28 tile-grid@ 40 0 fill
	sync
;

: console-emit ( char -- )
	dup 10 = if
		console-newline
		drop exit
	else
		cc @ + cx @ 28 tile-grid@ !
		cx inc cx @ 39 >
		if console-newline then
	then
;

: input-clear ( -- )
	input 41 0 fill
	0 cursor !
	0 used   !
;

: console-prompt ( -- )
	imm mode !
	input-clear
	loop
		loop
			KB @ dup -1 = if drop break then
			dup 10 = if
				# return
				drop :? -if cr then
				code-text input typeln plain-text
				0 lines !
				cursor-s hide
				interpret
				cursor-s show
				input-clear
			else
				dup 8 = if
					drop
					# backspace
					cursor @ 0 > if
						cursor @ input + dup 1 -
						used @ cursor @ - 1 + <move
						cursor dec@ used dec@
					then
				else
					# insert
					used @ 40 >= if drop else
						cursor @ input + dup dup 1 +
						used @ cursor @ - >move !
						cursor inc@ used inc@
					then
				then
			then
		again

		keys key-rt and if cursor @ 1 + used @ min cursor ! then
		keys key-lf and if cursor @ 1 -      0 max cursor ! then

		39 for
			i used @ >= if 0 else i input + @ 32 - then
			96 + i 29 tile-grid@ !
		next
		8x8 191 cursor @ 8 * 232 cursor-s >sprite

		sync sync
	again
;

: main
	init
	' console-emit '  emit revector
	' console-prompt restart-vector !

	0 0 "MakoDE BIOS 0.2" grid-type
	0 1 "4096k OK"        grid-type
	console-prompt
;