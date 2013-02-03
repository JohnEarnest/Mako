Forth Warrior Manual
====================
(This game is currently a work in progress, as is this brief manual.)

Forth Warrior is a game of programming, stabbing and low cunning. Your Forth code controls the actions of a valiant adventurer as she plunges ever deeper into a mysterious dungeon. Gather precious gems, defeat slime creatures and watch your step!

To begin a game, use the `begin` command. It reads the name of a word, so `begin example` will start the game using the word `example` as an entrypoint. An individual level may be tested independently by using the `test` command, which works like begin but additionally takes a level number from the stack. Thus, `2 test example` will start on level 2 and use the word `example` as an entrypoint.
The `words` command provides a listing of all currently defined words. To find out more information about a particular word, use `help` followed by the name of a word. For example, `help +`. An asterisk after the stack effect indicates an immediate word.

If you create a file called `warrior.fs` in the same directory as the game, the `load` command can be used to execute all the code in this file- this makes it easy to work on your Warrior in your favorite text editor. If you aren't starting the game "cold" every time you make some changes, you may want to use `forget` to clear out old definitions from your dictionary before issuing `load` again.

While the game is running, any debugging output will be shown a line at a time at the bottom of the screen, with brief pauses between lines. Control+C will immediately halt the program and return to the Forth prompt.

The Forth Warrior Dialect
-------------------------
Forth Warrior provides a very small Forth kernel with only the essential primitives. Most of these words will seem familiar if you've programmed in Forth before. The following is a summary of major deviations from a common ANS-style Forth.

- Word names become available immediately after `:`. Thus, recursive procedures can simply refer to themselves. Mutual recursion is best accomplished by using vectored words and `'`.

- `'` is state-smart. If used in interpreting mode, it will push an xt onto the stack. Otherwise it will compile a literal into the current definition.

- `#` is a single-line comment, rather than `\`.

- String constants have special-cased syntax for convenience. When compiling, simply enclose a string in double quotes and the address to the head of the resulting null-terminated string will be compiled as a literal. All string-oriented routines use C-style null-terminated strings. Thus, a hello world program can simply be `: hello "Hello, World!" typeln ;`

- Loop constructs are a little different from usual. This dialect provides a word `loop` to begin a loop, and this can be matched with `again`, `until` or `while` to loop unconditionally, until a flag is true or while a flag is true, respectively. `break` exits the innermost loop.

To demonstrate, here's a simple counted loop written three different ways:

		: A    0 loop dup . 1 +  dup 10 > until ;
		: B    0 loop dup . 1 +  dup 11 < while ;
		: C    0 loop dup . 1 +  dup 10 > if break then again ;

Each will print out:

		0 1 2 3 4 5 6 7 8 9 10

If you think I've left something really important out, let me know and I'll consider expanding the built-in vocabulary.

Direction Constants
-------------------
- `N` (0)
- `E` (1)
- `S` (2)
- `W` (3)

Type Constants
--------------
- `FLOOR`  (0) Passable ground terrain.
- `WALL`   (1) Impassible.
- `STAIRS` (2) The exit to a given level.
- `DOOR`   (3) Locked, impassible barrier. Use a key to `open` it.
- `KEY`    (4) Key for opening doors. `take` them to collect them.
- `GEM`    (5) Worth points and heals damage. `take` them to collect them.
- `SLIME`  (6) Deadly enemy. `attack` them to kill them and don't step on them.
- `SPIKES` (7) These hurt to walk on. Avoid it when possible.

Queries
-------
(These actions do not advance time in the game world)
- `health` ( -- n )     The number of hearts the player has.
- `level`  ( -- n )     The current floor of the dungeon.
- `gems`   ( -- n )     How many gems the player has collected.
- `keys`   ( -- n )     How many keys the player is carrying.
- `listen` ( -- n )     How many enemies are still on this level.
- `look`   ( dir -- type ) View the type of thing/tile in an adjacent space.

Commands
--------
- `wait`   ( -- )     Do nothing for a game tick.
- `walk`   ( dir -- ) Move to an adjacent tile.
- `attack` ( dir -- ) Attack any enemies in an adjacent tile.
- `take`   ( dir -- ) Pick up an item in an adjacent tile.
- `open`   ( dir -- ) Unlock an adjacent door.

Misc
----
- `fast`  ( -- ) Display game animations more quickly.
- `slow`  ( -- ) Display game animations at normal speed. (default)
- `help`  ( -- ) Given a word name, print a brief explanation of what it does.
- `load`  ( -- ) Reload the source file.