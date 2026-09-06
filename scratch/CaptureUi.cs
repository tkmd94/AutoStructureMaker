using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using AutoStructure;
using AutoStructure.Common;
using AutoStructure.ViewModels;

namespace UiCapture
{
    public class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            try
            {
                var app = new Application();

                // Theme.xaml をマージ
                var themeDict = new ResourceDictionary
                {
                    Source = new Uri("pack://application:,,,/AutoStructureMaker.esapi;component/Theme.xaml", UriKind.Absolute)
                };
                app.Resources.MergedDictionaries.Add(themeDict);

                var control = new MainControl();
                var vm = control.ViewModel;

                // 臨床的サンプルデータを設定
                vm.ProtocolName = "Prostate_VMAT_78Gy";
                vm.UserId = "Physicist_T";

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

                // Step 4: Boolean (⚡ Auto-Align 発動例: Bladder[STD] - PTV_78Gy[HIGH])
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

                // Step 5: 無効化されたステップのサンプル (IsEnabled = false)
                var op5 = new OperationStepViewModel
                {
                    StepNumber = 5,
                    Category = OperationCategory.AddStructure,
                    TargetStructure = "Temp_Ring_Helper",
                    DicomType = DicomType.CONTROL,
                    IsEnabled = false
                };
                op5.SetStatusSkipped();

                // 初期患者輪郭（Base Structures）を設定
                vm.SetBaseStructures(new[]
                {
                    new StructureInfo("Bladder", false, "ORGAN"),
                    new StructureInfo("Rectum", false, "ORGAN"),
                    new StructureInfo("CTV_Prostate", false, "CTV")
                });

                vm.Operations.Clear();
                vm.Operations.Add(op1);
                vm.Operations.Add(op2);
                vm.Operations.Add(op3);
                vm.Operations.Add(op4);
                vm.Operations.Add(op5);

                // 各ステップの輪郭伝播・解像度マップを本番同様に完全同期
                vm.RefreshStepStructureContexts();

                vm.LogText = "(INIT) AutoStructureMaker v2.0 (Write-Access ESAPI Plugin) Initialized.\r\n" +
                             "(TEMPLATE) Loaded protocol: Prostate_VMAT_78Gy (5 steps, 1 disabled).\r\n" +
                             "(VALIDATE) Pre-Flight Validator: Check passed with 0 Errors, 0 Warnings.\r\n" +
                             "(LOG-Add) [Step 1] Created empty structure: PTV_78Gy [Type: PTV] -> Success.\r\n" +
                             "(LOG-Margin) [Step 2] Applied 3D anisotropic margin to CTV_Prostate -> PTV_78Gy (R:5, L:5, A:5, P:3, I:5, S:5 mm) -> Done.\r\n" +
                             "(LOG-HiRes) [Step 3] Converted PTV_78Gy to High-Resolution (512x512) -> Done.\r\n" +
                             "(READY) [Step 4] Auto-Align active: Bladder[STD] and PTV_78Gy[HIGH] will be unified to High-Res.";

                var window = new Window
                {
                    Title = "AutoStructureMaker v2.0 - Varian Eclipse (ESAPI v16.1 / v15.6)",
                    Content = control,
                    Width = 1020,
                    Height = 880,
                    WindowStartupLocation = WindowStartupLocation.CenterScreen,
                    Background = (Brush)themeDict["Theme.Background.Main"]
                };

                window.Show();

                // レイアウト更新
                control.UpdateLayout();

                int width = 1020;
                int height = 880;

                var rtb = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
                rtb.Render(window);

                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(rtb));

                string outDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Screenshots");
                Directory.CreateDirectory(outDir);
                string outPath = Path.Combine(outDir, "AutoStructureMaker_UI_Sample.png");

                using (var fs = File.OpenWrite(outPath))
                {
                    encoder.Save(fs);
                }

                Console.WriteLine("SUCCESS: Screenshot saved to: " + outPath);
                window.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR: " + ex);
            }
        }
    }
}
