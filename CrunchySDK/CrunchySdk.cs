using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using CrunchySDK.Reactive;
using Game.Objectives;
using Game.SceneInfo;
using Harmony;
using Interaction.Hands.Misc;
using Interaction.VR;
using Library.Actions;
using MelonLoader;
using Networking;
using Networking.PlayerSpawning;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using Players;
using Procedural.Scripts;
using Procedural.Scripts.WaveFunctionCollapse;
using Props.Truck;
using Spawning;
using UI;
using UI.Mission_Terminal;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Valve.VR.InteractionSystem;
using Object = UnityEngine.Object;
using Player = Valve.VR.InteractionSystem.Player;
using PlayerSpawner = Networking.PlayerSpawning.PlayerSpawner;

namespace CrunchySDK
{
    public class CrunchySdk : MelonMod
    {
        public static CrunchySdk instance;
        public Dictionary<string,AssetBundle> AssetBundles { get; internal set; }
        public static InfiniteMap Map{ get; internal set; }

        //Prefabs
        public GameObject uiPrefab;
        public GameObject broadcastPrefab;

        internal GameObject currentUi;
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

            SceneManager.activeSceneChanged += (arg0, scene) =>
            {
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
                var self = CrunchPlayer.GetSelf();
                self.Position = (self.Position += (Vector3.forward * 10));
            };
            
            Events.Instance.PlayerSpawnEvent += ev =>
            {
                MelonLogger.Log($"Player has been spawned: {ev.CrunchPlayer.name}");
            };
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

        public override void OnFixedUpdate()
        {
            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.Q)) QuickJoin();
            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.I)) MelonLogger.Log(CrunchPlayer.GetSelf());
            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.B))
                CreateTimedBroadcast(CrunchPlayer.GetSelf(), "This is an example broadcast!", null, null);
        }

        public async Task QuickJoin()
        {
            RoomUIManager roomManager = null;
            await Enqueue(() => {
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
        
        public void CreateUI()
        {
            var playerTransform = Player.instance.leftHand.transform;
            currentUi = Object.Instantiate(uiPrefab, playerTransform.TransformPoint(0.0f,0.0f, 0.5f), playerTransform.rotation);
            var euler = currentUi.transform.rotation.eulerAngles;
            euler.x += 45;
            currentUi.AddComponent<UIPointerAttach>();
            
            currentUi.transform.rotation = Quaternion.Euler(euler);
            currentUi.transform.SetParent(playerTransform);
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

        public async Task CreateTimedBroadcast(Vector3 position, Quaternion rotation, string text, Vector3? offset, TimeSpan? timeSpan,
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
            await CreateTimedBroadcast(player.HeadPosition, player.transform.rotation, text, Vector3.forward * 2, timeSpan, color);
        }
        

        #endregion
    }
}