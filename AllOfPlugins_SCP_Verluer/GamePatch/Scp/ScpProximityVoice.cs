using CentralAuth;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;
using PlayerRoles;
using PlayerRoles.Voice;
using System.Collections.Generic;
using VoiceChat;
using VoiceChat.Networking;

namespace AllOfPlugins_SCP_Verluer.GamePatch.Scp
{
    public static class ScpProximityVoice
    {
        private static readonly Dictionary<ReferenceHub, bool> VoiceEnabled =
            new Dictionary<ReferenceHub, bool>();

        private static readonly HashSet<ReferenceHub> PressedKeys =
            new HashSet<ReferenceHub>();

        public static void Enable()
        {
            PlayerEvents.SendingVoiceMessage += OnSendingVoiceMessage;
        }

        public static void Disable()
        {
            PlayerEvents.SendingVoiceMessage -= OnSendingVoiceMessage;

            VoiceEnabled.Clear();
            PressedKeys.Clear();
        }

        public static void HandleKey(
            ReferenceHub player,
            bool isPressed)
        {
            if (player == null)
                return;

            if (isPressed)
            {
                if (!PressedKeys.Add(player))
                    return;

                SetVoiceEnabled(
                    player,
                    !IsVoiceEnabled(player));

                return;
            }

            PressedKeys.Remove(player);
        }

        public static bool IsVoiceEnabled(
            ReferenceHub player)
        {
            return VoiceEnabled.TryGetValue(
                player,
                out bool enabled) && enabled;
        }

        public static void SetVoiceEnabled(
            ReferenceHub player,
            bool enabled)
        {
            if (player == null)
                return;

            VoiceEnabled[player] = enabled;

            if (enabled)
                Core.HUD.ShowProximity(player);
            else
                Core.HUD.HideProximity(player);
        }

        private static void OnSendingVoiceMessage(
            PlayerSendingVoiceMessageEventArgs ev)
        {
            VoiceMessage message = ev.Message;

            if (message.Speaker == null)
                return;

            ReferenceHub speaker = message.Speaker;

            if (!IsVoiceEnabled(speaker))
                return;

            if (speaker.roleManager.CurrentRole.Team != Team.SCPs)
                return;

            if (message.Channel != VoiceChatChannel.ScpChat)
                return;

            ev.IsAllowed = false;

            SendProximity(message);
        }

        private static void SendProximity(
            VoiceMessage message)
        {
            ReferenceHub speaker = message.Speaker;

            if (speaker == null)
                return;

            message.Channel = VoiceChatChannel.Proximity;

            foreach (ReferenceHub receiver in ReferenceHub.AllHubs)
            {
                if (receiver == null ||
                    receiver == speaker)
                {
                    continue;
                }

                if (receiver.Mode ==
                    ClientInstanceMode.DedicatedServer)
                {
                    continue;
                }

                if (receiver.connectionToClient == null)
                    continue;

                if (!(receiver.roleManager.CurrentRole
                    is IVoiceRole receiverVoiceRole))
                {
                    continue;
                }

                VoiceChatChannel channel =
                    receiverVoiceRole.VoiceModule.ValidateReceive(
                        speaker,
                        VoiceChatChannel.Proximity);

                if (channel == VoiceChatChannel.None)
                    continue;

                receiver.connectionToClient.Send<VoiceMessage>(
                    message,
                    0);
            }
        }
    }
}