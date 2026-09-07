using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Media;
using AutoStructure.Common;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;

namespace AutoStructure.ViewModels
{
    /// <summary>
    /// 全輪郭操作モジュールの基底クラス
    /// </summary>
    public abstract class OperationItemViewModel : ViewModelBase
    {
        private string _status = "--";
        private Brush _statusBrush = new SolidColorBrush(Color.FromRgb(245, 245, 245));
        private Brush _statusForeground = new SolidColorBrush(Color.FromRgb(158, 158, 158));
        private Brush _statusBorderBrush = new SolidColorBrush(Color.FromRgb(224, 224, 224));
        private int _stepNumber = 1;
        private bool _isEnabled = true;

        public bool IsEnabled
        {
            get => _isEnabled;
            set => SetProperty(ref _isEnabled, value);
        }

        public int StepNumber
        {
            get => _stepNumber;
            set => SetProperty(ref _stepNumber, value);
        }

        public string Status
        {
            get => _status;
            set
            {
                if (SetProperty(ref _status, value))
                {
                    UpdateStatusAppearance();
                }
            }
        }

        public Brush StatusBrush
        {
            get => _statusBrush;
            set => SetProperty(ref _statusBrush, value);
        }

        public Brush StatusForeground
        {
            get => _statusForeground;
            set => SetProperty(ref _statusForeground, value);
        }

        public Brush StatusBorderBrush
        {
            get => _statusBorderBrush;
            set => SetProperty(ref _statusBorderBrush, value);
        }

        private string _statusToolTip = "Status";

        public string StatusToolTip
        {
            get => _statusToolTip;
            set => SetProperty(ref _statusToolTip, value);
        }

        private ObservableCollection<string> _availableStructures;

        public ObservableCollection<string> AvailableStructures
        {
            get => _availableStructures ?? (_availableStructures = new ObservableCollection<string>());
            set => SetProperty(ref _availableStructures, value);
        }

        private ObservableCollection<StructureInfo> _availableStructureInfos;

        public ObservableCollection<StructureInfo> AvailableStructureInfos
        {
            get => _availableStructureInfos ?? (_availableStructureInfos = new ObservableCollection<StructureInfo>());
            set => SetProperty(ref _availableStructureInfos, value);
        }

        private IDictionary<string, bool> _resolutionMap;

        public IDictionary<string, bool> ResolutionMap
        {
            get => _resolutionMap;
            set
            {
                if (SetProperty(ref _resolutionMap, value))
                {
                    NotifyResolutionChanged();
                }
            }
        }

        private bool _isSyncingContext;
        public bool IsSyncingContext
        {
            get => _isSyncingContext;
            set => SetProperty(ref _isSyncingContext, value);
        }

        private string _targetStructure = "";

        public string TargetStructure
        {
            get => _targetStructure;
            set
            {
                // コンテキスト同期中または無用な ComboBox 選択解除による意図しない空文字上書きを防御
                if (IsSyncingContext && string.IsNullOrEmpty(value) && !string.IsNullOrEmpty(_targetStructure))
                {
                    return;
                }

                if (SetProperty(ref _targetStructure, value))
                {
                    OnPropertyChanged(nameof(HasTargetResolution));
                    OnPropertyChanged(nameof(TargetResolutionBadgeText));
                    OnPropertyChanged(nameof(TargetResolutionBadgeBrush));
                    OnPropertyChanged(nameof(TargetResolutionToolTip));
                }
            }
        }

        public static readonly Brush HighResBrush = new SolidColorBrush(Color.FromRgb(106, 27, 154)); // Deep Purple
        public static readonly Brush StdResBrush = new SolidColorBrush(Color.FromRgb(84, 110, 122));  // BlueGrey
        public static readonly Brush NewStructureBrush = new SolidColorBrush(Color.FromRgb(46, 125, 50)); // Green

        protected string GetResolutionBadgeText(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return "";
            if (AvailableStructureInfos != null)
            {
                var info = AvailableStructureInfos.FirstOrDefault(x => string.Equals(x.Id, name, StringComparison.OrdinalIgnoreCase));
                if (info != null)
                {
                    return info.ResolutionBadgeText;
                }
            }
            if (ResolutionMap != null && ResolutionMap.TryGetValue(name, out bool isHi))
            {
                return isHi ? "HIGH" : "STD";
            }
            return "NEW";
        }

        protected Brush GetResolutionBadgeBrush(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return Brushes.Transparent;
            if (AvailableStructureInfos != null)
            {
                var info = AvailableStructureInfos.FirstOrDefault(x => string.Equals(x.Id, name, StringComparison.OrdinalIgnoreCase));
                if (info != null)
                {
                    return info.ResolutionBadgeBrush;
                }
            }
            if (ResolutionMap != null && ResolutionMap.TryGetValue(name, out bool isHi))
            {
                return isHi ? HighResBrush : StdResBrush;
            }
            return NewStructureBrush;
        }

        protected string GetResolutionToolTip(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return "";
            if (AvailableStructureInfos != null)
            {
                var info = AvailableStructureInfos.FirstOrDefault(x => string.Equals(x.Id, name, StringComparison.OrdinalIgnoreCase));
                if (info != null)
                {
                    if (info.IsNew)
                    {
                        return info.IsHighResolution
                            ? "New structure from earlier step (High Resolution)"
                            : "New structure from earlier step (Standard Resolution)";
                    }
                    return info.IsHighResolution
                        ? "High Resolution structure"
                        : "Standard Resolution structure";
                }
            }
            if (ResolutionMap != null && ResolutionMap.TryGetValue(name, out bool isHi))
            {
                return isHi ? "High Resolution structure" : "Standard Resolution structure";
            }
            return "New structure (not yet created in any earlier step)";
        }

        public bool HasTargetResolution => !string.IsNullOrWhiteSpace(TargetStructure);
        public string TargetResolutionBadgeText => GetResolutionBadgeText(TargetStructure);
        public Brush TargetResolutionBadgeBrush => GetResolutionBadgeBrush(TargetStructure);
        public string TargetResolutionToolTip => GetResolutionToolTip(TargetStructure);

        public virtual void NotifyResolutionChanged()
        {
            OnPropertyChanged(nameof(HasTargetResolution));
            OnPropertyChanged(nameof(TargetResolutionBadgeText));
            OnPropertyChanged(nameof(TargetResolutionBadgeBrush));
            OnPropertyChanged(nameof(TargetResolutionToolTip));
        }

        public void UpdateStatusAppearance()
        {
            switch (_status)
            {
                case "Done":
                case "OK":
                case "Pass":
                    StatusBrush = new SolidColorBrush(Color.FromRgb(232, 245, 233)); // Pale Green (#E8F5E9)
                    StatusBorderBrush = new SolidColorBrush(Color.FromRgb(165, 214, 167)); // Light Green (#A5D6A7)
                    StatusForeground = new SolidColorBrush(Color.FromRgb(46, 125, 50)); // Forest Green (#2E7D32)
                    break;
                case "Warn":
                case "Warning":
                    StatusBrush = new SolidColorBrush(Color.FromRgb(255, 248, 225)); // Pale Amber (#FFF8E1)
                    StatusBorderBrush = new SolidColorBrush(Color.FromRgb(255, 224, 130)); // Light Amber (#FFE082)
                    StatusForeground = new SolidColorBrush(Color.FromRgb(230, 81, 0)); // Dark Orange (#E65100)
                    break;
                case "Ready":
                    StatusBrush = new SolidColorBrush(Color.FromRgb(227, 242, 253)); // Pale Blue (#E3F2FD)
                    StatusBorderBrush = new SolidColorBrush(Color.FromRgb(144, 202, 249)); // Light Blue (#90CAF9)
                    StatusForeground = new SolidColorBrush(Color.FromRgb(21, 101, 192)); // Deep Blue (#1565C0)
                    break;
                case "Skip":
                    StatusBrush = new SolidColorBrush(Color.FromRgb(236, 239, 241)); // Light BlueGrey (#ECEFF1)
                    StatusBorderBrush = new SolidColorBrush(Color.FromRgb(207, 216, 220)); // BlueGrey Border (#CFD8DC)
                    StatusForeground = new SolidColorBrush(Color.FromRgb(120, 144, 156)); // BlueGrey Text (#78909C)
                    break;
                case "Fail":
                case "Error":
                    StatusBrush = new SolidColorBrush(Color.FromRgb(255, 235, 238)); // Pale Red (#FFEBEE)
                    StatusBorderBrush = new SolidColorBrush(Color.FromRgb(239, 154, 154)); // Light Red (#EF9A9A)
                    StatusForeground = new SolidColorBrush(Color.FromRgb(198, 40, 40)); // Dark Red (#C62828)
                    break;
                default: // "--" など
                    StatusBrush = new SolidColorBrush(Color.FromRgb(245, 245, 245)); // Neutral Gray (#F5F5F5)
                    StatusBorderBrush = new SolidColorBrush(Color.FromRgb(224, 224, 224)); // Gray Border (#E0E0E0)
                    StatusForeground = new SolidColorBrush(Color.FromRgb(158, 158, 158)); // Gray Text (#9E9E9E)
                    break;
            }
        }

        public void SetStatusResult(bool success)
        {
            Status = success ? "Done" : "Fail";
            StatusToolTip = success ? "Execution completed successfully" : "Execution failed";
        }

        public void SetStatusSkipped()
        {
            Status = "Skip";
            StatusToolTip = "Step is disabled (skipped)";
        }

        public void ResetStatus()
        {
            Status = "--";
            StatusToolTip = "Status";
        }

        public void UpdateAvailableStructures(StructureSet structureSet)
        {
            AvailableStructures.Clear();
            if (structureSet?.Structures != null)
            {
                foreach (var str in structureSet.Structures)
                {
                    AvailableStructures.Add(str.Id);
                }
            }
        }

        public abstract bool Execute(StructureSet structureSet, Action<string> logAction);
        public abstract string ToCsvLine();
        public abstract TemplateStep ToTemplateStep();

        public static OperationStepViewModel FromTemplateStep(
            TemplateStep step, 
            ObservableCollection<string> sharedStructures = null,
            ObservableCollection<StructureInfo> sharedStructureInfos = null,
            IDictionary<string, bool> resolutionMap = null)
        {
            if (step == null) return null;
            return OperationStepViewModel.CreateFromTemplateStep(step, sharedStructures, sharedStructureInfos, resolutionMap);
        }
    }

    /// <summary>
    /// 単一統合された操作ステップ ViewModel（案B）
    /// 操作カテゴリーを動的に切り替え可能。
    /// </summary>
    public class OperationStepViewModel : OperationItemViewModel
    {
        public static readonly List<OperationCategoryItem> CategoryOptions = new List<OperationCategoryItem>
        {
            new OperationCategoryItem { Label = "Add Structure", Value = OperationCategory.AddStructure },
            new OperationCategoryItem { Label = "Delete Structure", Value = OperationCategory.DeleteStructure },
            new OperationCategoryItem { Label = "Boolean Operators", Value = OperationCategory.BooleanOperation },
            new OperationCategoryItem { Label = "Margin", Value = OperationCategory.Margin },
            new OperationCategoryItem { Label = "High Res. Segment", Value = OperationCategory.ConvertHighRes }
        };

        public List<OperationCategoryItem> Categories => CategoryOptions;

        private OperationCategory _category = OperationCategory.AddStructure;

        // Add / Del 関連
        private DicomType _dicomType = DicomType.PTV;

        // Boolean 関連
        private BoolOpeType _boolOpType = BoolOpeType.SUB;
        private string _structureA = "";
        private string _structureB = "";

        // Margin 関連
        private string _origStructure = "";
        private GeoType _marginGeometry = GeoType.Outer;
        private bool _isUniformMargin = false;
        private string _marginUniformText = "7";
        private string _x1Text = "7";
        private string _x2Text = "7";
        private string _y1Text = "7";
        private string _y2Text = "7";
        private string _z1Text = "7";
        private string _z2Text = "7";

        // ConvertHighRes 関連
        private ConvertType _convertType = ConvertType.HiRes;

        public OperationStepViewModel()
        {
            // 設定ファイルからの既定値ロード
            var cfg = AppConfig.Current;
            if (cfg != null)
            {
                if (Enum.TryParse(cfg.DefaultDicomType, out DicomType dt)) _dicomType = dt;
                if (Enum.TryParse(cfg.DefaultBoolOperation, out BoolOpeType bt)) _boolOpType = bt;
                _marginGeometry = string.Equals(cfg.DefaultMarginGeometry, "Inner", StringComparison.OrdinalIgnoreCase)
                    ? GeoType.Inner : GeoType.Outer;
                _x1Text = cfg.DefaultMarginX1.ToString();
                _x2Text = cfg.DefaultMarginX2.ToString();
                _y1Text = cfg.DefaultMarginY1.ToString();
                _y2Text = cfg.DefaultMarginY2.ToString();
                _z1Text = cfg.DefaultMarginZ1.ToString();
                _z2Text = cfg.DefaultMarginZ2.ToString();
                _marginUniformText = _x1Text;
            }
        }

        #region カテゴリー & 表示制御

        public OperationCategory Category
        {
            get => _category;
            set
            {
                if (SetProperty(ref _category, value))
                {
                    OnPropertyChanged(nameof(IsAddStructure));
                    OnPropertyChanged(nameof(IsDeleteStructure));
                    OnPropertyChanged(nameof(IsAddDelVisible));
                    OnPropertyChanged(nameof(IsBooleanVisible));
                    OnPropertyChanged(nameof(IsMarginVisible));
                    OnPropertyChanged(nameof(IsConvertHighResVisible));
                    OnPropertyChanged(nameof(CategoryBadgeText));
                    OnPropertyChanged(nameof(CategoryBadgeColor));
                    OnPropertyChanged(nameof(IsBooleanResolutionMismatch));
                }
            }
        }

        public bool IsAddStructure => Category == OperationCategory.AddStructure;
        public bool IsDeleteStructure => Category == OperationCategory.DeleteStructure;
        public bool IsAddDelVisible => IsAddStructure || IsDeleteStructure;
        public bool IsBooleanVisible => Category == OperationCategory.BooleanOperation;
        public bool IsMarginVisible => Category == OperationCategory.Margin;
        public bool IsConvertHighResVisible => Category == OperationCategory.ConvertHighRes;

        public string CategoryBadgeText
        {
            get
            {
                switch (Category)
                {
                    case OperationCategory.AddStructure: return "ADD";
                    case OperationCategory.DeleteStructure: return "DEL";
                    case OperationCategory.BooleanOperation: return "BOOL";
                    case OperationCategory.Margin: return "MARGIN";
                    case OperationCategory.ConvertHighRes: return "HI-RES";
                    default: return "OP";
                }
            }
        }

        public Brush CategoryBadgeColor
        {
            get
            {
                switch (Category)
                {
                    case OperationCategory.AddStructure: return new SolidColorBrush(Color.FromRgb(46, 125, 50)); // Green
                    case OperationCategory.DeleteStructure: return new SolidColorBrush(Color.FromRgb(198, 40, 40)); // Red
                    case OperationCategory.BooleanOperation: return new SolidColorBrush(Color.FromRgb(21, 101, 192)); // Blue
                    case OperationCategory.Margin: return new SolidColorBrush(Color.FromRgb(230, 81, 0)); // Orange
                    case OperationCategory.ConvertHighRes: return new SolidColorBrush(Color.FromRgb(106, 27, 154)); // Purple
                    default: return new SolidColorBrush(Color.FromRgb(55, 71, 79));
                }
            }
        }

        #endregion

        #region Add / Del プロパティ

        public DicomType DicomType
        {
            get => _dicomType;
            set => SetProperty(ref _dicomType, value);
        }

        #endregion

        #region Boolean プロパティ

        public BoolOpeType BoolOpType
        {
            get => _boolOpType;
            set => SetProperty(ref _boolOpType, value);
        }

        public string StructureA
        {
            get => _structureA;
            set
            {
                if (IsSyncingContext && string.IsNullOrEmpty(value) && !string.IsNullOrEmpty(_structureA))
                {
                    return;
                }

                if (SetProperty(ref _structureA, value))
                {
                    OnPropertyChanged(nameof(HasStructureAResolution));
                    OnPropertyChanged(nameof(StructureAResolutionBadgeText));
                    OnPropertyChanged(nameof(StructureAResolutionBadgeBrush));
                    OnPropertyChanged(nameof(StructureAResolutionToolTip));
                    OnPropertyChanged(nameof(IsBooleanResolutionMismatch));
                }
            }
        }

        public string StructureB
        {
            get => _structureB;
            set
            {
                if (IsSyncingContext && string.IsNullOrEmpty(value) && !string.IsNullOrEmpty(_structureB))
                {
                    return;
                }

                if (SetProperty(ref _structureB, value))
                {
                    OnPropertyChanged(nameof(HasStructureBResolution));
                    OnPropertyChanged(nameof(StructureBResolutionBadgeText));
                    OnPropertyChanged(nameof(StructureBResolutionBadgeBrush));
                    OnPropertyChanged(nameof(StructureBResolutionToolTip));
                    OnPropertyChanged(nameof(IsBooleanResolutionMismatch));
                }
            }
        }

        public bool HasStructureAResolution => !string.IsNullOrWhiteSpace(StructureA);
        public string StructureAResolutionBadgeText => GetResolutionBadgeText(StructureA);
        public Brush StructureAResolutionBadgeBrush => GetResolutionBadgeBrush(StructureA);
        public string StructureAResolutionToolTip => GetResolutionToolTip(StructureA);

        public bool HasStructureBResolution => !string.IsNullOrWhiteSpace(StructureB);
        public string StructureBResolutionBadgeText => GetResolutionBadgeText(StructureB);
        public Brush StructureBResolutionBadgeBrush => GetResolutionBadgeBrush(StructureB);
        public string StructureBResolutionToolTip => GetResolutionToolTip(StructureB);

        public bool IsBooleanResolutionMismatch
        {
            get
            {
                if (Category != OperationCategory.BooleanOperation) return false;
                if (string.IsNullOrWhiteSpace(StructureA) || string.IsNullOrWhiteSpace(StructureB)) return false;

                bool? aIsHi = GetIsHighResolution(StructureA);
                bool? bIsHi = GetIsHighResolution(StructureB);

                if (aIsHi.HasValue && bIsHi.HasValue)
                {
                    return aIsHi.Value != bIsHi.Value;
                }
                return false;
            }
        }

        private bool? GetIsHighResolution(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return null;
            if (AvailableStructureInfos != null)
            {
                var info = AvailableStructureInfos.FirstOrDefault(x => string.Equals(x.Id, name, StringComparison.OrdinalIgnoreCase));
                if (info != null) return info.IsHighResolution;
            }
            if (ResolutionMap != null && ResolutionMap.TryGetValue(name, out bool isHi))
            {
                return isHi;
            }
            return null;
        }

        #endregion

        #region Margin プロパティ

        public string OrigStructure
        {
            get => _origStructure;
            set
            {
                if (IsSyncingContext && string.IsNullOrEmpty(value) && !string.IsNullOrEmpty(_origStructure))
                {
                    return;
                }

                if (SetProperty(ref _origStructure, value))
                {
                    OnPropertyChanged(nameof(HasOrigStructureResolution));
                    OnPropertyChanged(nameof(OrigStructureResolutionBadgeText));
                    OnPropertyChanged(nameof(OrigStructureResolutionBadgeBrush));
                    OnPropertyChanged(nameof(OrigStructureResolutionToolTip));
                }
            }
        }

        public bool HasOrigStructureResolution => !string.IsNullOrWhiteSpace(OrigStructure);
        public string OrigStructureResolutionBadgeText => GetResolutionBadgeText(OrigStructure);
        public Brush OrigStructureResolutionBadgeBrush => GetResolutionBadgeBrush(OrigStructure);
        public string OrigStructureResolutionToolTip => GetResolutionToolTip(OrigStructure);

        public override void NotifyResolutionChanged()
        {
            base.NotifyResolutionChanged();
            OnPropertyChanged(nameof(HasStructureAResolution));
            OnPropertyChanged(nameof(StructureAResolutionBadgeText));
            OnPropertyChanged(nameof(StructureAResolutionBadgeBrush));
            OnPropertyChanged(nameof(StructureAResolutionToolTip));

            OnPropertyChanged(nameof(HasStructureBResolution));
            OnPropertyChanged(nameof(StructureBResolutionBadgeText));
            OnPropertyChanged(nameof(StructureBResolutionBadgeBrush));
            OnPropertyChanged(nameof(StructureBResolutionToolTip));

            OnPropertyChanged(nameof(HasOrigStructureResolution));
            OnPropertyChanged(nameof(OrigStructureResolutionBadgeText));
            OnPropertyChanged(nameof(OrigStructureResolutionBadgeBrush));
            OnPropertyChanged(nameof(OrigStructureResolutionToolTip));

            OnPropertyChanged(nameof(IsBooleanResolutionMismatch));
        }

        public GeoType MarginGeometry
        {
            get => _marginGeometry;
            set => SetProperty(ref _marginGeometry, value);
        }

        public bool IsUniformMargin
        {
            get => _isUniformMargin;
            set
            {
                if (SetProperty(ref _isUniformMargin, value))
                {
                    if (value)
                    {
                        ApplyUniformMargin();
                    }
                }
            }
        }

        public string MarginUniformText
        {
            get => _marginUniformText;
            set
            {
                if (SetProperty(ref _marginUniformText, value) && IsUniformMargin)
                {
                    ApplyUniformMargin();
                }
            }
        }

        private void ApplyUniformMargin()
        {
            X1Text = _marginUniformText;
            X2Text = _marginUniformText;
            Y1Text = _marginUniformText;
            Y2Text = _marginUniformText;
            Z1Text = _marginUniformText;
            Z2Text = _marginUniformText;
        }

        public string X1Text { get => _x1Text; set => SetProperty(ref _x1Text, value); }
        public string X2Text { get => _x2Text; set => SetProperty(ref _x2Text, value); }
        public string Y1Text { get => _y1Text; set => SetProperty(ref _y1Text, value); }
        public string Y2Text { get => _y2Text; set => SetProperty(ref _y2Text, value); }
        public string Z1Text { get => _z1Text; set => SetProperty(ref _z1Text, value); }
        public string Z2Text { get => _z2Text; set => SetProperty(ref _z2Text, value); }

        private static int ParseMargin(string text, int fallback = 0)
        {
            return int.TryParse(text, out int val) ? val : fallback;
        }

        #endregion

        #region ConvertHighRes プロパティ

        public ConvertType ConvertType
        {
            get => _convertType;
            set => SetProperty(ref _convertType, value);
        }

        #endregion

        #region ESAPI 実行ロジック

        public override bool Execute(StructureSet structureSet, Action<string> logAction)
        {
            if (structureSet == null)
            {
                logAction?.Invoke("(LOG) Error: StructureSet is null.");
                SetStatusResult(false);
                return false;
            }

            switch (Category)
            {
                case OperationCategory.AddStructure:
                    return ExecuteAdd(structureSet, logAction);

                case OperationCategory.DeleteStructure:
                    return ExecuteDel(structureSet, logAction);

                case OperationCategory.BooleanOperation:
                    return ExecuteBoolean(structureSet, logAction);

                case OperationCategory.Margin:
                    return ExecuteMargin(structureSet, logAction);

                case OperationCategory.ConvertHighRes:
                    return ExecuteHighRes(structureSet, logAction);

                default:
                    logAction?.Invoke($"(LOG) Unknown category: {Category}");
                    SetStatusResult(false);
                    return false;
            }
        }

        private bool ExecuteAdd(StructureSet structureSet, Action<string> logAction)
        {
            if (string.IsNullOrWhiteSpace(TargetStructure))
            {
                logAction?.Invoke($"(LOG-Add) Invalid structure name: '{TargetStructure}'");
                SetStatusResult(false);
                return false;
            }

            Structure structure = structureSet.Structures.FirstOrDefault(x => x.Id == TargetStructure);
            if (structure == null)
            {
                if (structureSet.CanAddStructure(DicomType.ToString(), TargetStructure))
                {
                    structureSet.AddStructure(DicomType.ToString(), TargetStructure);
                    logAction?.Invoke($"(LOG-Add) Add new structure: {TargetStructure} ({DicomType})");
                    SetStatusResult(true);
                    return true;
                }
                else
                {
                    logAction?.Invoke($"(LOG-Add) Can not add new structure: {TargetStructure}");
                    SetStatusResult(false);
                    return false;
                }
            }
            else
            {
                logAction?.Invoke($"(LOG-Add) Structure already exists: {TargetStructure}");
                SetStatusResult(false);
                return false;
            }
        }

        private bool ExecuteDel(StructureSet structureSet, Action<string> logAction)
        {
            if (string.IsNullOrWhiteSpace(TargetStructure))
            {
                logAction?.Invoke($"(LOG-Del) Invalid structure name: '{TargetStructure}'");
                SetStatusResult(false);
                return false;
            }

            Structure structure = structureSet.Structures.FirstOrDefault(x => x.Id == TargetStructure);
            if (structure == null)
            {
                logAction?.Invoke($"(LOG-Del) No structure found: {TargetStructure}");
                SetStatusResult(false);
                return false;
            }

            if (structureSet.CanRemoveStructure(structure))
            {
                structureSet.RemoveStructure(structure);
                logAction?.Invoke($"(LOG-Del) Delete structure: {TargetStructure}");
                SetStatusResult(true);
                return true;
            }
            else
            {
                logAction?.Invoke($"(LOG-Del) Can not remove structure: {TargetStructure}");
                SetStatusResult(false);
                return false;
            }
        }

        private bool ExecuteBoolean(StructureSet structureSet, Action<string> logAction)
        {
            Structure oStr = structureSet.Structures.FirstOrDefault(x => x.Id == TargetStructure);
            Structure strA = structureSet.Structures.FirstOrDefault(x => x.Id == StructureA);
            Structure strB = structureSet.Structures.FirstOrDefault(x => x.Id == StructureB);

            if (oStr == null || strA == null || strB == null)
            {
                logAction?.Invoke($"(LOG-Bool) Structure not found: Target='{TargetStructure}', StrA='{StructureA}', StrB='{StructureB}'");
                SetStatusResult(false);
                return false;
            }

            if (strA.IsEmpty)
            {
                logAction?.Invoke($"(LOG-Bool) Warning: Structure A '{strA.Id}' has an empty volume.");
            }
            if (strB.IsEmpty)
            {
                logAction?.Invoke($"(LOG-Bool) Warning: Structure B '{strB.Id}' has an empty volume.");
            }

            // 解像度がすべて一致している場合は直接実行
            bool allSameResolution = (oStr.IsHighResolution == strA.IsHighResolution && strA.IsHighResolution == strB.IsHighResolution);

            if (allSameResolution)
            {
                return PerformBooleanOperation(oStr, strA, strB, logAction);
            }

            // 異なる分解能タイプが混在する場合：案1（一時的作業構造体による安全な高解像度整合）
            logAction?.Invoke($"(LOG-Bool) Resolution mismatch detected (Target:{oStr.IsHighResolution}, A:{strA.IsHighResolution}, B:{strB.IsHighResolution}). Auto-resolving via temporary high-resolution conversion...");

            var tempStructures = new List<Structure>();
            try
            {
                Structure effectiveA = strA;
                Structure effectiveB = strB;

                // 1. Structure A が低解像度の場合は一時作業構造体で高解像度化
                if (!strA.IsHighResolution)
                {
                    string tempIdA = GetUniqueTempStructureId(structureSet, "_tA_");
                    if (structureSet.CanAddStructure("CONTROL", tempIdA))
                    {
                        var tempA = structureSet.AddStructure("CONTROL", tempIdA);
                        tempStructures.Add(tempA);
                        tempA.SegmentVolume = strA.SegmentVolume;
                        if (tempA.CanConvertToHighResolution())
                        {
                            tempA.ConvertToHighResolution();
                        }
                        effectiveA = tempA;
                    }
                    else
                    {
                        logAction?.Invoke($"(LOG-Bool) Failed to create temporary structure for StructureA: {tempIdA}");
                        SetStatusResult(false);
                        return false;
                    }
                }

                // 2. Structure B が低解像度の場合は一時作業構造体で高解像度化
                if (!strB.IsHighResolution)
                {
                    string tempIdB = GetUniqueTempStructureId(structureSet, "_tB_");
                    if (structureSet.CanAddStructure("CONTROL", tempIdB))
                    {
                        var tempB = structureSet.AddStructure("CONTROL", tempIdB);
                        tempStructures.Add(tempB);
                        tempB.SegmentVolume = strB.SegmentVolume;
                        if (tempB.CanConvertToHighResolution())
                        {
                            tempB.ConvertToHighResolution();
                        }
                        effectiveB = tempB;
                    }
                    else
                    {
                        logAction?.Invoke($"(LOG-Bool) Failed to create temporary structure for StructureB: {tempIdB}");
                        SetStatusResult(false);
                        return false;
                    }
                }

                // 3. 出力先（Target）が低解像度の場合は高解像度へ昇格
                if (!oStr.IsHighResolution)
                {
                    if (oStr.CanConvertToHighResolution())
                    {
                        oStr.ConvertToHighResolution();
                        logAction?.Invoke($"(LOG-Bool) Elevated target structure '{TargetStructure}' to high resolution for precision matching.");
                    }
                }

                // 4. 高解像度同士で Boolean 演算を実行
                return PerformBooleanOperation(oStr, effectiveA, effectiveB, logAction);
            }
            catch (Exception ex)
            {
                logAction?.Invoke($"(LOG-Bool) Exception during resolution alignment: {ex.Message}");
                SetStatusResult(false);
                return false;
            }
            finally
            {
                // 5. 作成した一時作業構造体を確実にクリーンアップ（StructureSet から削除）
                foreach (var temp in tempStructures)
                {
                    try
                    {
                        if (structureSet.CanRemoveStructure(temp))
                        {
                            structureSet.RemoveStructure(temp);
                        }
                    }
                    catch (Exception ex)
                    {
                        logAction?.Invoke($"(LOG-Bool) Warning: Failed to clean up temp structure '{temp?.Id}': {ex.Message}");
                    }
                }
            }
        }

        private bool PerformBooleanOperation(Structure oStr, Structure strA, Structure strB, Action<string> logAction)
        {
            try
            {
                switch (BoolOpType)
                {
                    case BoolOpeType.SUB:
                        oStr.SegmentVolume = strA.Sub(strB);
                        break;
                    case BoolOpeType.AND:
                        oStr.SegmentVolume = strA.And(strB);
                        break;
                    case BoolOpeType.OR:
                        oStr.SegmentVolume = strA.Or(strB);
                        break;
                    case BoolOpeType.XOR:
                        oStr.SegmentVolume = strA.Xor(strB);
                        break;
                    default:
                        logAction?.Invoke($"(LOG-Bool) Invalid Bool ope: {BoolOpType}");
                        SetStatusResult(false);
                        return false;
                }

                logAction?.Invoke($"(LOG-Bool) Done: {BoolOpType}:{TargetStructure}/{StructureA}/{StructureB}");
                SetStatusResult(true);
                return true;
            }
            catch (Exception ex)
            {
                logAction?.Invoke($"(LOG-Bool) Error modifying '{TargetStructure}': {ex.Message}. (Note: The structure might be approved, locked, or used in an existing plan)");
                SetStatusResult(false);
                return false;
            }
        }

        private static string GetUniqueTempStructureId(StructureSet structureSet, string prefix)
        {
            for (int i = 1; i <= 999; i++)
            {
                string id = $"{prefix}{i}";
                if (id.Length > 16) id = id.Substring(0, 16);
                if (!structureSet.Structures.Any(s => string.Equals(s.Id, id, StringComparison.OrdinalIgnoreCase)))
                {
                    return id;
                }
            }
            return $"{prefix}_{Guid.NewGuid().ToString("N").Substring(0, 4)}";
        }

        private bool ExecuteMargin(StructureSet structureSet, Action<string> logAction)
        {
            Structure outStr = structureSet.Structures.FirstOrDefault(x => x.Id == TargetStructure);
            Structure origStr = structureSet.Structures.FirstOrDefault(x => x.Id == OrigStructure);

            if (outStr != null && origStr != null)
            {
                if (origStr.IsEmpty)
                {
                    logAction?.Invoke($"(LOG-Margin) Warning: Source structure '{OrigStructure}' is empty (no volume). Setting target '{TargetStructure}' to empty volume.");
                    try
                    {
                        outStr.SegmentVolume = origStr.SegmentVolume;
                        SetStatusResult(true);
                        return true;
                    }
                    catch (Exception ex)
                    {
                        logAction?.Invoke($"(LOG-Margin) Error setting empty volume for '{TargetStructure}': {ex.Message}");
                        SetStatusResult(false);
                        return false;
                    }
                }

                var marginType = MarginGeometry == GeoType.Outer ? StructureMarginGeometry.Outer : StructureMarginGeometry.Inner;

                int x1 = ParseMargin(X1Text, 7);
                int x2 = ParseMargin(X2Text, 7);
                int y1 = ParseMargin(Y1Text, 7);
                int y2 = ParseMargin(Y2Text, 7);
                int z1 = ParseMargin(Z1Text, 7);
                int z2 = ParseMargin(Z2Text, 7);

                var margins = new AxisAlignedMargins(
                    marginType,
                    x1, // R
                    y1, // A
                    z1, // I
                    x2, // L
                    y2, // P
                    z2  // S
                );

                try
                {
                    outStr.SegmentVolume = origStr.AsymmetricMargin(margins);
                    logAction?.Invoke($"(LOG-Margin) Done: {TargetStructure}/{OrigStructure}/{MarginGeometry}({x1}/{x2}/{y1}/{y2}/{z1}/{z2})");
                    SetStatusResult(true);
                    return true;
                }
                catch (Exception ex)
                {
                    logAction?.Invoke($"(LOG-Margin) Error modifying '{TargetStructure}': {ex.Message}. (Note: The structure might be approved, locked, or used in an existing plan)");
                    SetStatusResult(false);
                    return false;
                }
            }
            else
            {
                logAction?.Invoke($"(LOG-Margin) No structure found: {TargetStructure}/{OrigStructure}");
                SetStatusResult(false);
                return false;
            }
        }

        private bool ExecuteHighRes(StructureSet structureSet, Action<string> logAction)
        {
            Structure structure = structureSet.Structures.FirstOrDefault(x => x.Id == TargetStructure);
            if (structure != null)
            {
                if (structure.CanConvertToHighResolution())
                {
                    structure.ConvertToHighResolution();
                    logAction?.Invoke($"(LOG-Hires) Done: {TargetStructure}");
                    SetStatusResult(true);
                    return true;
                }
                else
                {
                    logAction?.Invoke($"(LOG-Hires) Can not convert Hi.Res.: {TargetStructure}");
                    SetStatusResult(false);
                    return false;
                }
            }
            else
            {
                logAction?.Invoke($"(LOG-Hires) No structure found: {TargetStructure}");
                SetStatusResult(false);
                return false;
            }
        }

        #endregion

        #region シリアライズ & 相互変換

        public override TemplateStep ToTemplateStep()
        {
            TemplateStep step;
            switch (Category)
            {
                case OperationCategory.AddStructure:
                    step = new TemplateStep
                    {
                        StepNumber = StepNumber,
                        Type = "Add",
                        TargetStructure = TargetStructure,
                        DicomType = DicomType.ToString()
                    };
                    break;

                case OperationCategory.DeleteStructure:
                    step = new TemplateStep
                    {
                        StepNumber = StepNumber,
                        Type = "Del",
                        TargetStructure = TargetStructure
                    };
                    break;

                case OperationCategory.BooleanOperation:
                    step = new TemplateStep
                    {
                        StepNumber = StepNumber,
                        Type = "Boolean",
                        TargetStructure = TargetStructure,
                        BooleanOperation = BoolOpType.ToString(),
                        StructureA = StructureA,
                        StructureB = StructureB
                    };
                    break;

                case OperationCategory.Margin:
                    step = new TemplateStep
                    {
                        StepNumber = StepNumber,
                        Type = "Margin",
                        TargetStructure = TargetStructure,
                        OrigStructure = OrigStructure,
                        MarginGeometry = MarginGeometry.ToString(),
                        Margins = new MarginValues
                        {
                            X1 = ParseMargin(X1Text, 7),
                            X2 = ParseMargin(X2Text, 7),
                            Y1 = ParseMargin(Y1Text, 7),
                            Y2 = ParseMargin(Y2Text, 7),
                            Z1 = ParseMargin(Z1Text, 7),
                            Z2 = ParseMargin(Z2Text, 7)
                        }
                    };
                    break;

                case OperationCategory.ConvertHighRes:
                    step = new TemplateStep
                    {
                        StepNumber = StepNumber,
                        Type = "ConvertHighRes",
                        TargetStructure = TargetStructure
                    };
                    break;

                default:
                    step = new TemplateStep { StepNumber = StepNumber, Type = "Unknown", TargetStructure = TargetStructure };
                    break;
            }

            step.Enabled = IsEnabled;
            return step;
        }

        public override string ToCsvLine()
        {
            switch (Category)
            {
                case OperationCategory.AddStructure:
                    return $"AddDelControl,Add,{TargetStructure},{DicomType}";

                case OperationCategory.DeleteStructure:
                    return $"AddDelControl,Del,{TargetStructure}";

                case OperationCategory.BooleanOperation:
                    return $"BoolOpControl,{BoolOpType},{TargetStructure},{StructureA},{StructureB}";

                case OperationCategory.Margin:
                    int x1 = ParseMargin(X1Text, 7);
                    int x2 = ParseMargin(X2Text, 7);
                    int y1 = ParseMargin(Y1Text, 7);
                    int y2 = ParseMargin(Y2Text, 7);
                    int z1 = ParseMargin(Z1Text, 7);
                    int z2 = ParseMargin(Z2Text, 7);
                    return $"AddMarginControl,Asymmetry,{TargetStructure},{OrigStructure},{MarginGeometry},{x1},{x2},{y1},{y2},{z1},{z2}";

                case OperationCategory.ConvertHighRes:
                    return $"ConvertHighResControl,HiRes,{TargetStructure}";

                default:
                    return "";
            }
        }

        public static OperationStepViewModel CreateFromTemplateStep(
            TemplateStep step, 
            ObservableCollection<string> sharedStructures = null,
            ObservableCollection<StructureInfo> sharedStructureInfos = null,
            IDictionary<string, bool> resolutionMap = null)
        {
            if (step == null) return null;
            var vm = new OperationStepViewModel
            {
                StepNumber = step.StepNumber,
                TargetStructure = step.TargetStructure ?? "",
                IsEnabled = step.Enabled
            };

            switch (step.Type?.ToLowerInvariant())
            {
                case "add":
                    vm.Category = OperationCategory.AddStructure;
                    if (!string.IsNullOrEmpty(step.DicomType) && Enum.TryParse(step.DicomType, out DicomType dt))
                    {
                        vm.DicomType = dt;
                    }
                    break;

                case "del":
                    vm.Category = OperationCategory.DeleteStructure;
                    break;

                case "boolean":
                case "bool":
                    vm.Category = OperationCategory.BooleanOperation;
                    vm.StructureA = step.StructureA ?? "";
                    vm.StructureB = step.StructureB ?? "";
                    if (!string.IsNullOrEmpty(step.BooleanOperation) && Enum.TryParse(step.BooleanOperation, out BoolOpeType bt))
                    {
                        vm.BoolOpType = bt;
                    }
                    break;

                case "margin":
                    vm.Category = OperationCategory.Margin;
                    vm.OrigStructure = step.OrigStructure ?? "";
                    vm.MarginGeometry = string.Equals(step.MarginGeometry, "Inner", StringComparison.OrdinalIgnoreCase)
                        ? GeoType.Inner : GeoType.Outer;
                    if (step.Margins != null)
                    {
                        vm.X1Text = step.Margins.X1.ToString();
                        vm.X2Text = step.Margins.X2.ToString();
                        vm.Y1Text = step.Margins.Y1.ToString();
                        vm.Y2Text = step.Margins.Y2.ToString();
                        vm.Z1Text = step.Margins.Z1.ToString();
                        vm.Z2Text = step.Margins.Z2.ToString();
                        vm.MarginUniformText = vm.X1Text;
                    }
                    break;

                case "converthighres":
                case "hires":
                    vm.Category = OperationCategory.ConvertHighRes;
                    break;

                default:
                    vm.Category = OperationCategory.AddStructure;
                    break;
            }

            if (sharedStructures != null)
            {
                vm.AvailableStructures = sharedStructures;
            }
            if (sharedStructureInfos != null)
            {
                vm.AvailableStructureInfos = sharedStructureInfos;
            }
            if (resolutionMap != null)
            {
                vm.ResolutionMap = resolutionMap;
            }

            return vm;
        }

        #endregion
    }

    #region 後方互換クラス群（既存コード・テストとの互換性維持用）

    public class AddDelViewModel : OperationStepViewModel
    {
        public AddDelStrType OpType
        {
            get => Category == OperationCategory.DeleteStructure ? AddDelStrType.Del : AddDelStrType.Add;
            set => Category = value == AddDelStrType.Del ? OperationCategory.DeleteStructure : OperationCategory.AddStructure;
        }
        public bool IsAdd => OpType == AddDelStrType.Add;
    }

    public class BoolOpViewModel : OperationStepViewModel
    {
        public BoolOpViewModel()
        {
            Category = OperationCategory.BooleanOperation;
        }
        public BoolOpeType OpType
        {
            get => BoolOpType;
            set => BoolOpType = value;
        }
    }

    public class AddMarginViewModel : OperationStepViewModel
    {
        public AddMarginViewModel()
        {
            Category = OperationCategory.Margin;
        }
        public MarginType OpType
        {
            get => MarginType.Asymmetry;
            set { }
        }
        public GeoType Geometry
        {
            get => MarginGeometry;
            set => MarginGeometry = value;
        }
    }

    public class ConvertHighResViewModel : OperationStepViewModel
    {
        public ConvertHighResViewModel()
        {
            Category = OperationCategory.ConvertHighRes;
        }
        public ConvertType OpType
        {
            get => ConvertType;
            set => ConvertType = value;
        }
    }

    #endregion
}
