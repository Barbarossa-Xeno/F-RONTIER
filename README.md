# F-RONTIER
リズムゲーム「F-RONTIER」のリポジトリです。Unityで制作しています。（現在開発6か月目）

## ゲームについて
最近音ゲーに触発され、一文無しでもアーケードみたいな音ゲーをプレイしたい。そしてあんなものやこんなものといった楽曲を自分の音ゲーに仕立て上げたい。このゲームが制作された目的はそんなところです。

## ソースコードについて
面倒くさくて全部ソースコードを上げました。まるまる全部見ようものなら卒倒すると思うので、以下のファイルを見てくれたらうれしい。全部Scriptsフォルダ内のGameフォルダにあります。

### おぬぬめ
- [JudgementManager.cs](Scripts/Game/JudgementManager.cs)<br/>音ゲーで一番大事な判定をとるクラス
- [Notes.cs](Scripts/Game/NotesManagement/Notes.cs)<br/>ノーツそのものを動かしたり、種類分けしたりするためのクラス
- [NotesGenerator.cs](Scripts/Game/NotesManagement/NotesGenerator.cs)<br/>ノーツを生成するクラス。なお、[NotesManager.cs](Scripts/Game/NotesManagement/NotesManager.cs)を継承

### 注意点
- 初めてC#で本格的なコードを描いたので、見づらい部分があるとは承知していますが許してください。<br/>
- 名前空間名はディレクトリ構造に沿った形にしています。
- ほかの人のスクリプトを参照させていただいた部分には、適宜URLを貼付しています。

## 外部アセット
- [UniRx](https://assetstore.unity.com/packages/tools/integration/unirx-reactive-extensions-for-unity-17276)
- [FancyScrollView](https://github.com/setchi/FancyScrollView)

## 「F-RONTIER」の今後の展望
・操作感がより快適になるようプログラムを調整していきます。<br/>
・もっとゲームが楽しくなるように新機能の導入を検討しています。<br/>
・全年齢版とそうでないもの（Fを察して）の2バージョンをマスターアップさせる気でいます。<br/>
・他の方のプログラムを参考にしていたり、曲の権利問題だったりがあるので、こんなゲームを世に出して売る、配信するつもりはありません。<br/>
・頑張ってアーケード版も作りたいです。<br/>
