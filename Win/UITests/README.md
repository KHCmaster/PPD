# UITests

アプリを実際に起動して、画面を操作する自動テストです（xUnit と FlaUI.UIA3）。

## 実行のしかた

デスクトップにログオンした状態で実行します（画面のないセッションでは動きません）。

```
dotnet test UITests\UITests.csproj -c Debug -p:Platform=x64 --blame-hang --blame-hang-timeout 3min --blame-hang-dump-type none
```

`--blame-hang` は、テストが止まったときに 3 分で終わらせるための指定です。
テストで起動したアプリは Windows のジョブに入れてあるので、テストのプロセスが終わると（止まって強制終了されたときも）一緒に終わります。

テストするアプリ（BMSTOPPD、KeyConfiger、FlowScriptControlTest、BezierDrawControlTest、PPDUpdater、FolderDiffChecker、PPDInstaller、PPDConfig、PPDEditor、Effect2DEditor）は、テストの前にビルドされます。Visual Studio のテスト エクスプローラーからも実行できます。

## テストの書き方

- `AppSession.Start(プロジェクトのフォルダ, exe の名前)` でアプリを起動します。
  ビルドしたフォルダを一時フォルダにコピーしてから起動し、終わったらプロセスを終了してフォルダを消します。
  開発用のフォルダの設定ファイルは変わりません。PPD.ini、credentials.txt、Data.pak、鍵（*.ppk）、lastUser.txt はコピーしません。
- アプリは普通には閉じません（閉じるときに設定を保存するアプリがあるため）。
- テスト用のデータを置く、コピーしてはいけないものがないか確かめる、などは `AppStartOptions.Prepare` で、アプリを起動する前に行います
  （テストのクラスのコンストラクターで失敗すると、Dispose が呼ばれずアプリが残るため）。
- PPDInstaller はインストールしません。本物の Data.pak は使わず、テストで小さなものを作ります。
  「かんたんインストール」と、確認のページの「次へ」は押しません。
- PPDConfig は、テスト用の PPD.ini を作って使います。BlueSky のタブは開かず、OK（保存）は押しません。
- PPDEditor は、プロジェクト（Projects）、最近使ったプロジェクト、PPDFramework.setting.db をコピーしません。
  設定のダイアログでは OK（設定の保存）を押しません。
- 起動に時間がかかるアプリ（PPDEditor）は、`IClassFixture` で 1 回だけ起動して、テストのクラスの中で使い回します。
  テストの後には、開いたダイアログを閉じておきます。
- ウィンドウ（メインのウィンドウ、ダイアログ、メッセージボックス）は Win32 で探します。
  UI Automation はアプリの UI のスレッドに問い合わせるので、PPDEditor のようにゲームのループで UI のスレッドが忙しいアプリでは、
  1 回の問い合わせに 1 フレームほどかかります。PPDEditor では、子孫をすべて探す検索はなるべく避けてください。
- 画面の部品は `AutomationProperties.AutomationId` で探します（表示の文字は言語で変わるため）。
- メッセージボックスなどのモーダルのウィンドウを開くボタンは、`AppSession.InvokeAsync` で押します（閉じるまで戻らないため）。
- メッセージボックスは `MessageBoxes`、ファイルを開く・保存するダイアログは `FileDialogs` で操作します
  （Windows 11 ではボタンを UI Automation で押せないことがあるので、ウィンドウにコマンドを送る）。
- DirectX で描くところなど、UI Automation で中身を調べられないところは、`Screenshots` で画面を撮って比べます。
  マウスとキーボードを実際に動かすテストもあるので、テストの間はパソコンを操作しないでください。
- テストのプロセスはモニターごとの DPI に対応させています（UI Automation の座標と、画面を撮る・マウスの座標を合わせるため）。
- テストで使うファイルは、テストのフォルダの `TestData` に置きます（出力のフォルダにコピーされる）。
- テストは並べて実行しません（`xunit.runner.json`）。
- インストール、アップロード、ネットワークにつなぐ操作はテストしません。
  ネットワークにつなぐ設定のファイルは、`AppSession.Start` の最後の引数でコピーしないようにします（PPDUpdater の PPDUpdater.ini など）。

## カバレッジ

単体テストと UI テストを合わせたカバレッジは、ソリューションのフォルダの `coverage.ps1` で測ります。
UI テストで起動したアプリ（PPDEditor など）も、子のプロセスとして測ります。

```powershell
dotnet tool install --global dotnet-coverage   # 初めてのときだけ
powershell -ExecutionPolicy Bypass -File .\coverage.ps1          # 単体テストと UI テスト
powershell -ExecutionPolicy Bypass -File .\coverage.ps1 -SkipUI  # 単体テストだけ
```

先にソリューションを Debug|x64 でビルドしておきます。結果は `CoverageResults` に書き、プロジェクトごとの割合を表示します。
テストで一度も読み込まれないプロジェクト（PPDMovie など）は、結果に出ません。
