using System;
using System.IO;
using System.Reflection;
using System.Xml.Serialization;

namespace AutoStructure.Common
{
    /// <summary>
    /// 外部設定ファイル（AutoStructureMaker.config.xml）で管理される設定データモデル
    /// </summary>
    [Serializable]
    [XmlRoot("AppSettings")]
    public class AppSettings
    {
        public string NetworkDirectory { get; set; } = @"\\Server\Share\AutoStructure\";
        public string LocalFallbackDirectory { get; set; } = "";
        public string SaveFileNamePrefix { get; set; } = "case_";
        public string DefaultDicomType { get; set; } = "PTV";
        public string DefaultBoolOperation { get; set; } = "SUB";
        public string DefaultMarginGeometry { get; set; } = "Outer";

        public int DefaultMarginX1 { get; set; } = 7;
        public int DefaultMarginX2 { get; set; } = 7;
        public int DefaultMarginY1 { get; set; } = 7;
        public int DefaultMarginY2 { get; set; } = 7;
        public int DefaultMarginZ1 { get; set; } = 7;
        public int DefaultMarginZ2 { get; set; } = 7;
    }

    /// <summary>
    /// アプリケーション設定管理クラス。外部XML設定ファイルの読み込み、自動生成、および安全なパス解決を提供します。
    /// </summary>
    public static class AppConfig
    {
        public const string ConfigFileName = "AutoStructureMaker.config.xml";
        private static AppSettings _current;

        public static AppSettings Current
        {
            get
            {
                if (_current == null)
                {
                    _current = LoadSettings();
                }
                return _current;
            }
            set => _current = value;
        }

        /// <summary>
        /// 外部XML設定ファイルを探索して読み込みます。
        /// 1. プラグインDLLと同一ディレクトリの AutoStructureMaker.config.xml
        /// 2. カレントディレクトリの AutoStructureMaker.config.xml
        /// 3. 実行プロセスの BaseDirectory の AutoStructureMaker.config.xml
        /// 4. %APPDATA%\AutoStructureMaker\AutoStructureMaker.config.xml
        /// 見つからない場合はデフォルト設定を生成して返します。
        /// </summary>
        public static AppSettings LoadSettings()
        {
            string dllDir = GetAssemblyDirectory();

            string[] candidatePaths = new[]
            {
                !string.IsNullOrEmpty(dllDir) ? Path.Combine(dllDir, ConfigFileName) : null,
                Path.Combine(Directory.GetCurrentDirectory(), ConfigFileName),
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ConfigFileName),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "AutoStructureMaker", ConfigFileName)
            };

            foreach (var path in candidatePaths)
            {
                if (!string.IsNullOrEmpty(path) && File.Exists(path))
                {
                    try
                    {
                        using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                        {
                            var serializer = new XmlSerializer(typeof(AppSettings));
                            var settings = (AppSettings)serializer.Deserialize(fs);
                            if (settings != null)
                            {
                                return settings;
                            }
                        }
                    }
                    catch
                    {
                        // 読み込み失敗時は次の候補へ
                    }
                }
            }

            var defaultSettings = new AppSettings();
            TrySaveDefaultSettings(dllDir, defaultSettings);
            return defaultSettings;
        }

        private static string GetAssemblyDirectory()
        {
            try
            {
                string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                if (!string.IsNullOrEmpty(codeBase))
                {
                    var uri = new Uri(codeBase);
                    return Path.GetDirectoryName(uri.LocalPath);
                }
            }
            catch
            {
                try
                {
                    return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                }
                catch { }
            }
            return null;
        }

        private static void TrySaveDefaultSettings(string preferredDir, AppSettings settings)
        {
            try
            {
                string targetDir = preferredDir;
                if (string.IsNullOrEmpty(targetDir) || !Directory.Exists(targetDir))
                {
                    targetDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "AutoStructureMaker");
                }

                if (!Directory.Exists(targetDir))
                {
                    Directory.CreateDirectory(targetDir);
                }

                string targetPath = Path.Combine(targetDir, ConfigFileName);
                if (!File.Exists(targetPath))
                {
                    using (var fs = new FileStream(targetPath, FileMode.Create, FileAccess.Write))
                    {
                        var serializer = new XmlSerializer(typeof(AppSettings));
                        serializer.Serialize(fs, settings);
                    }
                }
            }
            catch
            {
                // 書き込み権限がない場合などは安全に無視
            }
        }

        /// <summary>
        /// パラメータファイル（CSV）保存/読込の既定ディレクトリを取得します。
        /// ネットワークUNCパスへのアクセスは1秒のタイムアウトを設け、院外環境でのUIフリーズを防止します。
        /// </summary>
        public static string GetInitialDirectory()
        {
            var s = Current;

            // 1. 外部設定のネットワークディレクトリ (1秒タイムアウトで高速フォールバック)
            if (!string.IsNullOrWhiteSpace(s.NetworkDirectory))
            {
                bool exists = false;
                try
                {
                    var task = System.Threading.Tasks.Task.Run(() => Directory.Exists(s.NetworkDirectory));
                    if (task.Wait(1000))
                    {
                        exists = task.Result;
                    }
                }
                catch { }

                if (exists)
                {
                    return s.NetworkDirectory;
                }
            }

            // 2. 外部設定のローカルフォールバックディレクトリ
            if (!string.IsNullOrWhiteSpace(s.LocalFallbackDirectory))
            {
                try
                {
                    if (!Directory.Exists(s.LocalFallbackDirectory))
                    {
                        Directory.CreateDirectory(s.LocalFallbackDirectory);
                    }
                    return s.LocalFallbackDirectory;
                }
                catch { }
            }

            // 3. ドキュメント配下の AutoStructure フォルダ
            string defaultLocal = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "AutoStructure");
            try
            {
                if (!Directory.Exists(defaultLocal))
                {
                    Directory.CreateDirectory(defaultLocal);
                }
                return defaultLocal;
            }
            catch
            {
                return Path.GetTempPath();
            }
        }
    }
}
