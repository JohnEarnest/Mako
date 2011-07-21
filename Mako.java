import java.awt.*;
import java.awt.event.*;
import javax.swing.*;
import java.util.*;
import java.io.*;

public class Mako {

	public static void main(String[] args) {
		try {
			ArrayList<Integer> rom = new ArrayList<Integer>();
			Scanner in = new Scanner(new File(args[0]));
			while(in.hasNextInt()) {
				rom.add(in.nextInt());
			}
			int[] code = new int[rom.size()];
			for(int x = 0; x < rom.size(); x++) {
				code[x] = rom.get(x);
			}
			exec(code);
		}
		catch(FileNotFoundException e) {
			e.printStackTrace();
		}
	}

	public static void exec(int[] rom) {
		JFrame window   = new JFrame();
		MakoPanel view  = new MakoPanel(rom);

		window.addKeyListener(view);
		window.add(view);
		window.setTitle("Mako");
		window.setDefaultCloseOperation(JFrame.EXIT_ON_CLOSE);
		window.setResizable(false);
		window.pack();

		while(true) {
			view.vm.run();
			// if sync is never called, we'll assume it's meant
			// as a 'headless' application or test fixture.
			if (!window.isVisible()) { window.setVisible(true); }
			view.vm.keys = view.keys;
			view.repaint();
			try { Thread.sleep(10); }
			catch(InterruptedException ie) {}
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
		masks.put(KeyEvent.VK_D,      KEY_RT);
		masks.put(KeyEvent.VK_S,      KEY_DN);
		masks.put(KeyEvent.VK_A,      KEY_LF);
		masks.put(KeyEvent.VK_ENTER,  KEY_A); // key-a
		masks.put(KeyEvent.VK_SPACE,  KEY_A);
		masks.put(KeyEvent.VK_Z,      KEY_A);
		masks.put(KeyEvent.VK_X,      KEY_B); // key-b
		masks.put(KeyEvent.VK_ESCAPE, KEY_B);
	}

	private final int w = 960;
	private final int h = 720;
	public final MakoVM vm;
	public int keys = 0;
	
	public MakoPanel(int[] rom) {
		vm = new MakoVM(rom);
		setPreferredSize(new Dimension(w, h));
	}

	public void paint(Graphics g) {
		super.paint(g);
		Graphics2D g2 = (Graphics2D)g;
		g2.setRenderingHint( RenderingHints.KEY_RENDERING, RenderingHints.VALUE_RENDER_SPEED);
		g2.setRenderingHint( RenderingHints.KEY_INTERPOLATION, RenderingHints.VALUE_INTERPOLATION_NEAREST_NEIGHBOR);
		g2.drawImage(vm.buffer, 0, 0,   w,   h,
		                        0, 0, 320, 240, this);
	}

	public void keyPressed(KeyEvent k)  {
		if (k.getKeyCode() == KeyEvent.VK_ESCAPE) { System.exit(0); }
		if (masks.containsKey(k.getKeyCode())) { keys |=   masks.get(k.getKeyCode()) ; }
	}
	public void keyReleased(KeyEvent k) { if (masks.containsKey(k.getKeyCode())) { keys &= (~masks.get(k.getKeyCode())); }}
	public void keyTyped(KeyEvent k) {}
}