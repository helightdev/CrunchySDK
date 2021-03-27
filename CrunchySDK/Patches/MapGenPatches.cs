using System;
using Harmony;
using MelonLoader;
using Photon.Pun;
using Procedural.Scripts;
using Procedural.Scripts.WaveFunctionCollapse;
using UnityEngine;

namespace CrunchySDK.Patches
{
    [HarmonyPatch(typeof(MapBuilder), nameof(MapBuilder.PerformPostProcessing))]
    public class PerformPostProcessingPatch
    {
        public static bool Prefix(MapBuilder __instance)
        {
            __instance.Map.gatedEntrancePrefab = __instance.compoundConfig.gateEntrace;
            __instance.Map.postBuildDetailSolver.enemyPrefab = __instance.compoundConfig.enemyPrefab;
            __instance.Map.postBuildDetailSolver.interiorModuleData = __instance.ModuleData;
            __instance.Map.postBuildDetailSolver.midpointGateSlots = __instance.Map.midpointGateSlots;
            __instance.Map.postBuildDetailSolver.InitializeDetailSolver(__instance, __instance.propSolver, __instance.shouldBuildProps);
            CrunchySdk.Map = __instance.Map;
            Events.Instance.InvokePreProcessingEvent(__instance, out var allowPreProcess);
            if (!allowPreProcess) return false;
            foreach (MapBuilder.ProcessingStep processingStep in __instance.postProcessingSteps)
            {
                if (!processingStep.ignore && (!processingStep.onlyMaster || PhotonNetwork.IsMasterClient))
                {
                    if (processingStep.pauseBefore)
                    {
                        Debug.Break();
                    }
                    Events.Instance.InvokePostProcessingStepEvent(processingStep, __instance, out var allowPostProcess);
                    if (!allowPostProcess) continue;
                    processingStep.step.Process(__instance.Map.postBuildDetailSolver);
                    Events.Instance.InvokeFinalizePostProcessingStepEvent(processingStep, __instance);
                    if (processingStep.pauseAfter)
                    {
                        Debug.Break();
                    }
                }
            }
            return false;
        }
    }
}