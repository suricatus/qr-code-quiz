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
        Locked
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
            quizScreen.SetActive(screen == GameScreen.Quiz);
            correctScreen.SetActive(screen == GameScreen.Correct);
            hintScreen.SetActive(screen == GameScreen.Hint);
            wrongScreen.SetActive(screen == GameScreen.Wrong);
            finalScreen.SetActive(screen == GameScreen.Final);
            prizeScreen.SetActive(screen == GameScreen.Prize);
            lockedScreen.SetActive(screen == GameScreen.Locked);
        }
    }
}