using AllOfPlugins_SCP_Verluer.Core;
using AllOfPlugins_SCP_Verluer.EventModule;
using AllOfPlugins_SCP_Verluer.GamePatch;
using HarmonyLib;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.CustomHandlers;
using LabApi.Events.Handlers;
using LabApi.Features.Wrappers;
using MEC;
using PlayerRoles;
using PlayerRoles.RoleAssign;
using System;
using UnityEngine;
using VoiceChat.Networking;

namespace AllOfPlugins_SCP_Verluer
{
    public class AllOfPlugins : LabApi.Loader.Features.Plugins.Plugin
    {
        private Harmony _harmony;
        private MyKeybind _myKeybind;

        public override string Name => "All Of Plugins";
        public override string Description => "Catalog of plugins and patches.";
        public override string Author => "Verluer";
        public override Version RequiredApiVersion => new Version(1, 1, 7);

        public override void Enable()
        {
            _harmony = new Harmony("verluer.allofplugins");

            EventModule.EventModule.Enable();
            GamePatch.GamePatch.Enable(_harmony);
            PluginsPatch.PluginsPatch.Enable(_harmony);
            CandyExpansion.CandyExpansion.Enable(_harmony);
            CustomModule.CustomModule.Enable(_harmony);

            DoorManager.DoorManager.Enable();

            _myKeybind = new MyKeybind();
            CustomHandlersManager.RegisterEventsHandler(_myKeybind);
        }

        public override void Disable()
        {
            CustomHandlersManager.UnregisterEventsHandler(_myKeybind);
            _myKeybind?.Dispose();
            _myKeybind = null;

            DoorManager.DoorManager.Disable();
            CustomModule.CustomModule.Disable();
            CandyExpansion.CandyExpansion.Disable();
            PluginsPatch.PluginsPatch.Disable();
            GamePatch.GamePatch.Disable();
            EventModule.EventModule.Disable();

            _harmony?.UnpatchAll("verluer.allofplugins");
            _harmony = null;
        }
    }
}