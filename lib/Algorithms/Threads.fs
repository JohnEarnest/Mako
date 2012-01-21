######################################################
##
##  Threads:
##
##  A basic implementation of a cooperative multitasking
##  system for Mako. Each task has an independent data
##  and return stack and may call 'yield' to transfer
##  control to the next task in round-robin order.
##  When all tasks 'finish', the program will halt.
##
##  John Earnest
##
######################################################

:data root-task 0 0 root-task
:data curr-task root-task

: .task-dp       ;
: .task-rp   1 + ;
: .task-next 2 + ;

: yield ( -- )
	DP @ curr-task @ .task-dp !
	RP @ curr-task @ .task-rp !
: [yield]
	curr-task @ .task-next @ curr-task !
	curr-task @ .task-dp @ DP !
	curr-task @ .task-rp @ RP !
;

: task ( entrypoint* task-record* -- )
	curr-task @ .task-next @ over .task-next !
	dup curr-task @ .task-next !
	.task-rp swap over @ ! dup @ 1 + swap !
;

: last-task? ( -- flag )
	curr-task @ dup .task-next @
	xor -if true else false then
;

: finish ( -- )
	last-task? if halt then
	curr-task @ dup .task-next @
	loop
		2dup .task-next @ xor -if break then
		.task-next @
	again
	swap .task-next @ swap .task-next !
	[yield]
;

######################################################
##
##  Buffered circular queues which use the
##  yielding system to block for input/output.
##
######################################################

: .buff-tail     ;
: .buff-head 1 + ;
: .buff-size 2 + ;
: .buff-addr 3 + ;

: >buffer ( value buff* -- )
	>r
	loop
		i .buff-tail @ i .buff-head @ 1 + i .buff-size @ mod
		xor if break then yield
	again
	i .buff-head @ i .buff-addr @ + !
	i .buff-head @ 1 + i .buff-size @ mod r> .buff-head !
;

: buffer> ( buff* -- )
	loop
		dup .buff-tail @ over .buff-head @
		xor if break then yield
	again
	>r
	i .buff-tail @ i .buff-addr @ + @
	i .buff-tail @ 1 + i .buff-size @ mod r> .buff-tail !
;

######################################################
##
##  Tests and examples:
##
######################################################

(
:include "Print.fs"

# producer 
:array prod-d 20 0
:array prod-r 20 0
:data  prod prod-d prod-r 0

# consumer
:array cons-d 20 0
:array cons-r 20 0
:data  cons cons-d cons-r 0

# queue
:const queue-size 5
:array queue-buff queue-size 0
:data  queue 0 0 queue-size queue-buff

: produce
	29 for
		i queue >buffer
	next
	"done producing" typeln
	finish
;

: consume
	29 for
		queue buffer> .
	next
	"done consuming" typeln
	finish
;

: main
	' produce prod task
	' consume cons task

	loop
		yield
		"interleaved..." typeln
		last-task?
	until
	"child processes complete." typeln
	finish
;
)