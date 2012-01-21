
: square    2 / / 2 mod 128 * ;
: sawtooth  swap over mod swap 127 swap / * ;
: abs       dup 0 < if -1 * then ;
: triangle  swap over mod swap - 1 swap - 127 * ;

:var note
:var voice
: sample voice @ exec ;

: rst drop 0 ;
: C-1 245 4 / sample ; # 32.70
: C$1 231 4 / sample ; # 34.65
: D-1 218 4 / sample ; # 36.71
: D$1 206 4 / sample ; # 38.89
: E-1 194 4 / sample ;
: F-1 183 4 / sample ;
: F$1 173 4 / sample ; # 46.25
: G-1 163 4 / sample ;
: G$1 154 4 / sample ; # 51.91
: A-1 145 4 / sample ;
: A$1 137 4 / sample ; # 58.27
: B-1 130 4 / sample ; # 61.74
: C-2 122 4 / sample ; # 65.41

: C$2 231 2 / sample ;
: D-2 218 2 / sample ;
: D$2 206 2 / sample ;
: E-2 194 2 / sample ;
: F-2 183 2 / sample ;
: F$2 173 2 / sample ;
: G-2 163 2 / sample ;
: G$2 154 2 / sample ;
: A-2 145 2 / sample ;
: A$2 137 2 / sample ;
: B-2 130 2 / sample ;
: C-3 122 2 / sample ;

:data moon
	F$2 C$2 F$2 G$2 C$2 F$2 G$2 C$2
	B-2 C$2 B-2 A$2 C$2 A$2 G$2 F$2
:data moon2
	G$2 F$2 F$1 C$2 F$2 G$2 C$2 F$2
	G$2 C$2 B-2 C$2 B-2 A$2 C$2 A$2

:data lead
	D$1 rst G$1 rst D$1 rst G$1 rst
	D-1 rst G$1 rst D-1 rst G$1 rst
	C$1 rst G$1 rst C$1 rst G$1 rst
	C-1 rst G$1 rst C-1 rst C$1 D-1
	D$1 rst G$1 rst D$1 rst G$1 rst
	D-1 rst G$1 rst D-1 rst G$1 rst
	C$1 rst G$1 rst C$1 rst G$1 rst
	B-1 rst A$1 rst G$1 rst rst rst

:data harmony
	G$1 G$1 rst G$1 G$1 rst rst rst
	G$1 G$1 rst G$1 G$1 rst rst rst
	G$1 G$1 rst G$1 G$1 rst F$1 G$1
	G$1 F$1 C-2 C-2 G$1 G$1 G$1 rst
	G$1 G$1 rst G$1 G$1 rst rst rst
	G$1 G$1 rst G$1 G$1 rst rst rst
	G$1 G$1 rst G$1 G$1 rst F$1 G$1
	G$1 F$1 C-2 C-2 G$1 G$1 G$1 rst

: main
	0 note !
	loop
		2000 for
			#' square   voice ! i note @ lead    + @ exec
			#' sawtooth voice ! i note @ harmony + @ exec
			#+
			' square   voice ! i note @ moon  + @ exec
			' sawtooth voice ! i note @ moon2 + @ exec
			or
			AU !
		next
		note @ 1 + 16 mod note !
	again
;
