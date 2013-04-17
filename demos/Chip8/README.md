Chip 8
======

Chip8 is a Mako-based implementation of an ancient VM designed to ease the programming of video games on early 8-bit microcomputers. In some ways it's a spiritual predecessor of Mako.

While Chip8 implementations are available for a wide variety of platforms today, there seems to be a dearth of original software. In the interest of doing something about this (and more importantly, having fun), I've designed a high-level assembler named Octo which can produce Chip8 roms.

Octo
----

Invoking Octo is very simple- compile it and then provide a source filename as a commandline argument:

	javac Octo.java
	java Octo OctoExample.8o

You'll see a hex dump of the rom printed (16 bits at a time, to correspond to the size and alignment of chip8 instructions), and a binary will be saved with a `.ch8` extension:

	124E FCFC FCFC FCFC FC00 7105 6303 8302
	3303 00EE 610A 7206 00EE 6000 610A 6202
	F029 D125 220A 7001 4010 122E 1220 00EE
	6305 611F 6201 A202 D127 7101 7201 F00A
	F318 F029 D125 F00A D125 1240 00EE 221A
	2230 00EE

Octo's syntax is based on Forth- a series of whitespace-delimited tokens. Subroutines are defined with `:` followed by a name, and simply using the name will perform a call. `;` terminates subroutines with a return. `#` indicates a single-line comment. Numbers can use `0x` or `0b` prefixes to indicate hexadecimal or binary encodings, respectively. Whenever numbers are encountered outside a statement they will be compiled as literal bytes. Names must always be defined before they can be used- programs are written in "reading" order. An entrypoint named `main` must be defined.

In the following descriptions, `vx` and `vy` refer to some register name (v0-vF), `i` refers to a (forth-style) identifier and `n` refers to some number.

Statements
----------
These mostly have a 1:1 mapping to chip8 opcodes:

- `:const i n`    declare a constant with some numeric value.
- `:data i`       declare a label- can be used like a constant.
- `exit`          return from the current subroutine.
- `cls`           clear the screen.
- `bcd vx`        decode vx into BCD at A, A+1, A+2.
- `save vx`       save registers v0-vx to A.
- `load vy`       load registers v0-vx from A.
- `draw vx vy n`  draw a sprite at x/y position, n rows tall.
- `jump0 n`       jump to address n + v0.

Assignments
-----------
The various chip8 copy/fetch/arithmetic opcodes have been abstracted to fit into a consistent `<dest-reg> <operator> <source>` format. For some instructions, `<source>` can have several forms.

- `dt := vx`      set delay timer to register value.
- `st := vx`      set sound timer to register value.
- `a  := n`       set A to constant.
- `a  := hex vx`  set A to hex char corresponding to register value.
- `a  += vx`      increment A by a register value.
- `vx := vy`      copy register to register.
- `vx := n`       set register to constant.
- `vx := rnd n`   set register to random number AND n.
- `vx := dt`      set register to delay timer.
- `vx := key`     block for a keypress and then store code in register.
- `vx += n`       add constant to register.
- `vx += vy`      add register to register.
- `vx -= vy`      subtract register from register.
- `vx |= vy`      bitwise OR register with register.
- `vx &= vy`      bitwise AND register with register.
- `vx ^= vy`      bitwise XOR register with register.
- `vx >> n`       right shift by constant.
- `vx << n`       left shift by constant.

Control Flow
------------
The Chip8 conditional opcodes are all conditional skips, so Octo control structures have been designed to map cleanly to this approach. `if` conditionally executes a single statement (which could be a subroutine call or jump0), and `loop...again` is an infinite loop which can be exited by one of any contained `while` conditional breaks. Loops can be nested as desired. `if` and `while` should each be followed by a conditional expression. Conditional expressions can have one of the following six forms:

- `vx == n`
- `vx != n`
- `vx == vy`
- `vx != vy`
- `vx key` (true if the key indicated by vx is pressed)
- `vy -key` (true if the key indicated by vy is not pressed)

Here are some examples:

	if v3 != 3 then exit
	loop
		v1 += 1
		while v1 != 10
		v1 += 2
		while v1 != 10
	again
