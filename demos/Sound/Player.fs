######################################################
##
##  Player:
##
##  A more sophisticated tech demo of realtime audio
##  synthesis using the Audio port, including
##  amplitude envelopes and echo.
##
##  John Earnest
##
######################################################

:include <Print.fs>
:include <Util.fs>
:include <Sprites.fs>
:include <Bitmap.fs>
:include <Grid.fs>

: max   2dup < if swap then drop ;
: min   2dup > if swap then drop ;

######################################################
##
##  Core Synthesis
##
######################################################

# oscillators all take ( timer freq -- sample )
: square   4 / / 2 mod 128 * ;
: noise    2 / / 2 mod RN @ 96 mod 32 + * ;
: saw      4 / swap over mod swap 127 swap / * ;

:const C-3 245 :const C$3 231 :const D-3 218
:const D$3 206 :const E-3 194 :const F-3 183
:const F$3 173 :const G-3 163 :const G$3 154
:const A-3 145 :const A$3 137 :const B-3 130
:const C-2 122 :const C$2 115 :const D-2 109
:const D$2 103 :const E-2  97 :const F-2  91
:const F$2  86 :const G-2  81 :const G$2  77
:const A-2  72 :const A$2  68 :const B-2  65
:const ---   0

# A simple circular buffer can be used
# to provide an echo effect:
:const reverb-len 1000
:array reverb reverb-len 64
:var   revi
: rev@ revi @ reverb +                         ; (        -- addr   )
: >rev rev@ ! revi @ 1 + reverb-len mod revi ! ; ( sample --        )
: rev> rev@ @                                  ; (        -- sample )

:const tempo  1000 # samples/note
:array env0   tempo 0
:array env1   tempo 0
:array env2   tempo 0
:array env3   tempo 0

: init-sound ( -- )
	# I *could* pre-calculate my envelopes offline,
	# but this approach is easier and more flexible.
	128 for
		i env1 i + !
		i env2 i + !
	next
	128 for
		4 for
			128 j - tempo j 4 * - env1 + i + !
			    j   tempo j 4 * - env2 + i + !
		next
	next
	
	128 for
		128 RN @ i 1 + mod -
		8 for
			dup j 8 * i + env3 + !
		next
		drop
	next
;

:var aindex
:var lindex
:var atimer

: voice ( 'envelope 'oscillator 'track -- sample )
	aindex @ + @
	lindex @ + @
	dup if
		swap >r atimer @ swap r> exec
		swap atimer @ + @ - 0 max
	else
		2drop drop 64
	then
;

######################################################
##
##  Track/Mix description
##
######################################################

:const looplen    8
:const songlen   16

:data L1  D$2 --- G$2 --- D$2 --- G$2 ---
:data L2  D-2 --- G$2 --- D-2 --- G$2 ---
:data L3  C$2 --- G$2 --- C$2 --- G$2 ---
:data L4  C-2 --- G$2 --- C-2 --- C$2 D-2
:data L5  B-2 --- A$2 --- G$2 --- --- ---

:data H1  G$3 G$3 --- G$3 G$3 --- --- ---
:data H2  G$3 G$3 --- G$3 G$3 --- F$3 G$3
:data H3  G$3 F$3 C-2 C-2 G$3 G$3 G$3 ---

:data P1  G$2 --- D-2 --- G$2 --- D-2 ---
:data --  --- --- --- --- --- --- --- ---

:data track1   L1 L2 L3 L4 L1 L2 L3 L5 L1 L2 L3 L4 L1 L2 L3 L5
:data track2   H1 H1 H2 H3 H1 H1 H2 H3 H1 H1 H2 H3 H1 H1 H2 H3
:data track3   P1 P1 P1 P1 P1 P1 P1 P1 P1 P1 P1 P1 P1 P1 P1 P1

# state controlled by the UI:
:data flags
:data view   true
:data reverb false
:data chan1  true
:data chan2  true
:var lastsample
:data voices
:data voice1 2
:data voice2 0
:data voicetable square noise saw

: play ( -- mixed )

	chan1 @ chan2 @ or if
		chan1 @ if
			env1 voice1 @ voicetable + @ track1 voice
		else
			64
		then
		chan2 @ if
			env2 voice2 @ voicetable + @ track2 voice
		else
			64
		then
		+ 2 /
		reverb @ if rev> + 2 / dup >rev then
		dup lastsample !
		dup AU !
	else
		lastsample @
	then

	atimer inc@
	atimer @ tempo >= if
		0 atimer !
		lindex inc@
		lindex @ looplen >= if
			0 lindex !
			aindex @ 1 + songlen mod aindex !
		then
	then
;

######################################################
##
##  UI stuff
##
######################################################

:image grid-tiles "text.png" 8 8

:data vname "VIEW"
:data rev  "ECHO"
:data voc1 "CHAN1"
:data voc2 "CHAN2"
:data fnames vname rev voc1 voc2

:data o1 "SQUARE"
:data o2 "NOISE "
:data o3 "SAW   "
:data onames o1 o2 o3

:var select
:var pressed

: sel-voice select @ 4 - voices + ;

: main
	init-fullscreen
	init-sound

	loop
		200 for
			play
			view @ if
				128 for
					j 92 + # x
					i 56 + # y
					pixel
					over i < if 0xFFFFCC00 else 0xFF440000 then
					swap !
				next
			then
			drop
		next

		# flag ui
		3 for
			1 i 4 +
			select @ i = if
				flags i + @ if "[x]" else "[ ]" then
			else
				flags i + @ if " x " else "   " then
			then
			grid-type
			5 i 4 + i fnames + @ grid-type
		next

		# selector ui
		1 for
			select @ i 4 + = if
				1 i 9 + "<" grid-type
				8 i 9 + ">" grid-type
			else
				1 i 9 + " " grid-type
				8 i 9 + " " grid-type
			then
			2 i 9 + i voices + @ onames + @ grid-type
		next

		pressed @ if
			keys -if pressed off then
		else
			keys if pressed on then
			keys key-up and if select dec@             then
			keys key-dn and if select inc@             then
			select @ 6 mod select !

			select @ 4 < if
				keys key-a  and if flags select @ + toggle then
			else
				keys key-lf and if sel-voice dec@ then
				keys key-rt and if sel-voice inc@ then
				sel-voice @ 3 mod sel-voice !
			then
		then

		sync
	again
;