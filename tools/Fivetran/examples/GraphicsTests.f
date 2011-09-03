
C	mg(0,  0) = 31
C	mg(29, 0) = 32
C	mg(0, 39) = 33
C	mg(29,39) = 34
C1	sync
C	go to 1

5	do 25 k = 0, 29, 1
	do 20 i = 0, 29, 1
	do 10 j = 0, 39, 1
	mg(i, j) = (1 + mg(i, j)) % 96 
10
20
25  sync
30	go to 5

30	print mc
40	sync
	go to 40