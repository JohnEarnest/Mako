TileEngine:

A quick reference guide to the utility routines comprising the TileEngine example.

constants:

actor-limit    # many actors are maintained?
text-offset    # offset to add to an ASCII char to get the corresponding tile index
text-start-x   # x-offset of message dialogs, in tiles
text-start-y   # y-offset of message dialogs, in tiles

variables:

sprite-id      # lookup table- actor-id to sprite index 
sprite-solid   # lookup table- is a given actor 'solid'?
sprite-trigger # lookup table- callback address or 0 for no trigger

words:

player         (     -- sid )     # push the sprite index of the player
c-tile?        ( x y -- flag )    # is the tile at pixel x,y impassible?
c-ground?      ( sid -- flag )    # is a sprite colliding with impassible terrain?
solid?         ( sid -- flag )    # is a sprite 'solid'?
solid!         ( flag sid -- )    # set a sprite's solid status
c-npc?         ( a b -- flag )    # are two sprites colliding?
c-npcs?        ( sid -- flag )    # is a sprite colliding with any other actors?
c-px!          ( x sid -- )       # set a sprite's x position if doing so doesn't collide.
c-py!          ( y sid -- )       # set a sprite's y position if doing so doesn't collide.
trigger?       ( sid -- addr )    # get the trigger address associated with a sprite
trigger!       ( addr sid -- )    # set the trigger address associated with a sprite
use-object     ( sid -- flag )    # is the player positioned to activate an npc?
indexof        ( n array -- addr) # get the address of a value, starting in an array
wait           ( -- )             # wait for key-a to be pressed and then released
show-text      ( msg -- )         # display dialog. pointer to to line count, then strings
init-blinds    ( -- )             # set up sprites for venetian blinds animation
animate-blinds ( -- )             # scroll blinds to conceal screen. call again to clear.
load-map       ( 'a 'm 'i -- 'a ) # load map with main 'm and init 'i.
face-player    ( sid -- )         # make a sprite turn to face the player.
player-center  ( -- x y )         # get coords for the center of the player.
animate-clean  ( -- )             # draw Mr. Scrubby's cleaning animation
use-return     ( -- )             # call in a trigger routine to return to prev. map.
clear-actors   ( -- )             # blank out all actors

>actor         (status tile x y solid? sprite-id -- ) # configure actor
actor>         (sprite-id -- status tile x y solid? ) # read actor data

move-player  ( -- ) # main loop logic block for moving the player via keys
use-prompt   ( -- ) # main loop logic block for showing the 'use action' icon
sort-sprites ( -- ) # main loop logic block for y-sorting sprites
use-logic    ( -- ) # main loop logic block for the context-sensitive use key
