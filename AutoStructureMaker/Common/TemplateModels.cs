using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace AutoStructure.Common
{
    /// <summary>
    /// 輪郭作成プロトコル全体のXMLテンプレート定義
    /// </summary>
    [Serializable]
    [XmlRoot("StructureTemplate")]
    public class StructureTemplate
    {
        [XmlElement("ProtocolName")]
        public string ProtocolName { get; set; } = "AutoStructure Protocol";

        [XmlElement("Author")]
        public string Author { get; set; } = "";

        [XmlElement("CreatedAt")]
        public string CreatedAt { get; set; } = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        [XmlElement("Description")]
        public string Description { get; set; } = "";

        [XmlArray("Steps")]
        [XmlArrayItem("Step")]
        public List<TemplateStep> Steps { get; set; } = new List<TemplateStep>();
    }

    /// <summary>
    /// テンプレート内の個別の操作ステップ
    /// </summary>
    [Serializable]
    public class TemplateStep
    {
        [XmlAttribute("Number")]
        public int StepNumber { get; set; }

        [XmlAttribute("Type")]
        public string Type { get; set; } // "Add", "Del", "Boolean", "Margin", "ConvertHighRes"

        [XmlAttribute("Enabled")]
        public bool Enabled { get; set; } = true;

        [XmlElement("TargetStructure")]
        public string TargetStructure { get; set; }

        // Add / Del 用
        [XmlElement("DicomType")]
        public string DicomType { get; set; }

        // Boolean 用
        [XmlElement("BooleanOperation")]
        public string BooleanOperation { get; set; }

        [XmlElement("StructureA")]
        public string StructureA { get; set; }

        [XmlElement("StructureB")]
        public string StructureB { get; set; }

        // Margin 用
        [XmlElement("OrigStructure")]
        public string OrigStructure { get; set; }

        [XmlElement("MarginGeometry")]
        public string MarginGeometry { get; set; }

        [XmlElement("Margins")]
        public MarginValues Margins { get; set; }
    }

    /// <summary>
    /// マージン各軸の値 (mm)
    /// </summary>
    [Serializable]
    public class MarginValues
    {
        [XmlAttribute("X1")]
        public int X1 { get; set; } = 7;

        [XmlAttribute("X2")]
        public int X2 { get; set; } = 7;

        [XmlAttribute("Y1")]
        public int Y1 { get; set; } = 7;

        [XmlAttribute("Y2")]
        public int Y2 { get; set; } = 7;

        [XmlAttribute("Z1")]
        public int Z1 { get; set; } = 7;

        [XmlAttribute("Z2")]
        public int Z2 { get; set; } = 7;
    }
}
