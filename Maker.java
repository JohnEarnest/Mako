import java.util.*;
import java.io.*;

public class Maker implements MakoConstants {

	private Map<String, Integer> dictionary = new TreeMap<String, Integer>();
	private Map<String, Integer> variables = new TreeMap<String, Integer>();
	private Map<String, Integer> constants = new TreeMap<String, Integer>();
	private Map<String, List<Integer>> prototypes = new HashMap<String, List<Integer>>();
	private Set<String> imported = new HashSet<String>();
	private MakoRom rom = new MakoRom();
	private boolean compiling = false;
	private Stack<Integer> branchStack = new Stack<Integer>();
	private Stack<Integer> loopStack   = new Stack<Integer>();
	private Queue<Integer> breaks      = new LinkedList<Integer>();

	private String path;

	public static void main(String[] args) {
		List<String> argList = new ArrayList<String>(Arrays.asList(args));
		boolean run = false;
		boolean standalone = false;
		boolean packed = false;
		if (argList.contains("--run")) {
			run = true;
			argList.remove("--run");
		}
		if (argList.contains("--standalone")) {
			standalone = true;
			argList.remove("--standalone");
		}
		if (argList.contains("--packed")) {
			packed = true;
			argList.remove("--packed");
		}

		Maker compiler = new Maker();
		compiler.compile(argList.get(0), standalone);

		if (argList.contains("--word")) {
			int index = argList.indexOf("--word");
			String word = argList.get(index + 1);
			compiler.disassemble(word);
			argList.remove("--word");
			argList.remove(word);
		}
		else {
			compiler.rom.disassemble(System.out);
		}
		if (argList.size() > 1) {
			try {
				if (packed) {
					DataOutputStream out = new DataOutputStream(new FileOutputStream(argList.get(1)));
					for(int x : compiler.rom.toArray()) { out.writeInt(x); }
					out.close();
				}
				else {
					PrintWriter out = new PrintWriter(new File(argList.get(1)));
					for(int x : compiler.rom.toArray()) { out.println(x); }
					out.close();
				}
			}
			catch(IOException ioe) { ioe.printStackTrace(); }
		}
		if (run) { Mako.exec(compiler.rom.toArray()); }
	}

	public MakoRom compile(String filename, boolean standalone) {
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
		dict.put("AU", AU);

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

		// export debug symbols:
		for(Map.Entry<String, Integer> entry : variables.entrySet()) {
			rom.label(entry.getKey(), entry.getValue());
		}
		for(Map.Entry<String, Integer> entry : dictionary.entrySet()) {
			rom.label(entry.getKey(), entry.getValue());
		}

		return rom;
	}

	private void buildRegion(String name, int size) {
		if (!variables.containsKey(name) && !constants.containsKey(name)) {
			variables.put(name, rom.size());
			for(int x = 0; x < size; x++) { rom.add(0, MakoRom.Type.Array); }
		}
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
				if (compiling) { rom.add(OP_CONST, MakoRom.Type.Code); }
				rom.add((Integer)token, MakoRom.Type.Array);
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
			rom.addJump(rom.size()+2);
		}
		else if (token.equals(";")) {
			compiling = false;
			rom.addReturn();
		}
		else if (token.equals(":var")) {
			variables.put(tokens.remove().toString(), rom.size());
			rom.add(0, MakoRom.Type.Data);
		}
		else if (token.equals(":array")) {
			String name = tokens.remove().toString();
			int count = (Integer)tokens.remove();
			int value = (Integer)tokens.remove();
			variables.put(name, rom.size());
			for(int x = 0; x < count; x++) { rom.add(value, MakoRom.Type.Array); }
		}
		else if (token.equals(":data")) {
			variables.put(tokens.remove().toString(), rom.size());
		}
		else if (token.equals(":string")) {
			variables.put(tokens.remove().toString(), rom.size());
			rom.addString(unquote(tokens.remove().toString()));
		}
		else if (token.startsWith("\"")) {
			int start = rom.addJump(-1);
			rom.addString(unquote(token));
			rom.set(start, rom.size());
			rom.addConst(start + 1);
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
			String fileName = unquote(tokens.remove().toString());
			if (path != null) { fileName = path + File.separator + fileName; }
			variables.put(imageName, rom.size());
			int tileWidth  = (Integer)tokens.remove();
			int tileHeight = (Integer)tokens.remove();
			rom.addImage(fileName, tileWidth, tileHeight);
		}
		else if (token.equals(":include")) {
			String fileName = unquote(tokens.remove().toString());
			if (path != null) { fileName = path + File.separator + fileName; }
			compileFile(fileName);
		}
		else if (token.equals(":proto")) {
			String wordName = tokens.remove().toString();
			prototypes.put(wordName, new ArrayList<Integer>());
		}

		// branching constructs
		else if (token.equals("if")) {
			branchStack.push(rom.addJumpZ(-2));
		}
		else if (token.equals("-if")) {
			branchStack.push(rom.addJumpIf(-2));
		}
		else if (token.equals("else")) {
			int over = rom.addJump(-2);
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
			rom.addJumpIf(loopStack.pop());
			while(breaks.size() > 0) {
				rom.set(breaks.remove(), rom.size());
			}
		}
		else if (token.equals("until")) {
			rom.addJumpZ(loopStack.pop());
			while(breaks.size() > 0) {
				rom.set(breaks.remove(), rom.size());
			}
		}
		else if (token.equals("again")) {
			rom.addJump(loopStack.pop());
			while(breaks.size() > 0) {
				rom.set(breaks.remove(), rom.size());
			}
		}
		else if (token.equals("break")) {
			breaks.add(rom.addJump(-3));
		}

		// bounded loops
		else if (token.equals("for")) {
			rom.addStr();
			loopStack.push(rom.size());
		}
		else if (token.equals("next")) {
			rom.addNext(loopStack.pop());
			rom.addRts();
			rom.addDrop();
		}

		// basic ops
		else if (token.equals("exit")) { rom.addReturn(); }
		else if (token.equals("@"))    { rom.addLoad();   }
		else if (token.equals("!"))    { rom.addStor();   }
		else if (token.equals("drop")) { rom.addDrop();   }
		else if (token.equals("dup"))  { rom.addDup();    }
		else if (token.equals("over")) { rom.addOver();   }
		else if (token.equals("swap")) { rom.addSwap();   }
		else if (token.equals(">r"))   { rom.addStr();    }
		else if (token.equals("r>"))   { rom.addRts();    }
		else if (token.equals("+"))    { rom.addAdd();    }
		else if (token.equals("-"))    { rom.addSub();    }
		else if (token.equals("*"))    { rom.addMul();    }
		else if (token.equals("/"))    { rom.addDiv();    }
		else if (token.equals("mod"))  { rom.addMod();    }
		else if (token.equals("and"))  { rom.addAnd();    }
		else if (token.equals("or"))   { rom.addOr();     }
		else if (token.equals("xor"))  { rom.addXor();    }
		else if (token.equals("not"))  { rom.addNot();    }
		else if (token.equals(">"))    { rom.addSgt();    }
		else if (token.equals("<"))    { rom.addSlt();    }
		else if (token.equals("sync")) { rom.addSync();   }
		
		// pseudo-ops
		else if (token.equals("<="))    { rom.addSgt();  rom.addNot();  }
		else if (token.equals(">="))    { rom.addSlt();  rom.addNot();  }
		else if (token.equals("2dup"))  { rom.addOver(); rom.addOver(); }
		else if (token.equals("2drop")) { rom.addDrop(); rom.addDrop(); }
		else if (token.equals("rdrop")) { rom.addRts();  rom.addDrop(); }
		else if (token.equals("halt"))  { rom.addJump(-1); }
		else if (token.equals("keys")) {
			rom.addConst(KY);
			rom.addLoad();
		}
		else if (token.equals("i")) {
			rom.addRts();
			rom.addDup();
			rom.addStr();
		}
		else if (token.equals("j")) {
			rom.addRts();
			rom.addRts();
			rom.addDup();
			rom.addStr();
			rom.addSwap();
			rom.addStr();
		}
		else if (token.equals("k")) {
			rom.addRts();
			rom.addRts();
			rom.addRts();
			rom.addDup();
			rom.addStr();
			rom.addSwap();
			rom.addStr();
			rom.addSwap();
			rom.addStr();
		}
		else if (token.equals("'")) {
			String methodName = tokens.remove().toString();
			if (prototypes.containsKey(methodName)) {
				prototypes.get(methodName).add(rom.size());
				rom.addConst(-5);
			}
			else {
				rom.addConst(dictionary.get(methodName));
			}
		}
		else if (token.equals("exec")) {
			rom.addConst(rom.size() + 4);
			rom.addStor();   // hooray for self-modifying code!
			rom.addCall(-4);

		}

		else if (prototypes.containsKey(token)) {
			int address = rom.size();
			if (compiling) { address = rom.addCall(-5); }
			else { rom.add(-5, MakoRom.Type.Array); }
			prototypes.get(token).add(address);
		}
		else if (constants.containsKey(token)) {
			int address = constants.get(token);
			if (compiling) { rom.addConst(address); }
			else { rom.add(address, MakoRom.Type.Array); }
		}
		else if (variables.containsKey(token)) {
			int address = variables.get(token);
			if (compiling) { rom.addConst(address); }
			else { rom.add(address, MakoRom.Type.Array); }
		}
		else if (dictionary.containsKey(token)) {
			int address = dictionary.get(token);
			if (compiling) { rom.addCall(address); }
			else { rom.add(address, MakoRom.Type.Array); }
		}
		else {
			throw new Error("Unknown word '"+token+"'");
		}
	}

	public void disassemble(String word) {
		if (!dictionary.containsKey(word)) {
			throw new Error("Unknown word '"+word+"'");
		}
		int first = dictionary.get(word);
		int last = first + 1;
		while(last < rom.size() && "".equals(rom.getLabel(last))) { last++; }
		rom.disassemble(first, last-1, System.out);
	}

	private static String unquote(String s) {
		if (s.startsWith("\"")) {
			return s.substring(1, s.length() - 1);
		}
		return s;
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
				token = '"' + token + '"';
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