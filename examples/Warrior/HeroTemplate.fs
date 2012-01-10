######################################################
##
##  HeroTemplate
##
##  A starting point for creating your own
##  Forth Warrior.
##
##  John Earnest
##
######################################################

:const start-level 0
:include "Warrior.fs"

# 'level' is executed once at the beginning
# of each level, and is given the level number.
# You can use this to tailor your strategy for
# specific levels or reset caches.

: level ( n -- )
	
;

# 'tick' is executed once every game tick.
# Observe the environment via 'look' and 'listen',
# and choose an action via 'walk', 'take' and 'attack'.
# If multiple actions are indicated, the last
# choice will be applied.

: tick ( -- )
	
;
