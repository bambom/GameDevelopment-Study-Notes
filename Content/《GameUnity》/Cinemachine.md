
<h1 id = "000">目录</h2>

* [1、Cinemachine Brain](#001)
* [2、Cinimachine Virtual Camera](#002)
* [3、Cinimachine Virtual Camera Body](#003)
* [4、Cinimachine Virtual Camera Aim ](#004)
* [5、Cinimachine Virtual Camera Noise ](#005)
* [6、2D Camera](#006)
* [6.1Managing & Grouping Virtual Cameras](#0066)
* [7、FreeLook Camera](#007)
* [8、Blend List Camera](#008)
* [9、State-Driven Camera](#009)
* [10.0、相机拓展](#0100)
* [10、ClearShot Camera](#010)
* [11、Dolly Camera with Track](#011)
* [12、Dolly Track with Cart](#012)
* [13、Target Group Camera](#013)
* [14、Mixing Camera](#014)
* [15、Cinemachine Impulse](#015)
* [end](#999)

<br><br><br><br><br><br><br><br>

* [返回](#000)
<h1 id = "001">1、Cinemachine Brain</h2>

Cinemachine Brain： 所有相机组件的重要组件，控制主相机，同时允许用创建和控制许多不同的虚拟相机。

    * Live Camera：当前激活的虚拟相机。
    * Show Debug Text: 显示当前是哪个虚拟相机控制的主相机。 这里在多个相机切换时候，会以最后一个激活的相机作为当前主控制相机。
    * Show Camera Frustum:显示相机裁剪范围。
    * Ignore Time Scale: 忽略timescale的影响，不受unity加减速的影响。
    * World Up Override： 默认空相机使用世界Y作为构建视图矩阵的up向量。拖拽物体并旋转下，就会使用当前这个物体的Y方向作为构建up的向量。
    * Update Method: 更新相机位置和旋转的时机。 如果由于更新地方导致的不流畅可以在这里进行选择更新时机。
    * Default Blend：默认的相机融合时的国度曲线和持续时间。
    * Custom Blends: 如果选择了自定义的融合，就会覆盖默认的。自定义的可以定义多个相机的间的融合关系。
    * Events：
      Camera cut：任何虚拟相机要进入控制并且没有融合的时候切入到这个相机前，出发该事件。
      Camera Actived：相机激活的时候出发，cut或融合的方式都在第一帧触发该事件。

![](MediaTimeline/CinemachineMenue.jpg)



<br><br><br><br><br><br><br><br>

* [返回](#000)
<h1 id ="002">2、 Cinimachine Virtual Camera </h2>
CinimachineVirtualCamera: 是使用时间轴创建相机行为和镜头的关键。 

    Status: Live 当前相机处于显示状态。 Solo 关注当前相机直接显示在Game窗口。用于相机的调试。

    Game Window Guides：勾选时，Game窗口会显示辅助线，用于设置虚拟相机的各个属性。
        仅在以下任意一属性赋值时显示辅助线：
            * Look At 属性设置了物体，Aim设置为Composer或Group Composer
            * Follow 属性设置了物体，Body设置为Framing Composer

    Save During Play：虚拟相机的属性在运行时的修改可以被保存下来，退出Play状态时不会被重置。它是通过Cinemachine命名空间中的一个特殊的属性[SaveDuringPlay]。如果你自己的脚本也需要运行时保存的功能，只需要在类上加上这个属性即可。如果不想要类中的某些成员在运行时保存，可以给这些成员加上[NoSaveDuringPlay]属性。
![](MediaTimeline/SaveDuringPlay.png)

    Priority：虚拟相机的优先级，用于Live镜头的选择。数值越高代表优先级越高。Cinemachine Brain会根据这个属性从所有激活的虚拟相机中选择活动的虚拟相机。在Timeline上使用时这个属性不生效。

    Follow： 相机需要跟随的物体

    Look At：相机需要镜头对准的物体

    Standby Update：相机待命时的更新方式，当虚拟相机物体没有被禁用，但是优先级不足时，虚拟相机处于待命状态。这个属性会影响性能，通常设置为Never是最好的，但是有时候可能需要虚拟相机更新来做一些镜头相关的计算判断。
        * Never 不更新
        * Always 每帧更新
        * Round Robin 循环更新：所有的待命虚拟相机循环更新，每帧更新一个相机（例如有10个待命的相机，第一帧更新第一个相机，第2帧更新第2个相机，….，第11帧更新第1个相机，以此类推）

    Lens：镜头设置，对应Unity相机中的属性，也可以从Preset中选择或者从现有的设置创建新的Preset。
        Field of view: 
        Near Clip Plane: 近裁面
        Far Clip Plane:  远裁面
        Dutch:   Camera Z roll，or tilt ，in degrees 。 这个属性在Unity相机中是没有的，可以用来控制镜头的旋转。（在屏幕空间的旋转）

    Transitions: 相机转换的相关设置

        Blend Hint: 混合方式
            None:无，默认线性混合
            Spherical Position:根据Look At的物体球面旋转混合
            Cylindrical Position:根据Look At的物体柱面旋转混合（水平方向圆弧，垂直方向线性）
            Screen Space Aim When Targets Differ:在屏幕空间瞄准目标

        Inherit Position: 下一个相机变成活动相机时，从上一个相机继承位置，即保持两个相机位置相同。

        On Camera Live: 相机激活事件：事件，相机变为活动时会触发对应的事件。







<br><br><br><br><br><br><br><br>

* [返回](#000)
<h1 id = "003">3、 Cinimachine Virtual Camera Body</h3>

Body: 主要用于设置相机的移动是的算法 . 

[Body参考地址](https://mp.weixin.qq.com/s?__biz=MjM5Mzg2Nzg2MQ==&mid=2456961795&idx=1&sn=6aefc3d506572248e84e675af0476448&chksm=b116923986611b2ff6fd0b9aa72eece0ee7eb66dbd435b98e677c8e15e9958397f94bd711fe0&token=1108897709&lang=zh_CN#rd)

<table><tr><td bgcolor=#6495ED><font color=black size=5> ------------Donothing算法------------- </font> </td></tr></table>

<font color=#6495ED size=4> Donothing </font>：不移动虚拟相机。选中这个算法时，虚拟相机激活时，会控制Unity相机会固定在当前虚拟相机的位置，不会移动。用于固定位置的镜头，也可以通过自定义脚本来控制相机的位置。通常和Look At配合使用，模拟固定位置的跟随镜头。


<table><tr><td bgcolor=#6495ED><font color=black size=5> ----------Framing Transposer算法----------- </font> </td></tr></table>
<font color=#6495ED size=4> Framing Transposer </font>: 跟随目标移动，并在屏幕空间保持相机和跟随目标的相对位置。

选中这个算法时，Cinemachine会在屏幕空间将相机和跟随物体保持固定的相对位置关系。只会改变相机的位置，不会改变相机的旋转。还可以设置偏移、阻尼、构图规则等等。

<font color=Coral face="加粗">Framing Transposer </font>算法是为2D和正交相机设计的，主要用于2D情况。但是对于透视相机和3D环境也可以使用。<br>

![](MediaTimeline/FramingTransposer.png)

<br>
这个算法工作流程大概是：首先沿着相机的Z轴移动相机直到<font color=Coral face="加粗">Camera Distance </font>距离的XY平面上，然后在XY面上平移，直到目标物体在屏幕空间到达期望的位置。
<br>
<font color=Coral face="加粗">特别注意：使用Framing Transposer时，Look At属性必须为空。</font>
<br>
<font color=Coral face="加粗">Lookahead Time</font>：根据目标的运动，调整虚拟相机与“跟随”目标的偏移量。Cinemachine预测目标在未来数秒之内到达的位置并提前设置Unity相机的位置。这个功能对微动的动画敏感，并且会放大噪点，导致非预期的相机抖动。如果不能接受目标运动时的相机抖动，减小这个属性可能会使相机动画更流畅。
<br>
<font color=Coral face="加粗">Lookahead Smoothing</font>：预测算法的平滑度。较大的值可以消除抖动但会使预测滞后。
<br>
<font color=Coral face="加粗">Lookahead Ignore Y</font>：如果选中，在预测计算时会忽略沿Y轴的移动。
<br>
<font color=Coral face="加粗">X Damping</font>：相机在X轴上移动的阻力系数。较小的值会使相机反应更快。较大的值会使相机的反应速度变慢。每个轴使用不同的设置可以制造出各种类型相机的行为。
<br>
<font color=Coral face="加粗">Y Damping</font>：相机在Y轴上移动的阻力系数。较小的值会使相机反应更快。较大的值会使相机的反应速度变慢。
<br>
<font color=Coral face="加粗">Z Damping</font>：相机在Z轴上移动的阻力系数。较小的值会使相机反应更快。较大的值会使相机的反应速度变慢。
<br><br>

![](MediaTimeline/FramingTransposer1.jpg)
<br><br>

<font color=Coral face="加粗">Screen X</font>：目标的水平屏幕位置。相机移动的结果是使目标处于此位置。<br>
<font color=Coral face="加粗">Screen Y</font> ：目标的垂直屏幕位置，相机移动的结果是使目标处于此位置。<br>
<font color=Coral face="加粗">Camera Distance</font> ：沿摄像机Z轴与跟随目标保持的距离。<br>
<font color=Coral face="加粗">Dead Zone Width</font> ：当目标在此位置范围内时，不会水平移动相机。<br>
<font color=Coral face="加粗">Dead Zone Height</font> ：当目标在此位置范围内时，不会垂直移动相机。<br>
<font color=Coral face="加粗">Dead Zone Depth</font> ：当跟随目标距离相机在此范围内时，不会沿其z轴移动相机。<br>

<font color=Coral face="加粗">Unlimited Soft Zone</font> ：如果选中，Soft Zone没有边界.<br>
<font color=Coral face="加粗"></font> Soft Zone Width：当目标处于此范围内时，会水平移动相机，将目标移回到Dead Zone中。Damping属性会影响摄像机的运动速度。<br>
<font color=Coral face="加粗">Soft Zone Height</font> ：当目标处于此范围内时，会垂直移动相机，将目标移回到Dead Zone中。Damping属性会影响摄像机的运动速度。<br>
<font color=Coral face="加粗">Bias X</font> ：Soft Zone的中心与目标位置的水平偏移。<br>
<font color=Coral face="加粗">Bias Y</font> ：Soft Zone的中心与目标位置的竖直偏移。<br>
<font color=Coral face="加粗">Center On Active</font> ：选中时，虚拟相机激活时会将镜头中心对准物体。不选中时，虚拟相机会将目标物体放置在最近的dead zone边缘。

<br><br>
<table><tr><td bgcolor=#6495ED><font color=black size=5> ----------Cinemachine Target Group组件---------- </font> </td></tr></table>

Cinemachine Target Group组件可以让多个GameObjects作为一个组，设置为Look At的对象。在虚拟相机中使用Group Composer算法，可以设置Follow属性为这个TargetGroup。

<font face="加粗"> 如何创建一个Target Group呢？ </font>

1、菜单栏选择<font color=Coral face="加粗"> Cinemachine > Create Target Group Camera </font>。Unity会在场景中添加一个虚拟相机和Target Group。Follow和Look At属性会自动设置为这个Group。<br>
2、Hierarchy中选中这个Group。<br>
3、Inspector上可以点击加号+添加新的物体到这个组。<br>
4、点击加号后，设置GameObject、权重和半径。<br>
5、可以重复多次3-4步来添加更多物体。<br>

 ![](MediaTimeline/CinemachineTargetGroup.png)<br>

<font color=Coral face="加粗">属性详解</font><br>
<font color=Coral face="加粗">Position Mode</font> 如何计算Group的位置<br>
   * Group Center 根据所有物体的包围盒计算一个包含它们的大包围盒.Group Center就是这个大包围盒的中心.
   * Group Average 根据所有物体的位置加权重计算平均值.

<font color=Coral face="加粗">Rotation Mode</font>如何计算Target Group的旋转
   * Manual 使用TargetGroup根物体的旋转。推荐设置
   * Group Average 根据所有物体的旋转加权重计算

<font color=Coral face="加粗">Update Method</font> 更新Target Group的Transform的时机

<font color=Coral face="加粗"> Targets </font> 组内包含的物体列表
   * Weight 每个对象的权重
   * Radius 每个物体的半径，用于计算包围盒。不可为负数。


<br><br>
<table><tr><td bgcolor=#6495ED><font color=black size=5> ----------Hard Lock To Target算法---------- </font> </td></tr></table>
<font color=#6495ED size=4> Hard Lock To Target </font> : 虚拟相机和跟随目标使用相同位置。Unity相机保持和目标位置一致，即位置同步

![](MediaTimeline/HardLockToTarget.png)<br>
<font color=Coral face="加粗"> Damping </font>：相机追赶上目标位置的时间。如果为0，那就是保持同步，如果大于0，相当于经过多少秒相机和目标位置重合。


<br><br>
<table><tr><td bgcolor=#6495ED><font color=black size=5> --------Orbital Transposer------- </font> </td></tr></table>
<font color=Coral face="加粗"> Orbital Transposer </font>: 相机和跟随目标的相对位置是可变的，还能接收用户的输入。常见于玩家控制的相机。

Orbital Transposer引入了一个新的概念叫 <font color=Coral face="加粗"> heading </font>，代表了目标移动的方向或面朝的方向。Orbital Transposer会尝试移动相机，让镜头朝向heading的方向。默认情况下，相机的位置会在target的正后面。也可以通过<font color=Coral face="加粗"> Heading Bias </font>属性设置。

如果给Orbital Transposer添加了输入控制器，玩家就可以控制相机围绕目标旋转。可以设置为Input Manager中的轴，也可以直接用脚本控制。

当<font color=Coral face="加粗"> Recenter To Target Heading </font> 属性选中时，在没有输入时Orbital Transposer可以自动重新居中相机。

<div align=center><img  src="MediaTimeline/OrbitalTransposer.png"/></div>

<font color=Coral face="加粗"> 属性详解 </font> <br>
<font color=Coral face="加粗"> Binding Mode </font> 绑定模式：从目标推断位置时使用的坐标空间。<br>
   * <font color=DodgerBlue face="加粗"> Lock To Target On Assign </font> ：本地空间，相机被激活或target赋值时的相对位置.
   * <font color=DodgerBlue face="加粗"> Lock To Target With World Up </font>：本地空间，保持相机y轴朝上，yaw和roll为0.
   * <font color=DodgerBlue face="加粗"> Lock To Target No Roll </font>：本地空间，锁定到目标物体，roll为0.
   * <font color=DodgerBlue face="加粗"> Lock To Target </font>：本地空间，锁定到目标物体.
   * <font color=DodgerBlue face="加粗"> World Space </font>：世界空间.
   * <font color=DodgerBlue face="加粗"> Simple Follow With World Up </font>：相对于目标的位置，使用相机的本地坐标系，保持相机y轴朝上.

<div align=center><img  src="MediaTimeline/BindingMode.jpg"/></div>

<font color=Coral face="加粗"> Follow Offset </font>：跟随目标时的位置偏移<br>
<font color=Coral face="加粗"> X Damping </font>：相机在X轴上移动的阻力系数。较小的值会使相机反应更快。较大的值会使相机的反应速度变慢。每个轴使用不同的设置可以制造出各种类型相机的行为。 绑定模式为Simple Follow With World Up时不可用。<br>
<font color=Coral face="加粗"> Y Damping </font>：相机在Y轴上移动的阻力系数。较小的值会使相机反应更快。较大的值会使相机的反应速度变慢。<br>
<font color=Coral face="加粗"> Z Damping </font>：相机在Z轴上移动的阻力系数。较小的值会使相机反应更快。较大的值会使相机的反应速度变慢。<br>

【下面3个属性参考上面的飞机图】
<font color=Coral face="加粗"> Yaw Damping </font>：相机在y轴旋转的阻力系数。较小的数字会使相机反应更快。较大的数字会使相机的反应速度变慢。每个轴使用不同的设置可以制造出各种类型相机的行为。Binding Mode为Lock to Target With World Up、Lock to Target No Roll、Lock to Target时可用。<br>
<font color=Coral face="加粗"> Pitch Damping </font>：相机在x轴旋转的阻力系数。较小的数字会使相机反应更快。较大的数字会使相机的反应速度变慢。Binding Mode为Lock to Target No Roll、Lock to Target时可用。<br>
<font color=Coral face="加粗"> Roll Damping </font>：相机在z轴旋转的阻力系数。较小的数字会使相机反应更快。较大的数字会使相机的反应速度变慢。Binding Mode为Lock to Target时可用。<br>

<font color=Coral face="加粗"> Heading </font>：计算Follow朝向的方法<br>
    Definition ：计算方法<br>

        Position Delta：基于当前帧和上一帧的目标位置之间的变化
        Velocity：使用目标的刚体速度，如果目标没有Rigidbody组件，会使用Position Delta
        Target Forward ：使用目标的transform.forward作为heading的方向
        World Forward：使用世界坐标系中的Vector3.forward作为heading方向

<font color=Coral face="加粗"> Bias </font>：相机围绕旋转的偏移，单位是度数。<br>

<font color=Coral face="加粗"> Recenter To Target Heading </font>：接收不到用户输入时，自动居中。<br>
<font color=Coral face="加粗"> Enable </font>：是否启用<br>
<font color=Coral face="加粗"> Wait Time </font>：等待延迟时间，用户无输入后多长时间重新自动居中。<br>
<font color=Coral face="加粗"> Recentering Time </font>：重新自动居中的过程花费的时间。<br>

<font color=Coral face="加粗"> X Axis </font>：通过玩家输入控制Heading方向。<br>
<font color=Coral face="加粗"> Value </font>：当前值。<br>
<font color=Coral face="加粗"> Value Range </font>：输入范围。<br>
<font color=Coral face="加粗"> Speed </font>：最大速度（Max Speed）或者最大增加速度（Input Value Gain）。<br>
<font color=Coral face="加粗"> Accel Time </font>：加速到最高速度所需要的时间。<br>
<font color=Coral face="加粗"> Decel Time </font>：减速到0所需的时间。<br>
<font color=Coral face="加粗"> Input Axis Name </font>：接收输入的Input Manager中的轴名称，空字符串代表不接收输入。<br>
<font color=Coral face="加粗"> Input Axis Value </font>：玩家输入的值。可以直接通过自定义脚本控制。<br>
<font color=Coral face="加粗"> Invert </font>：是否反转输入的值（取相反数）。<br>

<br><br>
<table><tr><td bgcolor=#6495ED><font color=black size=5> --------Tracked Dolly------- </font> </td></tr></table>
<font color=Coral face="加粗"> Tracked Dolly </font> : 这个算法可以让相机沿预定路径移动。使用<font color=Coral face="加粗"> Path Position</font>属性来指定将虚拟相机放置在路径上的位置。

<div align=center><img  src="MediaTimeline/TrackedDolly.png"/></div>

使用<font color=Coral face="加粗"> Auto-Dolly </font>模式将虚拟相机移动到路径上最接近<font color=Coral face="加粗"> Follow </font>目标的位置。启用后，<font color=Coral face="加粗"> Auto-Dolly </font>会自动将虚拟相机的位置移动到最接近目标的路径上的位置。

<font color=Coral face="加粗">提示 </font>：使用Auto-Dolly模式时，一定要谨慎选择路径形状。在围绕某个点形成弧形的路径上可能会有问题。举一个极端的例子，考虑一条以<font color=Coral face="加粗">Follow </font>目标为中心的完美圆形路径。路径上最接近目标的点变得不稳定，因为圆形路径上的所有点都同样接近目标。在这种情况下，将<font color=Coral face="加粗">Follow</font>目标移动很小的距离会导致相机在轨道上移动很大的距离。

<font color=Coral face="加粗">属性详解</font><br>
<font color=Coral face="加粗">Path </font>相机移动的路径。此属性必须引用CinemachinePath或Cinemachine Smooth Path对象。<br>
<font color=Coral face="加粗">Path Position</font> 沿路径放置相机的位置。直接给这个属性作动画或启用Auto-Dolly。这个值以Position Units指定的单位为单位。<br>
<font color=Coral face="加粗">Position Units</font>  路径位置的度量单位。<br>
   * <font color=DodgerBlue face="加粗"> Path Units </font>：  沿路径使用路径点。0表示路径上的第一个路径点，1表示第二个路径点，依此类推。
   * <font color=DodgerBlue face="加粗"> Distance </font>：  沿路径使用距离。根据路径的Resolution属性对路径进行采样。Cinemachine创建一个距离查找表，并将其存储在内部缓存中。
   * <font color=DodgerBlue face="加粗"> Normalized </font>： 使用路径的开头和结尾。值0表示路径的起点，值1表示路径的终点。
   
<font color=Coral face="加粗"> Path Offset </font>: 相机相对于路径的位置。X垂直于路径，Y向上，而Z平行于路径。使用此属性可使相机偏离路径本身。<br>

<font color=Coral face="加粗"> X Damping </font>:  设置相机如何在垂直于路径的方向上保持其位置。较小的数字会使相机反应更快。较大的数字会使相机的响应速度变慢。每个轴使用不同的设置可以产生多种相机表现。<br>
<font color=Coral face="加粗"> Y Damping </font>: 设置相机如何在路径局部坐标向上方向上保持其位置。较小的数字会使相机反应更快。较大的数字会使相机的响应速度变慢。<br>
<font color=Coral face="加粗"> Z Damping </font>: 设置相机如何在平行于路径的方向上保持其位置。较小的数字会使相机反应更快。较大的数字会使相机的响应速度变慢。<br>
<div align=center><img  src="MediaTimeline/BindingMode.jpg"/></div>

<font color=Coral face="加粗"> Camera Up </font> : 如何为虚拟相机设置向上的方向。这会影响屏幕的组成，因为相机的Aim算法会尝试遵循向上方向。
   * <font color=DodgerBlue face="加粗"> Default </font> : 不修改虚拟相机的向上方向，而是使用Cinemachine Brain中的World Up Override属性。
   * <font color=DodgerBlue face="加粗"> Path  </font> : 在当前点使用路径的向上方向。
   * <font color=DodgerBlue face="加粗"> Path No Roll </font> :  在当前点使用路径的向上方向，但Roll设置为零。
   * <font color=DodgerBlue face="加粗"> Follow Target </font> :  使用Follow目标的向上向量。
   * <font color=DodgerBlue face="加粗"> Follow Target No Roll </font> :  使用Follow目标的变换中的向上向量，但Roll为零。

<font color=Coral face="加粗"> Pitch Damping  </font>:    相机如何跟踪目标旋转的x角。较小的数字会使相机反应更快。较大的数字会使相机的响应速度变慢。<br>
<font color=Coral face="加粗"> Yaw Damping  </font> :   相机如何跟踪目标旋转的y角。较小的数字会使相机反应更快。较大的数字会使相机的响应速度变慢。<br>
<font color=Coral face="加粗"> Roll Damping  </font> :   相机如何跟踪目标旋转的z角。较小的数字会使相机反应更快。较大的数字会使相机的响应速度变慢。<br>
<font color=Coral face="加粗"> Auto Dolly  </font>:    控制自动轨道位置选择方式。要使用此功能，必须设置Follow目标。<br>
   * <font color=DodgerBlue face="加粗"> Enabled  </font>:  选中以启用。注意：这可能会影响性能，具体取决于search resolution。<br>
   *<font color=DodgerBlue face="加粗"> Position Offset  </font>:   以position units为单位从路径上的最近点到跟随目标的偏移量。<br>
   * <font color=DodgerBlue face="加粗"> Search Radius  </font>:  当前段两侧的段数。如果只有一个路径使用0。当路径相对于目标位置的形状导致路径上最近的点变得不稳定时，请使用较小的数字。<br>
   * <font color=DodgerBlue face="加粗"> Search Resolution  </font>:  Cinemachine通过将片段分成许多直片段来搜索片段。数字越高，结果越准确。但是，对于更大的数字，性能成比例地变慢。<br>






<br><br>
<table><tr><td bgcolor=#6495ED><font color=black size=5> --------Transposer------- </font> </td></tr></table>
<font color=Coral face="加粗"> Transposer </font>: 跟随目标移动，并在世界空间保持相机和跟随目标的相对位置固定。这个算法将使用固定的相对位置将虚拟相机跟随目标，也可以使用Damping属性。
<div align=center><img  src="MediaTimeline/Transposer.png"/></div>

 <font color=Coral face="加粗"> 属性详解  </font> <br>                            
<font color=Coral face="加粗"> Binding Mode </font> 绑定模式：从目标推断位置时使用的坐标空间。
   *  <font color=DodgerBlue face="加粗"> Lock To Target On Assign  </font> ：本地空间，相机被激活或target赋值时的相对位置。
   *  <font color=DodgerBlue face="加粗"> Lock To Target With World Up </font>：本地空间，保持相机y轴朝上，yaw和roll为0。
   *  <font color=DodgerBlue face="加粗"> Lock To Target No Roll </font>：本地空间，锁定到目标物体，roll为0。
   *  <font color=DodgerBlue face="加粗"> Lock To Target  </font>：本地空间，锁定到目标物体
   *  <font color=DodgerBlue face="加粗"> World Space  </font> ：世界空间。
   * <font color=DodgerBlue face="加粗"> Simple Follow With World Up </font> ：相对于目标的位置，使用相机的本地坐标系。

<font color=Coral face="加粗"> Follow Offset </font>：跟随目标时的位置偏移<br>   
<font color=Coral face="加粗"> X Damping </font>：相机在X轴上移动的阻力系数。较小的值会使相机反应更快。较大的值会使相机的反应速度变慢。每个轴使用不同的设置可以制造出各种类型相机的行为。 绑定模式为Simple Follow With World Up时不可用。<br>   
<font color=Coral face="加粗"> Y Damping </font>：相机在Y轴上移动的阻力系数。较小的值会使相机反应更快。较大的值会使相机的反应速度变慢。<br>   
<font color=Coral face="加粗"> Z Damping </font>：相机在Z轴上移动的阻力系数。较小的值会使相机反应更快。较大的值会使相机的反应速度变慢。<br>   

【下面3个属性参考上面的飞机图】<br>   
<font color=Coral face="加粗"> Yaw Damping </font>：相机在y轴旋转的阻力系数。较小的数字会使相机反应更快。较大的数字会使相机的反应速度变慢。每个轴使用不同的设置可以制造出各种类型相机的行为。<br>   Binding Mode为Lock to Target With World Up、Lock to Target No Roll、Lock to Target时可用。
<font color=Coral face="加粗"> Pitch Damping </font>：相机在x轴旋转的阻力系数。较小的数字会使相机反应更快。较大的数字会使相机的反应速度变慢。Binding Mode为Lock to Target No Roll、Lock to Target时可用。<br>   
<font color=Coral face="加粗"> Roll Damping </font>：相机在z轴旋转的阻力系数。较小的数字会使相机反应更快。较大的数字会使相机的反应速度变慢。Binding Mode为Lock to Target时可用。<br>   



<br><br><br><br><br><br><br><br>

* [返回](#000)
  <h2 id = "004">4、Cinimachine Virtual Camera Aim </h2>
CinemachineVitualCamera组件中的 Aim属性用于设置相机<font color=Coral face="加粗"> 旋转 </font>旋转时使用什么算法。需要先设置<font color=Coral face="加粗"> Look At </font>属性。
<div align=center><img  src="MediaTimeline/Aim.png"/></div>
Aim
包含以下旋转的算法：
<div align=center><img  src="MediaTimeline/Aim0.png"/></div>

   * <font color=Coral face="加粗">Do nothing </font>: 不控制相机的旋转
   * <font color=Coral face="加粗"> Composer</font>: 保持目标物体在镜头内
   * <font color=Coral face="加粗"> Group Composer</font>: 保持多个目标在镜头内
   * <font color=Coral face="加粗"> Hard Look At </font>: 保持目标在镜头的中心
   * <font color=Coral face="加粗"> POV </font>: 基于玩家的输入旋转相机
   * <font color=Coral face="加粗"> Same As Follow Target </font>: 相机的旋转和目标的旋转保持同步

<table><tr><td bgcolor=#6495ED><font color=black size=5> --------Composer-------- </font> </td></tr></table>
这个算法旋转相机来朝向目标物体。也可以添加偏移、阻尼和构图规则。常见跟踪的目标有：角色的上半身或头部的骨骼、车辆、动画或程序控制的空物体。

<div align=center><img  src="MediaTimeline/ComposerMenue.png"/></div>

 <font color=Coral face="加粗"> 属性详解 </font>
<font color=Coral face="加粗"> Tracked Object Offset </font>: 相对于跟踪目标的偏移。当注视的位置不是被跟踪对象的中心时，可以通过这个属性微调跟踪目标位置。<br>
<font color=Coral face="加粗"> Lookahead Time </font>: 提前的时间。根据注视目标的运动来调整偏移量。该算法估计目标将在未来数秒之内到达的位置。这个功能对微动的动画敏感，并且会放大噪点，导致非预期的相机抖动。如果目标运动时相机抖动不可接受，降低此属性可能会使目标动画更流畅。<br>
<font color=Coral face="加粗"> Lookahead Smoothing </font>: 控制前瞻算法的平滑度。较大的值可以消除抖动预测但会使预测滞后。<br>
<font color=Coral face="加粗"> Lookahead Ignore Y </font>: 预测算法会忽略Y轴的运动。<br>
<font color=Coral face="加粗"> Horizontal Damping  </font>: 水平阻尼。相机在屏幕水平方向上对目标的反应速度如何。使用较小的数字可以使照相机更快地旋转，以使目标保持在dead zone。使用较大的数字来模拟较重，响应缓慢的相机。<br>
<font color=Coral face="加粗"> Vertical Damping </font>: 垂直阻尼。相机在屏幕垂直方向上对目标的反应速度如何。使用不同的垂直和水平设置可以模拟不同相机行为。<br>

<div align=center><img  src="MediaTimeline/ComposerArea.jpg"/>各区域示意图</div>

<font color=Coral face="加粗"> Screen X  </font>：dead zone中心的水平屏幕位置，相机旋转保持目标在此处。<br>
<font color=Coral face="加粗"> Screen Y  </font>dead zone中心的垂直屏幕位置，相机旋转保持目标在此处。<br>
<font color=Coral face="加粗"> Dead Zone Width </font> 目标在这个区域时，相机会忽略目标的任何移动，此属性设置这个区域的宽度。目标位于该区域内的任何位置时，虚拟相机不会更新其旋转角度。这对于忽略较小的目标移动很有用。<br>
<font color=Coral face="加粗"> Dead Zone Height </font> 目标在这个区域时，相机会忽略目标的任何移动，此属性设置这个区域的高度。如果目标位于该区域内的任何位置，则虚拟相机不会更新其旋转角度。这对于忽略较小的目标移动很有用。<br>
<font color=Coral face="加粗"> Soft Zone Width  </font>soft zone的宽度。如果目标出现在屏幕的此区域中，则相机将旋转，以在Horizontal Damping设置的时间内将其推回dead zone。<br>
<font color=Coral face="加粗"> Soft Zone Height  </font>soft zone的高度。如果目标出现在屏幕的此区域中，则相机将旋转，以在Vertical Damping设置的时间内将其推回dead zone。<br>
<font color=Coral face="加粗"> Bias X </font>soft zone 中心相对于dead zone中心的水平偏移。<br>
<font color=Coral face="加粗"> Bias Y  </font>soft zone中心相对于dead zone中心的垂直偏移。<br>

<font color=Coral face="加粗"> Center On Active </font>：选中时，虚拟相机激活时会将镜头中心对准物体。不选中时，虚拟相机会将目标物体放置在最近的dead zone边缘。<br>


<table><tr><td bgcolor=#6495ED><font color=black size=5> --------Group Composer-------- </font> </td></tr></table>
这个算法可以用来让镜头瞄准多个目标。如果Look At属性设置的是一个Cinemachine Target Group，这个算法会调整相机的FOV和举例来保证Group中的物体都能被镜头看到。如果Look At属性设置的是一个物体，那么会和Composer算法表现一致。
<div align=center><img  src="MediaTimeline/GroupComposerMenue.png"/></div>

<font color=Coral face="加粗"> 属性详解</font>
<font color=Coral face="加粗"> Tracked Object Offset</font>： 相对于跟踪目标的偏移。当注视的位置不是被跟踪对象的中心时，可以通过这个属性微调跟踪目标位置。<br>

<font color=Coral face="加粗"> Lookahead Time</font>： 提前的时间。根据注视目标的运动来调整偏移量。该算法估计目标将在未来数秒之内到达的位置。这个功能对微动的动画敏感，并且会放大噪点，导致非预期的相机抖动。如果目标运动时相机抖动不可接受，降低此属性可能会使目标动画更流畅。<br>
<font color=Coral face="加粗">Lookahead Smoothing</font>： 控制前瞻算法的平滑度。较大的值可以消除抖动预测但会使预测滞后。<br>
<font color=Coral face="加粗"> Lookahead Ignore Y</font> ：预测算法会忽略Y轴的运动。<br>

<font color=Coral face="加粗"> Horizontal Damping</font> ：水平阻尼。相机在屏幕水平方向上对目标的反应速度如何。使用较小的数字可以使照相机更快地旋转，以使目标保持在dead zone。使用较大的数字来模拟较重，响应缓慢的相机。<br>
<font color=Coral face="加粗"> Vertical Damping</font>： 垂直阻尼。相机在屏幕垂直方向上对目标的反应速度如何。使用不同的垂直和水平设置可以模拟不同相机行为。<br>

<div align=center><img  src="MediaTimeline/GroupComposerArea.jpg"/>各区域示意图</div>

<font color=Coral face="加粗"> Screen X  </font>：dead zone中心的水平屏幕位置，相机旋转保持目标在此处。<br>
<font color=Coral face="加粗"> Screen Y  </font>dead zone中心的垂直屏幕位置，相机旋转保持目标在此处。<br>
<font color=Coral face="加粗"> Dead Zone Width </font> 目标在这个区域时，相机会忽略目标的任何移动，此属性设置这个区域的宽度。目标位于该区域内的任何位置时，虚拟相机不会更新其旋转角度。这对于忽略较小的目标移动很有用。<br>
<font color=Coral face="加粗"> Dead Zone Height </font> 目标在这个区域时，相机会忽略目标的任何移动，此属性设置这个区域的高度。如果目标位于该区域内的任何位置，则虚拟相机不会更新其旋转角度。这对于忽略较小的目标移动很有用。<br>
<font color=Coral face="加粗"> Soft Zone Width  </font>soft zone的宽度。如果目标出现在屏幕的此区域中，则相机将旋转，以在Horizontal Damping设置的时间内将其推回dead zone。<br>
<font color=Coral face="加粗"> Soft Zone Height  </font>soft zone的高度。如果目标出现在屏幕的此区域中，则相机将旋转，以在Vertical Damping设置的时间内将其推回dead zone。<br>
<font color=Coral face="加粗"> Bias X </font>soft zone 中心相对于dead zone中心的水平偏移。<br>
<font color=Coral face="加粗"> Bias Y  </font>soft zone中心相对于dead zone中心的垂直偏移。<br>

<font color=Coral face="加粗"> Center On Active </font>：选中时，虚拟相机激活时会将镜头中心对准物体。不选中时，虚拟相机会将目标物体放置在最近的dead zone边缘。<br>

<font color=Coral face="加粗"> Group Framing Size</font>：目标应占据的屏幕大小比例。使用1填充整个屏幕，使用0.5填充一半的屏幕，依此类推。
<font color=Coral face="加粗"> Framing Mode</font>：指定构图时要考虑的屏幕尺寸。

   * <font color = DodgerBlue face="加粗"> Horizontal</font> 仅考虑水平尺寸。忽略垂直尺寸。
   * <font color = DodgerBlue face="加粗"> Vertical</font>  仅考虑垂直尺寸。忽略水平尺寸。
   * <font color = DodgerBlue face="加粗"> Horizontal And Vertical</font>  使用水平和垂直尺寸中较大的那个来获得最佳匹配。
Adjustment Mode  如何调整相机以获得所需的取景。可以是缩放、拉近拉远或同时进行。
   * <font color = DodgerBlue face="加粗"> Zoom Only </font> 不移动相机，仅调整FOV。
   * <font color = DodgerBlue face="加粗"> Dolly Only</font>  移动相机，不修改FOV。
   * <font color = DodgerBlue face="加粗"> Dolly Then Zoom</font>  将相机移动到范围允许的最大范围，然后根据需要调整FOV。

<font color=Coral face="加粗"> Max Dolly In </font>：朝目标拉近相机的最大距离。<br>
<font color=Coral face="加粗"> Max Dolly Out</font>：远离目标拉远相机的最大距离。<br>
<font color=Coral face="加粗"> Minimum Distance</font>：设置此项以限制相机可以接近目标的最小距离。<br>
<font color=Coral face="加粗"> Maximum Distance</font>：设置此项以限制相机可以达到的最远目标距离。<br>
<font color=Coral face="加粗"> Minimum FOV</font>：自动调节FOV时的最小值。<br>
<font color=Coral face="加粗"> Maximum FOV</font>：自动调节FOV时的最大值。<br>


<table><tr><td bgcolor=#6495ED><font color=black size=5> --------POV-------- </font> </td></tr></table>此算法基于玩家的输入来调节相机的旋转
<div align=center><img  src="MediaTimeline/POV.png"/></div>

<font color=Coral face="加粗"> 属性详解</font><br>
<font color=Coral face="加粗"> Apply Before Body </font>不勾选时，Aim算法会在Body之后设置Camera相关属性。勾选时，Aim会在Body之前设置Camera相关属性。通常Body使用Framing Transposer算法时会很有用。

<font color = DodgerBlue face="加粗"> </font>
<font color=Coral face="加粗"> Recenter Target</font><br>R 重置回中心的目标对象。
   * <font color = DodgerBlue face="加粗"> None</font> 无
   * <font color = DodgerBlue face="加粗"> Follow Target Forward</font> Follow属性的forward
   * <font color = DodgerBlue face="加粗">Look At Target Forward  </font>Look At属性的forward

<font color=Coral face="加粗"> Vertical Axis</font> 控制虚拟相机目标的垂直朝向。<br>
   * <font color = DodgerBlue face="加粗">Value </font> 轴的当前值，以度为单位。可接受的值为-90至90。<br>
   * <font color = DodgerBlue face="加粗">Value Range </font> 虚拟相机的垂直轴的最小值和最大值。<br>
   * <font color = DodgerBlue face="加粗">Wrap </font>    如果选中，则轴将在Value Range范围内，形成一个循环。<br>
   * <font color = DodgerBlue face="加粗">Max Speed </font> 该轴的最大速度，以度/秒为单位。
   * <font color = DodgerBlue face="加粗"> Accel Time Input Axis Value</font>处于最大值时，加速到最大速度<br>所花费的时间（以秒为单位）。
   * <font color = DodgerBlue face="加粗"> Decel Time</font> 轴减速为零所花费的时间（以秒为单位）。
   * <font color = DodgerBlue face="加粗">Input Axis Name </font> 输入轴的名称。在Unity Input Manager中指定的该轴的名称。将此属性设置为空字符串来禁用此轴的自动更新。
   * <font color = DodgerBlue face="加粗">Input Axis Value </font> 输入轴的值。值为0表示无输入。你可以直接从自定义脚本中修改这个值。或者设置Input Axis Name并由Unity Input Manager驱动。
   * <font color = DodgerBlue face="加粗">Invert  </font>将原始值反转。

<font color=Coral face="加粗"> Vertical Recentering</font> 当接收不到玩家输入时，自动在垂直方向重新居中。
   * <font color = DodgerBlue face="加粗"> Enable</font> 是否启用。选中以启用自动垂直居中。
   * <font color = DodgerBlue face="加粗"> Wait Time</font> 等待时间。如果在垂直轴上未检测到用户输入，则相机将等待几秒钟，然后再进行重新居中。
   * <font color = DodgerBlue face="加粗"> Recentering Time</font> 重新居中所花费的时间。


<font color=Coral face="加粗"> Horizontal Axis</font> 控制水平方向。
   * <font color = DodgerBlue face="加粗"> Value </font> 轴的当前值，以度为单位。可接受的值为-180至180。
   * <font color = DodgerBlue face="加粗"> Value Range </font> 虚拟相机水平轴的最小值和最大值。
   * <font color = DodgerBlue face="加粗"> Wrap </font>    如果选中，则轴将在Value Range范围内，形成一个循环。
   * <font color = DodgerBlue face="加粗"> Max Speed </font> 该轴的最大速度，以度/秒为单位。
   * <font color = DodgerBlue face="加粗"> Accel Time Input </font> Axis Value处于最大值时，加速到最大速度所花费的时间（以秒为单位）。
   * <font color = DodgerBlue face="加粗"> Decel Time </font> 轴减速为零所花费的时间（以秒为单位）。
   * <font color = DodgerBlue face="加粗"> Input Axis Name </font> 输入轴的名称。在Unity Input Manager中指定的该轴的名称。将此属性设置为空字符串来禁用此轴的自动更新。
   * <font color = DodgerBlue face="加粗"> Input Axis Value </font> 输入轴的值。值为0表示无输入。你可以直接从自定义脚本中修改这个值。或者设置Input Axis Name并由Unity Input Manager驱动。
   * <font color = DodgerBlue face="加粗"> Invert </font> 将原始值反转。

<font color=Coral face="加粗"> Horizontal Recentering </font> 当接收不到玩家输入时，自动在水平方向重新居中。
   * <font color = DodgerBlue face="加粗"> Enable </font> 是否启用。选中以启用自动水平居中。
   * <font color = DodgerBlue face="加粗"> Wait Time </font> 等待时间。如果在水平轴上未检测到用户输入，则相机将等待几秒钟，然后再进行重新居中。
   * <font color = DodgerBlue face="加粗"> Recentering Time </font>  重新居中所花费的时间。



<table><tr><td bgcolor=#6495ED><font color=black size=5> --------Same As Follow Target-------- </font> </td></tr></table>此算法基于玩家的输入来调节相机的旋转
这个算法会让相机和Follow Target一个方向。如果想让相机和目标保持同样的位置和朝向，可以在Body中使用Hard Lock to Target，在Aim中使用Same As Follow Target算法。






<br><br><br><br><br><br><br><br>

* [返回](#000)
  <h2 id = "005">5、Cinimachine Virtual Camera Noise </h2>
使用Noise属性可以模拟相机的晃动。Cinemachine中自带了一个Basic Multi Channel Perlin算法，可以给虚拟相机的运动添加柏林噪声。柏林噪声是一种随机算法技术，可以给相机添加比较自然的随机运动

<div align=center><img  src="MediaTimeline/Noise.png"/></div>

Basic Multi Channel Perlin需要设置一个<font color = DodgerBlue face="加粗"> Noise Profile </font>属性。Noise Profile是一个配置文件资产，里面有噪声的相关配置。Cinemachine中自带了几种profile配置，你也可以自己创建profile。<br><br>

<font color = Coral face="加粗"> Amplitude Gain </font> 振幅增益。相机最终的噪声振幅会是profile中的振幅乘以这个增益系数。设置为1时使用噪声配置文件中定义的振幅。将此设置为0时禁用噪声。提示：给这个属性添加动画可以通过动画控制噪波增强、减弱的效果。

<font color = Coral face="加粗"> Frequency Gain </font>：频率增益。相机最终的噪声频率会是profile中的频率乘以这个增益系数。设置为1时使用噪声配置文件中定义的频率。使用较大的值可以更快地晃动相机。提示：给这个属性添加动画可以通过动画控制噪波变快、变慢的效果。

<table><tr><td bgcolor=#6495ED><font color=black size=5> --------如何创建或修改Noise Profile-------- </font> </td></tr></table>

<font color = Coral face="加粗"> Noise profile </font>是定义程序化生成噪声曲线相关属性的资产。<font color = Coral face="加粗"> Basic Multi Channel Perlin </font>算法使用这个profile来控制相机的运动。Cinemachine在计算相机的位置后会添加噪声造成的偏移。这样，相机噪声不会影响将来相机运动的计算。
<br><br>
Cinemachine包含一些预定义的profile资产。你还可以在CinemachineVirtualCamera组件的Noise Profile属性上来修改、克隆、定位位置、新建。
    <div align=center><img  src="MediaTimeline/Noise0.png"/></div>

我先Clone出来一个6D Shake的Profile。

选中这个Noise Profile后，Inspector中可以直观地显示噪声曲线。x，y和z轴有位置和旋转的属性。每个轴可以具有多个层。
<div align=center><img  src="MediaTimeline/Noise1.png"/></div>

如果想创建逼真的程序化噪声，需要搞懂并选择合适的频率和振幅，以确保产生的噪声质量，噪声不会出现重复的情况。最有说服力的相机晃动还得使用<font color = Coral face="加粗"> Rotation </font>噪点，这会影响相机的瞄准。手持摄像机通常情况会有更多的旋转晃动，而不是位置移动。一般先添加<font color = Coral face="加粗"> Rotation旋转 </font>噪声，再添加<font color = Coral face="加粗"> Position位置 </font>噪声。<br><br>

令人信服的噪声曲线通常将低频，中频和高频混合在一起。创建新的noise profile时，可以从每个轴的这三层噪声开始。<br><br>

对于振幅（Amplitude），较大的值相机晃动越明显。对于远景，使用较小的振幅值，因为较小的FOV会放大晃动效果。<br><br>


对于频率（Frequency），典型的低频范围为0.1-0.5 Hz，中频范围为0.8-1.5，高频范围为3-4。最高可用频率取决于游戏的帧频。游戏通常以30或60Hz运行。高于游戏帧速率的噪声频率可能会出现采样出现跳跃的情况。<br><br>


例如，如果你的游戏以60帧/秒的速度运行，并且将噪声频率设置为100，则相机会发出断断续续的噪声。这是因为你的游戏渲染帧率无法比噪声频率更快。<br><br>

<font color = Coral face="加粗"> 属性详解 </font>
<font color = Coral face="加粗"> Preview Time  </font>在Inspector中的图形预览的时长。仅用于可视化预览，不会影响你编辑的噪声配置文件资产的内容。
<font color = Coral face="加粗"> Preview Height Inspector </font>中噪声曲线图的垂直高度。仅用于可视化预览，不会影响你编辑的噪声配置文件资产的内容。
<font color = Coral face="加粗"> Animated </font> 勾选后，Inspector中的噪声曲线会水平移动。示仅用于可视化预览，不会影响你编辑的噪声配置文件资产的内容。

<font color = Coral face="加粗"> Position Noise </font> 位置噪波图层。
<font color = Coral face="加粗"> Position X, Position Y, Position Z </font> 每个轴都可以设置多层的噪波。每个轴有一个图形展示。可以设置多层，点击下面的+或-可以添加或删除一层。
   * <font color = DodgerBlue face="加粗"> Frequency </font> 频率 噪声层中的频率，以Hz为单位。
   * <font color = DodgerBlue face="加粗"> Amplitude </font> 振幅 噪声层中波的振幅（高度），以距离为单位。
   * <font color = DodgerBlue face="加粗"> Non-random </font>  选中时不给波形添加柏林噪声。在没有Perlin噪声的情况下，Cinemachine使用规则的正弦波。取消选中可将Perlin噪声应用于该层，从而使频率和振幅随机化，同时保持在所选值的附近。

<font color = Coral face="加粗"> Rotation Noise </font>  所有轴的所有旋转噪波图层。
<font color = Coral face="加粗"> Rotation X, Rotation Y, Rotation Z </font>  每个轴都可以设置多层的噪波。每个轴有一个图形展示。可以设置多层，点击下面的+或-可以添加或删除一层。
   * <font color = DodgerBlue face="加粗"> Frequency频率</font> 噪声层中的频率，以Hz为单位。
   * <font color = DodgerBlue face="加粗">Amplitude振幅 </font> 噪声层中波的振幅（高度），以度数为单位。
   * <font color = DodgerBlue face="加粗"> Non-random</font> 选中时不给波形添加柏林噪声。在没有Perlin噪声的情况下，Cinemachine使用规则的正弦波。取消选中可将Perlin噪声应用于该层，从而使频率和振幅随机化，同时保持在所选值的附近。



<br><br><br>* [返回](#000)
<h1 id="006"> 6.1、Managing & Grouping Virtual Cameras </h1>
 虚拟相机分组管理器。相机管理器可以检测多个虚拟相机，但是对于Cinemachine和Timeline来说他们相当于还是一个虚拟相机。
  <font color = Coral face="加粗"> Free Look Camera </font>: 增强版的Orbital Transposer相机. 它管理三个水平轨道，垂直排列包围一个avator.

  <font color = Coral face="加粗">Mixing Camera </font>:最多使用八个子虚拟摄像头的加权平均值.

  <font color = Coral face="加粗"> Blend List Camera</font>: 执行其子虚拟摄影机的混合或剪切序列。.

  <font color = Coral face="加粗">Clear Shot Camera </font>: 选择具有目标最佳视图的子虚拟摄影机。

  <font color = Coral face="加粗">State-Driven Camera </font>: 选择子虚拟摄影机以响应动画状态的更改。

管理器相机和普通虚拟相机行为一样，可以通过多种相机组合实现复杂的需求。





<br><br><br>* [返回](#000)
<h1 id="007"> 15、FreeLook Camera </h1>
<font color = Coral face="加粗"> Cinemachine FreeLook Camera </font> 他提供了一个一种第三人称的相机体验。由顶、中，低三个单独的相机设置来围绕主体旋转。
 <div align=center><img  src="MediaTimeline/CinemachineFreeLook.png"/></div>

每个装配都围绕目标定义了一个环，有自定义的半径，高度，和旋转，noise的设置
 <div align=center><img  src="MediaTimeline/CinemachineFreeLookMenue.png"/></div>



<br><br><br>* [返回](#000)
<h1 id="008"> 8、Blend List Camera </h1>
<font color = Coral face="加粗"> Cinemachine Blend List Camera </font> 改类型虚拟相机激活后按添加的虚拟相机列表按设置的持续时间和混合方式依次进行混合播放，全波播放完成保持最后一个相机的状态。
 <div align=center><img  src="MediaTimeline/CinemachineBlendListMenue.png"/></div>






<br><br><br>* [返回](#000)
<h1 id="009"> 9、State-Driven Camera </h1>
<font color = Coral face="加粗"> Cinemachine State-Driven Camera </font> 状态驱动相机，通过该相机可以对每个动画指定特定的相机关注。 当动画目标变更状态时，Cinemachine State-Driven Camera会激活子虚拟摄像机。 例如Avator运动时，相机晃动的更多。 待机时处于一个自由相机下. 每个相机间的切换可以配置融合，每个动画可以配置一个指定的相机。
 <div align=center><img  src="MediaTimeline/CinemachineStateDrivenCamera.png"/></div>







<br><br><br>* [返回](#000)
<h1 id="0100"> 10.0、相机拓展 </h1>

<h3 color = Coral> Avoiding collisions and evaluating shots  避免碰撞和快速评估</h3>
<table><tr><td bgcolor=#6495ED><font color=black size=5> --------Cinemachine Collider-------- </font> </td></tr></table>
<font color = Coral face="加粗">Cinemachine Collider </font>是Cinemachine虚拟摄像机的扩展。它对虚拟摄像机的最终位置进行后处理，以尝试保留虚拟摄像机的“注视”目标的视线。它是通过远离阻碍视图的GameObjects来实现的。<br>
将<font color = Coral face="加粗">Cinemachine Collider </font>扩展添加到Cinemachine虚拟摄像机以执行以下任何任务：<br>

   * 将相机推开，使其不妨碍场景中的障碍物。<br>
   * 将相机放置在虚拟相机与其“ 看向”目标之间的障碍物前面。<br>
   * 评估镜头质量。拍摄质量是虚拟相机到其理想位置的距离，虚拟相机到其目标的距离以及阻碍目标视线的障碍物的度量。其他模块使用镜头质量，包括<font color = Coral face="加粗">Clear Shot </font>。<br>
   * 对撞机使用<font color = Coral face="加粗"> Physics Raycaster </font>。因此，Cinemachine Collider要求潜在障碍物具有碰撞体体积。此要求会产生性能成本。如果您的游戏费用高昂，请考虑以其他方式实现此功能。<br>
 <div align=center><img  src="MediaTimeline/CinemachineCollider.png"/></div>

<font color = Coral face="加粗">Collide Against </font>碰撞:Cinemachine Collider认为这些层中的GameObjects是潜在的障碍。它会忽略不在选定图层中的GameObject。

<font color = Coral face="加粗">Minimum Distance From Target </font>: 忽略距离目标枢轴点小于此距离的障碍物。

<font color = Coral face="加粗">Avoid Obstacles </font>: 当目标被障碍物遮挡时，请选中以允许对撞机在场景中移动相机。使用距离限制，相机半径和策略属性来调整如何避开障碍物。如果未选中，Cinemachine Collider将根据障碍物报告镜头质量，但不会尝试移动相机来改善镜头质量。

<font color = Coral face="加粗">Distance Limit </font>检查到本相机目标的视线是否清晰时的最大光线投射距离。输入0以使用当前到目标的实际距离。当选中避免障碍物时可用。

<font color = Coral face="加粗">Camera Radius </font>与任何障碍物保持的距离。尝试保持较小的值以获得最佳结果。如果由于摄像机的FOV大而看到内部障碍物时，请增大该值。当选中避免障碍物时可用。

<font color = Coral face="加粗">Strategy </font>对撞机试图保留目标视线的方式。当选中避免障碍物时可用。

   * <font color = DodgerBlue face="加粗"> Pull Camera Forward </font> 沿其Z轴向前移动相机，直到它位于距离目标最近的障碍物的前面。
   * <font color = DodgerBlue face="加粗"> Preserve Camera Height </font> 尝试将相机移至另一视角，同时尝试将其保持在其原始高度。
   * <font color = DodgerBlue face="加粗"> Preserve Camera Distance </font> 尝试将相机移动到另一视角，同时尝试将相机保持在与目标之间的原始距离。
   * <font color = DodgerBlue face="加粗"> Smoothing Time </font> 将相机保持在距目标最近的位置的最小秒数。在有许多障碍物的环境中可用于减少过多的相机移动。当选中避免障碍物时可用。

<font color = Coral face="加粗"> Damping</font>遮挡消失后，将相机恢复到其正常位置的速度。较小的数字会使相机反应更快。较大的数字会使相机响应速度变慢。当选中避免障碍物时可用。
<font color = Coral face="加粗"> Damping When Occluded</font>相机移动速度如何避免障碍。较小的数字会使相机反应更快。较大的数字会使相机响应速度变慢。当选中避免障碍物时可用。
<font color = Coral face="加粗">Optimal Target Distance </font>如果大于零，则当目标距离该距离较近时，给镜头更高的分数。将此属性设置为0以禁用此功能。


<table><tr><td bgcolor=#6495ED><font color=black size=5> --------Cinemachine Confiner-------- </font> </td></tr></table>
Cinemachine Confiner 扩展程序将摄像机的位置限制在一个体积或区域内。Confier以2D或3D模式运行。模式影响其接受的边界形状的种类。在3D模式下，相机在3D模式下的位置仅限于一个体积。这也适用于2D游戏，但您需要考虑深度。在2D模式下，您不必担心深度。

对于正交相机，还有一个附加选项可以限制屏幕边缘，而不仅仅是相机点。这样可以确保整个屏幕区域都位于边界区域内。
<font color = Coral face="加粗">Confine Mode</font>使用2D边界区域或3D边界体积进行操作。

   * <font color = DodgerBlue face="加粗">Confine 2D</font>使用Collider2D边界区域。
   * <font color = DodgerBlue face="加粗">Confine 3D</font>使用3D Collider边界体积。

<font color = Coral face="加粗">Bounding Volume</font>包含相机的3D体积。当“限制模式”设置为“限制3D”时，此属性可用。

<font color = Coral face="加粗">Bounding Shape 2D</font>包含照相机的2D区域。当“限制模式”设置为“限制2D”时，此属性可用。

<font color = Coral face="加粗">Confine Screen Edges</font>正交摄影机时，请检查以将屏幕边缘限制在该区域。取消选中时，仅限制摄像机中心。如果相机处于透视模式，则无效。

<font color = Coral face="加粗">Damping</font>如果超出边界，如何逐渐将相机返回到包围的体积或区域。更高的数字更渐进。

 <div align=center><img  src="MediaTimeline/CinemachineConfiner.png"/></div>

<br><br><br>




<table><tr><td bgcolor=#6495ED><font color=black size=5> --------Cinemachine Follow Zoom-------- </font> </td></tr></table>
此扩展功能可调节镜头的FOV，以使目标物体在屏幕上保持恒定大小，而不管相机和目标位置如何。

 <div align=center><img  src="MediaTimeline/FollowZoom.png"/></div>
FollowZoom.png
<font color = Coral face="加粗">Width</font>射程宽度，以世界单位保持在目标距离。将调整FOV，以使目标距离下此大小的对象充满整个屏幕。

<font color = Coral face="加粗">Damping</font>增大此值可软化跟随缩放的响应性。较小的数字会使相机反应更快。较大的数字会使相机响应速度变慢。

<font color = Coral face="加粗">Min FOV</font>此行为生成的FOV的下限。

<font color = Coral face="加粗">Max FOV</font>此行为生成的FOV的上限。



<br><br><br>
<table><tr><td bgcolor=#6495ED><font color=black size=5> --------CinemachinePostProcessing-------- </font> </td></tr></table>
使用Cinemachine后处理扩展将后处理V2配置文件附加到虚拟相机。
Cinemachine Post Processing扩展拥有一个后处理配置文件资产，以便在激活后应用于虚拟摄像机。如果摄像机正在与其他虚拟摄像机混合，则混合权重也将应用于后期处理效果。

<font color = Coral face="加粗">Focus Tracks Target</font>: 检查以将“聚焦距离”设置为相机与“注视”目标之间的距离。
<font color = Coral face="加粗">Offset</font>: 选中“聚焦轨迹目标”后，在设置聚焦，聚焦距离时，此偏移量将应用于目标位置。如果没有“注视”目标，则这是与Unity相机位置（实际焦距）的偏移量。


<br><br><br>
<table><tr><td bgcolor=#6495ED><font color=black size=5> --------Cinemachine Storyboard-------- </font> </td></tr></table>
 <div align=center><img  src="MediaTimeline/StoryBoard.png"/></div>

<font color = Coral face="加粗"> Show Image </font>:切换情节提要图像的可见性。

<font color = Coral face="加粗"> Image </font>:图像显示为虚拟相机输出的覆盖图。

<font color = Coral face="加粗"> Aspect </font>:如何处理图像外观和屏幕外观之间的差异。

   * <font color = DodgerBlue face="加粗">Best Fit</font>在屏幕上将图像调整为尽可能大的尺寸，而不进行裁切。保留垂直和水平比例。
   * <font color = DodgerBlue face="加粗">Crop Image To Fit</font>调整图像大小以填满屏幕，必要时裁切。保留垂直和水平比例。
   * <font color = DodgerBlue face="加粗">Stretch To Fit</font>调整图像大小以填满屏幕，必要时调整垂直或水平宽度。

<font color = Coral face="加粗"> Alpha </font>:图像的不透明度。使用0表示透明，使用1表示不透明。

<font color = Coral face="加粗"> Center </font>:图像的屏幕空间位置。使用0作为中心。

<font color = Coral face="加粗"> Rotation </font>:图像的屏幕空间旋转。

<font color = Coral face="加粗"> Scale </font>:图像的屏幕空间缩放。

<font color = Coral face="加粗"> Sync Scale </font>:检查以同步x和y轴的比例。

<font color = Coral face="加粗"> Mute Camera </font>:检查以防止虚拟摄像机更新Unity摄像机的位置，旋转或比例。使用此功能可以防止时间轴将摄像机混合到场景中的意外位置。

<font color = Coral face="加粗"> Split View </font>:水平擦拭图像。





<br><br><br>* [返回](#000)
<h1 id="010"> 10、Cinemachine ClearShot Camera </h1>

 <font color = Coral face="加粗">Cinemachine ClearShot Camera </font> 组件会给目标从他的子VC中选取最优视野评估的相机 。使用<font color = Coral face="加粗">Clear Shot</font>  可以设置场景的复杂多摄像机覆盖范围，以确保清晰地看到目标。

 这可能是一个非常强大的工具。具有Cinemachine Collider扩展功能的Virtual Camera child可以分析场景中的目标障碍物，最佳目标距离等。Clear Shot使用此信息来选择最佳的孩子来激活。

<font color = Coral face="加粗">提示 </font> ：要将单个<font color = Coral face="加粗">Cinemachine Collider </font>用于所有Virtual Camera子代，请将Cinemachine Collider扩展添加到ClearShot GameObject，而不是其每个Virtual Camera子元素。此<font color = Coral face="加粗"> Cinemachine Collider </font>扩展适用于所有子元素，好像每个孩子都有该Collider作为其自己的扩展一样。

如果多个子相机具有相同的拍摄质量，则“清晰拍摄”相机会选择优先级最高的相机。最终每个相机的视野清晰优先级由<font color = Coral face="加粗">Cinemachine Collider </font>设置计算的。

您还可以在ClearShot子项之间定义自定义混合。




 <div align=center><img  src="MediaTimeline/CinemachineClearShot.png"/></div>

<font color = Coral face="加粗"> Cinemachine ClearShot Camera </font> 
允许设置多个子相机，会自动选取最佳匹配的

<font color = Coral face="加粗"> Activate After </font>等待这几秒钟，然后再激活新的子相机。

<font color = Coral face="加粗"> Min Duration </font>除非有更高优先级的相机处于活动状态，否则活动的相机必须至少处于活动状态至少几秒钟。

<font color = Coral face="加粗"> Randomize Choice </font>如果多个摄像机具有相同的拍摄质量，请检查以选择随机摄像机。取消选中以使用子虚拟摄像机的顺序及其优先级。



<br><br><br>* [返回](#000)
<h1 id="014"> 14、Mixing Camera </h1>
<font color = Coral face="加粗"> Cinemachine Mixing Camera</font> 使用其子虚拟摄像机的加权平均来计算位置和unity相机的其他属性。
   <div align=center><img  src="MediaTimeline/CinemachineMixingCameraMenue.png/></div>

<font color = Coral face="加粗"> Cinemachine Mixing Camera</font> 最多可管理八个子虚拟摄像头。在混合摄像机组件中，这些虚拟摄像机是固定插槽，而不是动态阵列。混合摄像头使用此实现在时间轴中支持权重动画。时间轴无法为数组元素设置动画。


<br><br><br>* [返回](#000)
<h1 id="015"> 15、Cinemachine Impulse </h1>
响应游戏事件生成并管理相机震动。例如当两个GO碰撞或爆炸时，可以使用Impluse使VC抖动。
   
   * <font color = DodgerBlue face="加粗"> Raw vibration signal </font> ：原始脉冲信号：可以设置位置xyz，旋转pitch，roll，yaw6个维度的曲线。 有已经定义好的几种脉冲源。
   * <font color = DodgerBlue face="加粗"> Impulse Source</font> ：从场景中一点发射脉冲源的组件，同时定义信号特征 例如 持续时间，强度和振幅。
   * <font color = DodgerBlue face="加粗"> Impulse Listener </font>: 脉冲监听组件，vc的拓展组件，可以监听脉冲通过它抖动。

   <div align=center><img  src="MediaTimeline/ImpluseMenue.png"/></div>
 
 <font color = Coral face="加粗"> Amplitude Gain </font>：振幅增益。1表示完全引用振源。0静音。
 <font color = Coral face="加粗"> Frequency Gain </font>： 振幅增益。

 <font color = Coral face="加粗"> Cinemachine impluse Sources </font>: 场景中固定存在的振源。
 <font color = Coral face="加粗"> Cinemachine Collision impluse Sources </font>: 碰撞触发的震源
 








<br><br><br>
<h1 id="016"> 16、UpDown Game </h1>

<br><br><br><br><br><br><br><br><br><br><br>
* [返回](#000)
<h1 id="999"> End</h1>
    Extensions:
        Cinemachine Camera Offset:
        Cinemachine Collider:
        Cinemachine Confiner:
        Cinemachine Stotryboard:
        Cinemachine Impulse Listener:
        Cinemachine Post Processing:






