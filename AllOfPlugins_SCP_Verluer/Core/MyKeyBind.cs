using LabApi.Events.CustomHandlers;
using LabApi.Features.Console;
using System.Collections.Generic;
using UnityEngine;
using UserSettings.ServerSpecific;

namespace AllOfPlugins_SCP_Verluer.Core
{
    public class MyKeybind : CustomEventsHandler
    {
        private static readonly int[] KeybindId = { 1, 2, 3, 4 };
        private static  bool ScpHoodActive = true;

        public override void OnServerWaitingForPlayers()
        {
            ServerSpecificSettingsSync.DefinedSettings = new ServerSpecificSettingBase[]
            {
            
                new SSGroupHeader("SCP Настройки"),
            
                new SSKeybindSetting(KeybindId[0], "Кнопка обмена SCP-ролями", KeyCode.Y, hint: "Обе стороны для обмена должны один раз нажать кнопку обмена"),

                new SSKeybindSetting(KeybindId[1],"Говорить как обычный человек", KeyCode.V, hint: "При разговоре с включенным режимом - вас будут слышать другие игроки, но не будет слышать напарник"),
                
                new SSKeybindSetting(KeybindId[2],"Scp Hood", KeyCode.U, hint: "Включает Scp State Hood"),

                new SSGroupHeader("Scp-261 Event Настройки"),

            
                new SSKeybindSetting(KeybindId[3],"Получить Coin", KeyCode.I, hint: "При нажатии выдает монетку, если вы эвентовый автомат")

            };

            ServerSpecificSettingsSync.SendToAll();

            ServerSpecificSettingsSync.ServerOnSettingValueReceived += OnSettingValueReceived;
        }
        public void Dispose()
        {
            ServerSpecificSettingsSync.ServerOnSettingValueReceived -=
                OnSettingValueReceived;
        }
        private void OnSettingValueReceived(
            ReferenceHub sender,
            ServerSpecificSettingBase setting)
        {
            if (!(setting is SSKeybindSetting keybind))
                return;

            if (setting.SettingId == KeybindId[0])
            {
                if (keybind.SyncIsPressed)
                {
                    Features.SwapScpRole.MethodSwapRole(sender);
                }

                return;
            }

            if (setting.SettingId == KeybindId[1])
            {
                GamePatch.Scp.ScpProximityVoice.HandleKey(
                    sender,
                    keybind.SyncIsPressed);

                return;
            }
            if (setting.SettingId == KeybindId[2])
            {
                if (keybind.SyncIsPressed)
                {
                    if (ScpHoodActive)
                    {
                        HUD.HideScpHood(sender);
                        ScpHoodActive = false;
                    }
                    else
                    {
                        HUD.ScpHpHood();
                        ScpHoodActive = true;
                    }
                }
                return;
            }
            if (setting.SettingId == KeybindId[3])
            {
                if (keybind.SyncIsPressed)
                {
                    GamePatch.GiveSpawnItem.GiveCoin(sender);
                }

                return;
            }

        }
    }
}