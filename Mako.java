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
		window.pack();
		window.setResizable(false);
		window.setVisible(true);

		while(true) {
			view.vm.run();
			view.vm.keys = view.keys;
			view.repaint();
			try { Thread.sleep(10); }
			catch(InterruptedException ie) {}
		}
	}
}

class MakoPanel extends JPanel implements KeyListener {

	private static final Map<Integer, Integer> masks = new HashMap<Integer, Integer>();
	{
		masks.put(KeyEvent.VK_UP,     0x01); // key-up
		masks.put(KeyEvent.VK_RIGHT,  0x02); // key-rt
		masks.put(KeyEvent.VK_DOWN,   0x04); // key-dn
		masks.put(KeyEvent.VK_LEFT,   0x08); // key-lf
		masks.put(KeyEvent.VK_W,      0x01);
		masks.put(KeyEvent.VK_D,      0x02);
		masks.put(KeyEvent.VK_S,      0x04);
		masks.put(KeyEvent.VK_A,      0x08);
		masks.put(KeyEvent.VK_ENTER,  0x10); // key-a
		masks.put(KeyEvent.VK_SPACE,  0x10);
		masks.put(KeyEvent.VK_Z,      0x10);
		masks.put(KeyEvent.VK_X,      0x20); // key-b
		masks.put(KeyEvent.VK_ESCAPE, 0x20);
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
		g2.drawImage(vm.getBuffer(), 0, 0,   w,   h,
		                             0, 0, 320, 240, this);
	}

	public void keyPressed(KeyEvent k)  {
		if (k.getKeyCode() == KeyEvent.VK_ESCAPE) { System.exit(0); }
		if (masks.containsKey(k.getKeyCode())) { keys |=   masks.get(k.getKeyCode()) ; }
	}
	public void keyReleased(KeyEvent k) { if (masks.containsKey(k.getKeyCode())) { keys &= (~masks.get(k.getKeyCode())); }}
	public void keyTyped(KeyEvent k) {}
}