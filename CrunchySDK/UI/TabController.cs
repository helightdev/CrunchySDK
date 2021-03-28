using System.Linq;
using MelonLoader;
using UnityEngine;
using UnityEngine.UI;

namespace CrunchySDK.UI
{
    public class TabController : MonoBehaviour
    {
        public static TabController Intance { get; internal set; }

        public GameObject[] tabs;
        public GameObject[] buttons;

        public int Selected
        {
            get => _value;
            set
            {
                _value = value;
                SetTab();
            }
        }
    
        private Button[] _buttonComponents;
        private int _value = 0;

        public TabController()
        {
            Intance = this;
        }

        // Start is called before the first frame update
        public void Start()
        {
            _buttonComponents = buttons.Select(x => x.GetComponent<Button>()).ToArray();
            SetTab();
            for (var i = 0; i < buttons.Length; i++)
            {
                var num = i;
                var button = buttons[num].GetComponent<Button>();
                button.onClick.AddListener(() => Selected = num);
            }
        }

        public void SetTab()
        {
            for (var i = 0; i < tabs.Length; i++)
            {
                tabs[i].SetActive(i == _value);
                if (i == _value)
                {
                    MelonLogger.Log($"{i} select!");
                    _buttonComponents[i].OnSelect(null);
                }
                else
                {
                    MelonLogger.Log($"{i} deselect :(");
                    _buttonComponents[i].OnDeselect(null);
                }
            }
        }
    }
}
