using System;
using System.Linq;
using Health;
using JetBrains.Annotations;
using MelonLoader;
using Photon.Pun;
using Photon.Realtime;
using Procedural.Scripts;
using UnityEngine;
using Player = Valve.VR.InteractionSystem.Player;

namespace CrunchySDK
{
    [Serializable]
    public class CrunchPlayer : MonoBehaviour
    {
        public int ActorId { get; internal set; }
        public string UserId { get; internal set; }
        public string UserName { get; internal set; }

        private Transform _transform;
        private HealthHandlerNetworked _healthHandler;
        public Vector3 Position
        {
            get => _transform.position;
            set => _transform.position = value;
        }
        public Vector3Int PositionInt
        {
            get => Vector3Int.RoundToInt(Position);
            set => _transform.position = value;
        }

        public Vector3 HeadPosition => GameObject
            .Find($"{name}/LocalPlayer/Armature/Root/Hips/Spine_01/Spine_02/Spine_03/Neck/Head").transform.position;

        public float MaxHealth
        {
            get => _healthHandler.hitpointsMax;
            set => _healthHandler.hitpointsMax = value;
        }

        public float Health
        {
            get => _healthHandler.hitpointsCurrent;
            set
            {
                var difference = Mathf.Clamp(value, 0, MaxHealth) - Health;
                if (difference >= 0)
                {
                    _healthHandler.Heal(difference);
                }
                else
                {
                    _healthHandler.ApplyDamage(Math.Abs(difference));
                }
            }
        }

        [CanBeNull] public WalkableArea CurrentRoom => CrunchySdk.Map.GetSlot(PositionInt)?.room; 

        public void OnEnable()
        {
            _transform = transform;
            _healthHandler = GetComponent<HealthHandlerNetworked>();
        }

        public void SendBroadcast(string text) => 
            CrunchySdk.instance.CreateTimedBroadcast(this, text, null, null);

        public void SendBroadcast(string text, Color color) =>
            CrunchySdk.instance.CreateTimedBroadcast(this, text, null, color);

        public void SendBroadcast(string text, Color color, TimeSpan timeSpan) =>
            CrunchySdk.instance.CreateTimedBroadcast(this, text, timeSpan, color);
        

        public override string ToString()
        {
            return $"Player {ActorId}\n" +
                   "(\n" +
                   $"GameObject: {gameObject.name}\n" +
                   $"UserId: {UserId}\n" +
                   $"UserName: {UserName}\n" +
                   $"Position: {Position}\n" +
                   $"HeadPosition: {HeadPosition}\n" +
                   $"Health: {Health}\n" +
                   ")";
        }

        public static CrunchPlayer[] AllPlayers => FindObjectsOfType<CrunchPlayer>();
        public static CrunchPlayer GetPlayer(int actorId) => AllPlayers.First(x => x.ActorId == actorId);
        public static CrunchPlayer GetPlayerByUserId(string userId) => AllPlayers.First(x => x.UserId == userId);
        public static CrunchPlayer GetSelf() => Player.instance.gameObject.GetComponent<CrunchPlayer>();
    }
}