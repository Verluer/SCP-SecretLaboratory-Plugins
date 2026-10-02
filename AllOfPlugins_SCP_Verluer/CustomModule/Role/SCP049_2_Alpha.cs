using AllOfPlugins_SCP_Verluer.CandyExpansion;
using CentralAuth;
using CustomPlayerEffects;
using HarmonyLib;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;
using LabApi.Features.Wrappers;
using MapGeneration;
using MEC;
using PlayerRoles;
using PlayerRoles.PlayableScps.Scp049.Zombies;
using PlayerRoles.Ragdolls;
using PlayerRoles.RoleAssign;
using PlayerStatsSystem;
using System.Collections.Generic;
using System.Reflection;
using UncomplicatedCustomRoles.API.Enums;
using UncomplicatedCustomRoles.API.Features;
using UncomplicatedCustomRoles.API.Features.Behaviour;
using UncomplicatedCustomRoles.Extensions;
using UncomplicatedCustomRoles.Manager;
using UnityEngine;

namespace AllOfPlugins_SCP_Verluer.CustomModule.Role
{
    public static class SCP049_2_Alpha
    {
        private static readonly HashSet<Player> Scp0492AlphaPlayers = new();
        public static Scp0492AlphaRole _role;
        public static bool SpawnScp0492Alpha(ReferenceHub hub, Vector3? position = null)
        {
            if (_role == null)
            {
                LabApi.Features.Console.Logger.Error(
                    "[Scp049_2_Alpha] Роль не зарегистрирована.");

                return false;
            }

            Player target = Player.Get(hub);

            target.SetCustomRole(_role);
            Timing.CallDelayed(0.1f, () =>
            {
                if (position != null)
                {
                    target.Position = position.Value;
                    LabApi.Features.Console.Logger.Info($"[Scp049_2_Alpha] {target.Position}");
                }
            });
            LabApi.Features.Console.Logger.Info($"[Scp049_2_Alpha] {target.Nickname} стал Scp049_2_Alpha.");


            Scp0492AlphaPlayers.Add(target);
            return true;
        }

        public static void Enable(Harmony _harmony)
        {
            if (_role != null)
            {
                LabApi.Features.Console.Logger.Warn(
                    "[Scp049_2_Alpha] Модуль уже включён.");

                return;
            }
            MethodInfo methodZombie = AccessTools.Method(typeof(ZombieConsumeAbility), "ServerComplete");

            if (methodZombie == null) return;

            _harmony.Patch(methodZombie, prefix: new HarmonyMethod(typeof(SCP049_2_Alpha), nameof(ServerCompletePrefix)));

            _role = new Scp0492AlphaRole
            {
                Id = CustomRole.GetFirstFreeId(0492)
            };

            LoadStatusType status = CustomRole.Register(_role);

            if (status != LoadStatusType.Success)
            {
                LabApi.Features.Console.Logger.Error(
                    $"[Scp049_2_Alpha] Не удалось зарегистрировать роль: {status}");

                _role = null;
                return;
            }

            LabApi.Features.Console.Logger.Info(
                $"[Scp049_2_Alpha] Роль зарегистрирована. ID = {_role.Id}");

            PlayerEvents.ChangedRole += OnChangedRole;

        }

        public static void Disable()
        {
            PlayerEvents.ChangedRole -= OnChangedRole;

            _role = null;
            Scp0492AlphaPlayers.Clear();
        }
        private static void OnChangedRole(PlayerChangedRoleEventArgs ev)
        {
            Scp0492AlphaPlayers.Remove(ev.Player);
        }
        public static bool IsHumanScp(Player player)
        {
            return player != null && Scp0492AlphaPlayers.Contains(player);
        }
        private static readonly PropertyInfo CurRagdollProperty = typeof(ZombieConsumeAbility).BaseType.GetProperty("CurRagdoll", BindingFlags.Instance | BindingFlags.NonPublic);

        private static bool ServerCompletePrefix(ZombieConsumeAbility __instance)
        {
            ReferenceHub zombie = __instance.Owner;
            BasicRagdoll corpse = CurRagdollProperty.GetValue(__instance) as BasicRagdoll;

            if (corpse == null)
                return true;

            ReferenceHub victim = corpse.Info.OwnerHub;

            if (victim == null)
                return true;


            HealthStat health = zombie.playerStats.GetModule<HealthStat>();
            if (health.MaxValue == 2000)
            {
                if (victim.roleManager.CurrentRole.RoleTypeId == RoleTypeId.Spectator)
                {
                    victim.roleManager.ServerSetRole(RoleTypeId.Scp0492, RoleChangeReason.RemoteAdmin, RoleSpawnFlags.None);
                }
            }

            return true;
        }
    }


    public class Scp0492AlphaRole : EventCustomRole
    {
        public override int Id { get; set; }

        public override string Name { get; set; } =
            "<color=#C50000>Предвестник чумы</color>";

        public override bool OverrideRoleName { get; set; } = true;

        public override string Nickname { get; set; } = "";

        public override string CustomInfo { get; set; } = "";

        public override string BadgeName { get; set; } = "";

        public override string BadgeColor { get; set; } = "";

        public override RoleTypeId Role { get; set; } = RoleTypeId.Scp0492;

        public override Team? Team { get; set; } =
            PlayerRoles.Team.SCPs;

        public override RoleTypeId RoleAppearance { get; set; } = RoleTypeId.Scp0492;

        public override List<Team> IsFriendOf { get; set; } =
            new List<Team>
            {
            };

        public override HealthBehaviour Health { get; set; } =
            new HealthBehaviour
            {
                Amount = 1500,
                Maximum = 2000,
            };

        public override AhpBehaviour Ahp { get; set; } =
            new AhpBehaviour
            {
            };

        public override HumeShieldBehaviour HumeShield { get; set; } =
            new HumeShieldBehaviour()
            {
                Amount = 1000,
                Maximum = 150,
            };

        public override List<Effect> Effects { get; set; } =
            new List<Effect>()
            {
                        new Effect
                        {
                            EffectType = "MovementBoost",
                            Duration = -1f,
                            Intensity = 25,
                            Removable = false
                        }
            };

        public override StaminaBehaviour Stamina { get; set; } =
            new StaminaBehaviour
            {
            };

        public override int MaxScp330Candies { get; set; } = 2;

        public override bool CanEscape { get; set; } = false;

        public override Dictionary<string, string> RoleAfterEscape { get; set; } =
            new Dictionary<string, string>
            {
                { "default", "InternalRole Spectator" }
            };

        public override UnityEngine.Vector3 Scale { get; set; } =
            UnityEngine.Vector3.one;

        public override string SpawnBroadcast { get; set; } = $"Ваше тело поглотила <color=red>чума</color>. Как нулевой пациент вы распространяете <color=red>Чуму</color> при поедании других существ";

        public override ushort SpawnBroadcastDuration { get; set; } = 10;

        public override string SpawnHint { get; set; } = "";

        public override float SpawnHintDuration { get; set; } = 0f;

        public override Dictionary<ItemCategory, sbyte> CustomInventoryLimits { get; set; } =
            new Dictionary<ItemCategory, sbyte>();

        public override List<ItemType> Inventory { get; set; } =
            new List<ItemType>();

        public override List<uint> CustomItemsInventory { get; set; } =
            new List<uint>();

        public override Dictionary<ItemType, ushort> Ammo { get; set; } =
            new Dictionary<ItemType, ushort>();

        public override float DamageMultiplier { get; set; } = 1f;

        public override SpawnBehaviour SpawnSettings { get; set; } =
            new SpawnBehaviour
            {
                CanReplaceRoles = new List<RoleTypeId>(),
                MaxPlayers = 0,
                MinPlayers = 0,
                Spawn = SpawnType.KeepCurrentPositionSpawn,
                SpawnChance = 0f,
                SpawnPoints = new List<string>(),
                SpawnRoles = new List<RoleTypeId>(),
                SpawnRooms = new List<string>(),
                SpawnZones = new List<FacilityZone>()
            };

        public override List<object> CustomFlags { get; set; } =
            new List<object>
            {
                new Dictionary<object, object>
                {
                    {
                        "CustomScpAnnouncer",
                        new Dictionary<object, object>
                        {
                            {
                                "name",
                                "Scp-0492A"
                            }
                        }
                    }
                }
            };

        // UCR не должен самостоятельно спавнить эту роль.
        public override bool IgnoreSpawnSystem { get; set; } = true;
    }
}