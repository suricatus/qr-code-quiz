using Core;
using UnityEngine;

namespace UI
{
    public class FinalScreenController : MonoBehaviour
    {
        public void OnRestartClicked()
        {
            GameManager.Instance.Restart();
        }
    }
}