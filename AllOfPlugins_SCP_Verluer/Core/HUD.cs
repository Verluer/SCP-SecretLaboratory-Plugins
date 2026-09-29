using CentralAuth;
using LabApi.Features.Wrappers;
using MEC;
using RemoteAdmin.Communication;
using RueI.API;
using RueI.API.Elements;
using RueI.API.Elements.Enums;
using System;
using System.Collections.Generic;
using Unity.Profiling;
using UnityEngine;

namespace AllOfPlugins_SCP_Verluer.Core
{
    public static class HUD
    {
        private static readonly Tag ProximityModeTag =
            new Tag("proximity_mode");

        private static readonly Tag RoundTimeTag =
    new Tag("round_time_custom");
        public static void ShowProximity(ReferenceHub player)
        {
            BasicElement element = new BasicElement(
                200f,
                "<space=-1000><color=yellow>Proximity SCP Chat Active</color>")
            {
                ZIndex = 10,
                VerticalAlign = VerticalAlign.Down
            };

            RueDisplay.Get(player).Show(
                ProximityModeTag,
                element);
        }

        public static void HideProximity(ReferenceHub player)
        {
            RueDisplay.Get(player).Remove(
                ProximityModeTag);
        }

        public static void AllPlayerHud()
        {
            foreach (ReferenceHub hub in ReferenceHub.AllHubs)
            {
                if (hub == null || hub.Mode == ClientInstanceMode.DedicatedServer)
                    continue;
    
                DynamicElement element = new DynamicElement(
                    975f,
                    _ =>
                    {
                        float elapsed = Time.time - AllOfPlugins._roundStartTime;

                        int minutes = Mathf.FloorToInt(elapsed / 60f);
                        int seconds = Mathf.FloorToInt(elapsed % 60f);

                        return $"<color=yellow>Время раунда: {minutes:00}:{seconds:00}</color>";
                    })
                {
                    ZIndex = 10,
                    VerticalAlign = VerticalAlign.Up,
                    UpdateInterval = TimeSpan.FromSeconds(1),
                    ShowToSpectators = false
                };

                RueDisplay.Get(hub).Show(
                    RoundTimeTag,
                    element);
            }
        }
    }
}