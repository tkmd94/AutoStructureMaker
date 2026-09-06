using System.Windows.Media;

namespace AutoStructure.Common
{
    /// <summary>
    /// 輪郭のID、解像度、DICOMタイプ、および新規作成フラグ情報を保持するモデル
    /// </summary>
    public class StructureInfo
    {
        public string Id { get; set; } = "";
        public bool IsHighResolution { get; set; }
        public string DicomType { get; set; } = "";
        public bool IsNew { get; set; }

        public StructureInfo() { }

        public StructureInfo(string id, bool isHighResolution, string dicomType = "", bool isNew = false)
        {
            Id = id;
            IsHighResolution = isHighResolution;
            DicomType = dicomType;
            IsNew = isNew;
        }

        public string ResolutionBadgeText
        {
            get
            {
                if (IsNew)
                {
                    return IsHighResolution ? "NEW: HIGH" : "NEW: STD";
                }
                return IsHighResolution ? "HIGH" : "STD";
            }
        }

        public Brush ResolutionBadgeBrush
        {
            get
            {
                if (IsNew)
                {
                    return IsHighResolution
                        ? new SolidColorBrush(Color.FromRgb(123, 31, 162)) // Vivid Purple for New High-Res
                        : new SolidColorBrush(Color.FromRgb(46, 125, 50));  // Forest Green for New Std-Res
                }
                return IsHighResolution
                    ? new SolidColorBrush(Color.FromRgb(106, 27, 154))  // Deep Purple for Existing High-Res
                    : new SolidColorBrush(Color.FromRgb(84, 110, 122)); // BlueGrey for Existing Std-Res
            }
        }

        public override string ToString() => Id;
    }
}
