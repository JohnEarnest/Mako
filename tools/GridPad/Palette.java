import java.io.*;
import java.util.*;
import java.util.List;
import java.awt.*;
import java.awt.event.*;
import javax.swing.*;

/**
* The Palette handles drawing (you guessed it) the
* tile palette as well as handling everything related to
* selecting or manipulating tiles.
**/
public class Palette extends JPanel implements MouseListener, MouseMotionListener, KeyListener {

	private static final List<Image> tilesets = new ArrayList<Image>();

	private final GridPad host;
	private Image tiles;
	private int tilesetIndex = 0;
	private int x = 0;
	private int y = 0;
	private int w = 1;
	private int h = 1;

	public Palette(GridPad host) {
		this.host = host;
		addMouseListener(this);
		addMouseMotionListener(this);
		loadTiles();
		setTiles(tilesets.get(tilesetIndex));
	}

	private void loadTiles() {
		tilesets.clear();
		for(String filename : new File("tilesets/").list()) {
			try {
				if (!filename.toLowerCase().endsWith(".png")) { continue; }
				Image tiles = Toolkit.getDefaultToolkit().createImage("tilesets/"+filename);
				while(tiles.getWidth(this) < 1) {
					try { Thread.sleep(1); }
					catch(InterruptedException ie) { }
				}
				tilesets.add(tiles);
			}
			catch(Throwable t) { continue; }
		}
	}

	private void setTiles(Image tiles) {
		this.tiles = tiles;
		setPreferredSize(new Dimension(
			tiles.getWidth(this)  * GridPad.SCALE,
			tiles.getHeight(this) * GridPad.SCALE
		));
		x = 0;
		y = 0;
	}

	public Image getTiles() {
		return tiles;
	}

	private int getTile(int a, int b) {
		return a + ((tiles.getWidth(this) / GridPad.TILE_WIDTH) * b);
	}

	public int[][] getSelected() {
		int[][] ret = new int[h][w];
		for(int a = 0; a < h; a++) {
			for(int b = 0; b < w; b++) {
				ret[a][b] = getTile(x + b, y + a);
			}
		}
		return ret;
	}

	public void paint(Graphics g) {
		super.paint(g);
		g.setColor(Color.GREEN.darker());
		g.fillRect(0, 0, getWidth(), getHeight());
		g.setColor(Color.GREEN);
		g.fillRect(getWidth()/2, 0, getWidth()/2, getHeight());
		g.drawImage(
			tiles,
			0,
			0,
			tiles.getWidth(this)  * GridPad.SCALE,
			tiles.getHeight(this) * GridPad.SCALE,
			0,
			0,
			tiles.getWidth(this),
			tiles.getHeight(this),
			this
		);
		g.setXORMode(Color.RED);
		g.drawRect(
			x * GridPad.TILE_WIDTH  * GridPad.SCALE,
			y * GridPad.TILE_HEIGHT * GridPad.SCALE,
			w * GridPad.TILE_WIDTH  * GridPad.SCALE,
			h * GridPad.TILE_HEIGHT * GridPad.SCALE
		);
		g.setPaintMode();
	}

	public void mouseDragged(MouseEvent e) {
		int nx = e.getX() / (GridPad.TILE_WIDTH  * GridPad.SCALE);
		int ny = e.getY() / (GridPad.TILE_HEIGHT * GridPad.SCALE);
		w = (nx - x) + 1;
		h = (ny - y) + 1;
		w = Math.max(w, 1);
		h = Math.max(h, 1);
		repaint();
	}

	public void mouseClicked(MouseEvent e) {
		x = e.getX() / (GridPad.TILE_WIDTH  * GridPad.SCALE);
		y = e.getY() / (GridPad.TILE_HEIGHT * GridPad.SCALE);
		w = 1;
		h = 1;
		host.updateStatus();
		repaint();
	}

	// update cursor position:
	public void keyPressed(KeyEvent e) {
		if      (e.getKeyCode() == KeyEvent.VK_RIGHT ||
		         e.getKeyCode() == KeyEvent.VK_D)       { x++; }
		else if (e.getKeyCode() == KeyEvent.VK_LEFT  ||
		         e.getKeyCode() == KeyEvent.VK_A)       { x--; }
		else if (e.getKeyCode() == KeyEvent.VK_UP    ||
		         e.getKeyCode() == KeyEvent.VK_W)       { y--; }
		else if (e.getKeyCode() == KeyEvent.VK_DOWN  ||
		         e.getKeyCode() == KeyEvent.VK_S)       { y++; }
		else { return; }

		final int maxWidth = (tiles.getWidth(this) / GridPad.TILE_WIDTH) - 1;
		x = Math.min(maxWidth, Math.max(0, x));
		y = Math.max(0, y);
		repaint();
	}

	// cycle through available tilesets:
	public void keyTyped(KeyEvent e) {
		if (e.getKeyChar() == '[') {
			tilesetIndex--;
			if (tilesetIndex < 0) { tilesetIndex = tilesets.size()-1; }
			setTiles(tilesets.get(tilesetIndex));
		}
		else if (e.getKeyChar() == ']') {
			tilesetIndex++;
			if (tilesetIndex >= tilesets.size()) { tilesetIndex = 0; }
			setTiles(tilesets.get(tilesetIndex));
		}
		else if (e.getKeyChar() == 'p') {
			loadTiles();
			setTiles(tilesets.get(tilesetIndex));
		}
		else { return; }
		host.repaint();
	}

	public void mousePressed(MouseEvent e) { mouseClicked(e); }

	public void mouseReleased(MouseEvent e) {}
	public void mouseEntered(MouseEvent e) {}
	public void mouseExited(MouseEvent e) {}
	public void mouseMoved(MouseEvent e) {}
	public void keyReleased(KeyEvent e) {}
}