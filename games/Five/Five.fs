######################################################
##
##  Five Hours of Power
##
##  A Five-Hour Energy tribute in game form.
##  Third CogNation Game Jam-
##  "...And now, a word from our sponsors..."
##
##  John Earnest
##
######################################################

:include <Print.fs>
:include <Grid.fs>
:include <Sprites.fs>
:include <Util.fs>
:include <String.fs>
:include <Math.fs>
:include <Game/Blip.fs>

:image title-tiles "titleTiles.png" 8 8

:data t1
:data sprite-tiles
:image grid-tiles  "tiles.png"  8  8
:image hands       "hands.png" 32 32

:image t2 "tiles2.png" 8 8
:image t3 "tiles3.png" 8 8
:image t4 "tiles4.png" 8 8

:data title-grid
	  0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0 
	  0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0 
	  0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0 
	  0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0 
	  0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0 
	  0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0 
	  0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0 
	  0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   1   2   3   4   5   6   7   8   9  10   0   0   0   0   0   0  11  12  13  14 
	  0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0   0  15  16  17  17  17  17  17  17  17  17  17  17  18  19   3   3   3  20  17  17  17  17 
	  0   0   0   0   0   0   0   0   0  21  22  22  23   0  24  25  26  27  17  17  17  17  17  17  17  17  17  17  17  17  17  17  17  17  17  17  17  17  17  17 
	  0   0   0   0   0   0   0   0  28  29  30  31  32  33  17  17  17  17  17  17  17  17  17  17  17  17  17  17  17  17  17  17  17  17  17  17  17  17  17  17 
	  0   0   0   0  34  35  36  37  38  39  40  41  17  17  42  43  17  17  17  17  17  17  17  17  17  17  17  17  17  17  17  17  17  17  17  17  17  17  17  17 
	 44  45  46  33  17  17  17  17  47  48  49  50  51  52  53  54  55  56  57  58  59  60  61  62  63  64  65  66  17  17  17  17  17  67  68  69  70  71  72  73 
	 17  74  75  17  17  17  17  17  76  17  77  78  79  80  81  82  83  84  85  86  87  88  89  90  91  92  93  94  73  95  96  97  98  94  94  94  94  94  94  94 
	 99 100 101  17  17  17  17 102 103 104 105 106 107 108 109 110 111 112 113 114 115 116 117 118 119 120  94  94  94  94  94  94  94  94  94  94  94  94  94  94 
	121 122 123 124 125 126 125 127 128 129 130 131 132 132 133 134 135 132 136 137 138  94  94 139 140 141 132 132 142 143 132 132 144 145  94  94  94  94  94  94 
	146 147 148 149  94  94  94  94  94  94 150 151 152 153 154 155 156 157 158 159 132 160 161 132 162 132 163 164 165 166 167 168 132 169  94  94  94  94  94  94 
	132 132 132 170 171  94  94  94  94  94 172 132 173 174 175 132 176  94 177 178 132 132 132 132 179 132 180 181  94 182 183 132 184 185  94  94  94  94 186 187 
	132 132 132 132 188 189  94  94  94  94 131 190  94  94 191 192 132 193 132 194 195 196 197 132 198 132 199 200 201 132 202 132 203  94 204 205 206 207 207 207 
	132 132 132 132 132 208  94  94  94  94 209 210  94  94  94 211 212 212 213 214 215 216 217 212 218 212 212 212 219 220 221 222 223 224 225 207 207 207 207 226 
	132 132 132 132 132 132 227 228 229  94  94  94  94  94  94  94  94  94  94  94  94  94  94  94  94  94  94 230 231 232 233 205 234 207 235 236 237 238 239 240 
	132 132 132 132 132 132 132 132 241 242 243  94  94  94  94  94  94  94  94  94  94  94  94 186 244 245 246 207 207 207 207 207 239 247 248 248 248 248 248 248 
	132 132 132 132 132 132 132 132 132 132 132 249 250 171  94  94 251 252 253 254 255 233 256 257 207 207 258 259 260 261 262 263 264 248 248 248 248 248 248 248 
	132 132 132 132 132 132 132 132 132 132 132 132 132 265 266 267 268 269 270 271 272 273 274 275 276 277 248 248 248 248 248 248 248 248 248 248 248 248 248 248 
	132 132 132 132 132 132 132 132 132 132 132 132 132 278 279 248 248 248 248 248 248 248 248 248 248 248 248 248 248 248 248 248 248 248 248 248 248 248 248 248 
	132 132 132 132 132 132 132 132 132 132 132 132 132 132 280 281 248 248 248 248 248 248 248 248 248 248 248 248 248 248 248 248 248 248 248 248 248 248 248 248 
	132 132 132 132 132 132 132 132 132 132 132 132 132 132 132 282 248 248 248 248 248 248 283 284 285 286 287 288 289 290 291 292 248 248 248 248 248 248 248 248 
	132 132 132 132 132 132 132 132 132 132 132 132 132 132 132 132 293 294 248 248 248 248 295 296 297 298 299 300 301 302 303 292 248 248 248 248 248 248 248 248 
	132 132 132 132 132 132 132 132 132 132 132 132 132 132 132 132 132 304 305 306 248 248 248 248 248 248 248 248 248 248 248 248 248 248 248 248 248 248 248 248 
	132 132 132 132 132 132 132 132 132 132 132 132 132 132 132 132 132 132 132 280 307 248 248 248 248 248 248 248 248 248 248 248 248 248 248 248 248 248 248 248

:data grid
	 4  5  5  5  5  5  4  5  4  5  5  5  4  5  4  4  5  4  5  5  5  4  5  5  5  5  4  5  5  4  5  5  5  5  5  5  5  4  5  5 -1 
	 5  8  8  8  8  8  8  8  8  8  8  8  4  5  5  5  4  5  8  8  8  8  8  8  8  8  8  8  8  8  8  8  8  8  8  8  8  8  8  5 -1 
	 5  8  9  9  9  9  9  8 11 10 11  8  8  8  8  8  5  4  8 28 28 28 28  8 28 28 28 28  8 28 74 28  8 28 28 28 28 57  8  4 -1 
	 4  8 29 30  7  7  7  8  7  7  7 12 88 88 88  8  5  5  8 23 23 23 23 23 23 23 23 23  8 23 23 23  8 58 23 23 23 73  8  5 -1 
	 5  8  7  7  7  7  7  7  7  7  7 86 86 87 86  8  4  5  8 95 58 23 23  8 23 58 41 23  8 23 23 23  8 23 23 23 23 23  8  5 -1 
	 5  8  7  7  7  7 31  8  7  7  7  6  6  6  6  8  5  5  8  8  8  8  8  8 23 23 23 23  8 23 23 23  8  8 23  8  8  8  8  5 -1 
	 4  8  8  8  8  8  8  8  7  7  7  8  8  8  8  8  5  4 13  8 28 28 28 28 23 23 23 23 28 23 23 23 28 28 23 28 28 59  8  5 -1 
	 5  8 14 15 15 12 12 12  7  7  7  8 13 13 13 13  5  5  5  8 23 94 23 23 23 23 23 23 23 23 23 23 23 23 23 23 23 75  8  4 -1 
	 5  8 20 21 22  2  2  2  7  7  7  8 55 56  4  5  5  4  5  8 40 25 58 23 23 23 23 23 23 23 23 23 23 23 23 23 23 23  8  5 -1 
	 4  8  2  2  2  2  2  8  8  7  8  8 71 72  5  5  5  5  5  8 23 23 23 23 23  8  8 23  8  8  8 23  8  8  8 23  8  8  8  4 -1 
	 5  8  8  8  8  8  8  8 13  6 13 13  5  5  4 55 56  5  5  8 23 23 23 23 23  8 27 23 13  8 27 23 13  8 27 23 13  8 13  5 -1 
	 5 13 13 13 13 13 13 13  5  3  5  5  4  5  5 71 72  5  5  8  8  8 23  8  8  8 26 93 23  8 26 93 23  8 26 23 23  8  5  5 -1 
	 5 55 56  5  5  5  4  5  5  3  5  5  5  4  5  5  5  5  5 13 13 13  2 13 13  8  8  8  8  8  8  8  8  8  8  8  8  8  5  5 -1 
	 5 71 72  4  5  4  5  4  5  3  5  5  4  5  5  4  5  4  5  5  4  5  3  4  5 13 13 13 13 13 13 13 13 13 13 13 13 13 55 56 -1 
	 5  4  5 55 56  5  5  5  5  3  5  5  4  5 60 61  5  4  5  5  5  5  3  5  5  5  5  5  4  5  5  5  5  5  5  4  5  5 71 72 -1 
	 5  5  5 71 72  5  4  5  5  3  5 55 56  4 77 79  5  5  5  5  4  5  3  5  4  5  5  5  5  5  4  5  5  5 55 56  5  5  5  4 -1 
	 5  4  5  5  5  5  5  5  5  3  5 71 72  5 40 25  5  5  5  5  5  5  3  5  5  4  5  5  5  5  5  5  5  5 71 72  5  4 55 56 -1 
	 5  5  5  4  5  5  5  5  5  3  5  5  5  5  5  5 36  5  5 36  3  3  3  5  8  8  8  8  8  8  8  8  8  8  8  8  8  8  8 72 -1 
	 3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  3  4  5  5  8 44 45 44 44 44 44 43 44 44 44 44 44 44  8  5 -1 
	 5  5  5  5  5  5  5  3  4  5  5  5  5  5  5  5 36 36  5  3  5  5  4  5  8 39 39 39 39 90 39 42 92 39 39 39 39 39  8  5 -1 
	 5  5  4  5  4  5  4  3  5  5  5  5  4  5 36  4 37  4 36  3  5  5  5  5  8 39 39 39 39 39 39 40 25 41 41 41 41 41  8  4 -1 
	 5  5  5  5  5  5  5  3  5  5  5  5  5  5  5  4 36  4  5  3  5  8  8  8  8 39 39 39 39 39 39 39 38 39 38 39 89 39  8  5 -1 
	 4 55 56  4  5  5  5  3  5  5  4 46 47  4  5  4  4  5  5  3  5  8 44 44  8 39 38 40 41 38 39 39 39 39 39 39 39 39  8  5 -1 
	 8  8  8  8  8 24  5  3  5  5  5 62 63  5  5  5  5  5  5  3  4  8 39 39  6 39 39 39 39 39 39 39 39 39 39 39 39 76  8  5 -1 
	 8 16 17 18 19  8  5  3  5  5  5 78 79  5  5  4 55 56  5  3  5  6 39 39  8 39 39 39 39 39 39 39 39 91 39 39 39 39  8  4 -1 
	 8 32 33 34 35  8  5  3  3  5  5 40 25  5  5  4 71 72  5  3  5  8 39 39  8 89 38 40 41 38 39 39 38 40 41 38 39 39  8  5 -1 
	 8 48 49 50 51  8  5  5  3  5  5  5  4  5  5  5  5  5  5  3  5  8  8  8  8  8  8  8  8  8  8  8  8  8  8  8  8  8  8  5 -1 
	 8 64 65 66 67  8  5  5  3  3  3  3  3  3  3  3  3  3  3  3  5 13 13 13 13 13 13 13 13 13 13 13 13 13 13 13 13 13 13  5 -1 
	 8  8  8  8  8  8 56  4  5  5  5  5  5  5  5  4  5  5  5  3  4  5  5  5  5  5  5  5  5  5  5  5  5  5  5  5  5  5  5  5 -1 
	 8  8  8  8  8  8  8  8  8  8  8  8  8  8  8  8  8  8  8  8  8  8  8  8  8  8  8  8  8  8  8  8  8  8  8  8  8  8  8  8 -1 
	-1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 

:var timer
:var energy
:var quota
:data money        500
:data vitamins       0
:data work-count     0
:data alarmset    true

:var dayover
:var worked
:var evened

:var drone-amp
:const player        64
:const clock        201
:const marker-base   65
:const dart         200

########################
# brass tacks
########################

: status ( addr -- )
	0 29 tile-grid@ 40 8 fill
	1 swap 29 swap grid-type
;

: drone ( -- )
	drone-amp @ -if exit then
	0 0 134 for
		RN @ drone-amp @ mod + 64 mod swap
		1 + 64 mod drone-amp @ swap - swap
		over over + 2 / AU !
	next 2drop
;

:const h1 32x32
:const h2 144129 
:const h3 209665
:const h4 78593
:const hand-len 24
:data hand-id  13 14 15 16 17 18 18 17 16 15 14 13 13 14 15 16 17 18 18 17 16 15 14 13
:data hand-st  h1 h1 h1 h1 h1 h1 h2 h2 h2 h2 h2 h2 h3 h3 h3 h3 h3 h3 h4 h4 h4 h4 h4 h4

: pass-time ( -- )
	timer inc@
	timer @ 200 /
	hand-len mod dup
	hand-st + @ swap
	hand-id + @
	8 192 clock >sprite
;

: dsync ( -- )
	energy @ if drone else pass-time then
	sync
;

: keyin ( -- )
	loop dsync keys key-a and while
	loop dsync keys key-a and until
;

: prompt ( addr -- )
	status keyin
;

:data progbar
	102 103 103 103 103 103 103 103 103 103 103 104 
	108   8   8   8   8   8   8   8   8   8   8 108 
	105 106 106 106 106 106 106 106 106 106 106 107 

: >swap ( a b len -- )
	1 - for
		2dup swap@
		1 + swap
		1 + swap
	next
	2drop
;

:const arrows 15
: progress ( -- )
	"Press arrows rapidly!" status
	14 12 tile-grid@ progbar      12 >swap
	14 13 tile-grid@ progbar 12 + 12 >swap
	14 14 tile-grid@ progbar 24 + 12 >swap
	15 13 tile-grid@ 10 8 fill
	
	29 for
		loop dsync keys arrows and while
		loop dsync keys arrows and until
		i 3 mod
		-if 30 i - 3 / 14 + 13 tile-grid@ 109 swap ! then
	next

	14 12 tile-grid@ progbar      12 >swap
	14 13 tile-grid@ progbar 12 + 12 >swap
	14 14 tile-grid@ progbar 24 + 12 >swap
;

########################
# gameplay objects
########################

:const bed          0
:const alarm-clock  1
:const shower       2
:const closet       3
:const kitchen      4

:const front-desk   5
:const secretary    6
:const slave1       7
:const slave2       8
:const desk         9
:const meeting     10
:const supplies    11
:const collate     12
:const copy        13
:const boss        14
:const water       15

:const sandwich    16
:const god         17
:const flower      18

:const slots       19
:const dude1       20
:const dude2       21
:const blonde      22
:const redhead     23
:const barkeep     24
:const dartboard   25

# descs
:data d-bed         "Place of sleep."
:data d-alarm-clock "The alarm clock."
:data d-shower      "Place of bodily cleansing."
:data d-closet      "My Clothing."
:data d-kitchen     "Food preparation materials."
:data d-front-desk  "The Front Desk."
:data d-secretary   "Wage slave secretary."
:data d-slave1      "Wage slave."
:data d-slave2      "Other wage slave."
:data d-desk        "My place of work."
:data d-meeting     "The place of meeting."
:data d-supplies    "SUPPLIES."
:data d-collate     "The place for collating."
:data d-copy        "The Xerographer."
:data d-boss        "Wage slave master."
:data d-water       "Liquid refreshment dispensary."
:data d-sandwich    "Sandwich artisan."
:data d-god         "Seller of 5-hour energy."
:data d-flower      "Smellable flowers. Stop."
:data d-slots       "Machine of the slots."
:data d-dude1       "Inebriate."
:data d-dude2       "Inebriate."
:data d-blonde      "Attractive Female."
:data d-redhead     "Secondary Attractive Female."
:data d-barkeep     "Inebriatologist/Vendor."
:data d-dartboard   "Board of the darts."

: no-action      ;
: end-sequence   ;
: always    true ;
:const object-size  6
:const object-count 26

:proto start-sequence
:proto sleep
:proto wake

: give-money ( n -- )
	god      wake
	sandwich wake
	money +@
;

# sequences
:data s-slave1   desk copy collate slave1 end-sequence
:data s-slave2   secretary supplies slave2 end-sequence
:data s-desk     secretary supplies water desk meeting end-sequence
:data s-meeting  slave1 slave2 boss end-sequence
:data s-blonde   barkeep blonde barkeep blonde end-sequence
:data s-redhead  barkeep redhead redhead redhead end-sequence

# scripts ( -- )
: a-bed
	"At last I sleep." prompt
	dayover on
;

: a-alarm-clock
	"Set my alarm clock." prompt
	alarmset on
	alarm-clock sleep
;

: a-front-desk
	"Signed in at work." prompt
	worked on
	0 work-count !
	front-desk  sleep
	alarm-clock sleep
	slave1      wake
	slave2      wake
	desk        wake
	meeting     wake
	water       wake
;

: a-slave1
	{
		"Completed task for Wage slave." prompt
		slave1 sleep
		work-count inc@
	}
	"Wage slave requires a report." prompt
	s-slave1 start-sequence
;

: a-slave2
	{
		"Helped other Wage slave." prompt
		"Asshole."                 prompt
		slave2 sleep
		work-count inc@
	}
	"Other wage slave wants supplies." prompt
	s-slave2 start-sequence
;

: a-desk
	{
		"My presentation was elucidating." prompt
		desk sleep
		work-count inc@
	}
	"I must invoke a Powerpoint." prompt
	s-desk start-sequence
;

: leave-meeting
	200 player px!
	 40 player py!
	7 for dsync 1 player +py next
;

: a-meeting
	{
		"My project was a success." prompt
		meeting sleep
		work-count inc@
		leave-meeting
	}
	"I am assigned a project." prompt
	s-meeting start-sequence
;

: a-boss
	work-count @ quota @ >= if
		"I fulfilled my quotas."          prompt
		"I am compensated for my work."   prompt
		"Obtained $50."                   prompt
		5000 give-money
	else
		"I did not meet my quotas today." prompt
		"I was not compensated."          prompt
		"Obtained $0."                    prompt
	then
	0 work-count !
	boss sleep
;

: a-water
	"Quaffed some water." prompt
	"Acceptable."         prompt
	1 vitamins +@
	water sleep
;

: a-sandwich
	money @ 375 >= if
		"Purchased a sandwich." prompt
		"Spent $3.75."          prompt
		375 money -@
		"Delicious."            prompt
		60 vitamins +@
	else
		"I cannot afford a sandwich." prompt
		"I need $3.75."               prompt
	then
	sandwich sleep
;

:data p1 "I need $1.99."
:data p2 "I need $4.99."
:data p3 "I need... $10.99?"
:data p4 "I need $14.99!"
:data p5 "I need $30.99! Outrageous!"
:data s1 "I spent $1.99."
:data s2 "I spent $4.99."
:data s3 "I spent $10.99. Hm."
:data s4 "I spent... $14.99?"
:data s5 "I spent $30.99! Highway robbery!"
:data energy-needed p1 p2 p3 p4 p5
:data energy-spent  s1 s2 s3 s4 s5
:data energy-prices 199 499 1099 1499 3099
:var energy-price

: a-god
	money @ energy-price @ energy-prices + @ >= if
		"Purchased a 5-hour energy."     prompt
		energy-price @ energy-spent  + @ prompt
		energy-price @ energy-prices + @ money -@
		energy-price @ 1 + 4 min energy-price !
		
		"Power courses through my veins..." prompt
		1000 energy !
		8333 vitamins +@
	else
		"I can't afford a 5-hour energy." prompt
		energy-price @ energy-needed + @  prompt
	then
	god sleep
;

: a-flower
	"I pause and smell the flowers." prompt
	"Relaxing."                      prompt
	flower sleep
;

: a-slots
	money @ 200 >= if
		"I take my chances at the slots." prompt
		"Spent $2."                       prompt
		200 money -@
		RN @ 10 mod 2 < if
			"I Win!"         prompt
			"Obtained $199!" prompt
			19900 give-money
		else
			"Nuts. I lost." prompt
		then
	else
		"I cannot afford to play slots." prompt
		"I need $2."                     prompt
	then
	barkeep   wake
	slots sleep
;

: a-blonde
	RN @ 10 mod 8 > if
		"The female is not thirsty." prompt
		blonde sleep
		exit
	then
	{
		"I have inebriated the female." prompt
		"Phone number obtained."        prompt
		blonde sleep
	}
	"The female wants alcohol." prompt
	s-blonde start-sequence
;

: a-redhead
	RN @ 10 mod 9 > if
		"The female is aloof." prompt
		redhead sleep
		exit
	then
	{
		"This female is actually very nice." prompt
		"Phone number obtained."             prompt
		redhead sleep
	}
	"The female wants alcohol." prompt
	s-redhead start-sequence
;

: leave-bar
	256 player px!
	168 player py!
	7 for dsync 1 player +py next
;

: a-barkeep
	money @ 172 >= if
		"I order an alcohol." prompt
		"Spent $1.72."        prompt
		172 money -@
		"Mmm, an alcohol."    prompt
		10 vitamins +@
	else
		"I cannot afford booze." prompt
		"I need $1.72."          prompt
	then
	leave-bar
	barkeep sleep
	slots wake
;

: weave ( -- )
	loop
		 7 for -1 player +py dsync keys key-a and if rdrop exit then next
		15 for  1 player +py dsync keys key-a and if rdrop exit then next
		 7 for -1 player +py dsync keys key-a and if rdrop exit then next
	again
;

: a-dartboard
	"I try my luck at the darts." prompt
	288 player px!
	184 player py!
	sync
	7 for
		-1 player +px
		sync
	next
	"Press space to throw!" status
	weave
	8x8 54 288 player py dart >sprite
	13 for
		1 dart +px
		sync
	next
	182 dart py 186 within if
		"Eye of the bull!"      prompt
		"Intensely gratifying." prompt
	else
		"I missed." prompt
	then
	280 player px!
	184 player py!
;

# tasks ( -- flag )
: t-closet
	player tile if
		0 player tile!
		"Stripped naked." prompt
	else
		1 player tile!
		"Got dressed." prompt
	then
	true
;

: t-shower
	"Cleansed Myself." prompt
	true
;

: t-kitchen
	"Ate breakfast." prompt
	20 vitamins +@
	true
;

: t-secretary
	"Obtained permission for supplies." prompt
	true
;

: t-slave1
	"Consulted with Wage Slave." prompt
	true
;

: t-slave2
	"Gave materials to other Wage slave." prompt
	true
;

: t-desk
	"Composing work..." prompt
	progress
	"Composed work." prompt
	true
;

: t-meeting
	"I presented my materials." prompt
	leave-meeting
	true
;

: t-supplies
	"Obtained Post-It notes." prompt
	true
;

: t-collate
	"Collating paperwork..." prompt
	progress
	"Collated paperwork." prompt
	true
;

: t-copy
	"Xerographed paperwork." prompt
	true
;

: t-boss
	"Asked questions of slave master." prompt
	true
;

: t-water
	"Quenched my thirst." prompt
	true
;

:data bs1 "I engage in smalltalk with the female."
:data bs2 "I flatter the female."
:data bs3 "I attempt to charm the female."
:data bs4 "I make idle banter with the female."
:data bs5 "She is not very interested."
:data bs6 "She yawns."
:data bs7 "She rolls her eyes."
:data bs8 "She glances at the barkeep."
:data blonde1 bs1 bs2 bs3 bs4
:data blonde2 bs5 bs6 bs7 bs8

: t-blonde
	RN @ 4 mod blonde1 + @ prompt
	RN @ 4 mod blonde2 + @ prompt
	true
;

:data rs1 "I discuss work with the female."
:data rs2 "I discuss my hobbies."
:data rs3 "We discuss our favorite movies."
:data rs4 "I talk about my allergies."
:data rs5 "She feigns interest politely."
:data rs6 "She giggles."
:data rs7 "She nods and smiles."
:data rs8 "She seems to be enjoying herself."
:data red1 rs1 rs2 rs3 rs4
:data red2 rs5 rs6 rs7 rs8

: t-redhead
	RN @ 4 mod red1 + @ prompt
	RN @ 4 mod red2 + @ prompt
	true
;

: t-barkeep
	money @ 382 >= if
		"I order a cocktail." prompt
		"Spent $3.82."        prompt
		382 money -@
		true
	else
		"I cannot afford booze." prompt
		"I need $3.82."          prompt
		false
	then
	leave-bar
;


# structs
:data objects
d-bed          3  3 false  a-bed          always
d-alarm-clock  6  5 false  a-alarm-clock  always
d-shower       2  7 false  no-action      t-shower
d-closet       9  2 false  no-action      t-closet
d-kitchen     13  4 false  no-action      t-kitchen
d-front-desk  21  8 false  a-front-desk   always
d-secretary   21  7 false  no-action      t-secretary
d-slave1      27 11 false  a-slave1       t-slave1
d-slave2      31 11 false  a-slave2       t-slave2
d-desk        34 11 false  a-desk         t-desk
d-meeting     25  4 false  a-meeting      t-meeting
d-supplies    30  2 false  no-action      t-supplies
d-collate     33  3 false  no-action      t-collate
d-copy        37  3 false  no-action      t-copy
d-boss        19  4 false  a-boss         t-boss
d-water       37  7 false  a-water        t-water
d-sandwich    15 16 false  a-sandwich     always
d-god         12 25 false  a-god          always
d-flower      16 20 false  a-flower       always
d-slots       26 18 false  a-slots        always
d-dude1       25 25 false  no-action      always
d-dude2       36 21 false  no-action      always
d-blonde      29 19 false  a-blonde       t-blonde
d-redhead     33 24 false  a-redhead      t-redhead
d-barkeep     32 20 false  a-barkeep      t-barkeep
d-dartboard   37 23 false  a-dartboard    always

: nth-object  object-size * objects + ; ( index -- addr )
: .desc           ;
: .x          1 + ;
: .y          2 + ;
: .awake      3 + ;
: .action     4 + ;
: .task       5 + ;

########################
# object interaction
########################

:var closest

: find-adjacent ( -- )
	-1 closest !
	object-count 1 - for
		player px 8 / i nth-object .x @ - -1 swap 1 within
		player py 8 / i nth-object .y @ - -1 swap 1 within and
		if
			i closest !
			rdrop exit
		then
	next
;

: draw-hud
	 0 29 tile-grid@ 40 8 fill
	36 29 ".00" grid-type
	38 29 money @ 100 mod draw-number
	35 29 money @ 100 /   draw-number
	37 money @ n-digits - 29 "$" grid-type
;

: scan ( -- )
	find-adjacent
	closest @ -1 > if
		closest @ nth-object .desc @ status
	else
		draw-hud
	then
;

: init-marker ( id -- )
	>r
	8x8 invisible 70
	i nth-object .x @     8 *
	i nth-object .y @ 1 - 8 *
	marker-base i + >sprite
	rdrop
;

: init-markers ( -- )
	object-count 1 - for
		i init-marker
	next
;

:data bounce -1 -1 0 1 1 0

: anim-markers ( -- )
	object-count 1 - for
		i nth-object .awake @ if
			timer @ 3 / 6 mod bounce + @
			i marker-base + +py
		then
	next
;

: sleep ( id -- )
	dup nth-object .awake off
	marker-base + hide
;

: wake ( id -- )
	dup nth-object .awake on
	dup init-marker
	marker-base + show
;

:array wakeful object-count 0

: save-status ( -- )
	object-count 1 - for
		i nth-object .awake @ i wakeful + !
	next
;

: restore-status ( -- )
	init-markers
	object-count 1 - for
		i wakeful + @ if i wake then
	next
;

:var sequence
:var sequence-index
:var sequence-finale

: start-sequence ( code addr -- )
	sequence on
	save-status
	init-markers
	sequence-index  !
	sequence-finale !
	sequence-index @ @ wake
;

: try-sequence ( id -- )
	dup sequence-index @ @ =
	if
		sequence-index @ @ nth-object .task @ exec if
			sleep
			sequence-index inc@
			sequence-index @ @ ' end-sequence = if
				sequence off
				restore-status
				sequence-finale @ exec
			else
				sequence-index @ @ wake
			then
		else
			drop
			sequence off
			restore-status
		then
	else
		drop
	then
;

########################
# time scripts
########################

:data morning-seq  shower closet kitchen end-sequence

: evening
	evened @ if exit then
	evened on

	sequence @ if
		sequence off
		restore-status
	then

	front-desk  sleep
	slave1      sleep
	slave2      sleep
	desk        sleep
	meeting     sleep
	water       sleep
	boss        wake
	slots       wake
	blonde      wake
	redhead     wake
	barkeep     wake
	dartboard   wake
	bed         wake
;

: late
	worked @ if exit then
	worked on
	"I am late for work."       prompt
	"Today is unpaid vacation." prompt
	sequence @ if
		sequence off
		restore-status
	then
	evening
;

: morning
	alarmset @ if
		"I awaken to my alarm." prompt
	else
		"My alarm does not go off." prompt
		"I am running late."        prompt
		200 5 * timer !
	then
	alarmset off

	{
		"I am prepared for the day." prompt
		init-markers
		sandwich    wake
		god         wake
		flower      wake
		alarm-clock wake
		front-desk  wake
	}
	morning-seq start-sequence
;

########################
# main game logic
########################

:const tilesetc 7
:data  tilesets t2 t3 t3 t3 t4 t4 t4

: tick ( -- )
	# drone logic
	energy @ if
		clock hide
		drone-amp @ 64 < if
			t2 GT !
			t2 ST !
			drone-amp inc@	
		else
			RN @ tilesetc mod tilesets + @
			dup GT !
			    ST !
		then
		drone
		energy dec@
	else
		drone-amp @ if
			t2 GT !
			t2 ST !
			drone-amp dec@
			drone
		else
			t1 GT !
			t1 ST !
			pass-time
			anim-markers
			
			#timer @ 200 / . cr
			timer @ 200 /  8 > if late then
			timer @ 200 / 15 > if evened @ -if "Work time is over." prompt then evening then
		then
	then
	sync
;

: cm ( dx dy )
	player py + swap
	player px + swap
	over 0 swap 312 within -if 2drop false exit then
	dup  0 swap 224 within -if 2drop false exit then
	c-tile? not
;

: *nrg energy @ if 2 * then ;
: /nrg energy @ if 2 / then ;


:array title-buff 20 248
: title ( -- )
	-1 GS !
	title-tiles GT !
	title-grid  GP !
	0 loop
		1 + dup 50 mod -if
			22 26 tile-grid@ title-buff      10 >swap
			22 27 tile-grid@ title-buff 10 + 10 >swap
		then
		sync
		keys key-a and
	until drop
	loop sync keys key-a and while
	0 GS !
	t1 GT !
	grid GP !
;

:data day1 "MONDAY."
:data day2 "TUESDAY."
:data day3 "WEDNESDAY."
:data day4 "THURSDAY."
:data day5 "FRIDAY."
:data day-names day1 day2 day3 day4 day5
:data quotas       1    2    2    3    4
:array start-buff 1271 8 0

: summary-mode  t1 GT ! t1 ST ! start-buff GP ! 255 for i hide next ;
: game-mode     start-buff 1271 8 fill grid GP ! ;

: day-start ( index -- )
	dup quotas + @ quota !

	summary-mode
	day-names + @ 40 over size - 2 / swap 10 swap grid-type
	50 for sync next

	 5 18 "Work task quota:" grid-type
	35 18 quota @ draw-number
	50 for sync next

	7 28 "Press any key to begin... " grid-type
	20 for sync next
	loop sync keys while
	loop sync keys until
	game-mode
;

: score-summary
	20 for sync next
	 5 16 "Money:" grid-type
	33 16 ".00" grid-type
	35 16 money @ 100 mod draw-number
	32 16 money @ 100 /   draw-number
	32 money @ 100 / n-digits - 16 "$" grid-type
	20 for sync next
	 5 18 "% Daily B-Vitamins:" grid-type
	35 18 vitamins @ draw-number
	20 for sync next
;

: day-summary
	summary-mode
	10 10 "END OF DAY SUMMARY: " grid-type
	score-summary
	11 28 "Press any key... " grid-type
	loop sync keys while
	loop sync keys until
	game-mode
;

: game-over
	summary-mode
	15 10 "GAME OVER." grid-type
	score-summary
	loop sync keys while
	loop sync keys until
	halt
;

: success
	summary-mode
	16 10 "VICTORY!" grid-type
	10 12 "I Survived the week!" grid-type
	score-summary
	loop sync keys while
	loop sync keys until
	halt
;

: gameloop
	8x8 0 24 32 player >sprite
	object-count 1 - for
		i nth-object .awake off
	next
	init-markers
	dart hide

	0 timer  !
	0 energy !
	0 work-count !
	0 drone-amp !
	dayover off
	worked  off
	evened  off

	morning
	loop
		keys key-rt and if
			player face-right
			 8  0 cm if 7 /nrg for  1 *nrg player +px tick next then
		then
		keys key-lf and if
			player face-left
			-8  0 cm if 7 /nrg for -1 *nrg player +px tick next then
		then
		keys key-up and if
			 0 -8 cm if 7 /nrg for -1 *nrg player +py tick next then
		then
		keys key-dn and if
			 0  8 cm if 7 /nrg for  1 *nrg player +py tick next then
		then
		
		scan
		keys key-a and if
			closest @ -1 > if
				sequence @ if
					closest @ try-sequence
				else
					closest @ nth-object .awake @ if
						closest @ nth-object .action @ exec
					then
				then
			then
		then

		player px 8 / 8 * player px!
		player py 8 / 8 * player py!

		dayover @ if break then
		timer @ 200 / hand-len 3 - > if "I NEED SLEEP BADLY." status then
		timer @ 200 / hand-len     > if game-over then

		tick
	again
	day-summary
;

: main
	64 16 + ascii !
	title
	0 day-start gameloop
	1 day-start gameloop
	2 day-start gameloop
	3 day-start gameloop
	4 day-start gameloop
	success
	halt
;