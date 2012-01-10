Forth Warrior
=============

Forth Warrior is a game of programming, stabbing and low cunning. Your Forth code controls the actions of a valiant knight as he plunges ever deeper into a mysterious dungeon.

To play, create a copy of the included `HeroTemplate.fs` and fill in definitions for the `tick` and `level` words. `level` is executed once at the beginning of each level, and is given the level number. You can use this to tailor your strategy for specific levels or reset caches. `tick` is executed once every game tick, and is where you'll make decisions about what the hero should do next.

Give your program a spin by invoking Maker from the commandline-

	./maker examples/Warrior/HeroTemplate.fs --run

You can then sit back and watch the adventure unfold in a step-by-step animation. Icons are used to indicate abstract actions, such as looking in a direction.

Observing the Environment
-------------------------
The `look` word takes a direction and returns the object type directy adjacent to the player in that direction. (It's dark in dungeons, man.) Constants are available for the possible directions:

- `north` (0)
- `east` (1)
- `south` (2)
- `west` (3)

Constants are also provided for the types of objects and terrain you might see.

- `empty` an empty space you can walk on
- `solid` an impassible wall
- `stairs` the exit to this level of the dungeon
- `gem` a gem you can pick up for points
- `slime` a dangerous cave slime. Don't walk on 'em!

You can also use your acute sense of hearing via `listen` to obtain the number of enemies on the current level.

Taking Action
-------------
Every game tick, the player should call either `walk`, `take` or `attack`. `walk` moves the player one tile in the given direction. `take` is used for picking up gems on a given adjacent tile. `attack` is used for swinging the sword in a given direction. If multiple actions are indicated during one call to 'tick', the most recent action will take place. If no action is specified or an impossible task is attempted (such as taking a nonexistent gem), Forth Warrior will assume your code has a bug and stop immediately.

Scoring
-------
As you go from level to level, you will accumulate points for collecting gems, and Forth Warrior will also track the number of observations and actions. Try to see if you can write a hero that can make it through all the levels, and then try to maximize your score while minimizing the number of actions taken. If you just want to cut to the chase and see if your Hero can beat a specific level, change the definition of the `start-level` constant.