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

            var completed = ProgressManager.Instance.CompletedCount;
            var total = ProgressManager.Instance.TotalStations;
            var remaining = total - completed;

            progressText.text = remaining > 0
                ? $"Faltam {remaining} de {total} dicas para o prêmio"
                : "Você coletou todas as dicas!";
            
            puzzleGrid.Refresh();
        }

        public void OnRestartClicked()
        {
            GameManager.Instance.Restart();
        }
    }
}