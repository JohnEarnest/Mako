 10 PRINT "I'm thinking of a number between 1 and 10..."
 20 LET num = RND(10)+1
 30 PRINT "What's your guess?"
 40 INPUT guess
 50 IF guess<num THEN GOTO 90
 60 IF guess>num THEN GOTO 110
 70 PRINT "Correct! You Win!"
 80 END
 90 PRINT "Nope, too low."
100 GOTO 30
110 PRINT "Nah, too high."
120 GOTO 30