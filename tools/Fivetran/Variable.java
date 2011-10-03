import java.util.*;

class Variable {
	String  name;
	Integer address; // compiled storage address
	Object  value;   // initialized value

	Map<String, Variable> variables;
	List<String> args = new ArrayList<String>();

	public Variable(String line, Map<String, Variable> variables) {
		// name [(arg [, arg2, arg3 ... ] ) ]
		this.variables = variables;
		Cursor cursor = new Cursor(line);
		name = cursor.parseVarName();

		if (cursor.at('(')) {
			Cursor subcursor = cursor.parseParens();
			while(true) {
				args.add(subcursor.parseExpression());
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
			rom.addConst(address);
		}
		else if (value instanceof int[][][]) {
			int[][][] v3 = (int[][][])value;
			new Expression(
				String.format("((%s) x %d) + ((%s) x %d) + (%s) + %s",
					args.get(0),
					v3[0].length * v3[0][0].length,
					args.get(1),
					v3[0][0].length,
					args.get(2),
					address
				),
				variables
			).emit(rom);

			/*
			args.get(0).emit(rom);

			rom.addConst(v3[0].length * v3[0][0].length);
			rom.addMul();
			
			args.get(1).emit(rom);
			rom.addConst(v3[0][0].length);
			rom.addMul();
			rom.addAdd();

			args.get(2).emit(rom);
			rom.addAdd();
			
			rom.addConst(address);
			rom.addAdd();
			*/
		}
		else if (value instanceof int[][]) {
			int[][] v2 = (int[][])value;
			new Expression(
				String.format("((%s) x %s) + (%s) + %s",
					args.get(0),
					v2[0].length,
					args.get(1),
					address
				),
				variables
			).emit(rom);

			/*
			args.get(0).emit(rom);
			rom.addConst(v2[0].length);
			rom.addMul();

			args.get(1).emit(rom);
			rom.addAdd();

			rom.addConst(address);
			rom.addAdd();
			*/
		}
		else if (value instanceof int[]) {
			new Expression(
				String.format("(%s) + %s",
					args.get(0),
					address
				),
				variables
			).emit(rom);
			
			/*
			args.get(0).emit(rom);
			rom.addConst(address);
			rom.addAdd();
			*/
		}
	}
}