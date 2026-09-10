using System.Collections.Generic;
using System.Linq;
using Data;
using UnityEngine;

namespace Core
{
    public class ProgressManager : MonoBehaviour
    {
        private const string SaveKey = "completed_stations";
        private const string BuildKey = "progress_build_id";
        public static ProgressManager Instance { get; private set; }

        [Header("Configuration")]
        [SerializeField] private GameConfig config;

        [Header("Testes")]
        [Tooltip("Zera o progresso automaticamente quando o jogador abre uma build diferente da " +
                 "que ele jogou por último. Permite reusar os mesmos QR Codes a cada teste, sem " +
                 "?reset=1. DESLIGUE ANTES DO EVENTO: publicar uma build nova com isso ligado " +
                 "apaga o progresso de quem já estiver jogando.")]
        [SerializeField] private bool resetProgressOnNewBuild = true;

        private readonly HashSet<int> _completedStations = new();
        public int TotalStations => config.stations.Length;
        public int CompletedCount => _completedStations.Count;
        
        public int MissingForFinal =>
            config.stations.Count(s => !s.isFinalStation && !_completedStations.Contains(s.stationId));

        // A estação final não é uma dica: ela revela o prêmio. Contar as dicas por
        // TotalStations/CompletedCount inclui a final e dá um resultado a mais.
        public int TotalHints =>
            config.stations.Count(s => !s.isFinalStation);

        public int CollectedHints =>
            config.stations.Count(s => !s.isFinalStation && _completedStations.Contains(s.stationId));

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            LoadProgress();
        }
        
        public void MarkStationComplete(int stationId)
        {
            if (_completedStations.Add(stationId))
                SaveProgress();
        }
        
        public bool IsStationComplete(int stationId) => _completedStations.Contains(stationId);
        
        public bool CanAccessFinalStation()
        {
            return config.stations
                .Where(s => !s.isFinalStation)
                .All(s => _completedStations.Contains(s.stationId));
        }
        
        public void ResetProgress()
        {
            _completedStations.Clear();
            PlayerPrefs.DeleteKey(SaveKey);
            PlayerPrefs.Save();
        }

        private void SaveProgress()
        {
            PlayerPrefs.SetString(SaveKey, string.Join(",", _completedStations));
            PlayerPrefs.Save();
        }

        private void LoadProgress()
        {
            _completedStations.Clear();

            if (resetProgressOnNewBuild && IsDifferentBuildThanLastPlayed())
            {
                ResetProgress();
                return;
            }

            var raw = PlayerPrefs.GetString(SaveKey, "");
            if (string.IsNullOrWhiteSpace(raw)) return;

            foreach (var part in raw.Split(','))
                if (int.TryParse(part.Trim(), out var id))
                    _completedStations.Add(id);
        }

        /// <summary>
        /// Compara a build atual com a que gravou o progresso. O buildGUID muda a cada
        /// build, então o mesmo QR Code sempre abre limpo depois de um deploy novo.
        /// </summary>
        private static bool IsDifferentBuildThanLastPlayed()
        {
            var currentBuild = Application.buildGUID;

            // No Editor o buildGUID é vazio; aí não há build nova para detectar.
            if (string.IsNullOrEmpty(currentBuild))
                return false;

            if (PlayerPrefs.GetString(BuildKey, "") == currentBuild)
                return false;

            PlayerPrefs.SetString(BuildKey, currentBuild);
            PlayerPrefs.Save();
            return true;
        }
    }
}
