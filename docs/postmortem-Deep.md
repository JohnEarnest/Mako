Deep: A Postmortem
==================

As I begin to make more sophisticated games for the MakoVM platform, I thought it would be a good idea to try to record some of my experiences, both from the perspective of programming in Forth and lessons learned in game design.

Background
----------
With Deep, I set out to create an arcade-style action game in the vein of Space Invaders. I was also deeply inspired by the minimalist but impactful "storytelling mechanics" of Missile Command, so I wanted to try to capture some of that spirit. In an arcade game, I think the main storytelling element that must be included is a setting, which provides framing for what the game will communicate to the player. The way the rules of the game cause elements to act can express ideas within this context.

And so the game begins with a man in a small boat, in the middle of a deep ocean. I spent a lot of time building a nice animated background that sets the scene before I built any significant gameplay logic, which was quite satisfying.

Entities
--------
Structurally, I tried some different ideas with this game. The core is what I call the 'Entity' system. In previous games I'd always allocated sprite registers statically. Sprite 0 for the player, sprite 1 for a door, sprites 10-30 for enemies, etc. This becomes fairly clumsy for complex scenes, since you have to keep track of which registers are free for various types of objects and you may not make the best use of the available space. The alternative is to allocate the registers dynamically via a malloc/free-like mechanism. Here's a simplified version of the relevant words:

	:array types    257 0
	:array sprites 1024 0
	:array bogus      4 0
	
	: valid ( id -- flag )
		types + @ if true else false then
	;
	
	: alloc ( -- n )
		0 256 types + !
		0 loop
			dup valid -if exit then
			1 +
		again
	;
	
	: free ( id -- )
		dup types + 0 swap !
		sprite@     0 swap !
	;

The `types' array is used to keep track of the object type associated with the corresponding sprite register. A value of zero indicates a free register. Rather than coming up with a table of unique IDs and having a dispatch loop elsewhere to link objects up with their game logic I just store function pointers in the type table. They're consistent and unique, so they work as well as anything for an identifier.

In a normal malloc, I might need to worry about what happens when I run out of space. In games, failing to spawn an entity is often not a big deal- who cares if the screen is full of enemies and I can't create all five bubble sprites for my particle effect? Rather than forcing code throughout the game engine to worry about handling this error condition, I make the sprite table and the type table one entry larger than they need to be. If allocating a real sprite register fails, the index of this 'bogus' sprite register is returned. Game logic that cares about getting a valid register can check for the error condition and deal with it, while more forgiving scripts can harmlessly write to this slot.

Higher-Order Functions
----------------------
I also wanted to try to use as few loops as possible in this game. I've been working my way through _The Structure and Interpretation of Computer Programs_  (SICP) and I'm really seeing the value of abstracting patterns like iteration and dispatch. Forth doesn't have lexical scope or closures, but you can get pretty far on just function pointers.

	: whoever ( 'filter 'func -- )
		>r >r first-e
		loop
			dup i exec over valid and
			if dup j exec then
			1 + dup last-e <=
		while
		drop r> r> 2drop
	;

	: count+      drop swap 1 + swap            ;
	: count       0 swap ' count+ whoever       ; ( 'filter -- )
	: always      drop true                     ;
	: apply-type  dup types + @ exec            ;
	: think       ' always ' apply-type whoever ;

`whoever` is the single end-all be-all word that iterates over valid entities and does something to them. It incorporates a filter because I often want to work with a subset of the objects in the game. Think of `whoever` as a combination of filter() and map() from SICP.

Counting the number of entities which match a predicate and evaluating the game logic for all valid entities falls out very cleanly. Definitions like `always` suggest that working with higher-order functions like this can benefit from having an anonymous function syntax for throwaway predicates and helpers- definitely something worth considering for the future.

One thing I was a little unsatisified with about `count` is the fact that it's actually very tightly coupled with the way `whoever` manipulates the stack. `whoever` cannot be treated as a black box, since we want to keep a running total floating around between the stack arguments it uses. If I'd needed to keep track of more than one value for `count`'s purposes, the solution would need to become more invasive. Passing individual values around on the stack seems to lead to this sort of abstraction problem pretty frequently- I think object-oriented concatenative languages like PostScript and Factor have a big advantage in this area. If you can bundle an arbitrary number of values together as one 'thing', you can write generic functions which don't care about the datatypes they manipulate. In Forth I think the best alternative is trying a C-like technique of allocating structures on the stack and passing a pointer to them along to 'deeper' functions. Perhaps there's some kind of syntactic sugar I can build which will ease the process?

Entity Scripting
----------------
Normally entities are controlled by some sort of state machine- game objects transfer between states as they go about their animations, experience stimuli and get exploded. In the past I might've given entities a field which indicates their current state and written a branching dispatch structure like a case block to choose which behaviors should be carried out.

I already have a better mechanism for this, though. Why do a bunch of checks and branches every time my entities act when I can simply remap the function pointer in their 'type' field? In effect, I've turned the state machine inside-out by letting the states drive the conditions rather than the other way around. I also get the side benefit that entities can very easily transform into other types of entities. Check out how nicely this works:

	: menace
		32 wave-time
		sprite@ .sprite-t dup @ 1 xor swap !
	;

	: seek-player
		dup can-capsize if capsize then
		speed @ 2 / wave-time
		dup py 40 > if dup sprite@ .sprite-y dec@ then
		dup px player px < if 2 else -2 then
		over sprite@ .sprite-x +@
		menace
	;

	: swim
		dup py 64 < if ' seek-player type! exit then
		4 wave-time
		direction @ over sprite@ .sprite-x +@
			speed @ wave-time
		dup sprite@ .sprite-y dec@
		menace
	;

An entity that is swimming can turn into an entity that seeks the player with a simple `' seek-player type!`- the state transitions are extremely clear to an observer. By using this type of organization, the previously discussed higher-order functions and an opt-out condition checking style I was able to write virtually all entity logic without using loops or nesting if statements. Now that's clean!

You may have noticed the `wave-time` function there. Remember how I started the project by building an animated background with rolling waves? As it so happens, entities frequently have use for a global timer to control their animations and movement. The waves need one too, and it has a longer cycle than virtually any other timer in the game. Co-opting the system for entity logic was natural, so I built a helper word which could be used to exit an entity script early based on some cyclic period of the global timer. A+ for code reuse, but if I was doing it again from scratch (or my next game) I'd build a global timer from the start and give it a better name.

At the end of the day, I was very happy with my main loop:

	: main
		reset-game
		loop
			move-player
			fire
			waves
			storm
			bounce
			draw-score
			spawn-crab
	
			' is-monster count -if spawn-wave then
			ended @ gameover @ keys key-a and and and if reset-game then
	
			think
			sync
		again
	;

It comes pretty close to the goal of describing what needs to happen in a readable, high-level fashion which glosses over all the unimportant details. The main routines don't really depend on one another to function properly and could be re-ordered freely, indicating a good separation of concerns.

Emergent Game Design
--------------------
I started out with the idea of building a game that communicated some kind of narrative without using words, and I think the places I succeeded were almost entirely accidental.

When I writing the code to spawn waves of enemies, I quite straightforwardly said "when there are no enemies alive, spawn the next wave". When I later added crabs which spawn randomly and add a little chaos to the strategy of gameplay I didn't initially realize that as written a crab would always spawn before the first wave. This meant that the game would start with a lone crab attacking the player, and if he was destroyed a horde of his allies from the deep would immediately appear, as if to avenge his death. Genius! Not only does this slow start give the player a brief chance to get used to the basic game mechanics, it also raises a question- does the player bring inevitable destruction upon themselves? Who is really the protagonist here?

Perhaps not as deep and socially-aware as Missile Command's statement about nuclear proliferation and hard real-life decisionmaking, but I'll settle for anything thought provoking.

Game design lesson: pay attention to unintended consequences and bugs. As Bob Ross would say, we don't have mistakes here, we just have happy accidents.

Summary
-------
Dynamically allocating sprite registers via the entity system is a good abstraction. Reserving "bogus buffers" and failing silently can prevent error checking code from leaking out into main game logic. Factoring loops and conditionals into utility words and providing the 'loop bodies' via function pointers can lead to very terse, clean code. Inverting the usual structure of a state machine results in code which more closely resembles the transition diagram, lessening the maintenance problems traditionally associated with using state machines. Sometimes great design ideas can arise completely by accident- it's important to pay attention to bugs and try to use them to the game's favor.