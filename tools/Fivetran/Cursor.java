public class Cursor {
	String line;

	Cursor(String s)   { line = s; }
	void trim()        { line = line.trim(); }
	void next()        { skip(1); }
	char current()     { return line.charAt(0); }
	char read()        { char c = current(); next(); return c; }
	boolean done()     { return line.length() < 1; }
	boolean at(char c) { return !done() && current() == c; }
	boolean isDigit()  { return !done() && Character.isDigit(current()); }
	boolean isAlpha()  { return !done() && Character.isLetter(current()); }
	int digit()        { return Character.digit(current(), 10); }

	void expect(char c) {
		if (!at(c)) { fail(); }
		next();
		trim();
	}

	String skip(int n)   {
		String skipped = line.substring(0, n);
		line = line.substring(n);
		return skipped;
	}
	
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

	private String matchParens() {
		trim();
		int count = 0;
		int index = 0;
		while(true) {
			if (line.charAt(index) == '(') { count++; }
			if (line.charAt(index) == ')') { count--; }
			index++;
			if (count == 0) { break; }
			if (count <  0) { fail(); }
			if (index >= line.length()) { fail(); }
		}
		return skip(index);
	}

	Cursor parseParens() {
		String paren = matchParens();
		paren = paren.substring(1, paren.length()-1).trim();
		trim();
		return new Cursor(paren);
	}

	String parseVarName() {
		if (!isAlpha()) { fail(); }
		if ("ijklmn".indexOf(current()) < 0) { fail(); }
		String name = "" + read();
		if (isAlpha() || isDigit()) { name += read(); }
		trim();
		return name;
	}
	
	String parseVar() {
		String name = parseVarName();
		if (at('(')) { name += matchParens(); }
		trim();
		return name;
	}

	String parseExpression() {
		String ret = "";
		while(true) {
			ret += read();
			if (done())  { break; }
			if (at('(')) { ret += matchParens(); }
			if (done())  { break; }
			if (at(',')) { break; }
			if (at('>') || at('=')) { break; }
		}
		trim();
		return ret;
	}

	void fail() {
		throw new Error("SYNTAX ERROR. WHOOPSIE-DAISY.");
	}

	public static void main(String[] args) {
		// skip test:
		{
			Cursor c = new Cursor("abcd");
			assert 'a' == c.current();
			c.skip(1);
			assert 'b' == c.current();
			c.skip(2);
			assert 'd' == c.current();
		}
		// basic competence test:
		{
			Cursor c = new Cursor("10 i = 4");
			assert 10 == c.parseNumber();
			assert "i".equals(c.parseVarName());
			assert c.at('=');
			c.expect('=');
			assert 4 == c.parseNumber();
			assert c.done();
		}
		// again with ludicrous whitespace:
		{
			Cursor c = new Cursor("10i=4");
			assert 10 == c.parseNumber();
			assert "i".equals(c.parseVarName());
			assert c.at('=');
			c.expect('=');
			assert 4 == c.parseNumber();
			assert c.done();
		}
		// parentheses:
		{
			Cursor c = new Cursor("() (ab) (a(b)(c))");
			assert "()".equals(c.matchParens());
			assert "(ab)".equals(c.matchParens());
			assert "(a(b)(c))".equals(c.matchParens());
		}
	}
}