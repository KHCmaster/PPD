====================================================
PPDInstaller

2010/06/24
http://projectdxxx.blog9.fc2.com/
====================================================

○About this software

  this software helps to install PPD's relational softwares.
  Also, software creator doesn't have responsibility for any problem with using this software.

○How to use
Please run PPDInstaller.exe, and follow instruction.

○How to uninstall
Please uninstall manually.

・PPD,PPDeditor,BMSTOPPD,DirectShowLib
　Please just delete directory(or file)

・IPAFont
　Please delete "IPAゴシック(IPAGothic)" in Font of the ControlPanel.
　
・SlimDX,ffdshow
　Please delete with Uninstall(delete) Programs in ControlPanel.
　
・MP4Splitter,FLVSplitter
　Please type following command to the dialog which opens with WindowsKey + R
　
　regsvr32 -u MP4Splitter.ax (when uninstalling MP4Splitter)
　regsvr32 -u FLVSplitter.ax (when uninstalling FLVSplitter)
　
　And files in the drive to which you installed Windows (Default is C:\)
　
　C:\WINDOWS\system32\MP4Splitter.ax  (when uninstalling MP4Splitter)
　C:\WINDOWS\system32\FLVSplitter.ax  (when uninstalling FLVSplitter)
　
　Please delete above files.