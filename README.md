# ArcGIS的二次开发
一个完整的ArcGIS二次开发项目，具有根据要素字段进行模糊查询的功能

运行C#程序后，在打开的Arcmap中选择Customize，找到Customize mode工具，在弹出的窗口中选择Commands找到“查询工具”，将Query按钮拖入Arcmap。导入地图后即可点击Query按钮进行查询。
 
# 1.1 登录界面（MainForm）

 ![image](https://github.com/user-attachments/assets/7f56e39f-eba9-4dd9-95d2-bfb8687add86)

登录界面主要的用途是作为程序的入口，其中的用户名和密码均为设置好并且默认输入的，当然也自行输入，点击Forgot Password可以看到默认设置的用户名和密码。


![image](https://github.com/user-attachments/assets/0362ef74-200c-4427-ae01-d4bf4320131d)

 
用户名和密码无误后，点击Login按钮，即可进入查询界面，如果用户名或者密码错误将会弹出提示。同时界面有一个Exit按钮用来退出程序。
# 1.2 查询界面（QueryForm）

 ![image](https://github.com/user-attachments/assets/7a45182d-2180-4c1f-bfe5-a2f1863c93a8)

查询界面完成了程序的主要功能：进行地图要素的查询。程序会遍历所有图层以及他们的字段，并默认显示第一个图层，选择Name字段进行查询（没有Name字段则选择第一个字段）。选择字段选项存在的意义在于让用户知道自己在使用什么字段进行查询，而默认选择Name字段是因为Name字段使用的频率最高，输入地物的名称即可进行模糊查询。这里输入“馆”进行演示：
如果有相应的名称则会提示找到了匹配的要素。 

 ![image](https://github.com/user-attachments/assets/dc7c50c0-525d-4487-a4e4-91cf09eca9fb)

与此同时，Arcmap中的地图移动到相应的区域，并且对查询到的要素进行高亮并闪烁显示。并且会弹出下一个要介绍的属性显示界面（FinalForm）。

![image](https://github.com/user-attachments/assets/2474f91a-98b3-4dac-a6b5-4846769c0a0a)

 
# 1.3 属性显示界面（FinalForm）

![image](https://github.com/user-attachments/assets/1e4d5eb1-c3dc-4386-a821-f8226c80a10a)

属性显示界面主要有两个作用：一方面是对查询到的要素进行其属性的显示，这里点击某个单元格可以显示其中的详细内容，可以更好的与用户进行交互。
这是点击了建筑馆的Remark字段，可以更清楚的看到其中的内容。

 ![image](https://github.com/user-attachments/assets/b87c2071-cdae-4063-92e8-30a33833bf90)

另一个功能则是可以对查询到的要素进行二次选择，可以看到的是，如果所有查询到的要素均高亮显示，会导致用户难以分辨某个具体的要素，但我的这个界面可以很好的解决这个问题，在这个界面上点击某个要素的任一字段，即可在地图上将这个要素单独高亮出来，比如我点击建筑馆后，可以看到，建筑馆被单独高亮闪烁显示了出来，这样可以更好的让用户将建筑及其名称对应起来。

 ![image](https://github.com/user-attachments/assets/64411169-c179-4893-b788-6a8f543d022c)

