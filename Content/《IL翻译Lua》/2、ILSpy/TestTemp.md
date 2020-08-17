

进行LoopDetection前
/*  进过内联简化后的AST
			 {BlockContainer {
	Block IL_0000 (incoming: 1) {
		stloc a(ldc.i4 10)
		br IL_001c
	}

	Block IL_001c (incoming: 3) {
		if (ldloc condition) br IL_0006
		leave IL_0000 (nop)
	}

	Block IL_0006 (incoming: 1) {
		stloc a(binary.add.i4(ldloc a, ldc.i4 10))
		if (comp.i4(comp.i4.signed(ldloc a > ldc.i4 100) == ldc.i4 0)) br IL_001c
		br IL_0018
	}

	Block IL_0018 (incoming: 1) {
		stloc condition(ldc.i4 0)
		br IL_001c
	}

} at IL_0000}



LoopDetection 完成
{ILFunction Test {
	local a : System.Int32(Index=0, LoadCount=2, AddressCount=0, StoreCount=2)
	param condition : System.Boolean(Index=0, LoadCount=1, AddressCount=0, StoreCount=2)

	BlockContainer {
		Block IL_0000 (incoming: 1) {
			stloc a(ldc.i4 10)
			br IL_001c
		}

		Block IL_001c (incoming: 1) {
			BlockContainer (while-true) {
				Block IL_001c (incoming: 3) {
					if (ldloc condition) br IL_0006
					leave IL_0000 (nop)
				}

				Block IL_0006 (incoming: 1) {
					stloc a(binary.add.i4(ldloc a, ldc.i4 10))
					if (comp.i4(comp.i4.signed(ldloc a > ldc.i4 100) == ldc.i4 0)) br IL_001c
					br IL_0018
				}

				Block IL_0018 (incoming: 1) {
					stloc condition(ldc.i4 0)
					br IL_001c
				}

			}
		}

	}
}
}

Condition Detection前

{BlockContainer {
	Block IL_0000 (incoming: 1) {
		stloc a(ldc.i4 10)
		br IL_001c
	}

	Block IL_001c (incoming: 1) {
		BlockContainer (while-true) {
			Block IL_001c (incoming: 3) {
				if (ldloc condition) br IL_0006
				leave IL_001c (nop)
			}

			Block IL_0006 (incoming: 1) {
				stloc a(binary.add.i4(ldloc a, ldc.i4 10))
				if (comp.i4(comp.i4.signed(ldloc a > ldc.i4 100) == ldc.i4 0)) br IL_001c
				br IL_0018
			}

			Block IL_0018 (incoming: 1) {
				stloc condition(ldc.i4 0)
				br IL_001c
			}

		}
		leave IL_0000 (nop)
	}

} at IL_0000}


之后：
{BlockContainer {
	Block IL_0000 (incoming: 1) {
		stloc a(ldc.i4 10)
		br IL_001c
	}

	Block IL_001c (incoming: 1) {
		BlockContainer (while-true) {
			Block IL_001c (incoming: 3) {
				if (ldloc condition) br IL_0006
				leave IL_001c (nop)
			}

			Block IL_0006 (incoming: 1) {
				stloc a(binary.add.i4(ldloc a, ldc.i4 10))
				if (comp.i4(comp.i4.signed(ldloc a > ldc.i4 100) == ldc.i4 0)) br IL_001c
				br IL_0018
			}

			Block IL_0018 (incoming: 1) {
				stloc condition(ldc.i4 0)
				br IL_001c
			}

		}
		leave IL_0000 (nop)
	}

} at IL_0000}




{ILFunction Test {
	local a : System.Int32(Index=0, LoadCount=2, AddressCount=0, StoreCount=2)
	param condition : System.Boolean(Index=0, LoadCount=1, AddressCount=0, StoreCount=2)

	BlockContainer {
		Block IL_0000 (incoming: 1) {
			stloc a(ldc.i4 10)
			BlockContainer (while-true) {
				Block IL_001c (incoming: 2) {
					if (comp.i4(ldloc condition == ldc.i4 0)) leave IL_001c (nop)
					stloc a(binary.add.i4(ldloc a, ldc.i4 10))
					if (comp.i4.signed(ldloc a > ldc.i4 100)) Block IL_0019 {
						stloc condition(ldc.i4 0)
					}
					br IL_001c
				}

			}
			leave IL_0000 (nop)
		}

	}
}
}



===================================================
构建表达式 

{BlockContainer {
	Block IL_0000 (incoming: 1) {
		call WriteLine(if (ldc.i4 1) ldstr "true" else ldstr "false")
		leave IL_0000 (ldc.i4 0)
	}

} at IL_0000}

{{
	Console.WriteLine (true ? "true" : "false");
	return 0;
}
}


{BlockContainer {
	Block IL_0000 (incoming: 1) {
		stloc a(ldc.i4 10)
		BlockContainer (while) {
			Block IL_001c (incoming: 2) {
				if (ldloc condition) br IL_000b else leave IL_001c (nop)
			}

			Block IL_000b (incoming: 1) {
				stloc a(binary.add.i4(ldloc a, ldc.i4 10))
				if (comp.i4.signed(ldloc a > ldc.i4 100)) Block IL_0019 {
					stloc condition(ldc.i4 0)
				}
				br IL_001c
			}

		}
		leave IL_0000 (nop)
	}

} at IL_0000}




{Block IL_000b (incoming: 1) {
	stloc a(binary.add.i4(ldloc a, ldc.i4 10))
	if (comp.i4.signed(ldloc a > ldc.i4 100)) Block IL_0019 {
		stloc condition(ldc.i4 0)
	}
	br IL_001c
} at IL_000b}



{BlockContainer {
	Block IL_0000 (incoming: 1) {
		stloc a(ldc.i4 10)
		BlockContainer (while) {
			Block IL_001c (incoming: 2) {
				if (ldloc condition) br IL_000b else leave IL_001c (nop)
			}

			Block IL_000b (incoming: 1) {
				stloc a(binary.add.i4(ldloc a, ldc.i4 10))
				if (comp.i4.signed(ldloc a > ldc.i4 100)) Block IL_0019 {
					stloc condition(ldc.i4 0)
				}
				br IL_001c
			}

		}
		leave IL_0000 (nop)
	}

} at IL_0000}


{{
	a = 10;
	while (condition) {
		a = a + 10;
		if (a > 100) {
			condition = false;
		}
	}
}
}

{BlockContainer {
	Block IL_0000 (incoming: 1) {
		call Object..ctor(ldloc this)
		leave IL_0000 (nop)
	}

} at IL_0000}