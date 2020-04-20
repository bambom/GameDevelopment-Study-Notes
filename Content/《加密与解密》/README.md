IDA：

地址跳转快捷键：
 按 G ,输入地址：
 IDA快捷键

空格键    反汇编窗口切换文本跟图形

ESC退到上一个操作地址

G搜索地址或者符号

N重命名

分号键   注释

ALT+M  添加标签

CTRL+M 列出所有标签

CTRL +S  二进制段的开始地址结束地址

C code   光标地址出内容解析成代码

P       在函数开始处使用P，从当前地址处解析成函数

D  data解析成数据

A   ASCII解析成ASCII

U   unDefined解析成未定义的内容

X  交叉引用

F5  C伪代码

菜单栏中的搜索功能中

有ALT+T 搜索文本

ALT+B 搜索16进制 搜索opcode 如ELF文件头

打开断点列表 CTRL+ALT+B

单步步入 F7

单步不过 F8

运行到函数返回地址 CTRL+F7

运行到光标处 F4

IDC脚本

NOP指令

movs r0,r0 对应的16进制是00 00 B0 E1

在IDA中被识别成NOP指令

函数首部直接让函数返回

将函数头部的汇编指令修改成  mov pc,lr  对应的16进制0E F0 A0 E1

在IDA中被识别成RET指令

作者：给力哥
链接：https://www.jianshu.com/p/2877d3b43c00
来源：简书
著作权归作者所有。商业转载请联系作者获得授权，非商业转载请注明出处。