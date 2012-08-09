:include <Test/Tap.fs>
:include <String.fs>
:include <Parse.fs>

:var A
:var B
:var C

: main ( -- )
	33 plan
	666

	# text primitives:
	  9  white?  true = "white positive"    ok
	 65  white? false = "white negative"    ok
	 65 letter?  true = "letter positive 1" ok
	100 letter?  true = "letter positive 2" ok
	 13 letter? false = "letter negative"   ok
	 50  digit?  true = "digit positive"    ok
	 70  digit? false = "digit negative"    ok

	 50 d>num       2    = "digit conversion"  ok
	"A" @ to-lower "a" @ = "lower" ok
	"b" @ to-upper "B" @ = "upper" ok

	# buffered input:
	"ABCD" >read
		curr 65 = "queue head 1" ok
		curr 65 = "queue head 2" ok
		getc 65 = "getc"         ok
		curr 66 = "queue read"   ok
		skip
		curr 67 = "skip"         ok
		name? true = "name?" ok
	clear-q
	"123456" >read
		0 xq d>num 1 = "queue index 1" ok
		3 xq d>num 4 = "queue index 2" ok
		1 xq d>num 2 = "queue index 3" ok
	clear-q

	# high-level parsing words:
	"var  boolean	int" >read
	"var"     starts?  true = "starts positive"  ok
	"quux"    starts? false = "starts negative"  ok
	"var"     match?   true = "match positive 1" ok
	"boolean" match?   true = "match positive 2" ok
	"failure" match?  false = "match negative"   ok
	"int"     match?   true = "match positive 3" ok	

	"123 0 43 -721" >read
	number>  123 = "number> 1" ok
	number>    0 = "number> 2" ok
	number>   43 = "number> 3" ok
	signed> -721 = "signed>"   ok

	"quux Zort ka3poot" >read
	token> "quux"    -text 0 = "token 1" ok
	token> "Zort"    -text 0 = "token 2" ok
	token> "ka3poot" -text 0 = "token 3" ok

	# check canary
	666 = "canary" ok
;