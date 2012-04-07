import java.util.*;

/**
* CodeMatcher is a peephole optimizer for Mako bytecode.
* It recognizes patterns within a fixed window and replaces
* them with more efficient sequences with the same semantics.
* Examples include constant folding and tail-call optimization.
*
* @author John Earnest
**/

public class CodeMatcher implements MakoConstants {

	/*
	public static void main(String[] args) {
		CodeMatcher q = new CodeMatcher(new ArrayList<Integer>());
		q.add(OP_CONST, 1);
		q.add(OP_CONST, 2);
		q.add(OP_CONST, 3);
		q.print();
		q.add(OP_ADD);
		q.print();
		q.add(OP_ADD);
		q.print();
		q.flush();
		q.print();
	}

	private void print() {
		System.out.println("CODE: ");
		for(int x = queue.size() - 1; x >= 0; x--) {
			Ops o = (Ops)queue.get(x);
			System.out.format("\t%s", o);
			for(int a = 1; a < o.size; a++) {
				x--;
				System.out.format(" %s", queue.get(x));
			}
			System.out.println();
		}
		System.out.println();
	}
	*/

	public boolean print = false;
	private final List<Integer> rom;
	private final Stack<Object> queue = new Stack<Object>();

	public CodeMatcher(List<Integer> rom) {
		this.rom = rom;
	}

	private void addSeq(Object... vals) {
		for(int x = vals.length - 1; x >= 0; x--) {
			queue.push(vals[x]);
		}
		optimize();
	}

	public void add(int op) {
		for(Ops o : Ops.values()) {
			if (o.opcode == op) { addSeq(o); return; }
		}
		throw new Error("Unknown opcode: "+ op);
	}

	public void add(int op, int arg) {
		for(Ops o : Ops.values()) {
			if (o.opcode == op) { addSeq(o, arg); return; }
		}
		throw new Error("Unknown opcode: "+ op);
	}

	public void flush() {
		optimize();
		while(queue.size() > 0) { flushLast(); }
	}

	public int mark() {
		flush();
		return rom.size();
	}

	private void flushLast() {
		List<Integer> last = new ArrayList<Integer>();
		for(int x = queue.size() - 1; x >= 0; x--) {
			Ops o = (Ops)queue.get(x);
			last.clear();
			last.add(o.opcode);
			for(int a = 1; a < o.size; a++) {
				x--;
				last.add((Integer)queue.get(x));
			}
		}
		for(int i : last) {
			queue.remove(0);
			rom.add(i);
		}
	}

	private void optimize() {
		Object A = new Object();
		Object B = new Object();
		Map<Object, Integer> r = null;

		// constant folding:
		if (null != (r = match("const +", Ops.Add, Ops.Const, A, Ops.Const, B))) {
			addSeq(Ops.Const, r.get(B) + r.get(A));
		}
		if (null != (r = match("const -", Ops.Sub, Ops.Const, A, Ops.Const, B))) {
			addSeq(Ops.Const, r.get(B) - r.get(A));
		}
		if (null != (r = match("const *", Ops.Mul, Ops.Const, A, Ops.Const, B))) {
			addSeq(Ops.Const, r.get(B) * r.get(A));
		}
		if (null != (r = match("const /", Ops.Div, Ops.Const, A, Ops.Const, B))) {
			addSeq(Ops.Const, r.get(B) / r.get(A));
		}
		if (null != (r = match("const /", Ops.Div, Ops.Const, A, Ops.Const, B))) {
			addSeq(Ops.Const, r.get(B) / r.get(A));
		}
		if (null != (r = match("const and", Ops.And, Ops.Const, A, Ops.Const, B))) {
			addSeq(Ops.Const, r.get(B) & r.get(A));
		}
		if (null != (r = match("const or", Ops.Or, Ops.Const, A, Ops.Const, B))) {
			addSeq(Ops.Const, r.get(B) | r.get(A));
		}
		if (null != (r = match("const xor", Ops.Xor, Ops.Const, A, Ops.Const, B))) {
			addSeq(Ops.Const, r.get(B) ^ r.get(A));
		}
		if (null != (r = match("const >", Ops.SGT, Ops.Const, A, Ops.Const, B))) {
			addSeq(Ops.Const, r.get(B) > r.get(A) ? -1 : 0);
		}
		if (null != (r = match("const <", Ops.SLT, Ops.Const, A, Ops.Const, B))) {
			addSeq(Ops.Const, r.get(B) < r.get(A) ? -1 : 0);
		}
		if (null != (r = match("const %", Ops.Mod, Ops.Const, A, Ops.Const, B))) {
			int a = r.get(B);
			int b = r.get(A);
			a %= b;
			addSeq(Ops.Const, a < 0 ? a+b : a);
		}
		if (null != (r = match("const not", Ops.Not, Ops.Const, A))) {
			addSeq(Ops.Const, ~r.get(A));
		}

		// algebraic identities:
		if (null != (r = match("0 C *", Ops.Mul, Ops.Const, A, Ops.Const, 0))) {
			addSeq(Ops.Const, 0);
		}
		if (null != (r = match("1 C *", Ops.Mul, Ops.Const, A, Ops.Const, 1))) {
			addSeq(Ops.Const, r.get(A));
		}
		if (null != (r = match("0 C +", Ops.Add, Ops.Const, A, Ops.Const, 0))) {
			addSeq(Ops.Const, r.get(A));
		}
		if (null != (r = match("0 *", Ops.Mul, Ops.Const, 0))) {
			addSeq(Ops.Drop);
			addSeq(Ops.Const, 0);
		}
		if (null != (r = match("1 *", Ops.Mul, Ops.Const, 1))) {}
		if (null != (r = match("0 +", Ops.Add, Ops.Const, 0))) {}
		if (null != (r = match("0 -", Ops.Sub, Ops.Const, 0))) {}

		// misc:
		if (null != (r = match("C drop", Ops.Drop, Ops.Const, A)))   {}
		if (null != (r = match("dup drop", Ops.Drop, Ops.Dup)))      {}
		if (null != (r = match("swap swap", Ops.Swap, Ops.Swap)))    {}
		if (null != (r = match(">r r>", Ops.RTS, Ops.STR)))          {}
		if (null != (r = match("r> >r", Ops.STR, Ops.RTS)))          {}
		if (null != (r = match("not if", Ops.JumpZ, A, Ops.Not))) {
			addSeq(Ops.JumpN, r.get(A));
		}
		if (null != (r = match("not -if", Ops.JumpN, A, Ops.Not))) {
			addSeq(Ops.JumpZ, r.get(A));
		}
		if (null != (r = match("tail call", Ops.Ret, Ops.Call, A))) {
			addSeq(Ops.Jump, r.get(A));
		}
	}

	private Map<Object, Integer> match(String name, Object... pattern) {
		if (queue.size() < pattern.length) { return null; }
		Map<Object, Integer> captures = new HashMap<Object, Integer>();
		for(int x = 0; x < pattern.length; x++) {
			Object a = pattern[x];
			Object b = queue.get(queue.size()-1-x);

			if (a instanceof Ops) {
				// match literal opcodes
				if (a != b) { return null; }
			}
			else if (a instanceof Integer) {
				// match literal values
				if (!(b instanceof Integer)) { return null; }
				int av = ((Integer)a).intValue();
				int bv = ((Integer)b).intValue();
				if (av != bv) { return null; }
			}
			else if (captures.containsKey(a)) {
				// match previously captured symbols
				int av = captures.get(a).intValue();
				int bv = ((Integer)b).intValue();
				if (av != bv) { return null; }
			}
			else {
				// assign a capture group
				captures.put(a, ((Integer)b));
			}
		}
		// remove the matched region
		String ret = "";
		for(int x = 0; x < pattern.length; x++) {
			ret += String.format(" %s", queue.pop());
		}
		if (print) { System.out.format("found %s: %s%n", name, ret); }
		return captures;
	}

	private static enum Ops implements MakoConstants {
		Jump(  OP_JUMP,   2, true ),
		JumpZ( OP_JUMPZ,  2, true ),
		JumpN( OP_JUMPIF, 2, true ),
		Next(  OP_NEXT,   2, true ),
		Const( OP_CONST,  2, false),
		Call(  OP_CALL,   2, false),
		Load(  OP_LOAD,   1, false),
		Stor(  OP_STOR,   1, false),
		Ret(   OP_RETURN, 1, false),
		Drop(  OP_DROP,   1, false),
		Swap(  OP_SWAP,   1, false),
		Dup(   OP_DUP,    1, false),
		Over(  OP_OVER,   1, false),
		STR(   OP_STR,    1, false),
		RTS(   OP_RTS,    1, false),
		Add(   OP_ADD,    1, false),
		Sub(   OP_SUB,    1, false),
		Mul(   OP_MUL,    1, false),
		Div(   OP_DIV,    1, false),
		Mod(   OP_MOD,    1, false),
		And(   OP_AND,    1, false),
		Or(    OP_OR,     1, false),
		Xor(   OP_XOR,    1, false),
		Not(   OP_NOT,    1, false),
		SGT(   OP_SGT,    1, false),
		SLT(   OP_SLT,    1, false),
		Sync(  OP_SYNC,   1, false);
	
		final int opcode;
		final int size;
		final boolean branch;
	
		Ops(int opcode, int size, boolean branch) {
			this.opcode = opcode;
			this.size   = size;
			this.branch = branch;
		}
	}
}