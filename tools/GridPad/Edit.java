/**
* An Edit is used to represent any atomic change to a map.
* Edits store the original state of the grid before applying
* changes so they can be easily rolled back later.
**/
public class Edit {

	private final int x;			// x-offset of target
	private final int y;			// y-offset of target
	private final int[][] grid;		// reference to the map grid
	private final int[][] delta;	// change to apply
	private final int[][] orig;		// backup data

	public Edit(int x, int y, int[][] grid, int[][] delta) {
		this.x = x;
		this.y = y;
		this.grid  = grid;
		this.delta = delta;
		orig = new int[delta.length][delta[0].length];
		for(int a = 0; a < delta.length; a++) {
			for(int b = 0; b < delta[0].length; b++) {
				if (a+y >= grid.length || b+x >= grid[0].length) {
					 orig[a][b] = -2;
					delta[a][b] = -2;
				}
				else {
					orig[a][b] = grid[a+y][b+x];
				}
			}
		}
	}

	public void apply() {
		for(int a = 0; a < delta.length; a++) {
			for(int b = 0; b < delta[0].length; b++) {
				int n = delta[a][b];
				if (n < -1) { continue; }
				grid[a+y][b+x] = n;
			}
		}
	}

	public void undo() {
		for(int a = 0; a < delta.length; a++) {
			for(int b = 0; b < delta[0].length; b++) {
				if (delta[a][b] < -1) { continue; }
				grid[a+y][b+x] = orig[a][b];
			}
		}
	}
}