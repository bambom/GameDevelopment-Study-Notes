* [1、ILSpy重要的类型](#001)



<br><br><br><br><br><br><br><br><br><br><br><br><br><br>[返回目录](#001)
<h1 id="001">1、</h3>

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
	
```
