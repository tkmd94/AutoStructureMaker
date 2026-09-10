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
                    TargetStructure = "Real_Target",
                    StructureA = "Ghost_CTV",
                    StructureB = "Ghost_Bladder",
                    BoolOpType = BoolOpeType.SUB
                }
            };

            var result = PreFlightValidator.Validate(ops, new List<string> { "Real_Target" });
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
                    Category = OperationCategory.AddStructure,
                    TargetStructure = "PTV_Opt",
                    DicomType = DicomType.PTV
                },
                new OperationStepViewModel
                {
                    StepNumber = 3,
                    Category = OperationCategory.Margin,
                    TargetStructure = "PTV_Opt",
                    OrigStructure = "CTV_Opt"
                },
                new OperationStepViewModel
                {
                    StepNumber = 4,
                    Category = OperationCategory.AddStructure,
                    TargetStructure = "PTV_Final",
                    DicomType = DicomType.PTV
                },
                new OperationStepViewModel
                {
                    StepNumber = 5,
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

            var result = PreFlightValidator.Validate(ops, new List<string> { "CTV", "Empty_Result" });
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
            var result = PreFlightValidator.Validate(ops, new List<string> { "CTV_Prostate", "ptv_78gy" });
            Assert.IsTrue(result.IsValid, "Structure name resolution should be case-insensitive.");
            Assert.AreEqual(0, result.Errors.Count);
        }

        [TestMethod]
        public void Validate_WhenBooleanTargetDoesNotExist_ReturnsError()
        {
            var ops = new List<OperationItemViewModel>
            {
                new OperationStepViewModel
                {
                    StepNumber = 1,
                    Category = OperationCategory.BooleanOperation,
                    TargetStructure = "Missing_Target_PTV",
                    StructureA = "CTV",
                    StructureB = "Bladder",
                    BoolOpType = BoolOpeType.SUB
                }
            };

            var result = PreFlightValidator.Validate(ops, new List<string> { "CTV", "Bladder" });
            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.IsTrue(result.Errors.Exists(e => e.Message.Contains("Target structure 'Missing_Target_PTV' does not exist")));
        }

        [TestMethod]
        public void Validate_WhenMarginTargetDoesNotExist_ReturnsError()
        {
            var ops = new List<OperationItemViewModel>
            {
                new OperationStepViewModel
                {
                    StepNumber = 1,
                    Category = OperationCategory.Margin,
                    TargetStructure = "Missing_Target_Margin",
                    OrigStructure = "CTV"
                }
            };

            var result = PreFlightValidator.Validate(ops, new List<string> { "CTV" });
            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.IsTrue(result.Errors.Exists(e => e.Message.Contains("Target structure 'Missing_Target_Margin' does not exist")));
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

        [TestMethod]
        public void Validate_WhenBooleanStructureA_IsEmpty_GeneratesError()
        {
            var ops = new List<OperationItemViewModel>
            {
                new OperationStepViewModel
                {
                    StepNumber = 1,
                    Category = OperationCategory.BooleanOperation,
                    TargetStructure = "Target_Valid",
                    StructureA = "",
                    StructureB = "CTV",
                    BoolOpType = BoolOpeType.SUB
                }
            };

            var result = PreFlightValidator.Validate(ops, new List<string> { "Target_Valid", "CTV" });
            Assert.IsFalse(result.IsValid);
            Assert.IsTrue(result.Errors.Exists(e => e.Message.Contains("Structure A must be specified")));
        }

        [TestMethod]
        public void Validate_WhenBooleanStructureB_IsEmpty_GeneratesError()
        {
            var ops = new List<OperationItemViewModel>
            {
                new OperationStepViewModel
                {
                    StepNumber = 1,
                    Category = OperationCategory.BooleanOperation,
                    TargetStructure = "Target_Valid",
                    StructureA = "CTV",
                    StructureB = "   ",
                    BoolOpType = BoolOpeType.SUB
                }
            };

            var result = PreFlightValidator.Validate(ops, new List<string> { "Target_Valid", "CTV" });
            Assert.IsFalse(result.IsValid);
            Assert.IsTrue(result.Errors.Exists(e => e.Message.Contains("Structure B must be specified")));
        }

        [TestMethod]
        public void Validate_WhenConvertHighResTarget_DoesNotExist_GeneratesError()
        {
            var ops = new List<OperationItemViewModel>
            {
                new OperationStepViewModel
                {
                    StepNumber = 1,
                    Category = OperationCategory.ConvertHighRes,
                    TargetStructure = "Ghost_Target"
                }
            };

            var result = PreFlightValidator.Validate(ops, new List<string> { "Real_Structure" });
            Assert.IsFalse(result.IsValid);
            Assert.IsTrue(result.Errors.Exists(e => e.Message.Contains("Target structure 'Ghost_Target' to convert does not exist")));
        }

        [TestMethod]
        public void Validate_WhenAddStructureAlreadyCreatedInPipeline_GeneratesError()
        {
            var ops = new List<OperationItemViewModel>
            {
                new OperationStepViewModel
                {
                    StepNumber = 1,
                    Category = OperationCategory.AddStructure,
                    TargetStructure = "Duplicate_Pipeline"
                },
                new OperationStepViewModel
                {
                    StepNumber = 2,
                    Category = OperationCategory.AddStructure,
                    TargetStructure = "Duplicate_Pipeline"
                }
            };

            var result = PreFlightValidator.Validate(ops, new List<string>());
            Assert.IsFalse(result.IsValid);
            Assert.IsTrue(result.Errors.Exists(e => e.Message.Contains("is already created in an earlier step")));
        }

        [TestMethod]
        public void Validate_WhenDeletingNonExistentStructure_GeneratesError()
        {
            var ops = new List<OperationItemViewModel>
            {
                new OperationStepViewModel
                {
                    StepNumber = 1,
                    Category = OperationCategory.DeleteStructure,
                    TargetStructure = "Ghost_Structure"
                }
            };

            var result = PreFlightValidator.Validate(ops, new List<string> { "Other_Structure" });
            Assert.IsFalse(result.IsValid);
            Assert.IsTrue(result.Errors.Exists(e => e.Message.Contains("to delete does not exist")));
        }

        [TestMethod]
        public void Validate_AllBooleanOperations_AND_OR_XOR_ValidateProperly()
        {
            var ops = new List<OperationItemViewModel>
            {
                new OperationStepViewModel
                {
                    StepNumber = 1,
                    Category = OperationCategory.BooleanOperation,
                    TargetStructure = "Target_And",
                    StructureA = "CTV",
                    StructureB = "Bladder",
                    BoolOpType = BoolOpeType.AND
                },
                new OperationStepViewModel
                {
                    StepNumber = 2,
                    Category = OperationCategory.BooleanOperation,
                    TargetStructure = "Target_Or",
                    StructureA = "CTV",
                    StructureB = "Bladder",
                    BoolOpType = BoolOpeType.OR
                },
                new OperationStepViewModel
                {
                    StepNumber = 3,
                    Category = OperationCategory.BooleanOperation,
                    TargetStructure = "Target_Xor",
                    StructureA = "CTV",
                    StructureB = "Bladder",
                    BoolOpType = BoolOpeType.XOR
                }
            };

            var existing = new List<string> { "CTV", "Bladder", "Target_And", "Target_Or", "Target_Xor" };
            var result = PreFlightValidator.Validate(ops, existing);
            Assert.IsTrue(result.IsValid, result.SummaryText);
            Assert.AreEqual(0, result.Errors.Count);
        }
    }
}
