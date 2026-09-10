using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using AutoStructure;
using AutoStructure.Common;
using AutoStructure.ViewModels;

namespace AutoStructureMaker.Tests
{
    [TestClass]
    public class MainViewModelTests
    {
        private MainViewModel _vm;

        [TestInitialize]
        public void Setup()
        {
            _vm = new MainViewModel();
        }

        [TestMethod]
        public void AddStepCommand_AddsNewStep_WithCorrectStepNumber()
        {
            // Act
            _vm.AddStepCommand.Execute(null);
            _vm.AddStepCommand.Execute(null);
            _vm.AddStepCommand.Execute(null);

            // Assert
            Assert.AreEqual(3, _vm.Operations.Count);
            Assert.AreEqual(1, _vm.Operations[0].StepNumber);
            Assert.AreEqual(2, _vm.Operations[1].StepNumber);
            Assert.AreEqual(3, _vm.Operations[2].StepNumber);
            Assert.IsTrue(_vm.HasOperations);
            Assert.AreEqual("3 steps configured", _vm.OperationsSummaryText);
        }

        [TestMethod]
        public void AddStepCommand_WithSpecificCategory_SetsCategoryCorrectly()
        {
            // Act
            _vm.AddStepCommand.Execute(OperationCategory.Margin);
            _vm.AddStepCommand.Execute(OperationCategory.BooleanOperation);

            // Assert
            Assert.AreEqual(2, _vm.Operations.Count);
            var op1 = _vm.Operations[0] as OperationStepViewModel;
            var op2 = _vm.Operations[1] as OperationStepViewModel;

            Assert.IsNotNull(op1);
            Assert.AreEqual(OperationCategory.Margin, op1.Category);
            Assert.IsTrue(op1.IsMarginVisible);

            Assert.IsNotNull(op2);
            Assert.AreEqual(OperationCategory.BooleanOperation, op2.Category);
            Assert.IsTrue(op2.IsBooleanVisible);
        }

        [TestMethod]
        public void MoveUpCommand_WhenFirstItem_CannotExecute()
        {
            // Arrange
            _vm.AddStepCommand.Execute(null);
            _vm.AddStepCommand.Execute(null);

            // Act & Assert
            bool canMoveFirst = _vm.MoveUpCommand.CanExecute(_vm.Operations[0]);
            bool canMoveSecond = _vm.MoveUpCommand.CanExecute(_vm.Operations[1]);

            Assert.IsFalse(canMoveFirst, "First item should not be able to move up.");
            Assert.IsTrue(canMoveSecond, "Second item should be able to move up.");
        }

        [TestMethod]
        public void MoveDownCommand_WhenLastItem_CannotExecute()
        {
            // Arrange
            _vm.AddStepCommand.Execute(null);
            _vm.AddStepCommand.Execute(null);

            // Act & Assert
            bool canMoveFirst = _vm.MoveDownCommand.CanExecute(_vm.Operations[0]);
            bool canMoveLast = _vm.MoveDownCommand.CanExecute(_vm.Operations[1]);

            Assert.IsTrue(canMoveFirst, "First item should be able to move down.");
            Assert.IsFalse(canMoveLast, "Last item should not be able to move down.");
        }

        [TestMethod]
        public void MoveUpCommand_SwapsItemsAndReNumbers()
        {
            // Arrange
            _vm.AddStepCommand.Execute(OperationCategory.AddStructure);
            _vm.AddStepCommand.Execute(OperationCategory.Margin);
            _vm.Operations[0].TargetStructure = "ItemA";
            _vm.Operations[1].TargetStructure = "ItemB";

            // Act: 2番目 (ItemB) を MoveUp
            _vm.MoveUpCommand.Execute(_vm.Operations[1]);

            // Assert
            Assert.AreEqual("ItemB", _vm.Operations[0].TargetStructure);
            Assert.AreEqual("ItemA", _vm.Operations[1].TargetStructure);
            Assert.AreEqual(1, _vm.Operations[0].StepNumber);
            Assert.AreEqual(2, _vm.Operations[1].StepNumber);
        }

        [TestMethod]
        public void MoveDownCommand_SwapsItemsAndReNumbers()
        {
            // Arrange
            _vm.AddStepCommand.Execute(OperationCategory.AddStructure);
            _vm.AddStepCommand.Execute(OperationCategory.Margin);
            _vm.Operations[0].TargetStructure = "ItemA";
            _vm.Operations[1].TargetStructure = "ItemB";

            // Act: 1番目 (ItemA) を MoveDown
            _vm.MoveDownCommand.Execute(_vm.Operations[0]);

            // Assert
            Assert.AreEqual("ItemB", _vm.Operations[0].TargetStructure);
            Assert.AreEqual("ItemA", _vm.Operations[1].TargetStructure);
            Assert.AreEqual(1, _vm.Operations[0].StepNumber);
            Assert.AreEqual(2, _vm.Operations[1].StepNumber);
        }

        [TestMethod]
        public void RemoveItemCommand_RemovesItem_AndReNumbersRemainingItems()
        {
            // Arrange
            _vm.AddStepCommand.Execute(null); // Step 1
            _vm.AddStepCommand.Execute(null); // Step 2
            _vm.AddStepCommand.Execute(null); // Step 3
            _vm.Operations[0].TargetStructure = "A";
            _vm.Operations[1].TargetStructure = "B";
            _vm.Operations[2].TargetStructure = "C";

            // Act: 2番目 (B) を削除
            var itemToRemove = _vm.Operations[1];
            _vm.RemoveItemCommand.Execute(itemToRemove);

            // Assert
            Assert.AreEqual(2, _vm.Operations.Count);
            Assert.AreEqual("A", _vm.Operations[0].TargetStructure);
            Assert.AreEqual(1, _vm.Operations[0].StepNumber);
            Assert.AreEqual("C", _vm.Operations[1].TargetStructure);
            Assert.AreEqual(2, _vm.Operations[1].StepNumber);
        }

        [TestMethod]
        public void ResetStatus_ResetsAllOperationsStatus()
        {
            // Arrange
            _vm.AddStepCommand.Execute(null);
            _vm.AddStepCommand.Execute(null);
            _vm.Operations[0].SetStatusResult(true);
            _vm.Operations[1].SetStatusResult(false);

            Assert.AreEqual("Done", _vm.Operations[0].Status);
            Assert.AreEqual("Fail", _vm.Operations[1].Status);

            // Act
            foreach (var op in _vm.Operations)
            {
                op.ResetStatus();
            }

            // Assert
            Assert.AreEqual("--", _vm.Operations[0].Status);
            Assert.AreEqual("--", _vm.Operations[1].Status);
        }

        [TestMethod]
        public void SharedStructures_SynchronizedAcrossOperations()
        {
            // Arrange
            _vm.AddStepCommand.Execute(null);
            _vm.AddStepCommand.Execute(null);

            // Act: ViewModel のマスターリストに追加
            _vm.AvailableStructures.Add("CTV_Prostate");
            _vm.AvailableStructures.Add("Bladder");

            // Assert: 各操作のドロップダウン候補に即座に反映されていること
            Assert.IsTrue(_vm.Operations[0].AvailableStructures.Contains("CTV_Prostate"));
            Assert.IsTrue(_vm.Operations[1].AvailableStructures.Contains("Bladder"));
        }

        [TestMethod]
        public void DuplicateStepCommand_ClonesItemAndInsertsBelow()
        {
            // Arrange
            _vm.AddStepCommand.Execute(OperationCategory.Margin);
            var firstOp = _vm.Operations[0] as OperationStepViewModel;
            firstOp.TargetStructure = "PTV_Margin5";
            firstOp.OrigStructure = "CTV";
            firstOp.IsUniformMargin = true;
            firstOp.MarginUniformText = "5";

            // Act: 複製を実行
            _vm.DuplicateStepCommand.Execute(firstOp);

            // Assert: 要素数が2になり、直下に同じパラメータの要素が挿入され、Step番号が1, 2になっていること
            Assert.AreEqual(2, _vm.Operations.Count);
            Assert.AreEqual(1, _vm.Operations[0].StepNumber);
            Assert.AreEqual(2, _vm.Operations[1].StepNumber);

            var clonedOp = _vm.Operations[1] as OperationStepViewModel;
            Assert.IsNotNull(clonedOp);
            Assert.AreEqual(OperationCategory.Margin, clonedOp.Category);
            Assert.AreEqual("PTV_Margin5", clonedOp.TargetStructure);
            Assert.AreEqual("CTV", clonedOp.OrigStructure);
            Assert.AreEqual("5", clonedOp.X1Text);
        }

        [TestMethod]
        public void DisabledStep_ExcludedFromSubsequentAvailableStructures()
        {
            // Arrange
            _vm.AddStepCommand.Execute(OperationCategory.AddStructure);
            _vm.AddStepCommand.Execute(OperationCategory.Margin);

            _vm.Operations[0].TargetStructure = "CTV_Opt";

            // 最初は Step 2 の候補に CTV_Opt がある
            Assert.IsTrue(_vm.Operations[1].AvailableStructures.Contains("CTV_Opt"));

            // Act: Step 1 を無効化
            _vm.Operations[0].IsEnabled = false;

            // Assert: Step 2 の候補から CTV_Opt が除外されること
            Assert.IsFalse(_vm.Operations[1].AvailableStructures.Contains("CTV_Opt"));

            // Act: 再び有効化
            _vm.Operations[0].IsEnabled = true;

            // Assert: 再び候補に現れること
            Assert.IsTrue(_vm.Operations[1].AvailableStructures.Contains("CTV_Opt"));
        }

        [TestMethod]
        public void RemoveItemCommand_RecalculatesStructurePropagation()
        {
            // Arrange: Step 1 で PTV_Add を作成、Step 2 で参照
            _vm.AddStepCommand.Execute(OperationCategory.AddStructure);
            _vm.AddStepCommand.Execute(OperationCategory.Margin);
            _vm.Operations[0].TargetStructure = "PTV_Add";

            Assert.IsTrue(_vm.Operations[1].AvailableStructures.Contains("PTV_Add"));

            // Act: Step 1 を削除
            _vm.RemoveItemCommand.Execute(_vm.Operations[0]);

            // Assert: 残った Step 1 (旧 Step 2) から PTV_Add が消えていること
            Assert.AreEqual(1, _vm.Operations.Count);
            Assert.IsFalse(_vm.Operations[0].AvailableStructures.Contains("PTV_Add"));
        }

        [TestMethod]
        public void MoveUpCommand_RecalculatesStructurePropagation()
        {
            // Arrange:
            // Step 1: Add "ContourA"
            // Step 2: Add "ContourB"
            // Step 3: Margin from "ContourB"
            _vm.AddStepCommand.Execute(OperationCategory.AddStructure);
            _vm.AddStepCommand.Execute(OperationCategory.AddStructure);
            _vm.AddStepCommand.Execute(OperationCategory.Margin);
            _vm.Operations[0].TargetStructure = "ContourA";
            _vm.Operations[1].TargetStructure = "ContourB";

            // 最初は Step 2 (ContourB) の候補に ContourA がある
            Assert.IsTrue(_vm.Operations[1].AvailableStructures.Contains("ContourA"));
            Assert.IsFalse(_vm.Operations[1].AvailableStructures.Contains("ContourB"));

            // Act: Step 2 (ContourB) を MoveUp して先頭にする
            _vm.MoveUpCommand.Execute(_vm.Operations[1]);

            // Assert: 先頭になった ContourB の候補には ContourA は無く、
            // 2番目になった ContourA の候補に ContourB が現れること
            Assert.AreEqual("ContourB", _vm.Operations[0].TargetStructure);
            Assert.AreEqual("ContourA", _vm.Operations[1].TargetStructure);
            Assert.IsFalse(_vm.Operations[0].AvailableStructures.Contains("ContourA"));
            Assert.IsTrue(_vm.Operations[1].AvailableStructures.Contains("ContourB"));
        }

        [TestMethod]
        public void SyncStructureInfos_PreservesInstanceReferences_ForExistingItems()
        {
            // Arrange
            _vm.SetBaseStructures(new[]
            {
                new AutoStructure.Common.StructureInfo("CTV", false, "CTV", isNew: false),
                new AutoStructure.Common.StructureInfo("PTV", false, "PTV", isNew: true)
            });

            _vm.AddStepCommand.Execute(OperationCategory.Margin);
            var marginOp = _vm.Operations[0] as OperationStepViewModel;
            Assert.IsNotNull(marginOp);

            var originalCtvInstance = marginOp.AvailableStructureInfos.FirstOrDefault(x => x.Id == "CTV");
            Assert.IsNotNull(originalCtvInstance);

            // Act: 実行後の同期（PTV が実在輪郭になり、新しい輪郭 Other が追加される）
            _vm.SetBaseStructures(new[]
            {
                new AutoStructure.Common.StructureInfo("CTV", false, "CTV", isNew: false),
                new AutoStructure.Common.StructureInfo("Other", false, "CONTROL", isNew: false),
                new AutoStructure.Common.StructureInfo("PTV", false, "PTV", isNew: false)
            });

            // Assert: 既存の CTV のインスタンス参照が維持されていること（ComboBox の選択解除を防止）
            var updatedCtvInstance = marginOp.AvailableStructureInfos.FirstOrDefault(x => x.Id == "CTV");
            Assert.AreSame(originalCtvInstance, updatedCtvInstance, "Existing StructureInfo instance must be preserved across syncs to avoid ComboBox unselection.");
            Assert.AreEqual(3, marginOp.AvailableStructureInfos.Count);
        }

        [TestMethod]
        public void RefreshStepStructureContexts_PreservesStepInputs_WhenContextUpdatedWithNewStructures()
        {
            // Arrange: 臨床での典型的マージン構成
            _vm.SetBaseStructures(new[]
            {
                new AutoStructure.Common.StructureInfo("CTV", false, "CTV", isNew: false)
            });

            _vm.AddStepCommand.Execute(OperationCategory.AddStructure);
            _vm.AddStepCommand.Execute(OperationCategory.Margin);
            _vm.AddStepCommand.Execute(OperationCategory.AddStructure);

            _vm.Operations[0].TargetStructure = "PTV";
            var marginOp = _vm.Operations[1] as OperationStepViewModel;
            Assert.IsNotNull(marginOp);
            marginOp.TargetStructure = "PTV";
            marginOp.OrigStructure = "CTV";
            _vm.Operations[2].TargetStructure = "Other";

            _vm.RefreshStepStructureContexts();

            // Act: 実行完了後の同期（StructureSet に実際に PTV と Other が登録された状態）
            _vm.SetBaseStructures(new[]
            {
                new AutoStructure.Common.StructureInfo("CTV", false, "CTV", isNew: false),
                new AutoStructure.Common.StructureInfo("Other", false, "CONTROL", isNew: false),
                new AutoStructure.Common.StructureInfo("PTV", false, "PTV", isNew: false)
            });

            // Assert: マージン処理の TargetStructure および OrigStructure が保持されていること
            Assert.AreEqual("PTV", marginOp.TargetStructure, "TargetStructure must not be cleared after context sync.");
            Assert.AreEqual("CTV", marginOp.OrigStructure, "OrigStructure must not be cleared after context sync.");
        }

        [TestMethod]
        public void WPF_ComboBoxBinding_TargetAndOrigStructure_PreservedAfterContextSync()
        {
            // Arrange: WPF ComboBox と ViewModel を結合した実際の UI 動作検証
            _vm.SetBaseStructures(new[]
            {
                new AutoStructure.Common.StructureInfo("CTV", false, "CTV", isNew: false)
            });

            _vm.AddStepCommand.Execute(OperationCategory.AddStructure);
            _vm.AddStepCommand.Execute(OperationCategory.Margin);
            _vm.AddStepCommand.Execute(OperationCategory.AddStructure);

            _vm.Operations[0].TargetStructure = "PTV";
            var marginOp = _vm.Operations[1] as OperationStepViewModel;
            Assert.IsNotNull(marginOp);
            marginOp.TargetStructure = "PTV";
            marginOp.OrigStructure = "CTV";
            _vm.Operations[2].TargetStructure = "Other";

            _vm.RefreshStepStructureContexts();

            // ComboBox へのバインディング
            var cbTarget = new System.Windows.Controls.ComboBox
            {
                IsEditable = true,
                ItemsSource = marginOp.AvailableStructureInfos
            };
            System.Windows.Controls.TextSearch.SetTextPath(cbTarget, "Id");
            cbTarget.SetBinding(System.Windows.Controls.ComboBox.TextProperty, new System.Windows.Data.Binding("TargetStructure")
            {
                Source = marginOp,
                Mode = System.Windows.Data.BindingMode.TwoWay,
                UpdateSourceTrigger = System.Windows.Data.UpdateSourceTrigger.PropertyChanged
            });

            var cbOrig = new System.Windows.Controls.ComboBox
            {
                IsEditable = true,
                ItemsSource = marginOp.AvailableStructureInfos
            };
            System.Windows.Controls.TextSearch.SetTextPath(cbOrig, "Id");
            cbOrig.SetBinding(System.Windows.Controls.ComboBox.TextProperty, new System.Windows.Data.Binding("OrigStructure")
            {
                Source = marginOp,
                Mode = System.Windows.Data.BindingMode.TwoWay,
                UpdateSourceTrigger = System.Windows.Data.UpdateSourceTrigger.PropertyChanged
            });

            Assert.AreEqual("PTV", cbTarget.Text);
            Assert.AreEqual("CTV", cbOrig.Text);

            // Act: 実行完了後の同期（StructureSet の輪郭一覧更新）
            _vm.SetBaseStructures(new[]
            {
                new AutoStructure.Common.StructureInfo("CTV", false, "CTV", isNew: false),
                new AutoStructure.Common.StructureInfo("Other", false, "CONTROL", isNew: false),
                new AutoStructure.Common.StructureInfo("PTV", false, "PTV", isNew: false)
            });

            // Assert: ComboBox 表示テキストおよび ViewModel の値が共に消失せず保持されていること
            Assert.AreEqual("PTV", marginOp.TargetStructure, "ViewModel TargetStructure must be preserved.");
            Assert.AreEqual("CTV", marginOp.OrigStructure, "ViewModel OrigStructure must be preserved.");
            Assert.AreEqual("PTV", cbTarget.Text, "ComboBox Target text must be preserved.");
            Assert.AreEqual("CTV", cbOrig.Text, "ComboBox Orig text must be preserved.");
        }

        [TestMethod]
        public void DuplicateStepCommand_BooleanStep_WpfComboBoxSelection_PreservesValues()
        {
            // Arrange: PTV is created in Step 1 (not in base structures)
            _vm.SetBaseStructures(new[]
            {
                new AutoStructure.Common.StructureInfo("Bladder", false, "ORGAN", isNew: false),
                new AutoStructure.Common.StructureInfo("Rectum", false, "ORGAN", isNew: false)
            });

            _vm.AddStepCommand.Execute(OperationCategory.AddStructure);
            _vm.Operations[0].TargetStructure = "PTV";

            _vm.AddStepCommand.Execute(OperationCategory.BooleanOperation);
            var boolOp = _vm.Operations[1] as OperationStepViewModel;
            Assert.IsNotNull(boolOp);
            boolOp.TargetStructure = "Bladder_sub";
            boolOp.StructureA = "Bladder";
            boolOp.StructureB = "PTV";
            boolOp.BoolOpType = AutoStructure.BoolOpeType.SUB;

            _vm.RefreshStepStructureContexts();

            // Act 1: boolOp の ComboBox を作成（実際のUI表示状態）
            var cb1_Target = new System.Windows.Controls.ComboBox { IsEditable = true, ItemsSource = boolOp.AvailableStructureInfos };
            System.Windows.Controls.TextSearch.SetTextPath(cb1_Target, "Id");
            cb1_Target.SetBinding(System.Windows.Controls.ComboBox.TextProperty, new System.Windows.Data.Binding("TargetStructure")
            {
                Source = boolOp, Mode = System.Windows.Data.BindingMode.TwoWay, UpdateSourceTrigger = System.Windows.Data.UpdateSourceTrigger.PropertyChanged
            });
            var cb1_A = new System.Windows.Controls.ComboBox { IsEditable = true, ItemsSource = boolOp.AvailableStructureInfos };
            System.Windows.Controls.TextSearch.SetTextPath(cb1_A, "Id");
            cb1_A.SetBinding(System.Windows.Controls.ComboBox.TextProperty, new System.Windows.Data.Binding("StructureA")
            {
                Source = boolOp, Mode = System.Windows.Data.BindingMode.TwoWay, UpdateSourceTrigger = System.Windows.Data.UpdateSourceTrigger.PropertyChanged
            });
            var cb1_B = new System.Windows.Controls.ComboBox { IsEditable = true, ItemsSource = boolOp.AvailableStructureInfos };
            System.Windows.Controls.TextSearch.SetTextPath(cb1_B, "Id");
            cb1_B.SetBinding(System.Windows.Controls.ComboBox.TextProperty, new System.Windows.Data.Binding("StructureB")
            {
                Source = boolOp, Mode = System.Windows.Data.BindingMode.TwoWay, UpdateSourceTrigger = System.Windows.Data.UpdateSourceTrigger.PropertyChanged
            });

            // Act 2: 複製を実行
            _vm.DuplicateStepCommand.Execute(boolOp);
            Assert.AreEqual(3, _vm.Operations.Count);

            var clonedOp = _vm.Operations[2] as OperationStepViewModel;
            Assert.IsNotNull(clonedOp);

            // コレクションインスタンスが独立していること（共有されていないこと）を検証
            Assert.AreNotSame(boolOp.AvailableStructureInfos, clonedOp.AvailableStructureInfos, "Cloned step must have independent AvailableStructureInfos.");
            Assert.AreNotSame(boolOp.AvailableStructures, clonedOp.AvailableStructures, "Cloned step must have independent AvailableStructures.");
            Assert.AreNotSame(_vm.AvailableStructureInfos, clonedOp.AvailableStructureInfos, "Cloned step must not share ViewModel master collection.");

            // clonedOp の ComboBox を作成（実際のUI表示状態）
            var cbTarget = new System.Windows.Controls.ComboBox
            {
                IsEditable = true,
                ItemsSource = clonedOp.AvailableStructureInfos
            };
            System.Windows.Controls.TextSearch.SetTextPath(cbTarget, "Id");
            cbTarget.SetBinding(System.Windows.Controls.ComboBox.TextProperty, new System.Windows.Data.Binding("TargetStructure")
            {
                Source = clonedOp,
                Mode = System.Windows.Data.BindingMode.TwoWay,
                UpdateSourceTrigger = System.Windows.Data.UpdateSourceTrigger.PropertyChanged
            });

            var cbA = new System.Windows.Controls.ComboBox
            {
                IsEditable = true,
                ItemsSource = clonedOp.AvailableStructureInfos
            };
            System.Windows.Controls.TextSearch.SetTextPath(cbA, "Id");
            cbA.SetBinding(System.Windows.Controls.ComboBox.TextProperty, new System.Windows.Data.Binding("StructureA")
            {
                Source = clonedOp,
                Mode = System.Windows.Data.BindingMode.TwoWay,
                UpdateSourceTrigger = System.Windows.Data.UpdateSourceTrigger.PropertyChanged
            });

            var cbB = new System.Windows.Controls.ComboBox
            {
                IsEditable = true,
                ItemsSource = clonedOp.AvailableStructureInfos
            };
            System.Windows.Controls.TextSearch.SetTextPath(cbB, "Id");
            cbB.SetBinding(System.Windows.Controls.ComboBox.TextProperty, new System.Windows.Data.Binding("StructureB")
            {
                Source = clonedOp,
                Mode = System.Windows.Data.BindingMode.TwoWay,
                UpdateSourceTrigger = System.Windows.Data.UpdateSourceTrigger.PropertyChanged
            });

            // 複製直後の値を確認
            Assert.AreEqual("Bladder_sub", clonedOp.TargetStructure, "Cloned TargetStructure immediately after duplicate");
            Assert.AreEqual("Bladder", clonedOp.StructureA, "Cloned StructureA immediately after duplicate");
            Assert.AreEqual("PTV", clonedOp.StructureB, "Cloned StructureB immediately after duplicate");

            // Act 3: 複製したモジュールで StructureA をドロップダウンから選択 (Rectum を選択)
            var rectumItem = clonedOp.AvailableStructureInfos.FirstOrDefault(x => x.Id == "Rectum");
            Assert.IsNotNull(rectumItem, "Rectum must exist in AvailableStructureInfos");
            cbA.SelectedItem = rectumItem;

            // Assert: 選択変更後も全フィールドが保持されていること
            Assert.AreEqual("Rectum", clonedOp.StructureA, "StructureA should be Rectum");
            Assert.AreEqual("PTV", clonedOp.StructureB, "StructureB should still be PTV");
            Assert.AreEqual("Bladder_sub", clonedOp.TargetStructure, "TargetStructure should still be Bladder_sub");
            Assert.IsFalse(string.IsNullOrEmpty(clonedOp.StructureA), "StructureA must not be empty");
            Assert.IsFalse(string.IsNullOrEmpty(clonedOp.StructureB), "StructureB must not be empty");
            Assert.IsFalse(string.IsNullOrEmpty(clonedOp.TargetStructure), "TargetStructure must not be empty");

            // 元のモジュール (boolOp) の値も一切影響を受けていないこと
            Assert.AreEqual("Bladder_sub", boolOp.TargetStructure);
            Assert.AreEqual("Bladder", boolOp.StructureA);
            Assert.AreEqual("PTV", boolOp.StructureB);
        }

        [TestMethod]
        public void LoadTemplate_AndDuplicate_CollectionsAreIndependentAndPreserved()
        {
            // Arrange: テンプレート作成
            var template = new AutoStructure.Common.StructureTemplate
            {
                ProtocolName = "Test_Protocol",
                Steps = new List<AutoStructure.Common.TemplateStep>
                {
                    new AutoStructure.Common.TemplateStep { StepNumber = 1, Type = "Add", TargetStructure = "CTV", DicomType = "CTV" },
                    new AutoStructure.Common.TemplateStep { StepNumber = 2, Type = "Boolean", TargetStructure = "PTV_Opt", StructureA = "CTV", StructureB = "Bladder", BooleanOperation = "SUB" }
                }
            };
            string tempFile = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"AutoStructure_Test_{System.Guid.NewGuid():N}.xml");
            AutoStructure.Common.TemplateService.SaveTemplate(tempFile, template, asCsv: false);

            try
            {
                var loaded = AutoStructure.Common.TemplateService.LoadTemplate(tempFile);
                _vm.Operations.Clear();
                foreach (var step in loaded.Steps)
                {
                    var op = OperationItemViewModel.FromTemplateStep(step, _vm.AvailableStructures, _vm.AvailableStructureInfos, _vm.StructureResolutionMap);
                    _vm.Operations.Add(op);
                }
                _vm.RefreshStepStructureContexts();

                // ロード直後の全ステップのコレクションが独立していることを確認
                Assert.AreEqual(2, _vm.Operations.Count);
                Assert.AreNotSame(_vm.Operations[0].AvailableStructureInfos, _vm.Operations[1].AvailableStructureInfos);

                // Step 2 を複製
                var boolOp = _vm.Operations[1] as OperationStepViewModel;
                _vm.DuplicateStepCommand.Execute(boolOp);

                Assert.AreEqual(3, _vm.Operations.Count);
                var dupOp = _vm.Operations[2] as OperationStepViewModel;
                Assert.IsNotNull(dupOp);

                // 複製ステップも独立していること
                Assert.AreNotSame(boolOp.AvailableStructureInfos, dupOp.AvailableStructureInfos);
                Assert.AreEqual("PTV_Opt", dupOp.TargetStructure);
                Assert.AreEqual("CTV", dupOp.StructureA);
                Assert.AreEqual("Bladder", dupOp.StructureB);

                // 値の変更
                dupOp.StructureA = "Bladder";
                Assert.AreEqual("Bladder", dupOp.StructureA);
                Assert.AreEqual("Bladder", dupOp.StructureB);
                Assert.AreEqual("PTV_Opt", dupOp.TargetStructure);
            }
            finally
            {
                if (System.IO.File.Exists(tempFile))
                {
                    System.IO.File.Delete(tempFile);
                }
            }
        }

        [TestMethod]
        public void OpenHelpCommand_CanExecute_AndEmbeddedManualResourceExists()
        {
            // Assert OpenHelpCommand is available and executable
            Assert.IsNotNull(_vm.OpenHelpCommand);
            Assert.IsTrue(_vm.OpenHelpCommand.CanExecute(null));

            // Verify embedded PDF resource in AutoStructureMaker assembly
            var asm = typeof(MainViewModel).Assembly;
            var resourceNames = asm.GetManifestResourceNames();
            var manualResource = resourceNames.FirstOrDefault(r => r.EndsWith("AutoStructureMaker_Manual.pdf", StringComparison.OrdinalIgnoreCase));

            Assert.IsNotNull(manualResource, $"Embedded manual resource not found among: {string.Join(", ", resourceNames)}");

            using (var stream = asm.GetManifestResourceStream(manualResource))
            {
                Assert.IsNotNull(stream);
                Assert.IsTrue(stream.Length > 100000, $"Embedded manual size is unexpectedly small: {stream.Length} bytes");
            }
        }

        [TestMethod]
        public void ApplyValidationResult_UpdatesStatusAndToolTip_ForOperations()
        {
            // Setup base structures
            _vm.SetBaseStructures(new[]
            {
                new StructureInfo { Id = "CTV" },
                new StructureInfo { Id = "Bladder" }
            });

            // Step 1: 正常な Add (OK になるべき)
            var op1 = new OperationStepViewModel
            {
                StepNumber = 1,
                Category = OperationCategory.AddStructure,
                TargetStructure = "PTV_New"
            };

            // Step 2: 存在しない輪郭を参照する Boolean (Error になるべき)
            var op2 = new OperationStepViewModel
            {
                StepNumber = 2,
                Category = OperationCategory.BooleanOperation,
                TargetStructure = "Ghost_Target",
                StructureA = "CTV",
                StructureB = "Bladder",
                BoolOpType = BoolOpeType.SUB
            };

            // Step 3: 自己減算の Boolean (Warn になるべき)
            var op3 = new OperationStepViewModel
            {
                StepNumber = 3,
                Category = OperationCategory.BooleanOperation,
                TargetStructure = "CTV",
                StructureA = "CTV",
                StructureB = "CTV",
                BoolOpType = BoolOpeType.SUB
            };

            // Step 4: 無効化されたステップ (Skip になるべき)
            var op4 = new OperationStepViewModel
            {
                StepNumber = 4,
                Category = OperationCategory.Margin,
                TargetStructure = "CTV",
                OrigStructure = "CTV",
                IsEnabled = false
            };

            _vm.Operations.Clear();
            _vm.Operations.Add(op1);
            _vm.Operations.Add(op2);
            _vm.Operations.Add(op3);
            _vm.Operations.Add(op4);

            var result = PreFlightValidator.Validate(_vm.Operations, _vm.AvailableStructures);
            _vm.ApplyValidationResultToOperations(result);

            // Assert statuses and tooltips
            Assert.AreEqual("OK", op1.Status, "Step 1 should be OK");
            Assert.IsTrue(op1.StatusToolTip.Contains("Ready") || op1.StatusToolTip.Contains("Validation passed"));

            Assert.AreEqual("Error", op2.Status, "Step 2 should be Error");
            Assert.IsTrue(op2.StatusToolTip.Contains("Error"));

            Assert.AreEqual("Warn", op3.Status, "Step 3 should be Warn");
            Assert.IsTrue(op3.StatusToolTip.Contains("Warning"));

            Assert.AreEqual("Skip", op4.Status, "Step 4 should be Skip");
            Assert.IsTrue(op4.StatusToolTip.Contains("disabled") || op4.StatusToolTip.Contains("skipped"));
        }

        [TestMethod]
        public void MoveUpCommand_MiddleItem_SwapsAndReNumbers()
        {
            _vm.AddStepCommand.Execute(OperationCategory.AddStructure);
            _vm.AddStepCommand.Execute(OperationCategory.Margin);
            _vm.AddStepCommand.Execute(OperationCategory.BooleanOperation);

            _vm.Operations[0].TargetStructure = "Step1";
            _vm.Operations[1].TargetStructure = "Step2";
            _vm.Operations[2].TargetStructure = "Step3";

            // Act: 2番目 (Step2) を MoveUp
            _vm.MoveUpCommand.Execute(_vm.Operations[1]);

            Assert.AreEqual("Step2", _vm.Operations[0].TargetStructure);
            Assert.AreEqual("Step1", _vm.Operations[1].TargetStructure);
            Assert.AreEqual("Step3", _vm.Operations[2].TargetStructure);
            Assert.AreEqual(1, _vm.Operations[0].StepNumber);
            Assert.AreEqual(2, _vm.Operations[1].StepNumber);
            Assert.AreEqual(3, _vm.Operations[2].StepNumber);
        }

        [TestMethod]
        public void MoveDownCommand_MiddleItem_SwapsAndReNumbers()
        {
            _vm.AddStepCommand.Execute(OperationCategory.AddStructure);
            _vm.AddStepCommand.Execute(OperationCategory.Margin);
            _vm.AddStepCommand.Execute(OperationCategory.BooleanOperation);

            _vm.Operations[0].TargetStructure = "Step1";
            _vm.Operations[1].TargetStructure = "Step2";
            _vm.Operations[2].TargetStructure = "Step3";

            // Act: 2番目 (Step2) を MoveDown
            _vm.MoveDownCommand.Execute(_vm.Operations[1]);

            Assert.AreEqual("Step1", _vm.Operations[0].TargetStructure);
            Assert.AreEqual("Step3", _vm.Operations[1].TargetStructure);
            Assert.AreEqual("Step2", _vm.Operations[2].TargetStructure);
            Assert.AreEqual(1, _vm.Operations[0].StepNumber);
            Assert.AreEqual(2, _vm.Operations[1].StepNumber);
            Assert.AreEqual(3, _vm.Operations[2].StepNumber);
        }

        [TestMethod]
        public void DuplicateStepCommand_MiddleItem_InsertsAtNextIndexAndPreservesOrder()
        {
            _vm.AddStepCommand.Execute(OperationCategory.AddStructure);
            _vm.AddStepCommand.Execute(OperationCategory.Margin);
            _vm.AddStepCommand.Execute(OperationCategory.BooleanOperation);

            _vm.Operations[0].TargetStructure = "Step1";
            _vm.Operations[1].TargetStructure = "Step2";
            _vm.Operations[2].TargetStructure = "Step3";

            // Act: Step 2 を複製
            _vm.DuplicateStepCommand.Execute(_vm.Operations[1]);

            Assert.AreEqual(4, _vm.Operations.Count);
            Assert.AreEqual("Step1", _vm.Operations[0].TargetStructure);
            Assert.AreEqual("Step2", _vm.Operations[1].TargetStructure);
            Assert.AreEqual("Step2", _vm.Operations[2].TargetStructure); // 複製された要素
            Assert.AreEqual("Step3", _vm.Operations[3].TargetStructure);
            Assert.AreEqual(1, _vm.Operations[0].StepNumber);
            Assert.AreEqual(2, _vm.Operations[1].StepNumber);
            Assert.AreEqual(3, _vm.Operations[2].StepNumber);
            Assert.AreEqual(4, _vm.Operations[3].StepNumber);
        }

        [TestMethod]
        public void DuplicateStepCommand_DisabledStep_PreservesDisabledState()
        {
            _vm.AddStepCommand.Execute(OperationCategory.Margin);
            var op = _vm.Operations[0];
            op.TargetStructure = "PTV_Margin";
            op.IsEnabled = false;

            // Act: 複製を実行
            _vm.DuplicateStepCommand.Execute(op);

            Assert.AreEqual(2, _vm.Operations.Count);
            Assert.IsFalse(_vm.Operations[0].IsEnabled);
            Assert.IsFalse(_vm.Operations[1].IsEnabled);
        }

        [TestMethod]
        public void DeleteCommand_RemovesLastItemUntilEmpty_SummaryUpdatesAccurately()
        {
            _vm.AddStepCommand.Execute(OperationCategory.AddStructure);
            _vm.AddStepCommand.Execute(OperationCategory.Margin);

            Assert.AreEqual(2, _vm.Operations.Count);
            Assert.IsTrue(_vm.HasOperations);
            Assert.AreEqual("2 steps configured", _vm.OperationsSummaryText);

            // Act 1: 1回削除
            _vm.DeleteCommand.Execute(null);
            Assert.AreEqual(1, _vm.Operations.Count);
            Assert.IsTrue(_vm.HasOperations);
            Assert.AreEqual("1 step configured", _vm.OperationsSummaryText);

            // Act 2: もう1回削除
            _vm.DeleteCommand.Execute(null);
            Assert.AreEqual(0, _vm.Operations.Count);
            Assert.IsFalse(_vm.HasOperations);
            Assert.AreEqual("0 steps configured", _vm.OperationsSummaryText);
        }

        [TestMethod]
        public void RefreshStepStructureContexts_HighResPropagation_ThroughBooleanAndMargin()
        {
            _vm.SetBaseStructures(new[]
            {
                new StructureInfo("GTV_High", isHighResolution: true, dicomType: "GTV"),
                new StructureInfo("Bladder_Std", isHighResolution: false, dicomType: "ORGAN")
            });

            // Step 1: Margin from GTV_High -> Result Target_Margin should be High-Res
            _vm.AddStepCommand.Execute(OperationCategory.Margin);
            var step1 = (OperationStepViewModel)_vm.Operations[0];
            step1.TargetStructure = "Target_Margin";
            step1.OrigStructure = "GTV_High";

            // Step 2: Boolean SUB: Target_Margin (High) - Bladder_Std (Std) -> Result Target_Bool should be High-Res
            _vm.AddStepCommand.Execute(OperationCategory.BooleanOperation);
            var step2 = (OperationStepViewModel)_vm.Operations[1];
            step2.TargetStructure = "Target_Bool";
            step2.StructureA = "Target_Margin";
            step2.StructureB = "Bladder_Std";
            step2.BoolOpType = BoolOpeType.SUB;

            // Step 3: AddStructure (just to check context of step 3)
            _vm.AddStepCommand.Execute(OperationCategory.AddStructure);
            var step3 = (OperationStepViewModel)_vm.Operations[2];

            _vm.RefreshStepStructureContexts();

            // Assert: Step 3's available structures has Target_Margin (HIGH) and Target_Bool (HIGH)
            var marginInfo = step3.AvailableStructureInfos.FirstOrDefault(x => x.Id == "Target_Margin");
            var boolInfo = step3.AvailableStructureInfos.FirstOrDefault(x => x.Id == "Target_Bool");

            Assert.IsNotNull(marginInfo, "Target_Margin should be in step 3 context");
            Assert.IsTrue(marginInfo.IsHighResolution, "Target_Margin derived from GTV_High should be High-Res");

            Assert.IsNotNull(boolInfo, "Target_Bool should be in step 3 context");
            Assert.IsTrue(boolInfo.IsHighResolution, "Target_Bool combining High and Std should be High-Res");
        }
    }
}
