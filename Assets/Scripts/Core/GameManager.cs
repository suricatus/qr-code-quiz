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
        private const string DemoUrlParameter = "demo";

        public static GameManager Instance { get; private set; }

        /// <summary>
        /// Resolvido uma única vez no Start, a partir da URL. Quem precisa mudar de
        /// comportamento entre o evento e a demo do site consulta isto.
        /// </summary>
        public static GameMode Mode { get; private set; } = GameMode.Event;

        [Header("Configuration")]
        public GameConfig config;

        [Header("Debug(Editor only)")]
        [Tooltip("Query string do QR para simular no Editor, ex.: \"?station=1\" ou \"?prize=1\". " +
                 "Quando preenchido, tem prioridade sobre os campos abaixo.")]
        [SerializeField] private string debugQueryString = "";
        [SerializeField] private int debugStationId = 1;
        [SerializeField] private bool debugPrizeScreen = false;
        [Tooltip("Abre no modo demo (tela de mapa, sem QR), que é como o jogo roda no site.")]
        [SerializeField] private bool debugDemoMode = false;

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

            Mode = ResolveMode();

            if (Mode == GameMode.Demo)
            {
                // A demo é vitrine: cada visitante começa limpo, e o que ele joga não
                // encosta no progresso salvo de quem está no evento com o mesmo build.
                ProgressManager.Instance.BeginVolatileSession();
                ShowMap();
                return;
            }

            if (IsPrizeScreen())
            {
                UIController.Instance.ShowScreen(GameScreen.Prize);
                return;
            }
            LoadStationFromURL();
        }

        /// <summary>
        /// Sem "?station=" não existe estação para carregar: até aqui a tela ficava
        /// vazia, então a ausência de parâmetro passa a significar "abriu a demo".
        /// </summary>
        private GameMode ResolveMode()
        {
            if (!string.IsNullOrEmpty(URLParameterReader.GetParameter(DemoUrlParameter)))
                return GameMode.Demo;

#if UNITY_EDITOR
            // No Editor os campos de debug fazem o papel do QR, então só o toggle decide.
            return debugDemoMode ? GameMode.Demo : GameMode.Event;
#else
            var hasStation = int.TryParse(URLParameterReader.GetParameter(StationUrlParameter), out _);
            var hasPrize = !string.IsNullOrEmpty(URLParameterReader.GetParameter(PrizeUrlParameter));

            return hasStation || hasPrize ? GameMode.Event : GameMode.Demo;
#endif
        }

        public void LoadStationFromURL()
        {
            var stationId = GetStationIdFromURL();

            if (stationId < 0)
                return;

            LoadStation(stationId);
        }

        /// <summary>
        /// Abre a estação pelo id, venha ele do QR (evento) ou do mapa (demo).
        /// </summary>
        public void LoadStation(int stationId)
        {
            SelectedAnswerIndex = -1;
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

        /// <summary>
        /// Repete a estação atual sem depender da URL — é o "tentar de novo" da tela
        /// de erro, que precisa funcionar igual no evento e na demo.
        /// </summary>
        public void ReloadCurrentStation()
        {
            if (CurrentStation != null)
            {
                LoadStation(CurrentStation.stationId);
                return;
            }

            LoadStationFromURL();
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

        /// <summary>
        /// Reabre a dica de uma estação já concluída, sem refazer a pergunta. É o que
        /// o mapa da demo usa quando o visitante toca numa dica que já coletou.
        /// </summary>
        public void ShowStationHint(int stationId)
        {
            var station = config.GetStation(stationId);

            if (station == null)
                return;

            CurrentStation = station;
            SelectedAnswerIndex = -1;
            ShowHint();
        }

        /// <summary>
        /// Mapa do estande: a tela inicial da demo, que faz o papel do QR Code.
        /// </summary>
        public void ShowMap()
        {
            SelectedAnswerIndex = -1;
            CurrentStation = null;
            UIController.Instance.ShowScreen(GameScreen.Map);
        }

        public void Restart()
        {
            SelectedAnswerIndex = -1;
            CurrentStation = null;

            if (Mode == GameMode.Demo)
            {
                ShowMap();
                return;
            }

#if UNITY_WEBGL && !UNITY_EDITOR
            // Recarrega mantendo o ?station= do QR. Ir para a URL sem parâmetro deixaria
            // a tela vazia, porque não existe uma tela de "escaneie um QR Code".
            URLParameterReader.Reload();
#else
            LoadStationFromURL();
#endif
        }

        /// <summary>
        /// Zera a sessão da demo e volta ao mapa, para o próximo visitante começar limpo.
        /// </summary>
        public void RestartDemo()
        {
            ProgressManager.Instance.ResetProgress();
            ShowMap();
        }

        /// <summary>
        /// Botão da tela final. No evento ele devolve o jogador ao QR, porque o prêmio
        /// está atrás do QR da premiação; na demo não há QR, então segue direto.
        /// </summary>
        public void ContinueFromFinalScreen()
        {
            if (Mode == GameMode.Demo)
            {
                UIController.Instance.ShowScreen(GameScreen.Prize);
                return;
            }

            Restart();
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
