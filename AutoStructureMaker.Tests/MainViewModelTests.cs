using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using AutoStructure;
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
    }
}
