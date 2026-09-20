using AllOfPlugins_SCP_Verluer.GamePatch;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;
using LabApi.Features.Wrappers;
using MapGeneration;
using MEC;
using PlayerRoles;
using PlayerRoles.RoleAssign;
using ProjectMER.Features.Extensions;
using System.Collections.Generic;
using UncomplicatedCustomRoles.API.Enums;
using UncomplicatedCustomRoles.API.Features;
using UncomplicatedCustomRoles.API.Features.Behaviour;
using UncomplicatedCustomRoles.API.Interfaces;
using UncomplicatedCustomRoles.Extensions;
using UncomplicatedCustomRoles.Manager;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

namespace AllOfPlugins_SCP_Verluer.CustomModule.Role
{
    public static class HumanSCP
    {
        private static readonly HashSet<Player> HumanScpPlayers = new();
        private static HumanScpRole _role;
        public static bool SpawnHumanScpFromExistingScp()
        {
            if (_role == null)
            {
                LabApi.Features.Console.Logger.Error(
                    "[HumanSCP] Роль не зарегистрирована.");

                return false;
            }

            List<Player> scps = new List<Player>();

            foreach (Player player in Player.ReadyList)
            {
                if (player == null || !player.IsAlive)
                    continue;

                if (player.Team == Team.SCPs)
                {
                    scps.Add(player);
                }
            }

            if (scps.Count < 2)
            {
                LabApi.Features.Console.Logger.Warn(
                    "[HumanSCP] Недостаточно SCP.");

                return false;
            }
            int roll = UnityEngine.Random.Range(0, 100);
            if (roll > 15)
            {
                return false;
            }

            List<Player> specialSCP = new List<Player>();
            Player target = null;

            foreach (Player scpplayer in scps)
            {
                if (scpplayer.Role == RoleTypeId.Scp079 || scpplayer.Role == RoleTypeId.Scp096)
                    specialSCP.Add(scpplayer);                              
            }

            if (specialSCP.Count > 0)
            {
                target = specialSCP[UnityEngine.Random.Range(0, specialSCP.Count)];
            }
            else
                target = scps[UnityEngine.Random.Range(0, scps.Count)];

            LabApi.Features.Console.Logger.Info(
                $"[HumanSCP] Выбран SCP: " +
                $"{target.Nickname} ({target.Role})");


            target.SetCustomRole(_role);
           

            LabApi.Features.Console.Logger.Info(
                $"[HumanSCP] {target.Nickname} стал Human SCP.");

            Timing.CallDelayed(0.5f, () =>
            {
                HumanScpPlayers.Add(target);
            });
            return true;
        }
        private static void OnPlayersSpawned()
        {
            SpawnHumanScpFromExistingScp();

        }

        public static void Enable()
        {
            if (_role != null)
            {
                LabApi.Features.Console.Logger.Warn(
                    "[HumanSCP] Модуль уже включён.");

                return;
            }

            _role = new HumanScpRole
            {
                Id = CustomRole.GetFirstFreeId(3500)
            };

            LoadStatusType status = CustomRole.Register(_role);

            if (status != LoadStatusType.Success)
            {
                LabApi.Features.Console.Logger.Error(
                    $"[HumanSCP] Не удалось зарегистрировать роль: {status}");

                _role = null;
                return;
            }

            LabApi.Features.Console.Logger.Info(
                $"[HumanSCP] Роль зарегистрирована. ID = {_role.Id}");
            if (EventModule.EventModule.isZombieEventActive == false)
            {
                RoleAssigner.OnPlayersSpawned += OnPlayersSpawned;
            }
            PlayerEvents.ChangedRole += OnChangedRole;

        }

        public static void Disable()
        {
            RoleAssigner.OnPlayersSpawned -= OnPlayersSpawned;

            _role = null;
            HumanScpPlayers.Clear();
        }
        private static void OnChangedRole(PlayerChangedRoleEventArgs ev)
        {
            HumanScpPlayers.Remove(ev.Player);
        }
        public static bool IsHumanScp(Player player)
        {
            return player != null && HumanScpPlayers.Contains(player);
        }
    }


    public class HumanScpRole : EventCustomRole
    {
        public override int Id { get; set; }

        public override string Name { get; set; } =
            "<color=#EE7600>Персонал Класса D</color>";

        public override bool OverrideRoleName { get; set; } = true;

        public override string Nickname { get; set; } = "";

        public override string CustomInfo { get; set; } = "";

        public override string BadgeName { get; set; } = "Test";

        public override string BadgeColor { get; set; } = "";

        // Основа роли
        public override RoleTypeId Role { get; set; } =
            RoleTypeId.ClassD;

        // Но команда — SCP.
        public override Team? Team { get; set; } =
            PlayerRoles.Team.SCPs;

        // Внешность остаётся D-Class.
        public override RoleTypeId RoleAppearance { get; set; } =
            RoleTypeId.ClassD;

        // Human SCP является союзником обычных SCP.
        public override List<Team> IsFriendOf { get; set; } =
            new List<Team>
            {
                PlayerRoles.Team.SCPs
            };

        public override HealthBehaviour Health { get; set; } =
            new HealthBehaviour
            {
                Amount = 150,
                Maximum = 150,
            };

        public override AhpBehaviour Ahp { get; set; } =
            new AhpBehaviour
            {
                Limit = 450,
                Amount = 450,
                Decay = 0,
            };

        public override HumeShieldBehaviour HumeShield { get; set; } =
            new HumeShieldBehaviour();

        public override List<Effect> Effects { get; set; } =
            new List<Effect>();

        public override StaminaBehaviour Stamina { get; set; } =
            new StaminaBehaviour
            {
                Infinite = false,
                RegenMultiplier = 1f,
                UsageMultiplier = 1f
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

        public override string SpawnBroadcast { get; set; } =
            "Вы стали человекоподобным SCP!";

        public override ushort SpawnBroadcastDuration { get; set; } = 5;

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
                Spawn = SpawnType.ClassDCell,
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
                                "SCP-Human"
                            }
                        }
                    }
                }
            };

        // UCR не должен самостоятельно спавнить эту роль.
        public override bool IgnoreSpawnSystem { get; set; } = true;
    }
}