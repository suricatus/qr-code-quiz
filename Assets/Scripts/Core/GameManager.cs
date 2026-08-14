using System;
using Data;
using UI;
using UnityEngine;

namespace Core
{
    public class GameManager : MonoBehaviour
    {
        private const string StationUrlParameter = "station";
        private const string PrizeUrlParameter = "prize";
        
        public static GameManager Instance { get; private set; }
        
        [Header("Configuration")]
        public GameConfig config;

        [Header("Debug(Editor only)")]
        [SerializeField] private int debugStationId = 1;
        [SerializeField] private bool debugPrizeScreen = false;
        
        public StationData CurrentStation { get; private set; }
        public int SelectedAnswerIndex { get; private set; } = -1;
        
        public static event Action<StationData> OnStationLoaded;
        public static event Action<StationData> OnAnswerCorrect;
        public static event Action OnAnswerWrong;
        public static event Action<StationData> OnHintRequested;
        public static event Action<int> OnStationLocked;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void Start()
        {
            if (IsPrizeScreen())
            {
                UIController.Instance.ShowScreen(GameScreen.Prize);
                return;
            }
            LoadStationFromURL();
        }
        
        public void LoadStationFromURL()
        {
            SelectedAnswerIndex = -1;
            var stationId = GetStationIdFromURL();

            if (stationId < 0)
                return;

            CurrentStation = config.GetStation(stationId);

            if (CurrentStation == null)
            {
                Debug.LogWarning($"[GameManager] Station {stationId} not found.");
                return;
            }
            
            if (CurrentStation.isFinalStation && !ProgressManager.Instance.CanAccessFinalStation())
            {
                UIController.Instance.ShowScreen(GameScreen.Locked);
                OnStationLocked?.Invoke(ProgressManager.Instance.MissingForFinal);
                return;
            }

            UIController.Instance.ShowScreen(GameScreen.Quiz);
            OnStationLoaded?.Invoke(CurrentStation);
        }


        public void SelectAnswer(int index)
        {
            SelectedAnswerIndex = index;
        }

        public void SubmitAnswer()
        {
            if (SelectedAnswerIndex < 0)
                return;
            
            var isCorrect = SelectedAnswerIndex == CurrentStation.correctAnswerIndex;

            if (isCorrect)
            {
                ProgressManager.Instance.MarkStationComplete(CurrentStation.stationId);
                UIController.Instance.ShowScreen(GameScreen.Correct);
                OnAnswerCorrect?.Invoke(CurrentStation);
            }
            else
            {
                UIController.Instance.ShowScreen(GameScreen.Wrong);
                OnAnswerWrong?.Invoke();
            }
        }

        public void ShowHint()
        {
            var target = CurrentStation.isFinalStation ? GameScreen.Final : GameScreen.Hint;
            UIController.Instance.ShowScreen(target);
            OnHintRequested?.Invoke(CurrentStation);
        }

        public void Restart()
        {
            SelectedAnswerIndex = -1;
            CurrentStation = null;

#if UNITY_WEBGL && !UNITY_EDITOR
            Application.ExternalEval("window.location.href = window.location.pathname;");
#else
            LoadStationFromURL();
#endif
        }

        private int GetStationIdFromURL()
        {
#if UNITY_EDITOR
            return debugStationId;
#else
            var raw = URLParameterReader.GetParameter(StationUrlParameter);
            return int.TryParse(raw, out int id) ? id : -1;
#endif
        }
        
        private bool IsPrizeScreen()
        {
#if UNITY_EDITOR
            return debugPrizeScreen;
#else
    var raw = URLParameterReader.GetParameter(PrizeUrlParameter);
    return !string.IsNullOrEmpty(raw);
#endif
        }
    }
    

}