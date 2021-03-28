using System;
using UnityEngine;
using UnityEngine.UI;

namespace CrunchySDK.UI
{
    public class ResetSelfButton : MonoBehaviour
    {
        private Button _button;
    
        // Start is called before the first frame update
        void Start()
        {
            _button = GetComponent<Button>();
            _button.onClick.AddListener(() =>
            {
                _button.OnDeselect(null);
            });
        }

        // Update is called once per frame
        void Update()
        {
        
        }
    }
}
