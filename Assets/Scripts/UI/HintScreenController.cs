using System;
using Core;
using Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class HintScreenController : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI hintText;
        [SerializeField] private TextMeshProUGUI progressText;
        [SerializeField] private PuzzleGridController puzzleGrid;

        [Header("Demo")]
        [Tooltip("Botão \"Voltar ao mapa\", visível só na demo do site. No evento o jogador " +
                 "sai desta tela escaneando o QR da próxima estação, sem botão. " +
                 "Criado pelo menu Suricatus > Demo > Preparar cena da demo.")]
        [SerializeField] private Button demoContinueButton;

        private void Awake()
        {
            if (demoContinueButton != null)
                demoContinueButton.onClick.AddListener(OnRestartClicked);
        }

        private void OnEnable()
        {
            GameManager.OnHintRequested += Populate;

            if (demoContinueButton != null)
                demoContinueButton.gameObject.SetActive(GameManager.Mode == GameMode.Demo);
        }

        private void OnDisable()
        {
            GameManager.OnHintRequested -= Populate;
        }

        private void Populate(StationData data)
        {
            hintText.text = data.hintText;

            var collected = ProgressManager.Instance.CollectedHints;
            var total = ProgressManager.Instance.TotalHints;
            var remaining = total - collected;

            progressText.text = remaining switch
            {
                > 1 => $"Faltam {remaining} de {total} dicas para o prêmio",
                1 => $"Falta 1 de {total} dicas para o prêmio",
                _ => "Você coletou todas as dicas!"
            };

            puzzleGrid.Refresh();
        }

        public void OnRestartClicked()
        {
            GameManager.Instance.Restart();
        }
    }
}
