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

: +!       swap over @ swap + swap ! ;
: -!       swap over @ swap - swap ! ;
: inc      dup @ 1 + swap !          ;
: dec      dup @ 1 - swap !          ;
: abs      dup 0 < if -1 * then      ;
: min      2dup > if swap then drop  ;
: max      2dup < if swap then drop  ;

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

: .prev     ; # previous entry
: .type 1 + ; # entry type
: .code 2 + ; # code address
: .args 3 + ; # argument field/address
: .name 4 + ; # name (null-terminated)

: ,          here @ ! here inc   ; ( val -- )
: [const]    0 , ,               ; ( addr -- )
: [call]     1 , ,               ; ( addr -- )
: [jump]     2 , ,               ; ( addr -- )
: [jumpz]    3 , -2 , here @ 1 - ; ( -- patch-addr )
: [jumpif]   4 , -2 , here @ 1 - ; ( -- patch-addr )
: [return]  12 ,                 ; ( -- )

: find ( str-addr -- entry-addr? flag )
	head @ loop
		(str entry -- )
		2dup .name -text
		-if swap drop true exit then
		.prev @ dup
	while
	2drop false
;

: word ( -- )
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
	0 swap !
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
:var old-DP

: interpret ( -- )
	0 input-index !
	here @ old-here !
	DP   @ old-DP   !
	loop
		word
		word-buff size 1 < if  break then
		word-buff find
		if
			dup .type @ imm = if .code @ exec else
				dup .type @ def = if
					dup .code @ swap .args @ exec
				else
					.code @
					mode @ def = if [call] else exec then
				then
			then
		else
			>number? if
				mode @ def = if [const] then
			else
				"'"  type
				word-buff type
				"' ?" typeln
				mode @ def = if
					head @ .prev @ head !
					old-here @ here !
					old-DP   @ DP   !
					imm mode !
				then
				break
			then
		then
		input-index @ input + @
	while
	cr
;

: [create] ( name -- )
	head @ here @ head ! , # prev
	code , 0 , 0 ,         # type, code, args
	1 - loop               # name
		1 + dup @ , dup @
	while drop
;

: create ( -- )
	word word-buff [create]
	here @ head @ .code !
;

: does> ( -- )
	def head @ .type !
	r>  head @ .args !
;

######################################################
##
##  Base dictionary:
##
######################################################

# core primitives:
:data d_+    0      code :proto p_+    p_+    0 "+"    : p_+    +    ;
:data d_-    d_+    code :proto p_-    p_-    0 "-"    : p_-    -    ;
:data d_*    d_-    code :proto p_*    p_*    0 "*"    : p_*    *    ;
:data d_/    d_*    code :proto p_/    p_/    0 "/"    : p_/    /    ;
:data d_mod  d_/    code :proto p_mod  p_mod  0 "mod"  : p_mod  mod  ;
:data d_and  d_mod  code :proto p_and  p_and  0 "and"  : p_and  and  ;
:data d_or   d_and  code :proto p_or   p_or   0 "or"   : p_or   or   ;
:data d_xor  d_or   code :proto p_xor  p_xor  0 "xor"  : p_xor  xor  ;
:data d_not  d_xor  code :proto p_not  p_not  0 "not"  : p_not  not  ;
:data d_>    d_not  code :proto p_>    p_>    0 ">"    : p_>    >    ;
:data d_<    d_>    code :proto p_<    p_<    0 "<"    : p_<    <    ;
:data d_@    d_<    code :proto p_@    p_@    0 "@"    : p_@    @    ;
:data d_!    d_@    code :proto p_!    p_!    0 "!"    : p_!    !    ;
:data d_dup  d_!    code :proto p_dup  p_dup  0 "dup"  : p_dup  dup  ;
:data d_drop d_dup  code :proto p_drop p_drop 0 "drop" : p_drop drop ;
:data d_swap d_drop code :proto p_swap p_swap 0 "swap" : p_swap swap ;
:data d_over d_swap code :proto p_over p_over 0 "over" : p_over over ;
:data d_>r   d_over code :proto p_>r   p_>r   0 ">r"   : p_>r   r> swap >r >r ;
:data d_r>   d_>r   code :proto p_r>   p_r>   0 "r>"   : p_r>   r> r> swap >r ;

# extended 'primitives':
:data d_rdrop d_r>    code :proto p_rdrop p_rdrop 0 "rdrop" : p_rdrop r> r> drop r> ;
:data d_halt  d_rdrop code :proto p_halt  p_halt  0 "halt"  : p_halt  halt          ;
:data d_exit  d_halt  code :proto p_exit  p_exit  0 "exit"  : p_exit  rdrop         ;
:data d_sync  d_exit  code :proto p_sync  p_sync  0 "sync"  : p_sync  sync          ;
:data d_keys  d_sync  code :proto p_keys  p_keys  0 "keys"  : p_keys  keys          ;
:data d_i     d_keys  code :proto p_i     p_i     0 "i"     : p_i     j             ;
:data d_j     d_i     code :proto p_j     p_j     0 "j"     : p_j     k             ;

# core immediate words
:data d_[     d_j     imm :proto [       [       0 "["     : [       imm mode !  ;
:data d_]     d_[     imm :proto ]       ]       0 "]"     : ]       def mode !  ;
:data d_:     d_]     imm :proto p_:     p_:     0 ":"     : p_:     create ]    ;
:data d_;     d_:     imm :proto p_;     p_;     0 ";"     : p_;     [return] [  ;

:array free-space 4096 0
:data  last-cell  0
:const last-def   d_;
:array data-stack 200  0

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
	@ mode @ def = if [const] then
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
	' max      "max"      ' >move    ">move"
	' <move    "<move"    ' fill     "fill"
	' size     "size"     ' -text    "text"
	' digit?   "digit?"   ' white?   "white?"
	' ,        ","        ' [const]  "[const]"
	' [call]   "[call]"   ' [jump]   "[jump]"
	' [jumpz]  "[jumpz]"  ' [jumpif] "[jumpif]"
	' [return] "[return]" ' .prev    ".prev"
	' .type    ".type"    ' .code    ".code"
	' .args    ".args"    ' .name    ".name"
	' find     "find"     ' word     "word"
	' >number? ">number?" ' create   "create"
	' does>    "does>"    ' emit     "emit"
	' space    "space"    ' cr       "cr"
	' .        "."        ' type     "type"
	' typeln   "typeln"
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
	":  if        [jumpz]            ; immediate" direct
	": -if        [jumpif]           ; immediate" direct
	": then       here @ swap !      ; immediate" direct
	": loop       here @             ; immediate" direct
	": again      [jump]             ; immediate" direct
	": until      [jumpz]  !         ; immediate" direct
	": while      [jumpif] !         ; immediate" direct
	": for        17 , here @        ; immediate" direct
	": next       31 , , 18 , 13 ,   ; immediate" direct
	": else       -2 [jump] here @ 1 - swap here @ swap ! ; immediate" direct
	": words head @ loop dup .name type space .prev @ dup while cr ;"  direct
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
:data  cc -32

: console-newline
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

: plain-text -32 cc ! ;
: code-text   64 cc ! ;

: main
	init

	' console-emit ' emit revector

	0 0 "MakoDE BIOS 0.1" grid-type
	0 1 "4096k OK"        grid-type

	input 41 0 fill
	loop
		KB @ dup -1 = if drop else
			dup 10 = if
				# return
				drop cr
				code-text input typeln plain-text
				interpret
				input 41 0 fill
				0 cursor !
				0 used   !
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
					used @ 41 >= if drop else
						cursor @ input + dup dup 1 +
						used @ cursor @ - >move !
						cursor inc@ used inc@
					then
				then
			then
		then

		keys key-rt and if cursor @ 1 + used @ min cursor ! then
		keys key-lf and if cursor @ 1 -      0 max cursor ! then

		39 for
			i used @ >= if 0 else i input + @ 32 - then
			96 + i 29 tile-grid@ !
		next
		8x8 187 cursor @ 8 * 232 cursor-s >sprite

		4 for sync next
	again
;