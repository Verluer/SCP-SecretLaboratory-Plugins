using AllOfPlugins_SCP_Verluer.CandyExpansion;
using AllOfPlugins_SCP_Verluer.Core;
using CentralAuth;
using HarmonyLib;
using InventorySystem.Disarming;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Arguments.ServerEvents;
using LabApi.Events.Handlers;
using LabApi.Features.Wrappers;
using PlayerRoles;
using PlayerRoles.RoleAssign;
using ProjectMER.Commands.ToolGunLike;
using RemoteAdmin.Communication;
using Respawning.Waves;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UncomplicatedCustomRoles.Commands;
using UnityEngine;

namespace AllOfPlugins_SCP_Verluer.GamePatch
{
    public class WaveRespawn
    {
        private static List<ReferenceHub> ChaosTeam = new List<ReferenceHub>();
        private static List<ReferenceHub> NTFTeam = new List<ReferenceHub>();

        private static List<ReferenceHub> ScpTeam = new List<ReferenceHub>();

        private static readonly HashSet<ReferenceHub> CuffedPlayers = new HashSet<ReferenceHub>();

        public static void Enable(Harmony _harmony)
        {
            MethodInfo property = AccessTools.PropertyGetter(typeof(WaveSpawner), "AnyPlayersAvailable");

            MethodInfo propertyRespawnTokenNtf = AccessTools.PropertyGetter(typeof(NtfSpawnWave), "InitialRespawnTokens");
            MethodInfo propertyRespawnTokenChaos = AccessTools.PropertyGetter(typeof(ChaosSpawnWave), "InitialRespawnTokens");

            _harmony.Patch(property, prefix: new HarmonyMethod(typeof(WaveRespawn), nameof(AnyPlayersAvailablePrefix)));


            _harmony.Patch(propertyRespawnTokenNtf, prefix: new HarmonyMethod(typeof(WaveRespawn), nameof(InitialRespawnTokensPrefix)));
            _harmony.Patch(propertyRespawnTokenChaos, prefix: new HarmonyMethod(typeof(WaveRespawn), nameof(InitialRespawnTokensPrefix)));

            ServerEvents.WaveRespawning += OnWaveRespawning;

            RoleAssigner.OnPlayersSpawned += OnPlayersSpawned;

            PlayerEvents.Death += OnPlayerDeath;

            PlayerEvents.Escaped += OnPlayerEscaped;
            PlayerEvents.Escaping += OnPlayerEscaping;
        }

        public static void Disable()
        {
            ServerEvents.WaveRespawning -= OnWaveRespawning;
            RoleAssigner.OnPlayersSpawned -= OnPlayersSpawned;
            PlayerEvents.Death -= OnPlayerDeath;
            PlayerEvents.Escaped -= OnPlayerEscaped;
            PlayerEvents.Escaping -= OnPlayerEscaping;

            ChaosTeam.Clear();
            NTFTeam.Clear();
            ScpTeam.Clear();
        }
        private static void OnPlayerEscaped(PlayerEscapedEventArgs ev)
        {
            if (ev.Player == null)
                return;

            ReferenceHub hub = ev.Player.ReferenceHub;
            if (hub == null)
                return;

            if (!CuffedPlayers.Remove(hub))
                return;

            if (ev.OldRole == RoleTypeId.ClassD)
            {
                RemoveFromAllTeams(hub);
                NTFTeam.Add(hub);
            }
            else if (ev.OldRole == RoleTypeId.Scientist)
            {
                RemoveFromAllTeams(hub);
                ChaosTeam.Add(hub);
            }
        }
        private static void OnPlayersSpawned()
        {
            TeamSpawn();
            Balanced();
        }
        private static bool InitialRespawnTokensPrefix(ref int __result)
        {
            __result = 2;
            return false;
        }
        private static void OnPlayerDeath(PlayerDeathEventArgs ev)
        {
            if (ev.Player == null)
                return;

            ReferenceHub victimHub = ev.Player.ReferenceHub;

            if (victimHub == null)
                return;

            if (!ScpTeam.Contains(victimHub))
                return;

            ScpTeam.Remove(victimHub);

            if (ev.Attacker == null)
                return;

            ReferenceHub attackerHub = ev.Attacker.ReferenceHub;

            if (attackerHub == null)
                return;

            ChaosTeam.Remove(victimHub);
            NTFTeam.Remove(victimHub);

            if (ChaosTeam.Contains(attackerHub))
            {
                ChaosTeam.Add(victimHub);
                return;
            }

            if (NTFTeam.Contains(attackerHub))
            {
                NTFTeam.Add(victimHub);
                return;
            }
        }
        private static void OnPlayerEscaping(PlayerEscapingEventArgs ev)
        {
            if (ev.Player == null)
                return;

            ReferenceHub hub = ev.Player.ReferenceHub;
            if (hub == null)
                return;

            CuffedPlayers.Remove(hub);

            if (ev.Player.ReferenceHub.inventory.IsDisarmed())
                CuffedPlayers.Add(hub);
        }
        private static void Balanced()
        {
            List<ReferenceHub> countGuard = new List<ReferenceHub>();
            List<ReferenceHub> countPlayer = new List<ReferenceHub>();
            foreach (ReferenceHub hub in ReferenceHub.AllHubs)
            {
                if (hub == null || hub.Mode == ClientInstanceMode.DedicatedServer)
                    continue;

                if (hub.roleManager.CurrentRole.RoleTypeId == RoleTypeId.FacilityGuard)
                {
                    countGuard.Add(hub);
                }
                countPlayer.Add(hub);
            }

            if(countGuard.Count >= 3 && (countPlayer.Count == 9 || countPlayer.Count == 10))
            {
                int roll = UnityEngine.Random.Range(0, countGuard.Count);
                ReferenceHub selected = countGuard[roll];

                Player.Get(selected).ClearInventory();

                selected.roleManager.ServerSetRole(RoleTypeId.ClassD, RoleChangeReason.RemoteAdmin, RoleSpawnFlags.AssignInventory | RoleSpawnFlags.UseSpawnpoint);

                RemoveFromAllTeams(selected);
                ChaosTeam.Add(selected);
            }
        }
        private static void TeamSpawn()
        {
            ChaosTeam.Clear();
            NTFTeam.Clear();
            ScpTeam.Clear();

            foreach (ReferenceHub hub in ReferenceHub.AllHubs)
            {
                if (hub == null || hub.Mode == ClientInstanceMode.DedicatedServer)
                    continue;

                RoleTypeId role = hub.roleManager.CurrentRole.RoleTypeId;

                if (role == RoleTypeId.ClassD)
                {
                    AddToTeam(ChaosTeam, hub);
                }
                else if (role == RoleTypeId.Scientist)
                {
                    AddToTeam(NTFTeam, hub);
                }
                else if (role == RoleTypeId.FacilityGuard)
                {
                    AddToTeam(NTFTeam, hub);
                }
                else if (hub.roleManager.CurrentRole.Team == Team.SCPs)
                {
                    AddToTeam(ScpTeam, hub);
                }
            }
        }

        private static void AddToTeam(List<ReferenceHub> team, ReferenceHub hub)
        {
            if (!team.Contains(hub))
                team.Add(hub);
        }

        private static void RemoveFromAllTeams(ReferenceHub hub)
        {
            ChaosTeam.Remove(hub);
            NTFTeam.Remove(hub);
            ScpTeam.Remove(hub);
        }

        private static bool AnyPlayersAvailablePrefix(ref bool __result)
        {
            int totalPlayers = ReferenceHub.AllHubs.Count;

            int availablePlayers =
                ReferenceHub.AllHubs.Count(
                    WaveSpawner.CanBeSpawned);

            int requiredPlayers =
                Mathf.CeilToInt(totalPlayers * 0.4f);

            __result = availablePlayers >= requiredPlayers;

            return false;
        }

        public static void OnWaveRespawning(WaveRespawningEventArgs wave)
        {
            List<Player> playerList = wave.SpawningPlayers.ToList();

            List<Player> countChaos = new List<Player>();

            List<Player> countNTF = new List<Player>();

            foreach (Player player in playerList)
            {
                if (player == null)
                    continue;

                ReferenceHub playerHub = player.ReferenceHub;

                if (playerHub == null)
                    continue;

                if (wave.Wave == RespawnWaves.PrimaryMtfWave ||
                    wave.Wave == RespawnWaves.PrimaryChaosWave)
                {
                    if (NTFTeam.Contains(playerHub))
                    {
                        int roll = UnityEngine.Random.Range(0, 100);

                        if (roll < 75)
                            wave.Roles[player] = RoleTypeId.NtfPrivate;
                        else
                            wave.Roles[player] = RoleTypeId.NtfSergeant;

                        countNTF.Add(player);

                        continue;
                    }

                    if (ChaosTeam.Contains(playerHub))
                    {
                        int roll = UnityEngine.Random.Range(0, 100);

                        if (roll < 85)
                            wave.Roles[player] = RoleTypeId.ChaosRifleman;
                        else
                            wave.Roles[player] = RoleTypeId.ChaosMarauder;

                        countChaos.Add(player);
                    }
                }
            }


            if (countNTF.Count > 2)
            {
                Player player = countNTF[UnityEngine.Random.Range(0, countNTF.Count)];

                wave.Roles[player] = RoleTypeId.NtfCaptain;
            }


            if (countChaos.Count > 2)
            {
                Player player = countChaos[UnityEngine.Random.Range(0, countChaos.Count)];

                wave.Roles[player] = RoleTypeId.ChaosRepressor;
            }
        }
    }
}