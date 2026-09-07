using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Xml.Serialization;
using Microsoft.Win32;
using AutoStructure.Common;
using VMS.TPS.Common.Model.API;

namespace AutoStructure.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private StructureSet _structureSet;
        private string _userId = "";
        private string _logText = "";
        private string _outputFolderPath;
        private string _protocolName = "Default_Protocol";
        private bool _isRefreshingContexts = false;

        private readonly List<StructureInfo> _baseStructureInfos = new List<StructureInfo>();

        public ObservableCollection<OperationItemViewModel> Operations { get; } = new ObservableCollection<OperationItemViewModel>();
        public ObservableCollection<string> AvailableStructures { get; } = new ObservableCollection<string>();
        public ObservableCollection<StructureInfo> AvailableStructureInfos { get; } = new ObservableCollection<StructureInfo>();
        public Dictionary<string, bool> StructureResolutionMap { get; } = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);

        public StructureSet StructureSet
        {
            get => _structureSet;
            set
            {
                if (SetProperty(ref _structureSet, value))
                {
                    UpdateAvailableStructures();
                }
            }
        }

        public string UserId
        {
            get => _userId;
            set => SetProperty(ref _userId, value);
        }

        public string LogText
        {
            get => _logText;
            set => SetProperty(ref _logText, value);
        }

        public string OutputFolderPath
        {
            get => _outputFolderPath;
            set => SetProperty(ref _outputFolderPath, value);
        }

        public string ProtocolName
        {
            get => _protocolName;
            set => SetProperty(ref _protocolName, value);
        }

        public bool HasOperations => Operations.Count > 0;

        public string OperationsSummaryText => $"{Operations.Count} step{(Operations.Count == 1 ? "" : "s")} configured";

        // Commands
        public ICommand RunCommand { get; }
        public ICommand ValidateCommand { get; }
        public ICommand AddStepCommand { get; }
        public ICommand AddDelCommand { get; }
        public ICommand AddBoolCommand { get; }
        public ICommand AddMarginCommand { get; }
        public ICommand AddHiResCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand LoadCommand { get; }
        public ICommand OpenHelpCommand { get; }

        public ICommand MoveUpCommand { get; }
        public ICommand MoveDownCommand { get; }
        public ICommand RemoveItemCommand { get; }
        public ICommand DuplicateStepCommand { get; }

        public event Action RequestScrollToEnd;

        public MainViewModel()
        {
            OutputFolderPath = AppConfig.GetInitialDirectory();

            RunCommand = new RelayCommand(ExecuteRun, () => HasOperations && StructureSet != null);
            ValidateCommand = new RelayCommand(ExecuteValidate, () => HasOperations);

            // 案B: 統合ステップ追加コマンド
            AddStepCommand = new RelayCommand(p =>
            {
                OperationCategory cat = OperationCategory.AddStructure;
                if (p is OperationCategory oc) cat = oc;
                else if (p is string s && Enum.TryParse(s, out OperationCategory parsedCat)) cat = parsedCat;

                var vm = new OperationStepViewModel
                {
                    Category = cat,
                    TargetStructure = ""
                };
                RegisterOperation(vm);
            });

            // 後方互換コマンド
            AddDelCommand = new RelayCommand(ExecuteAddDel);
            AddBoolCommand = new RelayCommand(ExecuteAddBool);
            AddMarginCommand = new RelayCommand(ExecuteAddMargin);
            AddHiResCommand = new RelayCommand(ExecuteAddHiRes);
            DeleteCommand = new RelayCommand(ExecuteDelete, () => HasOperations);
            SaveCommand = new RelayCommand(ExecuteSave, () => HasOperations);
            LoadCommand = new RelayCommand(ExecuteLoad);
            OpenHelpCommand = new RelayCommand(ExecuteOpenHelp);

            MoveUpCommand = new RelayCommand(
                p => ExecuteMoveUp(p as OperationItemViewModel),
                p => CanMoveUp(p as OperationItemViewModel)
            );
            MoveDownCommand = new RelayCommand(
                p => ExecuteMoveDown(p as OperationItemViewModel),
                p => CanMoveDown(p as OperationItemViewModel)
            );
            RemoveItemCommand = new RelayCommand(
                p => ExecuteRemoveItem(p as OperationItemViewModel),
                p => p is OperationItemViewModel
            );
            DuplicateStepCommand = new RelayCommand(
                p => ExecuteDuplicateStep(p as OperationItemViewModel),
                p => p is OperationItemViewModel
            );

            Operations.CollectionChanged += (s, e) =>
            {
                if (e.OldItems != null)
                {
                    foreach (OperationItemViewModel item in e.OldItems)
                    {
                        item.PropertyChanged -= OnOperationPropertyChanged;
                    }
                }
                if (e.NewItems != null)
                {
                    foreach (OperationItemViewModel item in e.NewItems)
                    {
                        item.PropertyChanged -= OnOperationPropertyChanged;
                        item.PropertyChanged += OnOperationPropertyChanged;
                    }
                }

                UpdateStepNumbers();
                RefreshStepStructureContexts();
                OnPropertyChanged(nameof(HasOperations));
                OnPropertyChanged(nameof(OperationsSummaryText));
                CommandManager.InvalidateRequerySuggested();
            };

            AvailableStructures.CollectionChanged += (s, e) =>
            {
                if (_isRefreshingContexts) return;
                if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add && e.NewItems != null)
                {
                    foreach (string strId in e.NewItems)
                    {
                        if (!string.IsNullOrWhiteSpace(strId) && !_baseStructureInfos.Any(x => string.Equals(x.Id, strId, StringComparison.OrdinalIgnoreCase)))
                        {
                            _baseStructureInfos.Add(new StructureInfo(strId, isHighResolution: false, dicomType: "CONTROL", isNew: false));
                        }
                    }
                    RefreshStepStructureContexts();
                }
            };
        }

        public void UpdateStepNumbers()
        {
            for (int i = 0; i < Operations.Count; i++)
            {
                Operations[i].StepNumber = i + 1;
            }
        }

        private bool CanMoveUp(OperationItemViewModel item)
        {
            if (item == null) return false;
            int index = Operations.IndexOf(item);
            return index > 0;
        }

        private bool CanMoveDown(OperationItemViewModel item)
        {
            if (item == null) return false;
            int index = Operations.IndexOf(item);
            return index >= 0 && index < Operations.Count - 1;
        }

        private void ExecuteMoveUp(OperationItemViewModel item)
        {
            if (item == null) return;
            int index = Operations.IndexOf(item);
            if (index > 0)
            {
                Operations.Move(index, index - 1);
                UpdateStepNumbers();
                RefreshStepStructureContexts();
                CommandManager.InvalidateRequerySuggested();
            }
        }

        private void ExecuteMoveDown(OperationItemViewModel item)
        {
            if (item == null) return;
            int index = Operations.IndexOf(item);
            if (index >= 0 && index < Operations.Count - 1)
            {
                Operations.Move(index, index + 1);
                UpdateStepNumbers();
                RefreshStepStructureContexts();
                CommandManager.InvalidateRequerySuggested();
            }
        }

        private void ExecuteRemoveItem(OperationItemViewModel item)
        {
            if (item == null) return;
            item.PropertyChanged -= OnOperationPropertyChanged;
            Operations.Remove(item);
            UpdateStepNumbers();
            RefreshStepStructureContexts();
            CommandManager.InvalidateRequerySuggested();
        }

        public void AppendLog(string message)
        {
            LogText += message + Environment.NewLine;
            RequestScrollToEnd?.Invoke();
        }

        public void UpdateAvailableStructures()
        {
            AvailableStructures.Clear();
            AvailableStructureInfos.Clear();
            StructureResolutionMap.Clear();
            _baseStructureInfos.Clear();

            if (StructureSet?.Structures != null)
            {
                foreach (var str in StructureSet.Structures)
                {
                    AvailableStructures.Add(str.Id);
                    var info = new StructureInfo(str.Id, str.IsHighResolution, str.DicomType, isNew: false);
                    AvailableStructureInfos.Add(info);
                    _baseStructureInfos.Add(new StructureInfo(str.Id, str.IsHighResolution, str.DicomType, isNew: false));
                    StructureResolutionMap[str.Id] = str.IsHighResolution;
                }
            }

            RefreshStepStructureContexts();
        }

        /// <summary>
        /// テストやスタンドアロン環境向けにベース輪郭（患者に元々存在する輪郭）を直接設定し、
        /// 各ステップへの輪郭伝播・解像度同期を実行します。
        /// </summary>
        public void SetBaseStructures(IEnumerable<StructureInfo> baseInfos)
        {
            AvailableStructures.Clear();
            AvailableStructureInfos.Clear();
            StructureResolutionMap.Clear();
            _baseStructureInfos.Clear();

            if (baseInfos != null)
            {
                foreach (var info in baseInfos)
                {
                    AvailableStructures.Add(info.Id);
                    AvailableStructureInfos.Add(info);
                    _baseStructureInfos.Add(new StructureInfo(info.Id, info.IsHighResolution, info.DicomType, isNew: false));
                    StructureResolutionMap[info.Id] = info.IsHighResolution;
                }
            }

            RefreshStepStructureContexts();
        }

        /// <summary>
        /// 先行ステップの作成予定輪郭を後続ステップのドロップダウン候補に自動追加・伝播し、
        /// 各ステップ時点での解像度マップをリアルタイムに同期・計算します。
        /// 同期処理中も UI の入力値（TargetStructure, OrigStructure 等）が消失しないよう多層保護します。
        /// </summary>
        public void RefreshStepStructureContexts()
        {
            if (_isRefreshingContexts) return;
            _isRefreshingContexts = true;

            try
            {
                // ベース輪郭ディクショナリを準備
                var currentContext = new Dictionary<string, StructureInfo>(StringComparer.OrdinalIgnoreCase);
                foreach (var baseInfo in _baseStructureInfos)
                {
                    currentContext[baseInfo.Id] = new StructureInfo(baseInfo.Id, baseInfo.IsHighResolution, baseInfo.DicomType, isNew: false);
                }

                for (int i = 0; i < Operations.Count; i++)
                {
                    var op = Operations[i];
                    op.IsSyncingContext = true;

                    // 同期直前のユーザー入力値を保護・退避
                    string savedTarget = op.TargetStructure;
                    var stepVm = op as OperationStepViewModel;
                    string savedOrig = stepVm?.OrigStructure;
                    string savedStrA = stepVm?.StructureA;
                    string savedStrB = stepVm?.StructureB;

                    // ステップ i 開始時点のコンテキストを要素単位で同期（参照は維持）
                    SyncCollection(op.AvailableStructures, currentContext.Keys);
                    SyncStructureInfos(op.AvailableStructureInfos, currentContext.Values);
                    op.ResolutionMap = currentContext.ToDictionary(k => k.Key, v => v.Value.IsHighResolution, StringComparer.OrdinalIgnoreCase);
                    op.NotifyResolutionChanged();

                    // WPF ComboBox の選択解除イベントによる予期せぬ空文字上書きを多層防御で復元
                    if (!string.IsNullOrEmpty(savedTarget) && string.IsNullOrEmpty(op.TargetStructure))
                    {
                        op.TargetStructure = savedTarget;
                    }
                    if (stepVm != null)
                    {
                        if (!string.IsNullOrEmpty(savedOrig) && string.IsNullOrEmpty(stepVm.OrigStructure))
                        {
                            stepVm.OrigStructure = savedOrig;
                        }
                        if (!string.IsNullOrEmpty(savedStrA) && string.IsNullOrEmpty(stepVm.StructureA))
                        {
                            stepVm.StructureA = savedStrA;
                        }
                        if (!string.IsNullOrEmpty(savedStrB) && string.IsNullOrEmpty(stepVm.StructureB))
                        {
                            stepVm.StructureB = savedStrB;
                        }
                    }

                    // ステップ i による輪郭の追加・変更・削除をシミュレート
                    ApplyStepToContext(op, currentContext);
                }
            }
            finally
            {
                _isRefreshingContexts = false;
                foreach (var op in Operations)
                {
                    op.IsSyncingContext = false;
                }
            }
        }

        private static void SyncCollection(ObservableCollection<string> target, IEnumerable<string> source)
        {
            if (target == null) return;
            var sourceList = source.ToList();
            if (target.SequenceEqual(sourceList, StringComparer.OrdinalIgnoreCase)) return;

            // 存在しなくなったものを末尾から削除
            for (int i = target.Count - 1; i >= 0; i--)
            {
                if (!sourceList.Contains(target[i], StringComparer.OrdinalIgnoreCase))
                {
                    target.RemoveAt(i);
                }
            }

            // 新規追加および順序合わせ
            for (int sIdx = 0; sIdx < sourceList.Count; sIdx++)
            {
                var sItem = sourceList[sIdx];
                int existingIdx = -1;
                for (int t = 0; t < target.Count; t++)
                {
                    if (string.Equals(target[t], sItem, StringComparison.OrdinalIgnoreCase))
                    {
                        existingIdx = t;
                        break;
                    }
                }

                if (existingIdx >= 0)
                {
                    if (existingIdx != sIdx && sIdx < target.Count)
                    {
                        target.Move(existingIdx, sIdx);
                    }
                }
                else
                {
                    if (sIdx < target.Count)
                    {
                        target.Insert(sIdx, sItem);
                    }
                    else
                    {
                        target.Add(sItem);
                    }
                }
            }
        }

        private static void SyncStructureInfos(ObservableCollection<StructureInfo> target, IEnumerable<StructureInfo> source)
        {
            if (target == null) return;
            var sourceList = source.ToList();

            // 1. 存在しなくなった項目を末尾から削除（ComboBoxの選択中要素を極力維持）
            for (int i = target.Count - 1; i >= 0; i--)
            {
                if (!sourceList.Any(s => string.Equals(s.Id, target[i].Id, StringComparison.OrdinalIgnoreCase)))
                {
                    target.RemoveAt(i);
                }
            }

            // 2. 既存項目はインスタンスを再利用してプロパティのみ更新、新規項目は追加
            for (int sIdx = 0; sIdx < sourceList.Count; sIdx++)
            {
                var sItem = sourceList[sIdx];
                var existing = target.FirstOrDefault(t => string.Equals(t.Id, sItem.Id, StringComparison.OrdinalIgnoreCase));
                if (existing != null)
                {
                    // インスタンスを維持したままプロパティのみ更新（ComboBox の選択解除を防止）
                    existing.IsHighResolution = sItem.IsHighResolution;
                    existing.DicomType = sItem.DicomType;
                    existing.IsNew = sItem.IsNew;

                    int currentIdx = target.IndexOf(existing);
                    if (currentIdx != sIdx && sIdx < target.Count)
                    {
                        target.Move(currentIdx, sIdx);
                    }
                }
                else
                {
                    var newItem = new StructureInfo(sItem.Id, sItem.IsHighResolution, sItem.DicomType, sItem.IsNew);
                    if (sIdx < target.Count)
                    {
                        target.Insert(sIdx, newItem);
                    }
                    else
                    {
                        target.Add(newItem);
                    }
                }
            }
        }

        private void ApplyStepToContext(OperationItemViewModel op, Dictionary<string, StructureInfo> context)
        {
            if (!op.IsEnabled) return; // 無効化されたステップは輪郭コンテキストに伝播させない

            if (op is OperationStepViewModel step)
            {
                string target = step.TargetStructure?.Trim();
                if (string.IsNullOrEmpty(target)) return;

                switch (step.Category)
                {
                    case OperationCategory.AddStructure:
                        // 新規作成された輪郭 (初期は Standard Resolution)
                        context[target] = new StructureInfo(target, isHighResolution: false, dicomType: step.DicomType.ToString(), isNew: true);
                        break;

                    case OperationCategory.DeleteStructure:
                        // 輪郭が削除される
                        context.Remove(target);
                        break;

                    case OperationCategory.ConvertHighRes:
                        // 対象輪郭が高解像度化される（新しいインスタンスで置き換え、スナップショットへの副作用を防ぐ）
                        if (context.TryGetValue(target, out var existingHi))
                        {
                            context[target] = new StructureInfo(target, isHighResolution: true, dicomType: existingHi.DicomType, isNew: existingHi.IsNew);
                        }
                        else
                        {
                            context[target] = new StructureInfo(target, isHighResolution: true, isNew: true);
                        }
                        break;

                    case OperationCategory.BooleanOperation:
                        // Boolean 演算結果
                        // A または B が High-Res の場合、Target は High-Res に自動昇格
                        bool aIsHigh = !string.IsNullOrEmpty(step.StructureA) &&
                                       context.TryGetValue(step.StructureA, out var aInfo) && aInfo.IsHighResolution;
                        bool bIsHigh = !string.IsNullOrEmpty(step.StructureB) &&
                                       context.TryGetValue(step.StructureB, out var bInfo) && bInfo.IsHighResolution;
                        bool willBeHigh = aIsHigh || bIsHigh;

                        if (context.TryGetValue(target, out var existingBool))
                        {
                            context[target] = new StructureInfo(target, isHighResolution: willBeHigh || existingBool.IsHighResolution, dicomType: existingBool.DicomType, isNew: existingBool.IsNew);
                        }
                        else
                        {
                            context[target] = new StructureInfo(target, isHighResolution: willBeHigh, isNew: true);
                        }
                        break;

                    case OperationCategory.Margin:
                        // Margin 演算結果
                        bool origIsHigh = !string.IsNullOrEmpty(step.OrigStructure) &&
                                          context.TryGetValue(step.OrigStructure, out var origInfo) && origInfo.IsHighResolution;

                        if (context.TryGetValue(target, out var existingMargin))
                        {
                            context[target] = new StructureInfo(target, isHighResolution: origIsHigh || existingMargin.IsHighResolution, dicomType: existingMargin.DicomType, isNew: existingMargin.IsNew);
                        }
                        else
                        {
                            context[target] = new StructureInfo(target, isHighResolution: origIsHigh, isNew: true);
                        }
                        break;
                }
            }
        }

        private void OnOperationPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (_isRefreshingContexts) return;

            if (e.PropertyName == nameof(OperationItemViewModel.TargetStructure) ||
                e.PropertyName == nameof(OperationItemViewModel.IsEnabled) ||
                e.PropertyName == nameof(OperationStepViewModel.Category) ||
                e.PropertyName == nameof(OperationStepViewModel.StructureA) ||
                e.PropertyName == nameof(OperationStepViewModel.StructureB) ||
                e.PropertyName == nameof(OperationStepViewModel.OrigStructure) ||
                e.PropertyName == nameof(OperationStepViewModel.DicomType))
            {
                RefreshStepStructureContexts();
            }
        }

        private void RegisterOperation(OperationItemViewModel op)
        {
            if (op == null) return;
            op.PropertyChanged -= OnOperationPropertyChanged;
            op.PropertyChanged += OnOperationPropertyChanged;
            Operations.Add(op);
            UpdateStepNumbers();
            RefreshStepStructureContexts();
        }

        private void ExecuteDuplicateStep(OperationItemViewModel item)
        {
            if (item == null) return;
            int index = Operations.IndexOf(item);
            if (index < 0) return;

            var templateStep = item.ToTemplateStep();
            var cloned = OperationItemViewModel.FromTemplateStep(templateStep, AvailableStructures, AvailableStructureInfos, StructureResolutionMap);
            if (cloned != null)
            {
                cloned.PropertyChanged += OnOperationPropertyChanged;
                Operations.Insert(index + 1, cloned);
                UpdateStepNumbers();
                RefreshStepStructureContexts();
                CommandManager.InvalidateRequerySuggested();
                AppendLog($"(INFO) Duplicated Step {item.StepNumber} -> inserted at Step {index + 2}");
            }
        }

        public void ApplyValidationResultToOperations(ValidationResult result)
        {
            if (result == null || Operations == null) return;

            var issuesByStep = result.Issues.GroupBy(x => x.StepNumber).ToDictionary(g => g.Key, g => g.ToList());

            for (int i = 0; i < Operations.Count; i++)
            {
                var op = Operations[i];
                int stepNum = op.StepNumber > 0 ? op.StepNumber : (i + 1);

                if (!op.IsEnabled)
                {
                    op.SetStatusSkipped();
                    op.StatusToolTip = "Step is disabled (skipped)";
                    continue;
                }

                if (issuesByStep.TryGetValue(stepNum, out var issues) && issues.Count > 0)
                {
                    var firstError = issues.FirstOrDefault(x => x.IsError);
                    if (firstError != null)
                    {
                        op.Status = "Error";
                        op.StatusToolTip = $"[Error] {firstError.Message}";
                    }
                    else
                    {
                        var firstWarn = issues.First();
                        op.Status = "Warn";
                        op.StatusToolTip = $"[Warning] {firstWarn.Message}";
                    }
                }
                else
                {
                    op.Status = "OK";
                    op.StatusToolTip = "Validation passed: Ready to execute";
                }
            }
        }

        private void ExecuteValidate()
        {
            var existingIds = StructureSet?.Structures?.Select(s => s.Id).ToList() ?? AvailableStructures.ToList();
            var result = PreFlightValidator.Validate(Operations, existingIds);

            // 各ステップのステータスに検証結果を反映
            ApplyValidationResultToOperations(result);

            AppendLog($"--- Pre-Flight Validation ({DateTime.Now:HH:mm:ss}) ---");
            foreach (var issue in result.Issues)
            {
                AppendLog(issue.ToString());
            }
            AppendLog($"--- {result.SummaryText} ---");

            if (result.IsValid)
            {
                string msg = result.Warnings.Count > 0
                    ? $"Validation passed with {result.Warnings.Count} warning(s):\n\n" + string.Join("\n", result.Warnings.Take(5).Select(w => w.ToString()))
                    : "Validation passed successfully!\nNo issues found.";
                var icon = result.Warnings.Count > 0 ? MessageBoxImage.Warning : MessageBoxImage.Information;
                MessageBox.Show(msg, "Pre-Flight Validation", MessageBoxButton.OK, icon);
            }
            else
            {
                string msg = $"Validation failed with {result.Errors.Count} error(s):\n\n" +
                             string.Join("\n", result.Errors.Take(6).Select(e => e.ToString())) +
                             (result.Errors.Count > 6 ? $"\n... and {result.Errors.Count - 6} more." : "");
                MessageBox.Show(msg, "Validation Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecuteAddDel()
        {
            var dicomType = DicomType.PTV;
            Enum.TryParse(AppConfig.Current.DefaultDicomType, out dicomType);

            var vm = new OperationStepViewModel
            {
                Category = OperationCategory.AddStructure,
                DicomType = dicomType,
                TargetStructure = ""
            };
            RegisterOperation(vm);
        }

        private void ExecuteAddBool()
        {
            var boolType = BoolOpeType.SUB;
            Enum.TryParse(AppConfig.Current.DefaultBoolOperation, out boolType);

            var vm = new OperationStepViewModel
            {
                Category = OperationCategory.BooleanOperation,
                BoolOpType = boolType,
                TargetStructure = "",
                StructureA = "",
                StructureB = ""
            };
            RegisterOperation(vm);
        }

        private void ExecuteAddMargin()
        {
            var s = AppConfig.Current;
            var geo = string.Equals(s.DefaultMarginGeometry, "Inner", StringComparison.OrdinalIgnoreCase) 
                ? GeoType.Inner 
                : GeoType.Outer;

            var vm = new OperationStepViewModel
            {
                Category = OperationCategory.Margin,
                MarginGeometry = geo,
                TargetStructure = "",
                OrigStructure = "",
                X1Text = s.DefaultMarginX1.ToString(),
                X2Text = s.DefaultMarginX2.ToString(),
                Y1Text = s.DefaultMarginY1.ToString(),
                Y2Text = s.DefaultMarginY2.ToString(),
                Z1Text = s.DefaultMarginZ1.ToString(),
                Z2Text = s.DefaultMarginZ2.ToString(),
                MarginUniformText = s.DefaultMarginX1.ToString()
            };
            RegisterOperation(vm);
        }

        private void ExecuteAddHiRes()
        {
            var vm = new OperationStepViewModel
            {
                Category = OperationCategory.ConvertHighRes,
                TargetStructure = ""
            };
            RegisterOperation(vm);
        }

        private void ExecuteDelete()
        {
            if (Operations.Count > 0)
            {
                var last = Operations[Operations.Count - 1];
                last.PropertyChanged -= OnOperationPropertyChanged;
                Operations.RemoveAt(Operations.Count - 1);
                RefreshStepStructureContexts();
            }
        }

        private void ExecuteRun()
        {
            if (StructureSet == null)
            {
                MessageBox.Show("No structureSet is loaded.");
                return;
            }

            if (Operations.Count == 0)
            {
                MessageBox.Show("No operation found.");
                return;
            }

            // 実行前 Pre-Flight 検証
            var existingIds = StructureSet.Structures?.Select(s => s.Id).ToList() ?? AvailableStructures.ToList();
            var valResult = PreFlightValidator.Validate(Operations, existingIds);
            ApplyValidationResultToOperations(valResult);
            if (!valResult.IsValid)
            {
                AppendLog($"(ERROR) Pre-Flight Validation failed with {valResult.Errors.Count} error(s):");
                foreach (var err in valResult.Errors)
                {
                    AppendLog(err.ToString());
                }

                var confirm = MessageBox.Show(
                    $"Pre-Flight Validation detected {valResult.Errors.Count} error(s):\n\n" +
                    string.Join("\n", valResult.Errors.Take(4).Select(e => e.ToString())) +
                    (valResult.Errors.Count > 4 ? $"\n... and {valResult.Errors.Count - 4} more." : "") +
                    "\n\nDo you want to proceed with execution anyway?",
                    "Pre-Flight Validation Warning",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning
                );
                if (confirm != MessageBoxResult.Yes)
                {
                    AppendLog("Execution aborted by user.");
                    return;
                }
            }

            try
            {
                AppendLog($"=== Execution Started: {DateTime.Now:yyyy-MM-dd HH:mm:ss} ===");
                StructureSet.Patient.BeginModifications();

                // 実行前に全ステップのステータスをリセット
                foreach (var op in Operations)
                {
                    op.ResetStatus();
                }

                int successCount = 0;
                int failCount = 0;
                int skipCount = 0;

                foreach (var op in Operations)
                {
                    if (!op.IsEnabled)
                    {
                        op.SetStatusSkipped();
                        string catName = (op as OperationStepViewModel)?.Category.ToString() ?? "Step";
                        AppendLog($"(SKIP) Step {op.StepNumber} [{catName}] is disabled.");
                        skipCount++;
                        continue;
                    }

                    bool result = op.Execute(StructureSet, AppendLog);
                    if (result)
                    {
                        successCount++;
                    }
                    else
                    {
                        failCount++;
                    }
                }

                // 書き込み型処理後の最新輪郭一覧をマスターリストへ即時同期
                UpdateAvailableStructures();

                AppendLog($"=== Execution Finished: {successCount} Succeeded, {failCount} Failed, {skipCount} Skipped ===");

                string msg = $"Execution finished.\nSucceeded: {successCount}, Failed: {failCount}, Skipped: {skipCount}";
                var icon = failCount == 0 ? MessageBoxImage.Information : MessageBoxImage.Warning;
                MessageBox.Show(msg, "Execution Result", MessageBoxButton.OK, icon);
            }
            catch (Exception ex)
            {
                AppendLog($"(ERROR) Critical exception during execution: {ex.Message}");
                MessageBox.Show($"Error: {ex.Message}", "Execution Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecuteSave()
        {
            if (Operations.Count == 0)
            {
                MessageBox.Show("No parameter found.");
                AppendLog("No parameter found.");
                return;
            }

            var prefix = AppConfig.Current.SaveFileNamePrefix ?? "case_";
            var defaultBaseName = $"{prefix}{UserId}_{DateTime.Now:yyyyMMddHHmmss}";
            var sfd = new SaveFileDialog
            {
                FileName = $"{defaultBaseName}.xml",
                InitialDirectory = OutputFolderPath,
                Filter = "XML Template (*.xml)|*.xml|Legacy CSV (*.csv)|*.csv|All Files (*.*)|*.*",
                FilterIndex = 1,
                Title = "Save as",
                RestoreDirectory = true,
                OverwritePrompt = true,
                CheckPathExists = true
            };

            if (sfd.ShowDialog() == true)
            {
                try
                {
                    string ext = Path.GetExtension(sfd.FileName)?.ToLowerInvariant();
                    bool asCsv = (ext == ".csv");

                    string pName = !string.IsNullOrWhiteSpace(ProtocolName) 
                        ? ProtocolName 
                        : Path.GetFileNameWithoutExtension(sfd.FileName);

                    var template = TemplateService.CreateTemplate(
                        Operations,
                        protocolName: pName,
                        author: UserId ?? "",
                        description: "Generated by AutoStructureMaker"
                    );

                    TemplateService.SaveTemplate(sfd.FileName, template, asCsv);

                    OutputFolderPath = Path.GetDirectoryName(sfd.FileName);
                    AppendLog($"Save parameter.\nPath:{sfd.FileName}");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to save file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ExecuteLoad()
        {
            var ofd = new OpenFileDialog
            {
                InitialDirectory = OutputFolderPath,
                Filter = "Structure Template (*.xml;*.csv)|*.xml;*.csv|XML Template (*.xml)|*.xml|Legacy CSV (*.csv)|*.csv|All Files (*.*)|*.*",
                FilterIndex = 1,
                Title = "Open",
                CheckFileExists = true,
                CheckPathExists = true,
                RestoreDirectory = true
            };

            if (ofd.ShowDialog() == true)
            {
                try
                {
                    var template = TemplateService.LoadTemplate(ofd.FileName);
                    if (template?.Steps == null) return;

                    if (!string.IsNullOrWhiteSpace(template.ProtocolName))
                    {
                        ProtocolName = template.ProtocolName;
                    }

                    foreach (var oldOp in Operations)
                    {
                        oldOp.PropertyChanged -= OnOperationPropertyChanged;
                    }
                    Operations.Clear();

                    foreach (var step in template.Steps)
                    {
                        var op = OperationItemViewModel.FromTemplateStep(step, AvailableStructures, AvailableStructureInfos, StructureResolutionMap);
                        if (op != null)
                        {
                            op.PropertyChanged += OnOperationPropertyChanged;
                            Operations.Add(op);
                        }
                    }

                    UpdateStepNumbers();
                    RefreshStepStructureContexts();

                    OutputFolderPath = Path.GetDirectoryName(ofd.FileName);
                    AppendLog($"Load parameter ({template.Steps.Count} steps loaded).\nPath:{ofd.FileName}");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to load file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        public void ExecuteOpenHelp()
        {
            try
            {
                // 1. 実行アセンブリ周辺およびプロジェクト内の PDF ファイルを探索
                string assemblyLoc = null;
                try
                {
                    assemblyLoc = Assembly.GetExecutingAssembly().Location;
                }
                catch { }

                string assemblyDir = !string.IsNullOrEmpty(assemblyLoc) ? Path.GetDirectoryName(assemblyLoc) : AppDomain.CurrentDomain.BaseDirectory;

                var candidatePaths = new List<string>
                {
                    Path.Combine(assemblyDir, "AutoStructureMaker_Manual.pdf"),
                    Path.Combine(assemblyDir, "docs", "AutoStructureMaker_Manual.pdf"),
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "AutoStructureMaker_Manual.pdf"),
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "docs", "AutoStructureMaker_Manual.pdf")
                };

                foreach (var path in candidatePaths)
                {
                    if (File.Exists(path))
                    {
                        Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
                        AppendLog($"(HELP) Opened manual: {path}");
                        return;
                    }
                }

                // 2. ディスク上に見つからない場合、アセンブリ内に埋め込まれたリソース (EmbeddedResource) から %TEMP% に展開
                var assembly = Assembly.GetExecutingAssembly();
                string resourceName = assembly.GetManifestResourceNames()
                    .FirstOrDefault(n => n.EndsWith("AutoStructureMaker_Manual.pdf", StringComparison.OrdinalIgnoreCase)
                                      || n.EndsWith("Manual.pdf", StringComparison.OrdinalIgnoreCase));

                if (resourceName != null)
                {
                    string tempPdfPath = Path.Combine(Path.GetTempPath(), "AutoStructureMaker_Manual.pdf");
                    using (var stream = assembly.GetManifestResourceStream(resourceName))
                    {
                        if (stream != null)
                        {
                            using (var fs = new FileStream(tempPdfPath, FileMode.Create, FileAccess.Write, FileShare.Read))
                            {
                                stream.CopyTo(fs);
                            }
                            Process.Start(new ProcessStartInfo(tempPdfPath) { UseShellExecute = true });
                            AppendLog($"(HELP) Extracted and opened embedded manual: {tempPdfPath}");
                            return;
                        }
                    }
                }

                MessageBox.Show(
                    "マニュアル PDF が見つかりませんでした。\nファイルが存在するか、またはリソースが正しく埋め込まれているか確認してください。",
                    "AutoStructureMaker Help",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"マニュアルを開く際にエラーが発生しました:\n{ex.Message}",
                    "AutoStructureMaker Help Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }
    }
}
