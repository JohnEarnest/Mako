:var num
:var guess
:include "BasicLib.fs"
: main 
: line10 "I'm thinking of a number between 1 and 10..." prints cr 
: line20 10 rnd 1 + num ! 
: line30 "What's your guess?" prints cr 
: line40 input guess ! 
: line50 guess @ num @ < if :proto line90 ' line90 goto then 
: line60 guess @ num @ > if :proto line110 ' line110 goto then 
: line70 "Correct! You Win!" prints cr 
: line80 halt
: line90 "Nope, too low." prints cr 
: line100 ' line30 goto 
: line110 "Nah, too high." prints cr 
: line120 ' line30 goto 
halt
