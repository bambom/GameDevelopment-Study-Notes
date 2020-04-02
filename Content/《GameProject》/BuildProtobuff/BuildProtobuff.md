 [Cmake 一些基本语法的学习](#000)
 [编译Protobuff 源码](#001)
 
 <h3 id= "000">Cmake 一些基本语法的学习 </h3>    




 <h3 id= "001">编译Protobuff 源码</h3>    
 由于适配ILR protobuff 需要做些更改。涉及到protobuff的编译，和相关protoc 工具的编译。
 
需要准备的原材料[Protobuff]() 
[ILRuntime]()

一、打开vs的编译工具
  ![](Media/vs_cmd.jpg)
  
  1)、下载Protobuff源码创建目录 install ： mkdir install。
    进入到cmake 目录下，创建编译输出的目录build: mkdir build & cd build
  ![](Media/vs_cmd_1.jpg)

  2)、 查看cmake 命令行参数 cmake -h Usage , 不同vs版本有不同的一些生成命令
  ![](Media/vs_cmd_2_cmake_h.jpg)
  ![](Media/vs_cmd_2_cmake_h_0.jpg)

  3)、Relase版本编译：cmake -G "NMake Makefiles" -DCMAKE_BUILD_TYPE=Release -Dprotobuf_BUILD_TESTS=OFF -DCMAKE_INSTALL_PREFIX=../../../../install ../..

  ![](Media/vs_cmd_3_Release.jpg)
  nmake编译结果：
  ![](Media/vs_cmd_3_Release_nmake.jpg)
  3)、Debug版本编译：cmake -G "NMake Makefiles" -DCMAKE_BUILD_TYPE=Debug -Dprotobuf_BUILD_TESTS=OFF -DCMAKE_INSTALL_PREFIX=../../../../install ../..
  ![](Media/vs_cmd_4_Debug.jpg)
   nmake 编译结果：
  ![](Media/vs_cmd_4_Debug_nmake.jpg)

  4)、Solution编译： cmake -G "Visual Studio 16 2019" -DprotoBuf_Build_TESTS=OFF -DCMAKE_INSTALL_PREFIX=../../../../install ../../
  ![](Media/vs_cmd_5_solution.jpg)
  ![](Media/vs_cmd_5_solution_build.jpg)
  ![](Media/vs_cmd_6_install.jpg)

  [# 参考文档](https://blog.csdn.net/zxng_work/article/details/78936444#%E4%B8%8B%E8%BD%BD%E4%BB%A3%E7%A0%81)