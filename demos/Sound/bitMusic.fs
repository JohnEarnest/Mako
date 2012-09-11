
: inc       dup @ 1 + swap !      ;
: dec       dup @ 1 - swap !      ;
: revector  1 + !                 ; ('new-word 'word --)
: s/        dup -if 2drop 0 else / then ;

:var    bft     # background sound timer
:vector bfx 0 ; # background sound function

:var sfx  # sound effect function pointer
:var sft  # sound effect timer
:var sfa  # sound effect register A
:var sfb  # sound effect register B

: tick ( -- )
	140 for
		bfx
		sfx @ if
			sft dec
			sfx @ exec 2 * + 3 /
			sft @ -if 0 sfx ! then
		then
		AU !
	next
	sync
;

: music1 ( -- sample )
	bft @ 8 /  bft @ 16384 and if 8 else 1 then bft @ swap / or
	bft @ 4 /  bft @ 16384 and if 4 else 1 then bft @ swap / or
	+ 2 /  bft @ 2 + bft !
;

: music2 ( -- sample )
	1000  bft @ 1024 / bft @ and s/ 1 and 35 *
	1000  bft @ 2048 / bft @ and s/ 1 and 35 *
	+ 2 /  bft @ 1 + bft !
;

# (1000/(t&8191)&1)*35
# 8191 = lower 13 bits. By modifying this constant
# we can vary the period of the drum beat. By modifying
# the leading factor (1000) we can vary the pitch of the drum beat:
: drum ( -- sample )
	1000  bft @ 8191 and s/ 1 and 35 *
	bft @ 1 + bft !
;

# (t*4*"6689"[t>>10&3]/24&24)
: melody ( -- sample )
	bft @ dup 1024 / 4 mod "6689" + @ * 12 / 24 and
	bft @ dup 4096 / 4 mod "6989" + @ * 96 / 16 and
	250 bft @ 16383 and s/ 1 and 63 *
	+ + 3 /
	bft @ 4 + bft !
;

: main
	' melody ' bfx revector
	loop tick again
;