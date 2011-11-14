REM  A simple demo of using the grid and
REM  the sprite registers from within MASIC.

10 IMAGE   grid_tiles = "text.png", 8, 8
11 IMAGE sprite_tiles = "ratthing.png", 16, 16

REM  for each sprite: status, index, x, y
20 LET sprites(0) = 4353
21 LET sprites(1) = 0
22 LET sprites(2) = 100
23 LET sprites(3) = 50

30 LET index = 0
40 LET grid(index) = (grid(index) + index) % 64
50 LET index = (index + 1) % 1270
60 IF (index % 40) = 0 THEN SYNC
70 GOTO 40