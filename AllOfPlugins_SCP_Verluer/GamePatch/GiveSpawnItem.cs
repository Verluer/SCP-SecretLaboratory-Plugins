using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;
using LabApi.Features.Wrappers;
using MEC;
using PlayerRoles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AllOfPlugins_SCP_Verluer.GamePatch
{
    public class GiveSpawnItem
    {
        private static readonly HashSet<uint> CoinScheduled = new HashSet<uint>();
        public static void Enable()
        {
            PlayerEvents.ChangedRole += OnChangedRole;

            PlayerEvents.Escaped += OnEscaped;
        }

        public static void Disable()
        {
            PlayerEvents.ChangedRole -= OnChangedRole;

            PlayerEvents.Escaped -= OnEscaped;
            CoinScheduled.Clear();
        }

        private static void OnChangedRole(PlayerChangedRoleEventArgs ev)
        {
            bool giveCoin =
                ev.NewRole.RoleTypeId == RoleTypeId.ClassD ||
                ev.NewRole.RoleTypeId == RoleTypeId.Scientist ||
                ev.NewRole.RoleTypeId == RoleTypeId.FacilityGuard;

            if (!giveCoin)
                return;

            uint playerId = ev.Player.NetworkId;

            if (!CoinScheduled.Add(playerId))
                return;

            Timing.CallDelayed(0.5f, () =>
            {
                CoinScheduled.Remove(playerId);

                if (ev.Player == null)
                    return;

                if (ev.Player.Items.Any(x => x.Type == ItemType.Coin))
                    return;

                ev.Player.AddItem(ItemType.Coin);
            });
        }
        private static void OnEscaped(PlayerEscapedEventArgs ev)
        {
            LabApi.Features.Console.Logger.Info($"[GiveSpawnItem] Escaped: {ev.Player.Nickname}, Role = {ev.Player.Role}");
            if (ev.Player.Role == RoleTypeId.ChaosConscript)
            {
                ev.Player.AddItem(ItemType.GunLogicer);
                ev.Player.AddAmmo(ItemType.Ammo762x39, 120);
                ev.Player.AddAmmo(ItemType.Ammo556x45, 120);

            }
            if (ev.Player.Role == RoleTypeId.NtfSpecialist)
            {
                ev.Player.AddItem(ItemType.GunFRMG0);
                ev.Player.AddAmmo(ItemType.Ammo762x39, 120);
                ev.Player.AddAmmo(ItemType.Ammo556x45, 120);
            }
        }
        public static void GiveCoin(ReferenceHub hubsender)
        {
            if (hubsender.roleManager.CurrentRole.RoleTypeId == RoleTypeId.Tutorial)
            {
                Player.Get(hubsender).AddItem(ItemType.Coin);
            }
        }
    }
}
