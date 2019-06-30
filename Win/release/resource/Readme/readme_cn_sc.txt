====================================================
PPDInstaller

2010/06/24
http://projectdxxx.blog9.fc2.com/
====================================================

○关于本软件

  本软件用于安装与PPD相关的软件。
  本软件製作者没有义务对使用本软件出现的问题负责。

○如何使用
请运行PPDInstaller.exe,并按照指示操作。

○如何删除
请手动删除.

・PPD,PPDeditor,BMSTOPPD,DirectShowLib
　这几个组件绿色安装，只需要删除文件夹及文件就完成了。

・IPAFont
　请在控制面板中的字体删除“IPAゴシック(IPAGothic)”。
　
・SlimDX,ffdshow
　请在控制面板的添加删除（程序和功能）中删除。
　
・MP4Splitter,FLVSplitter
　在运行（win+R可以启动）中执行以下几个命令：
　
　regsvr32 -u MP4Splitter.ax (when uninstalling MP4Splitter)
　regsvr32 -u FLVSplitter.ax (when uninstalling FLVSplitter)
　
　还有一点文件在你的系统盘下(此处默认为 C:\)
　
　C:\WINDOWS\system32\MP4Splitter.ax  (当删除MP4Splitter时删除此文件)
　C:\WINDOWS\system32\FLVSplitter.ax  (当删除FLVSplitter时删除此文件)