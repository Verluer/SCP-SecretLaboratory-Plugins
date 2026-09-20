using HarmonyLib;

namespace AllOfPlugins_SCP_Verluer.PluginsPatch
{
    public class PluginsPatch
    {
        public static void Enable(Harmony harmony)
        {
            Scp261Patch.Enable(harmony);
        }


        public static void Disable()
        {
            Scp261Patch.Disable();
        }
    }
}
