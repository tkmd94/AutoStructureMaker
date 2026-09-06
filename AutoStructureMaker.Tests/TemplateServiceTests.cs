using System;
using System.IO;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using AutoStructure.Common;

namespace AutoStructureMaker.Tests
{
    [TestClass]
    public class TemplateServiceTests
    {
        private string _tempDir;

        [TestInitialize]
        public void Setup()
        {
            _tempDir = Path.Combine(Path.GetTempPath(), "AutoStructureTests_" + Guid.NewGuid().ToString("N"));
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
        public void SaveAndLoad_XmlTemplate_RoundtripSuccess()
        {
            // Arrange
            var template = new StructureTemplate
            {
                ProtocolName = "Test_IMRT_Protocol",
                Author = "Physicist_Test",
                Description = "Unit test protocol"
            };
            template.Steps.Add(new TemplateStep { StepNumber = 1, Type = "Add", TargetStructure = "PTV_High", DicomType = "PTV" });
            template.Steps.Add(new TemplateStep { StepNumber = 2, Type = "Del", TargetStructure = "Old_Contour" });
            template.Steps.Add(new TemplateStep { StepNumber = 3, Type = "Boolean", TargetStructure = "Bladder_sub", BooleanOperation = "SUB", StructureA = "Bladder", StructureB = "PTV_High" });
            template.Steps.Add(new TemplateStep
            {
                StepNumber = 4,
                Type = "Margin",
                TargetStructure = "PTV_High",
                OrigStructure = "CTV",
                MarginGeometry = "Outer",
                Margins = new MarginValues { X1 = 5, X2 = 5, Y1 = 5, Y2 = 3, Z1 = 5, Z2 = 5 }
            });
            template.Steps.Add(new TemplateStep { StepNumber = 5, Type = "ConvertHighRes", TargetStructure = "PTV_High" });

            string xmlPath = Path.Combine(_tempDir, "protocol.xml");

            // Act
            TemplateService.SaveTemplate(xmlPath, template, asCsv: false);
            var loaded = TemplateService.LoadTemplate(xmlPath);

            // Assert
            Assert.IsNotNull(loaded);
            Assert.AreEqual("Test_IMRT_Protocol", loaded.ProtocolName);
            Assert.AreEqual("Physicist_Test", loaded.Author);
            Assert.AreEqual(5, loaded.Steps.Count);

            Assert.AreEqual("Add", loaded.Steps[0].Type);
            Assert.AreEqual("PTV_High", loaded.Steps[0].TargetStructure);
            Assert.AreEqual("PTV", loaded.Steps[0].DicomType);

            Assert.AreEqual("Margin", loaded.Steps[3].Type);
            Assert.AreEqual(3, loaded.Steps[3].Margins.Y2);
        }

        [TestMethod]
        public void LoadTemplate_AutoDetectsCsv_AndParsesCorrectly()
        {
            // Arrange: レガシー CSV 形式の作成
            string csvPath = Path.Combine(_tempDir, "legacy.csv");
            string content = "AddDelControl,Add,PTV,PTV\r\n" +
                             "AddMarginControl,Asymmetry,PTV,CTV,Outer,5,5,5,5,5,5\r\n" +
                             "BoolOpControl,SUB,Bladder_sub,Bladder,PTV\r\n" +
                             "ConvertHighResControl,HiRes,PTV\r\n" +
                             "AddDelControl,Del,Temp_Contour\r\n";
            File.WriteAllText(csvPath, content, Encoding.UTF8);

            // Act: LoadTemplate (拡張子 .csv を自動判別)
            var loaded = TemplateService.LoadTemplate(csvPath);

            // Assert
            Assert.IsNotNull(loaded);
            Assert.AreEqual(5, loaded.Steps.Count);
            Assert.AreEqual("Add", loaded.Steps[0].Type);
            Assert.AreEqual("Margin", loaded.Steps[1].Type);
            Assert.AreEqual("Boolean", loaded.Steps[2].Type);
            Assert.AreEqual("ConvertHighRes", loaded.Steps[3].Type);
            Assert.AreEqual("Del", loaded.Steps[4].Type);
        }

        [TestMethod]
        public void LoadTemplate_WithMalformedCsvRows_SkipsInvalidRowsSafely()
        {
            // Arrange: 壊れた行や空行、未知の操作を含む CSV
            string csvPath = Path.Combine(_tempDir, "corrupted.csv");
            string content = "\r\n" +                                  // 空行
                             "UnknownOpControl,Foo,Bar\r\n" +          // 未知の操作
                             "AddDelControl\r\n" +                     // 列不足
                             "AddDelControl,Add,ValidStructure,PTV\r\n" + // 正しい行
                             "   \r\n";                                // 空白行
            File.WriteAllText(csvPath, content, Encoding.UTF8);

            // Act
            var loaded = TemplateService.LoadTemplate(csvPath);

            // Assert: クラッシュせず、正しい 1 行のみが抽出されること
            Assert.IsNotNull(loaded);
            Assert.AreEqual(1, loaded.Steps.Count);
            Assert.AreEqual("Add", loaded.Steps[0].Type);
            Assert.AreEqual("ValidStructure", loaded.Steps[0].TargetStructure);
        }

        [TestMethod]
        [ExpectedException(typeof(FileNotFoundException))]
        public void LoadTemplate_WhenFileDoesNotExist_ThrowsFileNotFoundException()
        {
            string nonExistentPath = Path.Combine(_tempDir, "non_existent.xml");
            TemplateService.LoadTemplate(nonExistentPath);
        }

        [TestMethod]
        [ExpectedException(typeof(System.InvalidOperationException))]
        public void LoadTemplate_CorruptedXml_ThrowsInvalidOperationException()
        {
            // Arrange: 閉じタグが壊れた XML
            string brokenXmlPath = Path.Combine(_tempDir, "broken.xml");
            File.WriteAllText(brokenXmlPath, "<StructureTemplate><ProtocolName>Test</StructureTemplate>"); // 構文不正

            // Act
            TemplateService.LoadTemplate(brokenXmlPath);
        }

        [TestMethod]
        public void LoadTemplate_CsvWithExtraWhitespace_TrimsAndParsesCorrectly()
        {
            // Arrange: カンマの周りに空白が入っているレガシー CSV
            string csvPath = Path.Combine(_tempDir, "spaced.csv");
            string content = " AddDelControl , Add , PTV_Spaced , PTV \r\n" +
                             " BoolOpControl , SUB , Target_Out , StrA , StrB \r\n";
            File.WriteAllText(csvPath, content, Encoding.UTF8);

            // Act
            var loaded = TemplateService.LoadTemplate(csvPath);

            // Assert
            Assert.IsNotNull(loaded);
            Assert.AreEqual(2, loaded.Steps.Count);
            Assert.AreEqual("Add", loaded.Steps[0].Type);
            Assert.AreEqual("PTV_Spaced", loaded.Steps[0].TargetStructure);
            Assert.AreEqual("Target_Out", loaded.Steps[1].TargetStructure);
            Assert.AreEqual("StrA", loaded.Steps[1].StructureA);
            Assert.AreEqual("StrB", loaded.Steps[1].StructureB);
        }

        [TestMethod]
        public void SaveAndLoad_XmlTemplate_WithDisabledStep_PreservesEnabledAttribute()
        {
            // Arrange
            var template = new StructureTemplate { ProtocolName = "Toggle_Test" };
            template.Steps.Add(new TemplateStep { StepNumber = 1, Type = "Add", TargetStructure = "Active", Enabled = true });
            template.Steps.Add(new TemplateStep { StepNumber = 2, Type = "Add", TargetStructure = "Inactive", Enabled = false });

            string xmlPath = Path.Combine(_tempDir, "toggle.xml");

            // Act
            TemplateService.SaveTemplate(xmlPath, template, asCsv: false);
            var loaded = TemplateService.LoadTemplate(xmlPath);

            // Assert
            Assert.IsNotNull(loaded);
            Assert.AreEqual(2, loaded.Steps.Count);
            Assert.IsTrue(loaded.Steps[0].Enabled);
            Assert.IsFalse(loaded.Steps[1].Enabled);
        }
    }
}
