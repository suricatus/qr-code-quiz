using Core;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class FinalScreenController : MonoBehaviour
    {
        [Header("Demo")]
        [Tooltip("Botão \"Ver meu prêmio\", visível só na demo do site. No evento o prêmio " +
                 "está atrás do QR Code da premiação, então esta tela não precisa de botão. " +
                 "Criado pelo menu Suricatus > Demo > Preparar cena da demo.")]
        [SerializeField] private Button demoContinueButton;

        private void Awake()
        {
            if (demoContinueButton != null)
                demoContinueButton.onClick.AddListener(OnRestartClicked);
        }

        private void OnEnable()
        {
            if (demoContinueButton != null)
                demoContinueButton.gameObject.SetActive(GameManager.Mode == GameMode.Demo);
        }

        public void OnRestartClicked()
        {
            GameManager.Instance.ContinueFromFinalScreen();
        }
    }
}
