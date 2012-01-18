import java.io.*;
import java.util.*;
import java.util.List;
import java.awt.*;
import java.awt.image.*;
import javax.imageio.*;

public class Slice {
	
	public static void main(String[] args) throws Exception {
	
		Image i = ImageIO.read(new File(args[0]));
		int w = i.getWidth(null);
		int h = i.getHeight(null);
		int a[] = new int[w * h];
		new PixelGrabber(i,0,0,w,h,a,0,w).grabPixels();

		Map<Integer, List<Integer>> indexToTile = new HashMap<Integer, List<Integer>>();
		Map<List<Integer>, Integer> tileToIndex = new HashMap<List<Integer>, Integer>();
		int[][] grid = new int[h/8][w/8];

		// scan and coalesce tiles
		final int maxTile = (w/8) * (h/8);
		int tileIndex = 0;
		for(int srcTile = 0; srcTile < maxTile; srcTile++) {
			List<Integer> tile = new ArrayList<Integer>(64);
			for(int pixel = 0; pixel < 64; pixel++) {
				tile.add(a[
					(((srcTile % (w/8)) * 8) + (pixel % 8)) +
					(((srcTile / (w/8)) * 8) + (pixel / 8)) * w
				]);
			}
			if (!tileToIndex.containsKey(tile)) {
				tileToIndex.put(tile, tileIndex);
				indexToTile.put(tileIndex++, tile);
			}
			grid[srcTile / (w/8)][srcTile % (w/8)] = tileToIndex.get(tile);
		}

		// write out tileset
		final int tw = 128;
		final int th = (int)Math.ceil(tileToIndex.size() / 16.0) * 8;
		BufferedImage o = new BufferedImage(tw, th, BufferedImage.TYPE_INT_ARGB);
		Graphics g = o.getGraphics();
		for(int dstTile = 0; dstTile < tileIndex; dstTile++) {
			for(int pixel = 0; pixel < 64; pixel++) {
				int color = indexToTile.get(dstTile).get(pixel);
				if ((color & 0xFF000000) == 0) { continue; }
				g.setColor(new Color(color));
				g.fillRect(
					((dstTile % 16) * 8) + (pixel % 8),
					((dstTile / 16) * 8) + (pixel / 8),
					1, 1
				);
			}
		}
		ImageIO.write(o, "PNG", new File("tiles.png"));

		// write out tilemap
		PrintWriter out = new PrintWriter(new File("grid.txt"));
		for(int[] row : grid) {
			out.print("\t");
			for(int tile : row) { out.format("%3d ", tile); }
			out.println();
		}
		out.close();

		System.out.format("Solved with %d tiles.%n", tileToIndex.size());
	}
}