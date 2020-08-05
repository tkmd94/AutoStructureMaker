using EsapiEssentials.Plugin;
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
            var window = new Window();
            var mainWindow = new AutoStructure.MainControl();
            window.Title = "AutoStructure";
            window.Content = mainWindow;
            mainWindow.structureSet = context.StructureSet as StructureSet;
            mainWindow.UserId = context.CurrentUser.Name;
            window.ShowDialog();
        }
    }
}
