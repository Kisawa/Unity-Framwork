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
* 全局唯一的UI类型，继承自SingleFgui：

![image](https://user-images.githubusercontent.com/71002504/161743816-a17ef5f5-f854-44aa-bca3-ff10cfe1f368.png)  

* 运行多次实例化的UI类型，继承自NoSingleFgui：

![image](https://user-images.githubusercontent.com/71002504/163168124-9031518e-37ab-4f68-92d4-8e9d85f00764.png)  

* fgui主ui选择，它将在项目运行时加载：  

![image](https://user-images.githubusercontent.com/71002504/161743443-584f2d82-f426-43f3-8f5d-1137f46b0955.png)  
