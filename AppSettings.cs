using System;
using System.Collections.Generic;
using System.Text;

namespace SKyrimSEModsSemiAutoTranslator
{
    internal class AppSettings
    {
        public string MO2ModDir { get; set; } = "";
        public string SseATPath { get; set; } = "";
        public string SevenZipPath { get; set; } = "";
        public string TranslationFileCacheDir { get; set; } = "";
        public string WorkDir { get; set; } = "";
        public string FinalDestDir { get; set; } = "";
    }
}
