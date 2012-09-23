import java.util.*;
import java.io.*;
import java.net.*;

public class Maker implements MakoConstants {

	private static boolean guardShadow = false;

	private Map<String, Integer> dictionary = new TreeMap<String, Integer>();
	private Map<String, Integer> variables = new TreeMap<String, Integer>();
	private Map<String, List<Integer>> prototypes = new HashMap<String, List<Integer>>();
	private Map<String, List<Integer>> references = new HashMap<String, List<Integer>>();
	private MakoRom rom;
	private boolean compiling = false;
	private String wordName = null;
	private Stack<Integer> branchStack = new Stack<Integer>();
	private Stack<Integer> loopStack   = new Stack<Integer>();
	private Queue<Integer> breaks      = new LinkedList<Integer>();

	private Stack<String> currentPath = new Stack<String>();

	public static void main(String[] args) {
		List<String> argList = new ArrayList<String>(Arrays.asList(args));

		boolean run        = pluckArg(argList, "--run") || pluckArg(argList, "-r");
		boolean fuzz       = pluckArg(argList, "--fuzz");
		boolean symbols    = pluckArg(argList, "--symbols");
		boolean quiet      = pluckArg(argList, "--quiet") || pluckArg(argList, "-q");
		boolean listing    = pluckArg(argList, "--listing") || pluckArg(argList, "-l");
		boolean trace      = pluckArg(argList, "--trace");
		boolean traceLive  = pluckArg(argList, "--traceLive");
		boolean showOpt    = pluckArg(argList, "--showOpt");
		boolean noOpt      = pluckArg(argList, "--noOpt");
		boolean gCode      = pluckArg(argList, "--guardCode");
		boolean gStack     = pluckArg(argList, "--guardStacks");
		guardShadow        = pluckArg(argList, "--guardShadow");

		if (argList.size() == 0) {
			System.out.println("usage: java -jar Maker.jar [options] file [output]\n"
							+ "Options:\n -r/--run\trun program after compiling\n"
							+ " -l/--listing\toutput program disassembly\n"
							+ " --fuzz\t\trandomize inputs when running\n"
							+ " --word <word>\tdisassemble given word\n"
							+ " --symbols\twrite debugging symbols\n"
							+ " -q/--quiet\t\tsuppress output\n"
							+ " --showOpt\t display peephole optimizations\n"
							+ " --noOpt\t disable peephole optimizer\n"
							+ " --guardCode\t halt if the VM attempts to read/write code words.\n"
							+ " --guardStacks\t halt on stack over/underflow.\n"
							+ " --guardShadow\t compiler warnings when names are reused.\n"
			);
			System.exit(1);
		}

		Maker compiler = new Maker();
		compiler.rom.showOptimizations(showOpt);
		compiler.rom.optimize = !noOpt;
		try { compiler.compileToken(":include", tokens("<Lang.fs>")); }
		catch(IOException f) { System.out.println("Warning: unable to load lib/Lang.fs!"); }
		compiler.compile(argList.get(0));

		if (argList.contains("--word")) {
			int index = argList.indexOf("--word");
			String word = argList.get(index + 1);
			compiler.disassemble(word);
			argList.remove("--word");
			argList.remove(word);
		}
		else {
			if (listing) {
				compiler.rom.disassemble(System.out);
			} else if (!quiet) {
				// print the size summary
				compiler.rom.disassemble(0, -1, System.out);
			}
		}
		if (argList.size() > 1) {
			compiler.rom.write(argList.get(1), symbols);
		}
		if (run) {
			int[] mem = compiler.rom.toArray();
			try {
				Mako.trace       = trace;
				Mako.traceLive   = traceLive;
				Mako.guardCode   = gCode;
				Mako.guardStacks = gStack;
				Mako.exec(mem, fuzz, compiler.rom);
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
				catch(IOException e) {
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

	public Maker(MakoRom rom) {
		this.rom = rom;

		// sprite size constants:
		for(int x = 0; x < 8; x++) {
			for(int y = 0; y < 8; y++) {
				int w = (x + 1) * 8;
				int h = (y + 1) * 8;
				int v = ((x << 8) & 0x0F00) | ((y << 12) & 0xF000) | 1;
				defineConstant(String.format("%dx%d", w, h), v);
			}
		}
	}

	public Maker() {
		this(new MakoRom());

		buildRegion("registers", RESERVED_HEADER);
		defineVariable("PC", PC);
		defineVariable("DP", DP);
		defineVariable("RP", RP);
		defineVariable("GP", GP);
		defineVariable("GT", GT);
		defineVariable("SP", SP);
		defineVariable("ST", ST);
		defineVariable("GS", GS);
		defineVariable("SX", SX);
		defineVariable("SY", SY);
		defineVariable("CL", CL);
		defineVariable("RN", RN);
		defineVariable("KY", KY);
		defineVariable("CO", CO);
		defineVariable("AU", AU);
		defineVariable("KB", KB);
	}

	public MakoRom compile(String filename) {
		try {
			File rootPath = new File(filename);
			currentPath.push(rootPath.getParent());
			compileFile(rootPath.getName());
		}
		catch(IOException e) {
			throw new Error(e.getMessage());
		}

		if      (dictionary.containsKey("main")) { rom.set(PC, dictionary.get("main")); }
		else if ( variables.containsKey("main")) { rom.set(PC,  variables.get("main")); }
		else { throw new Error("No entrypoint defined!"); }

		if (prototypes.size() > 0 || references.size() > 0) {
			for(String s : prototypes.keySet()) {
				System.out.println("unresolved prototype '"+s+"'");
			}
			for(String s : references.keySet()) {
				System.out.println("unresolved reference '"+s+"'");
			}
			throw new Error();
		}

		buildRegion("data-stack",     50);
		buildRegion("return-stack",   50);
		buildRegion("grid",         1271);
		buildRegion("grid-tiles",     64);
		buildRegion("sprites",      1024);
		buildRegion("sprite-tiles",   64);
		rom.set(DP, variables.get("data-stack"));
		rom.set(RP, variables.get("return-stack"));
		rom.set(GP, variables.get("grid"));
		rom.set(GT, variables.get("grid-tiles"));
		rom.set(SP, variables.get("sprites"));
		rom.set(ST, variables.get("sprite-tiles"));
		rom.set(GS, variables.get("grid-skip"));
		rom.set(SX, variables.get("scroll-x"));
		rom.set(SY, variables.get("scroll-y"));
		rom.set(CL, variables.get("clear-color"));

		return rom;
	}

	private void buildRegion(String name, int size) {
		if (!variables.containsKey(name)) {
			defineVariable(name, rom.size());
			for(int x = 0; x < size; x++) { rom.add(0, MakoRom.Type.Array); }
		}
	}

	private void compileFile(String filename) throws IOException {
		currentPath.push(new File(currentPath.peek(), filename).getParent());
		/*
		System.out.println("Compiling: " + new File(
			currentPath.peek(),
			new File(filename).getName()
		));
		*/

		FileReader in = new FileReader(new File(
			currentPath.peek(),
			new File(filename).getName()
		));
		StringBuilder contents = new StringBuilder();
		char[] buffer = new char[4096];
		int read = 0;
		while ((read = in.read(buffer)) >= 0) {
			contents.append(buffer, 0, read);
		}
		compileFragment(contents.toString());
		currentPath.pop();
	}

	private void compileFragment(String source) throws IOException {
		Queue<Object> tokens = tokens(source);
		while(tokens.size() > 0) {
			Object token = tokens.remove();
			if (token instanceof Integer) {
				if (compiling) { rom.addConst((Integer)token); }
				else           { rom.add((Integer)token, MakoRom.Type.Array); }
			}
			else {
				compileToken((String)token, tokens);
			}
		}
	}

	private void compileToken(String token, Queue<Object> tokens) throws IOException {
		// defining words
		if (token.equals(":")) {
			compiling = true;
			wordName = tokens.remove().toString();
			defineWord(wordName, rom.size());
		}
		else if (token.equals(":vector")) {
			compiling = true;
			wordName = tokens.remove().toString();
			defineWord(wordName, rom.size());
			rom.addJump(rom.size()+2);
			rom.setType(rom.size()-1, MakoRom.Type.Data);
		}
		else if (token.equals(";")) {
			compiling = false;
			if ("main".equals(wordName)) { rom.addJump(-1); }
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
			String name = tokens.remove().toString();
			defineVariable(name, rom.size());
			rom.add(0, MakoRom.Type.Data);
		}
		else if (token.equals(":array")) {
			String name = tokens.remove().toString();
			int count = getConstant(tokens.remove());
			int value = getConstant(tokens.remove());
			defineVariable(name, rom.size());
			for(int x = 0; x < count; x++) { rom.add(value, MakoRom.Type.Array); }
		}
		else if (token.equals(":data")) {
			String name = tokens.remove().toString();
			defineVariable(name, rom.size());
		}
		else if (token.equals(":table")) {
			String tableName = tokens.remove().toString();
			List<Integer> tableEntries = new ArrayList<Integer>();
			while(true) {
				Object o = tokens.remove();
				if (o instanceof Integer) {
					tableEntries.add((Integer)o);
				}
				else {
					String so = o.toString();
					if (so.equals(";")) { break; }
					if (so.startsWith("\"")) {
						tableEntries.add(rom.size());
						rom.addString(unquote(so));
					}
				}
			}
			defineVariable(tableName, rom.size());
			for(Integer i : tableEntries) {
				rom.add(i, MakoRom.Type.Data); // CHANGE TO ARRAY TYPE AFTER TESTING!!!!!!
			}
			defineConstant(tableName + "-size", tableEntries.size());
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
			defineConstant(constName, getConstant(constValue));
		}
		else if (token.equals(":image")) {
			String imageName = tokens.remove().toString();
			String fileName = new File(
				currentPath.peek(),
				new File(unquote(tokens.remove().toString())).getName()
			).toString();
			defineVariable(imageName, rom.size());
			int tileWidth  = getConstant(tokens.remove());
			int tileHeight = getConstant(tokens.remove());
			rom.addImage(fileName, tileWidth, tileHeight);
		}
		else if (token.equals(":include")) {
			String srcName = tokens.remove().toString();
			if (srcName.startsWith("<")) {
				// I highly suspect this is a brittle solution- I need
				// to test this on different machine configurations.
				String libPath = URLDecoder.decode(new File(Maker.class.getProtectionDomain()
					.getCodeSource().getLocation().getPath()).getParent(), "UTF-8");

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
			if (!prototypes.containsKey(wordName)) {
				prototypes.put(wordName, new ArrayList<Integer>());
			}
		}
		else if (token.equals(":ref")) {
			String varName = tokens.remove().toString();
			if (!references.containsKey(varName)) {
				references.put(varName, new ArrayList<Integer>());
			}
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
				rom.addConst(-5);
				prototypes.get(methodName).add(rom.size() - 1);
			}
			else {
				if (!dictionary.containsKey(methodName)) {
					throw new Error("Unknown word quoted '"+methodName+"'");
				}
				rom.addConst(dictionary.get(methodName));
			}
		}
		else if (prototypes.containsKey(token)) {
			int address = rom.size();
			if (compiling) { rom.addCall(-5); address = rom.size() - 1; }
			else { rom.add(-5, MakoRom.Type.Array); }
			prototypes.get(token).add(address);
		}
		else if (references.containsKey(token)) {
			int address = rom.size();
			if (compiling) { rom.addDelayConst(-6); address = rom.size() - 1; }
			else { rom.add(-6, MakoRom.Type.Array); }
			references.get(token).add(address);
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

	private void defineConstant(String name, int value) {
		clearDef(name);
		variables.put(name, value);
	}

	private void defineVariable(String name, int value) {
		clearDef(name);
		rom.label(name, value);
		variables.put(name, value);
		if (references.containsKey(name)) {
			for(Integer a : references.remove(name)) {
				rom.set(a, rom.size());
			}
		}
	}

	private void defineWord(String name, int value) {
		clearDef(name);
		rom.label(name, value);
		dictionary.put(name, value);
		if (prototypes.containsKey(name)) {
			for(Integer a : prototypes.remove(name)) {
				rom.set(a, rom.size());
			}
		}
	}

	private void clearDef(String name) {
		if (guardShadow) {
			if ( variables.containsKey(name)) { System.out.format("shadowed value '%s'%n", name); }
			if (dictionary.containsKey(name)) { System.out.format("shadowed word '%s'%n",  name); }
		}
		variables.remove(name);
		dictionary.remove(name);
	}

	private int getConstant(Object o) {
		if (o instanceof Integer) { return (Integer)o; }
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
		int begin;
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
				begin = index++;
				while(index < text.length() && text.charAt(index) != '"') {
					index++;
				}
				index++;
				token = text.substring(begin, index);
			}
			// whitespace-delimited words
			else {
				begin = index;
				while(index < text.length() && !Character.isWhitespace(text.charAt(index))) {
					index++;
				}
				token = text.substring(begin, index);
				index++;
			}

			if (token.length() > 0) {
				// hex, binary and decimal numbers with C-style prefixes.
				try {
					if (token.startsWith("0x")) {
						ret.add((int)Long.parseLong(token.substring(2), 16));
					}
					else if (token.startsWith("0b")) {
						ret.add((int)Long.parseLong(token.substring(2), 2));
					}
					else {
						// parse ints directly, since throwing an exception
						// for every identifier is expensive
						int i = 0;
						char c = token.charAt(0);
						int num = 0, sgn = 1;
						if (c == '-') {
							i++;
							sgn = -1;
						}
						if (i < token.length()) {
							for (; i < token.length(); i++) {
								c = token.charAt(i);
								if ('0' <= c && c <= '9') {
									num = num * 10 + (c - '0');
								} else {
									// a normal identifier
									break;
								}
							}
							if (i == token.length()) {
								ret.add(num * sgn);
							} else {
								ret.add(token);
							}
						} else {
							ret.add(token);
						}
					}
				}
				catch(NumberFormatException e) {
					e.printStackTrace();
					System.err.println(token);
					//if (token.length() > 0) { ret.add(token); }
				}
			}
		}
		return ret;
	}
}
