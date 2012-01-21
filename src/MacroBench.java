import java.util.*;

public class MacroBench implements MakoConstants {
	
	public static void main(String[] args) {
		
		System.out.println();
		System.out.format("%-15s  %8s   %8s   %8s   %10s  %6s  %s%n",
			"test:",
			"min",
			"avg",
			"max",
			"total",
			"size",
			"time(ms)"
		);

		// make sure the JVM is warmed up:
		runTest("Pong",      "examples/Pong/Pong.fs",             true);

		runTest("StressTest","examples/StressTest/StressTest.fs", false);
		runTest("TileEngine","examples/TileEngine/TileEngine.fs", false);
		runTest("OpenWorld", "examples/OpenWorld/OpenWorld.fs",   false);
		//runTest("Bitmaps",   "examples/Bitmaps/Bitmaps.fs",       false);
		runTest("Pong",      "examples/Pong/Pong.fs",             false);
		System.out.println();
	}

	public static void runTest(String name, String filename, boolean suppress) {

		Maker compiler = new Maker();
		MakoVM vm = new MakoVM(compiler.compile(filename).toArray());

		long totalRuns = 0;
		long totalSteps = 0;
		long minSteps = Integer.MAX_VALUE;
		long maxSteps = Integer.MIN_VALUE;

		long startTime = System.currentTimeMillis();				
		for(int k : new int[]{ KEY_UP, KEY_LF, KEY_RT, KEY_DN, KEY_A }) {
			vm.keys = k;
			for(int x = 0; x < 1000; x++) {
				int trial = run(vm);
				minSteps = Math.min(minSteps, trial);
				maxSteps = Math.max(maxSteps, trial);
				totalSteps += trial;
				totalRuns++;
			}
		}
		long endTime = System.currentTimeMillis();

		if (suppress) { return; }
		System.out.format("%15s  %8d   %8d   %8d   %10d  %6d  %4.2f%n",
			name,
			minSteps,
			totalSteps / totalRuns,
			maxSteps,
			totalSteps,
			vm.m.length,
			((double)(endTime-startTime))
		);
	}

	public static int run(MakoVM vm) {
		int ret = 0;
		while(vm.m[vm.m[PC]] != OP_SYNC) {
			ret++;
			vm.tick();
		}
		vm.m[PC]++;
		return ret;
	}
}