using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class AnswerButton : MonoBehaviour
    {
        private static readonly Color SelectedColor = new Color(0.00f, 0.72f, 0.82f);
        private static readonly Color DefaultColor = Color.white;
        private static readonly Color HoverColor = new Color(0.86f, 0.97f, 1f);
        private static readonly Color PressedColor = new Color(0.70f, 0.91f, 0.96f);

        [SerializeField] private Image background;
        [SerializeField] private TextMeshProUGUI label;
        
        public int AnswerIndex { get; private set; }
        
        private QuizUIController _controller;
        private Button _button;
        private string _optionText;
        private string _letter;

        private void Awake()
        {
            _button = GetComponent<Button>();
            _button.onClick.AddListener(OnClicked);
            _button.transition = Selectable.Transition.ColorTint;
        }

        public void Setup(string text, int index, QuizUIController quizUIController)
        {
            _optionText = text;
            _letter = $"{(char)('A' + index)}";
            AnswerIndex = index;
            _controller = quizUIController;
            SetSelected(false);
        }

        public void SetSelected(bool selected)
        {
            var colors = _button.colors;
            colors.normalColor = selected ? SelectedColor : DefaultColor;
            colors.highlightedColor = selected ? SelectedColor : HoverColor;
            colors.pressedColor = PressedColor;
            colors.selectedColor = selected ? SelectedColor : HoverColor;
            colors.fadeDuration = 0.08f;
            _button.colors = colors;
            background.color = Color.white;
            label.text = selected
                ? $"<color=#FFFFFF>✓</color>  {_letter}. {_optionText}"
                : $"{_letter}. {_optionText}";
        }

        private void OnClicked()
        {
            if (_controller == null)
                return;

            _controller.OnAnswerButtonClicked(this);
        }
    }
}
