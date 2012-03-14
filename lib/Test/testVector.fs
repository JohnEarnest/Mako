:include <Vector.fs>
:include <Test/Tap.fs>

:vector a 5 ;
:       b 8 ;

: main
	7 plan
	666

	a              5 = "baseline" ok
	' a vectored?  0 = "vectored?" ok

	' b ' a revector
	a              8 = "revector" ok
	' a vectored? -1 = "vectored?" ok
	' a default    5 = "default" ok

	' a devector
	a              5 = "devector" ok

	666 = "canary" ok
;