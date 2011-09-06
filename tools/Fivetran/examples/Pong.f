C
C	Pong
C
C	An implementation of the classic game Pong
C	demonstrating how the Mako graphics hardware
C	and peripherals are accessed when using FIVETRAN.
C

C	initialize the game
0
	do 1 i = 0, 7
C	configure left paddle
	ms(i, 0) = 1
	ms(i, 1) = 9
	ms(i, 2) = 0
	ms(i, 3) = 88 + (i x 8)
C	configure right paddle
	ms(i+8, 0) = 1
	ms(i+8, 1) = 8
	ms(i+8, 2) = 312
	ms(i+8, 3) = 88 + (i x 8)
1
C	configure ball
	ix = ((mr(11) % 2) x 2 - 1) x ((mr(11) % 2) + 1)
	iy = ((mr(11) % 2) x 2 - 1) x ((mr(11) % 2) + 1)
	ms(20, 0) = 1
	ms(20, 1) = 16
	ms(20, 2) = 160
	ms(20, 3) = 120

C	The main loop:
10

C	if up is pressed,
C	move all the left paddle sprites up.
	if (mr(12) = 1) 20, 21
20	do 201 i = 0, 7
	ms(i, 3) = ms(i, 3) - 4
201
21

C	if down is pressed,
C	move all the left paddle sprites down.
	if (mr(12) = 4) 30, 31
30	do	301 i = 0, 7
	ms(i, 3) = ms(i, 3) + 4
301
31

C	move right paddle
	if (160 > ms(20, 2)) 49, 40
40
	if (ms(20, 3) > ms(15, 3)) 401, 403
401	do 402 i = 8, 15
402	ms(i, 3) = ms(i, 3) + 2
403

	if (ms(8, 3) > ms(20, 3)) 405, 407
405	do 406 i = 8, 15
406	ms(i, 3) = ms(i, 3) - 2
407
49

C	move the ball
	ms(20, 2) = ms(20, 2) + ix
	ms(20, 3) = ms(20, 3) + iy
	if (ms(20, 2) > 320) 0, 61
61	if (0 > ms(20, 2))   0, 62
62
	if (ms(20, 3) > 232) 64, 65
64	ms(20, 3) = 232
	iy = -iy
65	if (0 > ms(20, 3)) 66, 67
66	ms(20, 3) = 0
	iy = -iy
67

C	collide ball with left paddle
	if (312 > ms(20, 2))         79, 70
70	if (ms(8, 3)  > ms(20, 3)+8) 79, 71
71	if (ms(20, 3) > ms(15, 3)+8) 79, 72
72	ms(20, 2) = 312
	ix = -ix
79

C	collide ball with right paddle
	if (ms(20, 2) > 8)           89, 80
80	if (ms(0, 3)  > ms(20, 3)+8) 89, 81
81	if (ms(20, 3) > ms(7, 3)+8)  89, 82
82	ms(20, 2) = 8
	ix = -ix
89
	
	sync
	go to 10