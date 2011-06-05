public interface MakoConstants {

	public static final int OP_CONST  = 0;
	public static final int OP_CALL   = 1;
	public static final int OP_JUMP   = 2;
	public static final int OP_JUMPZ  = 3;
	public static final int OP_JUMPIF = 4;

	public static final int OP_LOAD   = 10;
	public static final int OP_STOR   = 11;
	public static final int OP_RETURN = 12;
	public static final int OP_DROP   = 13;
	public static final int OP_SWAP   = 14;
	public static final int OP_DUP    = 15;
	public static final int OP_OVER   = 16;
	public static final int OP_STR    = 17;
	public static final int OP_RTS    = 18;

	public static final int OP_ADD    = 19;
	public static final int OP_SUB    = 20;
	public static final int OP_MUL    = 21;
	public static final int OP_DIV    = 22;
	public static final int OP_MOD    = 23;
	public static final int OP_AND    = 24;
	public static final int OP_OR     = 25;
	public static final int OP_XOR    = 26;
	public static final int OP_NOT    = 27;
	public static final int OP_SGT    = 28;
	public static final int OP_SLT    = 29;
	public static final int OP_ROR    = 30;
	public static final int OP_ROL    = 31;
	public static final int OP_SYNC   = 32;
	public static final int OP_KEYIN  = 33;

	public static final int PC =  0; // program counter
	public static final int DP =  1; // data stack pointer
	public static final int RP =  2; // return stack pointer

	public static final int GP =  3; // grid pointer
	public static final int GT =  4; // grid tile pointer
	public static final int SP =  5; // sprite pointer
	public static final int ST =  6; // sprite tile pointer
	public static final int SX =  7; // scroll X
	public static final int SY =  8; // scroll Y
	public static final int GS =  9; // grid horizontal skip
	public static final int CL = 10; // clear color
	public static final int RN = 11; // random number

	public static final int CO = 12; // character-out (debug)
	public static final int NO = 13; // numeric-out (debug)
	public static final int BK = 14; // breakpoint

	public static final int RESERVED_HEADER = 15;
}