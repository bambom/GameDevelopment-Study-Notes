
<h1 id="000">0、ILR </h3>

* [调用方式](#001)
* [ILR使用委托](#002)
* [ILR跨域继承](#003)
* [ILR中的反射](#004)
* [CLR重定向](#005)
* [CLR绑定](#006)
* [ILR实现原理](#007)
* [iOS IL2CPP打包注意事项](#008)
* [ILR性能优化建议](#009)
* [适应ILR的Protobuff改造](#010)
* [网络模块框架](#011)
* [IL下的逻辑热更框架](#012)

* [常见问题链接(FAQ)](https://github.com/Ourpalm/ILRuntime/tree/master/docs/source/src/v1/guide)
<h1 id="001">1、调用方式 </h3>








<br><br><br><br><br><br><br><br><br><br><br><br><br><br>[返回目录](#000)
<h1 id="002">2、ILR使用委托 </h3>

***
如果只在热更新的DLL项目中使用的委托，是不需要任何额外操作的，就跟在通常的C#里那样使用即可。<br>如果你需要将委托实例传给ILRuntime外部使用，那则根据情况，你需要额外添加适配器或者转换器。
需要注意的是，一些编译器功能也会生成将委托传出给外部使用的代码，例如：

* Linq当中where xxxx == xxx，会需要将xxx == xxx这个作为lambda表达式传给Linq.Where这个外部方法使用
* OrderBy()方法，原因同上

如果在运行时发现缺少注册某个指定类型的委托适配器或者转换器时，ILRuntime会抛出相应的异常，根据提示添加注册即可。

***

**<lable style="color:green">1、委托适配器（DelegateAdapter）</lable>**

***
如果将委托实例传出给ILRuntime外部使用，那就意味着需要将委托实例转换成真正的CLR（C#运行时）委托实例，这个过程需要动态创建CLR的委托实例。由于IL2CPP之类的AOT编译技术无法在运行时生成新的类型，所以在创建委托实例的时候ILRuntime选择了显式注册的方式，以保证问题不被隐藏到上线后才发现。

同一个参数组合的委托，只需要注册一次即可，例如：
```C#
delegate void SomeDelegate(int a, float b);

Action<int, float> act;
```
这两个委托都只需要注册一个适配器即可。 注册方法如下
```C#
appDomain.DelegateManager.RegisterMethodDelegate<int, float>();
```
如果是带返回类型的委托，例如：
```C#
delegate bool SomeFunction(int a, float b);

Func<int, float, bool> act;
```
需要按照以下方式注册
```C#
appDomain.DelegateManager.RegisterFunctionDelegate<int, float, bool>();
```

***
**<lable style="color:green">2、委托转换器（DelegateConvertor）</lable>**

ILRuntime内部是使用Action,以及Func这两个系统自带委托类型来生成的委托实例，所以如果你需要将一个不是Action或者Func类型的委托实例传到ILRuntime外部使用的话，除了委托适配器，还需要额外写一个转换器，将Action和Func转换成你真正需要的那个委托类型。

比如上面例子中的SomeFunction类型的委托，其所需的Convertor应如下实现：

```C#
app.DelegateManager.RegisterDelegateConvertor<SomeFunction>((action) =>
{
    return new SomeFunction((a, b) =>
    {
       return ((Func<int, float, bool>)action)(a, b);
    });
});
```
建议
=========
为了避免不必要的麻烦，以及后期热更出现问题，建议项目遵循以下几点：
* 尽量避免不必要的跨域委托调用
* 尽量使用Action以及Func这两个系统内置万用委托类型


















<br><br><br><br><br><br><br><br><br><br><br><br><br><br>[返回目录](#000)
<h1 id="003">3、ILR跨域继承 </h3>

如果你想在热更DLL项目当中继承一个Unity主工程里的类，或者实现一个主工程里的接口，你需要在Unity主工程中实现一个继承适配器。
方法如下：
```C#
    //你想在DLL中继承的那个类
    public abstract class ClassInheritanceTest
	{
	    public abstract void TestAbstract();
		public virtual void TestVirtual(ClassInheritanceTest a)
		{
		    
		}
	}

    //这个类就是继承适配器类
    public class ClassInheritanceAdaptor : CrossBindingAdaptor
    {
        public override Type BaseCLRType
        {
            get
            {
			    //如果你是想一个类实现多个Unity主工程的接口，这里需要return null;
                return typeof(ClassInheritanceTest);//这是你想继承的那个类
            }
        }
		
		public override Type[] BaseCLRTypes
        {
            get
            {
                //跨域继承只能有1个Adapter，因此应该尽量避免一个类同时实现多个外部接口，
                //ILRuntime虽然支持同时实现多个接口，但是一定要小心这种用法，使用不当很容易造成不可预期的问题
                //日常开发如果需要实现多个DLL外部接口，请在Unity这边先做一个基类实现那些个接口，然后继承那个基类
				//如需一个Adapter实现多个接口，请用下面这行
                //return new Type[] { typeof(IEnumerator<object>), typeof(IEnumerator), typeof(IDisposable) };
				return null;
            }
        }

        public override Type AdaptorType
        {
            get
            {
                return typeof(Adaptor);//这是实际的适配器类
            }
        }

        public override object CreateCLRInstance(ILRuntime.Runtime.Enviorment.AppDomain appdomain, ILTypeInstance instance)
        {
            return new Adaptor(appdomain, instance);//创建一个新的实例
        }

		//实际的适配器类需要继承你想继承的那个类，并且实现CrossBindingAdaptorType接口
        class Adaptor : ClassInheritanceTest, CrossBindingAdaptorType
        {
            ILTypeInstance instance;
            ILRuntime.Runtime.Enviorment.AppDomain appdomain;
            IMethod mTestAbstract;
			bool mTestAbstractGot;
            IMethod mTestVirtual;
			bool mTestVirtualGot;
            bool isTestVirtualInvoking = false;
			//缓存这个数组来避免调用时的GC Alloc
			object[] param1 = new object[1];

            public Adaptor()
            {

            }

            public Adaptor(ILRuntime.Runtime.Enviorment.AppDomain appdomain, ILTypeInstance instance)
            {
                this.appdomain = appdomain;
                this.instance = instance;
            }

            public ILTypeInstance ILInstance { get { return instance; } }
            
			//你需要重写所有你希望在热更脚本里面重写的方法，并且将控制权转到脚本里去
            public override void TestAbstract()
            {
                if(!mTestAbstractGot)
                {
                    mTestAbstract = instance.Type.GetMethod("TestAbstract", 0);
					mTestAbstractGot = true;
                }
                if (mTestAbstract != null)
                    appdomain.Invoke(mTestAbstract, instance, null);//没有参数建议显式传递null为参数列表，否则会自动new object[0]导致GC Alloc
            }

            public override void TestVirtual(ClassInheritanceTest a)
            {
                if (!mTestVirtualGot)
                {
                    mTestVirtual = instance.Type.GetMethod("TestVirtual", 1);
					mTestVirtualGot = true;
                }
				//对于虚函数而言，必须设定一个标识位来确定是否当前已经在调用中，否则如果脚本类中调用base.TestVirtual()就会造成无限循环，最终导致爆栈
                if (mTestVirtual != null && !isTestVirtualInvoking)
                {
                    isTestVirtualInvoking = true;
					param1[0] = a;
                    appdomain.Invoke(mTestVirtual, instance, a);
                    isTestVirtualInvoking = false;
                }
                else
                    base.TestVirtual(a);
            }

            public override string ToString()
            {
                IMethod m = appdomain.ObjectType.GetMethod("ToString", 0);
                m = instance.Type.GetVirtualMethod(m);
                if (m == null || m is ILMethod)
                {
                    return instance.ToString();
                }
                else
                    return instance.Type.FullName;
            }
        }
```





















<br><br><br><br><br><br><br><br><br><br><br><br><br><br>[返回目录](#000)
<h1 id="004">4、ILR中的反射 </h3>

 在脚本中使用反射其实是一个非常困难的事情。因为这需要把ILRuntime中的类型转换成一个真实的C#运行时类型，并把它们映射起来。<br>默认情况下，System.Reflection命名空间中的方法，并不可能得知ILRuntime中定义的类型，因此无法通过Type.GetType等接口取得热更DLL里面的类型。而且ILRuntime里的类型也并不是一个System.Type。<br>为了解决这个问题，ILRuntime额外实现了几个用于反射的辅助类：ILRuntimeType，ILRuntimeMethodInfo，ILRuntimeFieldInfo等，来模拟系统的类型来提供部分反射功能
通过反射获取Type。

***
在**热更DLL**当中，直接调用Type.GetType("TypeName")或者typeof(TypeName)均可以得到有效System.Type类型实例。

```C#

//在热更DLL中，以下两种方式均可以
Type t = typeof(TypeName);
Type t2 = Type.GetType("TypeName");
```
***

在**Unity主工程中**，无法通过Type.GetType来取得热更DLL内部定义的类，而只能通过以下方式得到System.Type实例：

```C#
IType type = appdomain.LoadedTypes["TypeName"];
Type t = type.ReflectedType;
```

***

**<lable style="color:green">1、通过反射创建实例</lable>**
***

在**热更DLL**当中，可以直接通过Activator来创建实例：
```C#
Type t = Type.GetType("TypeName");//或者typeof(TypeName)
//以下两种方式均可以
object instance = Activator.CreateInstance(t);
object instance = Activator.CreateInstance<TypeName>();
```

*** 

在**Unity主工程中**，无法通过Activator来创建热更DLL内类型的实例，必须通过AppDomain来创建实例：
```C#
object instance = appdomain.Instantiate("TypeName");
```

***
**<lable style="color:green">2、通过反射调用方法</lable>**

***

在**热更DLL**当中，通过反射调用方法跟通常C#用法没有任何区别

```C#
Type type = typeof(TypeName);
object instance = Activator.CreateInstance(type);
MethodInfo mi = type.GetMethod("foo");
mi.Invoke(instance, null);
```

***

在**Unity主工程中**，可以通过C#通常用法来调用，也可以通过ILRuntime自己的接口来调用，两个方式是等效的：
```C#
IType t = appdomain.LoadedTypes["TypeName"];
Type type = t.ReflectedType;

object instance = appdomain.Instantiate("TypeName");

//系统反射接口
MethodInfo mi = type.GetMethod("foo");
mi.Invoke(instance, null);

//ILRuntime的接口
IMethod m = t.GetMethod("foo", 0);
appdomain.Invoke(m, instance, null);
```

***
**<lable style="color:green">3、通过反射获取和设置Field的值</lable>**

***
在 **热更DLL** 和 **Unity主工程** 中获取和设置Field的值跟通常C#用法没有区别
```C#
Type t;
FieldInfo fi = t.GetField("field");
object val = fi.GetValue(instance);
fi.SetValue(instance, val);
```

*** 
**<lable style="color:green">4、通过反射获取Attribute标注</lable>**

在热更DLL和Unity主工程中获取Attribute标注跟通常C#用法没有区别
```C#
Type t;
FieldInfo fi = t.GetField("field");
object[] attributeArr = fi.GetCustomAttributes(typeof(SomeAttribute), false);
```

限制和注意事项
============

* 在Unity主工程中不能通过new T()的方式来创建热更工程中的类型实例















<br><br><br><br><br><br><br><br><br><br><br><br><br><br>[返回目录](#000)
<h1 id="005">5、CLR重定向 </h3>
在开发中，如ILRuntime的反射那篇文档中说的，一些依赖反射的接口是没有办法直接运行的，最典型的就是在Unity主工程中通过new T()创建热更DLL内类型的实例。
细心的朋友一定会好奇，为什么Activator.CreateInstance<Type>();这个明显内部是new T();的接口可以直接调用呢？

ILRuntime为了解决这类问题，引入了CLR重定向机制。 原理就是当IL解译器发现需要调用某个指定CLR方法时，将实际调用重定向到另外一个方法进行挟持，再在这个方法中对ILRuntime的反射的用法进行处理

刚刚提到的Activator.CreateInstance<T>的CLR重定向定义如下：
```C#
        public static StackObject* CreateInstance(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method, bool isNewObj)
        {
		    //获取泛型参数<T>的实际类型
            IType[] genericArguments = method.GenericArguments;
            if (genericArguments != null && genericArguments.Length == 1)
            {
                var t = genericArguments[0];
                if (t is ILType)//如果T是热更DLL里的类型
                {
				    //通过ILRuntime的接口来创建实例
                    return ILIntepreter.PushObject(esp, mStack, ((ILType)t).Instantiate());
                }
                else
                    return ILIntepreter.PushObject(esp, mStack, Activator.CreateInstance(t.TypeForCLR));//通过系统反射接口创建实例
            }
            else
                throw new EntryPointNotFoundException();
        }
```

要让这段代码生效，需要执行相对应的注册方法：
```C#
		foreach (var i in typeof(System.Activator).GetMethods())
		{
		    //找到名字为CreateInstance，并且是泛型方法的方法定义
			if (i.Name == "CreateInstance" && i.IsGenericMethodDefinition)
			{
				appdomain.RegisterCLRMethodRedirection(i, CreateInstance);
			}
		}
```

***
**<lable style="color:green">1、带参数的方法的重定向（DelegateAdapter）</lable>**

刚刚的例子当中，由于CreateInstance<T>方法并没有任何参数，所以需要另外一个例子来展示用法，最好的例子就是Unity的Debug.Log接口了，默认情况下，如果在DLL工程中调用该接口，是没有办法显示正确的调用堆栈的，会给开发带来一些麻烦，下面我会展示怎么通过CLR重定向来实现在Debug.Log调用中打印热更DLL中的调用堆栈

```C#
        public unsafe static StackObject* DLog(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
			//只有一个参数，所以返回指针就是当前栈指针ESP - 1
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);
			//第一个参数为ESP -1， 第二个参数为ESP - 2，以此类推
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
			//获取参数message的值
            object message = StackObject.ToObject(ptr_of_this_method, __domain, __mStack);
			//需要清理堆栈
            __intp.Free(ptr_of_this_method);
			//如果参数类型是基础类型，例如int，可以直接通过int param = ptr_of_this_method->Value获取值，
			//关于具体原理和其他基础类型如何获取，请参考ILRuntime实现原理的文档。
			
			//通过ILRuntime的Debug接口获取调用热更DLL的堆栈
            string stackTrace = __domain.DebugService.GetStackTrance(__intp);
            Debug.Log(string.Format("{0}\n{1}", format, stackTrace));

            return __ret;
        }
```

然后在通过下面的代码注册重定向即可：
```C#
appdomain.RegisterCLRMethodRedirection(typeof(Debug).GetMethod("Log"), DLog);
```













<br><br><br><br><br><br><br><br><br><br><br><br><br><br>[返回目录](#000)
<h1 id="005">6、CLR绑定 </h3>
通常情况下，如果要从热更DLL中调用Unity主工程或者Unity的接口，是需要通过反射接口来调用的，包括市面上不少其他热更方案，也是通过这种方式来对CLR方接口进行调用的。

但是这种方式有着明显的弊端，最突出的一点就是通过反射来调用接口调用效率会比直接调用低很多，再加上反射传递函数参数时需要使用 **object[]** 数组，这样不可避免的每次调用都会产生不少GC Alloc。众所周知GC Alloc高意味着在Unity中执行会存在较大的性能问题。

ILRuntime通过CLR方法绑定机制，可以选择性的对经常使用的CLR接口进行直接调用，从而尽可能的消除反射调用开销以及额外的GC Alloc 。
使用方法
---------
CLR绑定借助了ILRuntime的CLR重定向机制来实现，因为实质上也是将对CLR方法的反射调用重定向到我们自己定义的方法里面来。但是手动编写CLR重定向方法是个工作量非常巨大的事，而且要求对ILRuntime底层机制非常了解（比如如何装拆箱基础类型，怎么处理Ref/Out引用等等），因此ILRuntime提供了一个代码生成工具来自动生成CLR绑定代码。

CLR绑定代码的自动生成工具使用方法如下：
```C#
[MenuItem("ILRuntime/Generate CLR Binding Code")]
static void GenerateCLRBinding()
{
	List<Type> types = new List<Type>();
	//在List中添加你想进行CLR绑定的类型
	types.Add(typeof(int));
	types.Add(typeof(float));
	types.Add(typeof(long));
	types.Add(typeof(object));
	types.Add(typeof(string));
	types.Add(typeof(Console));
	types.Add(typeof(Array));
	types.Add(typeof(Dictionary<string, int>));
	//所有ILRuntime中的类型，实际上在C#运行时中都是ILRuntime.Runtime.Intepreter.ILTypeInstance的实例，
	//因此List<A> List<B>，如果A与B都是ILRuntime中的类型，只需要添加List<ILRuntime.Runtime.Intepreter.ILTypeInstance>即可
	types.Add(typeof(Dictionary<ILRuntime.Runtime.Intepreter.ILTypeInstance, int>));
	//第二个参数为自动生成的代码保存在何处
	ILRuntime.Runtime.CLRBinding.BindingCodeGenerator.GenerateBindingCode(types, "Assets/ILRuntime/Generated");
}
```

在CLR绑定代码生成之后，需要将这些绑定代码注册到AppDomain中才能使CLR绑定生效，但是一定要记得将CLR绑定的注册写在CLR重定向的注册后面，因为同一个方法只能被重定向一次，只有先注册的那个才能生效。

注册方法如下：
```C#
ILRuntime.Runtime.Generated.CLRBindings.Initialize(appdomain);
```


















<br><br><br><br><br><br><br><br><br><br><br><br><br><br>[返回目录](#000)
<h1 id="007">7、ILR实现原理 </h3>

ILRuntime借助Mono.Cecil库来读取DLL的PE信息，以及当中类型的所有信息，最终得到方法的IL汇编码，然后通过内置的IL解译执行虚拟机来执行DLL中的代码。

IL托管栈和托管对象栈

***

为了高性能进行运算，尤其是栈上的基础类型运算，如int,float,long之类类型的运算，直接借助C#的Stack类实现IL托管栈肯定是个非常糟糕的做法。因为这意味着每次读取和写入这些基础类型的值，都需要将他们进行装箱和拆箱操作，这个过程会非常耗时并且会产生巨量的GC Alloc，使得整个运行时执行效率非常低下。

因此ILRuntime使用unsafe代码以及非托管内存，实现了自己的IL托管栈。

ILRuntime中的所有对象都是以StackObject类来表示的，他的定义如下：
```C#
    struct StackObject
    {
        public ObjectTypes ObjectType;
        public int Value; //高32位
        public int ValueLow; //低32位
    }
    enum ObjectTypes
    {
        Null,//null
        Integer,
        Long,
        Float,
        Double,
        StackObjectReference,//引用指针，Value = 指针地址, 
        StaticFieldReference,//静态变量引用,Value = 类型Hash， ValueLow= Field的Index
        Object,//托管对象，Value = 对象Index
        FieldReference,//类成员变量引用，Value = 对象Index, ValueLow = Field的Index
        ArrayReference,//数组引用，Value = 对象Index, ValueLow = 元素的Index
    }
```
通过StackObject这个值类型，我们可以表达C#当中所有的基础类型，因为所有基础类型都可以表达为8位到64位的integer。对于非基础类型而言，我们额外需要一个List来储存他的object引用对象，而Value则可以存储这个对象在List中的Index。由此我们就可以表达C#中所有的类型了。

托管调用栈
-----------------------------
ILRuntime在进行方法调用时，需要将方法的参数先压入托管栈，然后执行完毕后需要将栈还原，并把方法返回值压入栈。

具体过程如下图所示

```
调用前:                                调用完成后:
|---------------|                     |---------------|
|     参数1     |     |-------------->|   [返回值]    |
|---------------|     |               |---------------|
|      ...      |     |               |     NULL      |
|---------------|     |               |---------------|
|     参数N     |     |               |      ...      |
|---------------|     |
|   局部变量1   |     |
|---------------|     |
|      ...      |     |
|---------------|     |
|   局部变量1   |     |
|---------------|     |
|  方法栈基址   |     |
|---------------|     |
|   [返回值]    |------
|---------------|
```
函数调用进入目标方法体后，栈指针（后面我们简称为ESP）会被指向方法栈基址那个位置，可以通过ESP-X获取到该方法的参数和方法内部申明的局部变量，在方法执行完毕后，如果有返回值，则把返回值写在方法栈基址位置即可（上图因为空间原因写在了基址后面）。

当方法体执行完毕后，ILRuntime会自动平衡托管栈，释放所有方法体占用的栈内存，然后把返回值复制到参数1的位置，这样后续代码直接取栈顶部就可以取到上次方法调用的返回值了。

1. Managed Object

|---------------|                     |---------------|
|  StackObject  |                     |  ManagedStack |
|---------------|                     |---------------|
|  Type:Object  |                     |   Slot(idx)   |
|---------------|                     |---------------|
|  Value:Index  |-------------------->|    ObjRef     |
|---------------|                     |---------------|

2.CallFrame

EnterFrame:                            LeaveFrame:
|---------------|                     |---------------|
|   Argument1   |     |-------------->|  [ReturnVal]  |
|---------------|     |               |---------------|
|      ...      |     |               |     NULL      |
|---------------|     |               |---------------|
|   ArgumentN   |     |               |      ...      |
|---------------|     |
|   LocalVar1   |     |
|---------------|     |
|      ...      |     |
|---------------|     |
|   LocalVarN   |     |
|---------------|     |
|   FrameBase   |     |
|---------------|     |
|  [ReturnVal]  |------
|---------------|


3. ValueType
Field1 - FieldN are Object Body, can be seperated from the Header, The ValueLow Field stores the pointer to Field1's StackObject
|---------------|                             |---------------|
|StackObj:Field1|                             |  StackObject  |
|---------------|                             |---------------|
|     ...       |                     /----\  | Type:ValueType|
|---------------|---------------------     -> |---------------|  
|StackObj:FieldN|                     \----/  |Value:TypeToken|
|---------------|                             |---------------|
|StackObj:ValTyp|<----Pointer at Here         |Value2:FieldPtr|
|---------------|                             |---------------|



















<br><br><br><br><br><br><br><br><br><br><br><br><br><br>[返回目录](#000)
<h1 id="008">8、IOS IL2Cpp打包注意事项 </h3>
鉴于IL2CPP的特殊性，实际在iOS的发布中可能会遇到一些问题，在这里给大家介绍几个iOS发布时可能会遇到的问题。

IL2CPP和mono的最大区别就是不能在运行时动态生成代码和类型，所以这就要求必须在编译时就完全确定需要用到的类型。

类型裁剪
--------
IL2CPP在打包时会自动对Unity工程的DLL进行裁剪，将代码中没有引用到的类型裁剪掉，以达到减小发布后ipa包的尺寸的目的。然而在实际使用过程中，很多类型有可能会被意外剪裁掉，造成运行时抛出找不到某个类型的异常。特别是通过反射等方式在编译时无法得知的函数调用，在运行时都很有可能遇到问题。

Unity提供了一个方式来告诉Unity引擎，哪些类型是不能够被剪裁掉的。具体做法就是在Unity工程的Assets目录中建立一个叫link.xml的XML文件，然后按照下面的格式指定你需要保留的类型：
```XML
<linker>
  <assembly fullname="UnityEngine" preserve="all"/>
  <assembly fullname="Assembly-CSharp">
    <namespace fullname="MyGame.Utils" preserve="all"/>
    <type fullname="MyGame.SomeClass" preserve="all"/>
  </assembly>  
</linker>
```

泛型实例
---------
每个泛型实例实际上都是一个独立的类型，`List<A>` 和 `List<B>`是两个完全没有关系的类型，这意味着，如果在运行时无法通过JIT来创建新类型的话，代码中没有直接使用过的泛型实例都会在运行时出现问题。

在ILRuntime中解决这个问题有两种方式，一个是使用CLR绑定，把用到的泛型实例都进行CLR绑定。另外一个方式是在Unity主工程中，建立一个类，然后在里面定义用到的那些泛型实例的public变量。这两种方式都可以告诉IL2CPP保留这个类型的代码供运行中使用。

因此建议大家在实际开发中，尽量使用热更DLL内部的类作为泛型参数，因为DLL内部的类型都是ILTypeInstance，只需处理一个就行了。此外如果泛型模版类就是在DLL里定义的的话，那就完全不需要进行任何处理。
















<br><br><br><br><br><br><br><br><br><br><br><br><br><br>[返回目录](#000)
<h1 id="008">9、ILR性能优化建议 </h3>
Release vs Debug
---------
ILRuntime的性能跟编译模式和Unity发布选项有着非常大的关系，要想ILRuntime发挥最高性能，需要确保以下两点：

* 热更用的DLL编译的时候一定要选择Release模式，或者开启代码优化选项，Release模式会比Debug模式的性能高至少2倍
* 关闭Development Build选项来发布Unity项目。在Editor中或者开启Development Build选项发布会开启ILRuntime的Debug框架，以提供调用堆栈行号以及调试服务，这些都会额外耗用不少性能，因此正式发布的时候可以不加载pdb文件，以节省更多内存

值类型
----------
由于值类型的特殊和ILRuntime的实现原理，目前没有办法做到直接在栈上为所有类型申请内存，因此依然只有将值类型进行装箱，然后在通过深层拷贝来模拟值类型的行为。

因此在ILRuntime中值类型的运行效率会低于引用类型，并且在赋值时可能还会产生额外的GC Alloc，因此在热更DLL当中应该尽量避免大量使用值类型

接口调用建议
-----------
为了调用方便，ILRuntime的很多接口使用了params可变参数，但是有可能会无意间忽视这个操作带来的GCAlloc，例如下面的操作：
```C#
appdomain.Invoke("MyGame.Main", "Initialize", null);
appdomain.Invoke("MyGame.Main", "Start", null, 100, 200);
```

这两个操作在调用的时候，会分别生成一个`object[0]`和`object[2]`，从而产生GC Alloc，这一点很容易被忽略。所以如果你需要在Update等性能关键的地方调用热更DLL中的方法，应该按照以下方式缓存这个参数数组：
```C#
object[] param0 = new object[0];
object[] param2 = new object[2];
IMethod m, m2;

void Start()
{
    m = appdomain.LoadedTypes["MyGame.SomeUI"].GetMethod("Update", 0);
	m2 = appdomain.LoadedTypes["MyGame.SomeUI"].GetMethod("SomethingAfterUpdate", 2);
}

void Update()
{
    appdomain.Invoke(m, null, param0);
	param2[0] = this;
	param2[1] = appdomain;
	appdomain.Invoke(m2, null, param2);
}
```

通过缓存IMethod实例以及参数列表数组，可以做到这个Update操作不会产生任何额外的GC Alloc，并且以最高的性能来执行















<br><br><br><br><br><br><br><br><br><br><br><br><br><br>[返回目录](#000)
<h1 id="010">10、适应ILR的Protobuff改造 </h3>














<br><br><br><br><br><br><br><br><br><br><br><br><br><br>[返回目录](#000)
<h1 id="011">11、网络模块框架 </h3>

目标
* 协议分层，NativeProto,HotFixProto.

执行
* 网络框架部分，需要将粘包和协议解析做分离处理，以适应热更层的协议热更

* proto生成工具做NativeProto 、 HotFixProto的分离生成

网络层： 
1、提供便捷的接口供热更层注册消息
2、Unity层的网络部分，【1、负责建立网络链接】，【2、建立接受消息的轮询】，【3、网络解析和PB的序列化反序列化】，【4、提供注册消息的接口，维护热更域中传过来的消息ID和消息回调】，【5、提供发送消息的接口，供热更域调用，unity域维护发送消息的ID】,【6、网络模块中对脚本的逻辑通过 Action 回调。 例如，断网重连时，Unity域对脚本反馈的回调。 选择重新登陆的时候对Unity域的调用】




















<br><br><br><br><br><br><br><br><br><br><br><br><br><br>[返回目录](#000)
<h1 id="012">12、IL下的逻辑热更框架 </h3>

非热更的模块系统：
* AssetManager Runtime
* AutoPath Runtime
* Player Actor Runtime
* Net Runtime
* UI Manger Runtime
* Module Kit Runtime

可热更的模块系统：
* Game Driver Script
* UI Logic Script
* Proto Struct Script
* Table Struct Script


游戏的逻辑脚本，都可以放到热更中。 因为游戏逻辑是事件驱动式的，只要吧重量级的东西放到unity层中就ok了。