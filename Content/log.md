Connected to server
[2020-02-04 09:02:35,105] Artifact test:Web exploded: Artifact is being deployed, please wait...
二月 04, 2020 9:02:35 下午 org.apache.catalina.deploy.WebXml setVersion
警告: Unknown version string [3.1]. Default version will be used.
二月 04, 2020 9:02:35 下午 org.apache.catalina.core.ContainerBase addChildInternal
严重: ContainerBase.addChild: start: 
org.apache.catalina.LifecycleException: Failed to start component [StandardEngine[Catalina].StandardHost[localhost].StandardContext[/test_Web_exploded]]
	at org.apache.catalina.util.LifecycleBase.start(LifecycleBase.java:162)
	at org.apache.catalina.core.ContainerBase.addChildInternal(ContainerBase.java:1018)
	at org.apache.catalina.core.ContainerBase.addChild(ContainerBase.java:994)
	at org.apache.catalina.core.StandardHost.addChild(StandardHost.java:652)
	at org.apache.catalina.startup.HostConfig.manageApp(HostConfig.java:1899)
	at sun.reflect.NativeMethodAccessorImpl.invoke0(Native Method)
	at sun.reflect.NativeMethodAccessorImpl.invoke(NativeMethodAccessorImpl.java:57)
	at sun.reflect.DelegatingMethodAccessorImpl.invoke(DelegatingMethodAccessorImpl.java:43)
	at java.lang.reflect.Method.invoke(Method.java:606)
	at org.apache.tomcat.util.modeler.BaseModelMBean.invoke(BaseModelMBean.java:301)
	at com.sun.jmx.interceptor.DefaultMBeanServerInterceptor.invoke(DefaultMBeanServerInterceptor.java:819)
	at com.sun.jmx.mbeanserver.JmxMBeanServer.invoke(JmxMBeanServer.java:801)
	at org.apache.catalina.mbeans.MBeanFactory.createStandardContext(MBeanFactory.java:619)
	at org.apache.catalina.mbeans.MBeanFactory.createStandardContext(MBeanFactory.java:566)
	at sun.reflect.NativeMethodAccessorImpl.invoke0(Native Method)
	at sun.reflect.NativeMethodAccessorImpl.invoke(NativeMethodAccessorImpl.java:57)
	at sun.reflect.DelegatingMethodAccessorImpl.invoke(DelegatingMethodAccessorImpl.java:43)
	at java.lang.reflect.Method.invoke(Method.java:606)
	at org.apache.tomcat.util.modeler.BaseModelMBean.invoke(BaseModelMBean.java:301)
	at com.sun.jmx.interceptor.DefaultMBeanServerInterceptor.invoke(DefaultMBeanServerInterceptor.java:819)
	at com.sun.jmx.mbeanserver.JmxMBeanServer.invoke(JmxMBeanServer.java:801)
	at com.sun.jmx.remote.security.MBeanServerAccessController.invoke(MBeanServerAccessController.java:468)
	at javax.management.remote.rmi.RMIConnectionImpl.doOperation(RMIConnectionImpl.java:1487)
	at javax.management.remote.rmi.RMIConnectionImpl.access$300(RMIConnectionImpl.java:97)
	at javax.management.remote.rmi.RMIConnectionImpl$PrivilegedOperation.run(RMIConnectionImpl.java:1328)
	at java.security.AccessController.doPrivileged(Native Method)
	at javax.management.remote.rmi.RMIConnectionImpl.doPrivilegedOperation(RMIConnectionImpl.java:1427)
	at javax.management.remote.rmi.RMIConnectionImpl.invoke(RMIConnectionImpl.java:848)
	at sun.reflect.NativeMethodAccessorImpl.invoke0(Native Method)
	at sun.reflect.NativeMethodAccessorImpl.invoke(NativeMethodAccessorImpl.java:57)
	at sun.reflect.DelegatingMethodAccessorImpl.invoke(DelegatingMethodAccessorImpl.java:43)
	at java.lang.reflect.Method.invoke(Method.java:606)
	at sun.rmi.server.UnicastServerRef.dispatch(UnicastServerRef.java:322)
	at sun.rmi.transport.Transport$2.run(Transport.java:202)
	at sun.rmi.transport.Transport$2.run(Transport.java:199)
	at java.security.AccessController.doPrivileged(Native Method)
	at sun.rmi.transport.Transport.serviceCall(Transport.java:198)
	at sun.rmi.transport.tcp.TCPTransport.handleMessages(TCPTransport.java:567)
	at sun.rmi.transport.tcp.TCPTransport$ConnectionHandler.run0(TCPTransport.java:828)
	at sun.rmi.transport.tcp.TCPTransport$ConnectionHandler.access$400(TCPTransport.java:619)
	at sun.rmi.transport.tcp.TCPTransport$ConnectionHandler$1.run(TCPTransport.java:684)
	at sun.rmi.transport.tcp.TCPTransport$ConnectionHandler$1.run(TCPTransport.java:681)
	at java.security.AccessController.doPrivileged(Native Method)
	at sun.rmi.transport.tcp.TCPTransport$ConnectionHandler.run(TCPTransport.java:681)
	at java.util.concurrent.ThreadPoolExecutor.runWorker(ThreadPoolExecutor.java:1145)
	at java.util.concurrent.ThreadPoolExecutor$Worker.run(ThreadPoolExecutor.java:615)
	at java.lang.Thread.run(Thread.java:745)
Caused by: java.lang.UnsupportedClassVersionError: org/springframework/web/SpringServletContainerInitializer : Unsupported major.minor version 52.0 (unable to load class org.springframework.web.SpringServletContainerInitializer)
	at org.apache.catalina.loader.WebappClassLoaderBase.findClassInternal(WebappClassLoaderBase.java:3255)
	at org.apache.catalina.loader.WebappClassLoaderBase.findClass(WebappClassLoaderBase.java:1420)
	at org.apache.catalina.loader.WebappClassLoaderBase.loadClass(WebappClassLoaderBase.java:1924)
	at org.apache.catalina.loader.WebappClassLoaderBase.loadClass(WebappClassLoaderBase.java:1798)
	at java.lang.Class.forName0(Native Method)
	at java.lang.Class.forName(Class.java:274)
	at org.apache.catalina.startup.WebappServiceLoader.loadServices(WebappServiceLoader.java:197)
	at org.apache.catalina.startup.WebappServiceLoader.load(WebappServiceLoader.java:158)
	at org.apache.catalina.startup.ContextConfig.processServletContainerInitializers(ContextConfig.java:1579)
	at org.apache.catalina.startup.ContextConfig.webConfig(ContextConfig.java:1273)
	at org.apache.catalina.startup.ContextConfig.configureStart(ContextConfig.java:881)
	at org.apache.catalina.startup.ContextConfig.lifecycleEvent(ContextConfig.java:388)
	at org.apache.catalina.util.LifecycleSupport.fireLifecycleEvent(LifecycleSupport.java:117)
	at org.apache.catalina.util.LifecycleBase.fireLifecycleEvent(LifecycleBase.java:90)
	at org.apache.catalina.core.StandardContext.startInternal(StandardContext.java:5606)
	at org.apache.catalina.util.LifecycleBase.start(LifecycleBase.java:145)
	... 46 more

二月 04, 2020 9:02:35 下午 org.apache.tomcat.util.modeler.BaseModelMBean invoke
严重: Exception invoking method manageApp
java.lang.IllegalStateException: ContainerBase.addChild: start: org.apache.catalina.LifecycleException: Failed to start component [StandardEngine[Catalina].StandardHost[localhost].StandardContext[/test_Web_exploded]]
	at org.apache.catalina.core.ContainerBase.addChildInternal(ContainerBase.java:1022)
	at org.apache.catalina.core.ContainerBase.addChild(ContainerBase.java:994)
	at org.apache.catalina.core.StandardHost.addChild(StandardHost.java:652)
	at org.apache.catalina.startup.HostConfig.manageApp(HostConfig.java:1899)
	at sun.reflect.NativeMethodAccessorImpl.invoke0(Native Method)
	at sun.reflect.NativeMethodAccessorImpl.invoke(NativeMethodAccessorImpl.java:57)
	at sun.reflect.DelegatingMethodAccessorImpl.invoke(DelegatingMethodAccessorImpl.java:43)
	at java.lang.reflect.Method.invoke(Method.java:606)
	at org.apache.tomcat.util.modeler.BaseModelMBean.invoke(BaseModelMBean.java:301)
	at com.sun.jmx.interceptor.DefaultMBeanServerInterceptor.invoke(DefaultMBeanServerInterceptor.java:819)
	at com.sun.jmx.mbeanserver.JmxMBeanServer.invoke(JmxMBeanServer.java:801)
	at org.apache.catalina.mbeans.MBeanFactory.createStandardContext(MBeanFactory.java:619)
	at org.apache.catalina.mbeans.MBeanFactory.createStandardContext(MBeanFactory.java:566)
	at sun.reflect.NativeMethodAccessorImpl.invoke0(Native Method)
	at sun.reflect.NativeMethodAccessorImpl.invoke(NativeMethodAccessorImpl.java:57)
	at sun.reflect.DelegatingMethodAccessorImpl.invoke(DelegatingMethodAccessorImpl.java:43)
	at java.lang.reflect.Method.invoke(Method.java:606)
	at org.apache.tomcat.util.modeler.BaseModelMBean.invoke(BaseModelMBean.java:301)
	at com.sun.jmx.interceptor.DefaultMBeanServerInterceptor.invoke(DefaultMBeanServerInterceptor.java:819)
	at com.sun.jmx.mbeanserver.JmxMBeanServer.invoke(JmxMBeanServer.java:801)
	at com.sun.jmx.remote.security.MBeanServerAccessController.invoke(MBeanServerAccessController.java:468)
	at javax.management.remote.rmi.RMIConnectionImpl.doOperation(RMIConnectionImpl.java:1487)
	at javax.management.remote.rmi.RMIConnectionImpl.access$300(RMIConnectionImpl.java:97)
	at javax.management.remote.rmi.RMIConnectionImpl$PrivilegedOperation.run(RMIConnectionImpl.java:1328)
	at java.security.AccessController.doPrivileged(Native Method)
	at javax.management.remote.rmi.RMIConnectionImpl.doPrivilegedOperation(RMIConnectionImpl.java:1427)
	at javax.management.remote.rmi.RMIConnectionImpl.invoke(RMIConnectionImpl.java:848)
	at sun.reflect.NativeMethodAccessorImpl.invoke0(Native Method)
[2020-02-04 09:02:35,439] Artifact test:Web exploded: Error during artifact deployment. See server log for details.
	at sun.reflect.NativeMethodAccessorImpl.invoke(NativeMethodAccessorImpl.java:57)
	at sun.reflect.DelegatingMethodAccessorImpl.invoke(DelegatingMethodAccessorImpl.java:43)
	at java.lang.reflect.Method.invoke(Method.java:606)
	at sun.rmi.server.UnicastServerRef.dispatch(UnicastServerRef.java:322)
	at sun.rmi.transport.Transport$2.run(Transport.java:202)
	at sun.rmi.transport.Transport$2.run(Transport.java:199)
	at java.security.AccessController.doPrivileged(Native Method)
	at sun.rmi.transport.Transport.serviceCall(Transport.java:198)
	at sun.rmi.transport.tcp.TCPTransport.handleMessages(TCPTransport.java:567)
	at sun.rmi.transport.tcp.TCPTransport$ConnectionHandler.run0(TCPTransport.java:828)
	at sun.rmi.transport.tcp.TCPTransport$ConnectionHandler.access$400(TCPTransport.java:619)
	at sun.rmi.transport.tcp.TCPTransport$ConnectionHandler$1.run(TCPTransport.java:684)
	at sun.rmi.transport.tcp.TCPTransport$ConnectionHandler$1.run(TCPTransport.java:681)
	at java.security.AccessController.doPrivileged(Native Method)
	at sun.rmi.transport.tcp.TCPTransport$ConnectionHandler.run(TCPTransport.java:681)
	at java.util.concurrent.ThreadPoolExecutor.runWorker(ThreadPoolExecutor.java:1145)
	at java.util.concurrent.ThreadPoolExecutor$Worker.run(ThreadPoolExecutor.java:615)
	at java.lang.Thread.run(Thread.java:745)

二月 04, 2020 9:02:35 下午 org.apache.tomcat.util.modeler.BaseModelMBean invoke
严重: Exception invoking method createStandardContext
javax.management.RuntimeOperationsException: Exception invoking method manageApp
	at org.apache.tomcat.util.modeler.BaseModelMBean.invoke(BaseModelMBean.java:309)
	at com.sun.jmx.interceptor.DefaultMBeanServerInterceptor.invoke(DefaultMBeanServerInterceptor.java:819)
	at com.sun.jmx.mbeanserver.JmxMBeanServer.invoke(JmxMBeanServer.java:801)
	at org.apache.catalina.mbeans.MBeanFactory.createStandardContext(MBeanFactory.java:619)
	at org.apache.catalina.mbeans.MBeanFactory.createStandardContext(MBeanFactory.java:566)
	at sun.reflect.NativeMethodAccessorImpl.invoke0(Native Method)
	at sun.reflect.NativeMethodAccessorImpl.invoke(NativeMethodAccessorImpl.java:57)
	at sun.reflect.DelegatingMethodAccessorImpl.invoke(DelegatingMethodAccessorImpl.java:43)
	at java.lang.reflect.Method.invoke(Method.java:606)
	at org.apache.tomcat.util.modeler.BaseModelMBean.invoke(BaseModelMBean.java:301)
	at com.sun.jmx.interceptor.DefaultMBeanServerInterceptor.invoke(DefaultMBeanServerInterceptor.java:819)
	at com.sun.jmx.mbeanserver.JmxMBeanServer.invoke(JmxMBeanServer.java:801)
	at com.sun.jmx.remote.security.MBeanServerAccessController.invoke(MBeanServerAccessController.java:468)
	at javax.management.remote.rmi.RMIConnectionImpl.doOperation(RMIConnectionImpl.java:1487)
	at javax.management.remote.rmi.RMIConnectionImpl.access$300(RMIConnectionImpl.java:97)
	at javax.management.remote.rmi.RMIConnectionImpl$PrivilegedOperation.run(RMIConnectionImpl.java:1328)
	at java.security.AccessController.doPrivileged(Native Method)
	at javax.management.remote.rmi.RMIConnectionImpl.doPrivilegedOperation(RMIConnectionImpl.java:1427)
	at javax.management.remote.rmi.RMIConnectionImpl.invoke(RMIConnectionImpl.java:848)
	at sun.reflect.NativeMethodAccessorImpl.invoke0(Native Method)
	at sun.reflect.NativeMethodAccessorImpl.invoke(NativeMethodAccessorImpl.java:57)
	at sun.reflect.DelegatingMethodAccessorImpl.invoke(DelegatingMethodAccessorImpl.java:43)
	at java.lang.reflect.Method.invoke(Method.java:606)
	at sun.rmi.server.UnicastServerRef.dispatch(UnicastServerRef.java:322)
	at sun.rmi.transport.Transport$2.run(Transport.java:202)
	at sun.rmi.transport.Transport$2.run(Transport.java:199)
	at java.security.AccessController.doPrivileged(Native Method)
	at sun.rmi.transport.Transport.serviceCall(Transport.java:198)
	at sun.rmi.transport.tcp.TCPTransport.handleMessages(TCPTransport.java:567)
	at sun.rmi.transport.tcp.TCPTransport$ConnectionHandler.run0(TCPTransport.java:828)
	at sun.rmi.transport.tcp.TCPTransport$ConnectionHandler.access$400(TCPTransport.java:619)
	at sun.rmi.transport.tcp.TCPTransport$ConnectionHandler$1.run(TCPTransport.java:684)
	at sun.rmi.transport.tcp.TCPTransport$ConnectionHandler$1.run(TCPTransport.java:681)
	at java.security.AccessController.doPrivileged(Native Method)
	at sun.rmi.transport.tcp.TCPTransport$ConnectionHandler.run(TCPTransport.java:681)
	at java.util.concurrent.ThreadPoolExecutor.runWorker(ThreadPoolExecutor.java:1145)
	at java.util.concurrent.ThreadPoolExecutor$Worker.run(ThreadPoolExecutor.java:615)
	at java.lang.Thread.run(Thread.java:745)
Caused by: java.lang.IllegalStateException: ContainerBase.addChild: start: org.apache.catalina.LifecycleException: Failed to start component [StandardEngine[Catalina].StandardHost[localhost].StandardContext[/test_Web_exploded]]
	at org.apache.catalina.core.ContainerBase.addChildInternal(ContainerBase.java:1022)
	at org.apache.catalina.core.ContainerBase.addChild(ContainerBase.java:994)
	at org.apache.catalina.core.StandardHost.addChild(StandardHost.java:652)
	at org.apache.catalina.startup.HostConfig.manageApp(HostConfig.java:1899)
	at sun.reflect.NativeMethodAccessorImpl.invoke0(Native Method)
	at sun.reflect.NativeMethodAccessorImpl.invoke(NativeMethodAccessorImpl.java:57)
	at sun.reflect.DelegatingMethodAccessorImpl.invoke(DelegatingMethodAccessorImpl.java:43)
	at java.lang.reflect.Method.invoke(Method.java:606)
	at org.apache.tomcat.util.modeler.BaseModelMBean.invoke(BaseModelMBean.java:301)
	... 37 more