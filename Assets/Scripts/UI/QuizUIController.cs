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

        private AnswerButton _selectedButton;

        private void OnEnable()
        {
            GameManager.OnStationLoaded += Populate;
        }

        private void OnDisable()
        {
            GameManager.OnStationLoaded -= Populate;
        }

        private void Populate(StationData data)
        {
            questionText.text = data.questionText;
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
        }
    }
}