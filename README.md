# SkyeimSEModsSemiAutoTranslator

**SkyeimSEModsSemiAutoTranslator** は、Mod Organizer 2 (MO2) 環境下の Skyrim SE/AE Mod 構築において、翻訳作業を半自動化するツールです。

「Skyrim Special Edition Mod データベース」から翻訳ファイルを自動ダウンロードし、`xTranslator` および `SSE-AT` (esp2dsd) と連携して、翻訳データの適用から辞書生成 (JSON) までを一貫してサポートします。

## 動作環境

* **OS**: Windows 10 / 11 (64bit)
* **必須ランタイム**: .NET Desktop Runtime 10.0 以上
* **必須ツール**:
    * [Mod Organizer 2](https://www.nexusmods.com/skyrimspecialedition/mods/6194)
    * [xTranslator](https://www.nexusmods.com/skyrimspecialedition/mods/134)
    * [SSE-AT](https://www.nexusmods.com/skyrimspecialedition/mods/29328) (esp2dsd 機能を使用)
    * [Dynamic String Distributor](https://www.nexusmods.com/skyrimspecialedition/mods/86905) (生成されたJSONの読み込みに必要)
    * [7-Zip](https://www.7-zip.org/) (アーカイブ展開用)

## 使い方 (Usage)

1.  **パス設定**:
    * 画面上部の各項目に、ご自身の環境に合わせたパスを設定してください。
    * **Final Output Dir** に指定したフォルダが、最終的な翻訳済みModとして出力されます。
    * ※注意: **Work Folder** は処理開始時に初期化（全削除）されます。他の重要なフォルダを指定しないでください。
2.  **実行**:
    * 「処理開始」ボタンを押します。
    * ツールが自動的に Step 1 ～ Step 3 までを実行します。
3.  **手動翻訳 (Step 4)**:
    * 処理が一時停止し、ガイド画面が表示されます。
    * 指示に従って **xTranslator** を起動し、自動生成されたバッチファイル (`Batch_ESP.txt` / `Batch_MCM.txt`) を読み込んで「プロセッサを実行」してください。
    * 翻訳完了後、ガイド画面の「翻訳完了」ボタンを押します。
4.  **完了**:
    * Step 5 が実行され、`Final Output Dir` に成果物が出力されます。
    * MO2 で `Final Output Dir` を新しい Mod として有効化してください。
    * **Note**: 生成されたJSONファイルをゲームに適用するため、**Dynamic String Distributor** がインストールされている必要があります。

## 内部処理の概要 (Internal Workflow)

本ツールは以下の5ステップで処理を行います。

### Step 1: 翻訳対象の抽出 (Extract)
* MO2のModsフォルダをスキャンし、未翻訳のプラグインファイル (.esp/.esm/.esl) を抽出します。
* Creation Club コンテンツや、既に翻訳済み (SSE-ATのJSONが存在する) のファイルは自動的に除外されます。
* Modに含まれる MCM用テキストファイル (`*_english.txt`) も抽出し、日本語ファイル名にリネームして準備します。

### Step 2: 翻訳ファイルのダウンロード (Download)
* 抽出したModの `meta.ini` から Mod ID とバージョンを取得します。
* 「Skyrim Special Edition Mod データベース」をスクレイピングし、適切なバージョンの翻訳ファイルを自動ダウンロードします。
* **バージョン照合**: Modのバージョンと完全に一致する翻訳ファイルを優先し、ない場合は最新バージョンをフォールバックとして選択します。
* **キャッシュ機能**: 一度ダウンロードしたファイルはローカルにキャッシュし、次回以降の通信を省略します。

### Step 3: 展開・仕分け・マージ (Process)
* ダウンロードしたアーカイブ (zip/7z/rar/xml) を展開します。
* **厳密なマッチング**: `<ESP名>_english_japanese.xml` というファイル名が見つかった場合、そのESP専用の翻訳ファイルとして紐付けます。
* **フォールバック**: 専用のXMLがない場合、残りのXMLをすべてマージした `Merged_ESP.xml` / `Merged_MCM.xml` を生成し、辞書として使用します。
* **バッチ生成**: xTranslator 用のバッチプロセッサファイル (`Batch_ESP.txt` / `Batch_MCM.txt`) を生成します。
* **MCM事前準備**: MCMテキストファイルを翻訳出力先フォルダへ事前にコピーし、上書き翻訳に備えます。

### Step 4: xTranslator による翻訳 (Translate)
* ユーザー操作によるステップです。
* 生成されたバッチファイルを使用して、xTranslator で一括翻訳を行います。
* ESPは直接翻訳され、MCMテキストは辞書に基づいて置換されます。

### Step 5: 成果物の配備 (Finalize)
* 翻訳されたESPを `SSE-AT` (esp2dsd) を使用して `.json` 形式 (Dynamic String Distributor用) に変換します。
* 翻訳されたMCMテキストファイル、および翻訳アーカイブに含まれていた翻訳済みテキストファイルを適切なフォルダ階層 (`interface\translations`) に配置します。
* 最終的な成果物を指定された出力フォルダ (`Final Output Dir`) に集約します。

## 謝辞とリンク (Credits & Links)

本ツールの開発にあたり、以下の素晴らしいツールおよびコミュニティのリソースを使用・参照させていただきました。

### Tools
* **xTranslator** by McGuffin
    * [Nexus Mods Link](https://www.nexusmods.com/skyrimspecialedition/mods/134)
    * Skyrim翻訳ツールのデファクトスタンダードです。
* **SSE-AT (Skyrim Special Edition Auto Translator)** by NiK
    * [Nexus Mods Link](https://www.nexusmods.com/skyrimspecialedition/mods/29328)
    * ESPをJSON形式(DSD)に変換する機能を使用しています。
* **Dynamic String Distributor** by Shad0wshayd3
    * [Nexus Mods Link](https://www.nexusmods.com/skyrimspecialedition/mods/86905)
    * 実行時に翻訳データをゲームへ適用するためのSKSEプラグインです。
* **7-Zip** by Igor Pavlov
    * [Official Site](https://www.7-zip.org/)
    * 高圧縮率のファイルアーカイバ。翻訳ファイルの展開に使用しています。

### Resources
* **Skyrim Special Edition Mod データベース**
    * [Site Link](https://skyrimspecialedition.2game.info/)
    * 翻訳ファイルの配布元として利用させていただいております。日本のSkyrimコミュニティに深く感謝します。

## 免責事項 (Disclaimer)
このソフトウェアは、個人的な利用を目的として作成されたものです。本ツールを使用したことによる、Mod環境の破損やセーブデータの不具合等について、作者は一切の責任を負いません。自己責任でご利用ください。
また、本ツールは各配布サイトへの過度なアクセス負荷を避けるため、ウェイト処理やキャッシュ機能を実装していますが、利用者はマナーを守って使用してください。

***

**Author**: hirohiro73
**License**: MIT License
