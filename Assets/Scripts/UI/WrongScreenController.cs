using Core;
using UnityEngine;

namespace UI
{
    public class WrongScreenController : MonoBehaviour
    {
        public void OnTryAgainClicked()
        {
            GameManager.Instance.LoadStationFromURL();
        }
    }
}