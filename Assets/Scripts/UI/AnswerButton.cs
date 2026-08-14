using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class AnswerButton : MonoBehaviour
    {
        private static readonly Color SelectedColor = new Color(0.22f, 0.78f, 0.32f);
        private static readonly Color DefaultColor = Color.white;

        [SerializeField] private Image background;
        [SerializeField] private TextMeshProUGUI label;
        
        public int AnswerIndex { get; private set; }
        
        private QuizUIController _controller;
        private Button _button;

        private void Awake()
        {
            _button = GetComponent<Button>();
            _button.onClick.AddListener(OnClicked);
        }

        public void Setup(string text, int index, QuizUIController quizUIController)
        {
            label.text = text;
            AnswerIndex = index;
            _controller = quizUIController;
        }

        public void SetSelected(bool selected)
        {
            background.color = selected ? SelectedColor : DefaultColor;
        }

        private void OnClicked()
        {
            _controller.OnAnswerButtonClicked(this);
        }
    }
}