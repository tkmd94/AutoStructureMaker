using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using AutoStructure;
using AutoStructure.Common;
using AutoStructure.ViewModels;

namespace AutoStructureMaker.Tests
{
    [TestClass]
    public class PreFlightValidatorTests
    {
        [TestMethod]
        public void Validate_WhenOperationsEmpty_ReturnsError()
        {
            var ops = new List<OperationItemViewModel>();
            var result = PreFlightValidator.Validate(ops, new List<string> { "PTV" });

            Assert.IsFalse(result.IsValid);
            Assert.IsTrue(result.Errors.Count > 0);
        }

        [TestMethod]
        public void Validate_WhenEmptyTargetStructure_ReturnsError()
        {
            var ops = new List<OperationItemViewModel>
            {
                new OperationStepViewModel
                {
                    StepNumber = 1,
                    Category = OperationCategory.AddStructure,
                    TargetStructure = ""
                }
            };

            var result = PreFlightValidator.Validate(ops, new List<string>());
            Assert.IsFalse(result.IsValid);
            Assert.IsTrue(result.Errors.Exists(e => e.Message.Contains("cannot be empty")));
        }

        [TestMethod]
        public void Validate_WhenReferencingNonExistentStructure_ReturnsError()
        {
            var ops = new List<OperationItemViewModel>
            {
                new OperationStepViewModel
                {
                    StepNumber = 1,
                    Category = OperationCategory.BooleanOperation,
                    TargetStructure = "CTV_SUB",
                    StructureA = "Ghost_CTV",
                    StructureB = "Ghost_Bladder",
                    BoolOpType = BoolOpeType.SUB
                }
            };

            var result = PreFlightValidator.Validate(ops, new List<string> { "Real_CTV" });
            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(2, result.Errors.Count); // Ghost_CTV and Ghost_Bladder
        }

        [TestMethod]
        public void Validate_WhenReferencingEarlierCreatedStructure_Passes()
        {
            var ops = new List<OperationItemViewModel>
            {
                new OperationStepViewModel
                {
                    StepNumber = 1,
                    Category = OperationCategory.AddStructure,
                    TargetStructure = "CTV_Opt",
                    DicomType = DicomType.CTV
                },
                new OperationStepViewModel
                {
                    StepNumber = 2,
                    Category = OperationCategory.Margin,
                    TargetStructure = "PTV_Opt",
                    OrigStructure = "CTV_Opt"
                },
                new OperationStepViewModel
                {
                    StepNumber = 3,
                    Category = OperationCategory.BooleanOperation,
                    TargetStructure = "PTV_Final",
                    StructureA = "PTV_Opt",
                    StructureB = "Bladder",
                    BoolOpType = BoolOpeType.SUB
                }
            };

            var result = PreFlightValidator.Validate(ops, new List<string> { "Bladder" });
            Assert.IsTrue(result.IsValid, result.SummaryText);
            Assert.AreEqual(0, result.Errors.Count);
        }

        [TestMethod]
        public void Validate_WhenStepIsDisabled_DoesNotPropagateToLaterSteps()
        {
            var ops = new List<OperationItemViewModel>
            {
                new OperationStepViewModel
                {
                    StepNumber = 1,
                    Category = OperationCategory.AddStructure,
                    TargetStructure = "CTV_Opt",
                    IsEnabled = false // 無効化
                },
                new OperationStepViewModel
                {
                    StepNumber = 2,
                    Category = OperationCategory.Margin,
                    TargetStructure = "PTV_Opt",
                    OrigStructure = "CTV_Opt" // 先行が無効化されているためエラーになるべき
                }
            };

            var result = PreFlightValidator.Validate(ops, new List<string>());
            Assert.IsFalse(result.IsValid);
            Assert.IsTrue(result.Errors.Exists(e => e.Message.Contains("does not exist")));
        }

        [TestMethod]
        public void Validate_WhenDuplicateAdd_ReturnsError()
        {
            var ops = new List<OperationItemViewModel>
            {
                new OperationStepViewModel
                {
                    StepNumber = 1,
                    Category = OperationCategory.AddStructure,
                    TargetStructure = "Existing_CTV"
                }
            };

            var result = PreFlightValidator.Validate(ops, new List<string> { "Existing_CTV" });
            Assert.IsFalse(result.IsValid);
            Assert.IsTrue(result.Errors.Exists(e => e.Message.Contains("already exists")));
        }

        [TestMethod]
        public void Validate_WhenSubtractingSelf_ReturnsWarning()
        {
            var ops = new List<OperationItemViewModel>
            {
                new OperationStepViewModel
                {
                    StepNumber = 1,
                    Category = OperationCategory.BooleanOperation,
                    TargetStructure = "Empty_Result",
                    StructureA = "CTV",
                    StructureB = "CTV",
                    BoolOpType = BoolOpeType.SUB
                }
            };

            var result = PreFlightValidator.Validate(ops, new List<string> { "CTV" });
            Assert.IsTrue(result.IsValid); // エラーではなく警告なので IsValid は true
            Assert.IsTrue(result.Warnings.Exists(w => w.Message.Contains("empty structure")));
        }

        [TestMethod]
        public void Validate_CaseInsensitiveStructureNames_RecognizesCorrectly()
        {
            var ops = new List<OperationItemViewModel>
            {
                new OperationStepViewModel
                {
                    StepNumber = 1,
                    Category = OperationCategory.Margin,
                    TargetStructure = "PTV_78GY",
                    OrigStructure = "ctv_prostate" // 小文字で参照
                }
            };

            // 患者輪郭は大文字混じり
            var result = PreFlightValidator.Validate(ops, new List<string> { "CTV_Prostate" });
            Assert.IsTrue(result.IsValid, "Structure name resolution should be case-insensitive.");
            Assert.AreEqual(0, result.Errors.Count);
        }

        [TestMethod]
        public void Validate_MaxDicomNameLength_GeneratesWarningAbove16Chars()
        {
            var ops = new List<OperationItemViewModel>
            {
                new OperationStepViewModel
                {
                    StepNumber = 1,
                    Category = OperationCategory.AddStructure,
                    TargetStructure = "StructureNameExceeding16Chars" // 29 文字
                }
            };

            var result = PreFlightValidator.Validate(ops, new List<string>());
            Assert.IsTrue(result.IsValid); // 警告なので IsValid は true
            Assert.IsTrue(result.Warnings.Exists(w => w.Message.Contains("exceeds 16 characters")));
        }

        [TestMethod]
        public void Validate_WhitespaceOnlyStructureName_GeneratesError()
        {
            var ops = new List<OperationItemViewModel>
            {
                new OperationStepViewModel
                {
                    StepNumber = 1,
                    Category = OperationCategory.AddStructure,
                    TargetStructure = "    \t  "
                }
            };

            var result = PreFlightValidator.Validate(ops, new List<string>());
            Assert.IsFalse(result.IsValid);
            Assert.IsTrue(result.Errors.Exists(e => e.Message.Contains("cannot be empty")));
        }
    }
}
