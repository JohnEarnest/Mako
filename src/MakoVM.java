import javax.sound.sampled.*;
import java.util.Random;

public class MakoVM implements MakoConstants {

	private final Random rand = new Random();
	public final int[] m;                      // main memory
	public final int[] p = new int[320 * 240]; // pixel buffer
	public int keys = 0;

	private SourceDataLine soundLine = null;
	private final byte[] abuffer = new byte[8040];
	private int apointer = 0;

	public final java.util.Queue<Integer> keyQueue = new java.util.LinkedList<Integer>();

	public MakoVM(int[] m) {
		this.m = m;
		try {
			AudioFormat format = new AudioFormat(8040f, 8, 1, false, false);
			DataLine.Info info = new DataLine.Info(SourceDataLine.class, format);
			soundLine = (SourceDataLine)AudioSystem.getLine(info);
			soundLine.open(format, 670);
			soundLine.start();
		}
		catch(IllegalArgumentException e) { System.out.println("Unable to initialize sound."); }
		catch(LineUnavailableException e) { e.printStackTrace(); }
	}

	private void push(int v)      { m[m[DP]++] = v; }
	private void rpush(int v)     { m[m[RP]++] = v; }
	private int pop()             { return m[--m[DP]]; }
	private int rpop()            { return m[--m[RP]]; }
	private int mod(int a, int b) { a %= b; return a < 0 ? a+b : a; }

	public void run() {
		while(m[m[PC]] != OP_SYNC) {
			tick();
			if (m[PC] == -1) { System.exit(0); }
		}
		sync();
		m[PC]++;
	}

	public void tick() {
		int o = m[m[PC]++];
		int a, b;

		switch(o) {
			case OP_CONST  :  push(m[m[PC]++]);                       break;
			case OP_CALL   : rpush(m[PC]+1); m[PC] = m[m[PC]];        break;
			case OP_JUMP   :                 m[PC] = m[m[PC]];        break;
			case OP_JUMPZ  : m[PC] = pop()==0 ? m[m[PC]] : m[PC]+1;   break;
			case OP_JUMPIF : m[PC] = pop()!=0 ? m[m[PC]] : m[PC]+1;   break;
			case OP_LOAD   : push(load(pop()));                       break;
			case OP_STOR   : stor(pop(),pop());                       break;
			case OP_RETURN : m[PC] = rpop();                          break;
			case OP_DROP   : pop();                                   break;
			case OP_SWAP   : a = pop(); b = pop(); push(a); push(b);  break;
			case OP_DUP    : push(m[m[DP]-1]);                        break;
			case OP_OVER   : push(m[m[DP]-2]);                        break;
			case OP_STR    : rpush(pop());                            break;
			case OP_RTS    : push(rpop());                            break;
			case OP_ADD    : a = pop(); b = pop(); push(b+a);         break;
			case OP_SUB    : a = pop(); b = pop(); push(b-a);         break;
			case OP_MUL    : a = pop(); b = pop(); push(b*a);         break;
			case OP_DIV    : a = pop(); b = pop(); push(b/a);         break;
			case OP_MOD    : a = pop(); b = pop(); push(mod(b,a));    break;
			case OP_AND    : a = pop(); b = pop(); push(b&a);         break;
			case OP_OR     : a = pop(); b = pop(); push(b|a);         break;
			case OP_XOR    : a = pop(); b = pop(); push(b^a);         break;
			case OP_NOT    : push(~pop());                            break;
			case OP_SGT    : a = pop(); b = pop(); push(b>a ? -1:0);  break;
			case OP_SLT    : a = pop(); b = pop(); push(b<a ? -1:0);  break;
			case OP_NEXT   : m[PC] = --m[m[RP]-1]<0?m[PC]+1:m[m[PC]]; break;
		}
	}

	private int load(int addr) {
		if (addr == RN) { return rand.nextInt(); }
		if (addr == KY) { return keys; }
		if (addr == KB) {
			if (keyQueue.size() > 0) { return keyQueue.remove(); }
			return -1;
		}
		if (addr == CO) {
			try { return System.in.read(); }
			catch(java.io.IOException e) { e.printStackTrace(); }
		}
		return m[addr];
	}

	private void stor(int addr, int value) {
		if (addr == CO) { System.out.print((char)value); return; }
		if (addr == AU) {
			abuffer[apointer] = (byte)value;
			if (apointer < abuffer.length - 1) { apointer++; }
		}
		m[addr] = value;
	}

	private void drawPixel(int x, int y, int c) {
		if ((c & 0xFF000000) != 0xFF000000)         { return; }
		if (x < 0 || x >= 320 || y < 0 || y >= 240) { return; }
		p[x + (y * 320)] = c;
	}

	private void drawTile(int tile, int px, int py) {
		tile &= ~GRID_Z_MASK;
		if (tile < 0) { return; }
		int i = m[GT] + (tile * 8 * 8);
		for(int y = 0; y < 8; y++) {
			for(int x = 0; x < 8; x++) {
				drawPixel(x+px, y+py, m[i++]);
			}
		}
	}

	private void drawSprite(int tile, int status, int px, int py) {
		if (status % 2 == 0) { return; }
		final int w = (((status & 0x0F00) >>  8) + 1) << 3;
		final int h = (((status & 0xF000) >> 12) + 1) << 3;
		int xd = 1; int x0 = 0; int x1 = w;
		int yd = 1; int y0 = 0; int y1 = h;
		if ((status & H_MIRROR_MASK) != 0) { xd = -1; x0 = w - 1; x1 = -1; }
		if ((status & V_MIRROR_MASK) != 0) { yd = -1; y0 = h - 1; y1 = -1; }
		int i = m[ST] + (tile * w * h);
		for(int y = y0; y != y1; y += yd) {
			for(int x = x0; x != x1; x += xd) {
				drawPixel(x+px, y+py, m[i++]);
			}
		}
	}

	private void drawGrid(boolean hiz, int scrollx, int scrolly) {
		int i = m[GP];
		for(int y = 0; y < 31; y++) {
			for(int x = 0; x < 41; x++) {
				if (!hiz && (m[i] & GRID_Z_MASK) != 0) { i++; continue; }
				if ( hiz && (m[i] & GRID_Z_MASK) == 0) { i++; continue; }
				drawTile(m[i++], x*8 - scrollx, y*8 - scrolly);
			}
			i += m[GS];
		}
	}

	public void sync() {
		final int scrollx = m[SX];
		final int scrolly = m[SY];
		java.util.Arrays.fill(p, m[CL]);
		drawGrid(false, scrollx, scrolly);
		for(int sprite = 0; sprite < 1024; sprite += 4) {
			final int status = m[m[SP] + sprite    ];
			final int tile   = m[m[SP] + sprite + 1];
			final int px     = m[m[SP] + sprite + 2];
			final int py     = m[m[SP] + sprite + 3];
			drawSprite(tile, status, px - scrollx, py - scrolly);
		}
		drawGrid(true, scrollx, scrolly);

		if (soundLine != null && apointer > 0) {
			soundLine.write(abuffer, 0, apointer);
			apointer = 0;
		}
	}
}
