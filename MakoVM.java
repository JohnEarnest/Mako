import java.awt.Image;
import java.awt.Graphics;
import java.awt.Toolkit;
import java.awt.image.BufferedImage;
import java.awt.image.DirectColorModel;
import java.awt.image.MemoryImageSource;
import java.util.Random;

public class MakoVM implements MakoConstants {

	public final int[] m;                      // main memory
	public final int[] p = new int[320 * 240]; // pixel buffer
	
	private int keys = 0;
	private final Random rand = new Random();

	private final DirectColorModel dcm;
	private final MemoryImageSource mis;
	private final Image target;
	private final Image buffer;
	private final Graphics g;

	public MakoVM(int[] m) {
		this.m = m;
		dcm = new DirectColorModel(32,0xFF0000,0x00FF00,0x0000FF,0xFF000000);
		mis = new MemoryImageSource(320,240,dcm,p,0,320);
		target = Toolkit.getDefaultToolkit().createImage(mis);
		buffer = new BufferedImage(320,240, BufferedImage.TYPE_INT_ARGB);
		g = buffer.getGraphics();
		mis.setAnimated(true);
		mis.setFullBufferUpdates(true);
		for(int x = 0; x < p.length; x++) { p[x] = 0xFF000000; }
	}

	private void push(int v)      { m[m[DP]++] = v; }
	private void rpush(int v)     { m[m[RP]++] = v; }
	private int pop()             { return m[--m[DP]]; }
	private int rpop()            { return m[--m[RP]]; }
	private int mod(int a, int b) { a %= b; return a < 0 ? a+b : a; }

	public void run() {
		while(true) {
			if (m[m[PC]] == OP_SYNC) { sync(); m[PC]++; break; }
			try { tick(); }
			catch(IndexOutOfBoundsException e) {
				System.out.format("PC: %d Return: %d%n", m[PC], rpop());
				throw e;
			}
		}
	}

	public void tick() {
		int o = m[m[PC]++];
		int a = 0;
		int b = 0;

		switch(o) {
			case OP_CONST  :  push(m[m[PC]++]);                      break;
			case OP_CALL   : rpush(m[PC]+1); m[PC] = m[m[PC]];       break;
			case OP_JUMP   :                 m[PC] = m[m[PC]];       break;
			case OP_JUMPZ  : m[PC] = pop()==0 ? m[m[PC]] : m[PC]+1;  break;
			case OP_JUMPIF : m[PC] = pop()!=0 ? m[m[PC]] : m[PC]+1;  break;
			case OP_LOAD   : push(load(pop()));                      break;
			case OP_STOR   : stor(pop(),pop());                      break;
			case OP_RETURN : m[PC] = rpop();                         break;
			case OP_DROP   : pop();                                  break;
			case OP_SWAP   : a = pop(); b = pop(); push(a); push(b); break;
			case OP_DUP    : push(m[m[DP]-1]);                       break;
			case OP_OVER   : push(m[m[DP]-2]);                       break;
			case OP_STR    : rpush(pop());                           break;
			case OP_RTS    : push(rpop());                           break;
			case OP_ADD    : a = pop(); b = pop(); push(b+a);        break;
			case OP_SUB    : a = pop(); b = pop(); push(b-a);        break;
			case OP_MUL    : a = pop(); b = pop(); push(b*a);        break;
			case OP_DIV    : a = pop(); b = pop(); push(b/a);        break;
			case OP_MOD    : a = pop(); b = pop(); push(mod(b,a));   break;
			case OP_AND    : a = pop(); b = pop(); push(b&a);        break;
			case OP_OR     : a = pop(); b = pop(); push(b|a);        break;
			case OP_XOR    : a = pop(); b = pop(); push(b^a);        break;
			case OP_NOT    : push(~pop());                           break;
			case OP_SGT    : a = pop(); b = pop(); push(b>a ? -1:0); break;
			case OP_SLT    : a = pop(); b = pop(); push(b<a ? -1:0); break;
			case OP_KEYIN  : push(keys);                             break;
		}
	}

	private int load(int addr) {
		if (addr == RN) { return rand.nextInt(); }
		return m[addr];
	}

	private void stor(int addr, int value) {
		if (addr == RN) { rand.setSeed(value); return; }
		if (addr == CO) { System.out.print((char)value); return; }
		if (addr == BK) {
			System.out.format("Breakpoint @%d.%n", m[PC]);
			try { System.in.read(); }
			catch(Exception e) { e.printStackTrace(); }
		}
		m[addr] = value;
	}

	private void drawPixel(int x, int y, int c) {
		if (x < 0 || x >= 320 || y < 0 || y >= 240) { return; }
		p[x + (y * 320)] = c;
	}

	private void drawTile(int tile, int px, int py) {
		if (tile < 0) { return; }
		int i = m[GT] + (tile * 8 * 8);
		for(int y = 0; y < 8; y++) {
			for(int x = 0; x < 8; x++) {
				int c = m[i++];
				if ((c & 0xFF000000) == 0xFF000000) { drawPixel(x+px, y+py, c); }
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
				int c = m[i++];
				if ((c & 0xFF000000) == 0xFF000000) { drawPixel(x+px, y+py, c); }
			}
		}
	}

	private void sync() {
		final int scrollx = m[SX];
		final int scrolly = m[SY];
		final int clear   = m[CL];
		for(int x = 0; x < 320*240; x++) {
			p[x] = clear;
		}
		int i = m[GP];
		for(int y = 0; y < 31; y++) {
			for(int x = 0; x < 41; x++) {
				drawTile(m[i++], x*8 - scrollx, y*8 - scrolly);
			}
			i += m[GS];
		}
		for(int sprite = 0; sprite < 1024; sprite += 4) {
			final int status = m[m[SP] + sprite];
			final int tile = m[m[SP] + sprite + 1];
			final int px   = m[m[SP] + sprite + 2];
			final int py   = m[m[SP] + sprite + 3];
			drawSprite(tile, status, px - scrollx, py - scrolly);
		}
		synchronized(target) {
			mis.newPixels(0, 0, 320, 240);
		}
	}

	public Image getBuffer() {
		synchronized(target) {
			g.drawImage(target, 0, 0, null);
		}
		return buffer;
	}

	public void setKeys(int k) {
		keys = k;
	}
}