######################################################
##
##  Chip8
##
##  An implementation of the Chip8 virtual machine.
##  The details of 'key8' will need to be customized
##  slightly on a per-game basis- keyboard layouts
##  seem quite heterogenous. The 'SPEED' constant
##  controls how many instructions are executed per
##  frame (60hz)- somewhere around 8 works well for
##  pong and breakout while more sophisticated games
##  and demos want something more like 32.
##
##  I've checked in a few testing ROMs found from
##  http://chip8.com/ - see there for more details.
##  Next step: write some Chip8 games of my own!
##
##  John Earnest
##
######################################################

:const DRAW  0xFFFFCC00
:const BACK  0xFF996600
:const SPEED 7

: inc  dup @ 1 + swap ! ; ( addr -- )
: dec  dup @ 1 - swap ! ; ( addr -- )

:array m 432 0
:data chars
	0xF0 0x90 0x90 0x90 0xF0 # 0
	0x20 0x60 0x20 0x20 0x70 # 1
	0xF0 0x10 0xF0 0x80 0xF0 # 2
	0xF0 0x10 0xF0 0x10 0xF0 # 3
	0x90 0x90 0xF0 0x10 0x10 # 4
	0xF0 0x80 0xF0 0x10 0xF0 # 5
	0xF0 0x80 0xF0 0x90 0xF0 # 6
	0xF0 0x10 0x20 0x40 0x40 # 7
	0xF0 0x90 0xF0 0x90 0xF0 # 8
	0xF0 0x90 0xF0 0x10 0xF0 # 9
	0xF0 0x90 0xF0 0x90 0x90 # A
	0xE0 0x90 0xE0 0x90 0xE0 # B
	0xF0 0x80 0x80 0x80 0xF0 # C
	0xE0 0x90 0x90 0x90 0xE0 # D
	0xF0 0x80 0xF0 0x80 0xF0 # E
	0xF0 0x80 0xF0 0x80 0x80 # F
:binary prog
	"Maze.ch8"
	#"Pong.ch8"
	#"Trip8.ch8"
1 3584

:var   I       # memory addresses
:var   pc      # program counter
:array sa 16 0 # call stack
:data  sp sa   # stack pointer
:array v  15 0 # registers (v0-vE)
:var   Vf      # carry register
:var   dt      # delay timer
:var   st      # sound timer

######################################################
##
##  Graphics Subsystems:
##
######################################################

:include <Sprites.fs>
:include <Bitmap.fs>
:const SCALE 5
: >x  64 mod SCALE *      ; ( logical-x -- screen-x )
: >y  32 mod SCALE * 40 + ; ( logical-y -- screen-y )

:var px
:var py
: fatbit ( val -- )
	SCALE 1 - for
		SCALE 1 - for
			dup
			i px @ +
			j py @ + pixel !
		next
	next drop
;

: clear ( -- )
	31 for
		i >y py !
		63 for
			i >x px !
			BACK fatbit
		next
	next
;

######################################################
##
##  Core Opcodes:
##
######################################################

: jj       pc @ m + 2 - @  ; ( -- 8bit )
: kk       pc @ m + 1 - @  ; ( -- 8bit )
: op       jj 16 / 0xF and ; ( -- 4bit )
: x        jj      0xF and ; ( -- 4bit )
: y        kk 16 / 0xF and ; ( -- 4bit )
: n        kk      0xF and ; ( -- 4bit )
: nnn      x 256 *  kk  or ; ( -- 12bit )

: Vx               x v + @ ; ( -- n )
: Vy               y v + @ ; ( -- n )
: Vx!     0xFF and x v + ! ; ( n -- )
: Vf!     1 and Vf !       ; ( n -- )
: skip    pc @ 2 + pc !    ; ( -- )
: carry?  dup 255 > Vf!    ; ( n -- n )
: msb     128 / 1 and      ; ( 8bit -- 1bit )
: lsb           1 and      ; ( 8bit -- 1bit )

: op1                     nnn pc ! ; # jump
: opB  nnn v @ + pc !              ; # jump + V0
: op2  sp inc pc @ sp @ ! nnn pc ! ; # call
: op3  Vx kk  = if skip then       ; # skip if equal
: op4  Vx kk != if skip then       ; # skip if not equal
: op5  Vx Vy  = if skip then       ; # skip if reg equal
: op9  Vx Vy != if skip then       ; # skip if reg not equal
: op6  kk      Vx!                 ; # constant to reg
: opA  nnn I !                     ; # constant to I
: op7  kk Vx + Vx!                 ; # add immediate
: opC  RN @ kk and Vx!             ; # random number (masked)

: m0   Vy                  Vx!     ; # Vx  = Vy
: m1   Vx Vy or            Vx!     ; # Vx |= Vy
: m2   Vx Vy and           Vx!     ; # Vx &= Vy
: m3   Vx Vy xor           Vx!     ; # Vx ^= Vy
: m4   Vx Vy + carry?      Vx!     ; # Vx += Vy, VF = carry
: m6   Vx lsb  Vf! Vx 2 /  Vx!     ; # Vx /= 2, roll into VF
: mE   Vx msb  Vf! Vx 2 *  Vx!     ; # Vx *= 2, roll into VF
: m5   Vx Vy > Vf! Vx Vy - Vx!     ; # Vx = Vx - Vy, VF = not borrow
: m7   Vx Vy < Vf! Vy Vx - Vx!     ; # Vx = Vy - Vx, VF = not borrow
:data mt  m0 m1 m2 m3 m4 m5 m6 m7
          -1 -1 -1 -1 -1 -1 mE -1
: op8  n mt + @ exec ;

######################################################
##
##  IO Opcodes:
##
######################################################

: bcd ( -- )
	Vx 100 / 10 mod I @ m +     !
	Vx  10 / 10 mod I @ m + 1 + !
	Vx       10 mod I @ m + 2 + !
;

: save     for i v + @ i I @ + m + ! next ; ( -- )
: restore  for i I @ + m + @ i v + ! next ; ( -- )

: key8 ( -- n )
	keys key-up and if 1 exit then
	keys key-dn and if 4 exit then
	keys key-lf and if 6 exit then
	keys key-rt and if 8 exit then
	0
;

:proto tick
: opF
	kk 0x0A = if loop tick key8 until key8 Vx!  exit then # wait for key
	kk 0x07 = if dt @ Vx!                       exit then # get delay timer
	kk 0x15 = if Vx dt !                        exit then # set delay timer
	kk 0x18 = if Vx st !                        exit then # set sound timer
	kk 0x1E = if Vx I @ + I !                   exit then # I += Vx
	kk 0x29 = if Vx 0xF and 5 * chars + m - I ! exit then # I = hex_digit(Vx)
	kk 0x33 = if bcd                            exit then # bcd display
	kk 0x55 = if x save                         exit then # dump V0-Vx to memory starting at I
	kk 0x65 = if x restore                      exit then # restore V0-Vx from memory starting at I
;

: opE
	kk 0x9E = if key8 Vx  = if skip then        exit then # skip if key Vx is pressed
	kk 0xA1 = if key8 Vx != if skip then        exit then # skip if key Vx is not pressed
;

: op0
	kk 0xE0 = if clear                          exit then # clear the display
	kk 0xEE = if sp @ @ pc ! sp dec             exit then # return
;

: pixel8 ( x y -- )
	>y py ! >x px !
	px @ py @ pixel @ DRAW = if
		1 Vf ! BACK
	else
		DRAW
	then fatbit
;

: row8 ( y bits -- )
	7 for
		dup 1 and if
			over Vx i + swap pixel8
		then
		2 /
	next 2drop
;

: opD # sprite drawing
	0 Vf !
	n 1 - for
		i Vy +
		I @ i + m + @ row8
	next
;

######################################################
##
##  Interpreter
##
######################################################

:data ops  op0 op1 op2 op3 op4 op5 op6 op7
           op8 op9 opA opB opC opD opE opF
: step     skip op ops + @ exec ; ( -- )

: tick ( -- )
	dt @ if dt dec then
	st @ if
		st dec
		143 for i 32 mod AU ! next
	else
		143 for 0        AU ! next
	then
	sync
;

: main ( -- )
	init-fullscreen clear
	0x200 pc !
	loop SPEED for step next tick again
;

