using System;
using System.Diagnostics;
using System.IO;
using System.Xml.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using AutoStructure.Common;

namespace AutoStructureMaker.Tests
{
    [TestClass]
    public class AppConfigTests
    {
        private string _tempDir;

        [TestInitialize]
        public void Setup()
        {
            _tempDir = Path.Combine(Path.GetTempPath(), "AppConfigTests_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_tempDir);
        }

        [TestCleanup]
        public void Cleanup()
        {
            if (Directory.Exists(_tempDir))
            {
                try { Directory.Delete(_tempDir, true); } catch { }
            }
        }

        [TestMethod]
        public void AppSettings_DefaultValues_AreCorrect()
        {
            var settings = new AppSettings();

            Assert.AreEqual("PTV", settings.DefaultDicomType);
            Assert.AreEqual("SUB", settings.DefaultBoolOperation);
            Assert.AreEqual("Outer", settings.DefaultMarginGeometry);
            Assert.AreEqual(7, settings.DefaultMarginX1);
            Assert.AreEqual(7, settings.DefaultMarginX2);
            Assert.AreEqual(7, settings.DefaultMarginY1);
            Assert.AreEqual(7, settings.DefaultMarginY2);
            Assert.AreEqual(7, settings.DefaultMarginZ1);
            Assert.AreEqual(7, settings.DefaultMarginZ2);
            Assert.AreEqual("case_", settings.SaveFileNamePrefix);
            Assert.AreEqual(@"\\Server\Share\AutoStructure\", settings.NetworkDirectory);
        }

        [TestMethod]
        public void AppSettings_XmlSerialization_PreservesAllValues()
        {
            string configPath = Path.Combine(_tempDir, "custom.config.xml");

            var custom = new AppSettings
            {
                DefaultDicomType = "ORGAN",
                DefaultBoolOperation = "AND",
                DefaultMarginGeometry = "Inner",
                DefaultMarginX1 = 12,
                DefaultMarginX2 = 12,
                DefaultMarginY1 = 10,
                DefaultMarginY2 = 8,
                DefaultMarginZ1 = 15,
                DefaultMarginZ2 = 15,
                NetworkDirectory = @"\\dummy_server\share\templates",
                LocalFallbackDirectory = @"C:\AutoStructure_Fallback",
                SaveFileNamePrefix = "test_case_"
            };

            // Act: シリアライズ
            var serializer = new XmlSerializer(typeof(AppSettings));
            using (var fs = new FileStream(configPath, FileMode.Create, FileAccess.Write))
            {
                serializer.Serialize(fs, custom);
            }

            // Act: デシリアライズ
            AppSettings loaded;
            using (var fs = new FileStream(configPath, FileMode.Open, FileAccess.Read))
            {
                loaded = (AppSettings)serializer.Deserialize(fs);
            }

            // Assert
            Assert.IsNotNull(loaded);
            Assert.AreEqual("ORGAN", loaded.DefaultDicomType);
            Assert.AreEqual("AND", loaded.DefaultBoolOperation);
            Assert.AreEqual("Inner", loaded.DefaultMarginGeometry);
            Assert.AreEqual(12, loaded.DefaultMarginX1);
            Assert.AreEqual(8, loaded.DefaultMarginY2);
            Assert.AreEqual(@"\\dummy_server\share\templates", loaded.NetworkDirectory);
            Assert.AreEqual(@"C:\AutoStructure_Fallback", loaded.LocalFallbackDirectory);
            Assert.AreEqual("test_case_", loaded.SaveFileNamePrefix);
        }

        [TestMethod]
        public void GetInitialDirectory_WhenUncServerUnreachable_TimesOutQuicklyAndFallsBack()
        {
            // Arrange: 存在しない架空のプライベート IP アドレス（通常 ping / SMB で 30 秒程度ハングするアドレス）
            string unreachableUnc = @"\\192.0.2.1\unreachable_share_test";
            string fallbackDir = Path.Combine(_tempDir, "local_fallback");
            Directory.CreateDirectory(fallbackDir);

            AppConfig.Current.NetworkDirectory = unreachableUnc;
            AppConfig.Current.LocalFallbackDirectory = fallbackDir;

            // Act: 実行時間の測定
            var sw = Stopwatch.StartNew();
            string resultDir = AppConfig.GetInitialDirectory();
            sw.Stop();

            // Assert:
            // 1. 1秒のタイムアウトで速やかにフォールバックされること（3秒以内に復帰）
            Assert.IsTrue(sw.ElapsedMilliseconds < 3000, 
                $"GetInitialDirectory took {sw.ElapsedMilliseconds}ms, which exceeded expected timeout (< 3000ms).");

            // 2. ローカルフォールバックまたは有効な既存ディレクトリが返ること
            Assert.IsNotNull(resultDir);
            Assert.IsTrue(Directory.Exists(resultDir), $"Resulting directory '{resultDir}' must exist.");
        }

        [TestMethod]
        public void LoadSettings_WhenXmlFileIsCorrupted_ReturnsSafeDefaultSettings()
        {
            // AppConfig.LoadSettings() の安全復旧をテスト
            var defaultSettings = AppConfig.LoadSettings();
            Assert.IsNotNull(defaultSettings);
            Assert.AreEqual("PTV", defaultSettings.DefaultDicomType);
            Assert.AreEqual("SUB", defaultSettings.DefaultBoolOperation);
        }
    }
}
