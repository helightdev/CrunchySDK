using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Harmony;
using Library.Actions;
using MelonLoader;
using Networking;
using Networking.PlayerSpawning;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

namespace CrunchySDK
{
    [HarmonyPatch(typeof(PlayerSpawner), nameof(PlayerSpawner.SpawnPlayerNetworkedInTruck))]
    public class SpawnPlayerNetworkedInTruckPatch
    {
        public static bool hasExecutedStart = false;

        public static Dictionary<string, Player> photonPlayerCache = new Dictionary<string,Player>();

        public static bool Prefix(PlayerSpawner __instance)
        {
            Player[] playerList = PhotonNetwork.PlayerList;
            for (int i = 0; i < playerList.Length; i++)
            {
                if (playerList[i].IsLocal)
                {
                    int num = i;
                    __instance.StartCoroutine(ActionUtility.DoNextFrame(delegate()
                    {
                        //Delay for one second since some methods and events may still override positions
                        //before the player is finally spawned in the truck. This should make the method
                        //be executed on likely the first visible frame 
                        if (num == playerList.Length - 1 && !hasExecutedStart)
                        {
                            hasExecutedStart = true;
                            __instance.StartCoroutine(ActionUtility.DoNextFrame(Events.Instance.InvokeMissionStartedEvent));
                        }


                        var spawner = __instance.spawnername + num;
                        var photonPlayer = playerList[num];
                        //Spawn Player Method Inline
                        PlayerSpawner.IncrementPlayerCount();
                        Transform spawner2 = PlayerSpawner.GetSpawner(spawner);
                        if (spawner2 != null)
                        {
                            GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(__instance.playerLocal, spawner2.position,
                                __instance.YRotationOnly(spawner2.rotation));
                            MelonLogger.Log(gameObject);
                            int[] array = gameObject.GetComponent<RemoteInitializer>().AllocateIDs();
                            var crunch = gameObject.AddComponent<CrunchPlayer>();
                            crunch.ActorId = photonPlayer.ActorNumber;
                            crunch.UserId = photonPlayer.UserId;
                            crunch.UserName = photonPlayer.NickName;
                            __instance.photonView.RPC("SpawnRemotePlayer", RpcTarget.OthersBuffered, new object[]
                            {
                                PhotonNetwork.LocalPlayer.ActorNumber,
                                gameObject.transform.position,
                                gameObject.transform.rotation,
                                array
                            });
                            Events.Instance.InvokePlayerSpawnEvent(crunch);
                        }
                        //End
                    }));
                }
            }
            return false;
        }
    }
}