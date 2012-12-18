######################################################
##
##  AudioKernel:
##
##  A modular system for playing music and blending
##  in sound effects. Instruments are represented
##  via a cyclic wavetable, with frequency determined
##  by how fast pointers advance through this table.
##  The song is controlled asynchronously from the
##  main program by maintaining a separate return
##  stack which is swapped with the primary return
##  stack by the word 'leave'.
##
##  John Earnest
##
######################################################

:array mstk 16 0 # music return stack
:var   mptr      # music return stack pointer

: leave       mptr @ RP @ mptr ! RP !      ; ( -- )
: init-music  mstk ! mstk 1 + mptr ! leave ; ( addr -- )

:const WAVE_MAX 1760
:const ---         0

:const C-0   37 :const C$0   39 :const D-0   41 :const D$0   44 :const E-0   46 :const F-0   49
:const F$0   52 :const G-0   55 :const G$0   58 :const A-0   62 :const A$0   66 :const B-0   70
:const C-1   74 :const C$1   78 :const D-1   83 :const D$1   88 :const E-1   93 :const F-1   99
:const F$1  105 :const G-1  111 :const G$1  117 :const A-1  125 :const A$1  132 :const B-1  140
:const C-2  148 :const C$2  157 :const D-2  166 :const D$2  176 :const E-2  187 :const F-2  198
:const F$2  210 :const G-2  222 :const G$2  235 :const A-2  250 :const A$2  264 :const B-2  280
:const C-3  297 :const C$3  314 :const D-3  333 :const D$3  353 :const E-3  374 :const F-3  396
:const F$3  420 :const G-3  445 :const G$3  471 :const A-3  500 :const A$3  529 :const B-3  561
:const C-4  594 :const C$4  629 :const D-4  667 :const D$4  707 :const E-4  749 :const F-4  793
:const F$4  840 :const G-4  890 :const G$4  943 :const A-4 1000 :const A$4 1059 :const B-4 1122

:data wave1 :include "Wave1.fs"
:data wave2 :include "Wave2.fs"

:var at  # audio timer
:var as  # audio speed (samples/note)
:var af1 # audio frequency, channel 1
:var af2 # audio frequency, channel 2
:var ap1 # audio pointer, channel 1
:var ap2 # audio pointer, channel 2
:var sfx # sound effect register
:var sft # sound effect timer

: tick ( -- )
	at @
	140 for
		# generate another sample:
		ap1 @ dup af1 @ + WAVE_MAX mod ap1 ! wave1 + @ 
		ap2 @ dup af2 @ + WAVE_MAX mod ap2 ! wave2 + @
		+ 2 /

		# mix in the sfx channel if necessary:
		sfx @ if
			sfx @ exec + 2 /
			sft @ 1 - dup sft !
			-if 0 sfx ! then
		then

		# bias and write out the sample:
		127 + AU !

		# if we're at the end of a note,
		# transfer to the audio program:
		1 - dup -if drop as @ leave then
	next
	at ! sync
;

: note   af2 ! af1 ! leave ; ( f1 f2 -- )

######################################################
##
##  A usage example:
##
######################################################

: gradius-song ( -- )
	900 dup as ! at !
	loop
		F-2 F-1 note
		E-2 F-1 note
		D-2 E-1 note
		E-2 D-1 note
		C-2 E-1 note
		G-1 C-1 note
		--- G-0 note
		--- --- note
		--- --- note
		G-1 --- note
		C-2 G-0 note
		G-1 C-1 note
		G$1 G-0 note
		C-2 G$0 note
		G$1 C-1 note
		D$2 G$0 note
		--- D$0 note
		--- --- note
		A$1 --- note
		D-2 A$0 note
		A$1 D-1 note
		F-2 A$0 note
		--- F-1 note
		--- --- note
		F-2 --- note
		E-2 F-1 note
		F-2 E-1 note
		G-2 F-1 note
		--- G-1 note
		--- --- note
		--- --- note
		--- --- note
		--- --- note
		G-1 --- note
		C-2 G-0 note
		G-1 C-1 note
		G$1 G-0 note
		D$2 G$0 note
		G$1 D$1 note
		C-2 G$0 note
		--- C-1 note
		--- --- note
		A$1 --- note
		F-1 A$0 note
		A$1 F-0 note
		D-2 A$0 note
		--- D-1 note
		--- --- note
		--- --- note
	again
;

: main ( -- )
	' gradius-song init-music
	loop
		tick
	again
;