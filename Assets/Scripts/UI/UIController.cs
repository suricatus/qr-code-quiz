using System;
using UnityEngine;

namespace UI
{
    public enum GameScreen
    {
        Quiz,
        Correct,
        Hint,
        Wrong,
        Final,
        Prize,
        Locked,

        /// <summary>
        /// Mapa do estande. Só existe na demo do site, onde não há QR Code para escanear.
        /// </summary>
        Map
    }

    public class UIController : MonoBehaviour
    {
        public static UIController Instance {get; private set;}

        [Header("Screens")]
        [SerializeField] private GameObject quizScreen;
        [SerializeField] private GameObject correctScreen;
        [SerializeField] private GameObject hintScreen;
        [SerializeField] private GameObject wrongScreen;
        [SerializeField] private GameObject finalScreen;
        [SerializeField] private GameObject prizeScreen;
        [SerializeField] private GameObject lockedScreen;

        [Header("Demo")]
        [Tooltip("Tela de mapa usada na demo do site. Montada pelo menu " +
                 "Suricatus > Demo > Preparar cena da demo.")]
        [SerializeField] private GameObject mapScreen;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this.gameObject);
                return;
            }

            Instance = this;
        }

        public void ShowScreen(GameScreen screen)
        {
            Toggle(quizScreen, screen == GameScreen.Quiz);
            Toggle(correctScreen, screen == GameScreen.Correct);
            Toggle(hintScreen, screen == GameScreen.Hint);
            Toggle(wrongScreen, screen == GameScreen.Wrong);
            Toggle(finalScreen, screen == GameScreen.Final);
            Toggle(prizeScreen, screen == GameScreen.Prize);
            Toggle(lockedScreen, screen == GameScreen.Locked);
            Toggle(mapScreen, screen == GameScreen.Map);

            if (screen == GameScreen.Map && mapScreen == null)
                Debug.LogError("[UIController] A tela de mapa não está atribuída. Rode " +
                               "Suricatus > Demo > Preparar cena da demo.");
        }

        /// <summary>
        /// A tela de mapa é opcional: um build antigo da cena não a tem, e sem o teste
        /// de nulo o jogo do evento quebraria em todas as trocas de tela.
        /// </summary>
        private static void Toggle(GameObject screen, bool isActive)
        {
            if (screen != null)
                screen.SetActive(isActive);
        }
    }
}
