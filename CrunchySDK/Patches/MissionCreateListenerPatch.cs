using System;
using Game.SceneInfo;
using Harmony;
using MelonLoader;
using Procedural.Scripts;
using UI.Mission_Terminal;
using Valve.VR.InteractionSystem;

namespace CrunchySDK
{
    [HarmonyPatch(typeof(MissionBuilder), nameof(MissionBuilder.SetSceneInfoToLoad))]
    public class MissionCreateListenerPatch
    {
        public static bool Prefix(GeneratorPhotonView __instance, ref SceneInfoInstance sceneInfoInstance)
        {
            MelonLogger.Log(ConsoleColor.Magenta,$"{sceneInfoInstance.mapKey}   {sceneInfoInstance.variantKey}");
            return true;
        }
    }
}