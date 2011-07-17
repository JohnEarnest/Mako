import java.util.*;
import java.io.*;
import java.awt.Toolkit;
import java.awt.Image;
import java.awt.image.*;

public class Maker implements MakoConstants {

	private Map<String, Integer> dictionary = new TreeMap<String, Integer>();
	private Map<String, Integer> variables = new TreeMap<String, Integer>();
	private Map<String, Integer> constants = new TreeMap<String, Integer>();
	private Map<String, List<Integer>> prototypes = new HashMap<String, List<Integer>>();
	private Set<String> imported = new HashSet<String>();
	private List<Integer> rom = new ArrayList<Integer>();
	private List<Integer> tag = new ArrayList<Integer>();
	private boolean compiling = false;
	private Stack<Integer> branchStack = new Stack<Integer>();
	private Stack<Integer> loopStack   = new Stack<Integer>();
	private Queue<Integer> breaks      = new LinkedList<Integer>();

	private String path;

	private static final int TAG_CODE   = 1;
	private static final int TAG_DATA   = 2;
	private static final int TAG_ARRAY  = 3;
	private static final int TAG_STRING = 4;

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

	public static void main(String[] args) {
		List<String> argList = new ArrayList<String>(Arrays.asList(args));
		boolean run = false;
		boolean standalone = false;
		if (argList.contains("--run")) {
			run = true;
			argList.remove("--run");
		}
		if (argList.contains("--standalone")) {
			standalone = true;
			argList.remove("--standalone");
		}
		Maker compiler = new Maker();
		int[] rom = compiler.compile(argList.get(0), standalone);
		compiler.disassemble();
		if (argList.size() > 1) {
			try {
				PrintWriter out = new PrintWriter(new File(argList.get(1)));
				for(int x : rom) { out.println(x); }
				out.close();
			}
			catch(IOException ioe) { ioe.printStackTrace(); }
		}
		if (run) { Mako.exec(rom); }
	}

	public int[] compile(String filename, boolean standalone) {
		dictionary.clear();
		variables.clear();
		rom.clear();
		tag.clear();
		compiling = false;
		branchStack.clear();
		loopStack.clear();
		breaks.clear();

		if (!standalone) {
			buildRegion("registers", RESERVED_HEADER);
		}
		
		Map<String, Integer> dict = standalone ? constants : variables;
		dict.put("PC", PC);
		dict.put("DP", DP);
		dict.put("RP", RP);
		dict.put("GP", GP);
		dict.put("GT", GT);
		dict.put("SP", SP);
		dict.put("ST", ST);
		dict.put("GS", GS);
		dict.put("SX", SX);
		dict.put("SY", SY);
		dict.put("CL", CL);
		dict.put("RN", RN);
		dict.put("KY", KY);
		dict.put("CO", CO); // character-out (debug)

		constants.put("key-up", KEY_UP);
		constants.put("key-rt", KEY_RT);
		constants.put("key-dn", KEY_DN);
		constants.put("key-lf", KEY_LF);
		constants.put("key-a",  KEY_A);
		constants.put("key-b",  KEY_B);

		constants.put("sprite-mirror-horiz", H_MIRROR_MASK);
		constants.put("sprite-mirror-vert",  V_MIRROR_MASK);

		constants.put("true", -1);
		constants.put("false", 0);

		// sprite size constants:
		for(int x = 0; x < 8; x++) {
			for(int y = 0; y < 8; y++) {
				int w = (x + 1) * 8;
				int h = (y + 1) * 8;
				int v = ((x << 8) & 0x0F00) | ((y << 12) & 0xF000) | 1;
				constants.put(String.format("%dx%d", w, h), v);
			}
		}

		constants.put("grid-skip", 0);
		constants.put("scroll-x", 0);
		constants.put("scroll-y", 0);
		constants.put("clear-color", 0xFF000000);
		try {
			path = new File(filename).getParent();
			compileFile(filename);
		}
		catch(FileNotFoundException e) {
			throw new Error(e.getMessage());
		}
		if (!dictionary.containsKey("main") && !standalone) {
			throw new Error("No entrypoint defined!");
		}
		if (prototypes.size() > 0) {
			for(String s : prototypes.keySet()) {
				System.out.println("unresolved prototype '"+s+"'");
			}
			throw new Error();
		}

		if (!standalone) {
			buildRegion("data-stack",     50);
			buildRegion("return-stack",   50);
			buildRegion("grid",         1271);
			buildRegion("grid-tiles",     64);
			buildRegion("sprites",      1024);
			buildRegion("sprite-tiles",   64);
			rom.set(PC, dictionary.get("main"));
			rom.set(DP, variables.get("data-stack"));
			rom.set(RP, variables.get("return-stack"));
			rom.set(GP, variables.get("grid"));
			if (variables.containsKey("grid-tiles")) {
				rom.set(GT, variables.get("grid-tiles"));
			}
			else {
				rom.set(GT, constants.get("grid-tiles"));
			}
			rom.set(SP, variables.get("sprites"));
			if (variables.containsKey("sprite-tiles")) {
				rom.set(ST, variables.get("sprite-tiles"));
			}
			else {
				rom.set(ST, constants.get("sprite-tiles"));
			}
			rom.set(GS, constants.get("grid-skip"));
			rom.set(SX, constants.get("scroll-x"));
			rom.set(SY, constants.get("scroll-y"));
			rom.set(CL, constants.get("clear-color"));
		}
		int[] ret = new int[rom.size()];
		for(int x = 0; x < rom.size(); x++) {
			ret[x] = rom.get(x);
		}
		return ret;
	}

	private void buildRegion(String name, int size) {
		if (!variables.containsKey(name) && !constants.containsKey(name)) {
			variables.put(name, rom.size());
			for(int x = 0; x < size; x++) { romAdd(0, TAG_ARRAY); }
		}
	}

	private void romAdd(int v, int t) {
		rom.add(v);
		tag.add(t);
	}

	private void compileFile(String filename) throws FileNotFoundException {
		if (imported.contains(filename)) { return; }
		imported.add(filename);
		Scanner in = new Scanner(new File(filename));
		String source = "";
		while(in.hasNextLine()) { source += in.nextLine() + '\n'; }
		Queue<Object> tokens = tokens(source);
		while(tokens.size() > 0) {
			Object token = tokens.remove();
			if (token instanceof Integer) {
				if (compiling) { romAdd(OP_CONST, TAG_CODE); }
				romAdd((Integer)token, TAG_ARRAY);
			}
			else {
				compileToken((String)token, tokens);
			}
		}
	}

	private void compileToken(String token, Queue<Object> tokens) throws FileNotFoundException {
		// defining words
		if (token.equals(":")) {
			compiling = true;
			String wordName = tokens.remove().toString();
			dictionary.put(wordName, rom.size());
			if (prototypes.containsKey(wordName)) {
				for(Integer a : prototypes.remove(wordName)) {
					rom.set(a, rom.size());
				}
			}
		}
		else if (token.equals(":vector")) {
			compiling = true;
			String wordName = tokens.remove().toString();
			dictionary.put(wordName, rom.size());
			if (prototypes.containsKey(wordName)) {
				for(Integer a : prototypes.remove(wordName)) {
					rom.set(a, rom.size());
				}
			}
			romAdd(OP_JUMP,      TAG_CODE);
			romAdd(rom.size()+1, TAG_CODE);
		}
		else if (token.equals(";")) {
			compiling = false;
			romAdd(OP_RETURN, TAG_CODE);
		}
		else if (token.equals(":var")) {
			variables.put(tokens.remove().toString(), rom.size());
			romAdd(0, TAG_DATA);
		}
		else if (token.equals(":array")) {
			String name = tokens.remove().toString();
			int count = (Integer)tokens.remove();
			int value = (Integer)tokens.remove();
			variables.put(name, rom.size());
			for(int x = 0; x < count; x++) { romAdd(value, TAG_ARRAY); }
		}
		else if (token.equals(":data")) {
			variables.put(tokens.remove().toString(), rom.size());
		}
		else if (token.equals(":string")) {
			variables.put(tokens.remove().toString(), rom.size());
			for(char c : tokens.remove().toString().toCharArray()) {
				romAdd((int)c, TAG_STRING);
			}
			romAdd(0, TAG_STRING);
		}
		else if (token.equals("string")) {
			romAdd(OP_JUMP, TAG_CODE);
			romAdd(-1,      TAG_CODE);
			int start = rom.size();
			for(char c : tokens.remove().toString().toCharArray()) {
				romAdd((int)c, TAG_STRING);
			}
			romAdd(0, TAG_STRING);
			rom.set(start-1, rom.size());
			romAdd(OP_CONST, TAG_CODE);
			romAdd(start,    TAG_CODE);
		}
		else if (token.equals(":const")) {
			String constName = tokens.remove().toString();
			Object constValue = tokens.remove();
			if (constValue instanceof Integer) {
				constants.put(constName, (Integer)constValue);
			}
			else if (constants.containsKey(constValue.toString())) {
				constants.put(constName, constants.get(constValue.toString()));
			}
			else if (variables.containsKey(constValue.toString())) {
				constants.put(constName, variables.get(constValue.toString()));
			}
			else if (dictionary.containsKey(constValue.toString())) {
				constants.put(constName, dictionary.get(constValue.toString()));
			}
		}
		else if (token.equals(":image")) {
			String imageName = tokens.remove().toString();
			String fileName = tokens.remove().toString();
			if (path != null) { fileName = path + File.separator + fileName; }
			variables.put(imageName, rom.size());
			int tileWidth  = (Integer)tokens.remove();
			int tileHeight = (Integer)tokens.remove();

			// load the image
			Toolkit toolkit = Toolkit.getDefaultToolkit();
			ClassLoader loader = Maker.class.getClassLoader();
			Image tiles = toolkit.getImage(loader.getResource(fileName));
			while (tiles.getWidth(null) < 0) {
				try {Thread.sleep(10);}
				catch(InterruptedException ie) { ie.printStackTrace(); }
			}

			// unpack the pixels of the image
			final int w = tiles.getWidth(null);
			final int h = tiles.getHeight(null);
			final int a[] = new int[w * h];
			final PixelGrabber pg = new PixelGrabber(tiles,0,0,w,h,a,0,w);
			try { pg.grabPixels(); }
			catch(InterruptedException ie) { ie.printStackTrace(); }
			
			// repack pixels as tiles
			for(int ty = 0; ty < h/tileHeight; ty++) {
				for(int tx = 0; tx < w/tileWidth; tx++) {

					for(int y = 0; y < tileHeight; y++) {
						for(int x = 0; x < tileWidth; x++) {
							romAdd(a[((tx*tileWidth) + x) + ((ty*tileHeight) + y)*w], TAG_ARRAY); 
						}
					}

				}
			}
		}
		else if (token.equals(":include")) {
			String fileName = tokens.remove().toString();
			if (path != null) { fileName = path + File.separator + fileName; }
			compileFile(fileName);
		}
		else if (token.equals(":proto")) {
			String wordName = tokens.remove().toString();
			prototypes.put(wordName, new ArrayList<Integer>());
		}

		// branching constructs
		else if (token.equals("if")) {
			romAdd(OP_JUMPZ, TAG_CODE);
			branchStack.push(rom.size());
			romAdd(-1, TAG_CODE);
		}
		else if (token.equals("-if")) {
			romAdd(OP_JUMPIF, TAG_CODE);
			branchStack.push(rom.size());
			romAdd(-1, TAG_CODE);
		}
		else if (token.equals("else")) {
			romAdd(OP_JUMP, TAG_CODE);
			int over = rom.size();
			romAdd(-1, TAG_CODE);
			rom.set(branchStack.pop(), rom.size());
			branchStack.push(over);
		}
		else if (token.equals("then")) {
			rom.set(branchStack.pop(), rom.size());
		}

		// unbounded loop constructs
		else if (token.equals("loop")) {
			loopStack.push(rom.size());
		}
		else if (token.equals("while")) {
			romAdd(OP_JUMPIF, TAG_CODE);
			romAdd(loopStack.pop(), TAG_CODE);
			while(breaks.size() > 0) {
				rom.set(breaks.remove(), rom.size());
			}
		}
		else if (token.equals("until")) {
			romAdd(OP_JUMPZ, TAG_CODE);
			romAdd(loopStack.pop(), TAG_CODE);
			while(breaks.size() > 0) {
				rom.set(breaks.remove(), rom.size());
			}
		}
		else if (token.equals("again")) {
			romAdd(OP_JUMP, TAG_CODE);
			romAdd(loopStack.pop(), TAG_CODE);
			while(breaks.size() > 0) {
				rom.set(breaks.remove(), rom.size());
			}
		}
		else if (token.equals("break")) {
			romAdd(OP_JUMP, TAG_CODE);
			breaks.add(rom.size());
			romAdd(-2, TAG_CODE);
		}

		// bounded loops
		else if (token.equals("for")) {
			romAdd(OP_STR, TAG_CODE);
			loopStack.push(rom.size());
		}
		else if (token.equals("next")) {
			/*
			romAdd(OP_RTS,          TAG_CODE);
			romAdd(OP_CONST,        TAG_CODE);
			romAdd(1,               TAG_CODE);
			romAdd(OP_SUB,          TAG_CODE);
			romAdd(OP_DUP,          TAG_CODE);
			romAdd(OP_STR,          TAG_CODE);

			romAdd(OP_CONST,        TAG_CODE); // loop while i >= 0
			romAdd(0,               TAG_CODE);
			romAdd(OP_SLT,          TAG_CODE);
			romAdd(OP_JUMPZ,        TAG_CODE);
			*/
			romAdd(OP_NEXT,         TAG_CODE);

			romAdd(loopStack.pop(), TAG_CODE);
			romAdd(OP_RTS,          TAG_CODE);
			romAdd(OP_DROP,         TAG_CODE);
		}

		// basic ops
		else if (token.equals("exit")) { romAdd(OP_RETURN, TAG_CODE); }
		else if (token.equals("@"))    { romAdd(OP_LOAD,   TAG_CODE); }
		else if (token.equals("!"))    { romAdd(OP_STOR,   TAG_CODE); }
		else if (token.equals("drop")) { romAdd(OP_DROP,   TAG_CODE); }
		else if (token.equals("dup"))  { romAdd(OP_DUP,    TAG_CODE); }
		else if (token.equals("over")) { romAdd(OP_OVER,   TAG_CODE); }
		else if (token.equals("swap")) { romAdd(OP_SWAP,   TAG_CODE); }
		else if (token.equals(">r"))   { romAdd(OP_STR,    TAG_CODE); }
		else if (token.equals("r>"))   { romAdd(OP_RTS,    TAG_CODE); }
		else if (token.equals("+"))    { romAdd(OP_ADD,    TAG_CODE); }
		else if (token.equals("-"))    { romAdd(OP_SUB,    TAG_CODE); }
		else if (token.equals("*"))    { romAdd(OP_MUL,    TAG_CODE); }
		else if (token.equals("/"))    { romAdd(OP_DIV,    TAG_CODE); }
		else if (token.equals("mod"))  { romAdd(OP_MOD,    TAG_CODE); }
		else if (token.equals("and"))  { romAdd(OP_AND,    TAG_CODE); }
		else if (token.equals("or"))   { romAdd(OP_OR,     TAG_CODE); }
		else if (token.equals("xor"))  { romAdd(OP_XOR,    TAG_CODE); }
		else if (token.equals("not"))  { romAdd(OP_NOT,    TAG_CODE); }
		else if (token.equals(">"))    { romAdd(OP_SGT,    TAG_CODE); }
		else if (token.equals("<"))    { romAdd(OP_SLT,    TAG_CODE); }
		else if (token.equals("sync")) { romAdd(OP_SYNC,   TAG_CODE); }
		
		// pseudo-ops
		else if (token.equals("<="))    { romAdd(OP_SGT, TAG_CODE); romAdd(OP_NOT, TAG_CODE); }
		else if (token.equals(">="))    { romAdd(OP_SLT, TAG_CODE); romAdd(OP_NOT, TAG_CODE); }
		else if (token.equals("2dup"))  { romAdd(OP_OVER, TAG_CODE); romAdd(OP_OVER, TAG_CODE); }
		else if (token.equals("2drop")) { romAdd(OP_DROP, TAG_CODE); romAdd(OP_DROP, TAG_CODE); }
		else if (token.equals("rdrop")) { romAdd(OP_RTS,  TAG_CODE); romAdd(OP_DROP, TAG_CODE); }
		else if (token.equals("halt"))  { romAdd(OP_JUMP, TAG_CODE); romAdd(-1, TAG_CODE); }
		else if (token.equals("keys")) {
			romAdd(OP_CONST, TAG_CODE);
			romAdd(KY,       TAG_CODE);
			romAdd(OP_LOAD,  TAG_CODE); 
		}
		else if (token.equals("i")) {
			romAdd(OP_RTS, TAG_CODE);
			romAdd(OP_DUP, TAG_CODE);
			romAdd(OP_STR, TAG_CODE);
			//romAdd(OP_CONST, TAG_CODE);
			//romAdd(RP,       TAG_CODE);
			//romAdd(OP_LOAD,  TAG_CODE);
			//romAdd(OP_CONST, TAG_CODE);
			//romAdd(1,        TAG_CODE);
			//romAdd(OP_SUB,   TAG_CODE);
			//romAdd(OP_LOAD,  TAG_CODE);
		}
		else if (token.equals("j")) {
			romAdd(OP_RTS, TAG_CODE);
			romAdd(OP_RTS, TAG_CODE);
			romAdd(OP_DUP, TAG_CODE);
			romAdd(OP_STR, TAG_CODE);
			romAdd(OP_SWAP, TAG_CODE);
			romAdd(OP_STR, TAG_CODE);
		}
		else if (token.equals("'")) {
			romAdd(OP_CONST, TAG_CODE);
			String methodName = tokens.remove().toString();
			if (prototypes.containsKey(methodName)) {
				prototypes.get(methodName).add(rom.size());
				romAdd(-5, TAG_CODE);
			}
			else {
				romAdd(dictionary.get(methodName), TAG_CODE);
			}
		}
		else if (token.equals("exec")) {
			romAdd(OP_CONST,       TAG_CODE);
			romAdd(rom.size() + 3, TAG_CODE); // hooray for self-modifying code!
			romAdd(OP_STOR,        TAG_CODE);
			romAdd(OP_CALL,        TAG_CODE);
			romAdd(-3,             TAG_CODE);
		}

		else if (prototypes.containsKey(token)) {
			if (compiling) { romAdd(OP_CALL, TAG_CODE); }
			prototypes.get(token).add(rom.size());
			romAdd(-5, compiling ? TAG_CODE : TAG_ARRAY);
		}
		else if (constants.containsKey(token)) {
			if (compiling) { romAdd(OP_CONST, TAG_CODE); }
			romAdd(constants.get(token), compiling ? TAG_CODE : TAG_ARRAY);
		}
		else if (variables.containsKey(token)) {
			if (compiling) { romAdd(OP_CONST, TAG_CODE); }
			romAdd(variables.get(token), compiling ? TAG_CODE : TAG_ARRAY);
		}
		else if (dictionary.containsKey(token)) {
			if (compiling) { romAdd(OP_CALL, TAG_CODE); }
			romAdd(dictionary.get(token), compiling ? TAG_CODE : TAG_ARRAY);
		}
		else {
			throw new Error("Unknown word '"+token+"'");
		}
	}

	public void disassemble() {
		for(int index = 0; index < rom.size(); index++) {
			int t = tag.get(index);
			System.out.format("%05d: %-16s", index, getLabel(index));
			if (t == TAG_ARRAY) {
				int start = index;
				while(index < rom.size() && tag.get(index) == TAG_ARRAY) {
					if (index != start && !getLabel(index).equals("")) {
						break;
					}
					index++;
				}
				if (index - start == 1) {
					System.out.format("%d%n", rom.get(start));
				}
				else {
					System.out.format("<<< %d words >>>%n", (index - start));
				}
				index--;
			}
			else if (t == TAG_STRING) {
				String s = "";
				while(rom.get(index) != 0) {
					s += (char)rom.get(index).intValue();
					index++;
				}
				s = s.replace("\n", "\\n");
				s = s.replace("\r", "\\r");
				s = s.replace("\t", "\\t");
				System.out.format("\"%s\"%n", s);
			}
			else if (t == TAG_CODE) {
				if (paramOps.contains(rom.get(index))) {
					if (rom.get(index) == OP_CALL) {
						System.out.format("%5s %d %s%n",
							mnemonics.get(rom.get(index)),
							rom.get(index+1),
							getLabel(rom.get(index+1))
						);
					}
					else {
						System.out.format("%5s %d%n",
							mnemonics.get(rom.get(index)),
							rom.get(index+1)
						);
					}
					index++;
				}
				else {
					System.out.format("%5s%n",
						mnemonics.get(rom.get(index))
					);
				}
			}
			else {
				System.out.format("%d%n", rom.get(index));
			}
		}
		System.out.format("%n%d words, %.3f kb.%n", rom.size(), ((double)rom.size())/256);
	}

	private String getLabel(int address) {
		for(Map.Entry entry : dictionary.entrySet()) {
			if (entry.getValue().equals(address)) { return String.format("(%s)", entry.getKey()); }
		}
		for(Map.Entry entry : variables.entrySet()) {
			if (entry.getValue().equals(address)) { return String.format("(%s)", entry.getKey()); }
		}
		return "";
	}

	private static Queue<Object> tokens(String text) {
		int index = 0;
		Queue<Object> ret = new LinkedList<Object>();

		while(index < text.length()) {
			while(Character.isWhitespace(text.charAt(index))) {
				index++;
				if (index >= text.length()) { return ret; }
			}
			String token = "";
			// Perl-style line comments
			if (text.charAt(index) == '#') {
				while(index < text.length() && text.charAt(index) != '\n') {
					index++;
				}
			}
			// Forth-style paren comments (so I can have normal notation for stack diagrams)
			else if (text.charAt(index) == '(') {
				index++;
				while(index < text.length() && text.charAt(index) != ')') {
					index++;
				}
				index++;
			}
			// quoted strings
			else if (text.charAt(index) == '"') {
				index++;
				while(index < text.length() && text.charAt(index) != '"') {
					token += text.charAt(index);
					index++;
				}
				index++;
			}
			// whitespace-delimited words
			else {
				while(index < text.length() && !Character.isWhitespace(text.charAt(index))) {
					token += text.charAt(index);
					index++;
				}
			}
			// hex, binary and decimal numbers with C-style prefixes.
			try {
				if (token.startsWith("0x")) {
					ret.add((int)Long.parseLong(token.substring(2), 16));
				}
				else if (token.startsWith("0b")) {
					ret.add((int)Long.parseLong(token.substring(2), 2));
				}
				else {
					ret.add(Integer.parseInt(token));
				}
			}
			catch(NumberFormatException e) {
				if (token.length() > 0) { ret.add(token); }
			}
		}
		return ret;
	}

}