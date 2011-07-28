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

Forth programs are made out of whitespace delimited tokens called _words_. Tokens can contain any combination of characters. Strings are a special exception, and include any text enclosed within double quotes. When a word's name is encountered, it is executed. Some special words are carried out at compilation time, while most are carried out at runtime.

The first word in this program is `:include`. By convention, words beginning with a colon are related to the compiler and have compile-time semantics. `:include` instructs the compiler to load the source file specified by the following string, in this case `Print.fs`, a library for console output.

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

The only definintion here we're really interested in is that of our `main` word. We can instruct Maker to only print a disassembly of a specific word by using the `--word` flag and specifying the word we want to see:

	java Maker examples/Hello.fs --word main
	00117: (main)           JUMP 133
	00119:                 "Hello, World!"
	00133:                 CONST 119
	00135:                  CALL 112 (typeln)
	00137:                  JUMP -1
	00139:                   RET

	2671 words, 10.434 kb.

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

Pure functions and combinators are all well and good, but to do much of interest we're going to need some flow control structures.

__if...else...then__

This is your basic bread and butter _if_ statement. The word `if` consumes a value from the stack and carries out some operations until a matching `then` statement if this value is not zero. An `else` is optional.

	: double-if-two ( a -- b )
		dup 2 = if dup + then
	;

	: odd? ( n -- flag )
		2 mod if true else false then
	;

Sometimes it's convenient to be able to check if something _is_ zero- Maker also provides a negated if statement called `-if` which can be used identically with `else` and `then`:

	: = ( a b -- flag )
		xor -if true else false then
	;

(Incidentally this is the most common idiom for confirming that two values are equal in Maker. The `=` definition is a handy utility routine.)

__loop...while/until/again__

The `loop` words are the Forth equivalent of C's `do { ... } while();` construct. `loop` is always paired with `while`, `until` or `again`. `while` and `until` consume a value from the stack and cause execution to jump back to the matching `loop` if this value is zero or nonzero, respectively. `again` is used for infinite loops and jumps back to the matching `loop` unconditionally.

	: palindrome? ( n -- flag )
		dup >r 0
		loop
			base * over base mod + swap
			base / swap
			over
		while
		swap drop r>
		xor -if true else false then
	;

	: stack-overflow ( -- )
		loop
			42
		again
	;

If you want to exit a loop early, the `break` word is what you're looking for- it will jump past the current loop's matching `while`, `until` or `again`.

	: count-to-five ( -- 0 1 2 3 4 5 )
		0 loop
			dup 1 +
			dup 4 > if break then
		again
	;

__for...next__

The Forth `for` loop is an easy way to repeat an operation several times. `for` reads a value from the stack, storing it as an index counter on the return stack. Every time `next` is encountered, this counter is examined. If the counter is greater than or equal to zero, execution resumes after the matching `for`.

	: five-copies ( a -- a a a a a )
		3 for
			dup
		next
	;

If you need to access the loop index, `r>` and `>r` can do the trick, but Maker also provides the words `i` and `j`, which will push a copy the top or second elements of the return stack to the parameter stack. You can thus easily nest two `for` loops while still accessing loop indices.

	: multiplication-table ( -- )
		10 for
			10 for
				i j *
				.      # a word from Print.fs for printing a number to stdout
			next
			cr         # another routine for printing a carriage return
		next
	;

Finally, `exit` can be used to return immediately from the current definition. If you use `exit` in the middle of a `for` loop be sure you first remove the loop index from the return stack!

A Few Definitions
-----------------

Defining words are words that create other words, much like `:`. Some Forth implementations allow you to create your own defining words, which can be a very powerful metaprogramming facility. Maker does not provide this functionality, but it does come packed with a rich set of built-in defining words.

`:const` is used to declare constants. It requires a name and a numerical value, which can be based on other constants, variables or word definitions. When the name of a constant is refrenced, the value of the constant will be inlined.

	:const byte-size 255
	:const hexconst  0xDEADBEEF

`:var` declares a single-cell (one 32-bit word) variable. It requires only a name, and will be initialized to a value of zero. When the name of a var is refrenced, the _address_ at which this variable is stored will be inlined. The word `@` can be used to fetch the value stored at an address, while the word `!` can be used to store a value to an address. `@` and `!` form the basis of all memory operations in Forth.

	:var storage
	: toggle ( -- )
		storage @	# load the value in storage
		not			# negate the value from storage
		storage !	# store the negated value to the var again
	;

`:array` is much like `:var`, except it is used to allocate a group of several cells at once. It requires a name, a number of cells and a value with which to initialize the array.

	:array buffer 256  0  # 256-element array filled with 0
	:array tile    64 -1  #  64-element array filled with -1

`:data` is the most freeform approach to describing data. It requires only a name. When numbers or word names (including code or constants) are encountered outside a word definition, it is simply appended literally into the MakoVM ROM.

	# initializing an array
	:data data-array  0 1 2 45 23 12 11

	# building a lookup table of function pointers
	: func1 + ;
	: func2 - ;
	: func3 * ;
	:data ftable
		43 func1
		45 func2
		42 func3

`:string` defines a named string. This is an alternative to the inline form described earlier which and be referenced from several places instead of only once. String definitions require a name and a quoted string, which will be stored with a null-terminator.

	:string hello "Hello, World!"

`:image` is used for including image data from external files. It requires a name, a quoted filename and a horizontal and vertical size (in pixels) of the tiles of the image. The pixels of the image will be stored one 32-bit color pixel to a cell in, one tile after another. Images will be discussed in more detail later on.

	# load a sheet of 16 pixel wide by 32 pixel tall tiles
	:image sprite-tiles "Scrubby.png"  16 32

Talking to the Hardware
-----------------------

Mako uses a single, contiguous addressing space for memory. The first dozen or so memory locations contain _registers_ which control I/O devices and dictate the layout of important memory regions. Maker exposes constants which allow you to treat registers as Forth variables. In this section we will deal with the most basic of these registers, and later we will discuss the specialized graphics registers controlling the Grid and Sprites.

`PC` contains Mako's program counter. Maker initializes this register to point to the first byte of a word called `main`, our entrypoint.

`DP` contains the address of the top element of the parameter stack, and `RP` does likewise for the return stack. Both stacks grow downwards- that is, the stack pointers are incremented as elements are pushed onto the stack. Maker allocates 50 cells for each by default, but this behavior can be overridden if you create your own array definitions with the names `data-stack` or `return-stack`.

	: pick ( index -- element )
		DP @ 2 - swap - @
	;

`RN` is a memory-mapped random number generator. Loading from this register will return a random 32-bit integer. Storing to this register will have no effect.

	: random ( max -- n )
		RN @ swap mod
	;

`KY` is similar to `RN`- a memory-mapped peripheral. Loading from this register will return a bit-vector representing the state of Mako's keypad. Maker provides the following masking constants for extracting individual keys: `key-up`, `key-dn`, `key-lf`, `key-rt`, `key-a` and `key-b`. The keys associated with these bits may vary from implementation to implementation, but the first four should generally be some sort of directional pad. Maker also offers the convenient word `keys` which is equivalent to `KY @`.

	: left-key-pressed?
		KY @ key-lf and if true else false then
	;

`CO` is a (possibly) bidirectional debug port. Writing a value to this address should print the corresponding ASCII character to stdout. Some implementations may also support reading from this register to grab input from stdin. The `Print.fs` and `String.fs` standard library files contain useful definitions for printing values and reading values with the debug port, respectively. When it doesn't make sense, or for simplicity, MakoVM implementations may choose to do nothing when this register is manipulated- as the name would suggest, it's mainly for debugging.

The Grid
--------

Mako has a 320x240 pixel display. Every time the `sync` instruction is executed, the VM pauses execution and the display is refreshed. First, the screen buffer is cleared with the background color, then the grid is drawn, then sprites are drawn and the completed image is made visible. It's worth noting that _the bit vector provided by querying `KY` is only updated when `sync` is called._

The clear color is stored in the `CL` register, and by default is 0xFF000000. Like all Mako color values, the clear color is stored as an ARGB packed integer, with 8 bits per color channel. If the alpha channel has a value other than 0xFF, a pixel is considered transparent.

The Grid itself consists of a matrix of 8x8 pixel tiles. Given a 320x240 display, this means 40x30 tiles are visible on-screen at once. The `GP` register points to a block of memory containing a sequential list of tile indices, counting from left to right and top to bottom. These indices indicate an offset added to the contents of the `GT` register, which should point to a set of sequential 64 word blocks, each representing the pixels of a tile. Tile indices count from zero, and tiles with a negative index are not drawn. Tiles can also contain transparent pixels.

The Grid can also be scrolled. The `SX` and `SY` registers contain offsets added to the screen coordinates of tiles before they are drawn. Mako actually renders a 41x31-tile area based on the position of `GP`. By shifting `GP` every time the screen is scrolled 8 pixels with the scroll registers it is possible to produce a free-scrolling grid of arbitrary size- take a look at the `OpenWorld.fs` example to see how this technique works.

The `GS` register is also key for scrolling displays. When the grid is rendered, 41 cells making up a row of the display are fetched at a time, and then 41 plus the value of the `GS` register is added to a counter. If the horizontal rows of a tilemap in memory are wider than a single display, `GS` can be used to compensate for this so that the next row of tile data is read from the correct position.
	
	# look up a tile in the current grid
	# by its x and y coordinates.
	: tile-grid@ ( x y -- tile-address )
		GS @ 41 + * swap + GP @ +
	;

Much like the return stack and data stack, if a grid and grid tileset are not defined explicitly, Maker will automatically allocate space for them. You can define your own grid and choose the initial configuration of the scroll and skip registers by defining arrays and constants with the names 'grid', 'grid-tiles', 'grid-skip', 'scroll-x', 'scroll-y' or 'clear-color'.

The standard library file `Grid.fs` contains a number of useful words for manipulating the grid.

Sprites
-------

In addition to the Grid, Mako has 256 sprites. Sprites are drawn in the order of their indices, from back to front. Also like the grid, sprite data is stored in a special block of memory- here indicated by the `SP` register. The sprite table consists of 256 entries which are 4 cells long, for a total of 1024 cells. The meaning of the fields of an entry are as follows:

0) Status flags. If the least significant bit is 0, the sprite is not drawn. Bits 15-12 and 10-8 indicate the height and width of the sprite, respectively, in increments of 8 pixels, with a minimum size of 8 on either axis. This means the maximum size of a sprite is 64x64 pixels. Maker provides constants for sprite sizes of the form "NxM" for every valid combination of sizes- for example `8x8`, `16x32` and `48x64`. Bits 16 and 17 indicate that a sprite should be mirrored horizontally and/or vertically, respectively. Maker provides bitmasks for these fields called `sprite-mirror-horiz` and `sprite-mirror-vert`.

	# given the status field, compute
	# the width and height of a sprite in pixels:
	: sprite-w  0x0F00 and  256 / 1 + 8 * ;
	: sprite-h  0xF000 and 4096 / 1 + 8 * ;

1) Tile index. An offset from the Sprite Tile register, `ST`. Tiles are zero-indexed and indexed based on the size of the current sprite, so tile 2 for an 8x8 sprite begins at SP+128, while tile 2 for an 8x16 sprite begins at SP+256.

2) X position. The value of `SX` and `SY` are subtracted from the X and Y position of a sprite to determine the final screen coordinates of a sprite.

3) Y position.

	# configure sprite 9 as a 24x32 sprite,
	# flipped horizontally and showing tile 37
	# in the bottom right corner of the screen.
	: showNine ( -- )
		9 4 * SP @ +
		24x32 sprite-mirror-horiz or over !
		37  over 1 + !
		296 over 2 + !
		208 swap 3 + !
	;

Also like the stacks and grid, Maker allocates memory for the sprite table if it is not explicitly allocated by the programmer. If you want to do it manually, use the names `sprites` and `sprite-tiles` for the sprite table and sprite tileset.

The standard library file `Sprite.fs` contains many useful words for manipulating sprites, including collision detection facilities.

Special Tricks
--------------

Maker has a few more spiffy capabilities that deserve brief mention.

`'` (pronounced "tick"), followed by a word name will, instead of compiling a `CALL` to the word definition, push the address of the first byte of the word definition. You can then call a dynamic address with `exec`. These instructions are useful for function pointer style tricks and supplying words with predicates.

	: print-A 65 CO ! ;
	: deferred-print ( -- )
		print-A          # the normal approach
		' print-A exec   # again with indirection
	;

Maker is a single-pass compiler, so everything must be defined before it can be used. In general, this just means your source files should be arranged in a logical reading order, but for mutually-recursive functions this is a problem. The `:proto` word defines a _prototype_ for a word that will be defined later. It only makes sense to do this for code definitions, and in all cases `:proto` should be used sparingly for purposes of readability. Maker will complain if you use `:proto` but fail to supply an implementation for the prototype.

	:proto defined-later
	: defined-first   defined-later 2 - ;
	: defined-later   42 78 +           ;

Finally, `:vector` is identical to `:` but starts a word definition with a preamble that makes it possible to _revector_ the word later, dynamically changing the behavior of calls to the word. See the standard library file `Vector.fs` for support code, examples and a more detailed explanation.

Forth Philosophy
----------------

Forth is as much an engineering approach as it is a programming language. When you're writing Forth programs, keep these ideas in mind:

1) __Factor.__ Avoid long word definitions. Break complex words down into simple words which can be reused. Subroutine calls are cheap in Forth- use them freely.

2) __Keep the stack shallow.__ If a word takes more than 3 arguments, consider breaking it down into smaller definitions. If you're having trouble remembering what's on the stack, it may be another sign you're doing too much in one place. You may be able to replace some arguments with constants by writing a less general word or simplify flow by introducing global variables.

3) __Solve only the problem at hand.__ Don't overgeneralize your solutions or leave hooks for future expansion. If you don't need it now, there's a good chance you don't need it at all. Focused solutions lead to simpler code.

4) __Change the problem.__ Sometimes the key to simplicity is realizing a problem doesn't need to be solved at all. Examine your underlying assumptions. Are there details of your model that can be left out? Features that are unnecessary? Can you use a different data structure or algorithm to accomplish a similar goal?