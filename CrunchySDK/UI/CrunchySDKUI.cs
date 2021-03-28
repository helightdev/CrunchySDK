using CrunchySDK.Zombies;
using Interaction.Hands.Hand.SteamVR.Overrides;
using MelonLoader;
using UI;
using UnityEngine;
using UnityEngine.UI;
using Valve.VR.InteractionSystem;

namespace CrunchySDK.UI
{
    public class CrunchySDKUI
    {
        public GameObject UI { get; internal set; }

        private bool _createLock = false;
        
        public void Create()
        {
            if (_createLock) return;
            if (UI != null) if (!UI.activeSelf || !UI.activeInHierarchy) Object.Destroy(UI);
            _createLock = true;
            var playerTransform = Player.instance.leftHand.transform;
            UI = Object.Instantiate(CrunchySdk.instance.uiPrefab, playerTransform.TransformPoint(0.0f, 0.0f, 0.5f),
                playerTransform.rotation);
            UI.AddComponent<UIPointerAttach>();
            var tabs = GameObject.Find($"{UI.name}/Root/Tabs");
            var tabGamemodes = GameObject.Find($"{UI.name}/Root/Tabs/Gamemodes");
            var tabActions = GameObject.Find($"{UI.name}/Root/Tabs/Actions");
            var tabSettings = GameObject.Find($"{UI.name}/Root/Tabs/Settings");
            var tabAbout = GameObject.Find($"{UI.name}/Root/Tabs/About");
            var tabGamemodesButton = GameObject.Find($"{UI.name}/Root/Navigation/Gamemodes");
            var tabActionsButton = GameObject.Find($"{UI.name}/Root/Navigation/Actions");
            var tabSettingsButton = GameObject.Find($"{UI.name}/Root/Navigation/Settings");
            var tabAboutButton = GameObject.Find($"{UI.name}/Root/Navigation/About");
            var tabController = tabs.AddComponent<TabController>();
            tabController.tabs = new [] {tabGamemodes, tabActions, tabSettings, tabAbout};
            tabController.buttons = new[] {tabGamemodesButton, tabActionsButton, tabSettingsButton, tabAboutButton};

            var showoff = GameObject.Find($"{UI.name}/Root/Tabs/Gamemodes/Showoff Button");
            showoff.AddComponent<ResetSelfButton>();
            showoff.GetComponent<Button>().onClick.AddListener(
                () => { MelonLogger.Log("Showoff"); });
            var zombies = GameObject.Find($"{UI.name}/Root/Tabs/Gamemodes/Zombies Button");
            zombies.AddComponent<ResetSelfButton>();
            zombies.GetComponent<Button>().onClick.AddListener(
                () =>
                {
                    ZombiesModule module = new ZombiesModule();
                    module.RegisterListener();
                    CrunchySdk.instance.QuickJoin();
                });
            var heal = GameObject.Find($"{UI.name}/Root/Tabs/Actions/Heal Button");
            heal.AddComponent<ResetSelfButton>();
            heal.GetComponent<Button>().onClick
                .AddListener(
                    () =>
                    {
                        var player = CrunchPlayer.GetSelf();
                        player.Health = player.MaxHealth;
                        player.SendBroadcast("Your health has been restored");
                    });
            var quickplay = GameObject.Find($"{UI.name}/Root/Tabs/Actions/Quickplay Button");
            quickplay.AddComponent<ResetSelfButton>();
            quickplay.GetComponent<Button>().onClick
                .AddListener(() => CrunchySdk.instance.QuickJoin());
            var euler = UI.transform.rotation.eulerAngles;
            euler.x += 45;
            UI.transform.rotation = Quaternion.Euler(euler);
            UI.transform.SetParent(playerTransform);
            UI.SetActive(false);
            _createLock = false;
        }

        public void Show()
        {
            if (UI == null) Create();
            UI.SetActive(true);
        }

        public void Hide()
        {
            UI.SetActive(false);
        }
    }
}