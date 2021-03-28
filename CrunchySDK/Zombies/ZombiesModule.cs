using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CrunchySDK.UI;
using MelonLoader;
using UnityEngine;
using UnityEngine.Assertions.Comparers;
using UnityEngine.SceneManagement;
using Valve.VR.InteractionSystem;
using Object = UnityEngine.Object;

namespace CrunchySDK.Zombies
{
    public class ZombiesModule
    {
        public int Round { get; set; } = 0;
        
        public void RegisterListener()
        {
            Events.Instance.PostProcessingStepEvent += OnGeneratorStep;
            Events.Instance.MissionStartedEvent += OnMissionStart;
            Events.Instance.MissionEndEvent += MissionEnd;
        }

        public void UnregisterListener()
        {
            Events.Instance.PostProcessingStepEvent -= OnGeneratorStep;
            Events.Instance.MissionStartedEvent -= OnMissionStart;
            Events.Instance.MissionEndEvent -= MissionEnd;
        }

        public void MissionEnd(NoArgs args)
        {
            
        }

        public void OnMissionStart(NoArgs args)
        {
            Round = 0;
            MelonCoroutines.Start(OnRoundCoroutine());
        }
        
        public void OnGeneratorStep(PostProcessingStepEventArgs args)
        {
            if (args.StepType == PostProcessingStepEnum.StepSpawnEnemies) args.Allow = false;
        }


        public void ClearDebris()
        {
            
        }
        
        public IEnumerator OnRoundCoroutine()
        {
            CrunchPlayer.GetSelf().SendBroadcast($"First wave will start in 30 seconds!");
            yield return new WaitForSeconds(30f);
            while (CrunchySdk.IsInMission)
            {
                MelonLogger.Log($"Enemies alive: {CrunchEnemy.LivingEnemies}");
                if (CrunchEnemy.LivingEnemies == 0)
                {
                    foreach (var gameObject in GameObject.FindObjectsOfType<GameObject>())
                    {
                        if (gameObject.name.Contains("Alien"))
                        {
                            if (!gameObject.TryGetComponent(typeof(CrunchEnemy), out var ignored))
                            {
                                Object.Destroy(gameObject);
                            }
                        }
                    }
                    
                    //New Round
                    Round++;
                    ClearDebris();
                    int enemyCount = 2 * Round;
                    if (Round >= 10) enemyCount += 3;
                    MelonLogger.Log("[1]");
                    var walkableAreas = CrunchySdk.Map.slots.Values.Select(x => x.room).ToList();
                    MelonLogger.Log(walkableAreas.ToString());
                    MelonLogger.Log("[2]");
                    CrunchPlayer.GetSelf().CurrentRoom?.neighbors?.ForEach(neighbor =>
                    {
                        if (neighbor.room == null) return;
                        walkableAreas.Remove(neighbor.room);
                    });
                    MelonLogger.Log("[3]");
                    var playerLoc = CrunchPlayer.GetSelf().Position;
                    var spawnPoints = walkableAreas.SelectMany(x =>
                    {
                        try
                        {
                            return x?.GetSpawnPoints() ?? new List<Vector3>();
                        }
                        catch (Exception e)
                        {
                            return new List<Vector3>();
                        }
                    }).Where(x => Vector3.Distance(playerLoc, x) >= 25).ToArray();
                    MelonLogger.Log("[4]");
                    CrunchEnemy.Spawn(spawnPoints, enemyCount);
                    MelonLogger.Log("[5]");
                    CrunchPlayer.AllPlayers.ForEach(x => x.Health = x.MaxHealth);
                    CrunchPlayer.GetSelf().SendBroadcast($"Round {Round-1} completed!\nNext Wave incoming...");
                }
                else
                {
                    try
                    {
                        var playerPositions = CrunchPlayer.AllPlayers.ToDictionary(x => x, x => x.Position);
                        foreach (var crunchEnemy in CrunchEnemy.Enemies)
                        {
                            var pos = crunchEnemy.Position;
                            var orderedDict = playerPositions.ToDictionary(
                                e => Vector3.Distance(e.Value, pos),
                                e => e.Key,
                                new FloatComparer());
                            crunchEnemy.Hunt(orderedDict.ToArray()[0].Value.transform);
                        }
                    }
                    catch (Exception e)
                    {
                        MelonLogger.LogError(e.ToString());
                    }
                }
                yield return new WaitForSeconds(1);
            }
        }
    }
    
}