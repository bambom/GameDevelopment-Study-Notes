
  * [Cinemachine](#01)

  <h3 id = "#01">Cinemachine</h3>

Cinemachine Brain： 所有相机组件的重要组件，控制主相机，同时允许用创建和控制许多不同的虚拟相机。
Show Debug Text: 显示当前是哪个虚拟相机控制的主相机。 这里在多个相机切换时候，会以最后一个激活的相机作为当前主控制相机。
Show Camera Frustum:显示相机裁剪范围。
Ignore Time Scale: 忽略timescale的影响，不受unity加减速的影响。
World Up Override： 默认空相机使用世界Y作为构建视图矩阵的up向量。拖拽物体并旋转下，就会使用当前这个物体的Y方向作为构建up的向量。
Update Method: 更新相机位置和旋转的时机。 如果由于更新地方导致的不流畅可以在这里进行选择更新时机。
Default Blend：默认的相机融合时的国度曲线和持续时间。
Custom Blends: 如果选择了自定义的融合，就会覆盖默认的。自定义的可以定义多个相机的间的融合关系。
Events：
Camera cut：任何虚拟相机要进入控制并且没有融合的时候切入到这个相机前，出发该事件。
Camera Actived：相机激活的时候出发，cut或融合的方式都在第一帧触发该事件。

CinimachineVirtualCamera: 是使用时间轴创建相机行为和镜头的关键。 
调整死区和软区，相机在死区内相机讲不会再次范围内旋转。在软区种，相机将按照相机上设置的衰减阐述在 软区内跟踪目标，逐渐重新对准目标。减震决定了在追踪目标时相机的松度或刚性。

