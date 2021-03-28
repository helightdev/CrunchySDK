using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Agent;
using CrunchySDK.UI;
using Game.Objectives;
using Game.SceneInfo;
using Harmony;
using Library.Actions;
using MelonLoader;
using UnityEngine;
using Procedural.Scripts;
using Procedural.Scripts.WaveFunctionCollapse;
using Props.Truck;
using UI;
using UI.Mission_Terminal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Valve.VR.InteractionSystem;
using Object = UnityEngine.Object;
using Player = Valve.VR.InteractionSystem.Player;

namespace CrunchySDK
{
    public class CrunchySdk : MelonMod
    {
        public static CrunchySdk instance;
        public Dictionary<string, AssetBundle> AssetBundles { get; internal set; }
        public static InfiniteMap Map { get; internal set; }
        public static bool IsInMission { get; internal set; }

        //Prefabs
        public GameObject uiPrefab;
        public GameObject broadcastPrefab;

        internal CrunchySDKUI currentUi = new CrunchySDKUI();
        internal bool assetsLoaded = false;

        public CrunchySdk() => instance = this;

        public static async Task Enqueue(Action action)
        {
            var completionSource = new TaskCompletionSource<bool>();
            MelonCoroutines.Start(ActionUtility.DoNextFrame(() =>
            {
                action.Invoke();
                completionSource.SetResult(true);
            }));
            await completionSource.Task;
        }

        public override void OnApplicationStart()
        {
            MelonLogger.Log($"CrunchElementVersion: ${Application.version}");
            MelonLogger.Log($"UnityVersion: ${Application.unityVersion}");
            MelonLogger.Log("------------------------------");
            Application.logMessageReceived += (condition, trace, type) =>
            {
                if (type != LogType.Error && type != LogType.Exception)
                {
                    MelonLogger.Log(ConsoleColor.Yellow, $"CEL: {condition}");
                }
                else
                {
                    MelonLogger.Log(ConsoleColor.Yellow, $"CEL: {condition}\nStacktrace: {trace}");
                }
            };
            MelonLogger.Log("Redirected UnityLogOutput");
            CreateBundleDirectory();
            CreateDependenciesDirectory();
            MelonLogger.Log("Loading dependencies");
            Directory.GetFiles("Dependencies").Where(x => x.EndsWith(".dll")).ForEach(x =>
            {
                var assembly = System.Reflection.Assembly.Load(File.ReadAllBytes(x));
                MelonLogger.Log($"* Loaded dependency {assembly}");
            });

            SceneManager.activeSceneChanged += (arg0, scene) =>
            {
                MelonLogger.Log($"Switching scenes: {scene}");
                SpawnPlayerNetworkedInTruckPatch.hasExecutedStart = false;
            };

            Events.Instance.PreProcessingEvent += ev =>
            {
                MelonLogger.Log("Starting Map Processing");
                MelonLogger.Log($"Seed is {Seed.instance.seed}");
            };

            Events.Instance.PostProcessingStepEvent += ev =>
            {
                MelonLogger.Log($"   #! {ev.ProcessingStep.step.name}");
            };

            Events.Instance.MissionStartedEvent += ev =>
            {
                IsInMission = true;
                
            };

            Events.Instance.MissionEndEvent += ev =>
            {
                IsInMission = false;
            };

            Events.Instance.PlayerSpawnEvent += ev =>
            {
                MelonLogger.Log($"Player has been spawned: {ev.CrunchPlayer.name}");
            };

            MelonCoroutines.Start(ScanForEnemies());
        }

        public IEnumerator ScanForEnemies()
        {
            while (true)
            {
                var enemies = Object.FindObjectsOfType<AgentFacade>();
                CrunchEnemy.LivingEnemies = enemies.Length;
                enemies.ForEach(agent =>
                {
                    var isRegistered = agent.gameObject.TryGetComponent(typeof(CrunchEnemy), out var ignored);
                    if (!isRegistered)
                    {
                        var enemy = agent.gameObject.AddComponent<CrunchEnemy>();
                        Events.Instance.InvokeEnemySpawnEvent(enemy);
                    }
                });
                yield return new WaitForSeconds(0.1f);
            }
        }

        public override void OnLevelWasLoaded(int level)
        {
            if (!assetsLoaded)
            {
                assetsLoaded = true;
                LoadBundles();
                uiPrefab = AssetBundles["crunchysdkui"].LoadAsset<GameObject>("Assets/CrunchySdkCanvas.prefab");
                broadcastPrefab = AssetBundles["broadcast"].LoadAsset<GameObject>("Assets/Broadcast.prefab");
            }
        }
        
        public void LoadBundles()
        {
            MelonLogger.Log("Loading AssetBundles");
            AssetBundles = Directory.GetFiles("Bundles")
                .Where(x => x.EndsWith(".bundle"))
                .ToDictionary(x =>
                {
                    MelonLogger.Log($"   * {x}");
                    return x.Replace("\\", "/").Split('/').Last().Split('.')[0];
                }, AssetBundle.LoadFromFile);
            MelonLogger.Log(ConsoleColor.Yellow, $"Bundles: [{AssetBundles.Keys.Join(delimiter: ", ")}]");
            MelonLogger.Log("Finished loading AssetBundles");
        }

        public static void CreateBundleDirectory() => Directory.CreateDirectory("Bundles");
        public static void CreateDependenciesDirectory() => Directory.CreateDirectory("Dependencies");

        public override void OnFixedUpdate()
        {
            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.Q)) QuickJoin();
            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.I))
                MelonLogger.Log(CrunchPlayer.GetSelf());
            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.N))
            {
                var player = CrunchPlayer.GetSelf();
                player.MaxHealth = 9999999;
                player.Health = 9999999;
            }
                if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.B))
                CreateTimedBroadcast(CrunchPlayer.GetSelf(), "This is an example broadcast!", null, null);
        }

        public async Task QuickJoin()
        {
            RoomUIManager roomManager = null;
            await Enqueue(() =>
            {
                roomManager = Object.FindObjectOfType<RoomUIManager>();
                roomManager.Awake();
            });
            await Task.Delay(TimeSpan.FromMilliseconds(500));
            await Enqueue(roomManager.TransitionFromCreateToMission);
            await Task.Delay(TimeSpan.FromMilliseconds(1000));
            TruckListener truck = null;
            await Enqueue(() =>
            {
                var loader = Object.FindObjectOfType<SceneInfoLoader>();
                var scene = new SceneInfoInstance(loader.sceneInfoLookup, "badlands", "day");
                MissionManager.instance.CreateMission(scene);
                truck = Object.FindObjectOfType<TruckListener>();
                Player.instance.gameObject.GetComponent<Rigidbody>().MovePosition(new Vector3(-4.5f, 0.3f, -10f));
            });
            await Task.Delay(TimeSpan.FromMilliseconds(500));
            await Enqueue(truck.OnAllPlayersEntered.Invoke);
        }

        #region Broadcasts

        public GameObject CreateBroadcast(Vector3 position, Quaternion rotation, string text, Color? color)
        {
            var gameObject = GameObject.Instantiate(broadcastPrefab, position, rotation);
            var textComponent = gameObject.GetComponentInChildren<Text>();
            textComponent.text = text;
            textComponent.color = color.GetValueOrDefault(Color.white);
            return gameObject;
        }

        public async Task CreateTimedBroadcast(Vector3 position, Quaternion rotation, string text, Vector3? offset,
            TimeSpan? timeSpan,
            Color? color)
        {
            GameObject gameObject = null;
            await Enqueue(() =>
            {
                gameObject = CreateBroadcast(position, rotation, text, color);
                gameObject.transform.Translate(offset.GetValueOrDefault(Vector3.zero));
            });
            await Task.Delay(timeSpan.GetValueOrDefault(TimeSpan.FromSeconds(5)));
            await Enqueue(() => Object.Destroy(gameObject));
        }

        public async Task CreateTimedBroadcast(CrunchPlayer player, string text, TimeSpan? timeSpan,
            Color? color)
        {
            await CreateTimedBroadcast(player.HeadPosition, player.transform.rotation, text, Vector3.forward * 2,
                timeSpan, color);
        }

        #endregion
    }
}