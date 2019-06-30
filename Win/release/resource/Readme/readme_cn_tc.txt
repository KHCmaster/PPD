====================================================
PPDInstaller

2010/06/24
http://projectdxxx.blog9.fc2.com/
====================================================

○關於本軟件

  本軟件用於安裝與PPD相關的軟件。
  本軟件製作者沒有義務對使用本軟件出現的問題負責。

○如何使用
請運行PPDInstaller.exe,并按照指示操作。

○如何刪除
請手動刪除.

・PPD,PPDeditor,BMSTOPPD,DirectShowLib
　這幾個組件綠色安裝，只需要刪除文件夾及文件就完成了。

・IPAFont
　請在控制面板中的字體刪除“IPAゴシック(IPAGothic)”。
　
・SlimDX,ffdshow
　請在控制面板的添加刪除（程序和功能）中刪除。
　
・MP4Splitter,FLVSplitter
　在運行（win+R可以啟動）中執行以下幾個命令：
　
　regsvr32 -u MP4Splitter.ax (when uninstalling MP4Splitter)
　regsvr32 -u FLVSplitter.ax (when uninstalling FLVSplitter)
　
　還有一點文件在你的系統盤下(此處默認為 C:\)
　
　C:\WINDOWS\system32\MP4Splitter.ax  (當刪除MP4Splitter時刪除此文件)
　C:\WINDOWS\system32\FLVSplitter.ax  (當刪除FLVSplitter時刪除此文件)