namespace AutoStructure
{
    /// <summary>
    /// ParameterList
    /// </summary>
    public class ParameterList
    {
        public string OpType { get; set; }

        public string AddDel_OpType { get; set; }
        public string AddDel_Status { get; set; }
        public string AddDel_OutputName { get; set; }
        public string AddDel_DicomType { get; set; }

        public string Bool_OpType { get; set; }
        public string Bool_Status { get; set; }
        public string Bool_OutputName { get; set; }
        public string Bool_StrA { get; set; }
        public string Bool_StrB { get; set; }

        public string Margin_OpType { get; set; }
        public string Margin_Status { get; set; }
        public string Margin_OutputName { get; set; }
        public string Margin_OrigName { get; set; }
        public string Margin_geoType { get; set; }
        public string Margin_X1 { get; set; }
        public string Margin_X2 { get; set; }
        public string Margin_Y1 { get; set; }
        public string Margin_Y2 { get; set; }
        public string Margin_Z1 { get; set; }
        public string Margin_Z2 { get; set; }

        public string Hires_OpType { get; set; }
        public string Hires_Status { get; set; }
        public string Hires_OutputName { get; set; }
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
