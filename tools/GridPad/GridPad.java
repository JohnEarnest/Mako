import java.util.*;
import java.util.List;
import java.awt.*;
import javax.swing.*;
import java.awt.datatransfer.*;

public class GridPad extends JFrame {

	public static final int TILE_WIDTH  = 8;
	public static final int TILE_HEIGHT = 8;
	public static final int SCALE = 3;

	private final JLabel  status  = new JLabel("ready.");
	private final Palette palette = new Palette(this);
	private final Editor  editor  = new Editor(this, palette);

	public static void main(String[] args) {
		new GridPad();
	}

	public GridPad() {
		int[][] grid = new int[31][41];
		for(int a = 0; a < grid.length; a++) {
			for(int b = 0; b < grid[0].length; b++) {
				grid[a][b] = -1;
			}
		}

		addKeyListener(editor);
		addKeyListener(palette);
		editor.setGrid(grid);

		JPanel tools = new JPanel();
		tools.setLayout(new GridLayout(3,1));

		JPanel toolbar = new JPanel();
		toolbar.setLayout(new BorderLayout());
		toolbar.add(palette, BorderLayout.CENTER);
		toolbar.add(tools,   BorderLayout.SOUTH);

		JScrollPane scroll = new JScrollPane(
			editor,
			JScrollPane.VERTICAL_SCROLLBAR_ALWAYS,
			JScrollPane.HORIZONTAL_SCROLLBAR_ALWAYS
		);
		scroll.getViewport().setPreferredSize(new Dimension(
			41 * TILE_WIDTH  * SCALE,
			31 * TILE_HEIGHT * SCALE	
		));
		scroll.getHorizontalScrollBar().setBlockIncrement(TILE_WIDTH * SCALE);
		scroll.getVerticalScrollBar().setBlockIncrement(TILE_HEIGHT * SCALE);

		JPanel mainPanel = new JPanel();
		mainPanel.setLayout(new BorderLayout());
		mainPanel.add(scroll,  BorderLayout.CENTER);
		mainPanel.add(toolbar, BorderLayout.EAST);

		setLayout(new BorderLayout());
		add(mainPanel, BorderLayout.CENTER);
		add(status,    BorderLayout.SOUTH);

		pack();
		setTitle("GridPad");
		setDefaultCloseOperation(EXIT_ON_CLOSE);
		setResizable(false);
		setVisible(true);
	}

	public void updateStatus() {
		int[][] selected = palette.getSelected();
		if (selected == null || selected.length < 1 || selected[0].length < 1) { return; }
		status.setText(String.format("tile: %3d   %02dx%02d (%02dx%02d) %s",
			selected[0][0],
			editor.x,
			editor.y,
			editor.x * TILE_WIDTH,
			editor.y * TILE_HEIGHT,
			editor.draw ? "draw" : "select"
		));
		status.repaint();
	}

	// load from the clipboard, splitting rows apart
	// by newlines and columns apart by whitespace.
	public static int[][] load() {
		try {
			Clipboard systemClipboard = Toolkit.getDefaultToolkit().getSystemClipboard();
			Transferable contents = systemClipboard.getContents(null);
			String result = (String)contents.getTransferData(DataFlavor.stringFlavor);
			Scanner in = new Scanner(result);
			List<List<Integer>> data = new ArrayList<List<Integer>>();
			while(in.hasNextLine()) {
				Scanner line = new Scanner(in.nextLine());
				List<Integer> a = new ArrayList<Integer>();
				while(line.hasNextInt()) {
					a.add(line.nextInt());
				}
				data.add(a);
			}
			int[][] ret = new int[data.size()][data.get(0).size()];
			int r = 0;
			for(List<Integer> row : data) {
				int c = 0;
				for(Integer i : row) {
					ret[r][c] = i;
					c++;
				}
				r++;
			}
			return ret;
		}
		catch(Exception e) {
			return null;
		}
	}
	
	// save to the clipboard:
	public static void save(int[][] grid) {
		String s = "";
		for(int a = 0; a < grid.length; a++) {
			s += '\t';
			for(int b = 0; b < grid[0].length; b++) {
				s += String.format("%2d ", grid[a][b]);
			}
			s += '\n';
		}
		Clipboard systemClipboard = Toolkit.getDefaultToolkit().getSystemClipboard();
		Transferable transferableText = new StringSelection(s);
		systemClipboard.setContents(transferableText, null);
	}
}