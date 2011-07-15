import java.util.Stack;
import java.awt.*;
import java.awt.event.*;
import javax.swing.*;

public class Editor extends JPanel implements MouseListener, MouseMotionListener, KeyListener {

	private final int CONTROL_C =  3;
	private final int CONTROL_F =  6;
	private final int CONTROL_L = 12;
	private final int CONTROL_O = 15;
	private final int CONTROL_R = 18;
	private final int CONTROL_V = 22;
	private final int CONTROL_Z = 26;

	private final JFrame  host;
	private final Palette palette;
	private final JLabel  status;

	private int[][] grid;
	private int[][] clipBoard;
	private int x;
	private int y;
	private int w = 1;
	private int h = 1;
	private boolean draw       = true;
	private boolean drawCursor = false;
	private int dragButton = MouseEvent.NOBUTTON;

	private Stack<Edit> undo = new Stack<Edit>();
	private Stack<Edit> redo = new Stack<Edit>();

	public Editor(JFrame host, Palette palette, JLabel status) {
		this.host    = host;
		this.palette = palette;
		this.status  = status;
		addMouseListener(this);
		addMouseMotionListener(this);
	}

	public void setGrid(int[][] grid) {
		this.grid = grid;
		setPreferredSize(new Dimension(
			grid[0].length * GridPad.TILE_WIDTH  * GridPad.SCALE,
			grid.length    * GridPad.TILE_HEIGHT * GridPad.SCALE
		));
		revalidate();
	}

	public void paint(Graphics g) {
		super.paint(g);
		g.setColor(Color.PINK);
		g.fillRect(0, 0, getWidth(), getHeight());

		for(int y = 0; y < grid.length; y += (y == 0) ? 31 : 30) {
			for(int x = 0; x < grid[0].length; x += (x == 0) ? 41 : 40) {
				g.setColor(new Color(255, 100, 100));
				g.fillRect(
					GridPad.TILE_WIDTH  * GridPad.SCALE * x,
					GridPad.TILE_HEIGHT * GridPad.SCALE * (y + ((y == 0) ? 30 : 29)),
					GridPad.TILE_WIDTH  * GridPad.SCALE *      ((x == 0) ? 41 : 40),
					GridPad.TILE_HEIGHT * GridPad.SCALE
				);
				g.fillRect(
					GridPad.TILE_WIDTH  * GridPad.SCALE * (x + ((x == 0) ? 40 : 39)),
					GridPad.TILE_HEIGHT * GridPad.SCALE * y,
					GridPad.TILE_WIDTH  * GridPad.SCALE,
					GridPad.TILE_HEIGHT * GridPad.SCALE *      ((y == 0) ? 31 : 30)
				);
				g.setColor(new Color(255, 200, 200));
				g.fillRect(
					GridPad.TILE_WIDTH  * GridPad.SCALE * x,
					GridPad.TILE_HEIGHT * GridPad.SCALE * (y + ((y == 0) ? 24 : 23)),
					GridPad.TILE_WIDTH  * GridPad.SCALE *      ((x == 0) ? 40 : 39),
					GridPad.TILE_HEIGHT * GridPad.SCALE * 6
				);
			}
		}

		final int xtiles = palette.getTiles().getWidth(this) / GridPad.TILE_WIDTH;
		for(int a = 0; a < grid.length; a++) {
			for(int b = 0; b < grid[0].length; b++) {

				int gx = b * GridPad.TILE_WIDTH  * GridPad.SCALE;
				int gy = a * GridPad.TILE_HEIGHT * GridPad.SCALE;
				int tx = (grid[a][b] % xtiles) * GridPad.TILE_WIDTH;
				int ty = (grid[a][b] / xtiles) * GridPad.TILE_HEIGHT;
				g.drawImage(
					palette.getTiles(),
					gx,
					gy,
					gx + GridPad.TILE_WIDTH  * GridPad.SCALE,
					gy + GridPad.TILE_HEIGHT * GridPad.SCALE,
					tx,
					ty,
					tx + GridPad.TILE_WIDTH,
					ty + GridPad.TILE_HEIGHT,
					this
				);
			}
		}
		if (drawCursor) {
			g.setColor(Color.BLUE);
			final int x1 = (w < 0) ? x + w : x;
			final int y1 = (h < 0) ? y + h : y;
			g.drawRect(
				x1 * GridPad.TILE_WIDTH  * GridPad.SCALE,
				y1 * GridPad.TILE_HEIGHT * GridPad.SCALE,
				Math.abs(w) * GridPad.TILE_WIDTH  * GridPad.SCALE,
				Math.abs(h) * GridPad.TILE_HEIGHT * GridPad.SCALE
			);
		}
	}

	public void mouseClicked(MouseEvent e) {
		if (draw) {
			dragButton = e.getButton();
			draw();
		}
		else {
			w = 1;
			h = 1;
			repaint();
		}
	}

	public void mouseDragged(MouseEvent e) {
		if (draw) {
			mouseMoved(e);
			draw();
		}
		else {
			int nx = e.getX() / (GridPad.TILE_WIDTH  * GridPad.SCALE);
			int ny = e.getY() / (GridPad.TILE_HEIGHT * GridPad.SCALE);
			nx = Math.min(grid[0].length-1, Math.max(0, nx));
			ny = Math.min(grid.length-1,    Math.max(0, ny));

			if (dragButton == MouseEvent.BUTTON1) {
				//w = Math.max(1, nx - x + 1);
				//h = Math.max(1, ny - y + 1);
				w = (nx - x) + 1;
				h = (ny - y) + 1;
			}
			else {
				x = nx;
				y = ny;
			}
			repaint();
		}
	}

	private void draw() {
		int tile = palette.getSelected();
		if (dragButton != MouseEvent.BUTTON1) {
			tile = -1;
		}
		if (grid[y][x] == tile) { return; }
		Edit change = new Edit(
			x, y, grid,
			new int[][] {{tile}}
		);
		change.apply();
		redo.clear();
		undo.push(change);
		repaint();
	}

	public void mouseMoved(MouseEvent e) {
		int nx = e.getX() / (GridPad.TILE_WIDTH  * GridPad.SCALE);
		int ny = e.getY() / (GridPad.TILE_HEIGHT * GridPad.SCALE);
		nx = Math.min(grid[0].length-1, Math.max(0, nx));
		ny = Math.min(grid.length-1,    Math.max(0, ny));

		if ((nx != x || ny != y)&&(w == 1)&&(h == 1)) {
			x = nx;
			y = ny;
			updateStatus();
			repaint();
		}
	}

	public void mouseEntered(MouseEvent e)  { drawCursor = true; repaint(); }
	public void mouseExited(MouseEvent e)   { drawCursor = false; repaint(); }
	public void mousePressed(MouseEvent e)  { dragButton = e.getButton(); }
	public void mouseReleased(MouseEvent e) { dragButton = MouseEvent.NOBUTTON; }
	public void keyPressed(KeyEvent e) {}

	public void keyReleased(KeyEvent e) {
		if (e.getKeyCode() == KeyEvent.VK_ALT) {
			draw = !draw;
			w = 1;
			h = 1;
		}
		else if (e.getKeyCode() == KeyEvent.VK_DELETE) {
			int[][] delta = new int[h][w];
			for(int a = 0; a < h; a++) {
				for(int b = 0; b < w; b++) {
					delta[a][b] = -1;
				}
			}
			Edit change = new Edit(x, y, grid, delta);
			change.apply();
			redo.clear();
			undo.push(change);
		}
		updateStatus();
		repaint();
	}

	public void keyTyped(KeyEvent e) {
		final int x1 = (w < 0) ? x + w : x;
		final int y1 = (h < 0) ? y + h : y;
		final int aw = Math.abs(w);
		final int ah = Math.abs(h);

		if (e.getKeyChar() == CONTROL_C) {
			clipBoard = new int[ah][aw];
			for(int a = 0; a < ah; a++) {
				for(int b = 0; b < aw; b++) {
					clipBoard[a][b] = grid[y1+a][x1+b];
				}
			}
		}
		else if (e.getKeyChar() == CONTROL_V) {
			if (clipBoard == null) { return; }
			int[][] delta = new int[clipBoard.length][clipBoard[0].length];
			for(int a = 0; a < clipBoard.length; a++) {
				for(int b = 0; b < clipBoard[0].length; b++) {
					delta[a][b] = clipBoard[a][b];
				}
			}
			Edit change = new Edit(x, y, grid, delta);
			change.apply();
			redo.clear();
			undo.push(change);
		}
		else if (e.getKeyChar() == CONTROL_F) {
			int[][] delta = new int[ah][aw];
			for(int a = 0; a < ah; a++) {
				for(int b = 0; b < aw; b++) {
					delta[a][b] = palette.getSelected();
				}
			}
			Edit change = new Edit(x1, y1, grid, delta);
			change.apply();
			redo.clear();
			undo.push(change);
		}
		else if (e.getKeyChar() == CONTROL_R) {
			if (redo.size() < 1) { return; }
			Edit change = redo.pop();
			change.apply();
			undo.push(change);
		}
		else if (e.getKeyChar() == CONTROL_Z) {
			if (undo.size() < 1) { return; }
			Edit change = undo.pop();
			change.undo();
			redo.push(change);
		}
		else if (e.getKeyChar() == CONTROL_O) {
			GridPad.save(grid);
			status.setText("Saved to clipboard.");
			status.repaint();
		}
		else if (e.getKeyChar() == CONTROL_L) {
			int[][] newGrid = GridPad.load();
			if (newGrid == null) {
				status.setText("Failed to load from clipboard.");
			}
			else {
				status.setText("Loaded from clipboard.");
				setGrid(newGrid);
			}
			status.repaint();
		}
		else if (e.getKeyChar() == ',') { // shrink horizontally
			if (grid[0].length < 81) { return; }
			int[][] newgrid = new int[grid.length][grid[0].length - 40];
			for(int a = 0; a < newgrid.length; a++) {
				for(int b = 0; b < newgrid[0].length; b++) {
					newgrid[a][b] = grid[a][b];
				}
			}
			setGrid(newgrid);
		}
		else if (e.getKeyChar() == '.') { // grow horizontally
			int[][] newgrid = new int[grid.length][grid[0].length + 40];
			for(int a = 0; a < newgrid.length; a++) {
				for(int b = 0; b < newgrid[0].length; b++) {
					newgrid[a][b] = (b < grid[0].length) ? grid[a][b] : -1;
				}
			}
			setGrid(newgrid);
		}
		else if (e.getKeyChar() == '<') { // shrink vertically
			if (grid.length < 61) { return; }
			int[][] newgrid = new int[grid.length - 30][grid[0].length];
			for(int a = 0; a < newgrid.length; a++) {
				for(int b = 0; b < newgrid[0].length; b++) {
					newgrid[a][b] = grid[a][b];
				}
			}
			setGrid(newgrid);
		}
		else if (e.getKeyChar() == '>') {
			int[][] newgrid = new int[grid.length + 30][grid[0].length];
			for(int a = 0; a < newgrid.length; a++) {
				for(int b = 0; b < newgrid[0].length; b++) {
					newgrid[a][b] = (a < grid.length) ? grid[a][b] : -1;
				}
			}
			setGrid(newgrid);
		}
		repaint();
	}

	private void updateStatus() {
		status.setText(String.format("tile: %3d  %2dx%2d  %s",
			palette.getSelected(),
			x,
			y,
			draw ? "draw" : "select"
		));
		status.repaint();
	}
}