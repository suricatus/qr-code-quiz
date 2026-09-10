using System.Text;
using System.Text.RegularExpressions;
using Core;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI
{
    public class PrizeScreenController : MonoBehaviour
    {
        private const int PhoneDigitCount = 11;
        private const string EmailPattern = @"^[^@\s]+@[^@\s]+\.(com|com\.br)$";

        [Header("Form Fields")]
        [SerializeField] private TMP_InputField nameField;
        [SerializeField] private TMP_InputField emailField;
        [SerializeField] private TMP_InputField phoneField;
        [SerializeField] private Toggle termsToggle;

        [Header("Submit")]
        [SerializeField] private Button submitButton;

        [Header("Confirmação")]
        [Tooltip("Painel exibido após o envio. A partir daí a tela fica bloqueada.")]
        [SerializeField] private GameObject thanksPanel;

        private bool _submitted;

        private void Awake()
        {
            nameField.onValueChanged.AddListener(_ => ValidateForm());
            emailField.onValueChanged.AddListener(_ => ValidateForm());
            phoneField.onValueChanged.AddListener(OnPhoneValueChanged);
            termsToggle.onValueChanged.AddListener(_ => ValidateForm());
            submitButton.onClick.AddListener(OnSubmitClicked);

            if (thanksPanel != null)
                thanksPanel.SetActive(false);

            ValidateForm();
        }

        private void OnPhoneValueChanged(string value)
        {
            var digits = ExtractDigits(value);
            var masked = ApplyPhoneMask(digits);
            
            phoneField.onValueChanged.RemoveListener(OnPhoneValueChanged);
            phoneField.text = masked;
            phoneField.caretPosition = masked.Length;
            phoneField.onValueChanged.AddListener(OnPhoneValueChanged);

            ValidateForm();
        }

        private static string ExtractDigits(string input)
        {
            var sb = new StringBuilder();
            foreach (var c in input)
                if (char.IsDigit(c))
                    sb.Append(c);
            return sb.ToString();
        }
        
        private static string ApplyPhoneMask(string digits)
        {
            if (digits.Length > PhoneDigitCount)
                digits = digits[..PhoneDigitCount];

            var sb = new StringBuilder();
            for (var i = 0; i < digits.Length; i++)
            {
                if (i == 0) sb.Append('(');
                sb.Append(digits[i]);
                if (i == 1) sb.Append(')');
                else if (i == 6) sb.Append('-');
            }

            return sb.ToString();
        }
        
        private static bool IsEmailValid(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return false;
            return Regex.IsMatch(email.Trim(), EmailPattern, RegexOptions.IgnoreCase);
        }

        private static bool IsPhoneValid(string phone)
        {
            return ExtractDigits(phone).Length == PhoneDigitCount;
        }

        private void ValidateForm()
        {
            // Depois do envio o botão fica desligado de vez: sem isso, qualquer
            // evento de mudança tardio poderia reabilitá-lo.
            if (_submitted)
                return;

            var isValid = !string.IsNullOrWhiteSpace(nameField.text)
                          && IsEmailValid(emailField.text)
                          && IsPhoneValid(phoneField.text)
                          && termsToggle.isOn;

            submitButton.interactable = isValid;
        }
        
        private void OnSubmitClicked()
        {
            // Um toque duplo no celular dispara o clique duas vezes.
            if (_submitted)
                return;

            _submitted = true;

            Debug.Log($"[PrizeScreen] Submitted — Name: {nameField.text} | Email: {emailField.text} | Phone: {phoneField.text}");
            // TODO: enviar dados para o backend

            LockScreen();

            if (thanksPanel != null)
                thanksPanel.SetActive(true);
        }

        /// <summary>
        /// Encerra a interação com o formulário. O painel de agradecimento não cobre a
        /// tela inteira, então travar só por cima dele deixaria os campos ao redor
        /// ainda clicáveis.
        /// </summary>
        private void LockScreen()
        {
            // Fecha o teclado do celular, que fica aberto se o envio veio de um campo.
            nameField.DeactivateInputField();
            emailField.DeactivateInputField();
            phoneField.DeactivateInputField();

            nameField.interactable = false;
            emailField.interactable = false;
            phoneField.interactable = false;
            termsToggle.interactable = false;
            submitButton.interactable = false;

            if (EventSystem.current != null)
                EventSystem.current.SetSelectedGameObject(null);
        }
    }
}
