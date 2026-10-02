using Core;
using TMPro;
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
        private TextMeshProUGUI _completionText;
        private string _eventCompletionText;

        private void Awake()
        {
            foreach (var text in GetComponentsInChildren<TextMeshProUGUI>(true))
            {
                if (!text.text.StartsWith("Parabéns!"))
                    continue;

                _completionText = text;
                _eventCompletionText = text.text;
                break;
            }

            if (demoContinueButton != null)
                demoContinueButton.onClick.AddListener(OnRestartClicked);
        }

        private void OnEnable()
        {
            var isDemo = GameManager.Mode == GameMode.Demo;
            if (demoContinueButton != null)
                demoContinueButton.gameObject.SetActive(isDemo);

            if (_completionText != null)
                _completionText.text = isDemo
                    ? "Você reuniu todas as dicas e desbloqueou o prêmio! Continue para ver sua recompensa."
                    : _eventCompletionText;
        }

        public void OnRestartClicked()
        {
            GameManager.Instance.ContinueFromFinalScreen();
        }
    }
}
