using AllOfPlugins_SCP_Verluer.Core;
using AllOfPlugins_SCP_Verluer.EventModule;
using AllOfPlugins_SCP_Verluer.GamePatch;
using HarmonyLib;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.CustomHandlers;
using LabApi.Events.Handlers;
using MEC;
using PlayerRoles.RoleAssign;
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
        private Core.MyKeybind _myKeybind;
        public static float _roundStartTime;
        public override void Enable()
        {
            _harmony = new Harmony("verluer.allofplugins");

            EventModule.EventModule.Enable(_harmony);

            PlayerEvents.SpawningRagdoll += OnSpawningRagdoll;

            GamePatch.GamePatch.Enable(_harmony);
            PluginsPatch.PluginsPatch.Enable(_harmony);
            CandyExpansion.CandyExpansion.Enable(_harmony);

            CustomModule.CustomModule.Enable(_harmony);

            DoorManager.DoorManager.Enable();
    
            _myKeybind = new Core.MyKeybind();

            CustomHandlersManager.RegisterEventsHandler(_myKeybind);

            RoleAssigner.OnPlayersSpawned += () =>
            {
                _roundStartTime = Time.time;
                HUD.AllPlayerHud();
            };
        }

        public override void Disable()
        {
            GamePatch.GamePatch.Disable();
            EventModule.EventModule.Disable();
            CustomModule.CustomModule.Disable();
            DoorManager.DoorManager.Disable();
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
            if (!Core.PlayerSchematicManager.HasSchematic(ev.Player))
                return;

            ev.IsAllowed = false;

            Core.PlayerSchematicManager.DetachAsCorpse(ev.Player);

            Timing.CallDelayed(0.1f, () =>
            {
                if (ev.Player == null)
                    return;

                Core.PlayerSchematicManager.ShowFor(ev.Player);
            });
        }
    }
}