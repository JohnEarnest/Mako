######################################################
##
##  Turtle:
##
##  A library which builds on 'Bitmap.fs' to supply
##  turtle-graphics line drawing functionality.
##  Designed with 'Loko.fs' in mind, but suitable
##  for general use.
##
##  John Earnest
##
######################################################

:const clearcolor 0xFF002200
:array screenbuffer 81920 clearcolor (320x256 pixel buffer)

:image turtle-sprites "turtles.png" 32 32
:image cursor          "cursor.png"  8  8

: pixel ( x y -- address )
	2dup 64 / 5 * swap 64 / + 4096 *
	swap 64 mod 64 * +
	swap 64 mod +
	ST @ +
;

: init-fullscreen ( -- )
	screenbuffer ST !
	19 for
		64x64 i
		i 5 mod 64 *
		i 5 /   64 *
		i >sprite
	next
;

# // a Processing snippet for generating our table:
# for(int a = 0; a < 360; a++) {
#   if (a % 8 == 0) { println(); }
#   System.out.format("% 6d ", (int)(sin(radians(a))*4096));
# }
#
:data sintable
     0     71    142    214    285    356    428    499 
   570    640    711    781    851    921    990   1060 
  1129   1197   1265   1333   1400   1467   1534   1600 
  1665   1731   1795   1859   1922   1985   2048   2109 
  2170   2230   2290   2349   2407   2465   2521   2577 
  2632   2687   2740   2793   2845   2896   2946   2995 
  3043   3091   3137   3183   3227   3271   3313   3355 
  3395   3435   3473   3510   3547   3582   3616   3649 
  3681   3712   3741   3770   3797   3823   3848   3872 
  3895   3917   3937   3956   3974   3991   4006   4020 
  4033   4045   4056   4065   4073   4080   4086   4090 
  4093   4095   4096   4095   4093   4090   4086   4080 
  4073   4065   4056   4045   4033   4020   4006   3991 
  3974   3956   3937   3917   3895   3872   3848   3823 
  3797   3770   3741   3712   3681   3649   3616   3582 
  3547   3510   3473   3435   3395   3355   3313   3271 
  3227   3183   3137   3091   3043   2995   2946   2896 
  2845   2793   2740   2687   2632   2577   2521   2465 
  2407   2349   2290   2230   2170   2109   2048   1985 
  1922   1859   1795   1731   1665   1600   1534   1467 
  1400   1333   1265   1197   1129   1060    990    921 
   851    781    711    640    570    499    428    356 
   285    214    142     71      0    -71   -142   -214 
  -285   -356   -428   -499   -570   -640   -711   -781 
  -851   -921   -990  -1060  -1129  -1197  -1265  -1333 
 -1400  -1467  -1534  -1600  -1665  -1731  -1795  -1859 
 -1922  -1985  -2047  -2109  -2170  -2230  -2290  -2349 
 -2407  -2465  -2521  -2577  -2632  -2687  -2740  -2793 
 -2845  -2896  -2946  -2995  -3043  -3091  -3137  -3183 
 -3227  -3271  -3313  -3355  -3395  -3435  -3473  -3510 
 -3547  -3582  -3616  -3649  -3681  -3712  -3741  -3770 
 -3797  -3823  -3848  -3872  -3895  -3917  -3937  -3956 
 -3974  -3991  -4006  -4020  -4033  -4045  -4056  -4065 
 -4073  -4080  -4086  -4090  -4093  -4095  -4096  -4095 
 -4093  -4090  -4086  -4080  -4073  -4065  -4056  -4045 
 -4033  -4020  -4006  -3991  -3974  -3956  -3937  -3917 
 -3895  -3872  -3848  -3823  -3797  -3770  -3741  -3712 
 -3681  -3649  -3616  -3582  -3547  -3510  -3473  -3435 
 -3395  -3355  -3313  -3271  -3227  -3183  -3137  -3091 
 -3043  -2995  -2946  -2896  -2845  -2793  -2740  -2687 
 -2632  -2577  -2521  -2465  -2407  -2349  -2290  -2230 
 -2170  -2109  -2048  -1985  -1922  -1859  -1795  -1731 
 -1665  -1600  -1534  -1467  -1400  -1333  -1265  -1197 
 -1129  -1060   -990   -921   -851   -781   -711   -640 
  -570   -499   -428   -356   -285   -214   -142    -71

: sin       360 mod sintable + @ ; ( angle -- sin )
: cos  90 + 360 mod sintable + @ ; ( angle -- cos )

######################################################
##
##  Turtle:
##
######################################################

:const turtle-sprite 128
:const turtle-base    80
:var   turtle-angle
:var   turtle-posx
:var   turtle-posy
:var   linecolor

: angle   turtle-angle @       ; ( -- degrees )
: posx    turtle-posx @ 4096 / ; ( -- pixels )
: posy    turtle-posy @ 4096 / ; ( -- pixels )

: angle-s ( -- status )
	angle 270 >= if 32x32                        sprite-mirror-vert or exit then
	angle 180 >= if 32x32 sprite-mirror-horiz or sprite-mirror-vert or exit then
	angle  90 >= if 32x32 sprite-mirror-horiz or                       exit then
	32x32
;

: angle-t ( -- tile )
	angle 90 mod 3 /
	angle 270 >= if 31 swap - exit then
	angle 180 >= if           exit then
	angle  90 >= if 31 swap - exit then
;

: update-turtle ( -- )
	angle-s angle-t turtle-base +
	posx 15 - posy 15 - turtle-sprite >sprite
;

: angle!  360 mod turtle-angle ! update-turtle ; ( degrees -- )
: angle+  angle + angle!                       ; ( degrees -- )
: posx!   4096 * turtle-posx ! update-turtle   ; ( pixels -- )
: posy!   4096 * turtle-posy ! update-turtle   ; ( pixels -- )
: posx+   turtle-posx @ + turtle-posx !        ; ( xd -- )
: posy+   turtle-posy @ + turtle-posy !        ; ( yd -- )

######################################################
##
##  Line Drawing:
##
##  Render lines on the bitmapped drawing surface
##  using Bresenham's algorithm.
##
######################################################

: plot ( x y -- )
	over dup 0 < swap 319 > or if 2drop exit then
	dup  dup 0 < swap 231 > or if 2drop exit then
	pixel linecolor @ swap !
;

: abs  dup 0 < if -1 * then ; ( a -- b )

:var dx :var dy
:var sx :var sy
:var x0 :var y0
:var x1 :var y1
:var err

: lineto ( x y -- )
	posy y0 ! y1 !
	posx x0 ! x1 !
	x1 @ x0 @ - abs dx !
	y1 @ y0 @ - abs dy !
	dx @ dy @ - err !
	x0 @ x1 @ < if 1 else -1 then sx !
	y0 @ y1 @ < if 1 else -1 then sy !
	loop
		x0 @ y0 @ plot
		x0 @ x1 @ =
		y0 @ y1 @ = and if break then
		err @ 2 *
		dup dy @ -1 * > if
			err @ dy @ - err !
			x0  @ sx @ + x0  !
		then
		dx @ < if
			err @ dx @ + err !
			y0  @ sy @ + y0  !
		then
	again
;

######################################################
##
##  High-level operations:
##
######################################################

:data pen true

: draw ( dist -- )
	dup  angle cos * turtle-posx @ +
	swap angle sin * turtle-posy @ +
	pen @ if over 4096 / over 4096 / lineto then
	turtle-posy ! turtle-posx ! update-turtle
;

: home ( -- )
	-90 angle!
	160 posx!
	120 posy!
;

: clearscreen ( -- )
	 81919 for
		clearcolor i screenbuffer + !
	next
	7 for
		319 for
			0x00000000 i j 232 + pixel !
		next
	next
;

: showturtle ( -- )
	init-fullscreen
	0xFF009900 linecolor !
	home
;

: hideturtle ( -- )
	19 for i hide next
	turtle-sprite hide
;

######################################################
##
##  Testing:
##
######################################################

:proto G

: F ( r -- r )
	dup -if 7 draw sync exit then
	1 -
	F -120 angle+
	G  120 angle+
	F  120 angle+
	G -120 angle+
	F
	1 +
;

: G ( r -- r )
	dup -if 7 draw sync exit then
	1 -
	G
	G
	1 +
;

: lsystem ( -- )
	loop
		4
		F -120 angle+
		G -120 angle+
		G  -60 angle+
		drop
	again
;