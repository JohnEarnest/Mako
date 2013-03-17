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
		exec(loadRom(Mako.class.getClassLoader().getResourceAsStream("Data.rom"), null));
	}

	private static int[] loadRom(InputStream i, int[] prev) {
		try {
			DataInputStream in = new DataInputStream(i);
			int[] rom = new int[in.available() / 4];
			for(int x = 0; x < rom.length; x++) {
				rom[x] = in.readInt();
			}
			in.close();
			System.out.println("Restored from save file!");
			return rom;
		}
		catch(IOException ioe) {
			System.out.println("Unable to load rom!");
			return prev;
		}
	}

	private static void saveRom(int[] rom) {
		try {
			DataOutputStream out = new DataOutputStream(new FileOutputStream("Freeze.rom"));
			for(int x : rom) {
				out.writeInt(x);
			}
			out.close();
			System.out.println("Wrote save file!");
		}
		catch(IOException ioe) {
			System.out.println("Unable to save rom!");
		}
	}

	public static void exec(int[] rom) {
		JFrame window  = new JFrame();
		MakoPanel view = new MakoPanel(rom);

		window.addKeyListener(view);
		window.add(view);
		window.setTitle("Mako");
		window.setDefaultCloseOperation(JFrame.EXIT_ON_CLOSE);
		window.setResizable(false);
		window.pack();
		window.setVisible(true);

		while(true) {
			if (view.signal == 1) {
				saveRom(view.vm.m);
				view.signal = 0;
			}
			if (view.signal == 2) {
				try {
					view.vm.m = loadRom(new FileInputStream("Freeze.rom"), view.vm.m);
				}
				catch(FileNotFoundException e) {
					System.out.println("No frozen rom found.");
				}
				view.signal = 0;
			}

			long start = System.currentTimeMillis();

			while(view.vm.m[view.vm.m[MakoConstants.PC]] != MakoConstants.OP_SYNC) {
				view.vm.tick();
				if (view.vm.m[MakoConstants.PC] == -1) { System.exit(0); }
			}

			view.vm.sync();
			view.vm.m[MakoConstants.PC]++;
			view.vm.keys = view.keys;
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

	public static final long serialVersionUID = 1337;

	private static final Map<Integer, Integer> masks = new HashMap<Integer, Integer>();
	{
		masks.put(KeyEvent.VK_UP,     KEY_UP); // key-up
		masks.put(KeyEvent.VK_RIGHT,  KEY_RT); // key-rt
		masks.put(KeyEvent.VK_DOWN,   KEY_DN); // key-dn
		masks.put(KeyEvent.VK_LEFT,   KEY_LF); // key-lf
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
	public MakoVM vm;
	public int keys  = 0;
	public int signal = 0;
	
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
	}

	public void keyPressed(KeyEvent k)  {
		if (k.getKeyCode() == KeyEvent.VK_ESCAPE) { System.exit(0); }
		if (masks.containsKey(k.getKeyCode())) { keys |= masks.get(k.getKeyCode()) ; }
	}

	public void keyReleased(KeyEvent k) {
		if (k.getKeyCode() == KeyEvent.VK_F3) { signal = 1; }
		if (k.getKeyCode() == KeyEvent.VK_F4) { signal = 2; }
		if (masks.containsKey(k.getKeyCode())) { keys &= (~masks.get(k.getKeyCode())); }
	}

	public void keyTyped(KeyEvent k) {
		vm.keyQueue.add((int)k.getKeyChar());
	}
}