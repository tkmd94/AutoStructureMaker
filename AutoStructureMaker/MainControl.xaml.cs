using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;

namespace AutoStructure
{
    /// <summary>
    /// MainControl.xaml
    /// </summary>
    public partial class MainControl : UserControl
    {
        public ObservableCollection<ParameterList> parameterList;
        public string outputFolderPath;

        public StructureSet structureSet;
        public string UserId;


        public MainControl()
        {
            InitializeComponent();

            // Sets the initial directory for saving and loading parameter files.
            outputFolderPath = @"\\172.16.10.181\va_transfer\MLC\--- ESAPI ---\AutoStructure\";
            //outputFolderPath = System.Environment.GetEnvironmentVariable("TEMP") + @"\";

        }

        /// <summary>
        /// loadAddDelControl
        /// </summary>
        /// <param name="structures"></param>
        private void loadAddDelControl(StructureSet structureSet, int index, string OutputName, string dicomTypeName)
        {
            UC_AddDel AddDelControl = new UC_AddDel();
            AddDelControl.Name = "AddDelControl";

            AddDelControl.OpCtrlComboBox.Name = "AddDelControl_OpCtrlComboBox";
            AddDelControl.OpCtrlComboBox.SelectedIndex = index;
            AddDelControl.OpCtrlComboBox.DropDownClosed += new EventHandler(AddDelControl_OpCtrlComboBox_DropDownClosed);

            AddDelControl.statusTextBox.Name = "AddDelControl_statusTextBox";
            AddDelControl.OutputNameComboBox.Name = "AddDelControl_OutputNameComboBox";
            foreach (var structure in structureSet.Structures)
            {
                AddDelControl.OutputNameComboBox.Items.Add(structure.Id);
            }
            int oNameIndex = Array.IndexOf(structureSet.Structures.Select(x => x.Id).ToArray(), OutputName);
            if (oNameIndex >= 0)
            {
                AddDelControl.OutputNameComboBox.SelectedIndex = oNameIndex;
            }
            else
            {
                AddDelControl.OutputNameComboBox.Text = OutputName;
            }
            int dicomTypeIndex = Array.IndexOf(Enum.GetNames(typeof(DicomType)), dicomTypeName);
            AddDelControl.dicomTypeComboBox.Name = "AddDelControl_DicomTypeComboBox";
            AddDelControl.dicomTypeComboBox.SelectedIndex = dicomTypeIndex;

            if (dicomTypeIndex < 0)
                AddDelControl.dicomTypeComboBox.IsEnabled = false;

            stackPanel_Tab.Children.Add(AddDelControl);
            runButton.IsEnabled = true;
            saveParaButton.IsEnabled = true;
        }

        /// <summary>
        /// AddDelControl_OpCtrlComboBox_DropDownClosed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddDelControl_OpCtrlComboBox_DropDownClosed(object sender, System.EventArgs e)
        {
            string AddDel_OpType = "";
            ShowLogMsg("AddDelControl_OpCtrlComboBox_DropDownClosed");
            WPFControlUtility.OperateLogicalChildren(stackPanel_Tab, t =>
            {
                string contName = "";
                Control cont = t as Control;
                if (cont != null && cont.Name.Length > 0)
                {
                    contName = cont.Name;
                }
                if (contName != "")
                {
                    if (contName == "AddDelControl_OpCtrlComboBox")
                    {
                        AddDel_OpType = cont.GetValue(ComboBox.SelectedValueProperty).ToString();
                        ShowLogMsg("get AddDelControl_OpCtrlComboBox:" + AddDel_OpType);
                    }
                    if (contName == "AddDelControl_DicomTypeComboBox")
                    {
                        ShowLogMsg("get AddDelControl_DicomTypeComboBox");
                        if (AddDel_OpType == "Del")
                        {
                            ShowLogMsg("del");
                            cont.SetValue(ComboBox.IsEnabledProperty, false);
                            cont.SetValue(ComboBox.SelectedIndexProperty, -1);
                        }
                        if (AddDel_OpType == "Add")
                        {
                            ShowLogMsg("add");
                            cont.SetValue(ComboBox.IsEnabledProperty, true);
                            cont.SetValue(ComboBox.SelectedIndexProperty, 0);
                        }
                    }
                }
                int depth = WPFControlUtility.GetDepthInLogicalTree(t);
            });
        }

        /// <summary>
        /// loadBoolOpControl
        /// </summary>
        /// <param name="structures"></param>
        private void loadBoolOpControl(StructureSet structureSet, int index, string OutputName, string strA, string strB)
        {
            int oNameIndex;
            UC_BoolOp BoolOpControl = new UC_BoolOp();
            BoolOpControl.Name = "BoolOpControl";

            BoolOpControl.OpCtrlComboBox.Name = "BoolOpControl_OpCtrlComboBox";
            BoolOpControl.OpCtrlComboBox.SelectedIndex = index;

            BoolOpControl.statusTextBox.Name = "BoolOpControl_statusTextBox";

            BoolOpControl.OutputNameComboBox.Name = "BoolOpControl_OutputNameComboBox";
            foreach (var structure in structureSet.Structures)
            {
                BoolOpControl.OutputNameComboBox.Items.Add(structure.Id);
            }
            oNameIndex = Array.IndexOf(structureSet.Structures.Select(x => x.Id).ToArray(), OutputName);
            if (oNameIndex >= 0)
            {
                BoolOpControl.OutputNameComboBox.SelectedIndex = oNameIndex;
            }
            else
            {
                BoolOpControl.OutputNameComboBox.Text = OutputName;
            }

            BoolOpControl.strA_ComboBox.Name = "BoolOpControl_strA_ComboBox";
            foreach (var structure in structureSet.Structures)
            {
                BoolOpControl.strA_ComboBox.Items.Add(structure.Id);
            }
            oNameIndex = Array.IndexOf(structureSet.Structures.Select(x => x.Id).ToArray(), strA);
            if (oNameIndex >= 0)
            {
                BoolOpControl.strA_ComboBox.SelectedIndex = oNameIndex;
            }
            else
            {
                BoolOpControl.strA_ComboBox.Text = strA;
            }

            BoolOpControl.strB_ComboBox.Name = "BoolOpControl_strB_ComboBox";
            foreach (var structure in structureSet.Structures)
            {
                BoolOpControl.strB_ComboBox.Items.Add(structure.Id);
            }
            oNameIndex = Array.IndexOf(structureSet.Structures.Select(x => x.Id).ToArray(), strB);
            if (oNameIndex >= 0)
            {
                BoolOpControl.strB_ComboBox.SelectedIndex = oNameIndex;
            }
            else
            {
                BoolOpControl.strB_ComboBox.Text = strB;
            }

            stackPanel_Tab.Children.Add(BoolOpControl);
            runButton.IsEnabled = true;
            saveParaButton.IsEnabled = true;
        }

        /// <summary>
        /// loadAddMarginControl
        /// </summary>
        /// <param name="structures"></param>
        private void loadAddMarginControl(StructureSet structureSet, int index, string OutputName, string OrigStrName, int geoIndex,
            int X1, int X2, int Y1, int Y2, int Z1, int Z2)
        {
            int oNameIndex;
            UC_AddMargin AddMarginControl = new UC_AddMargin();
            AddMarginControl.Name = "AddMarginControl";
            AddMarginControl.OpCtrlComboBox.Name = "AddMarginControl_OpCtrlComboBox";
            AddMarginControl.OpCtrlComboBox.SelectedIndex = index;
            AddMarginControl.statusTextBox.Name = "AddMarginControl_statusTextBox";
            AddMarginControl.OutputNameComboBox.Name = "AddMarginControl_OutputNameComboBox";

            foreach (var structure in structureSet.Structures)
            {
                AddMarginControl.OutputNameComboBox.Items.Add(structure.Id);
            }
            oNameIndex = Array.IndexOf(structureSet.Structures.Select(x => x.Id).ToArray(), OutputName);
            if (oNameIndex >= 0)
            {
                AddMarginControl.OutputNameComboBox.SelectedIndex = oNameIndex;
            }
            else
            {
                AddMarginControl.OutputNameComboBox.Text = OutputName;
            }

            AddMarginControl.origStrComboBox.Name = "AddMarginControl_origStrComboBox";
            foreach (var structure in structureSet.Structures)
            {
                AddMarginControl.origStrComboBox.Items.Add(structure.Id);
            }
            oNameIndex = Array.IndexOf(structureSet.Structures.Select(x => x.Id).ToArray(), OrigStrName);
            if (oNameIndex >= 0)
            {
                AddMarginControl.origStrComboBox.SelectedIndex = oNameIndex;
            }
            else
            {
                AddMarginControl.origStrComboBox.Text = OrigStrName;
            }

            AddMarginControl.geoComboBox.Name = "AddMarginControl_geoComboBox";
            AddMarginControl.geoComboBox.SelectedIndex = geoIndex;

            AddMarginControl.marginX1TextBox.Name = "AddMarginControl_marginX1TextBox";
            AddMarginControl.marginX1TextBox.Text = X1.ToString();
            AddMarginControl.marginX2TextBox.Name = "AddMarginControl_marginX2TextBox";
            AddMarginControl.marginX2TextBox.Text = X2.ToString();
            AddMarginControl.marginY1TextBox.Name = "AddMarginControl_marginY1TextBox";
            AddMarginControl.marginY1TextBox.Text = Y1.ToString();
            AddMarginControl.marginY2TextBox.Name = "AddMarginControl_marginY2TextBox";
            AddMarginControl.marginY2TextBox.Text = Y2.ToString();
            AddMarginControl.marginZ1TextBox.Name = "AddMarginControl_marginZ1TextBox";
            AddMarginControl.marginZ1TextBox.Text = Z1.ToString();
            AddMarginControl.marginZ2TextBox.Name = "AddMarginControl_marginZ2TextBox";
            AddMarginControl.marginZ2TextBox.Text = Z2.ToString();

            stackPanel_Tab.Children.Add(AddMarginControl);
            runButton.IsEnabled = true;
            saveParaButton.IsEnabled = true;
        }

        /// <summary>
        /// loadConvertHighResControl
        /// </summary>
        /// <param name="structures"></param>
        private void loadConvertHighResControl(StructureSet structureSet, int index, string OutputName)
        {
            UC_ConvertHighRes ConvertHighResControl = new UC_ConvertHighRes();
            ConvertHighResControl.Name = "ConvertHighResControl";
            ConvertHighResControl.OpCtrlComboBox.Name = "ConvertHighResControl_OpCtrlComboBox";
            ConvertHighResControl.OpCtrlComboBox.SelectedIndex = index;

            ConvertHighResControl.statusTextBox.Name = "ConvertHighResControl_statusTextBox";

            ConvertHighResControl.OutputNameComboBox.Name = "ConvertHighResControl_OutputNameComboBox";
            foreach (var structure in structureSet.Structures)
            {
                ConvertHighResControl.OutputNameComboBox.Items.Add(structure.Id);
            }

            int oNameIndex = Array.IndexOf(structureSet.Structures.Select(x => x.Id).ToArray(), OutputName);
            if (oNameIndex >= 0)
            {
                ConvertHighResControl.OutputNameComboBox.SelectedIndex = oNameIndex;
            }
            else
            {
                ConvertHighResControl.OutputNameComboBox.Text = OutputName;
            }

            stackPanel_Tab.Children.Add(ConvertHighResControl);
            runButton.IsEnabled = true;
            saveParaButton.IsEnabled = true;
        }

        /// <summary>
        /// checkParameter
        /// </summary>
        /// <returns></returns>
        public ObservableCollection<ParameterList> checkParameter()
        {

            parameterList = new ObservableCollection<ParameterList>();

            string OpType = "";
            string AddDel_OpType = "";
            string AddDel_Status = "";
            string AddDel_OutputName = "";
            string AddDel_DicomType = "";

            string Bool_OpType = "";
            string Bool_Status = "";
            string Bool_OutputName = "";
            string Bool_StrA = "";
            string Bool_StrB = "";

            string Margin_OpType = "";
            string Margin_Status = "";
            string Margin_OutputName = "";
            string Margin_OrigName = "";
            string Margin_geoType = "";
            string Margin_X1 = "";
            string Margin_X2 = "";
            string Margin_Y1 = "";
            string Margin_Y2 = "";
            string Margin_Z1 = "";
            string Margin_Z2 = "";

            string Hires_OpType = "";
            string Hires_Status = "";
            string Hires_OutputName = "";

            int nCtrl = 0;

            WPFControlUtility.OperateLogicalChildren(stackPanel_Tab, t =>
            {
                String contName = "";
                Control cont = t as Control;
                if (cont != null && cont.Name.Length > 0)
                {
                    contName = cont.Name;
                }
                if (contName != "")
                {
                    //AddDelControl
                    if (contName == "AddDelControl")
                    {
                        if (nCtrl > 0)
                        {
                            parameterList.Add(new ParameterList
                            {
                                OpType = OpType,

                                AddDel_OpType = AddDel_OpType,
                                AddDel_Status = AddDel_Status,
                                AddDel_OutputName = AddDel_OutputName,
                                AddDel_DicomType = AddDel_DicomType,

                                Bool_OpType = Bool_OpType,
                                Bool_Status = Bool_Status,
                                Bool_OutputName = Bool_OutputName,
                                Bool_StrA = Bool_StrA,
                                Bool_StrB = Bool_StrB,

                                Margin_OpType = Margin_OpType,
                                Margin_Status = Margin_Status,
                                Margin_OutputName = Margin_OutputName,
                                Margin_OrigName = Margin_OrigName,
                                Margin_geoType = Margin_geoType,
                                Margin_X1 = Margin_X1,
                                Margin_X2 = Margin_X2,
                                Margin_Y1 = Margin_Y1,
                                Margin_Y2 = Margin_Y2,
                                Margin_Z1 = Margin_Z1,
                                Margin_Z2 = Margin_Z2,

                                Hires_OpType = Hires_OpType,
                                Hires_Status = Hires_Status,
                                Hires_OutputName = Hires_OutputName,
                            });
                            OpType = "";
                            AddDel_OpType = "";
                            AddDel_Status = "";
                            AddDel_OutputName = "";
                            AddDel_DicomType = "";


                            Bool_OpType = "";
                            Bool_Status = "";
                            Bool_OutputName = "";
                            Bool_StrA = "";
                            Bool_StrB = "";

                            Margin_OpType = "";
                            Margin_Status = "";
                            Margin_OutputName = "";
                            Margin_OrigName = "";
                            Margin_geoType = "";
                            Margin_X1 = "";
                            Margin_X2 = "";
                            Margin_Y1 = "";
                            Margin_Y2 = "";
                            Margin_Z1 = "";
                            Margin_Z2 = "";

                            Hires_OpType = "";
                            Hires_Status = "";
                            Hires_OutputName = "";
                        }
                        OpType = contName;
                        nCtrl++;
                    }
                    if (contName == "AddDelControl_OpCtrlComboBox")
                        AddDel_OpType = cont.GetValue(ComboBox.SelectedValueProperty).ToString();
                    if (contName == "AddDelControl_statusTextBox")
                        AddDel_Status = cont.GetValue(TextBox.TextProperty).ToString();
                    if (contName == "AddDelControl_OutputNameComboBox")
                        AddDel_OutputName = cont.GetValue(ComboBox.TextProperty).ToString();
                    if (contName == "AddDelControl_DicomTypeComboBox")
                        AddDel_DicomType = cont.GetValue(ComboBox.TextProperty).ToString();

                    //BoolOpControl
                    if (contName == "BoolOpControl")
                    {
                        if (nCtrl > 0)
                        {
                            parameterList.Add(new ParameterList
                            {
                                OpType = OpType,

                                AddDel_OpType = AddDel_OpType,
                                AddDel_Status = AddDel_Status,
                                AddDel_OutputName = AddDel_OutputName,
                                AddDel_DicomType = AddDel_DicomType,

                                Bool_OpType = Bool_OpType,
                                Bool_Status = Bool_Status,
                                Bool_OutputName = Bool_OutputName,
                                Bool_StrA = Bool_StrA,
                                Bool_StrB = Bool_StrB,

                                Margin_OpType = Margin_OpType,
                                Margin_Status = Margin_Status,
                                Margin_OutputName = Margin_OutputName,
                                Margin_OrigName = Margin_OrigName,
                                Margin_geoType = Margin_geoType,
                                Margin_X1 = Margin_X1,
                                Margin_X2 = Margin_X2,
                                Margin_Y1 = Margin_Y1,
                                Margin_Y2 = Margin_Y2,
                                Margin_Z1 = Margin_Z1,
                                Margin_Z2 = Margin_Z2,

                                Hires_OpType = Hires_OpType,
                                Hires_Status = Hires_Status,
                                Hires_OutputName = Hires_OutputName,
                            });
                            OpType = "";
                            AddDel_OpType = "";
                            AddDel_Status = "";
                            AddDel_OutputName = "";
                            AddDel_DicomType = "";

                            Bool_OpType = "";
                            Bool_Status = "";
                            Bool_OutputName = "";
                            Bool_StrA = "";
                            Bool_StrB = "";

                            Margin_OpType = "";
                            Margin_Status = "";
                            Margin_OutputName = "";
                            Margin_OrigName = "";
                            Margin_geoType = "";
                            Margin_X1 = "";
                            Margin_X2 = "";
                            Margin_Y1 = "";
                            Margin_Y2 = "";
                            Margin_Z1 = "";
                            Margin_Z2 = "";

                            Hires_OpType = "";
                            Hires_Status = "";
                            Hires_OutputName = "";
                        }
                        OpType = contName;
                        nCtrl++;
                    }
                    if (contName == "BoolOpControl_OpCtrlComboBox")
                        Bool_OpType = cont.GetValue(ComboBox.SelectedValueProperty).ToString();
                    if (contName == "BoolOpControl_statusTextBox")
                        Bool_Status = cont.GetValue(TextBox.TextProperty).ToString();
                    if (contName == "BoolOpControl_OutputNameComboBox")
                        Bool_OutputName = cont.GetValue(ComboBox.TextProperty).ToString();
                    if (contName == "BoolOpControl_strA_ComboBox")
                        Bool_StrA = cont.GetValue(ComboBox.TextProperty).ToString();
                    if (contName == "BoolOpControl_strB_ComboBox")
                        Bool_StrB = cont.GetValue(ComboBox.TextProperty).ToString();

                    //AddMarginControl
                    if (contName == "AddMarginControl")
                    {
                        if (nCtrl > 0)
                        {
                            parameterList.Add(new ParameterList
                            {
                                OpType = OpType,

                                AddDel_OpType = AddDel_OpType,
                                AddDel_Status = AddDel_Status,
                                AddDel_OutputName = AddDel_OutputName,
                                AddDel_DicomType = AddDel_DicomType,

                                Bool_OpType = Bool_OpType,
                                Bool_Status = Bool_Status,
                                Bool_OutputName = Bool_OutputName,
                                Bool_StrA = Bool_StrA,
                                Bool_StrB = Bool_StrB,

                                Margin_OpType = Margin_OpType,
                                Margin_Status = Margin_Status,
                                Margin_OutputName = Margin_OutputName,
                                Margin_OrigName = Margin_OrigName,
                                Margin_geoType = Margin_geoType,
                                Margin_X1 = Margin_X1,
                                Margin_X2 = Margin_X2,
                                Margin_Y1 = Margin_Y1,
                                Margin_Y2 = Margin_Y2,
                                Margin_Z1 = Margin_Z1,
                                Margin_Z2 = Margin_Z2,

                                Hires_OpType = Hires_OpType,
                                Hires_Status = Hires_Status,
                                Hires_OutputName = Hires_OutputName,
                            });
                            OpType = "";
                            AddDel_OpType = "";
                            AddDel_Status = "";
                            AddDel_OutputName = "";
                            AddDel_DicomType = "";

                            Bool_OpType = "";
                            Bool_Status = "";
                            Bool_OutputName = "";
                            Bool_StrA = "";
                            Bool_StrB = "";

                            Margin_OpType = "";
                            Margin_Status = "";
                            Margin_OutputName = "";
                            Margin_OrigName = "";
                            Margin_geoType = "";
                            Margin_X1 = "";
                            Margin_X2 = "";
                            Margin_Y1 = "";
                            Margin_Y2 = "";
                            Margin_Z1 = "";
                            Margin_Z2 = "";

                            Hires_OpType = "";
                            Hires_Status = "";
                            Hires_OutputName = "";
                        }
                        OpType = contName;
                        nCtrl++;
                    }
                    if (contName == "AddMarginControl_OpCtrlComboBox")
                        Margin_OpType = cont.GetValue(ComboBox.SelectedValueProperty).ToString();
                    if (contName == "AddMarginControl_statusTextBox")
                        Margin_Status = cont.GetValue(TextBox.TextProperty).ToString();
                    if (contName == "AddMarginControl_OutputNameComboBox")
                        Margin_OutputName = cont.GetValue(ComboBox.TextProperty).ToString();
                    if (contName == "AddMarginControl_origStrComboBox")
                        Margin_OrigName = cont.GetValue(ComboBox.TextProperty).ToString();
                    if (contName == "AddMarginControl_geoComboBox")
                        Margin_geoType = cont.GetValue(ComboBox.TextProperty).ToString();
                    if (contName == "AddMarginControl_marginX1TextBox")
                        Margin_X1 = cont.GetValue(TextBox.TextProperty).ToString();
                    if (contName == "AddMarginControl_marginX2TextBox")
                        Margin_X2 = cont.GetValue(TextBox.TextProperty).ToString();
                    if (contName == "AddMarginControl_marginY1TextBox")
                        Margin_Y1 = cont.GetValue(TextBox.TextProperty).ToString();
                    if (contName == "AddMarginControl_marginY2TextBox")
                        Margin_Y2 = cont.GetValue(TextBox.TextProperty).ToString();
                    if (contName == "AddMarginControl_marginZ1TextBox")
                        Margin_Z1 = cont.GetValue(TextBox.TextProperty).ToString();
                    if (contName == "AddMarginControl_marginZ2TextBox")
                        Margin_Z2 = cont.GetValue(TextBox.TextProperty).ToString();

                    //ConvertHighResControl
                    if (contName == "ConvertHighResControl")
                    {
                        if (nCtrl > 0)
                        {
                            parameterList.Add(new ParameterList
                            {
                                OpType = OpType,

                                AddDel_OpType = AddDel_OpType,
                                AddDel_Status = AddDel_Status,
                                AddDel_OutputName = AddDel_OutputName,
                                AddDel_DicomType = AddDel_DicomType,

                                Bool_OpType = Bool_OpType,
                                Bool_Status = Bool_Status,
                                Bool_OutputName = Bool_OutputName,
                                Bool_StrA = Bool_StrA,
                                Bool_StrB = Bool_StrB,

                                Margin_OpType = Margin_OpType,
                                Margin_Status = Margin_Status,
                                Margin_OutputName = Margin_OutputName,
                                Margin_OrigName = Margin_OrigName,
                                Margin_geoType = Margin_geoType,
                                Margin_X1 = Margin_X1,
                                Margin_X2 = Margin_X2,
                                Margin_Y1 = Margin_Y1,
                                Margin_Y2 = Margin_Y2,
                                Margin_Z1 = Margin_Z1,
                                Margin_Z2 = Margin_Z2,

                                Hires_OpType = Hires_OpType,
                                Hires_Status = Hires_Status,
                                Hires_OutputName = Hires_OutputName,
                            });
                            OpType = "";
                            AddDel_OpType = "";
                            AddDel_Status = "";
                            AddDel_OutputName = "";
                            AddDel_DicomType = "";

                            Bool_OpType = "";
                            Bool_Status = "";
                            Bool_OutputName = "";
                            Bool_StrA = "";
                            Bool_StrB = "";

                            Margin_OpType = "";
                            Margin_Status = "";
                            Margin_OutputName = "";
                            Margin_OrigName = "";
                            Margin_geoType = "";
                            Margin_X1 = "";
                            Margin_X2 = "";
                            Margin_Y1 = "";
                            Margin_Y2 = "";
                            Margin_Z1 = "";
                            Margin_Z2 = "";

                            Hires_OpType = "";
                            Hires_Status = "";
                            Hires_OutputName = "";
                        }
                        OpType = contName;
                        nCtrl++;
                    }
                    if (contName == "ConvertHighResControl_OpCtrlComboBox")
                        Hires_OpType = cont.GetValue(ComboBox.SelectedValueProperty).ToString();
                    if (contName == "ConvertHighResControl_statusTextBox")
                        Hires_Status = cont.GetValue(TextBox.TextProperty).ToString();
                    if (contName == "ConvertHighResControl_OutputNameComboBox")
                        Hires_OutputName = cont.GetValue(ComboBox.TextProperty).ToString();

                }
                int depth = WPFControlUtility.GetDepthInLogicalTree(t);
            });

            parameterList.Add(new ParameterList
            {
                OpType = OpType,

                AddDel_OpType = AddDel_OpType,
                AddDel_Status = AddDel_Status,
                AddDel_OutputName = AddDel_OutputName,
                AddDel_DicomType = AddDel_DicomType,

                Bool_OpType = Bool_OpType,
                Bool_Status = Bool_Status,
                Bool_OutputName = Bool_OutputName,
                Bool_StrA = Bool_StrA,
                Bool_StrB = Bool_StrB,

                Margin_OpType = Margin_OpType,
                Margin_Status = Margin_Status,
                Margin_OutputName = Margin_OutputName,
                Margin_OrigName = Margin_OrigName,
                Margin_geoType = Margin_geoType,
                Margin_X1 = Margin_X1,
                Margin_X2 = Margin_X2,
                Margin_Y1 = Margin_Y1,
                Margin_Y2 = Margin_Y2,
                Margin_Z1 = Margin_Z1,
                Margin_Z2 = Margin_Z2,

                Hires_OpType = Hires_OpType,
                Hires_Status = Hires_Status,
                Hires_OutputName = Hires_OutputName,
            });

            return parameterList;
        }

        /// <summary>
        /// addDelControlButton_Click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void addDelControlButton_Click(object sender, RoutedEventArgs e)
        {
            loadAddDelControl(structureSet, 0, "", "PTV");
        }

        /// <summary>
        /// boolOpControlButton_Click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void boolOpControlButton_Click(object sender, RoutedEventArgs e)
        {
            loadBoolOpControl(structureSet, 0, "", "", "");
        }

        /// <summary>
        /// AddMarginControlButton_Click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddMarginControlButton_Click(object sender, RoutedEventArgs e)
        {
            loadAddMarginControl(structureSet, 0, "", "", 1, 7, 7, 7, 7, 7, 7);
        }

        /// <summary>
        /// convertHighResControlButton_Click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void convertHighResControlButton_Click(object sender, RoutedEventArgs e)
        {
            loadConvertHighResControl(structureSet, 0, "");
        }

        /// <summary>
        /// delButton_Click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void delButton_Click(object sender, RoutedEventArgs e)
        {
            int count = stackPanel_Tab.Children.Count;
            if (stackPanel_Tab.Children.Count > 0)
            {
                stackPanel_Tab.Children.RemoveAt(count - 1);
            }

            if (stackPanel_Tab.Children.Count == 0)
            {
                runButton.IsEnabled = false;
                saveParaButton.IsEnabled = false;
            }
        }

        /// <summary>
        /// process_AddDel
        /// </summary>
        /// <param name="AddDel_OpType"></param>
        /// <param name="AddDel_OutputName"></param>
        /// <returns></returns>
        private bool process_AddDel(string AddDel_OpType, string AddDel_OutputName, string dicomTypeName)
        {

            Structure structure = structureSet.Structures.FirstOrDefault(x => x.Id == AddDel_OutputName);
            if (AddDel_OpType == "Add")
            {
                if (structure == null)
                {
                    if (structureSet.CanAddStructure(dicomTypeName.ToString(), AddDel_OutputName))
                    {
                        structure = structureSet.AddStructure(dicomTypeName.ToString(), AddDel_OutputName);
                        ShowLogMsg("(LOG-Add/Del) Add new structure: " + AddDel_OutputName);
                        return true;
                    }
                    else
                    {
                        ShowLogMsg("(LOG-Add/Del) Can not add new structure: " + AddDel_OutputName);
                        return false;
                    }
                }
                else
                {
                    ShowLogMsg("(LOG-Add/Del) Exist structure: " + AddDel_OutputName);
                    return false;
                }
            }
            else if (AddDel_OpType == "Del")
            {
                if (structure == null)
                {
                    ShowLogMsg("(LOG-Add/Del) No structure found: " + AddDel_OutputName);
                    return false;
                }
                else
                {
                    if (structureSet.CanRemoveStructure(structure))
                    {
                        structureSet.RemoveStructure(structure);
                        ShowLogMsg("(LOG-Add/Del) Delete structure: " + AddDel_OutputName);
                        return true;
                    }
                    else
                    {
                        ShowLogMsg("(LOG-Add/Del) Can not remove structure: " + AddDel_OutputName);
                        return false;
                    }
                }
            }
            else
            {
                ShowLogMsg("(LOG-Add/Del) Invalid  Add/Del ope. : " + AddDel_OutputName);
                return false;
            }
        }

        /// <summary>
        /// process_Bool
        /// </summary>
        /// <param name="Bool_OpType"></param>
        /// <param name="Bool_OutputName"></param>
        /// <param name="Bool_StrA"></param>
        /// <param name="Bool_StrB"></param>
        /// <returns></returns>
        private bool process_Bool(string Bool_OpType, string Bool_OutputName, string Bool_StrA, string Bool_StrB)
        {

            Structure oStr = structureSet.Structures.FirstOrDefault(x => x.Id == Bool_OutputName);
            Structure strA = structureSet.Structures.FirstOrDefault(x => x.Id == Bool_StrA);
            Structure strB = structureSet.Structures.FirstOrDefault(x => x.Id == Bool_StrB);


            if (oStr != null &&
                strA != null &&
                strB != null &&
                oStr.IsHighResolution == strA.IsHighResolution &&
                strA.IsHighResolution == strB.IsHighResolution)
            {
                if (Bool_OpType == "SUB")
                {
                    oStr.SegmentVolume = strA.Sub(strB);
                    ShowLogMsg("(LOG-Bool) Done: " + Bool_OpType + ":" + Bool_OutputName + "/" + Bool_StrA + "/" + Bool_StrB);
                    return true;
                }
                else if (Bool_OpType == "AND")
                {
                    oStr.SegmentVolume = strA.And(strB);
                    ShowLogMsg("(LOG-Bool) Done: " + Bool_OpType + ":" + Bool_OutputName + "/" + Bool_StrA + "/" + Bool_StrB);
                    return true;
                }
                else if (Bool_OpType == "OR")
                {
                    oStr.SegmentVolume = strA.Or(strB);
                    ShowLogMsg("(LOG-Bool) Done: " + Bool_OpType + ":" + Bool_OutputName + "/" + Bool_StrA + "/" + Bool_StrB);
                    return true;
                }
                else if (Bool_OpType == "XOR")
                {
                    oStr.SegmentVolume = strA.Xor(strB);
                    ShowLogMsg("(LOG-Bool) Done: " + Bool_OpType + ":" + Bool_OutputName + "/" + Bool_StrA + "/" + Bool_StrB);
                    return true;
                }
                else
                {
                    ShowLogMsg("(LOG-Bool) Invalid  Bool ope. : " + Bool_OutputName + "/" + Bool_StrA + "/" + Bool_StrB);
                    return false;
                }
            }
            else
            {
                ShowLogMsg("(LOG-Bool) No structure found/Resoution mismatch: " + Bool_OutputName + "/" + Bool_StrA + "/" + Bool_StrB);
                return false;
            }
        }

        /// <summary>
        /// process_Margin
        /// </summary>
        /// <param name="Margin_OpType"></param>
        /// <param name="Margin_OutputName"></param>
        /// <param name="Margin_OrigName"></param>
        /// <param name="Margin_geoType"></param>
        /// <param name="marginValues"></param>
        /// <returns></returns>
        private bool process_Margin(string Margin_OpType, string Margin_OutputName, string Margin_OrigName, string Margin_geoType, int[] marginValues)
        {

            Structure outStr = structureSet.Structures.FirstOrDefault(x => x.Id == Margin_OutputName);
            Structure origStr = structureSet.Structures.FirstOrDefault(x => x.Id == Margin_OrigName);

            if (Margin_OpType == "Asymmetry")
            {
                if (outStr != null && origStr != null)
                {
                    var marginType = StructureMarginGeometry.Outer;
                    if (Margin_geoType == "Outer")
                    {
                        marginType = StructureMarginGeometry.Outer;
                    }
                    else if (Margin_geoType == "Inner")
                    {
                        marginType = StructureMarginGeometry.Inner;
                    }
                    else
                    {
                        ShowLogMsg("(LOG-Margin) Invalid margin geometry: " + outStr + "/" + origStr);
                        return false;
                    }
                    AxisAlignedMargins margins =
                        new AxisAlignedMargins(
                            marginType, //outer or inner
                            marginValues[0], //R:x1:negative X axis in mm
                            marginValues[2], //A:y1:negative Y axis in mm
                            marginValues[4], //I:z1:negative Z axis in mm
                            marginValues[1], //L:x2:positive X axis in mm
                            marginValues[3], //P:y2:positive Y axis in mm
                            marginValues[5]);//S:z2:positive Z axis in mm

                    outStr.SegmentVolume = origStr.AsymmetricMargin(margins);
                    ShowLogMsg("(LOG-Margin) Done: " + outStr + "/" +
                        origStr + "/" + Margin_geoType + "(" + string.Join("/", marginValues) + ")");
                    return true;
                }
                else
                {
                    ShowLogMsg("(LOG-Margin) No structure found: " + outStr + "/" + origStr);
                    return false;
                }
            }
            else
            {
                return false;
            }

        }

        /// <summary>
        /// process_Hires
        /// </summary>
        /// <param name="Hires_OpType"></param>
        /// <param name="Hires_OutputName"></param>
        /// <returns></returns>
        private bool process_Hires(string Hires_OpType, string Hires_OutputName)
        {

            Structure structure = structureSet.Structures.FirstOrDefault(x => x.Id == Hires_OutputName);
            if (Hires_OpType == "HiRes")
            {
                if (structure != null)
                {
                    if (structure.CanConvertToHighResolution())
                    {

                        structure.ConvertToHighResolution();
                        ShowLogMsg("(LOG-Hires) Done: " + Hires_OutputName);
                        return true;
                    }
                    else
                    {
                        ShowLogMsg("(LOG-Hires) Can not convert Hi.Res.: " + Hires_OutputName);
                        return false;
                    }
                }
                else
                {
                    ShowLogMsg("(LOG-Hires) No structure found: " + Hires_OutputName);
                    return false;
                }
            }
            else
            {
                ShowLogMsg("(LOG-Hires) Invalid  Hi.Res ope. : " + Hires_OutputName);
                return false;
            }

        }

        /// <summary>
        /// runButton_Click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void runButton_Click(object sender, RoutedEventArgs e)
        {
            parameterList = checkParameter();

            if (parameterList != null)
            {
                structureSet.Patient.BeginModifications();   // enable writing with this script.

                int count = 0;
                foreach (var row in parameterList)
                {
                    if (row.OpType == "AddDelControl")
                    {
                        bool status = process_AddDel(row.AddDel_OpType, row.AddDel_OutputName, row.AddDel_DicomType);
                        int statusCount = 0;
                        WPFControlUtility.OperateLogicalChildren(stackPanel_Tab, t =>
                        {
                            String contName = "";
                            Control cont = t as Control;
                            if (cont != null && cont.Name.Length > 0)
                            {
                                contName = cont.Name;
                            }
                            if (contName != "")
                            {
                                string[] text = contName.Split('_');
                                if (text.Count() == 2)
                                {
                                    if (text[1] == "statusTextBox")
                                    {
                                        if (count == statusCount)
                                        {
                                            if (status == true)
                                            {
                                                cont.SetValue(TextBox.TextProperty, "Done");
                                                cont.SetValue(TextBox.BackgroundProperty, Brushes.LightGreen);
                                            }
                                            else
                                            {
                                                cont.SetValue(TextBox.TextProperty, "Fail");
                                                cont.SetValue(TextBox.BackgroundProperty, Brushes.Red);
                                            }
                                        }
                                        statusCount++;
                                    }
                                }
                            }
                            int depth = WPFControlUtility.GetDepthInLogicalTree(t);
                        });
                    }
                    else if (row.OpType == "BoolOpControl")
                    {
                        bool status = process_Bool(row.Bool_OpType, row.Bool_OutputName, row.Bool_StrA, row.Bool_StrB);
                        int statusCount = 0;
                        WPFControlUtility.OperateLogicalChildren(stackPanel_Tab, t =>
                        {
                            String contName = "";
                            Control cont = t as Control;
                            if (cont != null && cont.Name.Length > 0)
                            {
                                contName = cont.Name;
                            }
                            if (contName != "")
                            {
                                string[] text = contName.Split('_');
                                if (text.Count() == 2)
                                {
                                    if (text[1] == "statusTextBox")
                                    {
                                        if (count == statusCount)
                                        {
                                            if (status == true)
                                            {
                                                cont.SetValue(TextBox.TextProperty, "Done");
                                                cont.SetValue(TextBox.BackgroundProperty, Brushes.LightGreen);
                                            }
                                            else
                                            {
                                                cont.SetValue(TextBox.TextProperty, "Fail");
                                                cont.SetValue(TextBox.BackgroundProperty, Brushes.Red);
                                            }
                                        }
                                        statusCount++;
                                    }
                                }
                            }
                            int depth = WPFControlUtility.GetDepthInLogicalTree(t);
                        });
                    }
                    else if (row.OpType == "AddMarginControl")
                    {
                        int[] marginValues ={
                            int.Parse(row.Margin_X1),
                            int.Parse(row.Margin_X2),
                            int.Parse(row.Margin_Y1),
                            int.Parse(row.Margin_Y2),
                            int.Parse(row.Margin_Z1),
                            int.Parse(row.Margin_Z2)};
                        bool status = process_Margin(row.Margin_OpType, row.Margin_OutputName, row.Margin_OrigName, row.Margin_geoType, marginValues);
                        int statusCount = 0;
                        WPFControlUtility.OperateLogicalChildren(stackPanel_Tab, t =>
                        {
                            String contName = "";
                            Control cont = t as Control;
                            if (cont != null && cont.Name.Length > 0)
                            {
                                contName = cont.Name;
                            }
                            if (contName != "")
                            {
                                string[] text = contName.Split('_');
                                if (text.Count() == 2)
                                {
                                    if (text[1] == "statusTextBox")
                                    {
                                        if (count == statusCount)
                                        {
                                            if (status == true)
                                            {
                                                cont.SetValue(TextBox.TextProperty, "Done");
                                                cont.SetValue(TextBox.BackgroundProperty, Brushes.LightGreen);
                                            }
                                            else
                                            {
                                                cont.SetValue(TextBox.TextProperty, "Fail");
                                                cont.SetValue(TextBox.BackgroundProperty, Brushes.Red);
                                            }
                                        }
                                        statusCount++;
                                    }
                                }
                            }
                            int depth = WPFControlUtility.GetDepthInLogicalTree(t);
                        });
                    }
                    else if (row.OpType == "ConvertHighResControl")
                    {
                        bool status = process_Hires(row.Hires_OpType, row.Hires_OutputName);
                        int statusCount = 0;
                        WPFControlUtility.OperateLogicalChildren(stackPanel_Tab, t =>
                        {
                            String contName = "";
                            Control cont = t as Control;
                            if (cont != null && cont.Name.Length > 0)
                            {
                                contName = cont.Name;
                            }
                            if (contName != "")
                            {
                                string[] text = contName.Split('_');
                                if (text.Count() == 2)
                                {
                                    if (text[1] == "statusTextBox")
                                    {
                                        if (count == statusCount)
                                        {
                                            if (status == true)
                                            {
                                                cont.SetValue(TextBox.TextProperty, "Done");
                                                cont.SetValue(TextBox.BackgroundProperty, Brushes.LightGreen);
                                            }
                                            else
                                            {
                                                cont.SetValue(TextBox.TextProperty, "Fail");
                                                cont.SetValue(TextBox.BackgroundProperty, Brushes.Red);
                                            }
                                        }
                                        statusCount++;
                                    }
                                }
                            }
                            int depth = WPFControlUtility.GetDepthInLogicalTree(t);
                        });
                    }
                    count++;
                }
            }
            ShowLogMsg("Done");
            MessageBox.Show("Done.");
        }

        /// <summary>
        /// loadParaButton_Click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void loadParaButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.InitialDirectory = outputFolderPath;
            ofd.Filter = "CSV(*.csv)|*.csv|All Files(*.*)|*.*";
            ofd.FilterIndex = 1;
            ofd.Title = "Open";
            ofd.CheckFileExists = true;
            ofd.CheckPathExists = true;
            ofd.RestoreDirectory = true;

            Nullable<bool> result = ofd.ShowDialog();
            if (result == true)
            {
                int count = stackPanel_Tab.Children.Count;
                while (count != 0)
                {
                    if (stackPanel_Tab.Children.Count > 0)
                    {
                        stackPanel_Tab.Children.RemoveAt(count - 1);
                    }
                    count = stackPanel_Tab.Children.Count;
                }
                if (stackPanel_Tab.Children.Count == 0)
                {
                    runButton.IsEnabled = false;
                    saveParaButton.IsEnabled = false;
                }

                using (var sr = new StreamReader(ofd.FileName))
                {
                    while (!sr.EndOfStream)
                    {
                        var line = sr.ReadLine();
                        var values = line.Split(',');
                        if (values[0] == "AddDelControl" &&
                            (values.Count() == 3 || values.Count() == 4))
                        {
                            int index;
                            string dicomTypeName = "";
                            if (values[1] == "Add")
                            {
                                index = 0;
                                dicomTypeName = values[3];
                            }
                            else if (values[1] == "Del")
                            {
                                index = 1;
                            }
                            else
                            {
                                index = -1;
                            }
                            string outputName = values[2];
                            loadAddDelControl(structureSet, index, outputName, dicomTypeName);
                        }
                        if (values[0] == "ConvertHighResControl" && values.Count() == 3)
                        {
                            int index;
                            if (values[1] == "HiRes")
                            {
                                index = 0;
                            }
                            else
                            {
                                index = -1;
                            }
                            string outputName = values[2];
                            loadConvertHighResControl(structureSet, index, outputName);
                        }
                        if (values[0] == "BoolOpControl" && values.Count() == 5)
                        {
                            int index;
                            if (values[1] == "SUB")
                            {
                                index = 0;
                            }
                            else if (values[1] == "AND")
                            {
                                index = 1;
                            }
                            else if (values[1] == "OR")
                            {
                                index = 2;
                            }
                            else if (values[1] == "XOR")
                            {
                                index = 3;
                            }
                            else
                            {
                                index = -1;
                            }
                            string outputName = values[2];
                            string strA = values[3];
                            string strB = values[4];

                            loadBoolOpControl(structureSet, index, outputName, strA, strB);
                        }
                        if (values[0] == "AddMarginControl" && values.Count() == 11)
                        {
                            int index;
                            if (values[1] == "Asymmetry")
                            {
                                index = 0;
                            }
                            else
                            {
                                index = -1;
                            }
                            string outputName = values[2];
                            string OrigStrName = values[3];
                            int geoIndex = -1;
                            if (values[4] == "Inner")
                            {
                                geoIndex = 0;
                            }
                            else if (values[4] == "Outer")
                            {
                                geoIndex = 1;
                            }
                            else
                            {
                            }
                            int X1 = int.Parse(values[5]);
                            int X2 = int.Parse(values[6]);
                            int Y1 = int.Parse(values[7]);
                            int Y2 = int.Parse(values[8]);
                            int Z1 = int.Parse(values[9]);
                            int Z2 = int.Parse(values[10]);

                            loadAddMarginControl(structureSet, index, outputName, OrigStrName, geoIndex, X1, X2, Y1, Y2, Z1, Z2);
                        }
                    }
                    parameterList = new ObservableCollection<ParameterList>();
                }
                outputFolderPath = System.IO.Path.GetDirectoryName(ofd.FileName);
                ShowLogMsg("Load parameter.\n" + "Path:" + ofd.FileName);
            }
        }

        /// <summary>
        /// saveParaButton_Click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void saveParaButton_Click(object sender, RoutedEventArgs e)
        {
            parameterList = checkParameter();
            var nrow = parameterList.Count();
            if (nrow == 0)
            {
                MessageBox.Show("No parameter found.");
                ShowLogMsg("No parameter found.");
            }
            else
            {
                //Create instance of SaveFileDialog class 
                SaveFileDialog sfd = new SaveFileDialog();

                DateTime dt = DateTime.Now;
                string datetext = dt.ToString("yyyyMMddHHmmss");
                sfd.FileName = "case_" + UserId + "_" + datetext + ".csv";
                sfd.InitialDirectory = outputFolderPath;
                sfd.Filter = "CSV(*.csv)|*.csv|All Files(*.*)|*.*";
                sfd.FilterIndex = 1;
                sfd.Title = "Save as";
                sfd.RestoreDirectory = true;
                sfd.OverwritePrompt = true;
                sfd.CheckPathExists = true;

                Nullable<bool> result = sfd.ShowDialog();
                if (result == true)
                {
                    string filepath = "";

                    int count = 0;
                    using (StreamWriter saveFile = new StreamWriter(sfd.FileName, false))
                    {
                        filepath = sfd.FileName;
                        string headerText = "";
                        foreach (var row in parameterList)
                        {
                            PropertyInfo[] infoArray = row.GetType().GetProperties();
                            string OpType = "";

                            foreach (PropertyInfo info in infoArray)
                            {
                                if (info.Name == "OpType")
                                {
                                    if (info.GetValue(row, null).ToString() == "AddDelControl")
                                        OpType = "AddDel";
                                    if (info.GetValue(row, null).ToString() == "BoolOpControl")
                                        OpType = "Bool";
                                    if (info.GetValue(row, null).ToString() == "AddMarginControl")
                                        OpType = "Margin";
                                    if (info.GetValue(row, null).ToString() == "ConvertHighResControl")
                                        OpType = "Hires";

                                    if (count == 0)
                                    {
                                        headerText += info.GetValue(row, null).ToString();
                                    }
                                    else
                                    {
                                        saveFile.WriteLine(headerText);
                                        headerText = info.GetValue(row, null).ToString();
                                    }
                                    count++;
                                }
                                else
                                {
                                    string[] text = info.Name.ToString().Split('_');
                                    if (text.Count() == 2)
                                    {
                                        // skip status information
                                        if (text[0] == OpType && text[1] != "Status")
                                        {
                                            headerText += "," + info.GetValue(row, null).ToString();
                                        }
                                    }
                                }
                            }
                        }
                        saveFile.WriteLine(headerText);
                        saveFile.Flush();

                    }
                    outputFolderPath = System.IO.Path.GetDirectoryName(sfd.FileName);
                    ShowLogMsg("Save parameter.\n" + "Path:" + sfd.FileName);
                }
            }
        }

        /// <summary>
        /// ShowLogMsg
        /// </summary>
        /// <param name="dataFile"></param>
        private void ShowLogMsg(string text)
        {
            logTextBox.AppendText(text + "\n");
            logTextBox.SelectionStart = logTextBox.Text.Length;
            logTextBox.ScrollToEnd();
        }

    }
}
