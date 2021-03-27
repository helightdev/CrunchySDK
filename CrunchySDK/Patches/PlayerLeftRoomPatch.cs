using System;
using Harmony;
using MelonLoader;
using Procedural.Scripts;
using Valve.VR.InteractionSystem;

namespace CrunchySDK
{
    [HarmonyPatch(typeof(GeneratorPhotonView), nameof(GeneratorPhotonView.OnPlayerLeftRoom))]
    public class PlayerLeftRoomPatch
    {
        public static bool Prefix(GeneratorPhotonView __instance, ref Player otherPlayer)
        {
            MelonLogger.Log(ConsoleColor.Magenta, $"Player {otherPlayer.name} left");
            return true;
        }
    }
}