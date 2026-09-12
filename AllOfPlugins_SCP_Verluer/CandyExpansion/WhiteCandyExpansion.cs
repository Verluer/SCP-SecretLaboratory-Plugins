using CustomPlayerEffects;
using HarmonyLib;
using InventorySystem;
using InventorySystem.Items.Usables.Scp330;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;
using MEC;
using System.Collections.Generic;
using System.Reflection;

namespace AllOfPlugins_SCP_Verluer.CandyExpansion
{
    public static class WhiteCandyExpansion
    {
        private static readonly HashSet<ReferenceHub> _blockedPlayers =
            new HashSet<ReferenceHub>();

        private static readonly Dictionary<ReferenceHub, CoroutineHandle> _unblockCoroutines =
            new Dictionary<ReferenceHub, CoroutineHandle>();

        public static void Enable(Harmony _harmony)
        {
            MethodInfo method = AccessTools.Method(typeof(HauntedCandyWhite), "ServerApplyEffects");


            if (method == null) return;


            PlayerEvents.PickingUpItem += OnPickingUpItem;
            PlayerEvents.PickingUpScp330 += OnPickingUpScp330;
            PlayerEvents.PickingUpArmor += OnPickingUpArmor;

            _harmony.Patch(method, prefix: new HarmonyMethod(typeof(WhiteCandyExpansion), nameof(ServerApplyEffectsPrefix)));
        }

        public static void Disable()
        {
            PlayerEvents.PickingUpItem -= OnPickingUpItem;
            PlayerEvents.PickingUpScp330 -= OnPickingUpScp330;
            PlayerEvents.PickingUpArmor -= OnPickingUpArmor;

            foreach (CoroutineHandle coroutine in _unblockCoroutines.Values)
            {
                if (coroutine.IsRunning)
                    Timing.KillCoroutines(coroutine);
            }

            _unblockCoroutines.Clear();
            _blockedPlayers.Clear();
        }
        private static bool ServerApplyEffectsPrefix(ReferenceHub hub)
        {
            ApplyWhiteCandyEffect(hub);
            return false;

        }
        public static void ApplyWhiteCandyEffect(ReferenceHub hub)
        {
            _blockedPlayers.Add(hub);

            hub.playerEffectsController.EnableEffect<Invisible>(15f);
            hub.playerEffectsController.EnableEffect<Ghostly>(15f);

            hub.inventory.ServerDropEverything();

            if (_unblockCoroutines.TryGetValue(hub, out CoroutineHandle oldCoroutine))
            {
                if (oldCoroutine.IsRunning)
                    Timing.KillCoroutines(oldCoroutine);
            }

            CoroutineHandle newCoroutine = Timing.RunCoroutine(UnblockPlayer(hub));
            _unblockCoroutines[hub] = newCoroutine;
        }
        private static IEnumerator<float> UnblockPlayer(ReferenceHub hub)
        {
            yield return Timing.WaitForSeconds(15f);

            _blockedPlayers.Remove(hub);
            _unblockCoroutines.Remove(hub);
        }

        private static void OnPickingUpItem(PlayerPickingUpItemEventArgs ev)
        {
            if (_blockedPlayers.Contains(ev.Player.ReferenceHub))
                ev.IsAllowed = false;
        }

        private static void OnPickingUpScp330(PlayerPickingUpScp330EventArgs ev)
        {
            if (_blockedPlayers.Contains(ev.Player.ReferenceHub))
                ev.IsAllowed = false;
        }
        private static void OnPickingUpArmor(PlayerPickingUpArmorEventArgs ev)
        {
            if (_blockedPlayers.Contains(ev.Player.ReferenceHub))
                ev.IsAllowed = false;
        }
    }
}
