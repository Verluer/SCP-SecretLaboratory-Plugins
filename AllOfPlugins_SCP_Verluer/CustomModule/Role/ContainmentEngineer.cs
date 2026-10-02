using CentralAuth;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;
using LabApi.Features.Wrappers;
using MapGeneration;
using MEC;
using PlayerRoles;
using PlayerRoles.RoleAssign;
using System.Collections.Generic;
using UncomplicatedCustomRoles.API.Enums;
using UncomplicatedCustomRoles.API.Features;
using UncomplicatedCustomRoles.API.Features.Behaviour;
using UncomplicatedCustomRoles.Extensions;
using UncomplicatedCustomRoles.Manager;

namespace AllOfPlugins_SCP_Verluer.CustomModule.Role
{
    public class ContainmentEngineer
    {
        private static readonly HashSet<Player> ContainmentEngineerPlayers = new();
        public static ContainmentEngineerRole _role;
        public static bool SpawnContainmentEngineer()
        {
            if (_role == null)
            {
                LabApi.Features.Console.Logger.Error(
                    "[ContainmentEngineer] Роль не зарегистрирована.");

                return false;
            }
            List<Player> Scientists = new List<Player>();
            int ScientistsCount = 0;

            foreach (Player player in Player.ReadyList)
            {
                if (player == null || !player.IsAlive)
                    continue;

                if (player.Team != Team.Scientists)
                    continue;

                ScientistsCount++;

                if (ResearchSupervisor.IsResearchSupervisor(player))
                    continue;

                Scientists.Add(player);
            }

            if (ScientistsCount == 0)
                return false;

            Player target = null;

            if (ScientistsCount < 3)
            {
                LabApi.Features.Console.Logger.Warn(
                    "[ContainmentEngineer] Недостаточно научных сотрудников для инженер по системам сдерживания");
                int roll = UnityEngine.Random.Range(0, 100);

                if (roll >= 25)
                    return false;
            }
           
            target = Scientists[UnityEngine.Random.Range(0, Scientists.Count)];
            LabApi.Features.Console.Logger.Info($"[ContainmentEngineer] Выбран инженером: " + $"{target.Nickname} ({target.Role})");

            target.SetCustomRole(_role);

            LabApi.Features.Console.Logger.Info(
            $"[ContainmentEngineer] {target.Nickname} стал ContainmentEngineer.");

            ContainmentEngineerPlayers.Add(target);

            return true;
        }
        private static void OnPlayersSpawned()
        {
            SpawnContainmentEngineer();

        }

        public static void Enable()
        {
            if (_role != null)
            {
                LabApi.Features.Console.Logger.Warn(
                    "[ContainmentEngineer] Модуль уже включён.");
                return;

            }
            _role = new ContainmentEngineerRole
            {
                Id = CustomRole.GetFirstFreeId(11)
            };

            LoadStatusType status = CustomRole.Register(_role);

            if (status != LoadStatusType.Success)
            {
                LabApi.Features.Console.Logger.Error($"[ContainmentEngineer] Не удалось зарегистрировать роль: {status}");
                _role = null;
                return;
            }

            LabApi.Features.Console.Logger.Info($"[ContainmentEngineer] Роль зарегистрирована. ID = {_role.Id}");
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
            ContainmentEngineerPlayers.Clear();


        }
        private static void OnChangedRole(PlayerChangedRoleEventArgs ev)
        {
            ContainmentEngineerPlayers.Remove(ev.Player);
        }
        public static bool IsContainmentEngineer(Player player)
        {
            return player != null && ContainmentEngineerPlayers.Contains(player);
        }
    }
    public class ContainmentEngineerRole : EventCustomRole
    {
        public override int Id { get; set; }

        public override string Name { get; set; } =
            "<color=#BFFF00>Инженер по системам сдерживания</color>";

        public override bool OverrideRoleName { get; set; } = true;

        public override string Nickname { get; set; } = "";

        public override string CustomInfo { get; set; } = "";

        public override string BadgeName { get; set; } = "";

        public override string BadgeColor { get; set; } = "";

        public override RoleTypeId Role { get; set; } =
            RoleTypeId.Scientist;

        public override Team? Team { get; set; } =
            PlayerRoles.Team.Scientists;

        public override RoleTypeId RoleAppearance { get; set; } =
            RoleTypeId.Scientist;

        public override List<Team> IsFriendOf { get; set; } = new List<Team>();

        public override HealthBehaviour Health { get; set; } =
            new HealthBehaviour
            {
                Amount = 150,
                Maximum = 150,
            };

        public override AhpBehaviour Ahp { get; set; } =
            new AhpBehaviour
            {
                Amount = 100,
                Limit = 100,
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

        public override bool CanEscape { get; set; } = true;

        public override Dictionary<string, string> RoleAfterEscape { get; set; } =
            new Dictionary<string, string>();

        public override UnityEngine.Vector3 Scale { get; set; } =
            UnityEngine.Vector3.one;

        public override string SpawnBroadcast { get; set; } =
            "Вы инженер по системам сдерживания. Благодаря своим знаниям вам будет легче выжить!";

        public override ushort SpawnBroadcastDuration { get; set; } = 10;

        public override string SpawnHint { get; set; } = "";

        public override float SpawnHintDuration { get; set; } = 0f;

        public override Dictionary<ItemCategory, sbyte> CustomInventoryLimits { get; set; } = new Dictionary<ItemCategory, sbyte>();

        public override List<ItemType> Inventory { get; set; } = new List<ItemType>
            { 
                ItemType.KeycardContainmentEngineer,
                ItemType.Medkit,
                ItemType.Radio,
                ItemType.Adrenaline
            };
            
        public override List<uint> CustomItemsInventory { get; set; } = new List<uint>();

        public override Dictionary<ItemType, ushort> Ammo { get; set; } = new Dictionary<ItemType, ushort>();

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
                SpawnRoles = new List<RoleTypeId>
                {
                    RoleTypeId.Scientist
                },
                SpawnRooms = new List<string>(),
                SpawnZones = new List<FacilityZone>()
            };

        public override List<object> CustomFlags { get; set; } =
            new List<object>();

        // UCR не должен самостоятельно спавнить эту роль.
        public override bool IgnoreSpawnSystem { get; set; } = true;
    }
}
