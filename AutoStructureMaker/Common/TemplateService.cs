using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using AutoStructure.ViewModels;

namespace AutoStructure.Common
{
    /// <summary>
    /// 輪郭操作テンプレートの入出力（XML / CSV）および相互変換を担うサービスクラス
    /// </summary>
    public static class TemplateService
    {
        private static readonly XmlSerializer Serializer = new XmlSerializer(typeof(StructureTemplate));

        /// <summary>
        /// ファイルパスからテンプレートを読み込む（拡張子により XML / CSV を自動判別）
        /// </summary>
        public static StructureTemplate LoadTemplate(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
            {
                throw new FileNotFoundException("Template file not found.", filePath);
            }

            string ext = Path.GetExtension(filePath)?.ToLowerInvariant();
            if (ext == ".csv")
            {
                return LoadFromCsv(filePath);
            }
            else
            {
                return LoadFromXml(filePath);
            }
        }

        /// <summary>
        /// XML 形式でテンプレートを読み込む
        /// </summary>
        public static StructureTemplate LoadFromXml(string filePath)
        {
            using (var reader = new StreamReader(filePath, Encoding.UTF8))
            {
                return (StructureTemplate)Serializer.Deserialize(reader);
            }
        }

        /// <summary>
        /// レガシー CSV 形式からテンプレートを読み込み、正規化された StructureTemplate を生成
        /// </summary>
        public static StructureTemplate LoadFromCsv(string filePath)
        {
            var template = new StructureTemplate
            {
                ProtocolName = Path.GetFileNameWithoutExtension(filePath),
                Author = "Legacy CSV Import",
                CreatedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                Description = "Imported from legacy CSV format",
                Steps = new List<TemplateStep>()
            };

            int stepNumber = 1;
            using (var sr = new StreamReader(filePath, Encoding.UTF8))
            {
                while (!sr.EndOfStream)
                {
                    var line = sr.ReadLine();
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    var values = line.Split(',').Select(v => v.Trim()).ToArray();
                    if (values.Length == 0) continue;

                    string opName = values[0];
                    TemplateStep step = null;

                    if (opName == "AddDelControl" && (values.Length == 3 || values.Length == 4))
                    {
                        step = new TemplateStep
                        {
                            Type = values[1] == "Del" ? "Del" : "Add",
                            TargetStructure = values[2],
                            DicomType = values.Length == 4 ? values[3] : null
                        };
                    }
                    else if (opName == "BoolOpControl" && values.Length == 5)
                    {
                        step = new TemplateStep
                        {
                            Type = "Boolean",
                            BooleanOperation = values[1],
                            TargetStructure = values[2],
                            StructureA = values[3],
                            StructureB = values[4]
                        };
                    }
                    else if (opName == "AddMarginControl" && values.Length == 11)
                    {
                        step = new TemplateStep
                        {
                            Type = "Margin",
                            TargetStructure = values[2],
                            OrigStructure = values[3],
                            MarginGeometry = values[4],
                            Margins = new MarginValues
                            {
                                X1 = ParseInt(values[5], 7),
                                X2 = ParseInt(values[6], 7),
                                Y1 = ParseInt(values[7], 7),
                                Y2 = ParseInt(values[8], 7),
                                Z1 = ParseInt(values[9], 7),
                                Z2 = ParseInt(values[10], 7)
                            }
                        };
                    }
                    else if (opName == "ConvertHighResControl" && values.Length == 3)
                    {
                        step = new TemplateStep
                        {
                            Type = "ConvertHighRes",
                            TargetStructure = values[2]
                        };
                    }

                    if (step != null)
                    {
                        step.StepNumber = stepNumber++;
                        template.Steps.Add(step);
                    }
                }
            }

            return template;
        }

        /// <summary>
        /// テンプレートをファイルへ保存（拡張子により XML または CSV で保存）
        /// </summary>
        public static void SaveTemplate(string filePath, StructureTemplate template, bool asCsv = false)
        {
            if (template == null) throw new ArgumentNullException(nameof(template));

            string ext = Path.GetExtension(filePath)?.ToLowerInvariant();
            if (asCsv || ext == ".csv")
            {
                SaveToCsv(filePath, template);
            }
            else
            {
                SaveToXml(filePath, template);
            }
        }

        /// <summary>
        /// XML 形式でテンプレートを保存
        /// </summary>
        public static void SaveToXml(string filePath, StructureTemplate template)
        {
            var settings = new XmlWriterSettings
            {
                Indent = true,
                Encoding = Encoding.UTF8
            };

            using (var writer = XmlWriter.Create(filePath, settings))
            {
                Serializer.Serialize(writer, template);
            }
        }

        /// <summary>
        /// レガシー CSV 形式でテンプレートを保存
        /// </summary>
        public static void SaveToCsv(string filePath, StructureTemplate template)
        {
            using (var sw = new StreamWriter(filePath, false, Encoding.UTF8))
            {
                foreach (var step in template.Steps)
                {
                    string line = StepToCsvLine(step);
                    if (!string.IsNullOrEmpty(line))
                    {
                        sw.WriteLine(line);
                    }
                }
            }
        }

        /// <summary>
        /// TemplateStep をレガシー CSV 行に変換
        /// </summary>
        public static string StepToCsvLine(TemplateStep step)
        {
            if (step == null) return null;
            switch (step.Type?.ToLowerInvariant())
            {
                case "add":
                    return $"AddDelControl,Add,{step.TargetStructure},{step.DicomType ?? "PTV"}";
                case "del":
                    return $"AddDelControl,Del,{step.TargetStructure}";
                case "boolean":
                case "bool":
                    return $"BoolOpControl,{step.BooleanOperation ?? "SUB"},{step.TargetStructure},{step.StructureA},{step.StructureB}";
                case "margin":
                    var m = step.Margins ?? new MarginValues();
                    return $"AddMarginControl,Asymmetry,{step.TargetStructure},{step.OrigStructure},{step.MarginGeometry ?? "Outer"},{m.X1},{m.X2},{m.Y1},{m.Y2},{m.Z1},{m.Z2}";
                case "converthighres":
                case "hires":
                    return $"ConvertHighResControl,HiRes,{step.TargetStructure}";
                default:
                    return null;
            }
        }

        /// <summary>
        /// ViewModel コレクションから StructureTemplate を構築
        /// </summary>
        public static StructureTemplate CreateTemplate(IEnumerable<OperationItemViewModel> operations, string protocolName, string author, string description = "")
        {
            var template = new StructureTemplate
            {
                ProtocolName = protocolName ?? "AutoStructure Protocol",
                Author = author ?? "",
                CreatedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                Description = description ?? "",
                Steps = new List<TemplateStep>()
            };

            int num = 1;
            if (operations != null)
            {
                foreach (var op in operations)
                {
                    var step = op.ToTemplateStep();
                    if (step != null)
                    {
                        step.StepNumber = num++;
                        template.Steps.Add(step);
                    }
                }
            }

            return template;
        }

        private static int ParseInt(string text, int fallback = 0)
        {
            return int.TryParse(text, out int val) ? val : fallback;
        }
    }
}
