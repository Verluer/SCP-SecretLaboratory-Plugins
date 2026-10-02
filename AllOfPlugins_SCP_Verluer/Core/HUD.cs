using CentralAuth;
using LabApi.Features.Wrappers;
using PlayerStatsSystem;
using RueI.API;
using RueI.API.Elements;
using RueI.API.Elements.Enums;
using System;
using UnityEngine;

namespace AllOfPlugins_SCP_Verluer.Core
{
    public static class HUD
    {
        private static readonly Tag ProximityModeTag = new Tag("proximity_mode");

        private static readonly Tag RoundTimeTag = new Tag("round_time_custom");
        private static readonly Tag ScpHood = new Tag("Scp_Hood");
        public static void ShowProximity(ReferenceHub player)
        {
            BasicElement element = new BasicElement(200f, "<space=-1000><color=yellow>Proximity SCP Chat Active</color>")
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
            RueDisplay.Get(player).Remove(ProximityModeTag);
        }
        public static void ScpHpHood()
        {
            foreach (ReferenceHub viewer in ReferenceHub.AllHubs)
            {
                if (viewer == null ||
                    viewer.Mode == ClientInstanceMode.DedicatedServer)
                    continue;

                RueDisplay display = RueDisplay.Get(viewer);

                if (viewer.roleManager.CurrentRole.Team != PlayerRoles.Team.SCPs)
                {
                    display.Remove(ScpHood);
                    continue;
                }

                DynamicElement element = new DynamicElement(850f, _ =>
                {
                    string result = "<space=700><color=white>SCP State:</color>\n";

                    foreach (ReferenceHub hub in ReferenceHub.AllHubs)
                    {
                        if (hub == null ||
                            hub.Mode == ClientInstanceMode.DedicatedServer ||
                            hub.roleManager.CurrentRole.Team != PlayerRoles.Team.SCPs)
                            continue;

                        Player player = Player.Get(hub);

                        if (player == null)
                            continue;

                        string Nick = player.Nickname.Length > 4
                            ? player.Nickname.Substring(0, 4)
                            : player.Nickname;
                        string role = hub.roleManager.CurrentRole.RoleTypeId.ToString();

                        HealthStat hp = hub.playerStats.GetModule<HealthStat>();
                        HumeShieldStat shield = hub.playerStats.GetModule<HumeShieldStat>();


                        result +=
                            $"<space=750><color=blue>{Nick}</color> || " +
                            $"<color=red>{role}</color> || " +
                            $"<color=green>{hp.CurValue:0}</color>:" +
                            $"<color=grey>{shield.CurValue:0}</color>\n";
                    }

                    return result;
                })
                {
                    ZIndex = 10,
                    VerticalAlign = VerticalAlign.Down,
                    UpdateInterval = TimeSpan.FromSeconds(0.1f),
                    ShowToSpectators = false
                };

                display.Show(ScpHood, element);
            }
        }
        public static void HideScpHood(ReferenceHub player)
        {
            RueDisplay.Get(player).Remove(ScpHood);
        }
        public static void AllPlayerHud()
        {
            foreach (ReferenceHub hub in ReferenceHub.AllHubs)
            {
                if (hub == null || hub.Mode == ClientInstanceMode.DedicatedServer)
                    continue;
    
                DynamicElement element = new DynamicElement(975f,_ =>
                    {
                        float elapsed = Time.time - EventModule.VanillaEventHandler.RoundStartTime;

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

                RueDisplay.Get(hub).Show(RoundTimeTag, element);
            }
        }
    }
}