import java.util.*;
import java.io.*;

// Mako All-purpose Symbolic Instruction Code
// A compiler from a TinyBASIC dialect to Maker Forth

public class MASIC {

	static Set<Integer> defined = new HashSet<Integer>();
	static Set<String> variables = new HashSet<String>();
	static Map<String, Integer> arrays = new HashMap<String, Integer>();
	static StringBuilder data = new StringBuilder();
	static StringBuilder prog = new StringBuilder();
	static String dataName = null;

	public static void main(String[] args) throws Exception {
		Scanner in = new Scanner(new File(args[0]));
		data.append(":array grid    1271 0\n");
		data.append(":array sprites 1024 0\n");
		arrays.put("grid", 1271);
		arrays.put("sprites", 1024);
		prog.append(":include \"BasicLib.fs\"\n");
		prog.append(": main \n");
		while(in.hasNextLine()) {
			Cursor line = new Cursor(in.nextLine());
			if (line.done()) { continue; }
			if (line.isDigit()) {
				int lineNumber = line.parseNumber();
				defined.add(lineNumber);
				prog.append(": line"+lineNumber+" ");
			}
			emitStatement(line);
			prog.append("\n");
		}
		prog.append("halt");
		if (args.length < 2) {
			System.out.println(data + "" + prog);
		}
		else {
			PrintWriter out = new PrintWriter(new File(args[1]));
			out.println(data + "" + prog);
			out.close();
		}
	}

	static void emitStatement(Cursor line) {
		if (line.match("REM")) {
			// comment- do nothing.
		}
		else if (line.match("DATA")) {
			if (line.isAlpha()) {
				dataName = line.parseVar();
				arrays.put(dataName, 0);
				data.append(":data "+dataName+" ");
			}
			while(true) {
				data.append(line.parseNumber()+" ");
				if (arrays.containsKey(dataName)) {
					arrays.put(dataName, arrays.get(dataName) + 1);
				}
				if (!line.match(",")) { break; }
			}
			data.append("\n");
		}
		else if (line.match("PRINT")) {
			while(true) {
				if (line.done()) { break; }
				if (line.current() == '"') {
					prog.append('"' + line.parseString() + '"' + " prints ");
				}
				else {
					emitExpression(line);
					prog.append("print ");
				}
				if (!line.match(",")) { break; }
			}
			prog.append("cr ");
		}
		else if (line.match("INPUT")) {
			while(true) {
				prog.append("input ");
				emitVar(line);
				prog.append("! ");
				if (!line.match(",")) { break; }
			}
		}
		else if (line.match("IF")) {
			emitExpression(line);
			String rel = line.parseRelOp();
			emitExpression(line);
			line.match("THEN");
			prog.append(rel + " if ");
			emitStatement(line);
			prog.append("then ");
		}
		else if (line.match("LET")) {
			String name = line.parseVar();
			Cursor args = null;
			if (arrays.containsKey(name)) { args = line.parseParens(); }
			line.expect('=');
			emitExpression(line);
			if (arrays.containsKey(name)) {
				prog.append(name + " ");
				emitExpression(args);
				prog.append(arrays.get(name) + " 1d ! ");
			}
			else {
				if (!variables.contains(name)) {
					data.append(":var "+name+"\n");
					variables.add(name);
				}
				prog.append(name+" ! ");
			}
		}
		else if (line.match("GOTO")) {
			reference(line.parseNumber(), true);
			prog.append("goto ");
		}
		else if (line.match("GOSUB")) {
			reference(line.parseNumber(), false);
		}
		else if (line.match("POKE")) {
			emitExpression(line);
			line.expect(',');
			emitExpression(line);
			prog.append("! ");
		}
		else if (line.match("CALL")) {
			emitExpression(line);
			prog.append("exec ");
		}
		else if (line.match("RETURN")) {
			prog.append(";");
		}
		else if (line.match("END")) {
			prog.append("halt");
		}
		else if (line.match("DIM")) {
			String var = line.parseVar();
			int size = line.parseParens().parseNumber();
			data.append(":array "+var+" "+size+" 0\n");
			arrays.put(var, size);
		}
		else if (line.match("IMAGE")) {
			String var = line.parseVar();
			line.expect('=');
			String file = line.parseString();
			line.expect(',');
			int w = line.parseNumber();
			line.expect(',');
			int h = line.parseNumber();
			data.append(":image "+var+" \""+file+"\" "+w+" "+h+"\n");
			arrays.put(var, w * h * 256); // just guess on tileset size for now.
		}
		else if (line.match("SYNC")) {
			prog.append("sync ");
		}
		else {
			System.out.println(line.line);
			throw new Error("SYNTAX ERROR IN LINE!");
		}
	}

	static void emitExpression(Cursor line) {
		//(-|ε) term ((+|-) term)*
		emitTerm(line);
		while(true) {
			if (line.exprDone()) { break; }
			else if (line.match("+")) {
				emitTerm(line);
				prog.append("+ ");
			}
			else if (line.match("-")) {
				emitTerm(line);
				prog.append("- ");
			}
			else {
				System.out.println(line.line);
				throw new Error("SYNTAX ERROR IN EXPRESSION!");
			}
		}
	}

	static void emitTerm(Cursor line) {
		// factor ((*|/|%) factor)*
		emitFactor(line);
		while(true) {
			if (line.match("*")) {
				emitFactor(line);
				prog.append("* ");
			}
			else if (line.match("/")) {
				emitFactor(line);
				prog.append("/ ");
			}
			else if (line.match("%")) {
				emitFactor(line);
				prog.append("mod ");
			}
			else { break; }
		}
	}

	static void emitFactor(Cursor line) {
		// var | number | (expression) | intrinsic(expression) | array(expression)
		if      (line.match("ABS"))   { emitExpression(line.parseParens()); prog.append("abs "); }
		else if (line.match("SGN"))   { emitExpression(line.parseParens()); prog.append("sgn "); }
		else if (line.match("PEEK"))  { emitExpression(line.parseParens()); prog.append("@ "); }
		else if (line.match("VAR"))   { emitVar(line.parseParens()); }
		else if (line.match("LEN"))   { prog.append(arrays.get(line.parseParens().parseVar()) + " "); }
		else if (line.match("RND"))   { emitExpression(line.parseParens()); prog.append("rnd "); }
		else if (line.match("MAX"))   { twoArg(line); prog.append("max "); }
		else if (line.match("MIN"))   { twoArg(line); prog.append("min "); }
		else if (line.match("INKEY")) { line.parseParens(); prog.append("inkey "); }
		else if (line.isDigit())      { prog.append(line.parseNumber()+" "); }
		else if (line.isAlpha())      { emitVar(line); prog.append("@ "); }
		else if (line.at('('))        { emitExpression(line.parseParens()); }
		else {
			throw new Error("SYNTAX ERROR IN FACTOR!");
		}
	}

	static void emitVar(Cursor line) {
		String name = line.parseVar();
		if (arrays.containsKey(name)) {
			prog.append(name + " ");
			emitExpression(line.parseParens());
			prog.append(arrays.get(name) + " 1d ");
		}
		else {
			if (!variables.contains(name)) {
				data.append(":var "+name+"\n");
				variables.add(name);
			}
			prog.append(name+" ");
		}
	}

	static void twoArg(Cursor line) {
		Cursor c = line.parseParens();
		emitExpression(c);
		c.expect(',');
		emitExpression(c);
	}

	static void reference(int line, boolean tick) {
		if (!defined.contains(line)) {
			prog.append(":proto line"+line+" ");
			defined.add(line);
		}
		if (tick) { prog.append("' "); }
		prog.append("line"+line+" ");
	}
}

class Cursor {
	String line;

	Cursor(String s)   { line = s; trim(); }
	void trim()        { line = line.trim(); }
	char current()     { return line.charAt(0); }
	boolean done()     { return line.length() < 1; }
	char read()        { char c = current(); next(); return c; }
	boolean isDigit()  { return !done() && Character.isDigit(current()); }
	boolean isAlpha()  { return !done() && Character.isLetter(current()); }
	int digit()        { return Character.digit(current(), 10); }

	String skip(int n) {
		String skipped = line.substring(0, n);
		line = line.substring(n);
		return skipped;
	}
	void next()        { skip(1); }

	boolean at(char c) { return !done() && current() == c; }
	void expect(char c) {
		if (!at(c)) { fail(); }
		next();
		trim();
	}

	void fail() { throw new Error("SYNTAX ERROR."); }
	int parseNumber() {
		if (!isDigit()) { fail(); }
		int n = 0;
		while(isDigit()) {
			n = (n * 10) + digit();
			next();
		}
		trim();
		return n;
	}

	String parseString() {
		expect('"');
		String ret = "";
		while(current() != '"') { ret += read(); }
		expect('"');
		return ret;
	}

	String parseVar() {
		String ret = "";
		while(isAlpha() || at('_')) { ret += read(); }
		trim();
		return ret.replace('_', '-');
	}

	String parseRelOp() {
		if (at('=')) { read(); trim(); return "="; }
		String ret = "";
		if (at('<') || at('>')) { ret += read(); } else { fail(); }
		if (at('>') || at('=')) { ret += read(); }
		trim();
		return ret;
	}

	Cursor parseParens() {
		String ret = "";
		expect('(');
		int parens = 1;
		while(parens > 0) {
			if (current() == '(') { parens++; }
			if (current() == ')') { parens--; }
			if (parens > 0) { ret += read(); }
			else { read(); }
		}
		trim();
		return new Cursor(ret.trim());
	}

	boolean exprDone() {
		return done() || at(',') || at('<') || at('>') || at('=') ||
			line.startsWith("THEN");
	}

	boolean match(String s) {
		if (!line.startsWith(s)) { return false; }
		skip(s.length());
		trim();
		return true;
	}

}