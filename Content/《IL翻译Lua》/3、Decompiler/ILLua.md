翻译优化，如果有 .  点调用的话,是对函数进行缓存。

翻译工作：

```C# 

 基于IlSpy进行翻译魔改主要有三个工作。

 一、 对ILTransform 的转换需要基于针对Lua语言进行，有些Transform的变换需要适合Lua语法。剔除或者魔改。
    例如 lua语法中不支持连续赋值 a=b=c=10 。 这种语法需要进行消除。 就可以在IL的transform中进行修改。
        这一步是重点和难点：从最原始的il栈语言到适合LuaAst的过程的Transform实现可以参考C#的，但是Lua的表达式结构比C#要简单。很多C#形式的逻辑lua也无法表达。
    需要对各个Transform有所了解的同时，移除不必要的Transform，和修改某些Transform。


 二、将经过ILTransform 转换的适合lua的得到的基于il元数据节点信息的lua语法树。 进行lua表达式翻译。修改翻译IlAst的相关
    StatementBuilder.cs
    ExpressionBuilder.cs

        这部分是对经过Transform后生成的luaAst 的il指令元数据进行翻译成lua表达式的过程


 三、lua的输出节点 CSharpOutputVisitor.cs 最终得到的ILAST 对其进行适配lua语法的输出。
  需要实现一个类似的Visitor.
        这一步是对生成的带有IL元数据LuaAst的语法输出的工具，例如： C#中{}作为函数域，而lua没有。


```