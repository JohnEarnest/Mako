import java.util.*;

public class Expression implements MakoConstants {

	final String line;
	final Map<String, Variable> variables;
	TreeNode root;

	public Expression(String line, Map<String, Variable> variables) {
		this.line = line;
		this.variables = variables;
		root = parse(new Cursor(fixPrecedence(line)));
		root = root.simplify();
	}

	public void emit(MakoRom rom) {
		root.emit(rom);
	}

	// Re-parenthesize expressions so that they can be
	// correctly parsed in a left-to-right fashion.
	// This is similar to the approach taken by the
	// original FORTAN 1 compiler:

	String fixPrecedence(String expression) {
		StringBuilder ret = new StringBuilder();
		ret.append("((((");

		for(int index = 0; index < expression.length(); index++) {
			char c = expression.charAt(index);
			char p = (index > 0) ? expression.charAt(index-1) : 0;

			if ("ijklmn".indexOf(c) >= 0) {
				// if we encounter a variable name, we must be
				// careful to avoid treating any subscripts as if they
				// were normal parentheses:
				String var = new Cursor(expression.substring(index)).parseVar();
				ret.append(var);
				index += var.length()-1;
			}
			else if (c == '(') { ret.append("((((");  }
			else if (c == ')') { ret.append("))))");  }
			else if (c == 'x') { ret.append("))x(("); }
			else if (c == '/') { ret.append("))/(("); }
			else if (c == '%') { ret.append("))%(("); }
			else if (c == '+') {
				if (index == 0 || "(x/%+-".indexOf(p) >= 0) { ret.append("+"); }
				else { ret.append(")))+((("); }
			}
			else if (c == '-') {
				if (index == 0 || "(x/%+-".indexOf(p) >= 0) { ret.append("-"); }
				else { ret.append(")))-((("); }
			}
			else { ret.append(c); }
		}

		ret.append("))))");
		//System.out.format("'%s' --> '%s'%n", expression, ret);
		return ret.toString();
	}

	TreeNode parse(Cursor cursor) {
		if ("-".indexOf(cursor.current()) >= 0) {
			char op = cursor.current();
			cursor.expect(op);
			return new UnaryOp(parse(cursor), op);
		}

		TreeNode a;
		if (cursor.isDigit()) {
			// constant
			a = new Constant(cursor.parseNumber());
			if (cursor.done()) { return a; }
		}
		else if (cursor.at('(')) {
			// parenthesized subexpression
			a = parse(cursor.parseParens());
			if (cursor.done()) { return a; }
		}
		else {
			// variable reference
			a = new VariableReference(cursor);
			if (cursor.done()) { return a; }
		}

		char op = cursor.current();
		cursor.expect(op);
		TreeNode b = parse(cursor);
		return new BinaryOp(a, b, op);
	}

	abstract class TreeNode {
		abstract void emit(MakoRom rom);
		TreeNode simplify() { return this; }
	}

	class Constant extends TreeNode {
		int value;

		Constant(int value) {
			this.value = value;
		}

		void emit(MakoRom rom) {
			rom.addConst(value);
		}
	}

	class VariableReference extends TreeNode {
		Variable v;

		VariableReference(Cursor cursor) {
			v = new Variable(cursor.parseVar(), variables);
		}

		void emit(MakoRom rom) {
			v.emit(rom);
			rom.addLoad();
		}
	}

	class UnaryOp extends TreeNode {
		TreeNode a;
		char op;
		int[] opcodes;

		UnaryOp(TreeNode a, char op) {
			this.a = a;
			this.op = op;
		}

		TreeNode simplify() {
			a = a.simplify();
			if (a instanceof Constant && op == '-') {
				Constant child = (Constant)a;
				return new Constant(child.value * -1);
			}
			return this;
		}

		void emit(MakoRom rom) {
			a.emit(rom);
			if (op == '-') {
				rom.addConst(-1);
				rom.addMul();
			}
		}
	}

	class BinaryOp extends TreeNode {
		TreeNode a;
		TreeNode b;
		char op;
		int[] opcodes;

		BinaryOp(TreeNode a, TreeNode b, char op) {
			this.a = a;
			this.b = b;
			this.op = op;
			if      (op == '+') { opcodes = new int[] { OP_ADD }; }
			else if (op == '-') { opcodes = new int[] { OP_SUB }; }
			else if (op == 'x') { opcodes = new int[] { OP_MUL }; }
			else if (op == '/') { opcodes = new int[] { OP_DIV }; }
			else if (op == '%') { opcodes = new int[] { OP_MOD }; }
			else { throw new Error("Unknown binary operator '"+op+"'!"); }
		}

		TreeNode simplify() {
			a = a.simplify();
			b = b.simplify();
			if (a instanceof Constant && b instanceof Constant) {
				int av = ((Constant)a).value;
				int bv = ((Constant)b).value;
				if (op == '+') { return new Constant(av + bv); }
				if (op == '-') { return new Constant(av - bv); }
				if (op == 'x') { return new Constant(av * bv); }
				if (op == '/') { return new Constant(av / bv); }
				if (op == '%') {
					av %= bv;
					return new Constant(av < 0 ? av+bv : av);
				}
			}
			return this;
		}

		void emit(MakoRom rom) {
			a.emit(rom);
			b.emit(rom);
			for(int o : opcodes) {
				rom.add(o, MakoRom.Type.Code);
			}
		}
	}
}