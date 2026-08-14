using Core;
using TMPro;
using UnityEngine;

namespace UI
{
    public class LockedScreenController : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI messageText;

        private void OnEnable()
        {
            GameManager.OnStationLocked += UpdateMessage;
        }

        private void OnDisable()
        {
            GameManager.OnStationLocked -= UpdateMessage;
        }

        private void UpdateMessage(int missing)
        {
            messageText.text = missing == 1
                ? "Você ainda precisa coletar 1 dica para ganhar o brinde!"
                : $"Você ainda precisa coletar {missing} dicas para ganhar o brinde!";
        }
    }
}