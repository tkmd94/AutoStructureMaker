using EsapiEssentials.Plugin;
using EsapiEssentials.PluginRunner;
using System.Windows;
using VMS.TPS;
using VMS.TPS.Common.Model.API;

[assembly: ESAPIScript(IsWriteable = true)]
namespace AutoStructureMaker.Runner
{
    public class RunnerScript : ScriptBase
    {
        public override void Run(PluginScriptContext context)
        {
            Script.Run(context.CurrentUser, context.StructureSet);
        }
    }

    /// <summary>
    /// App.xaml の相互作用ロジック
    /// </summary>
    public partial class App : System.Windows.Application
    {
        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            // Note: EsapiEssentials and EsapiEssentials.PluginRunner must be referenced,
            // as well as the project that contains the Script class
            ScriptRunner.Run(new RunnerScript());
        }

        // Fix UnauthorizedScriptingAPIAccessException
        public void DoNothing(PlanSetup plan) { }
    }
}
