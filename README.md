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

* 在项目入口的控制台可以控制某一个DataTable表是否在开始时加载：  

![image](https://user-images.githubusercontent.com/71002504/163184940-674da93a-65da-4ad0-bff8-333cefe05b45.png)  
****
## Json数据表模块  
![image](https://user-images.githubusercontent.com/71002504/163185984-1549010e-7d34-4edb-b5fe-a47389cb9399.png)  

* 为Json数据表创建脚本，继承自 __JsonDataUntility__ ：  

![image](https://user-images.githubusercontent.com/71002504/163191151-e35eb8e3-8902-4241-bb78-ec56417ffab6.png)  

* 为脚本中的变量妥善选择 __Attribute__ ，添加特性的变量会被识别为Json的数据对象并填充表数据：  

| Attribute | Type |
| ------ | ------ |
| JsonField | 普通类型，支持string, int, float等基础类型，以及Array, List列表类型 |
| JsonFieldGroup | 自定义类型，支持class, struct |

* 在项目入口的控制台可以控制某一个Json表是否在开始时加载：  

![image](https://user-images.githubusercontent.com/71002504/163192377-fd7fdffb-4de6-4861-952b-1f1eb947df56.png)  
****
## 本地数据模块
* 本地序列化数据在 __EasySave3__ 的基础上扩展了使用方法：  
>扩展了相关继承类，Runtime直接使用其中的变量并会在空闲以及结束时对其更新  
>使用 __Attribute（WaitingFreeToSave）__ 标记数据，实现自动保存与加载  
>扩展了对私有变量 __private__ 的存储  
>控制台数据可视化

* 为本地数据创建脚本，继承自 __LocalSaveUtility__ ：  

![image](https://user-images.githubusercontent.com/71002504/163330640-8739e8e2-e13a-4b07-b2ab-51b1d7a01d9c.png)  

* 项目入口的控制台查看与修改本地数据：  

![image](https://user-images.githubusercontent.com/71002504/163327276-4c98c0f9-1c3a-43ee-8052-c0934a258ee9.png)  

* 为脚本中你想要保存的数据变量添加 __Attribute__ ，下面列出了相关的特性：  

| Attribute | Description |
| ------ | ------ |
| WaitingFreeToSave(name) | 标记需要被保存的变量（键值对方式 name -> key） |
| Unsafe | 配合WaitingFreeToSave使用，标记此变量需要保存内部私有属性 |
| DepthUnsafe | 配合WaitingFreeToSave使用，类似Unsafe，在变量内部查找是否存在Unsafe特性标记的属性 |

* 清除本地数据  

![image](https://user-images.githubusercontent.com/71002504/163330490-871da4d8-9515-49bc-8a26-814350919493.png)  

* 解决Unity Il2cpp打包 __Reflection__ 反射报错的问题：  
错报的原因在于对自定义struct类型的加载和保存，反射代码时程序没有找到相关引用  
需要手动实现一下相关代码，参考 __Il2cppKeepCode.cs__ ，这里已经实现了对Unity常用struct类型的处理：  

![image](https://user-images.githubusercontent.com/71002504/163326224-5eae79a9-2c96-4274-9126-b923000f0171.png)  
****
## GameStart自定义资源集合加载  
* 游戏运行时需要同时加载的资源在这里标记，支持类型：  
>Prefab： Unity预制体  
>UnityAsset： Unity类型的资源，例如Sprite等  
>ObjectPool： Gameobject类型的对象池，自动为Prefab创建一个ObjectPool对象池  

* 创建一个集合脚本，继承自 __AssetUtility.cs__ ：  

![image](https://user-images.githubusercontent.com/71002504/163328471-564c33c1-8a24-4735-9fa5-cfde2fe82bc2.png)  

* 为资源标记加载方式以及路径的 __Attribute__ ：  

| Attribute | Description |
| ------ | ------ |
| Resources(path) | 标记此资源加载方式为 __Resources__ （path -> 路径） |
| Addressables(path) | 标记此资源加载方式为 __Addressables__ （path -> 路径） |

* 在项目入口的控制台可以控制某一个资源集合类是否在开始时加载：  

![image](https://user-images.githubusercontent.com/71002504/163329751-c291c54c-5901-42dc-a0ad-60b9f940a87f.png)  
****
## ObjectPool对象池  
* 对象池的类型有两种：  
>GmaeObject的对象池：  
>>![image](https://user-images.githubusercontent.com/71002504/163332148-51c1df59-2b1a-425b-b862-3404083de2ca.png)  

>自定义的对象池，需要实现IObjectPool接口：
>>![image](https://user-images.githubusercontent.com/71002504/163332637-9a6e70d7-6f1d-4881-bbf2-ca3693544522.png)  
>>![image](https://user-images.githubusercontent.com/71002504/163332909-be89eeb8-8ff7-40b1-9aaf-30a0db6fb49e.png)  
****
## 全局事件 EventUtility 与引用池 ReferencePool
* 创建一个事件，继承自 __EventUtility.cs__  
EventUtility类实现了 __IReferencePool__ 引用池接口， 在接口方法的 __Recircle()__ 中重置事件参数  
_自定义一个静态方法用于在引用池中取出一个事件实例，当然也可以在触发事件时手动实例化，对于会多次触发的事件使用引用池会更好：_  

![image](https://user-images.githubusercontent.com/71002504/163336336-f4742db1-b935-4b11-8f4c-9486701ffc89.png)  

* 在适合的地方触发这个事件：  

![image](https://user-images.githubusercontent.com/71002504/163336871-2de901a3-000e-49ad-abb4-cd2d2b4bcfed.png)  
