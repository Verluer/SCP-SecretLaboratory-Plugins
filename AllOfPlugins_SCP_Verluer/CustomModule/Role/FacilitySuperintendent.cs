using CentralAuth;
using Interactables.Interobjects.DoorUtils;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;
using LabApi.Features.Wrappers;
using MapGeneration;
using MEC;
using PlayerRoles;
using PlayerRoles.RoleAssign;
using System.Collections.Generic;
using System.Linq;
using UncomplicatedCustomRoles.API.Enums;
using UncomplicatedCustomRoles.API.Features;
using UncomplicatedCustomRoles.API.Features.Behaviour;
using UncomplicatedCustomRoles.Extensions;
using UncomplicatedCustomRoles.Manager;
using UnityEngine;

namespace AllOfPlugins_SCP_Verluer.CustomModule.Role
{
    public class FacilitySuperintendent
    {
        private static readonly HashSet<Player> FacilitySuperintendentPlayers = new();
        public static FacilitySuperintendentRole _role;

        public static bool SpawnFacilitySuperintendent()
        {
            if (_role == null)
            {
                LabApi.Features.Console.Logger.Error(
                    "[FacilitySuperintendent] Роль не зарегистрирована.");

                return false;
            }

            List<Player> FacilityGuards = new List<Player>();
            int FacilityGuardCount = 0;

            foreach (Player player in Player.ReadyList)
            {
                if (player == null || !player.IsAlive)
                    continue;

                if (player.Role != RoleTypeId.FacilityGuard)
                    continue;

                FacilityGuardCount++;

                FacilityGuards.Add(player);
            }

            if (FacilityGuardCount == 0)
                return false;

            Player target = null;

            if (FacilityGuardCount < 3)
            {
                LabApi.Features.Console.Logger.Warn(
                    "[FacilitySuperintendent] Недостаточно охранников для Коменданта обьекта");

                return false;
            }
            else
            {
                int roll = UnityEngine.Random.Range(0, 100);

                if (roll >= 25)
                    return false;
                target = FacilityGuards[UnityEngine.Random.Range(0, FacilityGuards.Count)];
            }
            LabApi.Features.Console.Logger.Info($"[FacilitySuperintendent] Выбран комендантом: " + $"{target.Nickname} ({target.Role})");

            target.SetCustomRole(_role);
            
            LabApi.Features.Console.Logger.Info(
            $"[FacilitySuperintendent] {target.Nickname} стал FacilitySuperintendent.");

            FacilitySuperintendentPlayers.Add(target);
            Timing.CallDelayed(0.5f, () =>
            {
                Room room = Room.List.FirstOrDefault(x => x.Name == RoomName.HczTestroom);

                if (room != null)
                {
                    Vector3 offset = new Vector3(1f, 0.5f, 5.8f);

                    target.Position = room.Position + offset;
                }
                else
                {
                    LabApi.Features.Console.Logger.Warn(
                        "[FacilitySuperintendent] Комната HczTestroom не найдена.");
                }

                KeycardItem.CreateCustomKeycardMetal(
                target,
                "Ключ-карта Коменданта объекта",
                target.Nickname,
                "Комендант объекта",
                new KeycardLevels(3, 2, 2),
                Color.black,
                Color.black,
                new Color32(255, 123, 0, 255),
                100,
                "75550184091");
            });

            return true;
        }
        private static void OnPlayersSpawned()
        {
            SpawnFacilitySuperintendent();

        }

        public static void Enable()
        {
            if (_role != null)
            {
                LabApi.Features.Console.Logger.Warn(
                    "[FacilitySuperintendent] Модуль уже включён.");

                return;

            }

            _role = new FacilitySuperintendentRole
            {
                Id = CustomRole.GetFirstFreeId(12)
            };

            LoadStatusType status = CustomRole.Register(_role);

            if (status != LoadStatusType.Success)
            {
                LabApi.Features.Console.Logger.Error($"[FacilitySuperintendent] Не удалось зарегистрировать роль: {status}");
                _role = null;
                return;
            }

            LabApi.Features.Console.Logger.Info($"[FacilitySuperintendent] Роль зарегистрирована. ID = {_role.Id}");
            if (EventModule.EventModule.IsZombieEventActive == false)
            {
                RoleAssigner.OnPlayersSpawned += OnPlayersSpawned;
            }
            PlayerEvents.ChangedRole += OnChangedRole;

        }

        public static void Disable()
        {
            RoleAssigner.OnPlayersSpawned -= OnPlayersSpawned;
            PlayerEvents.ChangedRole -= OnChangedRole;

            _role = null;
            FacilitySuperintendentPlayers.Clear();


        }
        private static void OnChangedRole(PlayerChangedRoleEventArgs ev)
        {
            FacilitySuperintendentPlayers.Remove(ev.Player);
        }
        public static bool IsContainmentEngineer(Player player)
        {
            return player != null && FacilitySuperintendentPlayers.Contains(player);
        }
    }
    public class FacilitySuperintendentRole : EventCustomRole
    {
        public override int Id { get; set; }

        public override string Name { get; set; } =
            "<color=#A0A0A0>Комендант объекта</color>";

        public override bool OverrideRoleName { get; set; } = true;

        public override string Nickname { get; set; } = "";

        public override string CustomInfo { get; set; } = "";

        public override string BadgeName { get; set; } = "";

        public override string BadgeColor { get; set; } = "";

        public override RoleTypeId Role { get; set; } = RoleTypeId.FacilityGuard;

        public override Team? Team { get; set; } = PlayerRoles.Team.FoundationForces;

        public override RoleTypeId RoleAppearance { get; set; } = RoleTypeId.FacilityGuard;

        public override List<Team> IsFriendOf { get; set; } = new List<Team>();

        public override HealthBehaviour Health { get; set; } =
            new HealthBehaviour
            {
                Amount = 150,
                Maximum = 150,
            };

        public override AhpBehaviour Ahp { get; set; } = new AhpBehaviour();

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

        public override bool CanEscape { get; set; } = true;

        public override Dictionary<string, string> RoleAfterEscape { get; set; } =
            new Dictionary<string, string>();

        public override UnityEngine.Vector3 Scale { get; set; } =
            UnityEngine.Vector3.one;

        public override string SpawnBroadcast { get; set; } =
            "Вы управляющий комплексом, у вас есть доступ ко многим местам комплекса!";

        public override ushort SpawnBroadcastDuration { get; set; } = 10;

        public override string SpawnHint { get; set; } = "";

        public override float SpawnHintDuration { get; set; } = 0f;

        public override Dictionary<ItemCategory, sbyte> CustomInventoryLimits { get; set; } = new Dictionary<ItemCategory, sbyte>();

        public override List<ItemType> Inventory { get; set; } = new List<ItemType>
            {
                    ItemType.Radio,
                    ItemType.ArmorCombat,
                    ItemType.GunE11SR,
                    ItemType.GrenadeFlash,
                    ItemType.Medkit,
            };

        public override List<uint> CustomItemsInventory { get; set; } = new List<uint>();

        public override Dictionary<ItemType, ushort> Ammo { get; set; } = new Dictionary<ItemType, ushort>
        {
            [ItemType.Ammo556x45] = 120
        };

        public override float DamageMultiplier { get; set; } = 1f;

        public override SpawnBehaviour SpawnSettings { get; set; } =
            new SpawnBehaviour
            {
                CanReplaceRoles = new List<RoleTypeId>(),
                MaxPlayers = 0,
                MinPlayers = 0,
                Spawn = SpawnType.RoleSpawn,
                SpawnChance = 0f,
                SpawnPoints = new List<string>(),
                SpawnRoles = new List<RoleTypeId>()
                { RoleTypeId.FacilityGuard },

                SpawnRooms = new List<string>(),
                SpawnZones = new List<FacilityZone>(),
            };

        public override List<object> CustomFlags { get; set; } =
            new List<object>();

        // UCR не должен самостоятельно спавнить эту роль.
        public override bool IgnoreSpawnSystem { get; set; } = true;
    }
}
