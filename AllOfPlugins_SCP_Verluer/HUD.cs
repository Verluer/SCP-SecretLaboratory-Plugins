using LabApi.Features.Wrappers;
using RueI.API;
using RueI.API.Elements;
using RueI.API.Elements.Enums;

namespace AllOfPlugins_SCP_Verluer
{
    public static class HUD
    {
        private static readonly Tag ProximityModeTag =
            new Tag("proximity_mode");

        public static void ShowProximity(ReferenceHub player)
        {
            BasicElement element = new BasicElement(
                200f,
                "<space=-1000><color=yellow>Proximity SCP Chat Active</color>")
            {
                ZIndex = 10,
                VerticalAlign = VerticalAlign.Down
            };

            RueDisplay.Get(player).Show(
                ProximityModeTag,
                element);
        }

        public static void HideProximity(ReferenceHub player)
        {
            RueDisplay.Get(player).Remove(
                ProximityModeTag);
        }
    }
}