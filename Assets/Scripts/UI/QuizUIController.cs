using System;
using Core;
using Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class QuizUIController : MonoBehaviour
    {
        private static Color SubmitEnabledColor = new Color(0.95f, 0.3f, 0.4f);
        private static Color SubmitDisabledColor = new Color(0.3f, 0.3f, 0.4f);
        
        [Header("Question")]
        [SerializeField] private TextMeshProUGUI questionText;
        
        [Header("Answer Buttons")]
        [SerializeField] private AnswerButton[] answerButtons;
        
        [Header("Submit Button")]
        [SerializeField] private Button submitButton;
        [SerializeField] private Image submitButtonBackground;

        [Header("Demo")]
        [Tooltip("Botão de voltar ao mapa, visível só na demo. No evento o jogador volta " +
                 "escaneando outro QR Code, então ele fica escondido.")]
        [SerializeField] private Button demoBackButton;

        private AnswerButton _selectedButton;
        private TextMeshProUGUI _submitLabel;

        private void Awake()
        {
            _submitLabel = submitButton != null
                ? submitButton.GetComponentInChildren<TextMeshProUGUI>(true)
                : null;
            if (demoBackButton != null)
                demoBackButton.onClick.AddListener(() => GameManager.Instance.ShowMap());
        }

        private void OnEnable()
        {
            GameManager.OnStationLoaded += Populate;

            if (demoBackButton != null)
                demoBackButton.gameObject.SetActive(GameManager.Mode == GameMode.Demo);
        }

        private void OnDisable()
        {
            GameManager.OnStationLoaded -= Populate;
        }

        private void Populate(StationData data)
        {
            var stations = GameManager.Instance != null
                           && GameManager.Instance.config != null
                           && GameManager.Instance.config.stations != null
                ? GameManager.Instance.config.stations
                : Array.Empty<StationData>();
            var total = 0;
            var number = 0;
            foreach (var station in stations)
            {
                if (station == null || station.isFinalStation)
                    continue;

                total++;
                if (station.stationId <= data.stationId)
                    number++;
            }

            if (number == 0)
                number = 1;
            if (total == 0)
                total = 1;

            questionText.text = $"<size=70%><color=#00D4E8>PERGUNTA {number} DE {total}</color></size>\n\n{data.questionText}";
            _selectedButton = null;
            SetSubmitEnabled(false);

            for (int i = 0; i < answerButtons.Length; i++)
            {
                var hasOption = i < data.answerOptions.Length;
                answerButtons[i].gameObject.SetActive(hasOption);

                if (hasOption)
                    answerButtons[i].Setup(data.answerOptions[i], i, this);
                
                answerButtons[i].SetSelected(false);
            }
        }

        public void OnAnswerButtonClicked(AnswerButton clicked)
        {
            if (_selectedButton != null)
                _selectedButton.SetSelected(false);
            
            _selectedButton = clicked;
            _selectedButton.SetSelected(true);
            
            GameManager.Instance.SelectAnswer(clicked.AnswerIndex);
            SetSubmitEnabled(true);
        }
        
        public void OnSubmitClicked()
        {
            GameManager.Instance.SubmitAnswer();
        }


        private void SetSubmitEnabled(bool enabled)
        {
            submitButton.interactable = enabled;
            submitButtonBackground.color = enabled ? SubmitEnabledColor : SubmitDisabledColor;
            if (_submitLabel != null)
                _submitLabel.text = enabled ? "Confirmar resposta" : "Escolha uma opção";
        }
    }
}
