import java.io.*;
import java.util.*;

// The Stroyent Processing Compiler

public class SPC {
	
	private final List<Struct>   structs   = new ArrayList<Struct>();
	private final List<Storage>  storage   = new ArrayList<Storage>();
	private final List<Function> functions = new ArrayList<Function>();
	private final MakoRom rom     = new MakoRom();
	private final Locals  locals  = new Locals(rom);
	private final Globals globals = new Globals(rom);
	private final List<Integer> breaks    = new ArrayList<Integer>();
	private final List<Integer> continues = new ArrayList<Integer>();
	private int stringIndex  = 0;
	private final Stack<String> currentPath = new Stack<String>();
	private final Map<String, Integer> constants = new HashMap<String, Integer>();

	public static void main(String[] args) {
		SPC compiler = new SPC(new ArrayList<String>(Arrays.asList(args)));
	}

	public SPC(List<String> args) {

		for(int x = MakoConstants.RESERVED_HEADER; x >= 0; x--) {
			rom.add(0, MakoRom.Type.Data);
		}

		globals.set("CO", MakoConstants.CO);

		storage.add(new Storage("data_stack",   4096));
		storage.add(new Storage("return_stack", 4096));
		storage.add(new Storage("grid",         1271));
		storage.add(new Storage("grid_tiles",     64));
		storage.add(new Storage("sprites",      1024));
		storage.add(new Storage("sprite_tiles",   64));

		boolean run   = pluckArg(args, "--run");
		boolean print = pluckArg(args, "--print");

		File rootPath = new File(args.get(0));
		currentPath.push(rootPath.getParent());
		compileFile(rootPath.getName());
		emit();

		rom.set(MakoConstants.PC, globals.get("main"));
		rom.set(MakoConstants.DP, globals.get("data_stack"));
		rom.set(MakoConstants.RP, globals.get("return_stack"));
		rom.set(MakoConstants.GP, globals.get("grid"));
		rom.set(MakoConstants.GT, globals.get("grid_tiles"));
		rom.set(MakoConstants.SP, globals.get("sprites"));
		rom.set(MakoConstants.ST, globals.get("sprite_tiles"));

		if (print) {
			System.out.println();
			rom.disassemble(System.out);
		}

		if (run) {
			int[] mem = rom.toArray();
			Mako.exec(mem, false, null);
		}
	}

	private static boolean pluckArg(List<String> argList, String name) {
		if (argList.contains(name)) {
			argList.remove(name);
			return true;
		}
		return false;
	}

	public void compileFile(String fileName) {
		try {
			currentPath.push(new File(currentPath.peek(), fileName).getParent());
			Scanner in = new Scanner(new File(
				currentPath.peek(),
				new File(fileName).getName()
			));

			StringBuilder fileText = new StringBuilder();
			while(in.hasNextLine()) {
				fileText.append(in.nextLine());
				fileText.append('\n');
			}
			compile(new Cursor(fileText.toString()));

			currentPath.pop();
		}
		catch(FileNotFoundException e) {
			System.out.println("Unable to open file '"+fileName+"'");
			System.exit(-1);
		}
	}

	private void compile(Cursor cursor) {
		while(!cursor.done()) {
			cursor.trim();
			if (cursor.match("var")) {
				storage.add(new Storage(cursor));
			}
			else if (cursor.match("func")) {
				functions.add(new Function(cursor));
			}
			else if (cursor.match("struct")) {
				structs.add(new Struct(cursor));
			}
			else if (cursor.match("const")) {
				String name = cursor.parseName();
				if (constants.containsKey(name)) {
					throw new Error("Error: Multiple declarations for '"+name+"'!");
				}
				constants.put(name, parseConstantExpression(cursor));
				cursor.expect(';');
			}
			else if (cursor.match("include")) {
				if (cursor.match("<")) {
					String libPath = new File(SPC.class.getProtectionDomain()
						.getCodeSource().getLocation().getPath()).getParent();
					currentPath.push(libPath + "/../lib/");
					compileFile(cursor.parseName() + ".snt");
					currentPath.pop();
					cursor.expect('>');
				}
				else {
					compileFile(cursor.parseString());
				}
				cursor.expect(';');
			}
			else if (cursor.match("image")) {
				throw new Error("Not Implemented!");
			}
			else {
				throw new Error("Unrecognized token '"+cursor.parseName()+"'!");
			}
		}
	}

	private void emit() {
		for(Storage  s :   storage) { s.emit(); }
		for(Function f : functions) { f.emit(); }

		for(String s : globals.undefined()) {
			System.out.println("Error: No declaration for '"+s+"'!");
			//System.exit(-1);
		}
	}

	private Statement parseStatement(Cursor c) {
		if (c.match("var")) {
			return new Local(c);
		}
		else if (c.match("return")) {
			return new Return(c);
		}
		else if (c.match("for")) {
			return new Loop(c, true);
		}
		else if (c.match("while")) {
			return new Loop(c, false);
		}
		else if (c.match("if")) {
			return new Conditional(c);
		}
		else if (c.match("break")) {
			return new Break(c);
		}
		else if (c.match("continue")) {
			return new Continue(c);
		}
		else if (c.match("sync")) {
			return new Sync(c);
		}
		else {
			// assume it's an assignment or call:
			Expression target = parseExpression(c);
			for(AssignOp op : AssignOp.values()) {
				if (c.match(op.name)) {
					Expression source = parseExpression(c);
					c.expect(';');

					if (target instanceof Deref) {
						target = target.children.get(0);
					}
					return new Assignment(target, source, op);
				}
			}
			c.expect(';');
			return new RawCall(target);
		}
	}

	private int parseConstantExpression(Cursor c) {
		Expression e = parseExpression(c);
		if (!(e instanceof Constant)) {
			throw new Error("Constant expression expected!");
		}
		return ((Constant)e).value;
	}

	private Expression parseExpression(Cursor c) {
		Expression ret = parseExpression(c, 3);
		ret = collapseTree(ret);

		//System.out.println();
		//printTree(ret, 0);
		//System.out.println();
		return ret;
	}

	// This will remove superfluous nodes from the AST
	// and perform basic constant folding. It isn't just
	// an optimization pass, though, since this is where
	// AddressOf nodes for unary & cancel with derefs.
	private Expression collapseTree(Expression e) {
		for(int x = 0; x < e.children.size(); x++) {
			e.children.set(x, collapseTree(e.children.get(x)));
		}
		if (e instanceof BinaryNode) {
			Expression a = e.children.get(0);
			Expression b = e.children.get(1);
			BinOp op = ((BinaryNode)e).op;

			if (a instanceof Constant && b instanceof Constant) {
				return new Constant(op.eval(
					((Constant)a).value,
					((Constant)b).value
				));
			}
		}
		if (e instanceof AddressOf) {
			if (e.children.get(0) instanceof Deref) {
				return e.children.get(0).children.get(0);
			}
		}
		return e;
	}

	// handy for debugging:
	/*
	private void printTree(Expression e, int indent) {
		for(int x = indent - 1; x >= 0; x--) { System.out.print("\t"); }
		if (e instanceof BinaryNode) {
			System.out.println("Binary "+((BinaryNode)e).op.name);
		}
		else if (e instanceof Constant) {
			System.out.println("Constant "+((Constant)e).value);
		}
		else if (e instanceof Reference) {
			System.out.println("Reference "+((Reference)e).name);
		}
		else if (e instanceof Deref) {
			System.out.println("Deref");
		}
		else if (e instanceof AddressOf) {
			System.out.println("Address");
		}
		else if (e instanceof Call) {
			System.out.println("Call " + ((Call)e).name);
		}
		else {
			System.out.println(e.getClass());
		}
		for(Expression child : e.children) {
			printTree(child, indent + 1);
		}
	}
	*/

	private Expression parseExpression(Cursor c, int level) {
		if (level == 0) { return parseFactor(c); }
		Expression a = parseExpression(c, level - 1);
		if (c.expressionDone()) { return a; }

		for(BinOp op : BinOp.values()) {
			if (op.level == level && c.match(op.name)) {
				return new BinaryNode(op, a, parseExpression(c, level));
			}
		}
		return a;
	}

	private Expression parseFactor(Cursor c) {
		if (c.match("~")) { return new       Not(parseFactor(c)); }
		if (c.match("*")) { return new     Deref(parseFactor(c)); }
		if (c.match("&")) { return new AddressOf(parseFactor(c)); }
		if (c.match("-")) { return new BinaryNode(BinOp.Mul, new Constant(-1), parseFactor(c)); }

		if (c.isNumber()) {
			return new Constant(c.parseNumber());
		}
		if (c.at('"')) {
			Storage text = new Storage(c.parseString());
			storage.add(text);
			return parseIndexer(new Reference(text.name), c);
		}
		if (c.match("(")) {
			Expression ret = parseExpression(c);
			c.expect(')');
			return parseIndexer(ret, c);
		}
		String name = c.parseName();
		if (c.at('(')) {
			return parseIndexer(new Call(name, c), c);
		}
		else if (constants.containsKey(name)) {
			return new Constant(constants.get(name));
		}
		else {
			Expression e = parseIndexer(new Reference(name), c);
			if (!(e instanceof Deref)) { e = new Deref(e); }
			return e;
		}
	}

	private Expression parseIndexer(Expression prev, Cursor c) {
		if (c.at('[')) {
			c.expect('[');
			Expression index = parseExpression(c);
			c.expect(']');
			return new Deref(new BinaryNode(BinOp.Add, prev, index));
		}
		return prev;
	}

	// this will emit code to leave the address of
	// a given name on the data stack, either referencing
	// the locals stack or global space:

	private void emitRef(String name, int instruction) {
		if (locals.has(name)) {
			// RP @ <n> -
			rom.addConst(MakoConstants.RP);
			rom.addLoad();
			rom.addConst(locals.get(name));
			rom.addSub();
		}
		else {
			rom.add(instruction,       MakoRom.Type.Code);
			rom.add(globals.get(name), MakoRom.Type.Code);
		}
	}

	class Struct {
		String name;
		List<Storage> fields = new ArrayList<Storage>();

		Struct(Cursor c) {
			// name { storagelist }
			name = c.parseName();
			c.expect('{');
			while(!c.match("}")) {
				fields.add(new Storage(c));
			}
		}
	}
	
	class Storage {
		String name;
		int    size = 1;
		List<Integer> data = new ArrayList<Integer>();
		boolean string = false;

		Storage(String name, int size) {
			this.name = name;
			this.size = size;
		}

		Storage(String s) {
			for(char c : s.toCharArray()) {
				data.add((int)c);
			}
			data.add(0);
			size = data.size();
			string = true;
			name = String.format("string%d", stringIndex++);
		}

		Storage(Cursor c) {
			// name | name[size] | name := number | name := { array }
			name = c.parseName();
			if (c.match(":=")) {
				if (c.match("{")) {
					size = 0;
					do {
						size++;
						data.add(parseConstantExpression(c));
					} while(c.match(","));
					c.expect('}');
				}
				else {
					size = 1;
					data.add(parseConstantExpression(c));
				}
			}
			else if (c.match("[")) {
				size = parseConstantExpression(c);
				c.expect(']');
			}
			c.expect(';');
		}

		void emit() {
			globals.set(name, rom.size());
			for(int x = 0; x < size; x++) {
				rom.add(
					x < data.size() ? data.get(x)       : 0,
					string    ? MakoRom.Type.String :
					size == 1 ? MakoRom.Type.Data   : MakoRom.Type.Array
				);
			}
		}
	}

	class Function {
		String name;
		List<String> args = new ArrayList<String>();
		Block body;

		Function(Cursor c) {
			// name (arglist) {block}
			name = c.parseName();
			c.expect('(');
			if (!c.match(")")) {
				do { args.add(c.parseName()); } while(c.match(","));
				c.expect(')');
			}
			body = new Block(c);
		}
	
		void emit() {
			globals.set(name, rom.size());
			locals.pushFrame();
			for(String arg : args) {
				rom.addStr();
				locals.push(arg);
			}
			body.emit();
			locals.pop();
			if (name.equals("main")) {
				rom.addJump(-1);
			}
			else {
				rom.addReturn();
			}
		}
	}
	
	class Block {
		List<Statement> statements = new ArrayList<Statement>();
	
		Block(Cursor c) {
			// { statements; }
			c.expect('{');
			while(!c.match("}")) {
				statements.add(parseStatement(c));
			}
		}

		void emit() {
			locals.pushFrame();
			for(Statement s : statements) {
				s.emit();
			}
			locals.pop();
		}
	}

	class Local extends Statement {
		String     name;
		Expression init;
		
		Local(Cursor c) {
			// name := expression;
			name = c.parseName();
			c.match(":=");
			init = parseExpression(c);
			c.expect(';');
		}

		void emit() {
			init.emit();
			rom.addStr();
			locals.push(name);
		}
	}
	
	class Assignment extends Statement {
		Expression target;
		Expression source;
		AssignOp   op;

		Assignment(Expression target, Expression source, AssignOp op) {
			this.target = target;
			this.source = source;
			this.op     = op;
		}
	
		void emit() {
			if (op == AssignOp.Normal) {
				// <src> <target> !
				source.emit();
				target.emit();
				rom.addStor();
			}
			else {
				// <target> dup @ <src> <op> swap !
				target.emit();
				rom.addDup();
				rom.addLoad();
				source.emit();
				rom.add(op.opcode, MakoRom.Type.Code);
				rom.addSwap();
				rom.addStor();
			}
		}
	}

	class RawCall extends Statement {
		Expression target;

		RawCall(Expression target) {
			this.target = target;
			if (!(target instanceof Call)) {
				throw new Error("Loose expressions must be an assignment or function call.");
			}
		}

		void emit() {
			target.emit();
		}
	}
	
	class Conditional extends Statement {
		List<Expression> guards = new ArrayList<Expression>();
		List<Block>      blocks = new ArrayList<Block>();
		Block end = null;
		
		Conditional(Cursor c) {
			// (cond) {block} elseif (cond) {block} ... else {block}
			do {
				c.expect('(');
				guards.add(parseExpression(c));
				c.expect(')');
				blocks.add(new Block(c));
			} while(c.match("elseif"));
			if (c.match("else")) {
				end = new Block(c);
			}
		}

		void emit() {
			List<Integer> ends = new ArrayList<Integer>();
			int prevJump = -1;

			for(int x = 0; x < guards.size(); x++) {
				if (prevJump > 0) {
					rom.set(prevJump, rom.size());
				}
				guards.get(x).emit();
				prevJump = rom.addJumpZ(-1);
				blocks.get(x).emit();
				if (end != null || x < guards.size() - 1) {
					ends.add(rom.addJump(-2));
				}
			}
			rom.set(prevJump, rom.size());
			if (end != null) {
				end.emit();
			}
			for(Integer x : ends) {
				rom.set(x, rom.size());
			}
		}
	}
	
	class Loop extends Statement {
		Statement  start;
		Expression condition;
		Statement  step;
		Block      body;

		Loop(Cursor c, boolean counted) {
			if (counted) {
				// (start; cond; step) { block }
				c.expect('(');
				if (!c.match(";")) {
					start = parseStatement(c);
					if (!(start instanceof Local)) {
						throw new Error("First argument of for loop must be a variable declaration!");
					}
				}
				if (!c.match(";")) {
					condition = parseExpression(c);
					c.expect(';');
				}
				if (!c.match(";")) {
					step = parseStatement(c);
				}
				c.expect(')');
				body = new Block(c);
			}
			else {
				// (cond) { block }
				c.expect('(');
				condition = parseExpression(c);
				c.expect(')');
				body = new Block(c);
			}
		}

		void emit() {
			if (start != null) {
				locals.pushFrame();
				start.emit();
			}
			int loopHead = rom.size();
			condition.emit();
			int loopTail = rom.addJumpZ(-1);
			body.emit();
			for(Integer x : continues) {
				rom.set(x, rom.size());
			}
			continues.clear();
			if (step != null) {
				step.emit();
			}
			rom.addJump(loopHead);
			rom.set(loopTail, rom.size());
			for(Integer x : breaks) {
				rom.set(x, rom.size());
			}
			breaks.clear();
			if (start != null) {
				locals.pop();
			}
		}
	}

	class Return extends Statement {
		Expression value;

		Return(Cursor c) {
			// return; | return expression;
			if (!c.match(";")) {
				value = parseExpression(c);
				c.expect(';');
			}
		}

		void emit() {
			if (value != null) {
				value.emit();
			}
			locals.emitPop(locals.size());
			rom.addReturn();
		}
	}

	class Break extends Statement {
		Break(Cursor c) {
			c.expect(';');
		}

		void emit() {
			breaks.add(rom.addJump(-1));
		}
	}

	class Continue extends Statement {
		Continue(Cursor c) {
			c.expect(';');
		}

		void emit() {
			continues.add(rom.addJump(-1));
		}
	}

	class Sync extends Statement {
		Sync(Cursor c) {
			c.expect(';');
		}

		void emit() {
			rom.addSync();
		}
	}

	class BinaryNode extends Expression {
		BinOp op;

		BinaryNode(BinOp op, Expression a, Expression b) {
			this.op = op;
			children.add(a);
			children.add(b);
		}

		void emit() {
			children.get(0).emit();
			children.get(1).emit();
			op.emit(rom);
		}
	}

	class AddressOf extends Expression {
		AddressOf(Expression expression) {
			this.children.add(expression);
		}

		void emit() {
			throw new Error("Unable get address of non-pointer value!");
		}
	}

	class Deref extends Expression {
		Deref(Expression expression) {
			this.children.add(expression);
		}

		void emit() {
			children.get(0).emit();
			rom.addLoad();
		}
	}

	class Not extends Expression {
		Not(Expression expression) {
			this.children.add(expression);
		}

		void emit() {
			children.get(0).emit();
			rom.addNot();
		}
	}

	class Reference extends Expression {
		String name;
		
		Reference(String name) {
			if (name == "") { throw new Error("WHAT"); }
			this.name = name;
		}

		void emit() {
			emitRef(name, MakoConstants.OP_CONST);
		}
	}

	class Constant extends Expression {
		int value;

		Constant(int value) {
			this.value = value;
		}

		void emit() {
			rom.addConst(value);
		}
	}

	class Call extends Expression {
		String name;
		List<Expression> args = new ArrayList<Expression>();

		Call(String name, Cursor c) {
			// name ( args )
			this.name = name;
			c.expect('(');
			if (!c.match(")")) {
				do {
					args.add(parseExpression(c));
				} while(c.match(","));
				c.expect(')');
			}
		}
		
		void emit() {
			// push arguments onto data stack backwards,
			// so that they'll be in order when transferred
			// to the return stack after the call.
			for(int x = args.size() - 1; x >= 0; x--) {
				args.get(x).emit();
			}
			emitRef(name, MakoConstants.OP_CALL);
		}
	}
}

abstract class Expression {
	List<Expression> children = new ArrayList<Expression>();
	abstract void emit();
}

abstract class Statement {
	abstract void emit();
}

class Locals {
	private final Stack<String> locals = new Stack<String>();
	private final MakoRom rom;

	Locals(MakoRom rom)      { this.rom = rom;                   }
	boolean has(String name) { return locals.search(name) != -1; }
	void pushFrame()         { locals.push(null);                }
	void push(String name)   { locals.push(name);                }

	int get(String name)     {
		int ret = 1;
		for(int x = locals.size() - 1; x >= 0; x--) {
			if (locals.get(x) == null) { continue; }
			if (locals.get(x).equals(name)) { return ret; }
			ret++;
		}
		return -767;
	}

	int size() {
		int ret = 0;
		for(String x : locals) {
			if (x != null) { ret++; }
		}
		return ret;
	}

	void emitPop(int depth) {
		if (depth > 3) {
			// RP dup @ <n> - swap !
			rom.addConst(MakoConstants.RP);
			rom.addDup();
			rom.addLoad();
			rom.addConst(locals.search(null) - 1);
			rom.addSub();
			rom.addSwap();
			rom.addStor();
		}
		else if (depth > 0) {
			while(depth-- > 0) {
				// rdrop
				rom.addRts();
				rom.addDrop();
			}
		}
	}

	void pop() {
		emitPop(locals.search(null) - 1);
		while(locals.peek() != null) { locals.pop(); }
		locals.pop();
	}
}

class Globals {
	private final Map<String, Integer>       names = new HashMap<String, Integer>();
	private final Map<String, List<Integer>> refs  = new HashMap<String, List<Integer>>();
	private final MakoRom rom;

	Globals(MakoRom rom) {
		this.rom = rom;
	}

	void set(String name, int value) {
		if (names.containsKey(name)) {
			throw new Error("Error: Multiple declarations for '"+name+"'!");
		}
		if (refs.containsKey(name)) {
			for(Integer i : refs.get(name)) { rom.set(i, value); }
			refs.remove(name);
		}
		names.put(name, value);
		rom.label(name, value);
	}

	int get(String name) {
		if (names.containsKey(name)) {
			return names.get(name);
		}
		if (refs.containsKey(name))  {
			refs.get(name).add(rom.size());
			return -666;
		}
		refs.put(name, new ArrayList<Integer>());
		refs.get(name).add(rom.size());
		throw new Error("what");
		//System.out.println("GET REF: '"+name+"'");
		//return -777;
	}

	Set<String> undefined() {
		return refs.keySet();
	}
}

enum AssignOp {
	Normal(":=", -1),
	And   ("&=", MakoConstants.OP_AND),
	Or    ("|=", MakoConstants.OP_OR ),
	Xor   ("^=", MakoConstants.OP_XOR),
	Add   ("+=", MakoConstants.OP_ADD),
	Sub   ("-=", MakoConstants.OP_SUB),
	Mul   ("*=", MakoConstants.OP_MUL),
	Div   ("/=", MakoConstants.OP_DIV),
	Mod   ("%=", MakoConstants.OP_MOD);
	
	final String name;
	final int opcode;

	AssignOp(String name, int opcode) {
		this.name   = name;
		this.opcode = opcode;
	}
}

enum BinOp {
	Equ("=",  3, MakoConstants.OP_JUMPIF), // synthetic
	Neq("!=", 3, MakoConstants.OP_JUMPZ),
	Sge(">=", 3, MakoConstants.OP_SLT),
	Sle("<=", 3, MakoConstants.OP_SGT),
	Ror(">>", 2, MakoConstants.OP_DIV),
	Rol("<<", 2, MakoConstants.OP_MUL),

	And("&", 3, MakoConstants.OP_AND), // primitive
	Or ("|", 3, MakoConstants.OP_OR ),
	Xor("^", 3, MakoConstants.OP_XOR),
	Add("+", 2, MakoConstants.OP_ADD),
	Sub("-", 2, MakoConstants.OP_SUB),
	Mul("*", 1, MakoConstants.OP_MUL),
	Div("/", 1, MakoConstants.OP_DIV),
	Mod("%", 1, MakoConstants.OP_MOD),
	Sgt(">", 3, MakoConstants.OP_SGT),
	Slt("<", 3, MakoConstants.OP_SLT);

	final String name;
	final int level;
	final int opcode;

	BinOp(String name, int level, int opcode) {
		this.name   = name;
		this.level  = level;
		this.opcode = opcode;
	}

	void emit(MakoRom rom) {
		if (this == Ror || this == Rol) {
			// 1 - for 2 <* or /> next
			rom.addConst(1);
			rom.addSub();
			rom.addStr();
			int mark = rom.size();
			rom.addConst(2);
			rom.add(opcode, MakoRom.Type.Code);
			rom.addNext(mark);
			rom.addRts();
			rom.addDrop();
		}
		else if (this == Sge || this == Sle) {
			// << or >> not
			rom.add(opcode, MakoRom.Type.Code);
			rom.addNot();
		}
		else if (this == Equ || this == Neq) {
			// xor -if true else false then
			rom.addXor();
			rom.add(opcode,       MakoRom.Type.Code);
			rom.add(rom.size()+5, MakoRom.Type.Code);
			rom.addConst(-1);
			rom.addJump(rom.size() + 4);
			rom.addConst(0);
		}
		else {
			rom.add(opcode, MakoRom.Type.Code);
		}
	}

	int eval(int a, int b) {
		switch(this) {
			case And: return a & b;
			case Or : return a | b;
			case Xor: return a ^ b;
			case Add: return a + b;
			case Sub: return a - b;
			case Mul: return a * b;
			case Div: return a / b;

			case Ror: return a >> b;
			case Rol: return a << b;

			case Sgt: return a  > b ? -1 : 0;
			case Slt: return a  < b ? -1 : 0;
			case Equ: return a == b ? -1 : 0;
			case Neq: return a != b ? -1 : 0;
			case Sge: return a >= b ? -1 : 0;
			case Sle: return a <= b ? -1 : 0;

			case Mod: a %= b; return a < 0 ? a+b : a;

			default : throw new Error("Unhandled eval!");
		}
	}
};