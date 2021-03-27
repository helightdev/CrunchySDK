using Harmony;
using UI.Menu;
using UnityEngine;

namespace CrunchySDK
{
    [HarmonyPatch(typeof(MenuInGame), nameof(MenuInGame.Pause))]
    public class MenuPatch
    {
        public static bool isCrunchyMenuOpen = false;
        
        public static bool Prefix(MenuInGame __instance)
        {
            if (__instance.isPaused && !isCrunchyMenuOpen)
            {
                isCrunchyMenuOpen = true;
                CrunchySdk.instance.CreateUI();
                return true;
            }  if (isCrunchyMenuOpen)
            {
                isCrunchyMenuOpen = false;
                Object.Destroy(CrunchySdk.instance.currentUi);
                return false;
            }

            return true;
        }
    }
}