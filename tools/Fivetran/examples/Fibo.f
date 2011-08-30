C
C	Calculate the first 20 terms of the
C   Fibonacci sequence and print them to stdout.
C
	ia = 1
	ib = 1
	print ia
	print ib

	do 10 k = 1, 18, 1
	ic = ia + ib
	ia = ib
	ib = ic
10	print ic

	stop