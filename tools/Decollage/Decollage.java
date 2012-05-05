import java.io.*;
import java.awt.*;
import java.awt.event.*;
import javax.imageio.*;
import javax.swing.*;
import javax.swing.border.*;
import javax.swing.event.*;

public class Decollage extends JPanel {

	private static void label(JComponent c, String label) {
		TitledBorder border = new TitledBorder(
			new EmptyBorder(0,0,10,0),
			label
		);
		c.setBorder(border);
	}

	private static JComponent labelField(JComponent c, String label) {
		JPanel ret = new JPanel(new BorderLayout());
		ret.add(new JLabel(label), BorderLayout.WEST);
		ret.add(c, BorderLayout.CENTER);
		return ret;
	}

	private Integer value(JTextField f) {
		try {
			int n = Integer.parseInt(f.getText());
			if (n < 0) { throw new NumberFormatException(); }
			return n;
		}
		catch(NumberFormatException n) {
			f.setText("0");
			return null;
		}
	}

	class TilePanel extends JPanel implements MouseListener {
		public TilePanel() {
			setPreferredSize(new Dimension(512, 0));
			addMouseListener(this);
		}

		public void mouseClicked(MouseEvent m) {
			try {
				int start      = (Integer)base.getValue();
				int tileWidth  = sizeWidth.getValue();
				int tileHeight = sizeHeight.getValue();

				int pw = 128 / tileWidth;
				int x  = m.getX() / 4 / tileWidth;
				int y  = m.getY() / 4 / tileHeight;

				selected = x + (pw * y);
				tileID.setText(""+selected);

				int i = (selected * tileWidth * tileHeight) + start;
				tileAddr.setText(""+i);

				repaint();
			}
			catch(Exception e) {
				System.out.println("Exception in clicked.");
			}
		}

		public void mousePressed(MouseEvent m) {}
		public void mouseReleased(MouseEvent m) {}
		public void mouseEntered(MouseEvent m) {}
		public void mouseExited(MouseEvent m) {}

		public void paint(Graphics g) {
			g.setColor(Color.RED);
			g.fillRect(0, 0, getWidth(), getHeight());

			try {
				int tileWidth  = sizeWidth.getValue();
				int tileHeight = sizeHeight.getValue();
				Integer start  = (Integer)base.getValue();
				Integer count  = value(extent);
				if (start == null || count == null || rom == null) { return; }

				for(int x = 0; x < count; x++) {
					int i = ( x      * tileWidth * tileHeight) + start;
					int j = ((x + 1) * tileWidth * tileHeight) + start;
					if (j >= rom.size()) { break; }
					drawTile(
						g,
						i,
						tileWidth,
						tileHeight,
						(x % (128 / tileWidth)) * tileWidth,
						(x / (128 / tileWidth)) * tileHeight
					);
					if (x == selected) {
						g.setColor(Color.RED);
						g.drawRect(
							(x % (128 / tileWidth)) * tileWidth  * 4,
							(x / (128 / tileWidth)) * tileHeight * 4,
							tileWidth  * 4 - 1,
							tileHeight * 4 - 1
						);
					}
				}
			}
			catch(Exception e) {
				e.printStackTrace();
			}
		}

		private void drawTile(Graphics g, int i, int w, int h, int x, int y) {
			for(int a = 0; a < h; a++) {
				for(int b = 0; b < w; b++) {
					int color = rom.get(i++);
					if ((color & 0xFF000000) != 0xFF000000) { color = 0xFF00FF00; }
					g.setColor(new Color(
						((color >> 16) & 0xFF),
						((color >>  8) & 0xFF),
						( color        & 0xFF)
					));
					g.fillRect(
						(x + b) * 4,
						(y + a) * 4,
						4,
						4
					);
				}
			}
		}
	}
	private int   selected = 0;
	private final MakoRom   rom;
	private final TilePanel panel;

	// base
	private final JButton  baseGT  = new JButton("GT");
	private final JButton  baseST  = new JButton("ST");
	private final JButton  baseSel = new JButton("Selection");
	private final JSpinner base    = new JSpinner();
	{
		baseGT.addActionListener(new ActionListener() {
			public void actionPerformed(ActionEvent e) {
				base.setValue(rom.get(MakoConstants.GT));
				sizeWidth.setValue(8);
				sizeHeight.setValue(8);
				panel.repaint();
			}
		});

		baseST.addActionListener(new ActionListener() {
			public void actionPerformed(ActionEvent e) {
				base.setValue(rom.get(MakoConstants.ST));
				panel.repaint();
			}
		});

		baseSel.addActionListener(new ActionListener() {
			public void actionPerformed(ActionEvent e) {
				base.setValue(value(tileAddr));
				panel.repaint();
			}
		});

		((SpinnerNumberModel)base.getModel()).setMinimum(0);
		base.addChangeListener(new ChangeListener() {
			public void stateChanged(ChangeEvent e) {
				panel.repaint();
			}
		});
	}

	// size
	private final JSlider    sizeWidth       = new JSlider(8, 64, 8);
	private final JSlider    sizeHeight      = new JSlider(8, 64, 8);
	private final JTextField sizeConstant    = new JTextField("1");
	{
		sizeWidth.setSnapToTicks(true);
		sizeWidth.setMinorTickSpacing(8);
		sizeWidth.setMajorTickSpacing(8);
		sizeWidth.setPaintLabels(true);
		sizeWidth.setPaintTicks(true);
		label(sizeWidth, "Width");

		sizeHeight.setSnapToTicks(true);
		sizeHeight.setMinorTickSpacing(8);
		sizeHeight.setMajorTickSpacing(8);
		sizeHeight.setPaintLabels(true);
		sizeHeight.setPaintTicks(true);
		label(sizeHeight, "Height");

		sizeWidth.addChangeListener(new SizeListener());
		sizeHeight.addChangeListener(new SizeListener());
		sizeConstant.setEditable(false);
	}

	private class SizeListener implements ChangeListener {
		public void stateChanged(ChangeEvent e) {
			int val = ((JSlider)e.getSource()).getValue();
			if (val / 8 * 8 != val) { return; }
			
			int x = (sizeWidth.getValue()  / 8) - 1;
			int y = (sizeHeight.getValue() / 8) - 1;
			int v = ((x << 8) & 0x0F00) | ((y << 12) & 0xF000) | 1;
			sizeConstant.setText(String.format("%d", v));
			panel.repaint();
		}
	}

	// extent
	private final JButton    extentCrop = new JButton("Crop");
	private final JButton    extentAll  = new JButton("All");
	private final JTextField extent     = new JTextField("8");
	{
		extentCrop.addActionListener(new ActionListener() {
			public void actionPerformed(ActionEvent e) {
				extent.setText(""+(selected+1));
				panel.repaint();
			}
		});

		extentAll.addActionListener(new ActionListener() {
			public void actionPerformed(ActionEvent e) {
				extent.setText(""+Integer.MAX_VALUE);
				panel.repaint();
			}
		});

		extent.addActionListener(new ActionListener() {
			public void actionPerformed(ActionEvent e) {
				if (value(extent) != null) { panel.repaint(); }
			}
		});
	}

	// tools
	private final JTextField tileID   = new JTextField();
	private final JTextField tileAddr = new JTextField();
	{
		tileID.setEditable(false);
		tileAddr.setEditable(false);
	}

	// window
	private final JFrame window = new JFrame("D\u00E9collage");
	{
		JPanel basePanel = new JPanel();
		basePanel.setLayout(new BoxLayout(basePanel, BoxLayout.Y_AXIS));
		basePanel.setBorder(new TitledBorder(new EtchedBorder(), "Base"));
		JPanel baseButtons = new JPanel(new FlowLayout());
		baseButtons.add(baseGT);
		baseButtons.add(baseST);
		baseButtons.add(baseSel);
		basePanel.add(baseButtons);
		basePanel.add(labelField(base, "Address: "));

		JPanel sizePanel = new JPanel();
		sizePanel.setLayout(new BoxLayout(sizePanel, BoxLayout.Y_AXIS));
		sizePanel.setBorder(new TitledBorder(new EtchedBorder(), "Size"));
		sizePanel.add(sizeWidth);
		sizePanel.add(sizeHeight);
		sizePanel.add(labelField(sizeConstant, "Size Constant: "));

		JPanel extentPanel = new JPanel();
		extentPanel.setLayout(new BoxLayout(extentPanel, BoxLayout.Y_AXIS));
		extentPanel.setBorder(new TitledBorder(new EtchedBorder(), "Extent"));
		JPanel extentButtons = new JPanel(new FlowLayout());
		extentButtons.add(extentCrop);
		extentButtons.add(extentAll);
		extentPanel.add(extentButtons);
		extentPanel.add(labelField(extent, "Tiles: "));

		JPanel toolsPanel = new JPanel();
		toolsPanel.setLayout(new BoxLayout(toolsPanel, BoxLayout.Y_AXIS));
		toolsPanel.setBorder(new TitledBorder(new EtchedBorder(), "Tools"));
		JPanel toolsFields = new JPanel(new GridLayout(2,2));
		toolsFields.add(new JLabel("Tile ID:"));
		toolsFields.add(tileID);
		toolsFields.add(new JLabel("Tile Address:"));
		toolsFields.add(tileAddr);
		toolsPanel.add(toolsFields);

		JPanel tools = new JPanel();
		tools.setLayout(new BoxLayout(tools, BoxLayout.Y_AXIS));
		tools.add(basePanel);
		tools.add(sizePanel);
		tools.add(extentPanel);
		tools.add(toolsPanel);
		JPanel toolbar = new JPanel(new BorderLayout());
		toolbar.add(tools, BorderLayout.NORTH);

		panel = new TilePanel();

		window.setLayout(new BorderLayout());
		window.add(toolbar, BorderLayout.EAST);
		window.add(panel, BorderLayout.CENTER);

		window.setDefaultCloseOperation(JFrame.EXIT_ON_CLOSE);
		window.pack();
		window.setVisible(true);
	}

	public Decollage(String s) {
		rom = new MakoRom(s);
		panel.repaint();
	}

	public static void main(String[] args) {
		new Decollage(args[0]);
	}
}