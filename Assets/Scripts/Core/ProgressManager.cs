using System.Collections.Generic;
using System.Linq;
using Data;
using UnityEngine;

namespace Core
{
    public class ProgressManager : MonoBehaviour
    {
        private const string SaveKey = "completed_stations";
        public static ProgressManager Instance { get; private set; }

        [Header("Configuration")]
        [SerializeField] private GameConfig config;

        private readonly HashSet<int> _completedStations = new();
        public int TotalStations => config.stations.Length;
        public int CompletedCount => _completedStations.Count;
        
        public int MissingForFinal =>
            config.stations.Count(s => !s.isFinalStation && !_completedStations.Contains(s.stationId));

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
            var raw = PlayerPrefs.GetString(SaveKey, "");
            if (string.IsNullOrWhiteSpace(raw)) return;

            foreach (var part in raw.Split(','))
                if (int.TryParse(part.Trim(), out var id))
                    _completedStations.Add(id);
        }
    }
}
