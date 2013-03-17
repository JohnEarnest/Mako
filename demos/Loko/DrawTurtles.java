import java.io.*;
import java.awt.*;
import java.awt.image.*;
import javax.imageio.*;

public class DrawTurtles {
	public static void main(String[] args) throws Exception {
		BufferedImage i = new BufferedImage(32, 32 * 32, BufferedImage.TYPE_INT_ARGB);
		Graphics g = i.getGraphics();
		g.setColor(new Color(0, 255, 0));

		for(int x = 0; x < 32; x++) {
			Graphics2D c = (Graphics2D)g.create();
			c.translate(15, 15 + (32*x));
			c.rotate(x * 3.0 * (Math.PI/180));
			c.drawLine( 0,  0, -6, -6);
			c.drawLine( 0,  0, -6,  6);
			c.drawLine(-6, -6, -6,  6);
		}

		ImageIO.write(i, "PNG", new File("turtles.png"));
	}
}