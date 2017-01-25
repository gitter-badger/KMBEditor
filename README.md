[![Build status](https://ci.appveyor.com/api/projects/status/vg1e2udxedr14h67/branch/master?svg=true&passingText=master)](https://ci.appveyor.com/project/tar-bin/kmbeditor-cyyma/branch/master)
[![Build status](https://ci.appveyor.com/api/projects/status/vg1e2udxedr14h67?svg=true)](https://ci.appveyor.com/project/tar-bin/kmbeditor-cyyma)

KMBEditor
==

※現状まだ開発途中です。実用に耐えうるものではありません。

# これはなに？

AA(アスキーアート) 作成用エディタ(になる予定のもの)です。

当面はOrinrinEditorの補助的なツール、将来的には代替+αを目標としています。

+ [SikigamiHNQ/OrinrinEditor: AsciiArt Story Editor for Japanese Only](https://github.com/SikigamiHNQ/OrinrinEditor)
+ [OrinrinEditor | やる夫 Wiki | Fandom powered by Wikia](http://yaruo.wikia.com/wiki/OrinrinEditor)

また、MLTViewerの仕様に関しては、Nanorymerを一部参考にしています

+ [ファザコンなのははAAビューアを作ってしまったようです - 1463138480 - したらば掲示板](http://jbbs.shitaraba.net/bbs/read.cgi/otaku/15956/1463138480/)

# 最新リリースパッケージ

最新のリリースパッケージは以下から取得できます。`KMBEditor-v(version).zip` をダウンロード後、解凍して下さい。

https://github.com/tar-bin/KMBEditor/releases/latest

ウイルス対策ソフトによって、添付のライブラリや実行ファイルが除外されてしまった場合は、お手数ですが除外設定と復元をするようにお願いします。

# 使用方法・オンラインドキュメント
詳しいドキュメントは以下を参照してください。

[Introduction · KMBEditor Document](https://tar-bin.gitbooks.io/kmbeditor-document/content/)

# コンセプト

+ 新規AA作成補助をメイン機能とする
+ 多言語対応はしない（日本語のみ）
+ オープンソース / フリーソフトウェア（[GPLv3](https://www.gnu.org/licenses/gpl-3.0.ja.html) : [日本語訳](https://mag.osdn.jp/07/09/02/130237)）

# サポート環境

+ Windows 7 ( .Net Framework 4.5 以上のランタイム必須 ※1 )
+ Windows 8 / 8.1
+ Windows 10

※ Window Vista / XP 以前はサポート外となります。[OrinrinEditor](http://yaruo.wikia.com/wiki/OrinrinEditor)をご利用ください。  
※1 :  .Net Framework 4.5 以上のランタイムについて  
必要に応じて[Microsoftの公式ページ](https://msdn.microsoft.com/ja-jp/library/5a4x27ek(v=vs.110).aspx)から、.NET Framework 4.5 以上のランタイムをインストールしてください。  
Windows 8 以降は標準で .NET Framework 4.5 以上がインストールされているため、基本的に対応は必要ありません。  

# 免責事項
+ 当ソフトウェアおよび当ソフトウェアを改変/再配布されたものを利用したことによるいかなる損害も作者は一切の責任を負いません。自己の責任の上でご利用ください。

# 開発環境
## 開発言語

+ C# 6.0

## 使用フレームワーク/ライブラリ

+ [Microsoft .NET Framework 4.5 (Microsoft Reference Source License)](https://www.microsoft.com/ja-jp/download/details.aspx?id=30653)
+ [MVVM Light 5.3.0 (MIT License)](https://www.nuget.org/packages/MvvmLight/)
+ [ReactiveProperty 3.4.0 (MIT License)](https://github.com/runceel/ReactiveProperty)
+ [Json.NET 9.0.1 (MIT License)](http://www.newtonsoft.com/json)
+ [SQLite 1.0.104 (Public Domain)](https://www.nuget.org/packages/System.Data.SQLite/)
+ [WPF Chrome Tabs - MVVM 1.2.7 (MIT License)](https://www.nuget.org/packages/WPFChromeTabsMVVM/)
+ [MahApps.Metro 1.4.1 (MIT License)](https://www.nuget.org/packages/MahApps.Metro)
+ [Material Design Themes XAML Resources 2.2.1.750 (Microsoft Public License)](https://www.nuget.org/packages/MaterialDesignThemes/)
+ [Dragablz - Dragable and tearable tab control for WPF 0.0.3.182 (Microsoft Public License)](https://www.nuget.org/packages/Dragablz/)

## アイコン画像

+ [Icon8](https://icons8.com/)([CC BY-ND 3.0](https://icons8.com/license/))

## 開発環境

+ Visual Studio 2015

# Q & A

Q1. AAの回覧機能だけほしい

A1. [AAMZ Viewer](http://aa.yaruyomi.com/)をご使用ください。
