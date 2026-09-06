using System;
using System.Collections.Generic;
using System.Linq;
using AutoStructure.ViewModels;

namespace AutoStructure.Common
{
    /// <summary>
    /// 事前妥当性検査（Pre-Flight Validation）の結果
    /// </summary>
    public class ValidationResult
    {
        public bool IsValid => Errors.Count == 0;
        public List<ValidationIssue> Issues { get; } = new List<ValidationIssue>();
        public List<ValidationIssue> Errors => Issues.Where(x => x.IsError).ToList();
        public List<ValidationIssue> Warnings => Issues.Where(x => !x.IsError).ToList();

        public string SummaryText
        {
            get
            {
                if (Issues.Count == 0) return "Validation passed: No issues found.";
                return $"Validation: {Errors.Count} error(s), {Warnings.Count} warning(s).";
            }
        }
    }

    /// <summary>
    /// 検証で検出された個別課題（エラーまたは警告）
    /// </summary>
    public class ValidationIssue
    {
        public int StepNumber { get; set; }
        public string Message { get; set; }
        public bool IsError { get; set; }

        public override string ToString()
        {
            string severity = IsError ? "ERROR" : "WARNING";
            return $"Step {StepNumber} [{severity}]: {Message}";
        }
    }

    /// <summary>
    /// 輪郭操作パイプラインの実行前一括妥当性検査エンジン
    /// </summary>
    public static class PreFlightValidator
    {
        public static ValidationResult Validate(IEnumerable<OperationItemViewModel> operations, IEnumerable<string> existingStructureIds = null)
        {
            var result = new ValidationResult();
            if (operations == null) return result;

            var opList = operations.ToList();
            if (opList.Count == 0)
            {
                result.Issues.Add(new ValidationIssue
                {
                    StepNumber = 0,
                    IsError = true,
                    Message = "No operations configured."
                });
                return result;
            }

            // 現在の利用可能輪郭セット（既存＋先行有効ステップで作成された輪郭）
            var availableStructures = new HashSet<string>(existingStructureIds ?? Enumerable.Empty<string>(), StringComparer.OrdinalIgnoreCase);
            var addedInPipeline = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < opList.Count; i++)
            {
                var op = opList[i];
                int stepNum = op.StepNumber > 0 ? op.StepNumber : (i + 1);

                // ステップが無効化されている場合はスキップ（先行チェーンにも伝播させない）
                if (!op.IsEnabled)
                {
                    continue;
                }

                if (op is OperationStepViewModel step)
                {
                    string target = step.TargetStructure?.Trim();

                    // 1. TargetStructure の空チェック
                    if (string.IsNullOrWhiteSpace(target))
                    {
                        result.Issues.Add(new ValidationIssue
                        {
                            StepNumber = stepNum,
                            IsError = true,
                            Message = "Target structure name cannot be empty."
                        });
                        continue;
                    }

                    // 2. カテゴリー別検証
                    switch (step.Category)
                    {
                        case OperationCategory.AddStructure:
                            // DICOM 規格（16文字推奨）チェック
                            if (target.Length > 16)
                            {
                                result.Issues.Add(new ValidationIssue
                                {
                                    StepNumber = stepNum,
                                    IsError = false,
                                    Message = $"Structure name '{target}' exceeds 16 characters (DICOM recommended maximum)."
                                });
                            }

                            // 既存輪郭との重複チェック
                            if (existingStructureIds != null && existingStructureIds.Contains(target, StringComparer.OrdinalIgnoreCase))
                            {
                                result.Issues.Add(new ValidationIssue
                                {
                                    StepNumber = stepNum,
                                    IsError = true,
                                    Message = $"Structure '{target}' already exists in patient dataset. AddStructure will fail."
                                });
                            }
                            else if (addedInPipeline.Contains(target))
                            {
                                result.Issues.Add(new ValidationIssue
                                {
                                    StepNumber = stepNum,
                                    IsError = true,
                                    Message = $"Structure '{target}' is already created in an earlier step."
                                });
                            }

                            // 有効ステップなので作成輪郭を登録
                            availableStructures.Add(target);
                            addedInPipeline.Add(target);
                            break;

                        case OperationCategory.DeleteStructure:
                            if (!availableStructures.Contains(target))
                            {
                                result.Issues.Add(new ValidationIssue
                                {
                                    StepNumber = stepNum,
                                    IsError = true,
                                    Message = $"Structure '{target}' to delete does not exist in patient dataset or earlier steps."
                                });
                            }
                            else
                            {
                                availableStructures.Remove(target);
                            }
                            break;

                        case OperationCategory.BooleanOperation:
                            string strA = step.StructureA?.Trim();
                            string strB = step.StructureB?.Trim();

                            if (string.IsNullOrWhiteSpace(strA))
                            {
                                result.Issues.Add(new ValidationIssue
                                {
                                    StepNumber = stepNum,
                                    IsError = true,
                                    Message = "Structure A must be specified."
                                });
                            }
                            else if (!availableStructures.Contains(strA))
                            {
                                result.Issues.Add(new ValidationIssue
                                {
                                    StepNumber = stepNum,
                                    IsError = true,
                                    Message = $"Structure A '{strA}' does not exist in patient dataset or earlier steps."
                                });
                            }

                            if (string.IsNullOrWhiteSpace(strB))
                            {
                                result.Issues.Add(new ValidationIssue
                                {
                                    StepNumber = stepNum,
                                    IsError = true,
                                    Message = "Structure B must be specified."
                                });
                            }
                            else if (!availableStructures.Contains(strB))
                            {
                                result.Issues.Add(new ValidationIssue
                                {
                                    StepNumber = stepNum,
                                    IsError = true,
                                    Message = $"Structure B '{strB}' does not exist in patient dataset or earlier steps."
                                });
                            }

                            if (!string.IsNullOrWhiteSpace(strA) && !string.IsNullOrWhiteSpace(strB) &&
                                string.Equals(strA, strB, StringComparison.OrdinalIgnoreCase))
                            {
                                if (step.BoolOpType == BoolOpeType.SUB)
                                {
                                    result.Issues.Add(new ValidationIssue
                                    {
                                        StepNumber = stepNum,
                                        IsError = false,
                                        Message = $"Subtracting '{strA}' from itself will result in an empty structure."
                                    });
                                }
                            }

                            // 演算結果輪郭を利用可能に追加
                            availableStructures.Add(target);
                            break;

                        case OperationCategory.Margin:
                            string orig = step.OrigStructure?.Trim();
                            if (string.IsNullOrWhiteSpace(orig))
                            {
                                result.Issues.Add(new ValidationIssue
                                {
                                    StepNumber = stepNum,
                                    IsError = true,
                                    Message = "Original structure must be specified for margin calculation."
                                });
                            }
                            else if (!availableStructures.Contains(orig))
                            {
                                result.Issues.Add(new ValidationIssue
                                {
                                    StepNumber = stepNum,
                                    IsError = true,
                                    Message = $"Original structure '{orig}' does not exist in patient dataset or earlier steps."
                                });
                            }

                            // 演算結果輪郭を利用可能に追加
                            availableStructures.Add(target);
                            break;

                        case OperationCategory.ConvertHighRes:
                            if (!availableStructures.Contains(target))
                            {
                                result.Issues.Add(new ValidationIssue
                                {
                                    StepNumber = stepNum,
                                    IsError = true,
                                    Message = $"Target structure '{target}' to convert does not exist in patient dataset or earlier steps."
                                });
                            }
                            break;
                    }
                }
            }

            return result;
        }
    }
}
