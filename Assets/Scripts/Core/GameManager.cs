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
        private const string ResetUrlParameter = "reset";
        
        public static GameManager Instance { get; private set; }
        
        [Header("Configuration")]
        public GameConfig config;

        [Header("Debug(Editor only)")]
        [Tooltip("Query string do QR para simular no Editor, ex.: \"?station=1\" ou \"?prize=1\". " +
                 "Quando preenchido, tem prioridade sobre os campos abaixo.")]
        [SerializeField] private string debugQueryString = "";
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

#if UNITY_EDITOR
            URLParameterReader.EditorQueryStringOverride = debugQueryString;
#endif
        }

        private void Start()
        {
            // "?reset=1" limpa o progresso salvo no navegador antes de carregar a tela.
            // Serve para testar do zero sem precisar apagar os dados do site no celular.
            if (!string.IsNullOrEmpty(URLParameterReader.GetParameter(ResetUrlParameter)))
            {
                ProgressManager.Instance.ResetProgress();
                Debug.Log("[GameManager] Progresso zerado via parâmetro de URL.");
            }

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
            // Recarrega mantendo o ?station= do QR. Ir para a URL sem parâmetro deixaria
            // a tela vazia, porque não existe uma tela de "escaneie um QR Code".
            URLParameterReader.Reload();
#else
            LoadStationFromURL();
#endif
        }

        private int GetStationIdFromURL()
        {
            var raw = URLParameterReader.GetParameter(StationUrlParameter);
            if (int.TryParse(raw, out var id))
                return id;

#if UNITY_EDITOR
            return debugStationId;
#else
            Debug.LogWarning($"[GameManager] Parâmetro '{StationUrlParameter}' ausente ou inválido na URL " +
                             $"'{Application.absoluteURL}'. O QR Code precisa apontar para a URL do jogo com ?{StationUrlParameter}=N.");
            return -1;
#endif
        }

        private bool IsPrizeScreen()
        {
            var raw = URLParameterReader.GetParameter(PrizeUrlParameter);
            if (!string.IsNullOrEmpty(raw))
                return true;

#if UNITY_EDITOR
            return string.IsNullOrEmpty(debugQueryString) && debugPrizeScreen;
#else
            return false;
#endif
        }
    }
    

}