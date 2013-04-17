import java.io.*;
import java.util.*;

/**
* Octo is a very simple high-level
* assembler for the Chip-8 virtual machine,
* partially inspired by the syntax of Forth.
*
* @author John Earnest
**/

public class Octo {

	static Queue<Object> tokens;
	static List<Integer> rom = new ArrayList<Integer>();
	static Stack<Integer> loops = new Stack<Integer>();
	static Stack<Integer> whiles = new Stack<Integer>();
	static Map<String, Integer> dict = new HashMap<String, Integer>();
	static Map<String, Integer> vars = new HashMap<String, Integer>();

	public static void main(String[] args) throws IOException {
		tokens = parse(args[0]);
		//System.out.println(tokens);
		inst(0, 0);
		while(tokens.size() > 0) {
			     if (tokens.peek() instanceof Integer) { rom.add((Integer)tokens.remove()); }
			else if (vars.containsKey(tokens.peek()))  { rom.add(vars.get(next())); }
			else                                       { compile(next()); }
		}
		jump(0x200, dict.get("main"));
		DataOutputStream out = new DataOutputStream(new FileOutputStream(args[0]+".ch8"));
		for(int x = 0; x < rom.size(); x+=2) {
			System.out.format("%02X%02X%s",
				rom.get(x)  &0xFF,
				rom.get(x+1)&0xFF,
				x % 16 == 14 ? "\n" : " "
			);
			out.write(rom.get(x));
			out.write(rom.get(x+1));
		}
		System.out.println();
		out.close();
	}

	static int     here()             { return 0x200 + rom.size(); }
	static int     reg()              { return reg(next()); }
	static int     value()            { return value(tokens.remove()); }
	static String  next()             { return tokens.remove().toString(); }
	static boolean isReg()            { return isReg(tokens.peek().toString()); }
	static void    inst(int a, int b) { rom.add(a); rom.add(b); }
	static void    imm(int a, int b)  { inst(a | ((b >> 8) & 0xF), (b & 0xFF)); }

	static void expect(String s) {
		if (!tokens.remove().equals(s)) { throw new Error("Expected '"+s+"'!"); }
	}

	static boolean isReg(String n) {
		return (n.length() == 2 && n.charAt(0) == 'v' &&
			"0123456789abcdef".indexOf(n.charAt(1)) != -1);
	}
	static int reg(String n) {
		if (!isReg(n)) { throw new Error("Register expected, got '"+n+"'"); }
		return n.charAt(1) - '0';
	}

	static void jump(int a, int b) {
		rom.set(a-0x200, 0x10 | ((b >> 8) & 0xF));
		rom.set(a-0x200+1, b & 0xFF);
	}

	static int value(Object o) {
		if (vars.containsKey(o)) { return vars.get(o); }
		if (dict.containsKey(o)) { return dict.get(o); }
		return (Integer)o;
	}

	static void cond(boolean negate) {
		int r = reg();
		String t = next();
		if (negate) {
			if      (  "==".equals(t)) { t =   "!="; }
			else if (  "!=".equals(t)) { t =   "=="; }
			else if ( "key".equals(t)) { t = "-key"; }
			else if ("-key".equals(t)) { t =  "key"; }
		}
		     if ( "key".equals(t)) { inst(0xE0 | r, 0xA1); }
		else if ("-key".equals(t)) { inst(0xE0 | r, 0x9E); }
		else if ("==".equals(t)) {
			if (isReg()) { inst(0x90 | r, (reg() << 4) | 0x0); }
			else         { inst(0x40 | r, value()); }
		}
		else if ("!=".equals(t)) {
			if (isReg()) { inst(0x50 | r, (reg() << 4) | 0x0); }
			else         { inst(0x30 | r, value()); }
		}
	}

	static void compile(String token) {
		     if (":data".equals(token))   { vars.put(next(), here()); }
		else if (":const".equals(token))  { vars.put(next(), value()); }
		else if (":".equals(token))       { dict.put(next(), here()); }
		else if (dict.containsKey(token)) { imm(0x20, dict.get(token)); }
		else if (";".equals(token))       { inst(0x00, 0xEE); }
		else if ("exit".equals(token))    { inst(0x00, 0xEE); }
		else if ("cls".equals(token))     { inst(0x00, 0xE0); }
		else if ("bcd".equals(token))     { inst(0xF0 | reg(), 0x33); }
		else if ("save".equals(token))    { inst(0xF0 | reg(), 0x55); }
		else if ("load".equals(token))    { inst(0xF0 | reg(), 0x65); }
		else if ("dt".equals(token))      { expect(":="); inst(0xF0 | reg(), 0x15); }
		else if ("st".equals(token))      { expect(":="); inst(0xF0 | reg(), 0x18); }
		else if ("if".equals(token))      { cond(false); expect("then"); }
		else if ("loop".equals(token))    { loops.push(here()); whiles.push(null); }
		else if ("while".equals(token))   { cond(true); whiles.push(here()); imm(0x10, 0); }
		else if ("jump0".equals(token))   { imm(0xB0, value()); }
		else if ("draw".equals(token))    { inst(0xD0 | reg(), (reg() << 4) | value()); }
		else if ("again".equals(token))   {
			imm(0x10, loops.pop());
			while(whiles.peek() != null) { jump(whiles.pop(), here()); }
			whiles.pop();
		}
		else if ("a".equals(token)) {
			token = next();
			if (":=".equals(token)) {
				Object o = next();
				if ("hex".equals(o)) { inst(0xF0 | reg(), 0x29); }
				else                 { imm(0xA0, value(o)); }
			}
			else if ("+=".equals(token))  { inst(0xF0 | reg(), 0x1E); }
		}
		else if (isReg(token)) {
			int t = reg(token);
			token = next();
			if (":=".equals(token)) {
				Object o = tokens.remove();
				     if (o.toString().startsWith("v")) { inst(0x80 | t, (reg(o.toString()) << 4) | 0x0); }
				else if ("rnd".equals(o))              { inst(0xC0 | t, value()); }
				else if ("key".equals(o))              { inst(0xF0 | t, 0x0A); }
				else if ( "dt".equals(o))              { inst(0xF0 | t, 0x07); }
				else                                   { inst(0x60 | t, value(o)); }
			}
			else if ("+=".equals(token)) {
				if (isReg()) { inst(0x80 | t, (reg() << 4) | 0x4); }
				else         { inst(0x70 | t, value()); }
			}
			else if ("<<".equals(token)) { for(int z = value(); z > 0; z--) { inst(0x80 | t, 0x06); }}
			else if (">>".equals(token)) { for(int z = value(); z > 0; z--) { inst(0x80 | t, 0x0E); }}
			else if ("|=".equals(token)) { inst(0x80 | t, (reg() << 4) | 0x1); }
			else if ("&=".equals(token)) { inst(0x80 | t, (reg() << 4) | 0x2); }
			else if ("^=".equals(token)) { inst(0x80 | t, (reg() << 4) | 0x3); }
			else if ("-=".equals(token)) { inst(0x80 | t, (reg() << 4) | 0x5); }
		}
		else {
			throw new Error("unknown token '"+token+"'.");
		}
	}

	static void parseToken(String token, Queue<Object> q) {
		if (token.length() == 0) { return; }
		try {
			if (token.startsWith("0x")) { q.add(Integer.parseInt(token.substring(2), 16)); return; }
			if (token.startsWith("0b")) { q.add(Integer.parseInt(token.substring(2),  2)); return; }
			q.add(Integer.parseInt(token)); return;
		}
		catch(NumberFormatException e) {}
		q.add(token);
	}

	static Queue<Object> parse(String filename) throws IOException {
		InputStream in = new FileInputStream(filename);
		Queue<Object> ret = new LinkedList<Object>();
		String t = "";
		while(true) {
			int c = in.read();
			if (c == -1) { break; }
			if (c == '#') {
				parseToken(t, ret);
				t = "";
				while(true) {
					c = in.read();
					if (c == -1 || c == '\n') { break; }
				}
			}
			else if (Character.isWhitespace(c)) {
				parseToken(t, ret);
				t = "";
			}
			else { t += (char)c; }
		}
		parseToken(t, ret);
		return ret;
	}
}