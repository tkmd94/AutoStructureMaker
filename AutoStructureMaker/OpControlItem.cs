namespace AutoStructure
{
    /// <summary>
    /// 操作種別の統合カテゴリー
    /// </summary>
    public enum OperationCategory
    {
        AddStructure,
        DeleteStructure,
        BooleanOperation,
        Margin,
        ConvertHighRes
    }

    /// <summary>
    /// カテゴリー選択 ComboBox 用アイテム
    /// </summary>
    public class OperationCategoryItem
    {
        public string Label { get; set; }
        public OperationCategory Value { get; set; }
    }

    /// <summary>
    /// AddDelStrType
    /// </summary>
    public enum AddDelStrType
    {
        Add,
        Del
    }

    /// <summary>
    /// AddDelOpControlComboBoxItem
    /// </summary>
    public class AddDelOpControlComboBoxItem
    {
        public string Label { get; set; }
        public AddDelStrType Value { get; set; }
    }

    /// <summary>
    /// DicomType
    /// </summary>
    public enum DicomType
    {
        PTV,
        ORGAN,
        CONTROL,
        DOSE_REGION,
        AVOIDANCE,
        CAVITY,
        CONTRAST_AGENT,
        CTV,
        EXTERNAL,
        GTV,
        IRRAD_VOLUME,     
        TREATED_VOLUME,
        SUPPORT,
        FIXATION,
    }

    /// <summary>
    /// AddDelDicomTypeControlComboBoxItem
    /// </summary>
    public class AddDelDicomTypeControlComboBoxItem
    {
        public string Label { get; set; }
        public DicomType Value { get; set; }
    }

    /// <summary>
    /// BoolOpeType
    /// </summary>
    public enum BoolOpeType
    {
        SUB,
        AND,
        OR,
        XOR
    }

    /// <summary>
    /// BoolOpControlItem
    /// </summary>
    public class BoolOpControlItem
    {
        public string Label { get; set; }
        public BoolOpeType Value { get; set; }
    }

    /// <summary>
    /// GeoType
    /// </summary>
    public enum GeoType
    {
        Inner,
        Outer,
    }

    /// <summary>
    /// AddMarginControlItem
    /// </summary>
    public class AddMarginControlItem
    {
        public string Label { get; set; }
        public GeoType Value { get; set; }
    }

    /// <summary>
    /// MarginType
    /// </summary>
    public enum MarginType
    {
        Asymmetry,
    }

    /// <summary>
    /// AddMarginControlComboBoxItem
    /// </summary>
    public class AddMarginControlComboBoxItem
    {
        public string Label { get; set; }
        public MarginType Value { get; set; }
    }

    /// <summary>
    /// ConvertType
    /// </summary>
    public enum ConvertType
    {
        HiRes,
    }

    public class ConvertHighResComboBoxItem
    {
        public string Label { get; set; }
        public ConvertType Value { get; set; }
    }




}
