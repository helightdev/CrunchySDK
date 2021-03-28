using System;
using UnityEngine;

namespace CrunchySDK.UI
{
    public class CrunchyUiReactivator : MonoBehaviour
    {
        private void OnDestroy()
        {
            CrunchySdk.instance.currentUi.Create();
        }
    }
}