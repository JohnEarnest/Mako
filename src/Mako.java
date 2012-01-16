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
			exec(rom, false);
		}
		catch(IOException ioe) {
			System.out.println("Unable to load '"+romFile+"'.");
			System.exit(0);
		}
	}

	public static void exec(int[] rom, boolean fuzz) {
		JFrame window  = new JFrame();
		MakoPanel view = new MakoPanel(rom);

		window.addKeyListener(view);
		window.add(view);
		window.setTitle("Mako");
		window.setDefaultCloseOperation(JFrame.EXIT_ON_CLOSE);
		window.setResizable(false);
		window.pack();

		Random rng = new Random();

		while(true) {
			long start = System.currentTimeMillis();

			view.ticks[view.tickptr] = 0;
			while(view.vm.m[view.vm.m[MakoConstants.PC]] != MakoConstants.OP_SYNC) {
				view.ticks[view.tickptr]++;
				view.vm.tick();
				if (view.vm.m[MakoConstants.PC] == -1) { System.exit(0); }
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
}

class MakoPanel extends JPanel implements KeyListener, MakoConstants {

	private static final Map<Integer, Integer> masks = new HashMap<Integer, Integer>();
	{
		masks.put(KeyEvent.VK_UP,     KEY_UP); // key-up
		masks.put(KeyEvent.VK_RIGHT,  KEY_RT); // key-rt
		masks.put(KeyEvent.VK_DOWN,   KEY_DN); // key-dn
		masks.put(KeyEvent.VK_LEFT,   KEY_LF); // key-lf
		masks.put(KeyEvent.VK_W,      KEY_UP);
		masks.put(KeyEvent.VK_A,      KEY_LF);
		masks.put(KeyEvent.VK_S,      KEY_DN);
		masks.put(KeyEvent.VK_D,      KEY_RT);
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
				ImageIO.write(shot, "png", new File("ScreenShot"+(screenShot++)+".png"));
			}
			catch(IOException e) { e.printStackTrace(); }
		}
		if (k.getKeyCode() == KeyEvent.VK_F5) {
			showTicks = !showTicks;
		}
		if (masks.containsKey(k.getKeyCode())) { keys &= (~masks.get(k.getKeyCode())); }
	}
	public void keyTyped(KeyEvent k) {}
}