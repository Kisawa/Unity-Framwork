# Unity-Framwork
此项目模块化了本人在游戏开发过程中程序向的各种工具  
仅为随笔方便日后查看  
****
## 项目入口：  
* 这里集合了游戏开始时的资源加载  

![image](https://user-images.githubusercontent.com/71002504/161743269-14b534f1-9a6a-4a65-81ed-4bb25ae3431a.png)  
* 配置加载序列  

![image](https://user-images.githubusercontent.com/71002504/163158287-a966fe27-8179-4ea6-9979-b7e7505e76e5.png)  
****
## 资源管理类  
* 集合了Resources和Addressables两种模式  
>使用Addressables  
>>在PackageManager中安装Addressables  
>>在ProjectSetting->Player->OtherSetting中添加全局Define：“ADDRESSABLES”  

* 自动管理资源的加载和释放  
>普通资源的加载与卸载，例如Sprite TextAsset等  
>>__AssetManagment.cs__  

>实例化物体以及销毁：  
>>__GameObjectReference.cs__  

* 资源的链结  
链结主要用于资源的卸载，例如一个GameObject加载后它可能会使用其他Sprite资源，这个功能可以确保可能被使用的资源时刻在内存中共存
>Asset与Asset：  
>GameObject与Asset：  
>>__AssetManagment.LinkAsset__  

>GameObject与GameObject：  
>GameObject与ObjectPool：  
>>__GameObjectReference.LinkInstance__  

* __AssetGroup.cs__  
使用它同时加载所有被链结资源，它会将Group中的资源一起打包加载并完成链结操作  
![image](https://user-images.githubusercontent.com/71002504/163180309-123116f3-6557-41d1-8214-e5e75c121b05.png)  
****
## UI模块仅整理了fgui相关（项目自带一个fgui sdk）  
* Fgui配置文件：  
>__CommonPackName：__   
>>Resources固定路径： Resources/FguiAssets/... （参考Fgui相关）  
>>Addressables路径： Addressables资源配置名  

>__FguiFontAssetName：__  
>>Resources路径： 完整路径名  
>>Addressables路径： Addressables资源配置名  

 ![image](https://user-images.githubusercontent.com/71002504/163169210-c808596b-2a1b-45a1-9a8b-64a4e7ed9d35.png)  
 ![image](https://user-images.githubusercontent.com/71002504/163169347-aa9d303b-0ffd-4e15-a303-99a31ad01152.png)  
 
* 全局唯一的UI类型，继承自SingleFgui，例如游戏主窗口：

![image](https://user-images.githubusercontent.com/71002504/161743816-a17ef5f5-f854-44aa-bca3-ff10cfe1f368.png)  

* 允许多次实例化的UI类型，继承自NoSingleFgui，例如提示窗口，可以配合IObjectPool接口作为对象池使用：

![image](https://user-images.githubusercontent.com/71002504/163168124-9031518e-37ab-4f68-92d4-8e9d85f00764.png)  

* 主UI选择，它将在项目运行时加载：  

![image](https://user-images.githubusercontent.com/71002504/163168918-4dc3dafe-8a93-4d1c-942a-748d0f187911.png)  

* 选择游戏开始时便加载到内存的UI：

![image](https://user-images.githubusercontent.com/71002504/163168810-e0b17e46-ff64-4b3f-b9ac-4bb9c00a9d61.png)  
