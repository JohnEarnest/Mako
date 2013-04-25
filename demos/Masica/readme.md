MASICA
======

An educational interactive TinyBASIC interpreter. MASICA is designed to provide an absolute minimum of features and complexity while including what is necessary to illustrate core imperative programming concepts such as variables, iteration and conditional branches. Variable names are single letters.

The MASICA interface is similar to that of early 8-bit microcomputers- a prompt which can evaluate commands immediately or store a sequence of commands prefixed with line numbers. Several commands for removing or editing existing lines are also provided.

Statements
----------
- `print <expr>` Evaluates and prints an expression to the terminal. `print` can be provided with multiple comma-separated expressions, which will be printed with spaces between them. `print` inserts a newline at the end of the statement unless a `;` is at the end of the line.
- `input <var>` Reads a token of input and stores it in the named variable.  Values may be numbers or a string- see `num()` and `str()` below. Like `print`, multiple comma-separated variables may be read in a single line.
- `goto <line>` Continues execution at a specified line number. Note that <line> must be a literal number, not an expression.
- `if <expr> then <statement>` Conditional execution. <expr> must provide a boolean value, and <statement> can be any statement including another `if`.
- `let <var> = <expr>` Variable assignment. The word `let` can be omitted optionally.
- `end` Halt execution.

Expressions
-----------
MASICA uses a simple dynamic type system- expressions can return strings, numbers or boolean values, and variables can hold any of these types. Predicates are provided for querying the type of an expression in the cases where this matters. Strings are enclosed in double-quotes (`"`), numbers are signed 32-bit integers and the words `true` and `false` are recognized as boolean literals.

The operators `+` `-` `*` `/` and `%` are implemented with normal arithmetic precedence, and parentheses can be used to further control evaluation order. The comparison operators `==` `!=` `>` `<` `>=` and `<=` are chosen to correspond to the syntax of C and Java. The boolean operators `and` `or` and `not` may only be applied to boolean arguments. Comparison operators are overloaded to compare strings (lexicographically) in addition to numbers. The `+` operator is also overloaded to perform string concatenation.

In addition to these simple operators, MASICA provides a few intrinsic functions:

- `num(<expr>)` returns true if the expression evaluates to a number.
- `str(<expr>)` returns true if the expression evaluates to a string.
- `bool(<expr>)` returns true if the expression evaluates to a boolean.
- `rnd(<expr>)` returns a random number between 0 (inclusive) and the argument (exclusive).
- `abs(<expr>)` returns the absolute value of the argument.
- `min(<expr>,<expr>)` returns the minimum of two arguments.
- `max(<expr>,<expr>)` returns the maximum of two arguments.

Editor Commands
---------------
The following editor commands are provided to ease programming:

- `help` provide a list of available commands.
- `new` erase the current program.
- `list` print, in order, all the lines of the current program.
- `run` execute the current program starting at the lowest-numbered line.
- `edit <line>` copy a specified line to the edit buffer, allowing the user to make modifications and re-enter it. This can be used either to revise existing lines or as a copy-and-paste functionality by editing the line number preceding the line.
- `erase <line>` remove a specified line from the program listing.

Examples
--------

Hello, World:

	10 print "Hello, World!";
	20 goto 10

Die Roller:

	10 c = 10
	20 print rnd(6)+1;
	30 c = c - 1
	40 if c > 0 then goto 20

Lunar Lander:

	10 a = 100 + rnd(20)
	11 v = rnd(5)
	12 f = 80 + rnd(10)
	20 print "a: ",a,"v: ",v,"f: ",f
	30 print "thrust? ";
	40 input t
	50 if t > f  then t = 0
	60 if t > 30 then t = 30
	70 f = f - t
	80 v = v + 4 - t
	90 a = a - v
	100 if a >  0 then goto 20
	110 if v >  5 then print "CRASH!"
	120 if v <= 5 then print "success!"
