![Title Image](http://i.imgur.com/zL89d34.png)

Forth Warrior is a game of programming, stabbing and low cunning. Your Forth code controls the actions of a valiant adventurer as she plunges ever deeper into a mysterious dungeon. Gather precious gems, defeat slime creatures and watch your step! Your program must fit in 8 kilobytes and is scored based on how many cycles it takes to execute, how many actions are taken, how many gems are collected and how many slimes are defeated. Once you've made it through all the levels, try to optimize your code and hone your strategy to maximize your score!

This game is intended to give beginning Forth enthusiasts a fun, well-defined task to cut their teeth on. While there are many differences in gameplay, I consider it to be a spiritual successor to [Ruby Warrior](https://github.com/ryanb/ruby-warrior).

Installation
------------

To build Forth Warrior from source, you'll need the Java SDK and Apache Ant. Download the Mako repository, build the "Maker" compiler and use it to compile and run Forth Warrior itself:

		git clone https://github.com/JohnEarnest/Mako.git
		cd Mako
		ant
		./maker games/Warrior2/Warrior2.fs --run

Maker can also be used to output a ROM which can be baked into a standalone JAR, for easier distribution:

		./maker games/Warrior2/Warrior2.fs tools/Standalone/Data
		cd tools/Standalone
		ant
		java -jar Mako.jar

Getting Started
---------------

When Forth Warrior boots up, you're given a command prompt which allows you to interactively edit and test Forth code. If you've never programmed in Forth before, read a few chapters of Leo Brodie's canonical [Starting Forth](http://www.forth.com/starting-forth/). For a faster-paced and somewhat less whimsical guide, take a look at [A Beginner's Guide to Forth](http://galileo.phys.virginia.edu/classes/551.jvn.fall01/primer.htm). As noted below, the dialect of Forth used in this game varies a bit from that used in these references, but the bulk of the language is the same.

To begin a game, use the `begin` command. It reads the name of a word, so `begin example` will start the game using the word `example` as an entrypoint. An individual level may be tested independently by using the `test` command, which works like `begin` but additionally takes a level number from the stack. Thus, `2 test example` will start on level 2 and use the word `example` as an entrypoint.
The `words` command provides a listing of all currently defined words. To find out more information about a particular word, use `help` followed by the name of a word. For example, `help +`. An asterisk after the stack effect indicates an immediate word.

If you create a file called `warrior.fs` in the same directory as the game, the `load` command can be used to execute all the code in this file- this makes it easy to work on your Warrior in your favorite text editor. If you aren't starting the game "cold" every time you make some changes, you may want to use `forget` to clear out old definitions from your dictionary before issuing `load` again. You can also specify a filename with `load`, which is useful if you're tinkering with several AIs- for example `load dummy.fs`.

While the game is running, any debugging output will be shown a line at a time at the bottom of the screen, with brief pauses between lines. This means if you wish to print less than 40 characters at a time you should be using `typeln` or terminating your output with `cr`, the command for printing a carriage return. Control+C will immediately halt the program and return to the Forth prompt.

Game Elements
-------------
![Liz](http://i.imgur.com/gUbEKVo.png) Our heroine, Liz, is a courageous and daring dungeon-delver. While she can ably dispatch many a foe in battle, she's not the best original thinker. Fortunately, like any adventurer worth their salt, Liz is fluent in Forth. With your instructions as her guide, she will plumb the depths of a mysterious and deadly ruin! Liz can be hurt by spikes and slimes, so keep a close eye on her health meter.

![Stairs](http://i.imgur.com/RevoVLx.png) These stairs lead deeper into the dungeon. Completing a level is a simple matter of making it to the stairs. Easy, right?

![Gem](http://i.imgur.com/sfbFmA4.png) Gems are exceedingly valuable, both for the way they catch the light and the more utilitarian fact that they will heal one heart's worth of an adventurer's wounds when picked up. Get as many as you can!

![Slime](http://i.imgur.com/XkyvnAb.png) Slime beasts lurk in this dungeon. Beware their acidic pseudopods and for heaven's sake don't step in them. I wonder where they all come from?

![Floor](http://i.imgur.com/I9hHLWk.png) The roughly finished stone floor of a dungeon is littered with the bones of previous visitors and marred by the viscous ichors of slime beasts. Otherwise unremarkable.

![Spikes](http://i.imgur.com/jqrhYIf.png) The floor menaces with spikes of iron! Stepping onto a spiked panel will hurt Liz by heart, but she only takes damage upon entering the tile.

![Door](http://i.imgur.com/jiDSLYI.png) Locked doors impede progress in your adventure. They must be `open`ed with a key.

![Key](http://i.imgur.com/mlmNBrh.png) Problem, meet solution. Keys can be carried between rooms, and odds are you'll need all of them eventually.

Examples
--------

An extremely simple brain that can make it past a few obstacles:

		: dummy ( -- )
			E loop
				dup look
				dup STAIRS = swap FLOOR = or if
					dup walk
				else
					1 + 4 mod
				then
			again
		;

A more sophisticated brain which handles many types of obstacles and attempts to traverse mazes by following the left wall:

		create actions
			' walk   , # floor
				 0   , # wall
			' walk   , # stairs
			' open   , # door
			' take   , # key
			' take   , # gem
			' attack , # slime
			' walk   , # spikes
		
		: act ( dir -- dir )
			dup 4 mod dup look
			dup WALL = if drop drop 0 exit then
			actions + @ exec -1
		;
		
		: try ( dir -- dir' )
			1 - act if exit then
			1 + act if exit then
			1 + act if exit then
		;
		
		: lefthand ( -- )
			E loop try again
		;

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

Many common words that are not included in the kernel can be synthesized in terms of the primitives available. For example, here's how we might define `allot`:

		: allot  loop 0 , 1 - dup while drop ; ( length -- )
		# used like:
		create buffer 45 allot

Game API Reference
------------------

Direction Constants:

- `N` (0)
- `E` (1)
- `S` (2)
- `W` (3)

Type Constants:

- `FLOOR`  (0) Passable ground terrain.
- `WALL`   (1) Impassible.
- `STAIRS` (2) The exit to a given level.
- `DOOR`   (3) Locked, impassible barrier. Use a key to `open` it.
- `KEY`    (4) Key for opening doors. `take` them to collect them.
- `GEM`    (5) Worth points and heals damage. `take` them to collect them.
- `SLIME`  (6) Deadly enemy. `attack` them to kill them and don't step on them.
- `SPIKES` (7) These hurt to walk on. Avoid it when possible.

Queries:

- `health` ( -- n )     The number of hearts the player has.
- `level`  ( -- n )     The current floor of the dungeon.
- `gems`   ( -- n )     How many gems the player has collected.
- `keys`   ( -- n )     How many keys the player is carrying.
- `listen` ( -- n )     How many enemies are still on this level.
- `look`   ( dir -- type ) View the type of thing/tile in an adjacent space.
(These actions do not advance time in the game world)

Commands:

- `wait`   ( -- )     Do nothing for a game tick.
- `walk`   ( dir -- ) Move to an adjacent tile.
- `attack` ( dir -- ) Attack any enemies in an adjacent tile.
- `take`   ( dir -- ) Pick up an item in an adjacent tile.
- `open`   ( dir -- ) Unlock an adjacent door.

Misc:

- `fast`  ( -- ) Display game animations more quickly.
- `slow`  ( -- ) Display game animations at normal speed. (default)
- `help`  ( -- ) Given a word name, print a brief explanation of what it does.
- `load`  ( -- ) Reload the source file.