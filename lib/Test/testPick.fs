:include <Pick.fs>
:include <Test/Tap.fs>

: main
	7 plan

	55 99 66 33
	1 pick 66 = "p1" ok
	0 pick 33 = "p2" ok
	3 pick 55 = "p3" ok
	2 pick 99 = "p4" ok

	43 >r 98 >r 41 >r
	1 rpick 98 = "rp1" ok
	0 rpick 41 = "rp2" ok
	2 rpick 43 = "rp3" ok
;