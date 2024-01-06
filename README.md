<div align = "center">

<h1>FIRM ICE</h1>
基于Unity的TPS项目

![Unity](https://img.shields.io/badge/Unity-000000.svg?style=flat-square&logo=unity&logoColor=white)
![C#](https://img.shields.io/badge/C%20Sharp-512BD4.svg?style=flat-square&logo=csharp&logoColor=white)
![GLSL](https://img.shields.io/badge/GLSL-5586A4.svg?style=flat-square&logo=opengl&logoColor=white)<br>
![Unity](https://img.shields.io/badge/Unity-2023.2.3f1-black?style=flat-square&logo=unity)
</div>

---
>项目目前处于早期规划阶段，目前还没有想好如何介绍这个项目。<br>
>等空下来应该会好好规划并补全项目介绍。<br>

>大概整体流程会在策划案写出后进行，目前先需要构建能使游戏正常进行的基础代码逻辑。

>目前的进度概览<br>
-->底层代码构建 &emsp;&emsp; 进行中<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;-->角色行动逻辑 &nbsp;&emsp;&emsp; 进行中<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;-->敌人AI &nbsp;&emsp;&emsp;&emsp;&emsp;&emsp; 排队中<br>
-->基础策划&emsp;&emsp;&emsp;&emsp;&nbsp; 排队中  
                
## 项目结构
<details>
  <summary>Asset结构</summary>

```
-Asset                                                //主目录
|    -ACTOR                                           //用于存放对象
|    -ART                                             //一般的平面美术资源
|    |    -Materials                                  //Prefab中用到的材质
|    -AstarNavMesh                                    //A*寻路缓存的NavMesh
|    -Component                                       //引用的外部组件
|    -Editor                                          //UnityEditor工具
|    -Plugins                                         //引用的外部插件
|    -Prefab                                          //系统预制体
|    |    -LSS                                        //LoadScene存放处
|    |    |    -Resources
|    |    |    |    -Loading Screens                  //LSS的预制体
|    -SCENE                                           //场景文件
|    -Scripts                                         //主要脚本
|    |    -SelfMade
|    |    |    -Actor                                 //角色控制器相关基础脚本
|    |    |    -Common                                //全局通用
|    |    |    -Detecor                               //检测器
|    |    |    -FollowScripts                         //跟随核心
|    |    |    -FPSUSED(Aborted)                      //FPS相关(项目已不使用)
|    |    |    -FUNDUDE                               //娱乐效果脚本
|    |    |    -GunBattle                             //射击相关
|    |    |    -TestScripts                           //临时测试脚本暂存
|    |    |    -UI                                    //UI相关
|    -TempAsset                                       //临时文件暂存
|    -Trash                                           //文件暂存
```
</details>

## 声明
FirmIce并非商业项目，仅为个人爱好。<br>
FirmIce中的部分素材源自网络，但均已拥有合规使用权。
