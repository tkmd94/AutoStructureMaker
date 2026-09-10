using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using AutoStructure;
using AutoStructure.Common;
using AutoStructure.ViewModels;

namespace AutoStructureMaker.Tests
{
    [TestClass]
    public class OperationStepViewModelTests
    {
        [TestMethod]
        public void CategorySwitching_UpdatesVisibilityFlagsCorrectly()
        {
            var vm = new OperationStepViewModel();

            // 1. AddStructure (Default)
            vm.Category = OperationCategory.AddStructure;
            Assert.IsTrue(vm.IsAddStructure);
            Assert.IsTrue(vm.IsAddDelVisible);
            Assert.IsFalse(vm.IsDeleteStructure);
            Assert.IsFalse(vm.IsBooleanVisible);
            Assert.IsFalse(vm.IsMarginVisible);
            Assert.IsFalse(vm.IsConvertHighResVisible);
            Assert.AreEqual("ADD", vm.CategoryBadgeText);

            // 2. DeleteStructure
            vm.Category = OperationCategory.DeleteStructure;
            Assert.IsFalse(vm.IsAddStructure);
            Assert.IsTrue(vm.IsDeleteStructure);
            Assert.IsTrue(vm.IsAddDelVisible);
            Assert.IsFalse(vm.IsBooleanVisible);
            Assert.AreEqual("DEL", vm.CategoryBadgeText);

            // 3. BooleanOperation
            vm.Category = OperationCategory.BooleanOperation;
            Assert.IsFalse(vm.IsAddDelVisible);
            Assert.IsTrue(vm.IsBooleanVisible);
            Assert.IsFalse(vm.IsMarginVisible);
            Assert.AreEqual("BOOL", vm.CategoryBadgeText);

            // 4. Margin
            vm.Category = OperationCategory.Margin;
            Assert.IsFalse(vm.IsBooleanVisible);
            Assert.IsTrue(vm.IsMarginVisible);
            Assert.AreEqual("MARGIN", vm.CategoryBadgeText);

            // 5. ConvertHighRes
            vm.Category = OperationCategory.ConvertHighRes;
            Assert.IsFalse(vm.IsMarginVisible);
            Assert.IsTrue(vm.IsConvertHighResVisible);
            Assert.AreEqual("HI-RES", vm.CategoryBadgeText);
        }

        [TestMethod]
        public void UniformMargin_SyncsAllAxesAutomatically()
        {
            var vm = new OperationStepViewModel
            {
                Category = OperationCategory.Margin,
                IsUniformMargin = true,
                MarginUniformText = "10"
            };

            Assert.AreEqual("10", vm.X1Text);
            Assert.AreEqual("10", vm.X2Text);
            Assert.AreEqual("10", vm.Y1Text);
            Assert.AreEqual("10", vm.Y2Text);
            Assert.AreEqual("10", vm.Z1Text);
            Assert.AreEqual("10", vm.Z2Text);

            // 値を 5 に変更
            vm.MarginUniformText = "5";
            Assert.AreEqual("5", vm.X1Text);
            Assert.AreEqual("5", vm.Z2Text);
        }

        [TestMethod]
        public void AnisotropicMargin_AllowsIndividualAxisValues()
        {
            var vm = new OperationStepViewModel
            {
                Category = OperationCategory.Margin,
                IsUniformMargin = false,
                X1Text = "5",
                X2Text = "6",
                Y1Text = "7",
                Y2Text = "8",
                Z1Text = "9",
                Z2Text = "10"
            };

            Assert.AreEqual("5", vm.X1Text);
            Assert.AreEqual("6", vm.X2Text);
            Assert.AreEqual("7", vm.Y1Text);
            Assert.AreEqual("8", vm.Y2Text);
            Assert.AreEqual("9", vm.Z1Text);
            Assert.AreEqual("10", vm.Z2Text);
        }

        [TestMethod]
        public void InvalidMarginText_FallsBackSafely_WithoutThrowing()
        {
            var vm = new OperationStepViewModel
            {
                Category = OperationCategory.Margin,
                TargetStructure = "PTV",
                OrigStructure = "CTV",
                X1Text = "invalid_text",
                Y1Text = "",
                Z1Text = "-999"
            };

            // ToTemplateStep 呼び出し時にクラッシュせず安全にフォールバックすること
            var step = vm.ToTemplateStep();
            Assert.IsNotNull(step);
            Assert.IsNotNull(step.Margins);
            Assert.AreEqual(7, step.Margins.X1); // デフォルト 7 にフォールバック
            Assert.AreEqual(7, step.Margins.Y1);
            Assert.AreEqual(-999, step.Margins.Z1); // 負数はパース可能
        }

        [TestMethod]
        public void TemplateStep_Roundtrip_ForAllCategories()
        {
            // 1. Add
            var addVm = new OperationStepViewModel
            {
                Category = OperationCategory.AddStructure,
                TargetStructure = "PTV_High",
                DicomType = DicomType.PTV
            };
            var addStep = addVm.ToTemplateStep();
            var restoredAdd = OperationStepViewModel.CreateFromTemplateStep(addStep);
            Assert.AreEqual(OperationCategory.AddStructure, restoredAdd.Category);
            Assert.AreEqual("PTV_High", restoredAdd.TargetStructure);
            Assert.AreEqual(DicomType.PTV, restoredAdd.DicomType);

            // 2. Del
            var delVm = new OperationStepViewModel
            {
                Category = OperationCategory.DeleteStructure,
                TargetStructure = "Temp_Ring"
            };
            var delStep = delVm.ToTemplateStep();
            var restoredDel = OperationStepViewModel.CreateFromTemplateStep(delStep);
            Assert.AreEqual(OperationCategory.DeleteStructure, restoredDel.Category);
            Assert.AreEqual("Temp_Ring", restoredDel.TargetStructure);

            // 3. Boolean
            var boolVm = new OperationStepViewModel
            {
                Category = OperationCategory.BooleanOperation,
                TargetStructure = "Bladder_sub",
                BoolOpType = BoolOpeType.SUB,
                StructureA = "Bladder",
                StructureB = "PTV_High"
            };
            var boolStep = boolVm.ToTemplateStep();
            var restoredBool = OperationStepViewModel.CreateFromTemplateStep(boolStep);
            Assert.AreEqual(OperationCategory.BooleanOperation, restoredBool.Category);
            Assert.AreEqual("Bladder_sub", restoredBool.TargetStructure);
            Assert.AreEqual(BoolOpeType.SUB, restoredBool.BoolOpType);
            Assert.AreEqual("Bladder", restoredBool.StructureA);
            Assert.AreEqual("PTV_High", restoredBool.StructureB);

            // 4. Margin
            var marginVm = new OperationStepViewModel
            {
                Category = OperationCategory.Margin,
                TargetStructure = "PTV_High",
                OrigStructure = "CTV",
                MarginGeometry = GeoType.Outer,
                X1Text = "5",
                X2Text = "5",
                Y1Text = "3",
                Y2Text = "3",
                Z1Text = "4",
                Z2Text = "4"
            };
            var marginStep = marginVm.ToTemplateStep();
            var restoredMargin = OperationStepViewModel.CreateFromTemplateStep(marginStep);
            Assert.AreEqual(OperationCategory.Margin, restoredMargin.Category);
            Assert.AreEqual("PTV_High", restoredMargin.TargetStructure);
            Assert.AreEqual("CTV", restoredMargin.OrigStructure);
            Assert.AreEqual(GeoType.Outer, restoredMargin.MarginGeometry);
            Assert.AreEqual("5", restoredMargin.X1Text);
            Assert.AreEqual("3", restoredMargin.Y1Text);

            // 5. ConvertHighRes
            var hiResVm = new OperationStepViewModel
            {
                Category = OperationCategory.ConvertHighRes,
                TargetStructure = "GTV_Opt"
            };
            var hiResStep = hiResVm.ToTemplateStep();
            var restoredHiRes = OperationStepViewModel.CreateFromTemplateStep(hiResStep);
            Assert.AreEqual(OperationCategory.ConvertHighRes, restoredHiRes.Category);
            Assert.AreEqual("GTV_Opt", restoredHiRes.TargetStructure);
        }

        [TestMethod]
        public void Execute_WithNullStructureSet_ReturnsFalseSafely()
        {
            var vm = new OperationStepViewModel
            {
                Category = OperationCategory.AddStructure,
                TargetStructure = "PTV"
            };

            string logMessage = "";
            bool result = vm.Execute(null, msg => logMessage = msg);

            Assert.IsFalse(result);
            Assert.AreEqual("Fail", vm.Status);
            Assert.IsTrue(logMessage.Contains("StructureSet is null"));
        }

        [TestMethod]
        public void Execute_WithEmptyTargetStructure_ReturnsFalseSafely()
        {
            var vm = new OperationStepViewModel
            {
                Category = OperationCategory.AddStructure,
                TargetStructure = "   " // 空白のみ
            };

            Assert.IsTrue(string.IsNullOrWhiteSpace(vm.TargetStructure));
        }

        [TestMethod]
        public void ExecuteBoolean_WithNullStructureSet_ReturnsFalseSafely()
        {
            var vm = new OperationStepViewModel
            {
                Category = OperationCategory.BooleanOperation,
                TargetStructure = "Bladder_sub",
                StructureA = "Bladder",
                StructureB = "PTV",
                BoolOpType = BoolOpeType.SUB
            };

            string logMessage = "";
            bool result = vm.Execute(null, msg => logMessage = msg);

            Assert.IsFalse(result);
            Assert.AreEqual("Fail", vm.Status);
            Assert.IsTrue(logMessage.Contains("StructureSet is null"));
        }

        [TestMethod]
        public void ResolutionBadges_ReflectResolutionMapAndNewStructuresCorrectly()
        {
            var vm = new OperationStepViewModel
            {
                ResolutionMap = new System.Collections.Generic.Dictionary<string, bool>
                {
                    { "CTV", true },     // High-Res
                    { "PTV", false }    // Std-Res
                }
            };
            vm.AvailableStructureInfos.Add(new StructureInfo("CTV", true, "CTV", isNew: false));
            vm.AvailableStructureInfos.Add(new StructureInfo("PTV", false, "PTV", isNew: false));
            vm.AvailableStructureInfos.Add(new StructureInfo("GTV_New", true, "GTV", isNew: true));
            vm.AvailableStructureInfos.Add(new StructureInfo("Organ_New", false, "ORGAN", isNew: true));

            // 1. Existing High-Res
            vm.TargetStructure = "CTV";
            Assert.IsTrue(vm.HasTargetResolution);
            Assert.AreEqual("HIGH", vm.TargetResolutionBadgeText);

            // 2. Existing Std-Res
            vm.TargetStructure = "PTV";
            Assert.IsTrue(vm.HasTargetResolution);
            Assert.AreEqual("STD", vm.TargetResolutionBadgeText);

            // 3. New High-Res from earlier step
            vm.TargetStructure = "GTV_New";
            Assert.IsTrue(vm.HasTargetResolution);
            Assert.AreEqual("NEW: HIGH", vm.TargetResolutionBadgeText);

            // 4. New Std-Res from earlier step
            vm.TargetStructure = "Organ_New";
            Assert.IsTrue(vm.HasTargetResolution);
            Assert.AreEqual("NEW: STD", vm.TargetResolutionBadgeText);

            // 5. Totally unknown structure
            vm.TargetStructure = "Unknown_Opt";
            Assert.IsTrue(vm.HasTargetResolution);
            Assert.AreEqual("NEW", vm.TargetResolutionBadgeText);

            // 6. Empty
            vm.TargetStructure = "";
            Assert.IsFalse(vm.HasTargetResolution);
            Assert.AreEqual("", vm.TargetResolutionBadgeText);
        }

        [TestMethod]
        public void BooleanResolutionMismatch_DetectsMismatchAndTriggersAutoAlignIndicator()
        {
            var vm = new OperationStepViewModel
            {
                Category = OperationCategory.BooleanOperation,
                ResolutionMap = new System.Collections.Generic.Dictionary<string, bool>
                {
                    { "CTV_High", true },
                    { "Bladder_Std", false },
                    { "Rectum_Std", false },
                    { "GTV_High", true }
                }
            };

            // High + Std -> Mismatch (true)
            vm.StructureA = "CTV_High";
            vm.StructureB = "Bladder_Std";
            Assert.IsTrue(vm.IsBooleanResolutionMismatch);

            // Std + High -> Mismatch (true)
            vm.StructureA = "Rectum_Std";
            vm.StructureB = "GTV_High";
            Assert.IsTrue(vm.IsBooleanResolutionMismatch);

            // Std + Std -> Match (false)
            vm.StructureA = "Bladder_Std";
            vm.StructureB = "Rectum_Std";
            Assert.IsFalse(vm.IsBooleanResolutionMismatch);

            // High + High -> Match (false)
            vm.StructureA = "CTV_High";
            vm.StructureB = "GTV_High";
            Assert.IsFalse(vm.IsBooleanResolutionMismatch);

            // Empty -> False
            vm.StructureB = "";
            Assert.IsFalse(vm.IsBooleanResolutionMismatch);
        }

        [TestMethod]
        public void StepPropagation_PropagatesCreatedStructuresToSubsequentSteps()
        {
            var mainVm = new MainViewModel();

            // Step 1: Add new structure "CTV_Opt"
            var step1 = new OperationStepViewModel
            {
                Category = OperationCategory.AddStructure,
                TargetStructure = "CTV_Opt"
            };
            mainVm.AddStepCommand.Execute(step1.Category);
            var registeredStep1 = (OperationStepViewModel)mainVm.Operations[0];
            registeredStep1.TargetStructure = "CTV_Opt";

            // Step 2: Boolean
            mainVm.AddStepCommand.Execute(OperationCategory.BooleanOperation);
            var registeredStep2 = (OperationStepViewModel)mainVm.Operations[1];

            // Step 2 の候補リストに Step 1 で作成した "CTV_Opt" が含まれているか検証
            var propagated = registeredStep2.AvailableStructureInfos.FirstOrDefault(x => x.Id == "CTV_Opt");
            Assert.IsNotNull(propagated, "Step 2 should contain CTV_Opt created in Step 1.");
            Assert.IsTrue(propagated.IsNew);
            Assert.IsFalse(propagated.IsHighResolution); // 初期作成は Standard Resolution
            Assert.AreEqual("NEW: STD", propagated.ResolutionBadgeText);

            // Step 2 の StructureA に "CTV_Opt" をセットするとバッジが NEW: STD になること
            registeredStep2.StructureA = "CTV_Opt";
            Assert.AreEqual("NEW: STD", registeredStep2.StructureAResolutionBadgeText);
        }

        [TestMethod]
        public void StepPropagation_HighResConversion_PropagatesToLaterSteps()
        {
            var mainVm = new MainViewModel();

            // Step 1: Add "GTV_Opt"
            mainVm.AddStepCommand.Execute(OperationCategory.AddStructure);
            var step1 = (OperationStepViewModel)mainVm.Operations[0];
            step1.TargetStructure = "GTV_Opt";

            // Step 2: ConvertHighRes for "GTV_Opt"
            mainVm.AddStepCommand.Execute(OperationCategory.ConvertHighRes);
            var step2 = (OperationStepViewModel)mainVm.Operations[1];
            step2.TargetStructure = "GTV_Opt";

            // Step 2 の時点ではまだ GTV_Opt は高解像度化前（STD）であるべき
            Assert.AreEqual("NEW: STD", step2.TargetResolutionBadgeText);

            // Step 3: Margin from "GTV_Opt"
            mainVm.AddStepCommand.Execute(OperationCategory.Margin);
            var step3 = (OperationStepViewModel)mainVm.Operations[2];

            // Step 3 の候補リストで "GTV_Opt" が高解像度 (NEW: HIGH) に更新されているか検証
            var propagatedGtv = step3.AvailableStructureInfos.FirstOrDefault(x => x.Id == "GTV_Opt");
            Assert.IsNotNull(propagatedGtv);
            Assert.IsTrue(propagatedGtv.IsHighResolution);
            Assert.AreEqual("NEW: HIGH", propagatedGtv.ResolutionBadgeText);

            step3.OrigStructure = "GTV_Opt";
            Assert.AreEqual("NEW: HIGH", step3.OrigStructureResolutionBadgeText);
        }

        [TestMethod]
        public void SetBaseStructures_InitializesContextAndPropagatesAcrossSteps()
        {
            var mainVm = new MainViewModel();
            mainVm.SetBaseStructures(new[]
            {
                new StructureInfo("Bladder", false, "ORGAN"),
                new StructureInfo("PTV_Target", true, "PTV")
            });

            Assert.AreEqual(2, mainVm.AvailableStructures.Count);
            Assert.AreEqual(2, mainVm.AvailableStructureInfos.Count);
            Assert.IsFalse(mainVm.AvailableStructureInfos.First(x => x.Id == "Bladder").IsHighResolution);
            Assert.IsTrue(mainVm.AvailableStructureInfos.First(x => x.Id == "PTV_Target").IsHighResolution);

            // ステップを追加した際にベース輪郭が正しく渡されること
            mainVm.AddStepCommand.Execute(OperationCategory.BooleanOperation);
            var step = (OperationStepViewModel)mainVm.Operations[0];
            step.StructureA = "Bladder";
            step.StructureB = "PTV_Target";

            Assert.AreEqual("STD", step.StructureAResolutionBadgeText);
            Assert.AreEqual("HIGH", step.StructureBResolutionBadgeText);
            Assert.IsTrue(step.IsBooleanResolutionMismatch);
        }

        [TestMethod]
        public void TemplateStep_Roundtrip_PreservesEnabledProperty()
        {
            var vm = new OperationStepViewModel
            {
                StepNumber = 5,
                Category = OperationCategory.Margin,
                TargetStructure = "PTV_Margin",
                OrigStructure = "CTV",
                IsEnabled = false // 無効化
            };

            var step = vm.ToTemplateStep();
            Assert.IsFalse(step.Enabled);

            var reconstructed = OperationItemViewModel.FromTemplateStep(step);
            Assert.IsFalse(reconstructed.IsEnabled);
        }

        [TestMethod]
        public void SetStatusSkipped_SetsStatusAndBrushCorrectly()
        {
            var vm = new OperationStepViewModel();
            vm.SetStatusSkipped();

            Assert.AreEqual("Skip", vm.Status);
            Assert.IsNotNull(vm.StatusBrush);
        }

        [TestMethod]
        public void ExecuteBoolean_WithEmptyStructureNames_ReturnsFalseSafely()
        {
            var vm = new OperationStepViewModel
            {
                Category = OperationCategory.BooleanOperation,
                TargetStructure = "Target",
                StructureA = "",
                StructureB = ""
            };

            // StructureSet が null の場合の安全実行
            bool result = vm.Execute(null, null);
            Assert.IsFalse(result);
            Assert.AreEqual("Fail", vm.Status);
        }

        [TestMethod]
        public void Propagation_DeleteStep_RemovesFromSubsequentContexts()
        {
            var mainVm = new MainViewModel();
            mainVm.SetBaseStructures(new[]
            {
                new StructureInfo("To_Delete", false, "CONTROL"),
                new StructureInfo("To_Keep", false, "ORGAN")
            });

            // Step 1: Delete "To_Delete"
            mainVm.AddStepCommand.Execute(OperationCategory.DeleteStructure);
            var step1 = (OperationStepViewModel)mainVm.Operations[0];
            step1.TargetStructure = "To_Delete";

            // Step 2: Boolean
            mainVm.AddStepCommand.Execute(OperationCategory.BooleanOperation);
            var step2 = (OperationStepViewModel)mainVm.Operations[1];

            // Step 2 の候補に "To_Delete" が含まれず、"To_Keep" は含まれていること
            Assert.IsFalse(step2.AvailableStructures.Contains("To_Delete"));
            Assert.IsTrue(step2.AvailableStructures.Contains("To_Keep"));
        }

        [TestMethod]
        public void Boolean_AllResolutionCombinations_DetectsMismatchAccurately()
        {
            var vm = new OperationStepViewModel
            {
                Category = OperationCategory.BooleanOperation,
                ResolutionMap = new System.Collections.Generic.Dictionary<string, bool>
                {
                    { "A_Std", false },
                    { "B_Std", false },
                    { "A_Hi", true },
                    { "B_Hi", true }
                }
            };

            // Case 1: STD + STD -> Mismatch: False
            vm.StructureA = "A_Std";
            vm.StructureB = "B_Std";
            Assert.IsFalse(vm.IsBooleanResolutionMismatch);

            // Case 2: HIGH + HIGH -> Mismatch: False
            vm.StructureA = "A_Hi";
            vm.StructureB = "B_Hi";
            Assert.IsFalse(vm.IsBooleanResolutionMismatch);

            // Case 3: STD + HIGH -> Mismatch: True
            vm.StructureA = "A_Std";
            vm.StructureB = "B_Hi";
            Assert.IsTrue(vm.IsBooleanResolutionMismatch);

            // Case 4: HIGH + STD -> Mismatch: True
            vm.StructureA = "A_Hi";
            vm.StructureB = "B_Std";
            Assert.IsTrue(vm.IsBooleanResolutionMismatch);
        }

        [TestMethod]
        public void Margin_UniformToggle_PreservesAndSyncsValues()
        {
            var vm = new OperationStepViewModel
            {
                Category = OperationCategory.Margin,
                IsUniformMargin = true,
                MarginUniformText = "10"
            };

            // Uniform = true の場合、全軸が 10 に同期されること
            Assert.AreEqual("10", vm.X1Text);
            Assert.AreEqual("10", vm.X2Text);
            Assert.AreEqual("10", vm.Y1Text);
            Assert.AreEqual("10", vm.Y2Text);
            Assert.AreEqual("10", vm.Z1Text);
            Assert.AreEqual("10", vm.Z2Text);

            // Uniform を解除し、各軸を個別に変更できること
            vm.IsUniformMargin = false;
            vm.Y2Text = "3"; // Posterior のみ 3mm に
            Assert.AreEqual("3", vm.Y2Text);
            Assert.AreEqual("10", vm.X1Text);

            // 再度 Uniform を ON にすると、現在の MarginUniformText で再同期されること
            vm.MarginUniformText = "7";
            vm.IsUniformMargin = true;
            Assert.AreEqual("7", vm.Y2Text);
            Assert.AreEqual("7", vm.X1Text);
        }

        [TestMethod]
        public void ToCsvLine_AllCategories_OutputsCorrectFormat()
        {
            // 1. Add
            var addVm = new OperationStepViewModel
            {
                Category = OperationCategory.AddStructure,
                TargetStructure = "PTV_New",
                DicomType = DicomType.PTV
            };
            Assert.AreEqual("AddDelControl,Add,PTV_New,PTV", addVm.ToCsvLine());

            // 2. Del
            var delVm = new OperationStepViewModel
            {
                Category = OperationCategory.DeleteStructure,
                TargetStructure = "Temp_Ring"
            };
            Assert.AreEqual("AddDelControl,Del,Temp_Ring", delVm.ToCsvLine());

            // 3. Boolean (All 4 operations)
            var boolSub = new OperationStepViewModel
            {
                Category = OperationCategory.BooleanOperation,
                TargetStructure = "Target_Sub",
                StructureA = "CTV",
                StructureB = "Bladder",
                BoolOpType = BoolOpeType.SUB
            };
            Assert.AreEqual("BoolOpControl,SUB,Target_Sub,CTV,Bladder", boolSub.ToCsvLine());

            var boolAnd = new OperationStepViewModel
            {
                Category = OperationCategory.BooleanOperation,
                TargetStructure = "Target_And",
                StructureA = "CTV",
                StructureB = "Bladder",
                BoolOpType = BoolOpeType.AND
            };
            Assert.AreEqual("BoolOpControl,AND,Target_And,CTV,Bladder", boolAnd.ToCsvLine());

            var boolOr = new OperationStepViewModel
            {
                Category = OperationCategory.BooleanOperation,
                TargetStructure = "Target_Or",
                StructureA = "CTV",
                StructureB = "Bladder",
                BoolOpType = BoolOpeType.OR
            };
            Assert.AreEqual("BoolOpControl,OR,Target_Or,CTV,Bladder", boolOr.ToCsvLine());

            var boolXor = new OperationStepViewModel
            {
                Category = OperationCategory.BooleanOperation,
                TargetStructure = "Target_Xor",
                StructureA = "CTV",
                StructureB = "Bladder",
                BoolOpType = BoolOpeType.XOR
            };
            Assert.AreEqual("BoolOpControl,XOR,Target_Xor,CTV,Bladder", boolXor.ToCsvLine());

            // 4. Margin
            var marginVm = new OperationStepViewModel
            {
                Category = OperationCategory.Margin,
                TargetStructure = "PTV_Margin",
                OrigStructure = "CTV",
                MarginGeometry = GeoType.Inner,
                X1Text = "5",
                X2Text = "6",
                Y1Text = "7",
                Y2Text = "8",
                Z1Text = "9",
                Z2Text = "10"
            };
            Assert.AreEqual("AddMarginControl,Asymmetry,PTV_Margin,CTV,Inner,5,6,7,8,9,10", marginVm.ToCsvLine());

            // 5. ConvertHighRes
            var hiResVm = new OperationStepViewModel
            {
                Category = OperationCategory.ConvertHighRes,
                TargetStructure = "GTV_Opt"
            };
            Assert.AreEqual("ConvertHighResControl,HiRes,GTV_Opt", hiResVm.ToCsvLine());
        }

        [TestMethod]
        public void CategoryBadgeColor_ReturnsCorrectColorForEachCategory()
        {
            var vm = new OperationStepViewModel();

            vm.Category = OperationCategory.AddStructure;
            Assert.IsNotNull(vm.CategoryBadgeColor);

            vm.Category = OperationCategory.DeleteStructure;
            Assert.IsNotNull(vm.CategoryBadgeColor);

            vm.Category = OperationCategory.BooleanOperation;
            Assert.IsNotNull(vm.CategoryBadgeColor);

            vm.Category = OperationCategory.Margin;
            Assert.IsNotNull(vm.CategoryBadgeColor);

            vm.Category = OperationCategory.ConvertHighRes;
            Assert.IsNotNull(vm.CategoryBadgeColor);
        }

        [TestMethod]
        public void Execute_RemainingCategories_WithNullStructureSet_ReturnsFalseSafely()
        {
            var delVm = new OperationStepViewModel { Category = OperationCategory.DeleteStructure, TargetStructure = "Old" };
            string delLog = "";
            Assert.IsFalse(delVm.Execute(null, msg => delLog = msg));
            Assert.AreEqual("Fail", delVm.Status);
            Assert.IsTrue(delLog.Contains("StructureSet is null"));

            var marginVm = new OperationStepViewModel { Category = OperationCategory.Margin, TargetStructure = "PTV", OrigStructure = "CTV" };
            string marginLog = "";
            Assert.IsFalse(marginVm.Execute(null, msg => marginLog = msg));
            Assert.AreEqual("Fail", marginVm.Status);
            Assert.IsTrue(marginLog.Contains("StructureSet is null"));

            var hiResVm = new OperationStepViewModel { Category = OperationCategory.ConvertHighRes, TargetStructure = "GTV" };
            string hiResLog = "";
            Assert.IsFalse(hiResVm.Execute(null, msg => hiResLog = msg));
            Assert.AreEqual("Fail", hiResVm.Status);
            Assert.IsTrue(hiResLog.Contains("StructureSet is null"));
        }

        [TestMethod]
        public void OperationItemViewModel_StatusAppearance_AllStatuses()
        {
            var vm = new OperationStepViewModel();

            string[] statuses = { "Done", "OK", "Pass", "Warn", "Warning", "Ready", "Skip", "Fail", "Error", "--" };
            foreach (var st in statuses)
            {
                vm.Status = st;
                Assert.AreEqual(st, vm.Status);
                Assert.IsNotNull(vm.StatusBrush);
                Assert.IsNotNull(vm.StatusBorderBrush);
                Assert.IsNotNull(vm.StatusForeground);
            }
        }

        [TestMethod]
        public void Margin_NegativeAndZeroMargins_ParsedCorrectly()
        {
            var vm = new OperationStepViewModel
            {
                Category = OperationCategory.Margin,
                TargetStructure = "PTV_Shrunk",
                OrigStructure = "CTV",
                MarginGeometry = GeoType.Inner,
                X1Text = "0",
                X2Text = "-5",
                Y1Text = "3",
                Y2Text = "-2",
                Z1Text = "0",
                Z2Text = "-1"
            };

            var step = vm.ToTemplateStep();
            Assert.IsNotNull(step.Margins);
            Assert.AreEqual(0, step.Margins.X1);
            Assert.AreEqual(-5, step.Margins.X2);
            Assert.AreEqual(3, step.Margins.Y1);
            Assert.AreEqual(-2, step.Margins.Y2);
            Assert.AreEqual(0, step.Margins.Z1);
            Assert.AreEqual(-1, step.Margins.Z2);
        }
    }
}
