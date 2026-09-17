using AllOfPlugins_SCP_Verluer.EventModule;
using AllOfPlugins_SCP_Verluer.GamePatch;
using LabApi.Events.CustomHandlers;
using LabApi.Features.Console;
using System.Collections.Generic;
using UnityEngine;
using UserSettings.ServerSpecific;

public class MyKeybind : CustomEventsHandler
{
    private static readonly int[] KeybindId = { 1, 2, 3 };
    private static readonly HashSet<ReferenceHub> PressedKeys =
    new HashSet<ReferenceHub>();

    public override void OnServerWaitingForPlayers()
    {
        ServerSpecificSettingsSync.DefinedSettings = new ServerSpecificSettingBase[]
        {
            new SSGroupHeader("SCP Настройки"),

            new SSKeybindSetting(
                KeybindId[0],
                "Кнопка обмена SCP-ролями",
                KeyCode.Y,
                hint: "Нажмите Y"
            ),
            new SSKeybindSetting(
                KeybindId[1],
                "Говорить как обычный человек",
                KeyCode.V,
                hint: "Удерживайте V"
            ),
              new SSGroupHeader("Scp-261 Event Настройки"),

            new SSKeybindSetting(
                KeybindId[2],
                "Получить Coin",
                KeyCode.U,
                hint: "Нажмите U"
            )

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
                SwapScpRole.MethodSwapRole(sender);
            }

            return;
        }

        if (setting.SettingId == KeybindId[1])
        {
            ScpProximityVoice.HandleKey(
                sender,
                keybind.SyncIsPressed);

            return;
        }
        if (setting.SettingId == KeybindId[2])
        {
            if (keybind.SyncIsPressed)
            {
                GiveSpawnItem.GiveCoin(sender);
            }

            return;
        }
    }
}
