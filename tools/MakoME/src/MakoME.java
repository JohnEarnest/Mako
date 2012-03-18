import java.io.*;
import javax.microedition.midlet.*;
import javax.microedition.lcdui.*;
import javax.microedition.lcdui.game.*;

public class MakoME extends MIDlet implements CommandListener {

    private Display display;
    private MakoCanvas canvas;

    public void startApp() {
        display = Display.getDisplay(this);
        canvas = new MakoCanvas();
        display.setCurrent(canvas);
    }

    public void pauseApp() {

    }

    public void destroyApp(boolean unconditional) {
        if (canvas != null) { canvas.stop(); }
    }

    public void commandAction(Command c, Displayable s) {

    }

}

class MakoCanvas extends GameCanvas implements Runnable, MakoConstants {

    private volatile Thread thread = null;
    private boolean run = true;
    private Graphics g;
    private MakoVM vm;

    public MakoCanvas() {
        super(true);
        setFullScreenMode(true);
        g = getGraphics();
        DataInputStream in = new DataInputStream(
            MakoME.class.getResourceAsStream("Data.rom")
        );
        int x = 0;
        try {
            int[] rom = new int[in.available() / 4];
            for(; x < rom.length; x++) {
                rom[x] = in.readInt();
            }
            vm = new MakoVM(rom);
        }
        catch(IOException ioe) {
            System.out.println(x);
            ioe.printStackTrace();
        }
    }

    public void stop() {
        run = false;
    }

    public void run() {
	boolean sflag = true;
        while(run) {
            vm.run();
	    if (sflag) { vm.sync(g); } // drop every other frame
            sflag = !sflag;
            flushGraphics();

            int keys = getKeyStates();
            vm.keys = 0;
            if ((keys & DOWN_PRESSED)  != 0) { vm.keys |= KEY_DN; }
            if ((keys & UP_PRESSED)    != 0) { vm.keys |= KEY_UP; }
            if ((keys & LEFT_PRESSED)  != 0) { vm.keys |= KEY_LF; }
            if ((keys & RIGHT_PRESSED) != 0) { vm.keys |= KEY_RT; }
            if ((keys & FIRE_PRESSED)  != 0) { vm.keys |= KEY_A;  }
        }
    }

    protected void hideNotify() {
        thread = null;
    }

    protected void showNotify() {
        thread = new Thread(this);
        thread.start();
    }
}