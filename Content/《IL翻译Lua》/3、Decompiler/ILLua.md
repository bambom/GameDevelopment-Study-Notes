现有的翻译方案

C# 源码 ,民间高手版  ---> Lua
    缺点: 工作量大。受语言语法的限制不够稳定。代码复杂度高，不好把控。
    优点：有开源代码

Dll反编译，腾讯网易内部版 ---> Lua
    缺点： 没有开源代码。需要自己实现。
    优点： 翻译难度比C#源码形式小， 可以享受dll的相关便利，编译优化，dll混淆。 编译向其他语言进行翻译发展.



C# 源码形式：
```C# 

  借助Roslyn获取C#AST,将C#AST转换成LuaAST.  
  主要解决的问题是通过Roslyn得到的AST是完整C#源码表达式的AST， 而C#表达式很多形式Lua表达式无法表达。
  需要做一系列的对表达式的Transform，涵盖C#版本各种语法表达式

```


Dll反编译形式：
借助成熟的ILSpy相关工具进行逆向AST构建。
反编译形式相对于C#源码形式，是一个逆过程，通过il指令一步步逆向得到原始的ILAst ，然后对ILAst进行转换得到LuaAst。 只关注Function的翻译。
借助ILSpy，分析dll字节码，得到最原始的带有元数据信息的il指令流，构建AST： 去栈化  --> 以跳转划分block ---> inling分析 、控制流程分析......... ---> ILAst

需要做的工作主要有两个难点： 
    1、基于ILSpy逆向构建C#Ast的Transform 很多lua不需要，需要屏蔽，有些需要适合lua语法需要修改。
    2、基于ILSpy得出的 IL语法形式的C#Ast 在转换成C#的表达式的翻译过程 需要修改成 翻译成 Lua表达式的过程。

```C# 
 基于IlSpy进行翻译魔改主要有三个工作。

 一、 对ILTransform 的转换需要基于针对Lua语言进行，有些Transform的变换需要适合Lua语法。剔除或者魔改。
    例如 lua语法中不支持连续赋值 a=b=c=10 。 这种语法需要进行消除。 就可以在IL的transform中进行修改。
        这一步是重点和难点：从最原始的il栈语言到适合LuaAst的过程的Transform实现可以参考C#的，但是Lua的表达式结构比C#要简单。很多C#形式的逻辑lua也无法表达。
    需要对各个ILTransform有所了解的同时，移除不必要的Transform，和修改某些Transform，从零开始的话， 难点在于理解Transform的逻辑，然后取舍修改，也是工作量的问题。


 二、将经过ILTransform 转换的适合lua的得到的基于il元数据节点信息的lua语法树。 进行lua表达式翻译。修改翻译IlAst的相关
    StatementBuilder.cs
    ExpressionBuilder.cs

        这部分是对经过Transform后生成的luaAst 的il指令元数据进行翻译成lua表达式的过程， 从零开始的话也是工作量问题。

 三、lua的输出节点 CSharpOutputVisitor.cs 最终得到的ILAST 对其进行适配lua语法的输出。
  需要实现一个类似的Visitor.
        这一步是对生成的带有IL元数据LuaAst的语法输出的工具，例如： C#中{}作为函数域，而lua没有。

```


翻译优化，如果有 .  点调用的话,是对函数进行缓存。