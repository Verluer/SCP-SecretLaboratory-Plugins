using HarmonyLib;
using VoiceChat.Networking;


namespace AllOfPlugins_SCP_Verluer.GamePatch
{
    public class GamePatch
    {
        public static void Enable(Harmony harmony)
        {
            GiveSpawnItem.Enable();
            ScpProximityVoice.Enable();
            Scp049_2Punishment.Enable(harmony);
        }
        public static void Disable()
        {
            GiveSpawnItem.Disable();
            ScpProximityVoice.Disable();
            Scp049_2Punishment.Disable();
        }
    }
}
