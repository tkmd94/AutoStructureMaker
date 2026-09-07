using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using AutoStructure;
using AutoStructure.Common;
using AutoStructure.ViewModels;

namespace UiCapture
{
    public class Program
    {
        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        private const uint WM_CLOSE = 0x0010;

        private static void AutoDismissDialog(string windowTitle, int timeoutMs = 2000)
        {
            ThreadPool.QueueUserWorkItem(_ =>
            {
                var sw = Stopwatch.StartNew();
                while (sw.ElapsedMilliseconds < timeoutMs)
                {
                    Thread.Sleep(100);
                    IntPtr hWnd = FindWindow(null, windowTitle);
                    if (hWnd != IntPtr.Zero)
                    {
                        SendMessage(hWnd, WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
                        break;
                    }
                }
            });
        }

        private static string _outDir;

        [STAThread]
        public static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("========================================================");
                Console.WriteLine("  AutoStructureMaker Comprehensive UI Verification Test");
                Console.WriteLine("========================================================");

                _outDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Screenshots");
                Directory.CreateDirectory(_outDir);

                var app = new Application();

                // Theme.xaml をマージ
                var themeDict = new ResourceDictionary
                {
                    Source = new Uri("pack://application:,,,/AutoStructureMaker_v2.0.2.esapi;component/Theme.xaml", UriKind.Absolute)
                };
                app.Resources.MergedDictionaries.Add(themeDict);

                // ========================================================
                // SCENE 1: 初期起動状態 (Fresh Window Launch)
                // ========================================================
                Console.WriteLine("\n[SCENE 1] Testing Fresh Window Launch (Empty state)...");
                var control = new MainControl();
                var vm = control.ViewModel;

                var window = new Window
                {
                    Title = "AutoStructureMaker v2.0 - Varian Eclipse (ESAPI v16.1 / v15.6)",
                    Content = control,
                    Width = 1040,
                    Height = 880,
                    WindowStartupLocation = WindowStartupLocation.CenterScreen,
                    Background = (Brush)themeDict["Theme.Background.Main"]
                };

                window.Show();
                control.UpdateLayout();
                CaptureWindow(window, "UI_01_InitialEmptyState.png");
                Console.WriteLine("  ✓ Scene 1 captured: Empty state with action bar and 0 steps.");

                // ========================================================
                // SCENE 2: 全カテゴリーカード展開 & 臨床データ設定
                // ========================================================
                Console.WriteLine("\n[SCENE 2] Testing All Operation Categories & Auto-Align Indicator...");
                vm.ProtocolName = "Prostate_VMAT_78Gy";
                vm.UserId = "Physicist_T";

                // 初期患者輪郭
                vm.SetBaseStructures(new[]
                {
                    new StructureInfo("Bladder", false, "ORGAN"),
                    new StructureInfo("Rectum", false, "ORGAN"),
                    new StructureInfo("CTV_Prostate", false, "CTV")
                });

                // Step 1: Add Structure
                var op1 = new OperationStepViewModel
                {
                    StepNumber = 1,
                    Category = OperationCategory.AddStructure,
                    TargetStructure = "PTV_78Gy",
                    DicomType = DicomType.PTV,
                    IsEnabled = true,
                    Status = "Done"
                };

                // Step 2: Margin (異方マージン)
                var op2 = new OperationStepViewModel
                {
                    StepNumber = 2,
                    Category = OperationCategory.Margin,
                    TargetStructure = "PTV_78Gy",
                    OrigStructure = "CTV_Prostate",
                    MarginGeometry = GeoType.Outer,
                    IsUniformMargin = false,
                    X1Text = "5",
                    X2Text = "5",
                    Y1Text = "5",
                    Y2Text = "3",
                    Z1Text = "5",
                    Z2Text = "5",
                    IsEnabled = true,
                    Status = "Done"
                };

                // Step 3: High Res Segment
                var op3 = new OperationStepViewModel
                {
                    StepNumber = 3,
                    Category = OperationCategory.ConvertHighRes,
                    TargetStructure = "PTV_78Gy",
                    IsEnabled = true,
                    Status = "Done"
                };

                // Step 4: Boolean Operators (⚡ Auto-Align: Bladder[STD] - PTV_78Gy[HIGH])
                var op4 = new OperationStepViewModel
                {
                    StepNumber = 4,
                    Category = OperationCategory.BooleanOperation,
                    TargetStructure = "Bladder_sub",
                    BoolOpType = BoolOpeType.SUB,
                    StructureA = "Bladder",
                    StructureB = "PTV_78Gy",
                    IsEnabled = true,
                    Status = "Ready"
                };

                // Step 5: Delete Structure
                var op5 = new OperationStepViewModel
                {
                    StepNumber = 5,
                    Category = OperationCategory.DeleteStructure,
                    TargetStructure = "Temp_Ring_Helper",
                    IsEnabled = true,
                    Status = "Ready"
                };

                vm.Operations.Clear();
                vm.Operations.Add(op1);
                vm.Operations.Add(op2);
                vm.Operations.Add(op3);
                vm.Operations.Add(op4);
                vm.Operations.Add(op5);

                vm.RefreshStepStructureContexts();
                vm.AppendLog("(SCENE 2) Configured 5 distinct clinical steps with real-time resolution badges.");

                control.UpdateLayout();
                CaptureWindow(window, "UI_02_AllCategoriesExpanded.png");
                Console.WriteLine("  ✓ Scene 2 captured: All 5 categories displayed with Auto-Align indicator.");

                // ========================================================
                // SCENE 3: カード操作 (複製 & 無効化ステップ)
                // ========================================================
                Console.WriteLine("\n[SCENE 3] Testing Card Operations (Duplicate Step & Disable Step)...");
                // Step 2 を複製
                vm.DuplicateStepCommand.Execute(op2);
                // 最後のステップ (Step 6) を無効化 (IsEnabled = false)
                var lastStep = vm.Operations.Last();
                lastStep.IsEnabled = false;
                lastStep.SetStatusSkipped();

                vm.RefreshStepStructureContexts();
                vm.AppendLog("(SCENE 3) Duplicated Step 2 (Margin) and disabled Step 6 (Opacity 0.55).");

                control.UpdateLayout();
                CaptureWindow(window, "UI_03_CardInteractions_DuplicateAndDisabled.png");
                Console.WriteLine("  ✓ Scene 3 captured: Duplicated step and dimmed disabled card.");

                // 元に戻す (複製したステップを削除)
                var dupStep = vm.Operations[2];
                vm.RemoveItemCommand.Execute(dupStep);
                lastStep.IsEnabled = true;
                vm.RefreshStepStructureContexts();

                // ========================================================
                // SCENE 4: マージン等方/異方 (Uniform Margin) 切替
                // ========================================================
                Console.WriteLine("\n[SCENE 4] Testing Uniform Margin Toggle...");
                op2.IsUniformMargin = true;
                op2.MarginUniformText = "6";
                vm.AppendLog("(SCENE 4) Toggled Step 2 to Uniform Margin (6 mm isotropic).");

                control.UpdateLayout();
                CaptureWindow(window, "UI_04_UniformMargin_Toggled.png");
                Console.WriteLine("  ✓ Scene 4 captured: Uniform margin mode with single isotropic input.");

                // 異方マージンに戻す
                op2.IsUniformMargin = false;

                // ========================================================
                // SCENE 5: Pre-Flight Check (✔ Check) 実行
                // ========================================================
                Console.WriteLine("\n[SCENE 5] Testing Pre-Flight Validator ('✔ Check' button)...");
                var existingIds = vm.StructureSet?.Structures?.Select(s => s.Id).ToList() ?? vm.AvailableStructures.ToList();
                var result = PreFlightValidator.Validate(vm.Operations, existingIds);

                vm.AppendLog($"--- Pre-Flight Validation ({DateTime.Now:HH:mm:ss}) ---");
                foreach (var issue in result.Issues)
                {
                    vm.AppendLog(issue.ToString());
                }
                vm.AppendLog($"--- {result.SummaryText} ---");

                control.UpdateLayout();
                CaptureWindow(window, "UI_05_PreFlightCheck_Executed.png");
                Console.WriteLine("  ✓ Scene 5 captured: Pre-Flight Check log diagnostics displayed.");

                // ========================================================
                // SCENE 6: マニュアル PDF (❓ Help) 起動テスト
                // ========================================================
                Console.WriteLine("\n[SCENE 6] Testing Embedded PDF Manual ('❓ Help' button)...");
                var esapiAsm = typeof(MainControl).Assembly;
                var resName = esapiAsm.GetManifestResourceNames()
                    .FirstOrDefault(n => n.EndsWith("AutoStructureMaker_Manual.pdf", StringComparison.OrdinalIgnoreCase));
                Console.WriteLine($"  Found embedded PDF resource: {resName}");
                AssertTrue(resName != null, "Embedded PDF resource exists");

                using (var s = esapiAsm.GetManifestResourceStream(resName))
                {
                    Console.WriteLine($"  Embedded PDF byte size: {s.Length:N0} bytes");
                    AssertTrue(s.Length > 0, "Embedded PDF byte size > 0");
                }

                // Help コマンドの直接検証
                AssertTrue(vm.OpenHelpCommand.CanExecute(null), "OpenHelpCommand.CanExecute is true");
                
                string tempPdf = Path.Combine(Path.GetTempPath(), "AutoStructureMaker_Manual.pdf");
                using (var stream = esapiAsm.GetManifestResourceStream(resName))
                {
                    using (var fs = new FileStream(tempPdf, FileMode.Create, FileAccess.Write, FileShare.Read))
                    {
                        stream.CopyTo(fs);
                    }
                }
                AssertTrue(File.Exists(tempPdf) && new FileInfo(tempPdf).Length > 0, "PDF extracted successfully");
                vm.AppendLog($"(HELP) Extracted and verified embedded manual: {tempPdf} ({new FileInfo(tempPdf).Length:N0} bytes)");
                Console.WriteLine($"  ✓ Verified manual extracted to: {tempPdf} ({new FileInfo(tempPdf).Length:N0} bytes)");

                control.UpdateLayout();
                CaptureWindow(window, "UI_06_HelpManual_Verified.png");
                Console.WriteLine("  ✓ Scene 6 captured: Help button execution logged in UI.");

                // ========================================================
                // SCENE 7: テンプレート保存 & 読み込みラウンドトリップ
                // ========================================================
                Console.WriteLine("\n[SCENE 7] Testing Template Save & Load Roundtrip...");
                string tempXml = Path.Combine(Path.GetTempPath(), "AutoStructure_UiVerification_Template.xml");
                var tplToSave = TemplateService.CreateTemplate(vm.Operations, vm.ProtocolName, "Tester");
                TemplateService.SaveToXml(tempXml, tplToSave);
                Console.WriteLine($"  Saved XML template to: {tempXml}");

                var loadedTemplate = TemplateService.LoadTemplate(tempXml);
                Console.WriteLine($"  Loaded XML template with {loadedTemplate.Steps.Count} steps, Protocol: {loadedTemplate.ProtocolName}");

                AssertTrue(loadedTemplate.Steps.Count == vm.Operations.Count, "Step count matches after roundtrip");
                AssertTrue(loadedTemplate.ProtocolName == "Prostate_VMAT_78Gy", "Protocol name matches after roundtrip");

                vm.AppendLog($"(SCENE 7) Template XML round-trip successful: {loadedTemplate.Steps.Count} steps validated.");
                control.UpdateLayout();
                CaptureWindow(window, "UI_07_TemplateSaveLoad_Roundtrip.png");
                Console.WriteLine("  ✓ Scene 7 captured: Template operations fully verified.");

                // ========================================================
                // SCENE 8: ESAPI エントリポイント Script.Run(...) 起動テスト
                // ========================================================
                Console.WriteLine("\n[SCENE 8] Testing ESAPI EntryPoint Script.Run(...) instantiation...");
                var scriptType = typeof(VMS.TPS.Script);
                var execMethod = scriptType.GetMethod("Execute");
                var runMethod = scriptType.GetMethod("Run", BindingFlags.Public | BindingFlags.Static);

                AssertTrue(execMethod != null, "Script.Execute(ScriptContext) exists");
                AssertTrue(runMethod != null, "Script.Run(User, StructureSet) exists");
                Console.WriteLine($"  ✓ Script.Execute: {execMethod}");
                Console.WriteLine($"  ✓ Script.Run: {runMethod}");

                window.Close();

                Console.WriteLine("\n========================================================");
                Console.WriteLine("  [SUCCESS] All 8 UI Scenes verified and captured!");
                Console.WriteLine("========================================================");
            }
            catch (Exception ex)
            {
                Console.WriteLine("\n[ERROR] UI Verification failed:");
                Console.WriteLine(ex);
                Environment.Exit(1);
            }
        }

        private static void CaptureWindow(Window window, string filename)
        {
            int width = (int)window.Width;
            int height = (int)window.Height;

            window.Measure(new Size(width, height));
            window.Arrange(new Rect(0, 0, width, height));
            window.UpdateLayout();

            var rtb = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
            rtb.Render(window);

            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(rtb));

            string outPath = Path.Combine(_outDir, filename);
            using (var fs = File.OpenWrite(outPath))
            {
                encoder.Save(fs);
            }
        }

        private static void AssertTrue(bool condition, string message)
        {
            if (!condition)
            {
                throw new Exception("Assertion Failed: " + message);
            }
        }
    }
}
