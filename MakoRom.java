import java.io.*;
import java.util.*;
import java.awt.Image;
import java.awt.image.PixelGrabber;
import javax.imageio.ImageIO;

public class MakoRom implements MakoConstants {

	private final List<Integer> data  = new ArrayList<Integer>();
	private final List<Type>    types = new ArrayList<Type>();
	private final Map<String, Integer> labels = new HashMap<String, Integer>();

	public enum Type { Code, Data, Array, String, Image, Unknown };

	public MakoRom() {
		
	}

	public MakoRom(String filename) {
		label("base rom", 0);
		try {
			Scanner in = new Scanner(new File(filename));
			while(in.hasNextInt()) { add(in.nextInt(), Type.Array); }
			in.close();
		}
		catch(IOException e) {
			throw new Error("Unable to load base memory image!");
		}
	}

	public MakoRom copy() {
		MakoRom ret = new MakoRom();
		ret.data.addAll(data);
		ret.types.addAll(types);
		ret.labels.putAll(labels);
		return ret;
	}

	public void label(String name, int value) {
		labels.put(name, value);
	}

	public int getAddress(String name) {
		return labels.get(name);
	}

	public void set(int index, int value) {
		data.set(index, value);
	}

	public void set(int index, int value, Type type) {
		data.set(index, value);
		types.set(index, type);
	}

	public void add(int value, Type type) {
		data.add(value);
		types.add(type);
	}

	public int size() {
		return data.size();
	}

	public int[] toArray() {
		int[] ret = new int[size()];
		for(int x = 0; x < size(); x++) {
			ret[x] = data.get(x);
		}
		return ret;
	}

	private int paramOp(int a, int b) {
		add(a, Type.Code);
		add(b, Type.Code);
		return size() - 1;
	}

	public void addString(String s) {
		for(char c : s.toCharArray()) {
			add((int)c, Type.String);
		}
		add(0, Type.String);
	}

	public void addImage(String filename, int tileWidth, int tileHeight) {
		try {
			addImage(ImageIO.read(new File(filename)), tileWidth, tileHeight);
		}
		catch(IOException e) {
			throw new Error("Unable to load image '"+filename+"'.");
		}
	}

	public void addImage(Image i, int tileWidth, int tileHeight) {
		// unpack the pixels of the image
		final int w = i.getWidth(null);
		final int h = i.getHeight(null);
		final int a[] = new int[w * h];
		final PixelGrabber pg = new PixelGrabber(i,0,0,w,h,a,0,w);
		try { pg.grabPixels(); }
		catch(InterruptedException ie) { ie.printStackTrace(); }
		
		// repack pixels as tiles
		for(int ty = 0; ty < h/tileHeight; ty++) {
			for(int tx = 0; tx < w/tileWidth; tx++) {
				for(int y = 0; y < tileHeight; y++) {
					for(int x = 0; x < tileWidth; x++) {
						add(a[((tx*tileWidth) + x) + ((ty*tileHeight) + y)*w], Type.Array); 
					}
				}
			}
		}
	}

	public int addConst(int value)  { return paramOp(OP_CONST,  value); }
	public int addCall(int value)   { return paramOp(OP_CALL,   value); }
	public int addJump(int value)   { return paramOp(OP_JUMP,   value); }
	public int addJumpZ(int value)  { return paramOp(OP_JUMPZ,  value); }
	public int addJumpIf(int value) { return paramOp(OP_JUMPIF, value); }
	public int addNext(int value)   { return paramOp(OP_NEXT,   value); }

	public void addReturn() { add(OP_RETURN, Type.Code); }
	public void addLoad() { add(OP_LOAD, Type.Code); }
	public void addStor() { add(OP_STOR, Type.Code); }
	public void addDrop() { add(OP_DROP, Type.Code); }
	public void addSwap() { add(OP_SWAP, Type.Code); }
	public void addDup()  { add(OP_DUP,  Type.Code); }
	public void addOver() { add(OP_OVER, Type.Code); }
	public void addStr()  { add(OP_STR,  Type.Code); }
	public void addRts()  { add(OP_RTS,  Type.Code); }
	public void addAdd()  { add(OP_ADD,  Type.Code); }
	public void addSub()  { add(OP_SUB,  Type.Code); }
	public void addMul()  { add(OP_MUL,  Type.Code); }
	public void addDiv()  { add(OP_DIV,  Type.Code); }
	public void addMod()  { add(OP_MOD,  Type.Code); }
	public void addAnd()  { add(OP_AND,  Type.Code); }
	public void addOr()   { add(OP_OR,   Type.Code); }
	public void addXor()  { add(OP_XOR,  Type.Code); }
	public void addNot()  { add(OP_NOT,  Type.Code); }
	public void addSgt()  { add(OP_SGT,  Type.Code); }
	public void addSlt()  { add(OP_SLT,  Type.Code); }
	public void addSync() { add(OP_SYNC, Type.Code); }

	private static final Map<Integer, String> mnemonics = new HashMap<Integer, String>();
	{
		mnemonics.put(OP_CONST,  "CONST");
		mnemonics.put(OP_CALL,   "CALL");
		mnemonics.put(OP_JUMP,   "JUMP");
		mnemonics.put(OP_JUMPZ,  "JUMPZ");
		mnemonics.put(OP_JUMPIF, "JUMPN");
		mnemonics.put(OP_LOAD,   "LOAD");
		mnemonics.put(OP_STOR,   "STOR");
		mnemonics.put(OP_RETURN, "RET");
		mnemonics.put(OP_DROP,   "DROP");
		mnemonics.put(OP_SWAP,   "SWAP");
		mnemonics.put(OP_DUP,    "DUP");
		mnemonics.put(OP_OVER,   "OVER");
		mnemonics.put(OP_STR,    "STR");
		mnemonics.put(OP_RTS,    "RTS");
		mnemonics.put(OP_ADD,    "ADD");
		mnemonics.put(OP_SUB,    "SUB");
		mnemonics.put(OP_MUL,    "MUL");
		mnemonics.put(OP_DIV,    "DIV");
		mnemonics.put(OP_MOD,    "MOD");
		mnemonics.put(OP_AND,    "AND");
		mnemonics.put(OP_OR,     "OR");
		mnemonics.put(OP_XOR,    "XOR");
		mnemonics.put(OP_NOT,    "NOT");
		mnemonics.put(OP_SGT,    "SGT");
		mnemonics.put(OP_SLT,    "SLT");
		mnemonics.put(OP_SYNC,   "SYNC");
		mnemonics.put(OP_NEXT,   "NEXT");
	}

	private static final Set<Integer> paramOps = new HashSet<Integer>();
	{
		paramOps.add(OP_CONST);
		paramOps.add(OP_CALL);
		paramOps.add(OP_JUMP);
		paramOps.add(OP_JUMPZ);
		paramOps.add(OP_JUMPIF);
		paramOps.add(OP_NEXT);
	}

	public String getLabel(int address) {
		for(Map.Entry entry : labels.entrySet()) {
			if (entry.getValue().equals(address)) {
				return String.format("(%s)", entry.getKey());
			}
		}
		return "";
	}

	public void disassemble(PrintStream out) {
		disassemble(0, size()-1, out);
	}

	public void disassemble(int first, int last, PrintStream out) {
		int prevOp = -1;
		for(int index = first; index <= last; index++) {
			Type t = types.get(index);
			out.format("%05d: %-16s", index, getLabel(index));
			int op = data.get(index);
			if      (t == Type.Array)  { index = disassembleArray(index, out); }
			else if (t == Type.String) { index = disassembleString(index, out); }
			else if (t == Type.Code)   { index = disassembleCode(index, prevOp, out); }
			else {
				out.format("%d%n", data.get(index));
			}
			prevOp = (t == Type.Code) ? op : -1;
		}
		out.format("%n%d words, %.3f kb.%n",
			size(),
			((double)size())/256
		);
	}

	private int disassembleArray(int index, PrintStream out) {
		int start = index;
		while(index < size() && types.get(index) == Type.Array) {
			if (index != start && !getLabel(index).equals("")) { break; }
			index++;
		}
		if (index - start == 1) {
			out.format("%d%n", data.get(start));
		}
		else {
			out.format("<<< %d words >>>%n", (index - start));
		}
		return index - 1;
	}

	private int disassembleString(int index, PrintStream out) {
		String s = "";
		while(data.get(index) != 0) {
			s += (char)data.get(index).intValue();
			index++;
		}
		s = s.replace("\n", "\\n");
		s = s.replace("\r", "\\r");
		s = s.replace("\t", "\\t");
		out.format("\"%s\"%n", s);
		return index;
	}

	private int disassembleCode(int index, int prevOp, PrintStream out) {
		int op = data.get(index);
		if (op == OP_CALL) {
			out.format("%5s %d %s%n",
				mnemonics.get(op),
				data.get(index+1),
				getLabel(data.get(index+1))
			);
			index++;
		}
		else if (paramOps.contains(op)) {
			out.format("%5s %d%n",
				mnemonics.get(op),
				data.get(index+1)
			);
			index++;
		}
		else if ((op == OP_LOAD || op == OP_STOR) && prevOp == OP_CONST) {
			out.format("%5s %s%n",
				mnemonics.get(op),
				getLabel(data.get(index-1))
			);
		}
		else {
			out.format("%5s%n", mnemonics.get(op) );
		}
		return index;
	}
}