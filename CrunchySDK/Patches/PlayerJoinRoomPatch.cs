using System;
using Harmony;
using MelonLoader;
using Networking.PlayerSpawning;
using Procedural.Scripts;
using Valve.VR.InteractionSystem;

namespace CrunchySDK
{
    
    [HarmonyPatch(typeof(GeneratorPhotonView), nameof(GeneratorPhotonView.OnPlayerEnteredRoom))]
    public class PlayerJoinRoomPatch
    {
        public static bool Prefix(GeneratorPhotonView __instance, ref Player newPlayer)
        {
            MelonLogger.Log(ConsoleColor.Magenta,$"Player {newPlayer.name} joined");
            return true;
        }
    }
}