Forth Warrior Manual
====================
Forth Warrior is currently a work in progress, as is this brief manual. Eventually the game will incorporate some built-in help facilities.

To begin a game, use the `begin` command. It reads the name of a word, so `begin example` will start the game using the word `example` as an entrypoint. An individual level may be tested independently by using the `test` command, which works like begin but additionally takes a level number from the stack. Thus, `2 test example` will start on level 2 and use the word `example` as an entrypoint.

Please consult the readme for MakoForth itself for an exhaustive list of provided general-purpose words.

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

Queries
-------
(These actions do not advance time in the game world)
- `health` ( -- n )     The number of hearts the player has.
- `level`  ( -- n )     The current floor of the dungeon.
- `gems`   ( -- n )     How many gems the player has collected.
- `keys`   ( -- n )     How many keys the player is carrying.
- `listen` ( -- n )     How many enemies are still on this level.
- `look`   ( dir -- t ) View the type of thing/tile in an adjacent space.

Commands
--------
- `wait`   ( -- )     Do nothing for a game tick.
- `walk`   ( dir -- ) Move to an adjacent tile.
- `attack` ( dir -- ) Attack any enemies in an adjacent tile.
- `take`   ( dir -- ) Pick up an item in an adjacent tile.
- `open`   ( dir -- ) Unlock an adjacent door.

Misc
----
- `var`   ( -- )       Construct a named variable, like the common Forth word `variable`.
- `const` ( n -- )     Construct a named constant, like the common Forth word `constant`.
- `=`     ( a b -- f ) Return -1 if a and b are equal. Otherwise, return 0.
- `exec`  ( xt -- )    Given an execution token (as obtained with `'`), call it.
- `fast`  ( -- )       Display game animations more quickly.
- `slow`  ( -- )       Display game animations at normal speed. (default)