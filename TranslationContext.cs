using System;
using System.IO;

namespace SKyrimSEModsSemiAutoTranslator
{
    /// <summary>
    /// アプリケーション全体のパス設定と状態を一元管理するコンテキストクラス
    /// </summary>
    internal class TranslationContext
    {
        private readonly AppSettings _settings;

        public TranslationContext(AppSettings settings)
        {
            _settings = settings;
        }

        // --- 基本設定へのアクセス ---
        public string Mo2ModDir => _settings.MO2ModDir;
        public string SseAtExePath => _settings.SseATPath;
        public string SevenZipPath => _settings.SevenZipPath;
        public string CacheDir => _settings.TranslationFileCacheDir;
        public string FinalDestDir => _settings.FinalDestDir;

        // --- ワークディレクトリ構成 ---
        // ユーザー指定のワークディレクトリをルートとする
        public string WorkRootDir => _settings.WorkDir;

        // [Step 1 Output / Step 3 Input] 翻訳対象ファイル置き場 (作業用)
        public string TargetDir => Path.Combine(WorkRootDir, "Target_Files");
        public string TargetEspDir => Path.Combine(TargetDir, "ESP");
        public string TargetMcmDir => Path.Combine(TargetDir, "MCM"); // Step1で抽出したTXT
        public string TempInternalDir => Path.Combine(WorkRootDir, "Temp_Internal"); // 一時作業用

        // [Step 2 Output / Step 3 Input] ダウンロードした翻訳アーカイブ
        public string DownloadDir => Path.Combine(WorkRootDir, "Downloaded_Translations");

        // [Step 3 Internal] 展開・仕分けされた翻訳ファイル (XML/TXT)
        public string SortedDir => Path.Combine(WorkRootDir, "Sorted_Translations");
        public string SortedEspDir => Path.Combine(SortedDir, "ESP");
        public string SortedMcmDir => Path.Combine(SortedDir, "MCM");
        public string SortedPexDir => Path.Combine(SortedDir, "PEX");
        public string SortedTxtDir => Path.Combine(SortedDir, "TXT"); // 翻訳済みTXT

        // [Step 3 Output] xTranslator用バッチファイル出力先
        public string BatchFilesDir => Path.Combine(WorkRootDir, "BatchFilesDir");

        // [Merged XML] フォールバック用マージXMLのパス
        public string MergedEspXmlPath => Path.Combine(SortedEspDir, "Merged_ESP.xml");
        public string MergedMcmXmlPath => Path.Combine(SortedMcmDir, "Merged_MCM.xml");

        // [Output] 最終的な翻訳結果 (xTranslator実行後の移動先イメージ)
        public string TranslatedDir => Path.Combine(WorkRootDir, "Translated");
        public string TranslatedEspDir => Path.Combine(TranslatedDir, "ESP");
        public string TranslatedTxtDir => Path.Combine(TranslatedDir, "TXT");

        /// <summary>
        /// 必要なディレクトリを一括作成・初期化する
        /// </summary>
        public void EnsureDirectories()
        {
            string[] dirs = {
                TargetEspDir, TargetMcmDir,
                DownloadDir,
                SortedEspDir, SortedMcmDir, SortedPexDir, SortedTxtDir,
                BatchFilesDir,
                TranslatedEspDir, TranslatedTxtDir,
                FinalDestDir
            };

            foreach (var dir in dirs)
            {
                Directory.CreateDirectory(dir);
            }
        }
    }
}