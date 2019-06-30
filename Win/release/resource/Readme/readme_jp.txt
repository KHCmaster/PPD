====================================================
PPDInstaller

2010/06/24
http://projectdxxx.blog9.fc2.com/
====================================================

○このソフトについて

  このソフトは、PPD関連のソフトウェアをインストールするものです。
  また、このソフトを使用して生じたいかなる損害も製作者は責任を負うことはありません

○使い方
PPDInstaller.exeを起動して指示に従ってください。

○アンインストール
アンインストールは手動で行ってください。

・PPD,PPDeditor,BMSTOPPD,DirectShowLib
　該当フォルダ(ファイル)を削除すればOKです。

・IPAフォント
　コントロールパネルのフォントから"IPAゴシック"を削除してください。
　
・SlimDX,ffdshow
　コントロールパネルのプログラムの削除(アンインストール)から削除してください。
　
・MP4Splitter,FLVSplitter
　Windowsキー+Rで出てきたダイアログに
　
　regsvr32 -u MP4Splitter.ax (MP4Splitterをアンインストールする場合)
　regsvr32 -u FLVSplitter.ax (FLVSplitterをアンインストールする場合)
　
　を入力し実行。さらにWindowsOSをインストールしたドライブ(デフォルトC:\なのでC:\で書きます)の
　
　C:\WINDOWS\system32\MP4Splitter.ax  (MP4Splitterをアンインストールする場合)
　C:\WINDOWS\system32\FLVSplitter.ax  (FLVSplitterをアンインストールする場合)
　
　上記のファイルを削除してください。