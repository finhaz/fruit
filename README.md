# fruit

#### 介绍
这是一个串口通讯软件，基础功能是读取数据存入数据库，同时将数据发给下层，修改下层的扩展变量。附加功能是对针对微网控制，传送的数据进行数据处理，采用过粒子群、鱼群等算法。

#### 软件架构
Winform版本  
主要用c#语言开发，基于微软的.net平台，form1为主界面，form2为串口配置窗口，from3为微网数据窗口，数据库为accees数据库，接口函数专门在Database.cs,通讯协议在Message.cs,其余的智能算法位于XX_AI.cs.  
WPF版本  
还在整理中，初步考虑是多个page来管理，取代以前的多窗口  
第一个page是串口助手，参考了别人的代码，取代原来form2的串口配置界面  
第二个page是将原来的form1界面迁移，实现通讯  
第三个page计划是把算法的界面移植过来  
第四个page计划是做个帮助说明  



#### 开源地址
主要地址：https://gitee.com/finhaz/fruit
其他地址：https://github.com/finhaz/fruit
