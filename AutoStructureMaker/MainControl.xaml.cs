using System.Windows.Controls;
using AutoStructure.ViewModels;
using VMS.TPS.Common.Model.API;

namespace AutoStructure
{
    /// <summary>
    /// MainControl.xaml の相互作用ロジック
    /// </summary>
    public partial class MainControl : UserControl
    {
        public MainViewModel ViewModel { get; }

        public StructureSet structureSet
        {
            get => ViewModel.StructureSet;
            set => ViewModel.StructureSet = value;
        }

        public string UserId
        {
            get => ViewModel.UserId;
            set => ViewModel.UserId = value;
        }

        public MainControl()
        {
            InitializeComponent();
            ViewModel = new MainViewModel();
            DataContext = ViewModel;

            ViewModel.RequestScrollToEnd += () =>
            {
                logTextBox.SelectionStart = logTextBox.Text.Length;
                logTextBox.ScrollToEnd();
            };
        }
    }
}
