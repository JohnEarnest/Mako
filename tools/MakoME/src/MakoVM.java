import javax.microedition.lcdui.Graphics;
import java.util.Random;

public class MakoVM implements MakoConstants {

	private final Random rand = new Random();
	public final int[] m;                 // main memory
	public int keys = 0;

	public MakoVM(int[] m) { this.m = m; }

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
			case OP_NEXT   : if (--m[m[RP]-1] >= 0) m[PC] = m[m[PC]]; break;
		}
	}

	private int load(int addr) {
		if (addr == RN) { return rand.nextInt(); }
		if (addr == KY) { return keys; }
		return m[addr];
	}

	private void stor(int addr, int value) {
		m[addr] = value;
	}

	private void drawTile(int tile, int px, int py, Graphics g) {
		if (tile < 0) { return; }
		int i = m[GT] + (tile * 8 * 8);
                g.drawRGB(m, i, 8, px, py, 8, 8, true);
	}

        private final int[] s = new int[4096]; // sprite buffer
	private void drawSprite(int tile, int status, int px, int py, Graphics g) {
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
                                int t =  m[i++];
				s[x + (y * 64)] = t;
			}
		}
                g.drawRGB(s, 0, 64, px, py, w, h, true);
	}

	public void sync(Graphics g) {
		final int scrollx = m[SX];
		final int scrolly = m[SY];

                // clear screen
                g.setColor(m[CL]);
                g.fillRect(0, 0, 320, 240);

                // draw grid
		int i = m[GP];
		for(int y = 0; y < 31; y++) {
			for(int x = 0; x < 41; x++) {
				drawTile(m[i++], x*8 - scrollx, y*8 - scrolly, g);
			}
			i += m[GS];
		}

                // draw sprites
		for(int sprite = 0; sprite < 1024; sprite += 4) {
			final int status = m[m[SP] + sprite    ];
			final int tile   = m[m[SP] + sprite + 1];
			final int px     = m[m[SP] + sprite + 2];
			final int py     = m[m[SP] + sprite + 3];
			drawSprite(tile, status, px - scrollx, py - scrolly, g);
		}
	}
}