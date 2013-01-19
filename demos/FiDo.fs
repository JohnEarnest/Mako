######################################################
##
##  FiDo:
##
##  A tiny compiler based on the simple programming
##  language described by Edsger Dijkstra in the book
##  "A Discipline of Programming".
##
##  John Earnest
##
######################################################
:include <Print.fs>
:include <String.fs>
:include <Parse.fs>
:include <Assembly.fs>

:array code-heap 4096 0
:data  head code-heap

: here       head @           ; ( -- addr )
: ,          here ! head inc  ; ( val -- ) 
: const,     CONST , ,        ; ( val -- )
: call,      CALL  , ,        ; ( addr -- )
: backwards  JUMP  , ,        ; ( addr -- )
: branch     JUMP  , here 0 , ; ( -- addr )
: branch0    JUMPZ , here 0 , ; ( -- addr )
: forwards   here swap !      ; ( addr -- )
: store,     const, STOR ,    ; ( addr -- )
: load,      const, LOAD ,    ; ( addr -- )

######################################################
##
##  Expression Parser:
##
######################################################

:array vars 26 0
: variable  getc trim 97 - vars + ; ( -- addr )

:proto expression
: primary ( -- )
	"(" match? if expression ")" expect exit then
	numeral?   if number> const,        exit then
	variable load,
;

: unary ( -- type )
	"-"     match? if unary -1 const, MUL , exit then
	"not"   match? if unary           NOT , exit then
	"true"  match? if -1 const,             exit then
	"false" match? if  0 const,             exit then
	primary
;

: multiplicative ( -- )
	unary
	"*"   match? if multiplicative MUL , exit then
	"/"   match? if multiplicative DIV , exit then
	"%"   match? if multiplicative MOD , exit then
	"and" match? if multiplicative AND , exit then
;

: additive ( -- )
	multiplicative
	"+"   match? if additive ADD , exit then
	"-"   match? if additive SUB , exit then
	"or"  match? if additive OR  , exit then
;

: expression ( -- )
	additive
	"<>"  match? if expression ' != call,  exit then
	"<="  match? if expression SGT , NOT , exit then
	"=>"  match? if expression SLT , NOT , exit then
	"<"   match? if expression SLT ,       exit then
	">"   match? if expression SGT ,       exit then
	"="   match? if expression '  = call,  exit then
;

######################################################
##
##  Compiler:
##
######################################################
:proto parse

: parse-if ( -- )
	0 loop
		expression ":" expect
		branch0 parse branch swap forwards
		"|" match?
	while "fi" expect
	{ "failed to match any guard." typeln halt } call,
	loop forwards dup while drop
;

: parse-do ( -- )
	here loop
		expression ":" expect
		branch0 parse over backwards forwards
		"|" match?
	while "od" expect drop
;

: parse-assign ( -- )
	0 loop variable "," match? while ":=" expect
	loop expression "," match? while
	loop store, dup while drop
;

: statement ( -- )
	"if"    match? if parse-if             exit then
	"do"    match? if parse-do             exit then
	"print" match? if expression ' . call, exit then
	parse-assign
;

: parse   loop statement ";" match? while         ; ( -- )
: compile >read parse RETURN , "compiled." typeln ; ( str -- )
: run     code-heap exec cr    "finished." typeln ; ( -- )

######################################################
##
##  Entrypoint:
##
######################################################

: main ( -- )
	# counted loop
	#"n := 5; do n > 0 : print n; n := n - 1 od"

	# conditional branch
	#"n := 5; if n > 3 : print 6 | n < 3 : print 9 fi"

	# euclid's algorithm
	"a, b := 54, 24; do b > 0 : a, b := b, a % b od; print a"

	compile run
;