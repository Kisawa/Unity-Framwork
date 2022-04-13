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
## UI模块仅整理了fgui相关（项目自带一个fgui sdk）  
* Fgui配置文件：  

 ![image](https://user-images.githubusercontent.com/71002504/163169210-c808596b-2a1b-45a1-9a8b-64a4e7ed9d35.png)  
 
* 全局唯一的UI类型，继承自SingleFgui，例如游戏主窗口：

![image](https://user-images.githubusercontent.com/71002504/161743816-a17ef5f5-f854-44aa-bca3-ff10cfe1f368.png)  

* 允许多次实例化的UI类型，继承自NoSingleFgui，例如提示窗口：

![image](https://user-images.githubusercontent.com/71002504/163168124-9031518e-37ab-4f68-92d4-8e9d85f00764.png)  

* 主UI选择，它将在项目运行时加载：  

![image](https://user-images.githubusercontent.com/71002504/163168918-4dc3dafe-8a93-4d1c-942a-748d0f187911.png)  

* 选择游戏开始时便加载到内存的UI：

![image](https://user-images.githubusercontent.com/71002504/163168810-e0b17e46-ff64-4b3f-b9ac-4bb9c00a9d61.png)  
