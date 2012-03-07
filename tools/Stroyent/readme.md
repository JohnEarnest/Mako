Stroyent
========

Stroyent is an infix, procedural systems programming language targeting MakoVM. Stroyent syntax superficially resembles that of C, but there are many important differences in the semantics of these languages. This guide will provide an introduction to the Stroyent language and how to take advantage of the MakoVM I/O capabilities.

Getting Started
---------------

SPC, the Stroyent Processing Compiler, is your one-stop shop for compiling and testing Stroyent programs. Start with a basic Hello World:

	include <Print>;
	
	func main() {
		print("Hello, World!");
	}

And then invoke SPC via the included shell script:

	> ./spc demos/Hello.snt --run
	Hello, World!

When SPC is executed with a `--run` flag, it will attempt to compile a program and then immediately execute it. If you instead provide a `--print` flag, a disassembly of your program will be printed to the console. If you specify a second filename, SPC will export a MakoVM memory image after compilation is complete.

Structural Elements
-------------------
Stroyent programs use C-style identifiers which can begin with alphabetic characters or underscores, and subsequent characters can additionally use numbers. All names are case-sensitive. Standard `//` and `/* */` comments work as you would expect. Curly braces are never optional, and most statements are terminated with a semicolon. At the top level of a program, keywords distinguish a number of types of statements. They are as follows:

`include`: Include is used to compile external files. An include statement should be followed by a filename enclosed in quotation marks or, if the file to include is relative to the `/lib/` directory, angle-braces, and terminated with a semicolon. Library references implicitly add the .snt extension.

	include "/dir/File.snt";
	include <Print>;

`image`: Image takes an identifier, a quoted filename and two dimensions specifying the width and height of tiles in an image, much like the maker directive `:image`, and incorporates the provided image into the memory image.

	image crabTiles "crab.png", 32, 32;

`const`: Const is used for defining compile-time constants. Specify a name followed by a literal value or constant expression. Hex constants may be preceded by `0x` and character constants (interpreted as the equivalent ascii value) can be enclosed by single quotes (`'`).

	const A_NUMBER   42;
	const FOO        0xDEADBEEF;
	const Calculated ~(9 * (3+4));
	const Character  'Z';

`var`: Var declares variables. These can take several forms- a single-cell variable, an empty array allocation or an inline array initializer. See the examples below, and note that arrays can only be given one dimension, which must be a constant expression.

	var a := 0;            // single variable
	var b[10];             // empty array (0-filled)
	var c := {1, 2, 3, 5}; // initialized array

`func`: Func is used for defining procedures. The name of the procedure is followed by a parenthesized list of comma-delimited argument names, followed by a block containing the procedure body. The entrypoint for a program has the special name `main`, and should take no arguments. Procedures can be called by placing the name of the procedure in an expression, followed by a parenthesized, comma-separated list of arguments, which will be passed by value. If a procedure is to yield a result directly, the `return` statement may be used with an expression. `return` by itself will simply exit the procedure immediately.

	func foo(arg1) {
		return;
	}

	func bar(a, b) {
		return (a % b) + a;
	}

	func main() {
		var a := 5;

		foo(a); // both calls are equivalent
		foo(5);

		var b := bar(a, 9);
	}

`struct`: Struct is used for declaring structures, which provide syntactic sugar for sizing and accessing fields relative to a pointer. The body of a struct declaration contains a series of field names, which can either be single variables or subscripted to delineate arrays. Once a struct is created, the name of the struct followed by `.size` can be used in a constant expression to represent the length in cells of an instance of the struct. Furthermore, a period followed by the struct name, followed by a period and a field name can be used to get a field address from the base address of the struct. See the examples:

	struct thing {
		a;    // single-cell field
		b[3]; // array field
	}

	var things[10 * thing.size]; // using the struct size
	var theThing[thing.size];

	func uses() {
		var thingFieldA := (&theThing).thing.a; // extracting a field
		(&theThing).thing.a := 66;              // writing to a field
	}

Statements
----------
Blocks, like the body of a procedure definition, can contain a series of statements. These can be procedure calls, as discussed with `func`, assignment statements or one of the following special statement types.

Assignments are expressions of the form `<destination> <assignment op> <source>`, where the destination is a variable or computed address, source is an expression and the assignment op is either the direct assignment operator `:=` or a compound assignment operator- `&=`, `|=`, `^=`, `+=`, `-=`, `*=`, `/=` or `%=`. Compound assignment operators function as one would expect in C.

	var a := 0;
	a := 5; // ordinary assignment
	a += 1; // increment

	// this will write to the _address_ given by the _value_ of a, plus one.
	(a+1) := 5;

	// write 30 to address 10.
	10 := 30;

Var can be used within blocks, with the caveat that you can only allocate single variables in this way and they must be initialized explicitly.

Stroyent provides standard `for` and `while` loops, and `break` and `continue` statements which function as they do in C. Note the third semicolon in the head of the `for` loop. Again, curly braces are never optional in Stroyent.

	for(var x := 0; x < 10; x += 1;) {
		if ((x % 2) = 0) { continue; } 
		printNum(x);
	}

	var y := 1;
	while(true) {
		y += 1;
		if (y > 9) { break; }
	}

Stroyent also provides a standard `if`...`elseif`...`else` mechanism, supporting as many `elseif` clauses as desired. These should function as expected.

	func ack(m, n) {
		if     (m = 0) { return n+1; }
		elseif (n = 0) { return ack(m-1, 1); }
		else           { return ack(m-1, ack(m, n-1)); }
	}

Finally, there is the MakoVM-specific statement `sync`, which interrupts computation to allow the Mako graphics hardware to update the display and re-scan button status.

	while(true) {
		sync;
	}


Binary Operators
----------------
Stroyent has a number of garden-variety comparison operators- `=`, `!=`, `<=`, `>=`, `<` and `>`. These always result in boolean flags- that is, -1 for true and 0 for false. Note that the equal-to operator in Stroyent looks like C's assignment operator.

There are also a host of bitwise and arithmetic operators- `<<`, `>>`, `&`, `|`, `^`, `+`, `-`, `*`, `/` and `%`. Be careful with the bit-shift operators- Mako does not have native shift instructions, so using them may be slower than divides and multiplies. This is potentially surprising to C programmers.

Unary Operators
---------------
`~` is your everyday bitwise not operator. Not much to say here, if you'll forgive the pun.

`*`, like in C, is the dereference operator. Given an address, it will return the value at that slot in memory.

`&`, also like in C, is the address-of operator. Practically speaking, `&` cancels with the outermost `*` in an expression, including the implicit dereference which occurs when a variable is referenced in an expression.

Indexers
--------
Stroyent provides a C-style square-bracket syntax for indexing into arrays. This has a few caveats. Most importantly, only one dimension of indexing is supported. See below for an example of how indexers de-sugar, to better understand how they work.

	const SIZE 11;
	var array[SIZE];

	func init() {
		for(var i := 1; i <= SIZE; i += 1;) {
			var b := 0;

			array[i] := i; // write to array
			b := array[i]; // read from array

			(&array + i) := i;  // equivalent to write
			b := *(&array + i); // equivalent to read
		}
	}


MakoVM Interface
----------------
Like Maker, Stroyent exposes constants for all of the MakoVM registers. Since the constants indicate the address of the register, remember to add an explicit dereference when reading from registers.

	var oldClearColor := *CL;
	CL := 0xFF00FF00; // make the clear color green

The `/lib/` directory contains analogs of the support libraries provided for Maker Forth including `Print`, which is for printing to the debug port, `Grid`, which is for manipulating the MakoVM grid, `Sprite`, full of sprite utility routines and `String`, which has routines useful for both memory and string manipulation.

Take a look at the examples in the `/demos/` directory for a deeper look at how Stroyent can be used in practice.