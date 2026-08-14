using System;
using Core;
using Data;
using TMPro;
using UnityEngine;

namespace UI
{
    public class CorrectScreenController : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI curiosityText;

        private void OnEnable()
        {
            GameManager.OnAnswerCorrect += Populate;
        }

        private void OnDisable()
        {
            GameManager.OnAnswerCorrect -= Populate;
        }

        private void Populate(StationData data)
        {
            curiosityText.text = data.curiosityText;
        }

        public void OnShowHintClicked()
        {
            GameManager.Instance.ShowHint();
        }
    }
}