using System;
using Harmony;
using MelonLoader;
using Procedural.Scripts;
using Valve.VR.InteractionSystem;

namespace CrunchySDK.Patches
{

    [HarmonyPatch(typeof(WFCGeneratorPlaymode), nameof(WFCGeneratorPlaymode.OnBuildSlotsComplete))]
    public class BuildAllSlotsPatch
    {
        public static bool Prefix(WFCGeneratorPlaymode __instance)
        {
            MelonLogger.Log(ConsoleColor.Magenta,$"Finished Map Construction");
            return true;
        }
    }
}