######################################################
##
##  Format:
##
##  An implementation of formatted printing routines
##  for Mako, building on the "Print.fs" library.
##  See below for the list of format sequences.
##
######################################################

:proto printf
: %% 37 emit ;

: .hex
	16 /mod "0123456789ABCDEF" + @
	swap dup if .hex else drop then emit
;

:data chars
	 37  %%     # %% - '%' character
	 99  emit   # %c - ascii character
	100  .num   # %d - decimal number
	120  .hex   # %x - hexadecimal number
	115  type   # %s - null-terminated string
	102  printf # %f - format string (these work recursively)
	110  cr     # %n - newline
	116  tab    # %t - tab

: dispatch ( char -- )
	chars loop
		2dup @ = if break then 2 +
	again
	swap drop 1 + @ exec
;

: printf ( ... string -- )
	loop
		dup @     -if drop break then
		dup @ 37 = if
			1 + dup >r @ dispatch r>
		else
			dup @ emit
		then
		1 +
	again
;

######################################################
##
##  Usage examples:
##
######################################################

(
: main
	1 2 3 "%d %d %d%n" printf
	"foo" "This dude said '%s'- what's with that?%n" printf
	1 2 3 "%d%t%d%t%d%n" printf
	45 "%n%t[%d]%n" 27 "%d {%f}%n" printf
	0xFF00 0x1AFEBABE "%x %x%n" printf
;
)