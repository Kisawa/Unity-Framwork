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
* 集合了 __Resources__ 和 __Addressables__ 两种模式  
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
## UI模块仅整理了Fgui相关（项目自带一个Fgui sdk）  
* Fgui配置文件：  
>__CommonPackName：__   
>>Resources固定路径： Resources/FguiAssets/... （参考Fgui相关）  
>>Addressables路径： Addressables资源配置名  

>__FguiFontAssetName：__  
>>Resources路径： 完整路径名  
>>Addressables路径： Addressables资源配置名  

 ![image](https://user-images.githubusercontent.com/71002504/163169210-c808596b-2a1b-45a1-9a8b-64a4e7ed9d35.png)  
 ![image](https://user-images.githubusercontent.com/71002504/163169347-aa9d303b-0ffd-4e15-a303-99a31ad01152.png)  
 
* 全局唯一的UI类型，继承自 __SingleFgui__ ，例如游戏主窗口：

![image](https://user-images.githubusercontent.com/71002504/161743816-a17ef5f5-f854-44aa-bca3-ff10cfe1f368.png)  

* 允许多次实例化的UI类型，继承自 __NoSingleFgui__ ，例如提示窗口，可以配合IObjectPool接口作为对象池使用：

![image](https://user-images.githubusercontent.com/71002504/163168124-9031518e-37ab-4f68-92d4-8e9d85f00764.png)  

* 主UI选择，它将在项目运行时加载：  

![image](https://user-images.githubusercontent.com/71002504/163168918-4dc3dafe-8a93-4d1c-942a-748d0f187911.png)  

* 选择游戏开始时便加载到内存的UI：

![image](https://user-images.githubusercontent.com/71002504/163168810-e0b17e46-ff64-4b3f-b9ac-4bb9c00a9d61.png)  
****
## DataTable数据表模块  
* 用于数据表的加载，所有数据表需要导出为 __txt__ 格式，表头等备注类型添加 __“#”__ 作为注释行  

![image](https://user-images.githubusercontent.com/71002504/163183448-576f1c0e-62fd-4c10-a54a-6ba91b028cf6.png)
![image](https://user-images.githubusercontent.com/71002504/163183563-de4ab4f4-aca8-42c6-9827-6bc293dbb82a.png)  

* 为数据表创建脚本，继承自 __DataTableUtility__ ：  

![image](https://user-images.githubusercontent.com/71002504/163184317-3a5f56ef-4009-437e-a90e-29ab6da049ec.png)  

* 在项目入口的控制台可以控制表是否在开始时加载：  

![image](https://user-images.githubusercontent.com/71002504/163184940-674da93a-65da-4ad0-bff8-333cefe05b45.png)  
****
## Json数据表模块  
![image](https://user-images.githubusercontent.com/71002504/163185984-1549010e-7d34-4edb-b5fe-a47389cb9399.png)  

* 为Json数据表创建脚本，继承自 __JsonDataUntility__ ：  

![image](https://user-images.githubusercontent.com/71002504/163191151-e35eb8e3-8902-4241-bb78-ec56417ffab6.png)  

* 为脚本中的变量妥善选择 __Attribute__ ，它会被识别为Json的数据对象并填充表数据：  
| 特性 | 类型 |
| ------ | ------ |
| [JsonField] | 普通类型，支持string, int, float等基础类型，以及Array, List列表类型 |
| [JsonFieldGroup] | 自定义类型，支持class, struct |
