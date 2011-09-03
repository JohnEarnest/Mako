import java.util.*;

class Variable implements MakoConstants {
	String  name;
	Integer address; // compiled storage address
	Object  value;   // initialized value

	Map<String, Variable> variables;
	List<Expression> args = new ArrayList<Expression>();

	public Variable(String line, Map<String, Variable> variables) {
		// name [(arg [, arg2, arg3 ... ] ) ]
		this.variables = variables;
		Cursor cursor = new Cursor(line);
		name = cursor.parseVarName();

		if (cursor.at('(')) {
			Cursor subcursor = cursor.parseParens();
			while(true) {
				args.add(new Expression(subcursor.parseExpression(), variables));
				if (subcursor.done()) { break; }
				subcursor.expect(',');
			}
		}

		if (!variables.containsKey(name)) {
			variables.put(name, this);
		}
		else if (variables.get(name).args.size() != args.size()) {
			throw new Error("Inconsistent variable subscripting.");
		}
		else if (args.size() > 3) {
			throw new Error("Variables may have no more than three subscripts.");
		}

		// these temporary assignments will probably be
		// blown away by a dimension statement later:
		if      (args.size() == 0) { value = 0; }
		else if (args.size() == 1) { value = new int[1]; }
		else if (args.size() == 2) { value = new int[1][1]; }
		else if (args.size() == 3) { value = new int[1][1][1]; }
	}

	public void emitStorage(MakoRom rom) {
		if (address != null) { return; }
		address = rom.size();
		rom.label(name, address);
		if (value instanceof Integer) {
			rom.add((Integer)value, MakoRom.Type.Data);
		}
		else if (value instanceof int[][][]) {
			for(int[][] m : (int[][][])value) {
				for(int[] r : m) {
					for(int i : r) {
						rom.add(i, MakoRom.Type.Array);
					}
				}
			}
		}
		else if (value instanceof int[][]) {
			for(int[] r : (int[][])value) {
				for(int i : r) {
					rom.add(i, MakoRom.Type.Array);
				}
			}
		}
		else if (value instanceof int[]) {
			for(int i : (int[])value) {
				rom.add(i, MakoRom.Type.Array);
			}
		}
	}

	public void emit(MakoRom rom) {
		// make sure the base address of this variable
		// reference points to the canonical instance.
		if (variables.get(name) != this) {
			address = variables.get(name).address;
			value   = variables.get(name).value;
		}

		if (value instanceof Integer) {
			rom.add(OP_CONST, MakoRom.Type.Code);
			rom.add(address,  MakoRom.Type.Code);
		}
		else if (value instanceof int[][][]) {
			int[][][] v3 = (int[][][])value;
			args.get(0).emit(rom);
			rom.add(OP_CONST, MakoRom.Type.Code);
			rom.add(v3[0].length * v3[0][0].length, MakoRom.Type.Code);
			rom.add(OP_MUL, MakoRom.Type.Code);
			
			args.get(1).emit(rom);
			rom.add(OP_CONST, MakoRom.Type.Code);
			rom.add(v3[0][0].length, MakoRom.Type.Code);
			rom.add(OP_MUL, MakoRom.Type.Code);
			rom.add(OP_ADD, MakoRom.Type.Code);

			args.get(2).emit(rom);
			rom.add(OP_ADD, MakoRom.Type.Code);
			
			rom.add(OP_CONST, MakoRom.Type.Code);
			rom.add(address,  MakoRom.Type.Code);
			rom.add(OP_ADD,   MakoRom.Type.Code);
		}
		else if (value instanceof int[][]) {
			int[][] v2 = (int[][])value;
			args.get(0).emit(rom);
			rom.add(OP_CONST,     MakoRom.Type.Code);
			rom.add(v2[0].length, MakoRom.Type.Code);
			rom.add(OP_MUL,       MakoRom.Type.Code);

			args.get(1).emit(rom);
			rom.add(OP_ADD,   MakoRom.Type.Code);

			rom.add(OP_CONST, MakoRom.Type.Code);
			rom.add(address,  MakoRom.Type.Code);
			rom.add(OP_ADD,   MakoRom.Type.Code);
		}
		else if (value instanceof int[]) {
			args.get(0).emit(rom);
			rom.add(OP_CONST, MakoRom.Type.Code);
			rom.add(address,  MakoRom.Type.Code);
			rom.add(OP_ADD,   MakoRom.Type.Code);
		}
	}
}