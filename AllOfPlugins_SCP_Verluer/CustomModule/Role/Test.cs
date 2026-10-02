using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;
using LabApi.Features.Wrappers;
using MapGeneration;
using MEC;
using PlayerRoles;
using PlayerRoles.RoleAssign;
using PlayerStatsSystem;
using System.Collections.Generic;
using UncomplicatedCustomRoles.API.Enums;
using UncomplicatedCustomRoles.API.Features;
using UncomplicatedCustomRoles.API.Features.Behaviour;
using UncomplicatedCustomRoles.Extensions;
using UncomplicatedCustomRoles.Manager;

namespace AllOfPlugins_SCP_Verluer.CustomModule.Role
{
    public class Test
    {
        private static readonly HashSet<Player> TestPlayers = new();

        public static TestRole _role;

        public static bool ApplyTestRole(ReferenceHub Playerhub,int currentHealth,int maxHealth,int currentAhp)
        {
            if (_role == null)
            {
                LabApi.Features.Console.Logger.Error(
                    "[Test] Роль не зарегистрирована.");

                return false;
            }

            Player target = null;
            target = Player.Get(Playerhub);


            if (target == null)
            {
                LabApi.Features.Console.Logger.Warn(
                    $"[Test] Игрок не найден.");

                return false;
            }

            ReferenceHub hub = target.ReferenceHub;

            if (hub == null)
            {
                LabApi.Features.Console.Logger.Error(
                    $"[Test] У игрока {target.Nickname} отсутствует ReferenceHub.");

                return false;
            }

            PlayerRoleBase currentRole = hub.roleManager.CurrentRole;

            if (currentRole == null)
            {
                LabApi.Features.Console.Logger.Error(
                    $"[Test] У игрока {target.Nickname} отсутствует текущая роль.");

                return false;
            }


            RoleTypeId currentRoleType = currentRole.RoleTypeId;
            _role.Role = currentRole.RoleTypeId;
            _role.Team = PlayerRoles.Team.Flamingos;
            _role.RoleAppearance = currentRole.RoleTypeId;

            _role.Health = new HealthBehaviour

            {
                Maximum = maxHealth,
                Amount = currentHealth,
            };

            _role.Ahp = new AhpBehaviour
            {
                Amount = currentAhp,
                Limit = 450,
                Decay = 0,
            };

            LabApi.Features.Console.Logger.Info(
                $"[Test] Найден игрок: {target.Nickname}");

            LabApi.Features.Console.Logger.Info(
                $"[Test] Исходная роль: {currentRole.RoleTypeId}");

            LabApi.Features.Console.Logger.Info(
                $"[Test] Назначение: Role={_role.Role}, " +
                $"Team={_role.Team}, " +
                $"Appearance={_role.RoleAppearance}");


            var oldPosition = target.Position;

            target.SetCustomRole(_role);

            TestPlayers.Add(target);

            LabApi.Features.Console.Logger.Info(
                $"[Test] Кастомная роль применена к {target.Nickname}.");

            return true;
        }

        public static void Enable()
        {
            if (_role != null)
            {
                LabApi.Features.Console.Logger.Warn(
                    "[Test] Модуль уже включён.");

                return;
            }

            _role = new TestRole
            {
                Id = CustomRole.GetFirstFreeId(111)
            };

            LoadStatusType status = CustomRole.Register(_role);

            if (status != LoadStatusType.Success)
            {
                LabApi.Features.Console.Logger.Error(
                    $"[Test] Не удалось зарегистрировать роль: {status}");

                _role = null;
                return;
            }

            LabApi.Features.Console.Logger.Info(
                $"[Test] Роль зарегистрирована. ID = {_role.Id}");

            PlayerEvents.ChangedRole += OnChangedRole;
        }

        public static void Disable()
        {
            PlayerEvents.ChangedRole -= OnChangedRole;

            _role = null;
            TestPlayers.Clear();
        }

        private static void OnChangedRole(PlayerChangedRoleEventArgs ev)
        {
            TestPlayers.Remove(ev.Player);
        }

        public static bool IsTestPlayer(Player player)
        {
            return player != null && TestPlayers.Contains(player);
        }
    }

    public class TestRole : EventCustomRole
    {
        public override int Id { get; set; }

        public override string Name { get; set; } =
            "<color=#FF96DE>Стреляй на поражение</color>";

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

        public override List<Team> IsFriendOf { get; set; } =
            new List<Team>();

        public override HealthBehaviour Health { get; set; } =
            new HealthBehaviour();

        public override AhpBehaviour Ahp { get; set; } =
            new AhpBehaviour();

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

        public override string SpawnBroadcast { get; set; } = "Вы - предатель. Вы предали сначала D-class, а теперь и SCP. Если о вашем предательстве узнают - то на вас будет охотится и SCP";

        public override ushort SpawnBroadcastDuration { get; set; } = 0;

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
            new List<object>();

        public override bool IgnoreSpawnSystem { get; set; } = true;
    }
}