* [1、ILSpy重要的类型](#001)



<br><br><br><br><br><br><br><br><br><br><br><br><br><br>[返回目录](#001)
<h1 id="001">1、</h3>

[IL语言入门](https://www.cnblogs.com/xiaoxiangfeizi/archive/2011/08/08/2130768.html)

TypeTreeNode.cs ILSpy的窗口节点。
```C#
	//进行反编译
		public override void Decompile(Language language, ITextOutput output, DecompilationOptions options)
		{
			language.DecompileType(TypeDefinition, output, options);
		}
```
Language.cs 每种语言的编译器。
```C#
   Language.cs
  
	 public virtual void DecompileType(ITypeDefinition type, ITextOutput output, DecompilationOptions options)
		{
			WriteCommentLine(output, TypeToString(type, includeNamespace: true));
		}

    CSharpLanguage.cs

    public override void DecompileType(ITypeDefinition type, ITextOutput output, DecompilationOptions options)
		{
			PEFile assembly = type.ParentModule.PEFile;
			CSharpDecompiler decompiler = CreateDecompiler(assembly, options);
			AddReferenceAssemblyWarningMessage(assembly, output);
			AddReferenceWarningMessage(assembly, output);
			WriteCommentLine(output, TypeToString(type, includeNamespace: true));
			WriteCode(output, options.DecompilerSettings, decompiler.Decompile(type.MetadataToken), decompiler.TypeSystem);
		}

    //SyntaxTree 语法树
    void WriteCode(ITextOutput output, DecompilerSettings settings, SyntaxTree syntaxTree, IDecompilerTypeSystem typeSystem)
	{
		syntaxTree.AcceptVisitor(new InsertParenthesesVisitor { InsertParenthesesForReadability = true });
		output.IndentationString = settings.CSharpFormattingOptions.IndentationString;
		TokenWriter tokenWriter = new TextTokenWriter(output, settings, typeSystem);
		if (output is ISmartTextOutput highlightingOutput) {
			tokenWriter = new CSharpHighlightingTokenWriter(tokenWriter, highlightingOutput);
		}
		syntaxTree.AcceptVisitor(new CSharpOutputVisitor(tokenWriter, settings.CSharpFormattingOptions));
	}

	public override void AcceptVisitor (IAstVisitor visitor)
	{
		visitor.VisitSyntaxTree (this);
	}

	//直接往硬盘上保存
	DecompilerTextView.cs  // 响应反编译的主体函数
	public void SaveToDisk(ILSpy.Language language, IEnumerable<ILSpyTreeNode> treeNodes, DecompilationOptions options, string fileName)
		{
			SaveToDisk(new DecompilationContext(language, treeNodes.ToArray(), options), fileName);
		}

	AvalonEditTextOutput.cs 往ui上输出反编译代码的代码存储类


   CSharpDecompiler.不需要显示到ILSpy上直接反编译的

   从测试用例 CorrectnessTestRunner.cs HelloWorld 探索CSharpDecomipler。
   HelloWorld.cs测试用例做的事情：
   1、先将HelloWorld.cs 编译成exe文件。 
   2、使用CSharpDecompir 成exe
   3、Tester.DecompileCSharp 进行对HelloWorld.exe的反编译。
   4、通过CSharpDecompir编译引擎获取到整个的语法树。 然后构建CSharpOutputVisitor 开始解读语法树
   5、然后构建CSharpOutputVisitor.visitor.VisitSyntaxTree (this); 开始解析语法树
	到了CSharpOutputVisitor：
	   public virtual void VisitSyntaxTree(SyntaxTree syntaxTree)
		{
			// don't do node tracking as we visit all children directly
			foreach (AstNode node in syntaxTree.Children) {
				node.AcceptVisitor(this);
				MaybeNewLinesAfterUsings(node);
			}
		}
	6、开始遍历。 在CSSharpDecompiler中 DecompileBody 解析方法体。





	解析语法树：
	1、var syntaxTree = decompiler.DecompileWholeModuleAsSingleFile(sortTypes: true);
    2、开始反编译类型 DoDecompileTypes(typeDefs, decompileRun, decompilationContext, syntaxTree);
	3、var typeDecl = DoDecompile(typeDef, decompileRun, decompilationContext.WithCurrentTypeDefinition(typeDef));
	4、DecompileBody(method, methodDecl, decompileRun, decompilationContext);
	通过ILReader可以直接得到ILAST
	5、var function = ilReader.ReadIL((MethodDefinitionHandle)method.MetadataToken, methodBody, cancellationToken: CancellationToken);
				function.CheckInvariant(ILPhase.Normal);

	ILFunction.cs 就是IL指令对应的方法体的 ILAST . 可以进一步将ILAST 转换成 C#AST
	ILReader.cs 就是将bytes转成ILAST的转换器
```

函数声明的实体名
{public static int Main (string[] args);
}

解析dll得到的il函数指令流
// (no C# code)
		IL_0000: nop
		// Console.WriteLine("Hello World!");
		IL_0001: ldstr "Hello World!"
		IL_0006: call void [mscorlib]System.Console::WriteLine(string)
		// (no C# code)
		IL_000b: nop
		// return 0;
		IL_000c: ldc.i4.0
		IL_000d: stloc.0
		// (no C# code)
		IL_000e: br.s IL_0010

		IL_0010: ldloc.0
		IL_0011: ret

IL_0000: nop
		IL_0001: ldstr "Hello World!"
		IL_0006: call void [mscorlib]System.Console::WriteLine(string)
		IL_000b: nop
		IL_000c: ldc.i4.0
		IL_000d: stloc.0
		IL_000e: br.s IL_0010

		IL_0010: ldloc.0
		IL_0011: ret


//去掉操作栈的
//第一步处理 ： 赋值取值处理 
// stloc指令表示的是从操作栈上pop一个值，赋给某个local变量;
// ldloc指令表示的是把某个local变量push到操作栈;

经过第一步 转换变量 赋值 取值，然后 划分block 
{BlockContainer {
	Block IL_0000 (incoming: 0) {
		nop
		stloc S_0(ldstr "Hello World!")
		call WriteLine(ldloc S_0)
		nop
		stloc S_1(ldc.i4 0)
		stloc CS$1$0000(ldloc S_1)
		br IL_0010
	}

	Block IL_0010 (incoming: 0) {
		stloc S_2(ldloc CS$1$0000)
		leave IL_0000 (ldloc S_2)
	}

} at IL_0000}

这时候得到的ILFunction是结构化的IL block，可以看做是IL的AST。
{ILFunction Main {
	param args : System.String[](Index=0, LoadCount=0, AddressCount=0, StoreCount=1)
	local CS$1$0000 : System.Int32(Index=0, LoadCount=1, AddressCount=0, StoreCount=2) init
	stack S_0 : System.Object(LoadCount=1, AddressCount=0, StoreCount=1)
	stack S_1 : System.Int32(LoadCount=1, AddressCount=0, StoreCount=1)
	stack S_2 : System.Int32(LoadCount=1, AddressCount=0, StoreCount=1)

	BlockContainer {
		Block IL_0000 (incoming: 1) {
			nop
			stloc S_0(ldstr "Hello World!")
			call WriteLine(ldloc S_0)
			nop
			stloc S_1(ldc.i4 0)
			stloc CS$1$0000(ldloc S_1)
			br IL_0010
		}

		Block IL_0010 (incoming: 1) {
			stloc S_2(ldloc CS$1$0000)
			leave IL_0000 (ldloc S_2)
		}

	}
}
}

进过各种Transform 后 得到这个

{ILFunction Main {

	BlockContainer {
		Block IL_0000 (incoming: 1) {
			call WriteLine(ldstr "Hello World!")
			leave IL_0000 (ldc.i4 0)
		}

	}
}
}


//然后再进行C#语言的转换，查找符号得到的函数代码
{{
	Console.WriteLine ("Hello World!");
	return 0;
}
}



====================================

{public HollowWorldTest ();
}

{BlockContainer {
	Block IL_0000 (incoming: 1) {
		call Object..ctor(ldloc this)
		leave IL_0000 (nop)
	}

} at IL_0000}


{{
	base..ctor ();
}
}



======================
public int AddNum ();
}


{ILFunction AddNum {
	local y : System.Int32(Index=1, LoadCount=1, AddressCount=0, StoreCount=1)
	local x : System.Int32(Index=0, LoadCount=1, AddressCount=0, StoreCount=1)

	BlockContainer {
		Block IL_0000 (incoming: 1) {
			stloc x(ldc.i4 32)
			stloc y(ldc.i4 18)
			leave IL_0000 (binary.add.i4(ldloc x, ldloc y))
		}
	}
}
}


{{
	x = 32;
	y = 18;
	return x + y;
}
}

=====================================================


{internal class HollowWorldTest
{
	public static int Main (string[] args)
	{
		Console.WriteLine ("Hello World!");
		return 0;
	}

	public int AddNum ()
	{
		x = 32;
		y = 18;
		return x + y;
	}

	public HollowWorldTest ()
	{
		base..ctor ();
	}
}
}


============================================
ILAST 和 C# AST对比

{public int Switch (int a);
}


{ILFunction Switch {
	local b : System.Int32(Index=0, LoadCount=1, AddressCount=0, StoreCount=2)
	param a : System.Int32(Index=0, LoadCount=1, AddressCount=0, StoreCount=1)

	BlockContainer {
		Block IL_0000 (incoming: 1) {
			stloc b(ldc.i4 0)
			BlockContainer (switch) {
				Block IL_0008 (incoming: 1) {
					switch (ldloc a) {
						case [1..2): br IL_001b
						case [2..3): br IL_001f
						case [3..4): br IL_0023
						case [long.MinValue..1),[4..long.MaxValue]: leave IL_0008 (nop)
					}
				}

				Block IL_0023 (incoming: 1) {
					call WriteLine(ldstr "222")
					leave IL_0008 (nop)
				}

				Block IL_001f (incoming: 1) {
					stloc b(ldc.i4 2)
					leave IL_0008 (nop)
				}

				Block IL_001b (incoming: 1) {
					leave IL_0000 (ldc.i4 1)
				}

			}
			leave IL_0000 (ldloc b)
		}

	}
}
}

----

{{
	b = 0;
	switch (a) {
	case 1: {
		return 1;
	}
	case 2: {
		b = 2;
		break;
	}
	case 3: {
		Console.WriteLine ("222");
		break;
	}
	}
	return b;
}
}

================================================

{public int ForFunc ();
}



{ILFunction ForFunc {
	local y : System.Int32(Index=1, LoadCount=2, AddressCount=0, StoreCount=1)
	local x : System.Int32(Index=0, LoadCount=3, AddressCount=0, StoreCount=2)

	BlockContainer {
		Block IL_0000 (incoming: 1) {
			stloc x(ldc.i4 32)
			stloc y(ldc.i4 18)
			BlockContainer (while) {
				Block IL_000f (incoming: 2) {
					if (comp.i4.signed(ldloc x > ldloc y)) br IL_000d else leave IL_000f (nop)
				}

				Block IL_000d (incoming: 1) {
					stloc x(binary.sub.i4(ldloc x, ldc.i4 1))
					br IL_000f
				}

			}
			leave IL_0000 (binary.add.i4(ldloc x, ldloc y))
		}

	}
}
}




{public int ForFunc ()
{
	x = 32;
	y = 18;
	while (x > y) {
		x = x - 1;
	}
	return x + y;
}
}



=================================================

{ILFunction Test {
	param this : ICSharpCode.Decompiler.Tests.BambomLuaTest.TestLoop(Index=-1, LoadCount=0, AddressCount=0, StoreCount=1)
	param condition : System.Boolean(Index=0, LoadCount=1, AddressCount=0, StoreCount=2)
	local a : System.Int32(Index=0, LoadCount=2, AddressCount=0, StoreCount=3) init
	local CS$4$0000 : System.Boolean(Index=1, LoadCount=2, AddressCount=0, StoreCount=3) init
	stack S_0 : System.Int32(LoadCount=1, AddressCount=0, StoreCount=1)
	stack S_1 : System.Int32(LoadCount=1, AddressCount=0, StoreCount=1)
	stack S_2 : System.Int32(LoadCount=1, AddressCount=0, StoreCount=1)
	stack S_3 : System.Int32(LoadCount=1, AddressCount=0, StoreCount=1)
	stack S_4 : System.Int32(LoadCount=1, AddressCount=0, StoreCount=1)
	stack S_5 : System.Int32(LoadCount=1, AddressCount=0, StoreCount=1)
	stack S_6 : System.Int32(LoadCount=1, AddressCount=0, StoreCount=1)
	stack S_7 : System.Int32(LoadCount=1, AddressCount=0, StoreCount=1)
	stack S_8 : System.Int32(LoadCount=1, AddressCount=0, StoreCount=1)
	stack S_9 : System.Int32(LoadCount=1, AddressCount=0, StoreCount=1)
	stack S_10 : System.Int32(LoadCount=1, AddressCount=0, StoreCount=1)
	stack S_11 : System.Int32(LoadCount=1, AddressCount=0, StoreCount=1)
	stack S_12 : System.Int32(LoadCount=1, AddressCount=0, StoreCount=1)

	BlockContainer {
		Block IL_0000 (incoming: 1) {
			nop
			stloc S_0(ldc.i4 10)
			stloc a(ldloc S_0)
			br IL_001c
		}

		Block IL_001c (incoming: 2) {
			stloc S_11(ldloc condition)
			stloc CS$4$0000(ldloc S_11)
			stloc S_12(ldloc CS$4$0000)
			if (ldloc S_12) br IL_0006
			br IL_0021
		}

		Block IL_0021 (incoming: 1) {
			leave IL_0000 (nop)
		}

		Block IL_0006 (incoming: 1) {
			nop
			stloc S_1(ldloc a)
			stloc S_2(ldc.i4 10)
			stloc S_3(binary.add.i4(ldloc S_1, ldloc S_2))
			stloc a(ldloc S_3)
			stloc S_4(ldloc a)
			stloc S_5(ldc.i4 100)
			stloc S_6(comp.i4.signed(ldloc S_4 > ldloc S_5))
			stloc S_7(ldc.i4 0)
			stloc S_8(comp.i4(ldloc S_6 == ldloc S_7))
			stloc CS$4$0000(ldloc S_8)
			stloc S_9(ldloc CS$4$0000)
			if (ldloc S_9) br IL_001b
			br IL_0018
		}

		Block IL_0018 (incoming: 1) {
			stloc S_10(ldc.i4 0)
			stloc condition(ldloc S_10)
			br IL_001b
		}

		Block IL_001b (incoming: 2) {
			nop
			br IL_001c
		}

	}
}
}






{ILFunction Test {
	local a : System.Int32(Index=0, LoadCount=2, AddressCount=0, StoreCount=2)
	param condition : System.Boolean(Index=0, LoadCount=1, AddressCount=0, StoreCount=2)

	BlockContainer {
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

	}
}
}

由上面这个转换成C# 语法树 ，主要是StatementBuilder. 所以关键是这个