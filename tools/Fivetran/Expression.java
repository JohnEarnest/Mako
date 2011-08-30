import java.util.*;

public class Expression implements MakoConstants {

	final Map<String, Variable> variables;
	final TreeNode root;

	public Expression(String line, Map<String, Variable> variables) {
		this.variables = variables;
		root = parse(new Cursor(line));
	}

	public void emit(MakoRom rom) {
		root.emit(rom);
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
			a = new Constant(cursor);
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
	}

	class Constant extends TreeNode {
		int value;

		Constant(Cursor cursor) {
			value = cursor.parseNumber();
		}

		void emit(MakoRom rom) {
			rom.add(OP_CONST, MakoRom.Type.Code);
			rom.add(value, MakoRom.Type.Code);
		}
	}

	class VariableReference extends TreeNode {
		Variable v;

		VariableReference(Cursor cursor) {
			v = new Variable(cursor.parseVarName(), variables);
		}

		void emit(MakoRom rom) {
			v.emit(rom);
			rom.add(OP_LOAD, MakoRom.Type.Code);
		}
	}

	class UnaryOp extends TreeNode {
		TreeNode a;
		int[] opcodes;

		UnaryOp(TreeNode a, char op) {
			this.a = a;
			if      (op == '-') { opcodes = new int[] { OP_CONST, -1, OP_MUL }; }
			else if (op == '+') { opcodes = new int[] {}; }
			else { throw new Error("Unknown unary operator '"+op+"'!"); }
		}

		void emit(MakoRom rom) {
			a.emit(rom);
			for(int o : opcodes) {
				rom.add(o, MakoRom.Type.Code);
			}
		}
	}

	class BinaryOp extends TreeNode {
		TreeNode a;
		TreeNode b;
		int[] opcodes;

		BinaryOp(TreeNode a, TreeNode b, char op) {
			this.a = a;
			this.b = b;
			if      (op == '+') { opcodes = new int[] { OP_ADD }; }
			else if (op == '-') { opcodes = new int[] { OP_SUB }; }
			else if (op == 'x') { opcodes = new int[] { OP_MUL }; }
			else if (op == '/') { opcodes = new int[] { OP_DIV }; }
			else if (op == '%') { opcodes = new int[] { OP_MOD }; }
			else { throw new Error("Unknown binary operator '"+op+"'!"); }
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