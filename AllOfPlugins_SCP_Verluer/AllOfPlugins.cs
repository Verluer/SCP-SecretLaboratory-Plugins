using HarmonyLib;
using System;

namespace AllOfPlugins_SCP_Verluer
{
    public class AllOfPlugins : LabApi.Loader.Features.Plugins.Plugin
    {
        public override string Name
        {
            get { return "All Of Plugins"; }
        }

        public override string Description
        {
            get
            {
                return "Catalog of plugins and patches.";
            }
        }

        public override string Author
        {
            get { return "Verluer"; }
        }

        public override Version RequiredApiVersion
        {
            get { return new Version(1, 1, 7); }
        }

        private Harmony _harmony;

        public override void Enable()
        {
            _harmony = new Harmony("verluer.allofplugins");

            GamePatch.GamePatch.Enable(_harmony);
            PluginsPatch.PluginsPatch.Enable(_harmony);
            CandyExpansion.CandyExpansion.Enable(_harmony);
            GiveItem.Enable();
        }

        public override void Disable()
        {
            GamePatch.GamePatch.Disable();
            PluginsPatch.PluginsPatch.Disable();
            CandyExpansion.CandyExpansion.Disable();
            GiveItem.Disable();

            if (_harmony != null)
            {
                _harmony.UnpatchAll("verluer.allofplugins");
                _harmony = null;
            }
        }
    }
}