######################################################
##
##  Sprites:
##
##  A lexicon for manipulating Mako sprites.
##
##  John Earnest
##
######################################################

: sprite@    4 * SP @ + ;
: .sprite-t  1 + ;
: .sprite-x  2 + ;
: .sprite-y  3 + ;
: .sprite-w  @ 0x0F00 and  256 / 1 + 8 * ;
: .sprite-h  @ 0xF000 and 4096 / 1 + 8 * ;
: invisible  1 xor ;

: px         sprite@ .sprite-x @ ;
: py         sprite@ .sprite-y @ ;
: tile       sprite@ .sprite-t @ ;
: px!        sprite@ .sprite-x ! ;
: py!        sprite@ .sprite-y ! ;
: tile!      sprite@ .sprite-t ! ;

: +px   sprite@ .sprite-x +@ ;
: +py   sprite@ .sprite-y +@ ;

: >sprite  >r i py! i px! i tile! r> sprite@ ! ; (status tile x y sprite-id -- )
: sprite>  >r i sprite@ @ i tile i px r> py    ; (sprite-id -- status tile x y )

: show       sprite@ dup @ 1 or      swap ! ;
: hide       sprite@ dup @ 1 not and swap ! ;

# Assume that sprites are drawn
# facing left normally: 

: face-left   sprite@ dup @ sprite-mirror-horiz not and swap ! ;
: face-right  sprite@ dup @ sprite-mirror-horiz or      swap ! ;
: flip-horiz  sprite@ dup @ sprite-mirror-horiz xor     swap ! ;
: flip-vert   sprite@ dup @ sprite-mirror-vert  xor     swap ! ;

:  rot   >r swap r> swap ; (a b c -- b c a)
: -rot   swap >r swap r> ; (a b c -- c a b)

# return true if a point lies within
# a given sprite, respecting its current
# location and size.
: c-sprite? ( x y sprite-id -- flag )

	rot >r sprite@ >r
	i .sprite-y @
	2dup i .sprite-h +
	<= -rot >= and
	
	r> r> swap >r
	i .sprite-x @
	2dup r> .sprite-w +
	<= -rot >= and

	and
;

# returns true if two sprites overlap,
# respecting their locations and sizes.
: c-sprites? ( sprite-a sprite-b -- flag )
	>r >r
	i py i sprite@ .sprite-h + j py >
	i py j sprite@ .sprite-h - j py < and
	i px i sprite@ .sprite-w + j px > and
	i px j sprite@ .sprite-w - j px < and
	rdrop rdrop
;