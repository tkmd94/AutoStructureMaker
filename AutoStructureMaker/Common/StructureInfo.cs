using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;

namespace AutoStructure.Common
{
    /// <summary>
    /// 輪郭のID、解像度、DICOMタイプ、および新規作成フラグ情報を保持するモデル
    /// インプレース同期時にプロパティ変更を UI（バッジ表示等）へ即座に通知します。
    /// </summary>
    public class StructureInfo : INotifyPropertyChanged
    {
        private string _id = "";
        private bool _isHighResolution;
        private string _dicomType = "";
        private bool _isNew;

        public event PropertyChangedEventHandler PropertyChanged;

        public string Id
        {
            get => _id;
            set
            {
                if (_id != value)
                {
                    _id = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsHighResolution
        {
            get => _isHighResolution;
            set
            {
                if (_isHighResolution != value)
                {
                    _isHighResolution = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(ResolutionBadgeText));
                    OnPropertyChanged(nameof(ResolutionBadgeBrush));
                }
            }
        }

        public string DicomType
        {
            get => _dicomType;
            set
            {
                if (_dicomType != value)
                {
                    _dicomType = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsNew
        {
            get => _isNew;
            set
            {
                if (_isNew != value)
                {
                    _isNew = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(ResolutionBadgeText));
                    OnPropertyChanged(nameof(ResolutionBadgeBrush));
                }
            }
        }

        public StructureInfo() { }

        public StructureInfo(string id, bool isHighResolution, string dicomType = "", bool isNew = false)
        {
            _id = id ?? "";
            _isHighResolution = isHighResolution;
            _dicomType = dicomType ?? "";
            _isNew = isNew;
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

        private static readonly Brush NewHighResBrush = CreateFrozenBrush(123, 31, 162);
        private static readonly Brush NewStdResBrush = CreateFrozenBrush(46, 125, 50);
        private static readonly Brush ExistingHighResBrush = CreateFrozenBrush(106, 27, 154);
        private static readonly Brush ExistingStdResBrush = CreateFrozenBrush(84, 110, 122);

        private static Brush CreateFrozenBrush(byte r, byte g, byte b)
        {
            var brush = new SolidColorBrush(Color.FromRgb(r, g, b));
            brush.Freeze();
            return brush;
        }

        public Brush ResolutionBadgeBrush
        {
            get
            {
                if (IsNew)
                {
                    return IsHighResolution ? NewHighResBrush : NewStdResBrush;
                }
                return IsHighResolution ? ExistingHighResBrush : ExistingStdResBrush;
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public override string ToString() => Id;
    }
}
