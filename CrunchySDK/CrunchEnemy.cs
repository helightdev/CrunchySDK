using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Agent;
using Interaction.Hands.Hand.SteamVR.Overrides;
using Library.Distribution;
using Procedural.Scripts.ObjectBuilder;
using Spawning;
using UnityEngine;
using Object = System.Object;
using Random = System.Random;

namespace CrunchySDK
{
    public class CrunchEnemy : MonoBehaviour
    {

        public static int LivingEnemies = 0;

        public static async Task<CrunchEnemy> Spawn(Vector3? position)
        {
            var completionSource = new TaskCompletionSource<CrunchEnemy>();
            await CrunchySdk.Enqueue(() =>
            {
                var prefab =  GetController.enemyPrefab;
                var sharedInstance = ObjectBuilder.sharedInstance;
                sharedInstance.AddToBuildQueueNetworked(prefab.name,
                    position.GetValueOrDefault(PointUtility.GetPointsEvenlyAroundCenter(1).First()),
                    Quaternion.identity, null, delegate(GameObject go)
                    {
                        var isRegistered = go.TryGetComponent(out CrunchEnemy enemy); 
                        if (!isRegistered) enemy = go.AddComponent<CrunchEnemy>();
                        completionSource.SetResult(enemy);
                    }, null);
            });
            return await completionSource.Task;
        }
        
        public static async Task<CrunchEnemy[]> Spawn(Vector3? position, int? amount)
        {
            var finalAmount = amount.GetValueOrDefault(1);
            var positions = PointUtility.GetPointsEvenlyAroundCenter(finalAmount);
            var enemies = new List<Task<CrunchEnemy>>();
            for (int i = 0; i < finalAmount; i++) enemies.Add(Spawn(position.GetValueOrDefault(positions[i])));
            await Task.WhenAll(enemies);
            return enemies.Select(x => x.Result).ToArray();
        }
        
        public static async Task<CrunchEnemy[]> Spawn(Vector3[] positionPool, int? amount)
        {
            var finalAmount = amount.GetValueOrDefault(1);
            var positions = new Vector3[amount.GetValueOrDefault(1)];
            var random = new Random();
            for (var i = 0; i < positions.Length; i++)
            {
                positions[i] = positionPool[random.Next(0, positionPool.Length)];
            }

            var enemies = new List<Task<CrunchEnemy>>();
            for (int i = 0; i < finalAmount; i++) enemies.Add(Spawn(positions[i]));
            await Task.WhenAll(enemies);
            return enemies.Select(x => x.Result).ToArray();
        }

        public static CrunchEnemy[] Enemies => FindObjectsOfType<CrunchEnemy>();
        
        private static ReinforcementController GetController => FindObjectOfType<ReinforcementController>();

        private AgentFacade _facade;
        private Transform _transform;

        public Vector3 Position
        {
            get => _transform.position;
            set => _transform.position = value;
        }
        
        public void Start()
        {
            _transform = transform;
            _facade = GetComponent<AgentFacade>();
        }

        public void Hunt(Transform transform)
        {
            _facade.SetHuntTarget(transform);
        }
    }
}