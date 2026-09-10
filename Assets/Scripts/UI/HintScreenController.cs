using System;
using Core;
using Data;
using TMPro;
using UnityEngine;

namespace UI
{
    public class HintScreenController : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI hintText;
        [SerializeField] private TextMeshProUGUI progressText;
        [SerializeField] private PuzzleGridController puzzleGrid;

        private void OnEnable()
        {
            GameManager.OnHintRequested += Populate;
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