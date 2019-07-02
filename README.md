# PPD

[PPD](https://projectdxxx.me/) is a rhythm game inspired by [Project Diva](http://miku.sega.jp/) series.

This repository only contains client side game source code.

## Installation

Please download installer from https://projectdxxx.me/download and execute it.

## Contribute

### Prerequisites

* Windows 10 x64
* Visual Studio Community 2019
  * Workload
    * .NET Desktop Development
  * Individual Component
    * Windows 10 SDK(10.0.18362.0)

### Build

1. Open `Win\PPD.sln`
2. Restore packages by nuget
3. Build `x64` `Debug`
4. Copy `Win\release\resource\Data\PPD\*` to `Win\PPD\PPD\bin\x64\Debug`
5. Run `PPD`

When running the programs not PPD.exe, it may requires copy `Win\release\resource\Data\PPD\*` to Debug folder.

### FAQ

Q. I want to implement/fix/redesign xxxxx, where is the document?

A. Sorry, there is no document about implementation because this project was maintained by only KHCmaster. If you have any questions, join [Slack(PPDDev Workspace)](https://join.slack.com/t/ppddev/shared_invite/enQtNjg0Mzg4NTY1MjcxLWFkOGUzYmFiYjY1NjA2Yjk1ZWUyOTY2OGJkZTI0NmY0NDNiZGRiODVmMTQ0NThjNjkxMTlmYzIzNTkzZGRhZjg) and ask them.

Q. I want to use songs dir which is in Program Files.

A. You can specify songs dir in `PPD.ini` like below.

```
[songdir]D:\KHC\PPD\songs
```

Q. I want to run PPDSingle.dll without selecting game.

A. Open `PPD(csproj)`, select `Debug` and edit `Command Line Arguments`

```sh
# Launch PPDSingle
-game PPDSingle
```

```sh
# Launch PPDSingle with login
-game PPDSingle -login
```
