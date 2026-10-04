# UITests

アプリを実際に起動して、画面を操作する自動テストです（xUnit と FlaUI.UIA3）。

## 実行のしかた

デスクトップにログオンした状態で実行します（画面のないセッションでは動きません）。

```
dotnet test UITests\UITests.csproj -c Debug -p:Platform=x64
```

テストするアプリ（今は BMSTOPPD）は、テストの前にビルドされます。Visual Studio のテスト エクスプローラーからも実行できます。

## テストの書き方

- `AppSession.Start(プロジェクトのフォルダ, exe の名前)` でアプリを起動します。
  ビルドしたフォルダを一時フォルダにコピーしてから起動し、終わったらプロセスを終了してフォルダを消します。
  開発用のフォルダの設定ファイルは変わりません。PPD.ini、credentials.txt、Data.pak はコピーしません。
- アプリは普通には閉じません（閉じるときに設定を保存するアプリがあるため）。
- 画面の部品は `AutomationProperties.AutomationId` で探します（表示の文字は言語で変わるため）。
- メッセージボックスなどのモーダルのウィンドウを開くボタンは、`AppSession.InvokeAsync` で押します（閉じるまで戻らないため）。
- テストは並べて実行しません（`xunit.runner.json`）。
- インストール、アップロード、ネットワークにつなぐ操作はテストしません。
