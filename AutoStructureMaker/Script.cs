using EsapiEssentials.Plugin;
using System.Diagnostics;
using System.Windows;
using VMS.TPS.Common.Model.API;

// TODO: Uncomment the following line if the script requires write access.
[assembly: ESAPIScript(IsWriteable = true)]

namespace VMS.TPS
{
    public class Script : ScriptBase
    {
        public override void Run(PluginScriptContext context)
        {
            // TODO : Add here the code that is called when the script is launched from Eclipse.
            User user = context.CurrentUser;
            StructureSet structureSet = context.StructureSet;

            if (structureSet == null)
            {
                MessageBox.Show("No structureSet is loaded.");
                return;
            }

            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            string scriptVersion = fvi.FileVersion;

            var window = new Window();
            var mainWindow = new AutoStructure.MainControl();
            window.Title = "AutoStructureMaker "+ scriptVersion;

            window.Content = mainWindow;
            mainWindow.structureSet = structureSet;
            mainWindow.UserId = user.Name;
            window.ShowDialog();
        }
    }
}
