using System;

namespace CrunchySDK
{
    public enum PostProcessingStepEnum
    {
        StepSubdivideInterior,
        StepCarveStairs,
        StepCreateRooms,
        StepCleanupRoomWalkability,
        StepPlaceFloorTiles,
        StepInteriorDoorSolve,
        StepPlaceReverbRooms,
        StepCreateBuildingsFromRooms,
        StepExteriorDoorSolve,
        StepObjectiveIntervention,
        StepCarveCover,
        StepCeilingLightSolve,
        StepGenerateInterior,
        StepSpawnEnemies,
        StepPlaceClaymores,
        StepPlaceGateEntrances,
        Other
    }

    public class PostProcessingStepEnumFactory
    {
        public static PostProcessingStepEnum Get(string name) => Enum.TryParse<PostProcessingStepEnum>(name, true, out var step) ? step : PostProcessingStepEnum.Other;
    }
}