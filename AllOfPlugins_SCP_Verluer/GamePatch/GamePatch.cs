using HarmonyLib;
using VoiceChat.Networking;


namespace AllOfPlugins_SCP_Verluer.GamePatch
{
    public class GamePatch
    {
        public static void Enable(Harmony harmony)
        {
            GiveSpawnItem.Enable();
            Scp.ScpProximityVoice.Enable();
            Scp.Scp049_2Punishment.Enable(harmony);
            WaveRespawn.Enable(harmony);
            Scp.CountSpawnSCP.Enable(harmony);
            Warhead.Enable(harmony);
        }
        public static void Disable()
        {
            GiveSpawnItem.Disable();
            Scp.ScpProximityVoice.Disable();
            Scp.Scp049_2Punishment.Disable();
            WaveRespawn.Disable();
            Scp.CountSpawnSCP.Disable();
            Warhead.Disable();
        }
    }
}
