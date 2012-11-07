Loko
====

Loko is an interactive interpreter for the Logo programming language. Logo is best known for beginner-oriented "Turtle Graphics" functionality, but internally the language is actually a dynamically-scoped member of the Lisp family. As such, Logo provides sophisticated list manipulation facilities and support for higher-order functions.

All values in Logo are either signed integers, _words_ or _lists_. Words can contain alphanumeric characters as well as `.,?!`, and when a word is encountered it is executed. If a word is prefaced with a `'`, it instead represents the _name_ of the word. If a word is prefaced with a `:`, it is a variable, and represents the value associated with the given name. Lists are enclosed in square brackets and none of the words within a list are executed immediately.

Logo expressions are given in prefix-notation, with the arguments to the right of the operators. Several operators (`+-*/%<>=`) also have an optional infix syntax. Infix expressions must always be separated from prefix expressions by parentheses.

Turtle Graphics
---------------

At startup, Loko awaits instructions in a scrolling command-line mode. The `showturtle` command switches to Turtle Graphics mode and `hideturtle` switches back. Turtle Graphics centers around manipulating the triangular object on the screen (the turtle) by instructing it to move or turn. As the turtle moves around the screen, it draws a line- there are commands for disabling the pen or changing the color it draws with. Instructions can be entered individually or together on a single line:

	showturtle
	forward 100
	right 75 forward 50 right 30 forward 20

- `showturtle` enable turtle graphics.
- `hideturtle` disable turtle graphics.
- `forward` or `fd` move the turtle forward by a specified number of pixels.
- `back` or `bk` move the turtle backward by a specified number of pixels.
- `right` or `rt` turn the turtle right by a specified number of degrees.
- `left` or `lt` turn the turtle left by a specified number of degrees.
- `clear` or `cs` clear the screen.
- `home` move the turtle to the center of the screen.
- `heading` return the angle of the turtle in degrees.
- `setheading` or `seth` set the angle of the turtle in degrees.
- `xcor` return the x position of the turtle.
- `ycor` return the y position of the turtle.
- `pos` return a list containing the turtle's coordinates.
- `setx` set the x position of the turtle.
- `sety` set the y position of the turtle.
- `setpos` given a list of coordinates, set the turtle's position.
- `penup` or `pu` tell the turtle to stop drawing a line as it moves.
- `pendown` or `pd` resume drawing a line as the turtle moves.
- `setcolor` given a number between 0 and 255, set the pen's grayscale value.

Defining Procedures
-------------------

Procedures are defined by using the `to` keyword, specifying a series of arguments, and a procedure body, terminated by `end`. When a line beginning with `to` is entered at the Loko command prompt, the line editor will expand and allow the procedure to be freely edited with the keyboard and cursor keys. If return is pressed with the last line of this editor selected, the procedure will be entered and further commands can be issued. Previously entered procedures can be modified by using the `edit` command and specifying a word corresponding to a procedure name.

Here's an example procedure calculating the maximum of two values:

	to maximum :a :b
		if less :a :b [ output :b ]
		output :a
	end

Flow Control and I/O
--------------------

Logo provides a minimal set of flow control primitives:

- `if` takes a boolean expression and a list. If the expression is `'true`, the list will be executed.
- `unless` is a counterpart to `if` which executes the list if the expression is not `'true`.
- `repeat` takes a number and a list. The list is executed 0 or more times based on the number.
- `run` takes a list and executes it. If this list is bound to arguments (see `bind`) it will consume additional arguments when `run` is invoked.
- `stop` exits the currently executing procedure and returns no value.
- `output` is a counterpoint to `stop` which returns an expression.

Logo also provides primitives for reading from and printing to the command console:

- `print` or `pr` takes a word, number or list and prints it. Lists are printed in a "flat" form ignoring any brackets or `:'` prefixes.
- `printlist` or `pl` functions like `print` but preserves the structure of arguments.
- `readlist` prompts the user to enter text, which is parsed as a Logo list (an outer set of square brackets are implied.)

Math
----

- `negate` returns the negation of a number.
- `random` returns a random integer from 0 up to but not including a given number.
- `equal` or infix `=` returns `'true` if both arguments are the same. Works for numbers, words or lists (which are compared recursively).
- `less` or infix `<` returns `'true` if the first number is less than the second.
- `greater` or infix `>` returns `'true` if the first number is greater than the second.
- `sum` or infix `+` returns the sum of two numbers.
- `difference` or infix `-` returns the difference of two numbers.
- `product` or infix `*` returns the product of two numbers.
- `quotient` or infix `/` returns the quotient of dividing two numbers.
- `remainder` or infix `%` returns the remainder of dividing two numbers.

List Operations
---------------

- `first` returns the first element of a list
- `last` returns the last element of a list
- `butfirst` or `bf` returns everything after the first element of a list
- `butlast` or `bl` returns everything before the last element of a list
- `size` returns the number of elements in a list.
- `item` takes an 0-based index followed by a list and extracts an element from that list, or returns an empty list if the index is out of range.
- `list` takes two values and returns a list containing them.
- `member` takes a value and a list, and returns a subsequence of the list starting with any instance of the given value, or an empty list if the original list did not contain the value.
- `flatten` takes a list which may contain sublists and flattens all the elements recursively into a single list.

All list operations are functional- that is, they return a new list rather than altering an existing list in-place:

- `fput` takes a value and a list and adds the value to the beginning of the list.
- `lput` takes a value and a list and adds the value to the end of the list.

Misc
----

- `make` takes a name and a value, and gives the specified name the specified value in a global scope.
- `local` takes a name and a value and gives the specified name the specified value in a local scope- that is, within the current function declaration.
- `bind` takes two lists. The first should be a list of names, while the second is an executable list. `bind` attaches the first list as an arglist for the second. This is roughly equivalent to a Lisp `lambda`.
- `thing` takes a name, and yields the value associated with that name. This works like `:` on a variable name, but can be chained to follow arbitrary degrees of indirect reference.
- `words` prints out a list of all globally-defined names.
- `trace` prints out a stack trace of the currently executing program, including all locally bound arguments and values.
- `free` performs garbage collection and prints out how many words of heap space are still free.

Examples
--------

Novel Control Structures:

	to forever :proc
		run :proc
		forever :proc
	end
	
	to while :cond :proc
		unless run :cond [ stop ]
		run :proc
		while :cond :proc
	end

Dragon Curve:

	to x :c
		if (:c = 0) [ stop ]
		x  (:c - 1)
		right 90
		y  (:c - 1)
		forward 4
	end
	
	to y :c
		if (:c = 0) [ stop ]
		forward 4
		x  (:c - 1)
		left  90
		y  (:c - 1)
	end
	
	to dragon
		showturtle
		x 11
	end

Curses:

	to any :list
		output item random size :list :list
	end
	
	to noun
		output any [
			[an enraged camel]
			[an ancient philosopher]
			[a cocker spaniel]
			[the Eiffel Tower]
			[a cowardly moose]
			[the silent majority]
		]
	end
	
	to verb
		output any [
			[get inspiration from]
			[redecorate]
			[become an obsession of]
			[make a salt lick out of]
			[buy an interest in]
		]
	end
	
	to object
		output any [
			[mother in law]
			[psychoanalyst]
			[rumpus room]
			[fern]
			[garage]
			[love letters]
		]
	end
	
	to curse
		pr	fput 'May
			fput noun
			fput verb
			fput 'your
			list object '!
	end
