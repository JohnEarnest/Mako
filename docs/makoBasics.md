An Introduction to MakoVM
=========================

MakoVM is an extremely simple stack-based virtual machine intended for the creation of video games. This tutorial will explain the workings of Mako, and at the same time introduce Maker, a Forth-inspired compiled language targeting Mako. While it is entirely possible to compile existing languages to MakoVM bytecodes, Maker code has a very tight relationship with Mako and makes it easy to explain the "hardware" on a low level.

First Steps
-----------

Let's begin with a simple "Hello, World" program:

	:include "Print.fs"
	: main
		"Hello, World!" typeln
		halt
	;

Forth programs are made out of whitespace delimited tokens called _words_. Tokens can contain any combination of characters. Strings are a special exception, and include any text enclosed within single quotes.

The first word in this program is `:include`. By convention, words beginning with a colon are related to the compiler. `:include` instructs the compiler to load the source file specified by the following string, in this case `Print.fs`, a library for console output.

We then reach the word `:`, which defines a word. The next token is used as a name. The name `main` is special, and will always be used as the entrypoint to a program. `:` should always be paired with a `;`, which terminates a definition.

The string `"Hello, World!"` will compile an inline string constant and leave the address of this string on the stack, to be used by the `typeln`	word to print a sequence of characters to stdout. Finally, the word `halt` ceases execution.

To run this example, place your code in the `examples` directory in a file called `Hello.fs`, and type the following:

	java Maker examples/Hello.fs --run

We see a whole bunch of output- a disassembly of our compiled program followed by the text "Hello, World!":

	00000: (PC)            117
	00001: (DP)            140
	00002: (RP)            190
		-- snip --
	00112: (typeln)         CALL 94 (type)
	00114:                  CALL 20 (cr)
	00116:                   RET
	00117: (main)           JUMP 133
	00119:                 "Hello, World!"
	00133:                 CONST 119
	00135:                  CALL 112 (typeln)
	00137:                  JUMP -1
	00139:                   RET
	00140: (data-stack)    <<< 50 words >>>
	00190: (return-stack)  <<< 50 words >>>
	00240: (grid)          <<< 1271 words >>>
	01511: (grid-tiles)    <<< 64 words >>>
	01575: (sprites)       <<< 1024 words >>>
	02599: (sprite-tiles)  <<< 64 words >>>
	
	2663 words, 10.402 kb.
	Hello, World!

Even though "Hello, World!" programs are a common starting point for teaching programming languages, this one has a fair amount of subtlety to it. Let's start over with the basics.

Actual First Steps
------------------

Mako is a stack-oriented architecture. Instead of storing intermediate values in registers, we push values onto a stack and operators consume operands from the top of the stack. Consider the following expression:

	2 3 +

The first two tokens are numbers, so they push their values onto the stack. The word `+` consumes the top two elements of the stack, adds them, and leaves the result on the stack. If you've ever used an RPN calculator, this kind of postfix math will seem pretty familiar.

Maker provides the following words without the use of any library, and all but the last two map directly to a single MakoVM bytecode:

- `+` (addition)
- `-` (subtraction)
- `*` (multiplication)
- `/` (integer division)
- `mod` (modulus)
- `and` (bitwise AND)
- `or`  (bitwise OR)
- `xor` (bitwise XOR)
- `not` (bitwise NOT)
- `<` (less-than)
- `>` (greater-than)
- `<=` (less-than or equal to)
- `>=` (greater-than or equal to)

Given a definition like the following:

	: operation 2 * 5 + not ;

The resulting disassembly might look like this:

	00126: (operation)     CONST 2
	00128:                   MUL
	00129:                 CONST 5
	00131:                   ADD
	00132:                   NOT
	00133:                   RET

`CONST` is a single bytecode that is followed by a parameter whose value will be pushed onto the stack. `MUL`, `ADD` and `NOT` are the assembly mnemonics corresponding to `*`, `+` and `not`, respectively. `RET` returns from a subroutine call, and will be discussed in more detail later.

Each element of the stack is a 32-bit two's complement signed integer. The comparison operations (greater-than, less-than and so on) result in 0 (all bits 0) for false, and -1 (all bits 1) for true, making it possible to perform a variety of useful bitwise operations on the results. These _boolean flags_ are used by convention to represent logical true and false, and the words `true` and `false` can be used in place of the numeric constants.

Numeric constants can be given as signed decimal integers (as above) or as hexadecimal or binary values. Hexadecimal values are prefixed with `0x`, and binary values are prefixed with `0b`:

	0 -31 45           # decimal constants
	0xC0FFEE 0xA 0x34  # hexadecimal constants
	0b110001 0b0 0b01  # binary constants

Finally, note that in the above example we've used `#` to delimit single-line comments. The paretheses, `(` and `)`, are used to delimit multiline comments. Multiline comments may not be nested.

Stack Effects
-------------

(drop dup over swap 2dup 2drop)
(>r r> rdrop)

Flow Control
------------

(if -if else then loop while until again break for i j next exit)

A Few Definitions
-----------------

(defining words. :const :var :array :data :string :image)
(@ !)

Indirection
-----------

(' exec :vector :proto)

Talking to the Hardware
-----------------------

( basic registers, sync keys )


The Grid
--------

Sprites
-------
