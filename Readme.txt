Mako is a simple stack-based virtual game console, designed to be as simple as possible to implement. Maker is a compiler for a Forth-like language that targets the Mako VM.

Mako has two stacks- a parameter stack and a return stack. Most MakoVM instructions manipulate the top elements of the parameter stack. Instructions are normally a single word (signed 32-bit integer), but some (like JUMP and CALL) are followed by a second word which provides an argument. The Mako memory layout is controlled by a number of memory-mapped registers starting in the lowest address- 0. In addition to the program counter and stack pointers, Mako has registers which control a pixel-scrollable 31x41 grid of 8x8 background tiles, a set of 256 variable-size sprites, a random number generator, optional character I/O facilities for debugging and an 8-bit audio output port.

The Maker source files provided in the examples directory can be executed by compiling Maker and then invoking it with a filename and the '--run' flag. Without the flag, Maker will simply print a disassembly of the prepared Mako memory image.

To compile, run `ant` from the top directory.

To run an example, invoke Maker from the command line:

	java -jar dist/Maker.jar /games/Pong/Pong.fs --run

Or use the 'maker' script to save a little typing:

	./maker /games/Pong/Pong.fs --run