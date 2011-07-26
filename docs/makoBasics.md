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

Numbers and Expressions
-----------------------

Mako is a stack-oriented architecture. Instead of storing intermediate values in registers, we push values onto a stack. Operators consume operands from the top of the stack and leave their results behind. Consider the following expression:

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

Simple expressions are not always sufficient for programming- sometimes we need multiple copies of an operand, or we need to discard 'junk'. Fortunately, Forth has a number of operators for manipulating the top of the stack.

`drop` and `dup` discard or duplicate the topmost element of the stack, respectively. `swap` exchanges the top two stack elements. `over` is similar to `dup` but instead pushes a copy of the second element of the stack. `2drop` discards the top two elements of the stack, and 2dup makes a copy of the top two elements of the stack. (Equivalent to the sequence `over over`.) All of these except `2dup` and `2drop` are primitives.

In addition to the main "parameter stack", Mako has a second "return stack". The return stack is primarily used to store the return addresses of subroutine calls, but it can be freely manipulated. `>r` (to-r) and `r>` (r-to) move a value from the parameter stack to the return stack and vice versa. These operators are represented with the assembly opcodes `STR` and `RTS`, respectively. These words are commonly used to provide an intermediate storage location during a complex set of stack manipulations, but if used skillfully they can also provide very flexible new flow-control capabilities. For convinience, Maker also provides the mnemonic `rdrop`, which is equivalent to the sequence `r> drop`.

Forth programmers commonly use _stack effect diagrams_ to illustrate the signature of their words. They consist of a block comment with a list of "before" and "after" stack elements separated by a `--`. The rightmost element of these lists is meant to be read as the topmost element of the stack.

	: sum-cubed ( a b -- c )
		+ dup dup * *
	;

The stack diagram for this example shows at a glance that the word takes two arguments and yields a single result. For complex word definitions it may be helpful to place stack comments throughout your code to remind yourself of what is on the stack.

Here's a summary of the stack manipulation operations we've learned and their stack effect diagrams:

- `dup`   `( a -- a a )`
- `drop`  `( a -- )`
- `swap`  `( a b -- b a )`
- `2drop` `( a b -- )`
- `2dup`  `( a b -- a b a b )`

If we use a pipe (`|`) to separate a view of the data stack from the return stack, we can illustrate the return stack words as well:

- `>r`    `( a | -- | a )`
- `r>`    `( | a -- a | )`
- `rdrop` `( | a -- | )`

Flow Control
------------

(if -if else then loop while until again break for i j next exit)

A Few Definitions
-----------------

(defining words. :const :var :array :data :string :image)
(@ !)

Talking to the Hardware
-----------------------

( basic registers, sync keys )


The Grid
--------

Sprites
-------

Indirection
-----------

(' exec :vector :proto)
