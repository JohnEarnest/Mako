C
C	Expression Tests:
C	A number of test cases for diagnosing
C	problems with order of operations and
C	constant folding.
C
C	For convinience, these always print out the
C	computed value followed by the expected value.
C

2	print -2+4,		 2
3	print 1+2x3,	 7
4	print (1+2)x3,	 9
5	print -(1+2)x3, -9