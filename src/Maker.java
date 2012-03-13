import java.util.*;
import java.io.*;

public class Maker implements MakoConstants {

	private Map<String, Integer> dictionary = new TreeMap<String, Integer>();
	private Map<String, Integer> variables = new TreeMap<String, Integer>();
	private Map<String, Integer> constants = new TreeMap<String, Integer>();
	private Map<String, List<Integer>> prototypes = new HashMap<String, List<Integer>>();
	private MakoRom rom = new MakoRom();
	private boolean compiling = false;
	private String wordName = null;
	private Stack<Integer> branchStack = new Stack<Integer>();
	private Stack<Integer> loopStack   = new Stack<Integer>();
	private Queue<Integer> breaks      = new LinkedList<Integer>();

	private Stack<String> currentPath = new Stack<String>();

	public static void main(String[] args) {
		List<String> argList = new ArrayList<String>(Arrays.asList(args));

		boolean run        = pluckArg(argList, "--run");
		boolean fuzz       = pluckArg(argList, "--fuzz");
		boolean symbols    = pluckArg(argList, "--symbols");
		boolean quiet      = pluckArg(argList, "--quiet");
		boolean trace      = pluckArg(argList, "--trace");

		Maker compiler = new Maker();

		try { compiler.compileToken(":include", tokens("<Lang.fs>")); }
		catch(FileNotFoundException f) { throw new Error("unable to load lib/Lang.fs!"); }
		compiler.compile(argList.get(0));

		if (argList.contains("--word")) {
			int index = argList.indexOf("--word");
			String word = argList.get(index + 1);
			compiler.disassemble(word);
			argList.remove("--word");
			argList.remove(word);
		}
		else if (!quiet) {
			compiler.rom.disassemble(System.out);
		}
		if (argList.size() > 1) {
			compiler.rom.write(argList.get(1), symbols);
		}
		if (run) {
			int[] mem = compiler.rom.toArray();
			try {
				Mako.exec(mem, fuzz, trace ? compiler.rom : null);
			}
			catch(Throwable t) {
				System.out.println("Runtime Error: ");
				t.printStackTrace();
				System.out.println("Analyzing and dumping core...");

				try {
					// we want to be able to see any significant differences,
					// so we should ungroup array data like the stacks:
					compiler.rom.swapType(MakoRom.Type.Array, MakoRom.Type.Data);

					compiler.rom.disassemble(new PrintStream(new File(argList.get(0) + ".before")));
					System.out.println("Wrote '"+argList.get(0) + ".before'.");

					for(int x = 0; x < mem.length; x++) { compiler.rom.set(x, mem[x]); }
					compiler.rom.write(argList.get(0) + ".coredump", false);
					System.out.println("Wrote '"+argList.get(0) + ".coredump'.");

					compiler.rom.disassemble(new PrintStream(new File(argList.get(0) + ".after")));
					System.out.println("Wrote '"+argList.get(0) + ".after'.");
				}
				catch(FileNotFoundException e) {
					e.printStackTrace();
				}
			}
		}
	}

	private static boolean pluckArg(List<String> argList, String name) {
		if (argList.contains(name)) {
			argList.remove(name);
			return true;
		}
		return false;
	}

	public Maker() {
		buildRegion("registers", RESERVED_HEADER);
		variables.put("PC", PC);
		variables.put("DP", DP);
		variables.put("RP", RP);
		variables.put("GP", GP);
		variables.put("GT", GT);
		variables.put("SP", SP);
		variables.put("ST", ST);
		variables.put("GS", GS);
		variables.put("SX", SX);
		variables.put("SY", SY);
		variables.put("CL", CL);
		variables.put("RN", RN);
		variables.put("KY", KY);
		variables.put("CO", CO); // character-out (debug)
		variables.put("AU", AU);

		constants.put("key-up", KEY_UP);
		constants.put("key-rt", KEY_RT);
		constants.put("key-dn", KEY_DN);
		constants.put("key-lf", KEY_LF);
		constants.put("key-a",  KEY_A);
		constants.put("key-b",  KEY_B);

		constants.put("sprite-mirror-horiz", H_MIRROR_MASK);
		constants.put("sprite-mirror-vert",  V_MIRROR_MASK);
		constants.put("grid-z",              GRID_Z_MASK);

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
	}

	public MakoRom compile(String filename) {
		try {
			File rootPath = new File(filename);
			currentPath.push(rootPath.getParent());
			compileFile(rootPath.getName());
		}
		catch(FileNotFoundException e) {
			throw new Error(e.getMessage());
		}
		if (!dictionary.containsKey("main")) {
			throw new Error("No entrypoint defined!");
		}
		if (prototypes.size() > 0) {
			for(String s : prototypes.keySet()) {
				System.out.println("unresolved prototype '"+s+"'");
			}
			throw new Error();
		}

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
		currentPath.push(new File(currentPath.peek(), filename).getParent());
		/*
		System.out.println("Compiling: " + new File(
			currentPath.peek(),
			new File(filename).getName()
		));
		*/
		Scanner in = new Scanner(new File(
			currentPath.peek(),
			new File(filename).getName()
		));
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
		currentPath.pop();
	}

	private void compileToken(String token, Queue<Object> tokens) throws FileNotFoundException {
		// defining words
		if (token.equals(":")) {
			compiling = true;
			wordName = tokens.remove().toString();
			dictionary.put(wordName, rom.size());
			if (prototypes.containsKey(wordName)) {
				for(Integer a : prototypes.remove(wordName)) {
					rom.set(a, rom.size());
				}
			}
		}
		else if (token.equals(":vector")) {
			compiling = true;
			wordName = tokens.remove().toString();
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
			if (wordName.equals("main")) { rom.addJump(-1); }
			else                         { rom.addReturn(); }
			wordName = null;
		}
		else if (token.equals("{")) {
			branchStack.push(rom.addJump(-7));
		}
		else if (token.equals("}")) {
			rom.addReturn();
			int addr = branchStack.pop();
			rom.set(addr, rom.size());
			rom.addConst(addr+1);
		}
		else if (token.equals(":var")) {
			variables.put(tokens.remove().toString(), rom.size());
			rom.add(0, MakoRom.Type.Data);
		}
		else if (token.equals(":array")) {
			String name = tokens.remove().toString();
			int count = getConstant(tokens.remove());
			int value = getConstant(tokens.remove());
			variables.put(name, rom.size());
			for(int x = 0; x < count; x++) { rom.add(value, MakoRom.Type.Array); }
		}
		else if (token.equals(":data")) {
			variables.put(tokens.remove().toString(), rom.size());
		}
		else if (token.startsWith("\"")) {
			if (compiling) {
				int start = rom.addJump(-1);
				rom.addString(unquote(token));
				rom.set(start, rom.size());
				rom.addConst(start + 1);
			}
			else {
				rom.addString(unquote(token));
			}
		}
		else if (token.equals(":const")) {
			String constName = tokens.remove().toString();
			Object constValue = tokens.remove();
			constants.put(constName, getConstant(constValue));
		}
		else if (token.equals(":image")) {
			String imageName = tokens.remove().toString();
			String fileName = new File(
				currentPath.peek(),
				new File(unquote(tokens.remove().toString())).getName()
			).toString();
			variables.put(imageName, rom.size());
			int tileWidth  = getConstant(tokens.remove());
			int tileHeight = getConstant(tokens.remove());
			rom.addImage(fileName, tileWidth, tileHeight);
		}
		else if (token.equals(":include")) {
			String srcName = tokens.remove().toString();
			if (srcName.startsWith("<")) {

				// I highly suspect this is a brittle solution- I need
				// to test this on different machine configurations.
				String libPath = new File(Maker.class.getProtectionDomain()
					.getCodeSource().getLocation().getPath()).getParent();

				currentPath.push(libPath + "/../lib/");
				String fileName = srcName.substring(1, srcName.length() - 1);
				compileFile(fileName);
				currentPath.pop();
			}
			else {
				String fileName = unquote(srcName);
				compileFile(fileName);
			}
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
				prototypes.get(methodName).add(rom.addConst(-5));
			}
			else {
				if (!dictionary.containsKey(methodName)) {
					throw new Error("Unknown word quoted '"+methodName+"'");
				}
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

	private int getConstant(Object o) {
		if (o instanceof Integer) { return (Integer)o; }
		if ( constants.containsKey(o.toString())) { return  constants.get(o.toString()); }
		if ( variables.containsKey(o.toString())) { return  variables.get(o.toString()); }
		if (dictionary.containsKey(o.toString())) { return dictionary.get(o.toString()); }
		throw new Error("Unknown constant '"+o+"'");
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