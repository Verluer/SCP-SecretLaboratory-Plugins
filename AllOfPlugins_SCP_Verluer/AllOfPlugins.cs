using AllOfPlugins_SCP_Verluer.EventModule;
using AllOfPlugins_SCP_Verluer.GamePatch;
using HarmonyLib;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.CustomHandlers;
using LabApi.Events.Handlers;
using System;
using UnityEngine;
using VoiceChat.Networking;

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
        private MyKeybind _myKeybind;
        public static float _roundStartTime;
        public override void Enable()
        {
            _harmony = new Harmony("verluer.allofplugins");

            EventModule.EventModule.Enable(_harmony);

            ServerEvents.RoundStarted += () =>
            {
                _roundStartTime = Time.time;
            };
            PlayerEvents.SpawningRagdoll += OnSpawningRagdoll;

            GamePatch.GamePatch.Enable(_harmony);
            PluginsPatch.PluginsPatch.Enable(_harmony);
            CandyExpansion.CandyExpansion.Enable(_harmony);

            CustomRoleModule.CustomRoleModule.Enable(_harmony);
    
            _myKeybind = new MyKeybind();

            CustomHandlersManager.RegisterEventsHandler(_myKeybind);

        }

        public override void Disable()
        {
            GamePatch.GamePatch.Disable();
            EventModule.EventModule.Disable();
            CustomRoleModule.CustomRoleModule.Disable();
            PluginsPatch.PluginsPatch.Disable();
            CandyExpansion.CandyExpansion.Disable();

            PlayerEvents.SpawningRagdoll -= OnSpawningRagdoll;

            if (_harmony != null)
            {
                _harmony.UnpatchAll("verluer.allofplugins");
                _harmony = null;
            }

            _myKeybind?.Dispose();

            CustomHandlersManager.UnregisterEventsHandler(_myKeybind);

            _myKeybind = null;
        }
        private static void OnSpawningRagdoll(PlayerSpawningRagdollEventArgs ev)
        {
            if (!PlayerSchematicManager.HasSchematic(ev.Player))
                return;

            ev.IsAllowed = false;
        }
    }
}