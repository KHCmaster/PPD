# .NET 10 / Vortice.Windows 移行計画

.NET Framework 4.8 + SharpDX 3.1.1 から、.NET 10 + Vortice.Windows へ移行するための計画です。
一度に切り替えず、**各フェーズの終わりで「ビルドが通り、PPD が遊べる」状態を保つ**ことを原則とします。

作成日: 2026-09-29

---

## 1. ゴールと範囲

### ゴール
- 全プロジェクトを `net10.0-windows` に移行する。
- SharpDX（2019 年に開発終了）への依存をなくし、Vortice.Windows に置き換える。
- 描画は DirectX 11 に一本化する。

### 範囲外（今回はやらない）
- Direct3D 12 への移行、描画方式の大幅な作り直し。
- WinForms / WPF から他の UI フレームワークへの移行。
- DirectShow による動画再生の置き換え（Media Foundation 化は別計画）。

---

## 2. 現状

| 項目 | 状況 |
|---|---|
| プロジェクト数 | 55（全て .NET Framework 4.8、packages.config 形式の旧 csproj） |
| コンパイル対象の .cs | 約 3,350 ファイル |
| SharpDX を使うプロジェクト | 22 |
| SharpDX に触れるファイル | 約 470（うち FlowScriptEngineSlimDX が 223） |
| 描画 API | 計画作成時は DX9（既定）と DX11（`[directx11]1` で有効化）の二本立て。**Phase 1 で DX11 のみになった** |
| UI | WinForms（365 ファイル）、WPF（43 ファイル） |
| 自動テスト | なし（PPDTest は目視確認用のシーン集） |

### SharpDX の使われ方

| 分類 | 主な場所 | 規模 | 移行先 |
|---|---|---|---|
| 数学型（Vector2/3/4、Matrix、Color4、Quaternion、Rectangle） | 全体。特に FlowScriptEngineSlimDX (223)、FlowScriptEnginePPD (51)、PPDFramework (42)、PPDSingle (31)、PPDMulti (27)、PPDCore (24) | 最大（約 470 ファイル） | System.Numerics + Vortice.Mathematics |
| Direct3D11 / DXGI | PPDFramework（DX11 実装層） | 中 | Vortice.Direct3D11 / Vortice.DXGI |
| Direct3D11.Effects（Effects11） | PPDFramework（Effect/DX11、全シェーダー） | 中〜大 | **Vortice に相当品なし**。自前のシェーダー管理に置き換え |
| Direct3D9 + D3DX9 | PPDFramework (19)、PPDMovie (2)、PPD (1) | 中 | **廃止**（DX11 に一本化） |
| Direct2D / DirectWrite / WIC | PPDFramework（文字画像生成 `Direct2DImageEncoder`） | 小 | Vortice.Direct2D1 系 |
| DirectInput | PPDInput (3)、PPD (4)、PPDEditor、KeyConfiger、PPDTest | 小 | Vortice.DirectInput |
| DirectSound | PPDSound (2)、PPD (1) | 小 | Vortice.DirectSound |
| SharpDX.Desktop（RenderForm / RenderLoop） | PPDFramework (3)、PPD (2)、PPDEditor (1) | 小 | 自前のメッセージループ |
| D3DCompiler | PPDFramework（ShaderBytecode 型のみ） | 小 | Vortice.D3DCompiler（またはビルド時コンパイルのみにして不要化） |

PPDFramework の公開メンバーのうち、SharpDX の数学型を引数・戻り値に使うものが約 130 箇所あります。
スキンや Mod から見える API が変わる点に注意が必要です（→ 決定事項 D2）。

### Vortice.Windows 側の状況（2026-09 時点で確認）
- 最新は 3.8.3（2026-03 公開）。Direct3D11、DXGI、Direct2D1、DirectInput、DirectSound、XAudio2、D3DCompiler、Direct3D9 などのパッケージがある。
- 対象は .NET 8 / 9 / 10。**.NET Framework では使えない**ため、.NET 10 への移行が先に必要。
- **Effects11（D3DX11 Effects）のラッパーはない。** D3DX9 相当もない。
- 数学型は System.Numerics を使い、補助型（Color4 など）は Vortice.Mathematics（2.1.1）が提供する。
- WIC / DirectWrite のパッケージ構成（Direct2D1 に含まれるか別パッケージか）は、着手時に確認する。

### 依存パッケージの状況

| パッケージ | 使っている場所 | .NET 10 での扱い |
|---|---|---|
| EntityFramework 6.1.3 / System.Data.SQLite.EF6 | **コード上の使用なし** | 削除 |
| System.Data.SQLite 1.0.100 | PPDFramework (7)、PPDExpansion | 新しい版（.NET 対応版）に更新するか、Microsoft.Data.Sqlite に置き換え。既存の `PPD.db` がそのまま読めることを確認する |
| MvvmLight 4.x | PPDEditorCommon (12)、PPDExpansion (6)、FlowScriptDrawControl (4)、PPDModSettingUI | 開発終了。CommunityToolkit.Mvvm に置き換え |
| WPFToolkit 3.5 / DataVisualization Toolkit | PPDExpansion（XAML 2 ファイル） | .NET Framework 専用。グラフ部分を代替ライブラリか自前描画に置き換え |
| MessagePack 1.9 | PPDMultiCommon (36)、PPDMulti、PPDMultiServer | 2.x 以降は API が変わる。**通信フォーマットの互換性**（旧版クライアントやサーバーとの接続）を確認して更新 |
| YamlDotNet 5.1.0 | FlowScriptEngineData | 最新版に更新（API 差分は小さい見込み）。現在ローカルでパッケージが復元できない問題あり |
| Newtonsoft.Json 13 | 各所 | そのまま |
| ICSharpCode.SharpZipLib（NuGet 管理外の DLL） | PPDInstaller、PPDUpdater、PPDSingle ほか | NuGet の最新版に切り替え |
| CoreTweet | PPDTwitter | **PPDTwitter はどこからも参照されていない**。削除候補 |
| WinSCP 5.7 | FolderDiffChecker | .NET 対応版へ更新 |
| WacomMTDN（NuGet 管理外） | PPD | .NET 10 から読み込めるか要確認 |
| System.Web（HttpUtility） | PPDFramework、PPDMultiCommon の WebManager | .NET でも `System.Web.HttpUtility` として使える |

---

## 3. 先に決めておくこと

2026-09-29 に以下で決定しました（D4〜D6 は推奨案のまま進め、問題があれば見直す）。

| # | 決定事項 | 決定 | 理由・補足 |
|---|---|---|---|
| D1 | DX9 と「シェーダー無効化」を廃止するか | **廃止する（決定）** | Vortice に D3DX9 がなく、DX9 経路は D3DX9 の Effect / テクスチャ読み込みに強く依存している。DX11 実装は PPDTest の全シーンで DX9 と同等の表示を確認済み |
| D2 | スキン・Mod との互換性 | **スキンの互換は不要（決定）。Mod の互換は必須（決定）** | スキンは実質 PPDSingle / PPDMulti のみで、同時に作り直せる。Mod は FlowScript で書かれているので、**FlowScript の互換を保てば Mod の互換も保てる**（下記「Mod 互換の条件」） |
| D3 | 配布形態 | **現状のまま（決定）** | インストーラー（PPDInstaller）・更新（PPDUpdater）・リリース作成（PPDReleaser）の仕組みを維持する。.NET 10 ではフレームワーク依存となるため、インストーラーで .NET 10 Desktop Runtime の有無を確認・案内する |
| D4 | 数学型の移行先 | System.Numerics + Vortice.Mathematics | Vortice の API がこれらを受け取るため、変換コードが不要になる |
| D5 | 音声 | まず Vortice.DirectSound で置き換え、XAudio2 化は別途 | 置き換え量を最小にする |
| D6 | PPDUpdater の扱い | 現状の配布形態に合わせる（D3） | 旧版（.NET Framework）から新版への更新経路は Phase 7 で設計する |

### Mod 互換の条件

Mod（`mods\*.mod`）は PPDPack 形式で、中に FlowScript（`Scripts\*.fsml`）と、作成時の FlowScript 各 DLL のバージョンを持っている。
`ModInfo.CanApply()` は、**インストール済みの DLL のバージョンが Mod に記録されたバージョン以上であること**を確認する（`PPDFramework/Mod/ModManager.cs`、`ModInfo.cs`）。
したがって移行中は、以下を全て守る。

- DLL のファイル名と配置を変えない: `FlowScriptEngine.dll`、`dlls\FlowScriptEngineBasic.dll`、`dlls\FlowScriptEnginePPD.dll`、`dlls\FlowScriptEngineSlimDX.dll`、`dlls\FlowScriptEngineBasicExtension.dll`、`dlls\FlowScriptEngineData.dll`
- これらの**アセンブリ（ファイル）バージョンを下げない**（移行時は上げる）。
- ノードのクラス名（名前空間を含む完全名）を変えない。`.fsml` はこれで保存されている。
- ノードの入出力ポート名（プロパティ名）を変えない。
- `.fsml` に書かれた値の文字列（`<Value Name="..." Value="..."/>`）を、従来どおり解釈できるようにする（Vector2 などの書式を含む）。
- ノードの動作（計算結果、呼び出し順序）を変えない。特に数学型の置き換え（Phase 5）で、ベクトル・行列・クォータニオン演算の結果が変わらないようにする。
- 検証用に、実際の Mod（`mods\` 以下）を複数用意し、各フェーズで「読み込める」「適用できる」「同じ動作をする」ことを確認する。

---

## 4. フェーズ計画

依存の少ない順に進めます。フェーズ 1〜3 は **SharpDX と .NET Framework 4.8 のまま**実施でき、それぞれ単独でリリースしても構いません。

```
Phase 0  準備（検証の仕組み、不要物の整理）
Phase 1  DX11 一本化（DX9 削除）                 ← .NET Framework / SharpDX のまま
Phase 2  Effects11 からの脱却                     ← .NET Framework / SharpDX のまま
Phase 3  SDK 形式 csproj 化・依存パッケージ整理    ← まだ net48
Phase 4  .NET 10 へ移行                          ← ここで net10.0-windows
Phase 5  数学型の移行
Phase 6  SharpDX → Vortice 差し替え
Phase 7  配布・更新の仕組みの対応、リリース
```

### Phase 0: 準備

**目的**: 以降の各フェーズで「壊れていないこと」を素早く確認できるようにする。

- [x] **回帰テストツールをリポジトリに追加する。** `Tools/RenderCheck/` に置いた（使い方は `RenderCheck.ps1` 冒頭のコメント）。
  - `SceneCapture`: PPDTest を起動して全シーンを自動で送り、描画が落ち着いてからスクリーンショットを撮る
  - `ImageCompare`: 2 枚の画像を画素単位で比較し、差分画像を出す
  - `ExceptionWatch`: PPD をデバッグ実行して例外（初回例外を含む）を記録し、ゲーム内の終了確認を K キーで確定して終了させる。アクセス違反があれば失敗
  - `ModCheck`: `mods\` の全 Mod（と譜面フォルダの .fsml）を PPD と同じ手順で読み込み、各ノードの既定値の変換結果を書き出す（基準との差分で FlowScript の互換を確認）
  - `RenderCheck.ps1`: 上記をまとめて実行する。`-UpdateBaseline` で基準を作り直す
- [x] 基準（`Tools/RenderCheck/baseline/`、git 管理外）を保存した。シーン画像 8 枚（DX11）と Mod の結果（Mod 28 個・譜面スクリプト 17 個、既定値 2,488 件）。
  - 注意: 最初に撮った基準は、DX11 の初期化が失敗して DX9 に切り替わった状態で撮られていた（原因は不明、その後は再現しない）。Phase 1 後に DX11 のみの状態で撮り直した。
- [ ] PPDTest に不足しているシーンを追加する。例: Glass ブレンド（背景に模様を敷く）、ボーダー、モザイク。→ **Phase 2 の着手前に行う**（Effects11 置き換えの検証に必要）。動画シーンは曲を置けば動作する（確認済み）。
- [ ] 実プレイの確認手順書（チェックリスト）を作る。確認対象は次のとおり。
  - 起動、選曲、プレイ、リザルト
  - 動画付き譜面
  - エディター（PPDEditor）での編集と再生
  - マルチプレイ
  - キー設定（KeyConfiger）
  - 終了時にアクセス違反が出ないこと
- [x] 不要物を整理する。→ Phase 3 で行った。
  - どこからも参照されていない PPDTwitter
  - 使われていない EntityFramework / System.Data.SQLite.EF6 / Linq パッケージ
  - コンパイル対象外の古い SlimDX 時代の .cs ファイル（PPD\PPD 配下など）
- [x] YamlDotNet 5.1.0 の復元エラーを解消し、ソリューション全体がビルドできる状態にする（`msbuild /t:restore /p:RestorePackagesConfig=true`）。
  - 復元時に MessagePack 1.9.11 の既知の脆弱性の警告（NU1902）が出る。Phase 3 の MessagePack 更新で対応する。
**完了条件**: ソリューション全体がビルドでき、回帰テストツールで基準画像との比較ができる。

### Phase 1: DX11 一本化（DX9 の削除）

**目的**: D3DX9 への依存をなくし、後のフェーズで扱う描画コードを半分にする。

- [x] PPDFramework から削除した
  - `DX9/`、`Effect/DX9/`、`Texture/DX9/`、`Vertex/DX9/`、`Resource/DX9/`、`Shaders/DX9/`（リソースの `d3d9_*` も）
  - `AlphaBlendLegacy`（シェーダー無効時の固定機能パイプライン描画）と、それだけが使っていた `DrawUserPrimitives`
  - `PPDDevice.Initialize` の `isDX11` 引数と DX9 へのフォールバック
  - `ScreenTexelOffset`、`ResetDevice`、`Workspace` / `WorkspaceTexture` の `OnLostDevice` / `OnResetDevice`（DX9 のデバイスロスト用）
  - `PPDSetting` の `MultiSample`（D3D9 の型）、`MoviePlayType`（と `MoviePlayType` 列挙型）、`ShaderDisabled`、`DirectX11Enabled`
  - `GameCoreBase` の `DeviceLost` 判定。Present の失敗は、従来と同じくそのフレームを捨てて次で再試行する
- [x] `CharCacheInfo` / `SizeTexture` の DX9 分岐を削除した。
- [x] 動画: `VMR9Movie` / `Allocator9` を削除し、常に SampleGrabber で再生するようにした。
- [x] 設定から削除した
  - `PPD.ini` の `shaderdisabled` / `directx11` / `movieplaytype` / `multisample` は読まなくなった（書かれていても無視される）
  - PPDConfig の「シェーダーを無効化」「DirectX11で描画する」、PPDEditor の「シェーダーを無効化」メニューと `-disableshader` の受け渡し
  - 起動引数 `-disableshader`（`PPDSetting.Initialize` の `disableShader` 引数ごと削除）
  - PPDConfig の「アンチエイリアス」は DX9 時代から実際には効いていなかった（TODO のまま）。画面配置の都合で残しているが、Phase 3 以降で削除を検討する
- [x] `PPD\Update\Checker.cs` と `PPDReleaser` の DLL 一覧から `SharpDX.Direct3D9.dll` を外した。
- [x] `compile_shader.bat` を DX11 のみに変更した。
- [x] DX11 の初期化に失敗したとき、原因（例外のメッセージ）を「DirectXのデバイスを初期化できませんでした」の後に表示するようにした（`GameCoreBase.InitializeDirectXException`）。DX9 への逃げ道がなくなったため。
- [ ] 見た目の変化を周知する。DX9 の半ピクセルずれがなくなるので、文字や画像の輪郭がわずかにシャープになる。→ リリース時（Phase 7）のお知らせに含める。

**結果（2026-09-29）**: `SharpDX.Direct3D9` への参照はゼロ。ソリューション全体（Debug / Release）のビルド、シーン 0〜6 の画素比較、PPD 終了時のアクセス違反チェック、Mod 28 個・譜面スクリプト 17 個の読み込みと既定値の比較が全て通った。Phase 1 前の DX11 の画像とも一致した。
**完了条件**: `SharpDX.Direct3D9` への参照がゼロ。回帰テストの結果が Phase 0 の基準（DX11 モード）と一致する。

**規模の目安**: 小〜中。削除が中心。

### Phase 2: Effects11 からの脱却

**目的**: Vortice に相当品がない Effects11 を、自前のシェーダー管理に置き換える。Vortice 移行で最も大きい技術的課題を、SharpDX のうちに片付ける。

設計方針:
- `EffectBase` / `EffectHandleBase` の公開インターフェース（`Technique`、`SetValue`、`SetTexture`、`Begin` / `BeginPass`）はなるべく保ち、呼び出し側（AlphaBlend、SpriteBatch、各フィルタ）の変更を最小にする。
- テクニックは「頂点シェーダー + ピクセルシェーダー + ブレンドステート」の組として扱う。`.fxt` テンプレートの展開結果から、テクニック名ごとに次を生成する。
  - `vs_4_0` / `ps_4_0` のバイトコード（`.cso`）
  - ブレンドステートの種類（NoBlend / AlphaBlending / PremultipliedAlphaBlending）
- 定数（`Projection`、`DrawInfo`、`FilterInfo`、`Weights[32]` など）は定数バッファにまとめる。変数名からオフセットへの対応表は、コンパイル時にシェーダーリフレクションで生成して一緒に埋め込む。実行時にリフレクションを使わないためと、D3DCompiler を実行時に不要にするため。
- `float Weights[32]` のように、配列要素ごとに 16 バイトへ詰め直す必要がある変数に注意する。
- テクスチャとサンプラーのスロットも、コンパイル時の情報から決める。

作業:
- [x] 準備として、Effects11 のままシェーダーを書き換えた（画素単位で変化なしを確認済み）。
  - 定数を `cbuffer Parameters : register(b0)` に明示的にまとめた
  - テクスチャ（`register(tN)`）とサンプラー（`register(s0)`）のスロットを明示した
- [x] ShaderCompiler を拡張した（`ShaderCompiler/EffectCompiler.cs`）。`ShaderCompiler.exe X.fxt d3d11_X.bin` で次の処理をする。
  1. `.fxt` を展開する
  2. `technique11` / `pass` の記述を読み取って取り除く
  3. パスごとの VS/PS を D3DCompiler でコンパイルする。同じ関数・引数の組は共有する
  4. 次を 1 つのバイナリ（`PPDFX` 形式）にまとめる
     - バイトコード
     - 定数バッファのレイアウト（リフレクションで取得）
     - テクスチャのスロット
     - テクニック / パス → VS・PS の番号とブレンドステート名
  - 注意: SharpDX の `ShaderBytecode.Compile(string)` は、ソースに日本語があると途中で切れる（文字数をバイト数として渡している）。UTF-8 のバイト列で渡している。
- [x] ビルド時に自動でシェーダーを生成するようにした。PPDFramework.csproj の `CompileShaders` ターゲットで行い、`.fxt` か共通の `.fx`（Common / Blend / Filter）が `.bin` より新しいときだけ作り直す。ShaderCompiler はプロジェクト参照（`ReferenceOutputAssembly=false`）で先にビルドされる。`compile_shader.bat` も新しい形式に合わせた。
- [x] `Effect/DX11/Effect.cs` を書き直した。
  - 定数は CPU 側の配列に書き、変更があったときだけ `UpdateSubresource` でまとめて送る
  - ブレンドステートとサンプラーは `DX11.PPDDevice` で共有する（Common.fx の定義と同じ設定）
  - シェーダーオブジェクトは初回使用時に作る（遅延生成）
- [x] `sharpdx_direct3d11_1_effects_x64.dll` と `SharpDX.Direct3D11.Effects` の参照を削除した。PPDFramework は実行時に D3DCompiler も使わなくなったので、`SharpDX.D3DCompiler` の参照も削除した（ShaderCompiler だけが使う）。`Checker.cs` / `PPDReleaser` の DLL 一覧からも外した。
- [x] AlphaBlend（420 テクニック、PS 420 個、約 1 MB）は、シェーダーオブジェクトを遅延生成にしたので、起動時に作るのは読み込みと解析だけになった。

**結果（2026-09-29）**: 次の確認が全て通った。
- `SharpDX.Direct3D11.Effects` への参照はゼロ。ソリューション全体（Debug / Release）がビルドできる。
- シーン 0〜7（EffectScene を含む）が基準と一致した。差は各 62 画素だけで、Windows 11 のウィンドウの角丸に写り込む背景の違い。
- PPD 終了時のアクセス違反はゼロ。
- Mod 28 個・譜面スクリプト 17 個の既定値が基準と一致した。

気になる点（→ Phase 4 で原因が分かった）: 最初の 1 回だけ、終了処理の途中で `0xC0000008`（無効なハンドル、初回例外）が出たあとプロセスが終了しなかった。原因は PPD ではなく、この PC に常駐している CLaunch のフック DLL（`ClHook.dll`）がプロセス終了時に無効なハンドルを閉じていたこと。この例外はデバッガー接続時にだけ発生し、ExceptionWatch が「未処理」として返していたために止まっていた。ExceptionWatch は、この例外を処理済みとして続けるように直した。
**完了条件**: Effects11 への参照がゼロ。回帰テストが基準と一致する。終了時のアクセス違反がない（Effects11 のファイナライザー問題も同時に解消する）。

**規模の目安**: 中〜大。本計画で最も設計判断が多いフェーズ。

### Phase 3: SDK 形式の csproj 化と依存パッケージの整理（net48 のまま）

**目的**: .NET 10 への切り替えを「TargetFramework を変えるだけ」に近づける。

- [x] ソリューションの 54 プロジェクトを SDK 形式の csproj に変換した（`TargetFramework` は `net48`、`PackageReference`）。
  - 変換は一括で行い、ビルド出力を変換前と比べて確認した（`Tools/ProjectCheck/AssemblyDump.ps1`）。比べたのは次の項目で、Debug / Release とも同じだった
    - アセンブリ名・バージョン・ファイルバージョン
    - 参照アセンブリ
    - 埋め込みリソースの名前と内容
    - 型の一覧
  - 違いは 2 つだけだった。
    - コンパイラーが生成する型の番号（`<>c__DisplayClass4_0` → `7_0` など）。ソースファイルのコンパイル順が変わったため
    - WPF の BAML（`*.g.resources`）のハッシュ。含まれるエントリーは同じ
  - 共通設定は `Directory.Build.props` に置いた。従来の出力を変えないためのもの。
    - 出力先に `net48\` を付けない
    - `AssemblyInfo.cs` を使い続ける
    - 埋め込みリソースの名前を従来の規則で付ける
  - 構成ごとの `OutputPath`（`release\x64\PPD\`、`skins\` など）や `DefineConstants` は、元の csproj からそのまま移した。
  - SDK 形式では .cs / .resx / .xaml がフォルダーから自動で取り込まれる。そのため、どのプロジェクトからもコンパイルされていなかった古いファイル 86 個を削除した（PPD\PPD、PPDEditor の SlimDX 時代のフォーム、PPDFramework・PPDSingle・FlowScriptControl・WinFormsUI の未使用ファイル）。
  - どこからも参照されていない PPDTwitter を削除した。
- [x] `packages\` フォルダへの HintPath 参照をなくし、`packages.config` を削除した。SQLite の `.targets` の手動インポートも不要になった。
- [x] NuGet 管理外の DLL を整理した。
  - SharpZipLib は NuGet 版にした（下記）
  - WacomMTDN はどのプロジェクトからも参照されていなかった（古いビルドの出力に残っているだけ）ので、`Checker.cs` / `PPDReleaser` の一覧から外した
- [x] ビルド後のコピー処理を MSBuild のターゲットに置き換えた（`Directory.Build.targets` の `CopyFlowScriptPlugin`）。
  - FlowScriptEngine* の 7 プロジェクトは、コピー先を `FlowScriptPluginFolder` で指定する
  - 従来は Release ビルドでも Debug の DLL をコピーしていたので、Debug ビルドのときだけコピーするようにした
- [ ] リソース（.resx）の対応。byte[] や画像を `ResXFileRef` で持つ .resx が 142 個ある。net48 を Visual Studio の MSBuild でビルドする限りは今のままで動く。.NET では文字列以外のリソースに `System.Resources.Extensions` が必要になるので、**Phase 4 で追加する**。
- [x] 使われていない EntityFramework / System.Data.SQLite.EF6 / System.Data.SQLite.Linq（と、これらをまとめて参照する System.Data.SQLite パッケージ）を削除した。
  - コードが使っているのは `System.Data.SQLite`（ADO.NET）だけなので、System.Data.SQLite.Core だけを残した
  - 5 つの app.config から、EF の設定セクションと EF6 のプロバイダー登録を削除した
  - `Checker.cs` / `PPDReleaser` の DLL 一覧からも外した
- [ ] 依存パッケージを整理する（2 章の表）。**.NET Framework と .NET 10 の両方で動く版を選び**、このフェーズで先に更新しておく。
  - [x] MvvmLight → **CommunityToolkit.Mvvm 8.4.2** と Microsoft.Xaml.Behaviors.Wpf に置き換えた。
    - 途中で自前の小さなライブラリ（PPDMvvm）にしたが、CommunityToolkit.Mvvm を使う方針にして置き換えた
    - 置き換えの対応
      - `ViewModelBase` → `ObservableObject`
      - `RaisePropertyChanged` → `OnPropertyChanged`
      - `Messenger.Default` → `WeakReferenceMessenger.Default`
      - `MessageBase` は Toolkit にないので、PPDEditorCommon と PPDExpansion にそれぞれ小さな基底クラス（`Sender` を持つだけ）を置いた
      - `canExecute` 付きの `RelayCommand` は使っていないので、`CommandManager` による再評価がなくなっても影響はない
    - XAML の `EventToCommand`（System.Windows.Interactivity）は、Microsoft.Xaml.Behaviors の `InvokeCommandAction` に置き換えた。`PassEventArgsToCommand` はそのまま使える
    - net48 では CommunityToolkit.Mvvm の依存 DLL（System.Memory、Unsafe 6.x、Bcl.AsyncInterfaces など）が増える。そのため次の 2 つを行った
      - **Central Package Management（`Directory.Packages.props`）を導入した。** 間接的な依存の版も固定する（`CentralPackageTransitivePinningEnabled`）。`release\x64\PPD` には複数の exe が出力されるため、プロジェクトごとに版が違うと、古い版で上書きされる
      - skins から読み込まれる PPDModSettingUI のために、**PPD.exe でも CommunityToolkit.Mvvm を参照する**。新しい版の依存 DLL が PPD.exe のフォルダーに置かれ、バインディングリダイレクトも自動生成される
    - バインディングの確認用に `Tools/ProjectCheck/BindingCheck.ps1` を作った。exe（とプラグインのフォルダー）から参照をたどり、置かれている版と違う版の参照が、リダイレクトで解決されるかを調べる。Debug / Release の全 exe で問題がないことを確認した
    - その他の確認
      - WPF のコントロールとウィンドウ（FlowScriptDrawControl、PPDEditorCommon、PPDModSettingUI）を全て生成して、XAML が読み込めることを確認した
      - PPDExpansion の画面が変更前と画素単位で一致することを確認した
      - PPDExpansion の「ファイル → 終了」でウィンドウが閉じることを確認した（RelayCommand → Messenger の流れ）
    - 見つけて直した既存の不具合: PPDExpansion で「Fatal Error Occurred」が出て、閉じてもプロセスが終了しなかった
      - 原因: `WeakEventListener` は、渡されたメソッドが static であることを確認している。しかし、C# 6（Roslyn）以降は、何も捕捉しないラムダ式もインスタンスメソッドにコンパイルされるため、この確認で例外になっていた
      - `CollectionChangedWeakEventSource` で、ラムダ式の代わりに static メソッドを渡すようにした
  - [x] SharpZipLib: プロジェクトフォルダーにあった 0.86 を NuGet の **1.3.3** にした。
    - 0.86 には zip のパストラバーサル（Zip Slip）の脆弱性がある。PPDSingle は Web からダウンロードした zip を展開するので対応が必要
    - 1.4 系は netstandard2.0 のみで、net48 では依存 DLL が増えるため 1.3.3（net45 版があり依存なし）にした。.NET 10 では netstandard2.1 版が使われる
    - 1.3.3 は、UTF-8 フラグのないファイル名を既定で UTF-8 として読む。そのため、`ZipStrings.CodePage` に OEM コードページを指定して 0.86 と同じ動作にした
    - 0.86 と 1.3.3 の間で作成・展開を相互に行い、日本語のファイル名・フォルダー・空フォルダーが一致することを確認した
  - [x] YamlDotNet は **5.1.0 のままにする**。既知の脆弱性はなく、netstandard1.3 版で .NET 10 でも動く。新しい版は `Yaml.Dump` ノードの出力書式が変わる可能性があり、Mod の互換を優先した
  - [x] morelinq は `MinBy` / `MaxBy` の 2 か所だけだったので、`OrderBy(...).First()`（安定ソートなので同じ要素を返す）に置き換えて削除した。新しい morelinq は戻り値の型が変わっているうえ、.NET 10 の `Enumerable.MinBy` と名前がぶつかるため
  - [ ] MessagePack（1.9.11）→ **Phase 4 で行う**。
    - 2.x / 3.x は net48 では依存 DLL が多い（System.Memory、Unsafe 6.x、Collections.Immutable など）。PPDMulti は skins から読み込まれるため、PPD.exe でバインディングリダイレクトの問題が起きやすい。.NET 10 ではこれらが標準に含まれるので、移行後に行う
    - 通信互換の確認用に `Tools/ProjectCheck/MessagePackDump.ps1` を作った（Phase 4 で .NET 10 用の `Tools/ProjectCheck/MessagePackDump` に置き換えた）
      - 34 種類の通信データ型に決まった値（日本語の文字列、入れ子の配列、Tuple を含む）を入れてシリアライズし、バイト列を書き出す
      - 1.9.11 の結果は `Tools/ProjectCheck/messagepack_v1.txt`
      - 新しい版では次の 2 点を確認する
        - 同じバイト列を出力すること
        - 古いバイト列を読み込んで書き直したときに、同じバイト列になること
      - 後者が必要なのは、`Parser` が受信データの長さを再シリアライズで求めているため
  - [x] System.Data.SQLite.Core を 1.0.100 から 1.0.119 に上げた（SQLite 3 のファイル形式は互換）。skins フォルダーにはネイティブ DLL をコピーしないようにした（`CopySQLiteInteropFiles=false`）
- [x] WPFToolkit / DataVisualization（PPDExpansion のグラフ）を、同じコードを引き継いでいるフォーク **DotNetProjects.WpfToolkit.DataVisualization 6.1.94** に置き換えた。
  - 対象は net40 と net6.0-windows で、依存なし、MS-PL
  - 名前空間と型が同じなので、XAML の assembly 名を変えるだけで済んだ
  - 変更前後の PPDExpansion のウィンドウを撮影して比べ、画素単位で一致することを確認した
  - WPFToolkit（3.5）は DataVisualization の依存としてだけ入っていたので削除した

**結果（2026-09-29）**: 次の確認が通った。
- 全プロジェクト（54 個）が、SDK 形式・PackageReference で Debug / Release ともビルドできる。
- シーン 0〜7 の画素比較、PPD 終了時のアクセス違反チェック、Mod 28 個・譜面スクリプト 17 個の既定値の比較。
- `PPDReleaser` の DLL 一覧にあるファイルが、全て Release の出力にある。

残りは次の 2 つで、どちらも Phase 4 に回した。
- .resx の `System.Resources.Extensions`（.NET でだけ必要）
- MessagePack の更新

実プレイの確認（エディター、マルチプレイ、PPDExpansion など）は未実施。
**完了条件**: 全プロジェクトが SDK 形式・PackageReference でビルドでき、回帰テストと実プレイ確認が通る。

**規模の目安**: 中。機械的な作業が多いが、プロジェクト数が多い。

### Phase 4: .NET 10 への移行

**目的**: 実行環境を .NET 10 に切り替える。

- [x] 全プロジェクトの `TargetFramework` を `net10.0-windows` にした（2026-09-29）。
  - フレームワークの `<Reference>` を削除した。.NET では共有フレームワークに含まれる
  - 追加したパッケージ
    - `System.ServiceProcess.ServiceController`: PPDMultiServerService
    - `WinForms.DataVisualization`: PPDEditor の統計ウィンドウ
  - `System.Configuration.ConfigurationManager`（設定）と `System.Resources.Extensions`（.resx の byte[] や画像）は、.NET 10 の Microsoft.WindowsDesktop.App に含まれるのでパッケージは不要だった
  - net48 のために入れていたもの（間接的な依存の版の固定、PPD での CommunityToolkit.Mvvm の参照）は、.NET 10 では不要なので削除した
  - `WFO1000`（WinForms デザイナー向けの分析）は、実行時の動作に関係しないので抑止した
  - BlueSky の `LangVersion 7.3` の指定を削除した。他のプロジェクトは .NET 10 の既定（C# 14）になった。C# 14 で `field` がキーワードになる影響（CS9258）はない
- [x] .NET Framework と動作が変わる箇所を、先に net48 のまま直した（コミット `151377e`）。
  - **`Thread.Abort`**（19 か所、.NET では例外になる）を次のように置き換えた
    - ThreadManager のスレッドはバックグラウンドスレッドにした
    - 通信のスレッドは、接続を閉じることで止める
    - Web の更新・ログイン・難易度計算は、中止の要求（または結果を反映しない）で止める
  - FlowScript の実行の中止（PPDEditor、FlowScriptControlTest）は、`DebugController.Abort` で次のノードの評価で止まるようにした（`FlowAbortedException`）。無限ループのスクリプト（`Repeat.While` の Condition が True）を実行し、`Abort` で 1 ms 以内に止まり、エラーが報告されないことを確認した。あわせて、ブレークポイントで止まっていないときに `DebugController.Abort` が NullReferenceException になる既存の不具合を直した
  - **`Encoding.Default`**（.NET では UTF-8 になる）は、システムの ANSI コードページ（`GetACP`）に置き換えた
  - URL と PPDUpdater の起動は、`UseShellExecute = true` にした
- [x] **SharpDX を 4.2.0 に上げた**（net48 のまま先に行った）。3.1.1 は net45 と古い portable 版しかなく、.NET 10 から使えないため。
  - このプロジェクトが使っている範囲では API の違いがなく、コードの変更なしでビルドできた
  - シーンの画素比較、PPD 終了時のアクセス違反チェック、Mod と譜面スクリプトの既定値の比較が通った
  - SharpDX.Desktop（`RenderLoop` / `RenderForm`）は 4.2.0 でも .NET Framework 版しかない。そこで PPDFramework に同じ動作の `RenderLoop` を作り、SharpDX.Desktop を削除した（Phase 6 の予定を前倒し）
  - SharpDX 4.2.0 は、.NET 10 からは netstandard1.1 / 1.3 の版が使われる
- [x] .NET 10 で動作が変わる箇所に対応した。
  - **起動時の共通設定**（`Shared/ProcessInitializer.cs`）。`Directory.Build.targets` で全ての exe に組み込み、Main より前に実行する
    - `CodePagesEncodingProvider` を登録する（Shift_JIS などのため）
    - WinForms の既定フォントを `SystemFonts.DefaultFont` にする（.NET Framework と同じ。日本語の Windows では MS UI Gothic 9pt）。PPDConfig の画面が .NET Framework 版と同じになることを確認した
  - **カルチャーのデータを Windows の NLS にした**（`System.Globalization.UseNls`）。.NET の既定の ICU では、日付や数値の書式・文字列の比較が変わるため
  - **FlowScript の数値の文字列化を .NET Framework と同じにした**（`FlowScriptEngine.LegacyNumberFormat`）。.NET では書式を指定しない `ToString` の結果が変わるため（例: `1f/3` が "0.33333334"、`0.1+0.2` が "0.30000000000000004"、-0 が "-0"）
    - 使っている場所: float / double / object から string への変換、Vector2/3/4 から string への変換、`String.Format` ノード（書式指定のない数値だけ）、デバッガーの表示
    - float は有効数字 7 桁、double は 15 桁にし、-0 は "0" と表示する。Windows PowerShell（.NET Framework）の出力と 12 通りの値で一致することを確認した
    - `.fsml` の既定値の解析（float / double / Vector）では、`"-0"` を 0 として読む（.NET Framework と同じ。.NET は -0 を返すため、`Atan2` などの結果が変わる）
  - **`<system.data>` を app.config から削除した**。.NET の ConfigurationManager が「Unrecognized configuration section」で失敗し、PPD が起動時にエラーダイアログを出していた。DbProviderFactories はコードで使っていない
  - **リプレイの読み込み**（`PPDPlayDecorder`）: .NET では `GZipStream.Read` が要求より少ないバイト数を返すことがあるため、要求したバイト数まで読むようにした（しないとリプレイのデータが壊れる）。その他の `Stream.Read`（CA2022）は、FileStream / MemoryStream などで読み方が変わらないので、そのままにした
  - 回帰テスト: RenderCheck の PPD の確認で、error.log に書き込まれた場合も失敗にした。ModCheck は .NET 10 のプロジェクトとしてビルドするようにした
  - 確認した結果（.NET 10）
    - シーン 0〜7 の画素比較が通った
    - PPD は起動・終了でき、アクセス違反も error.log への書き込みもない
    - Mod 28 個・譜面スクリプト 17 個の既定値が、.NET Framework で作った基準と完全に一致した
    - PPDEditor、PPDConfig、KeyConfiger、Effect2DEditor、BMSTOPPD、FlowScriptControlTest、BezierDrawControlTest、PPDExpansion が起動する
    - Debug / Release ともビルドできる
  - 未確認: 高 DPI（PPDConfig は同じ見た目だった）、プラグインの読み込みの細かい動作（skins / dlls は動作している）、DirectShow の動画（PPDTest の動画シーンは表示される）- [x] PPDEditor の統計ウィンドウ（`DockForm/StatsManager`）は WinForms の `System.Windows.Forms.DataVisualization`（Chart）を使っている。これは .NET 10 に含まれないので、.NET 向けの移植版（NuGet の `WinForms.DataVisualization` など）に置き換える。（Phase 3 で見つけた）
- [x] MessagePack を 1.9.11 から **3.1.10** に上げた（Phase 3 から移した）。MessagePackAnalyzer（1.x 用）は削除した。
  - コードの変更は不要だった
  - 通信互換の確認ツールを .NET 10 用に作り直した（`Tools/ProjectCheck/MessagePackDump`）。1.9.11 で作った基準（`messagepack_v1.txt`）と比べ、34 種類の通信データ全てで次のことを確認した
    - 同じバイト列を出力する
    - 古いバイト列を読み込んで書き直すと、同じバイト列になる
    - 後ろに別のメッセージが続いていても、先頭の 1 つを読める（`Parser` が前提にしている動作）
  - ツールが基準を作った .ps1 と同じ値を作ることは、1.9.11 の .NET 10 ビルドで基準と完全に一致することで確認した
  - 使い方: `MessagePackDump.exe <PPDMultiCommon.dll と MessagePack.dll のあるフォルダー> --compare Tools\ProjectCheck\messagepack_v1.txt`
- [ ] ShaderCompiler、PPDReleaser、FolderDiffChecker などのツール類も移行する。

**完了条件**: .NET 10 で全体がビルドでき、回帰テストと実プレイ確認が通る。

**規模の目安**: 中。SharpDX 4.2.0 の検証結果次第で変動する。

### Phase 5: 数学型の移行

**目的**: SharpDX.Mathematics の型を System.Numerics / Vortice.Mathematics に置き換える。量は最大だが、ほとんどが機械的な作業。

型の対応:

| SharpDX | 移行先 | 注意点 |
|---|---|---|
| `Vector2` / `Vector3` / `Vector4` | `System.Numerics.Vector2/3/4` | `Normalize()` などのインスタンスメソッドが静的メソッドになるものがある |
| `Matrix` | `System.Numerics.Matrix4x4` | メモリ上の並び（M11..M44）と行ベクトル規約は同じ。`AffineTransformation2D` などの生成メソッドは同等の組み合わせか自作ヘルパーで置き換える |
| `Vector3.TransformCoordinate` | `Vector3.Transform` | w による除算の有無が違う。PPD の用途（アフィン変換）では同じ結果になるが、射影変換で使っていないか確認する |
| `Quaternion` | `System.Numerics.Quaternion` | 生成メソッドの名前と引数順を確認する |
| `Color4` | `Vortice.Mathematics.Color4` | プロパティ名（`Red`→`R` など）、`ToBgra()` の有無 |
| `Rectangle` / `RectangleF` | `Vortice.Mathematics` の矩形型か自前の型 | `Left/Top/Right/Bottom` と `X/Y/Width/Height` の扱いの違い |

作業（2026-09-30 に実施）:
- [x] 互換レイヤー **PPDMath**（新しいプロジェクト）を作った。
  - C# 14 の拡張メンバーで、SharpDX にあって System.Numerics にないメンバーを同じ名前で提供する
    - 静的メンバーの例: `Matrix4x4.Transformation2D`、`Matrix4x4.RotationZ`
    - インスタンスのプロパティの例: `q.Angle`、`m.Row1`、`c.Alpha`
    - 演算子: `Matrix / float`
  - 計算は、SharpDX.Mathematics 4.2.0 を逆コンパイルした実装を移した。三角関数は `Math`（double）、行列の積は SharpDX と同じ順序のスカラー演算
  - System.Numerics に同じ名前のメンバーがあって拡張にできないものは、名前を変えて `MathCompat` に置いた
    - `Translation`（Matrix4x4 のプロパティと重なる）
    - `TransformToVector4`（SharpDX の Vector2/3.Transform は Vector4 を返す）
    - `NormalizeSharpDX`（長さ 0 のときそのまま返す。System.Numerics は NaN になる）
    - `ClampSharpDX`（min > max のときの結果が違う）
    - `ToBgraInt` / `ToRgbaInt` / `Color4FromRgba`（SharpDX と同じ切り捨てで int にする。Mod の設定の保存に使っている）
  - **SharpDX.Mathematics 4.2.0 と並べて、移した全ての関数を乱数と境界値（長さ 0、min > max、-0 など）で比べ、178,969 回の比較で全てビット単位で一致した**
- [x] 504 ファイルを機械的に置き換えた。
  - `SharpDX.Vector2` → `System.Numerics.Vector2`、`SharpDX.Matrix` → `Matrix4x4`、`SharpDX.Color4` → `Vortice.Mathematics.Color4` など
  - `using SharpDX;` のあるファイルには、使っている名前に応じて別名を付けた（`using Matrix = System.Numerics.Matrix4x4;` など）。型名を変えずに済むので、`Matrix` という名前のプロパティなどはそのまま
  - `using Vortice.Mathematics;` は使わない。WinForms のファイルで `System.Drawing.Size` と名前がぶつかるため
- [x] SharpDX の DirectX の API（Raw* 型を受け取る）に渡す箇所は、`RawColor4` / `RawVector2` / `RawViewportF` に明示的に変換した（Phase 6 で Vortice に置き換える部分）。
- [x] **FlowScriptEngineSlimDX**: アセンブリ名・名前空間・ノードのクラス名は変えていない。ノードの入出力の型を System.Numerics の型にし、TypeFormatter / TypeConverter / TypeSerializer も合わせた。
  - スクリプトに書かれた値（`"X:1 Y:2"` など）の解析と、文字列への変換（SharpDX と同じ書式）は変わらない
- [x] ModCheck の既定値の出力を、数学型についてはライブラリによらない表記（`Vector2{X=.., Y=..}`、`Color4{R=.., G=.., B=.., A=..}`）にした。移行前のビルドで基準を作り直し、変わったのが数学型の 246 行だけであることを確認した。
- 確認した結果
  - Debug / Release ともビルドできる
  - シーン 0〜7 の画素比較が通った
  - PPD の起動・終了でアクセス違反も error.log への書き込みもない
  - Mod 28 個・譜面スクリプト 17 個の既定値が移行前と完全に一致した
  - PPDEditor などのアプリが起動する
- 残る違い: アプリの中の行列同士の積（`*` 演算子）は System.Numerics の実装（SIMD）を使うので、SharpDX と最下位ビットが違うことがある（乱数の行列では 98% で違った）。描画の比較では差は出ていない。FlowScript のノードには行列同士の積はないが、PPD が計算した行列を Mod が受け取ることはある
**完了条件**: `SharpDX.Mathematics` への参照がゼロ。既存の譜面スクリプト（同梱の `.fsml` と、手元にある実際の譜面のスクリプト）が読み込めて同じ動作をする。回帰テストが一致する。

**規模の目安**: 大（量が多い）。ただし作業は機械的で、分割して進めやすい。

### Phase 6: SharpDX → Vortice の差し替え

**目的**: 残りの SharpDX API を Vortice に置き換える。Phase 1・2・5 が済んでいれば、対象は PPDFramework の DX11 層と入力・音声に絞られる。

| 対象 | 置き換え内容 |
|---|---|
| PPDFramework `DX11/`、`Texture/DX11/`、`Vertex/DX11/`、`Effect/DX11/` | `D3D11.D3D11CreateDeviceAndSwapChain`、各リソースの作成、`DeviceContext` の呼び出しを Vortice の API に置き換える。フラグやデスクリプションの型名が細かく違う |
| COM オブジェクトの寿命 | SharpDX の `EnableReleaseOnFinalizer`（GameCoreBase で有効化）への依存をやめ、`Dispose` で確実に解放する。Vortice では解放漏れの検出（`DXGI` デバッグ層、ReportLiveObjects）を開発中に使う |
| `Direct2DImageEncoder`（文字画像生成） | Vortice の Direct2D1 / DirectWrite / WIC に置き換える |
| RenderForm / RenderLoop（SharpDX.Desktop） | 自前の `Form` と、`PeekMessage` を使ったメッセージループに置き換える |
| PPDInput / KeyConfiger / PPDEditor の DirectInput | Vortice.DirectInput に置き換える（ゲームパッドは将来 XInput も検討） |
| PPDSound の DirectSound | Vortice.DirectSound に置き換える |
| PPDMovie（SampleGrabber 経由で描画） | テクスチャ書き込み部分のみ。DirectShow 部分はそのまま |

作業:
- [x] DirectInput（2026-09-30）: PPDInput・PPD・PPDEditor・KeyConfiger・PPDTest を Vortice.DirectInput 3.8.3 に置き換えた。
  - `Key` 列挙体は SharpDX と同じ名前・値。Vortice には別名の `CapsLock`（= `Capital`）が加わっているだけで、`ToString()` の結果は変わらない。キー設定の保存形式に影響はない
  - デバイスは `CreateDevice` と `SetDataFormat<RawKeyboardState>()` / `SetDataFormat<RawJoystickState>()` で作る
  - `Acquire()` / `Poll()` などは `Result` を返すので `.CheckError()` を付け、SharpDX と同じく失敗したときに例外にした
  - 配布ファイルの一覧（Checker / PPDReleaser）に `Vortice.DirectInput.dll`、`Vortice.DirectX.dll`、`SharpGen.Runtime.dll`、`SharpGen.Runtime.COM.dll` を加えた
  - 確認した結果: シーン 0〜8 を DirectInput で受けたキー入力で切り替えられ、画素比較も通った。PPD の起動・終了、Mod の既定値、KeyConfiger と PPDEditor の起動も問題なし
- [x] DirectSound（2026-09-30）: PPDSound を Vortice.DirectSound 3.8.3 に置き換えた。
  - `SecondarySoundBuffer` → `IDirectSoundBuffer`（`CreateSoundBuffer`）。`Volume` / `Pan` / `CurrentPosition` のプロパティは `SetVolume` などのメソッドになった
  - WAV の読み込みに使っていた SharpDX の `SoundStream` の代わりに、FMOD の出力から作る RIFF WAVE（fmt / data チャンク）を読む小さな関数を置いた
  - 確認した結果: 同梱の効果音 7 個（ogg / wav）を読み込み、バッファの中身・再生状態・再生速度の変更・停止を確かめた。RenderCheck も全て通った
  - 注意: Vortice 3.8.3 の `IDirectSoundBuffer.Caps` は E_INVALIDARG になる（PPD では使っていない）
- [x] 使っていない `using SharpDX;`（208 ファイル）と、18 プロジェクトの SharpDX への参照を削除した（2026-09-30）
- [x] ShaderCompiler（2026-09-30）: Vortice.D3DCompiler の `Compiler.Compile`（UTF-8 のバイト列で渡す）と、Vortice.Direct3D11 の `ID3D11ShaderReflection` に置き換えた。
  - **出力するシェーダーのバイナリ（d3d11_*.bin の 6 個）が移行前とバイト単位で一致することを確認した**
- [x] D3D11 / DXGI（2026-09-30）: PPDFramework の DX11 層（PPDDevice、Texture、TextureFactory、VertexBuffer、IndexBuffer、VertexDeclaration、Effect）を Vortice.Direct3D11 / Vortice.DXGI 3.8.3 に置き換えた。
  - `D3D11CreateDeviceAndSwapChain` で作る（失敗したら WARP。SharpDX のときと同じ）
  - 注意した違い
    - `CopyResource` の引数の順番が逆（SharpDX は (コピー元, コピー先)、Vortice は (コピー先, コピー元)）
    - `RSSetScissorRect(int, int, int, int)` は (x, y, 幅, 高さ)。SharpDX の `SetScissorRectangle` は (左, 上, 右, 下) なので、`RawRect` を渡す形にした
    - `ResourceRegion` → `Vortice.Mathematics.Box`（引数の順番は同じ）
  - `PPDDevice.Device` の型は `SharpGen.Runtime.ComObject`、`StretchRectangle` の引数は `Vortice.RawRect?` / `Int2?` にした（FlowScript からは使っていない）
  - `TextureDataBase.DataStream` は PPDFramework の `DataStream`（`UnmanagedMemoryStream` に `DataPointer` を加えたもの）にした。Vortice の DataStream はポインターから作れないため
- [x] Direct2D / DirectWrite / WIC（2026-09-30）: Direct2DImageEncoder を Vortice.Direct2D1 に置き換えた。SharpDX の既定値（D2D はシングルスレッド、DirectWrite は Shared、エンコーダーは NoCache）に合わせた。Dispose していなかった `TextFormat` と `WICStream` を Dispose するようにした
- [x] `EnableReleaseOnFinalizer`: SharpGen.Runtime にも同じ設定（`SharpGen.Runtime.Configuration.EnableReleaseOnFinalizer`）があるので、GameCoreBase で同じように有効にした（ファイナライザーで COM の参照を解放する。SharpDX と同じ動作）。
  - この設定への依存をなくす（全て Dispose で解放し、ReportLiveObjects で漏れを確かめる）のは今後の課題として残す
- 確認した結果（2026-09-30）
  - `SharpDX*` のパッケージ・DLL への参照はゼロ（PPD.deps.json にも出てこない）。Debug / Release ともビルドできる
  - シーン 0〜7 の画素比較が通った（文字は D2D で描画している）。PPD の起動・終了でアクセス違反なし。Mod 28 個・譜面スクリプト 17 個の既定値が一致
  - PPDEditor、PPDConfig、KeyConfiger、Effect2DEditor、BMSTOPPD、FlowScriptControlTest が起動する
  - 配布ファイルの一覧を `Vortice.Direct2D1.dll`、`Vortice.DXGI.dll`、`Vortice.Direct3D11.dll` に更新し、`SharpDX.dll` を外した
  - 既知の問題（移行前から）: 文字の描画方法を WinAPI（`[texturecharmode]1`）にすると文字が表示されない。移行前のビルドでも同じ画像になることを確認した。既定の D2D では問題ない

**完了条件**: `SharpDX*` への参照がゼロ。回帰テスト、実プレイ確認、終了時のアクセス違反チェックが通る。フレームレートが移行前と同等以上。

**規模の目安**: 中。

### Phase 7: 配布・更新の仕組みとリリース

- [ ] 配布形態（D3: 現状のまま、フレームワーク依存）に合わせて `dotnet publish` の設定を作り、インストーラーに .NET 10 Desktop Runtime の確認・案内を追加する。
- [ ] `PPDReleaser` の DLL 一覧、`PPD\Update\Checker.cs` の `CheckAssembly` の一覧を新しい構成に更新する。
- [ ] PPDUpdater（D6）: 旧バージョン（.NET Framework 版）から新バージョンへ更新できる経路を用意する。
  - ランタイムを含むため更新パッケージが大きくなる。初回だけフルインストーラーを案内する方式も検討する。
- [ ] PPDInstaller: 前提条件（.NET Framework 4.8 のチェックなど）を見直す。
- [ ] Mod 作者向けに、変更点のお知らせを書く（Mod はそのまま動く前提。FlowScript エディターで新しく作る Mod のバージョン記録が上がる旨など）。
- [ ] 同梱スキン（PPDSingle、PPDMulti）と FlowScript 拡張（`dlls\`）を新環境でビルドして同梱する。

---

## 5. プロジェクトの移行順序（Phase 3〜5 共通）

プロジェクト参照の依存関係から、下位のものから順に移行します。

1. **依存なし**: PPDConfiguration、PPDFrameworkCore、PPDPack、FlowScriptEngine、BezierCaliculator、ErrorHandle、BlueSky、WinFormsUI、DirectShow、ColorCanvas
2. **基盤**: Effect2D → PPDFramework → PPDInput / PPDSound / PPDMovie
3. **ゲームロジック**: PPDCoreModel → PPDExpansionCore / PPDMultiCommon → PPDCore → PPDShareComponent
4. **FlowScript 拡張**: FlowScriptEngineBasic / BasicExtension / Console / Data / SlimDX / PPD / PPDEditor
5. **UI 部品**: FlowScriptDrawControl → FlowScriptControl、BezierDrawControl、PPDEditorCommon、PPDModSettingUI
6. **スキン**: PPDSingle、PPDMulti（+ PPDMultiServer）
7. **アプリ**: PPD、PPDEditor、PPDConfig、KeyConfiger、PPDExpansion、Effect2DEditor、PPDMultiServerService
8. **ツール・テスト**: PPDTest、BezierDrawControlTest、FlowScriptControlTest、BlueSkyTest、ShaderCompiler、PPDReleaser、PPDInstaller、PPDUpdater、PPDUnpack、BMSTOPPD、FolderDiffChecker、FlowScriptAutoFastGenerator

---

## 6. 検証計画

各フェーズの完了時に、以下を全て実施します。

| 検証 | 方法 | 合格基準 |
|---|---|---|
| ビルド | ソリューション全体（Debug / Release、x64） | エラーなし |
| 描画の回帰 | PPDTest の全シーンを自動撮影し、基準画像と画素比較 | 差分が輪郭の数画素以内。差分画像を目視確認 |
| 終了時の例外 | 簡易デバッガーで PPD を起動・終了 | アクセス違反なし、終了コード 0 |
| 実プレイ | チェックリスト（Phase 0 で作成） | 全項目 OK |
| 性能 | タイトル・選曲・プレイ中の FPS、起動時間、メモリ使用量 | 移行前と同等以上 |
| 互換性 | 既存の `PPD.db`、`PPD.ini`、譜面・譜面スクリプト、同梱スキン | 読み込めて同じ動作 |
| マルチプレイ | 新旧クライアント・サーバーの組み合わせ（MessagePack 更新時） | 想定どおりの互換性（非互換にする場合は明示） |

---

## 7. リスクと対策

| リスク | 影響 | 対策 |
|---|---|---|
| Mod が動かなくなる | 利用者体験 | 3 章「Mod 互換の条件」を守る。各フェーズで実際の Mod を使って確認する |
| Effects11 置き換えで描画結果が変わる | 見た目 | Phase 0 の回帰テストで全テクニックを確認する。PPDTest にブレンド × フィルタ × マスクの組み合わせを網羅するシーンを追加する |
| SharpDX 4.2.0 が .NET 10 で動かない | Phase 4 が進められない | Phase 4 の最初に検証する。だめなら Phase 4〜6 を統合し、直接 Vortice に移る（その場合、中間の「動く状態」が作りにくくなる） |
| WinForms の既定フォント・DPI の変化 | 画面の配置崩れ | `Application.SetDefaultFont` と DPI モードで従来の見え方を再現し、全画面を目視確認する |
| MessagePack の更新で通信の互換性が崩れる | マルチプレイ | 同じフォーマットになる設定を確認する。互換が取れない場合はプロトコルのバージョンを上げ、サーバーと同時に更新する |
| SQLite ライブラリの変更で既存 DB が読めない | スコア・設定の消失 | 移行前に既存の `PPD.db` のコピーで読み書きを確認する。必要なら初回起動時に変換する |
| 利用者の PC に .NET 10 Desktop Runtime がない | 起動できない | インストーラー・アップデーターでランタイムの有無を確認し、導入を案内する |
| 数学型の置換で、微妙な計算の違いが出る | 譜面の動き・判定 | 互換用の拡張メソッドを使い、動きの差が出やすい箇所（ベジェ、マーカー移動、回転）を重点的に比較する |

---

## 8. 規模の目安

1 人で進める場合のおおよその目安です（着手後に見直す）。

| フェーズ | 目安 |
|---|---|
| Phase 0 準備 | 3〜5 日 |
| Phase 1 DX11 一本化 | 3〜5 日 |
| Phase 2 Effects11 脱却 | 1〜2 週 |
| Phase 3 SDK 形式化・依存整理 | 1〜2 週 |
| Phase 4 .NET 10 移行 | 1〜2 週 |
| Phase 5 数学型の移行 | 2〜3 週 |
| Phase 6 Vortice 差し替え | 1〜2 週 |
| Phase 7 配布・リリース | 1 週 |
| **合計** | **おおよそ 2〜3 か月** |

---

## 付録: 調べた内容のメモ

- 保存済みの譜面スクリプト（`.fsml`）には、`<Source Name="..." FullName="...">` でノードのクラス名だけが保存される。SharpDX の型名は含まれない（`PPDEditor/PPDEditor/Commands/*/Command.fsml` で確認）。
- `FlowScriptEngine.Serializer` は値の型を `type.FullName` で出力する。使っているのは、FlowScript のデバッガー表示（`FlowSourceManager.SerializeSource`）と、YAML ダンプ（FlowScriptEngineData）のみ。
- プラグインは `Assembly.LoadFrom` で読み込まれる（`PPD/PPD/GameLoader.cs` がスキン、`FlowScriptEngine/FlowSourceEnumerator.cs` が FlowScript 拡張）。
- SharpDX の DLL 名は、`PPD/PPD/Update/Checker.cs`（`CheckAssembly`）と `PPDReleaser/Program.cs` にハードコードされている。
- `EnableReleaseOnFinalizer = true` は `GameCoreBase` のコンストラクタで設定している。Effects11 の変数やパスに対してこれが働くと、終了時にアクセス違反が起きる（Phase 1 では `Effect.SuppressRelease` で回避していた。Phase 2 で Effects11 ごとなくなった）。
