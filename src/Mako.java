import java.awt.*;
import java.awt.event.*;
import java.awt.image.*;
import javax.swing.*;
import javax.imageio.*;
import java.util.*;
import java.io.*;
import java.awt.image.MemoryImageSource;

public class Mako {

	public static void main(String[] args) {
		String romFile = "Data.rom";

		try {
			DataInputStream in = new DataInputStream(Mako.class.getClassLoader().getResourceAsStream(romFile));
			//DataInputStream in = new DataInputStream(new FileInputStream(romFile));
			int[] rom = new int[in.available() / 4];
			for(int x = 0; x < rom.length; x++) {
				rom[x] = in.readInt();
			}
			exec(rom, false, null);
		}
		catch(IOException ioe) {
			System.out.println("Unable to load '"+romFile+"'.");
			System.exit(0);
		}
	}

	public static boolean trace       = false;
	public static boolean traceLive   = false;
	public static boolean guardCode   = false;
	public static boolean guardStacks = false;

	public static void exec(int[] rom, boolean fuzz, MakoRom traceRom) {
		JFrame window  = new JFrame();
		MakoPanel view = new MakoPanel(rom);

		window.addKeyListener(view);
		window.add(view);
		window.setTitle("Mako");
		window.setDefaultCloseOperation(JFrame.EXIT_ON_CLOSE);
		window.setResizable(false);
		window.pack();

		Random rng = new Random();
		Stack<String>        traceFunc   = new Stack<String>();
		Map<String, Integer> traceCalls  = new HashMap<String, Integer>();
		Map<String, Integer> traceCycles = new HashMap<String, Integer>();
		traceFunc.push("main");

		int datMin = view.vm.m[MakoConstants.DP];
		int retMin = view.vm.m[MakoConstants.RP];
		int datMax = arrayMax(datMin, traceRom);
		int retMax = arrayMax(retMin, traceRom);

		while(true) {
			long start = System.currentTimeMillis();

			view.ticks[view.tickptr] = 0;
			while(view.vm.m[view.vm.m[MakoConstants.PC]] != MakoConstants.OP_SYNC) {
				view.ticks[view.tickptr]++;

				if (trace) {
					int pc = view.vm.m[MakoConstants.PC];
					int op = view.vm.m[pc];
					int  a = view.vm.m[pc+1];
					incMap(traceCycles, traceFunc.peek());

					if (op == MakoConstants.OP_CALL) {
						traceFunc.push(traceRom.getLabel(a));
						incMap(traceCalls, traceFunc.peek());
					}
					if (op == MakoConstants.OP_RETURN) {
						traceFunc.pop();
					}
				}

				if (traceLive) {
					int pc = view.vm.m[MakoConstants.PC];
					int op = view.vm.m[pc];
					int  a = view.vm.m[pc+1];
					int ra = view.vm.m[view.vm.m[MakoConstants.RP]-1];

					if (op == MakoConstants.OP_CALL) {
						System.out.format("%-16s@%05d:\t( %s )%n", traceRom.getLabel(a), a, traceStack(datMin, retMin, rom));
					}
					if (op == MakoConstants.OP_RETURN) {
						System.out.format("RET              %05d:\t( %s )%n", ra, traceStack(datMin, retMin, rom));
					}
				}

				if (guardCode) {
					int pc = view.vm.m[MakoConstants.PC];
					int op   = view.vm.m[pc];
					int addr = view.vm.m[view.vm.m[MakoConstants.DP]-1];
					if (op == MakoConstants.OP_LOAD || op == MakoConstants.OP_STOR) {
						if (traceRom.getType(addr) == MakoRom.Type.Code) {
							System.out.format("Attempted to %s a code address!%n",
								op == MakoConstants.OP_LOAD ? "read" : "modify"
							);
							System.out.format("PC: %d Addr: %d%n", pc, addr);
							throw new Error("Guard violation.");
						}
					}
				}

				view.vm.tick();

				if (guardStacks) {
					int dp = view.vm.m[MakoConstants.DP];
					int rp = view.vm.m[MakoConstants.RP];
					if (dp < datMin || dp > datMax) {
						System.out.format("Data stack %sflow!%n", dp < datMin ? "under" : "over");
						System.out.format("DP: %d%n", view.vm.m[MakoConstants.DP]);
						System.out.format("PC: %d (Failed on previous instruction.)%n", view.vm.m[MakoConstants.PC]);
						throw new Error("Guard violation.");
					}
					if (rp < retMin || rp > retMax) {
						System.out.format("Return stack %sflow!%n", rp < retMin ? "under" : "over");
						System.out.format("RP: %d%n", view.vm.m[MakoConstants.RP]);
						System.out.format("PC: %d (Failed on previous instruction.)%n", view.vm.m[MakoConstants.PC]);
						throw new Error("Guard violation.");
					}
				}

				if (view.vm.m[MakoConstants.PC] == -1) {
					if (trace) { printTrace(traceCalls, traceCycles); }
					System.exit(0);
				}
			}
			view.vm.sync();
			view.vm.m[MakoConstants.PC]++;
			view.tickptr = (view.tickptr + 1) % view.ticks.length;
			
			view.vm.keys = view.keys;

			// 'fuzz' will generate a totally random key vector
			// every frame for the purposes of burn-in testing.
			if (fuzz) {
				view.vm.keys ^= rng.nextLong() & MakoConstants.KEY_MASK;
			}

			// if sync is never called, we'll assume it's meant
			// as a 'headless' application or test fixture.
			if (!window.isVisible()) { window.setVisible(true); }

			view.mis.newPixels();
			view.repaint();

			long total = System.currentTimeMillis() - start;

			if (total < 1000 / 60) {  // aim for 60fps
				try { Thread.sleep(1000 / 60 - total); }
				catch (InterruptedException ie) {}
			}
		}
	}

	private static int arrayMax(int base, MakoRom rom) {
		if (rom == null) { return base; }
		while(true) {
			if (base + 1 >= rom.size())                       { break; }
			if (rom.getType( base + 1) != MakoRom.Type.Array) { break; }
			if (rom.getLabel(base + 1).length() > 0)          { break; }
			base++;
		}
		return base;
	}

	private static void incMap(Map<String, Integer> m, String k) {
		if (k.startsWith("(")) { k = k.substring(1, k.length() - 1); }
		if (k.length() == 0) { k = "(unknown)"; }
		if (!m.containsKey(k)) { m.put(k, 1); }
		else { m.put(k, m.get(k)+1); }
	}

	private static void printTrace(
		final Map<String, Integer> calls,
		final Map<String, Integer> cycles
	) {
		System.out.println();
		System.out.println("Function           Cycles    Calls      Avg\n");
		long totalCalls  = 0;
		long totalCycles = 0;
		ArrayList<String> functions = new ArrayList<String>(calls.keySet());
		Collections.sort(functions, new Comparator<String>() {
			public int compare(String o1, String o2) {
				// I want to sort from greatest to smallest:
				return -1 * cycles.get(o1).compareTo(cycles.get(o2));
			}
		});

		for(String k : functions) {
			System.out.format("%-16s %8d %8d %8d%n",
				k,
				cycles.get(k),
				calls.get(k),
				cycles.get(k) / calls.get(k)
			);
			totalCalls  += calls.get(k);
			totalCycles += cycles.get(k);
		}
		System.out.println();
		System.out.format("%-16s %8d %8d%n",
			"Total:",
			totalCycles,
			totalCalls
		);
		System.out.println();
	}

	private static String traceStack(int datMin, int retMin, int[] rom) {
		String dstack = "";
		for(int x = rom[MakoConstants.DP] - 1; x >= datMin; x--) {
			dstack = rom[x] + " " + dstack;
		}
		String rstack = "";
		for(int x = rom[MakoConstants.RP] - 1; x >= retMin; x--) {
			rstack = rstack + " " + rom[x];
		}
		return String.format("%20s|%-30s", dstack, rstack);
	}
}

class MakoPanel extends JPanel implements KeyListener, MakoConstants {

	public static final long serialVersionUID = 1337;

	private static final Map<Integer, Integer> masks = new HashMap<Integer, Integer>();
	{
		masks.put(KeyEvent.VK_UP,     KEY_UP); // key-up
		masks.put(KeyEvent.VK_RIGHT,  KEY_RT); // key-rt
		masks.put(KeyEvent.VK_DOWN,   KEY_DN); // key-dn
		masks.put(KeyEvent.VK_LEFT,   KEY_LF); // key-lf
		/*
		masks.put(KeyEvent.VK_W,      KEY_UP);
		masks.put(KeyEvent.VK_A,      KEY_LF);
		masks.put(KeyEvent.VK_S,      KEY_DN);
		masks.put(KeyEvent.VK_D,      KEY_RT);
		*/
		masks.put(KeyEvent.VK_ENTER,  KEY_A); // key-a
		masks.put(KeyEvent.VK_SPACE,  KEY_A);
		masks.put(KeyEvent.VK_Z,      KEY_A);
		masks.put(KeyEvent.VK_X,      KEY_B); // key-b
		masks.put(KeyEvent.VK_SHIFT,  KEY_B);
	}

	private final int w = 960;
	private final int h = 720;
	private final Image buffer;
	public final MemoryImageSource mis;
	public final MakoVM vm;
	public int keys  = 0;
	
	public MakoPanel(int[] rom) {
		vm = new MakoVM(rom);
		setPreferredSize(new Dimension(w, h));
		mis = new MemoryImageSource(320, 240, vm.p, 0, 320);
		buffer = Toolkit.getDefaultToolkit().createImage(mis);
		mis.setAnimated(true);
	}

	public void paint(Graphics g) {
		super.paint(g);
		Graphics2D g2 = (Graphics2D)g;
		g2.setRenderingHint( RenderingHints.KEY_RENDERING, RenderingHints.VALUE_RENDER_SPEED);
		g2.setRenderingHint( RenderingHints.KEY_INTERPOLATION, RenderingHints.VALUE_INTERPOLATION_NEAREST_NEIGHBOR);
		g2.drawImage(buffer, 0, 0,   w,   h,
		                     0, 0, 320, 240, this);

		if (showTicks) {
			long sum = 0;
			for(Integer i : ticks) { sum += i; }
			g2.setColor(Color.GREEN);
			g2.drawString(""+(sum/ticks.length), 20, 20);
		}
	}

	private int screenShot = 0;
	private boolean showTicks = false;
	public int[] ticks = new int[30];
	public int tickptr = 0;

	public void keyPressed(KeyEvent k)  {
		if (k.getKeyCode() == KeyEvent.VK_ESCAPE) { System.exit(0); }
		if (masks.containsKey(k.getKeyCode())) { keys |= masks.get(k.getKeyCode()) ; }
	}
	public void keyReleased(KeyEvent k) {
		if (k.getKeyCode() == KeyEvent.VK_F6) {
			try {
				BufferedImage shot = new BufferedImage(320, 240, BufferedImage.TYPE_INT_ARGB);
				shot.getGraphics().drawImage(buffer, 0, 0, this);
				File file = new File("ScreenShot"+(screenShot++)+".png");
				System.out.println("Wrote screenshot: "+file);
				ImageIO.write(shot, "png", file);
			}
			catch(IOException e) { e.printStackTrace(); }
		}
		if (k.getKeyCode() == KeyEvent.VK_F5) {
			showTicks = !showTicks;
		}
		if (k.getKeyCode() == KeyEvent.VK_F7) {
			System.out.println("!");
			Mako.traceLive = !Mako.traceLive;
		}
		if (k.getKeyCode() == KeyEvent.VK_F8) {
			System.out.println("Interrupted!");
			vm.m[MakoConstants.PC] = Integer.MAX_VALUE;
		}
		if (masks.containsKey(k.getKeyCode())) { keys &= (~masks.get(k.getKeyCode())); }
	}
	public void keyTyped(KeyEvent k) {
		vm.keyQueue.add((int)k.getKeyChar());
	}
}