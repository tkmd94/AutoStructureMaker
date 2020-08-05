using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;

namespace AutoStructure
{
    public class WPFControlUtility
    {

        public static void OperateLogicalChildren(DependencyObject target, Action<DependencyObject> action)
        {
            action(target);
            foreach (var child in LogicalTreeHelper.GetChildren(target))
            {
                if (child is DependencyObject)
                {
                    OperateLogicalChildren((DependencyObject)child, action);
                }
            }
        }

        /// <summary>Returns the hierarchy in the target's logic tree</summary>
        public static int GetDepthInLogicalTree(DependencyObject target)
        {
            DependencyObject parent = LogicalTreeHelper.GetParent(target);
            int depth = 0;
            while (parent != null)
            {
                depth++;
                parent = LogicalTreeHelper.GetParent(parent);
            }
            return depth;
        }

        /// <summary>
        /// Output visual elements
        /// </summary>
        /// <param name="tgt"></param>
        public static void ShowVisualControls(DependencyObject tgt)
        {
            ShowVisualControl(tgt);
        }

        private static void ShowVisualControl(DependencyObject tgt, int position = 0)
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(tgt); i++)
            {
                DependencyObject childObj = VisualTreeHelper.GetChild(tgt, i);
                // log output
                ShowLog(childObj, position);

                if (VisualTreeHelper.GetChildrenCount(childObj) > 0)
                {
                    int nextPosition = position + 1;
                    ShowVisualControl(childObj, nextPosition);
                }
            }
        }

        /// <summary>
        /// Output logical elements
        /// </summary>
        /// <param name="tgt"></param>
        public static void ShowLogicalControls(DependencyObject tgt)
        {
            ShowLogicalControl(tgt);
        }

        private static void ShowLogicalControl(DependencyObject tgt, int position = 0)
        {
            if (tgt == null) return;

            ShowLog(tgt, position);
            foreach (var ctl in LogicalTreeHelper.GetChildren(tgt))
            {
                if (ctl is DependencyObject)
                {
                    int nextPosition = position + 1;
                    ShowLogicalControl((DependencyObject)ctl, nextPosition);
                }
            }
        }

        /// <summary>
        /// output information of controls
        /// </summary>
        /// <param name="tgt"></param>
        /// <param name="position"></param>
        private static void ShowLog(DependencyObject tgt, int position = 0)
        {
            string tab = null;
            string baseInfo = tgt.ToString();

            for (int i = 0; i < position; i++)
            {
                tab += "\t";
            }
            Debug.WriteLine(tab + baseInfo);
        }
    }
}
