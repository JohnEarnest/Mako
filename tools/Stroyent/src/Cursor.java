public class Cursor {
	String line;
	Cursor(String s) {
		line = s;
	}

	String skip(int n) {
		String skipped = line.substring(0, n);
		line = line.substring(n);
		return skipped;
	}

	void skipUntil(String s) {
		while(!match(s)) {
			if (done()) { throw new Error("Reached end of file while parsing."); }
			next();
		}
	}

	void trim() {
		line = line.trim();
		if (line.startsWith("//")) { skipUntil("\n"); }
		if (line.startsWith("/*")) { skipUntil("*/"); }
		line = line.trim();
	}

	void next()        { skip(1); }
	char current()     { return line.charAt(0); }
	char second()      { return line.charAt(1); }
	char read()        { char c = current(); next(); return c; }
	boolean done()     { return line.length() < 1; }
	boolean at(char c) { return !done() && current() == c; }
	boolean isDigit()  { return !done() && Character.isDigit(current()); }
	boolean isAlpha()  { return !done() && Character.isLetter(current()); }
	int digit()        { return Character.digit(current(), 10); }

	void expect(char c) {
		if (!at(c)) {
			System.out.println(line);
			throw new Error("Error while parsing- '"+c+"' expected.");
		}
		next();
		trim();
	}
	
	int parseNumber() {
		if (at('\'')) {
			next();
			int ret = (int)read();
			expect('\'');
			return ret;
		}
		if (!isDigit()) {
			System.out.println(line);
			throw new Error("Invalid number format!");
		}
		int n = 0;
		while(isDigit()) {
			n = (n * 10) + digit();
			next();
		}
		trim();
		return n;
	}

	boolean isAny(String s) {
		for(Character c : s.toCharArray()) {
			if (at(c)) { return true; }
		}
		return false;
	}

	String parseName() {
		String ret = "";
		if   (isAlpha() || isAny("_$"))              { ret += read(); }
		while(isAlpha() || isAny("_$") || isDigit()) { ret += read(); }
		trim();
		return ret;
	}

	// expressions are done when we encounter a
	// - semicolon (statement terminator)
	// - unmatched end parenthesis
	// - unmatched end bracket
	// - assignment operator
	boolean expressionDone() {
		trim();
		if (line.startsWith(";")) { return true; }
		if (line.startsWith(")")) { return true; }
		if (line.startsWith("]")) { return true; }
		if (isAny(":&|^+-*/%") && second() == '=') { return true; }
		return false;
	}

	boolean isNumber() {
		return at('\'') || isDigit();
	}

	String parseString() {
		expect('"');
		String ret = "";
		while(true) {
			if (at('"')) { break; }
			if (at('\\')) {
				next();
				if      (at('n')) { ret += '\n'; next(); }
				else if (at('t')) { ret += '\t'; next(); }
				else              { ret += read(); }
			}
			else {
				ret += read();
			}
		}
		expect('"');
		return ret;
	}

	boolean match(String s) {
		if (!line.startsWith(s)) { return false; }
		skip(s.length());
		trim();
		return true;
	}
}