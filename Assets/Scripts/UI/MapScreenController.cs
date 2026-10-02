using System.Collections.Generic;
using System.Linq;
using Core;
using Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    /// <summary>
    /// Tela inicial da demo do site: lista as estações do estande e faz o papel do QR
    /// Code, que não existe quando o jogo roda dentro de uma página.
    ///
    /// Os cartões são montados em tempo de execução, pelo mesmo motivo do
    /// <see cref="PuzzleGridController"/>: a quantidade vem do GameConfig, e campos
    /// serializados por cartão já se perderam em regravações da cena.
    /// </summary>
    public class MapScreenController : MonoBehaviour
    {
        private const float MinCardHeight = 110f;
        private const float MaxCardHeight = 220f;
        private const float CardSpacing = 20f;
        private const float CardPadding = 14f;
        private const float TitleFontSize = 46f;
        private const float StatusFontSize = 32f;

        private static readonly Color CardAvailableColor = new(0.16f, 0.18f, 0.30f, 0.95f);
        private static readonly Color CardCompletedColor = new(0.14f, 0.28f, 0.26f, 0.95f);
        private static readonly Color CardLockedColor = new(0.13f, 0.17f, 0.25f, 0.97f);
        private static readonly Color TitleColor = Color.white;
        private static readonly Color StatusAvailableColor = new(0.95f, 0.3f, 0.4f);
        private static readonly Color StatusCompletedColor = new(0.45f, 0.85f, 0.6f);
        private static readonly Color StatusLockedColor = new(0.48f, 0.88f, 0.98f);

        [Header("References")]
        [SerializeField] private GameConfig config;
        [SerializeField] private RectTransform cardContainer;
        [SerializeField] private TextMeshProUGUI progressText;
        [SerializeField] private Button restartButton;

        [Header("Style")]
        [Tooltip("Fonte dos cartões. Copiada de um texto da cena pelo builder da demo.")]
        [SerializeField] private TMP_FontAsset font;
        [Tooltip("Sprite de fundo do cartão. Vazio desenha um retângulo liso.")]
        [SerializeField] private Sprite cardSprite;
        [Tooltip("Imagem usada quando a estação não tem silhueta própria configurada.")]
        [SerializeField] private Sprite fallbackBadge;

        private readonly List<GameObject> _cards = new();
        private float _cardHeight = MinCardHeight;
        private float _badgeSize = MinCardHeight - CardPadding * 2f;

        private void Awake()
        {
            ApplyLayout();
            EnsureLayout();

            if (restartButton != null)
                restartButton.onClick.AddListener(OnRestartClicked);
        }

        private void OnEnable()
        {
            ApplyLayout();
            Refresh();
        }

        private void ApplyLayout()
        {
            if (cardContainer != null)
            {
                var offsets = cardContainer.offsetMin;
                cardContainer.offsetMin = new Vector2(offsets.x, 320f);
            }

            if (restartButton != null)
            {
                var buttonRect = restartButton.GetComponent<RectTransform>();
                buttonRect.anchoredPosition = new Vector2(buttonRect.anchoredPosition.x, 170f);
            }

            var logo = transform.Find("Logo - Empresa");
            if (logo != null && logo.TryGetComponent<RectTransform>(out var logoRect))
                logoRect.sizeDelta = new Vector2(280f, 280f);
        }

        public void OnRestartClicked()
        {
            GameManager.Instance.RestartDemo();
        }

        public void Refresh()
        {
            // O mapa pode ser ativado antes do ProgressManager existir se a tela ficar
            // ligada na cena por engano. Sem isto a demo quebraria no primeiro frame.
            if (ProgressManager.Instance == null || config == null || cardContainer == null)
                return;

            foreach (var card in _cards)
                if (card != null)
                    Destroy(card);

            _cards.Clear();
            EnsureLayout();

            var stations = config.stations
                .Where(s => s != null)
                .OrderBy(s => s.isFinalStation ? 1 : 0)
                .ThenBy(s => s.stationId)
                .ToArray();

            ResizeCardsToFit(stations.Length);

            var hintNumber = 0;

            foreach (var station in stations)
            {
                var title = station.isFinalStation ? "Prêmio" : $"Dica {++hintNumber}";
                CreateCard(station, title);
            }

            UpdateProgressText();
        }

        /// <summary>
        /// Cartões do maior tamanho que couber no container, para que mudar o número de
        /// estações no GameConfig não estoure a tela nem deixe sobra no rodapé.
        /// </summary>
        private void ResizeCardsToFit(int cardCount)
        {
            if (cardCount <= 0)
                return;

            var available = cardContainer.rect.size.y - CardSpacing * (cardCount - 1);
            _cardHeight = Mathf.Clamp(available / cardCount, MinCardHeight, MaxCardHeight);
            _badgeSize = Mathf.Max(_cardHeight - CardPadding * 2f, 24f);
        }

        private void UpdateProgressText()
        {
            if (progressText == null)
                return;

            var collected = ProgressManager.Instance.CollectedHints;
            var total = ProgressManager.Instance.TotalHints;

            progressText.text = collected == total
                ? $"{collected} de {total} dicas coletadas — o prêmio está liberado!"
                : $"{collected} de {total} dicas coletadas";
        }

        private void CreateCard(StationData station, string title)
        {
            var isCompleted = ProgressManager.Instance.IsStationComplete(station.stationId);
            var isLocked = station.isFinalStation && !ProgressManager.Instance.CanAccessFinalStation();

            var cardGO = new GameObject($"Card_{station.stationId}", typeof(Image), typeof(Button),
                typeof(HorizontalLayoutGroup), typeof(LayoutElement));
            cardGO.transform.SetParent(cardContainer, false);
            _cards.Add(cardGO);

            var background = cardGO.GetComponent<Image>();
            background.sprite = cardSprite;
            background.type = cardSprite != null && cardSprite.border != Vector4.zero
                ? Image.Type.Sliced
                : Image.Type.Simple;
            background.color = isLocked
                ? CardLockedColor
                : isCompleted ? CardCompletedColor : CardAvailableColor;

            var layoutElement = cardGO.GetComponent<LayoutElement>();
            layoutElement.preferredHeight = _cardHeight;
            layoutElement.minHeight = _cardHeight;

            var cardLayout = cardGO.GetComponent<HorizontalLayoutGroup>();
            cardLayout.padding = new RectOffset((int)CardPadding, (int)CardPadding,
                (int)CardPadding, (int)CardPadding);
            cardLayout.spacing = CardPadding;
            cardLayout.childAlignment = TextAnchor.MiddleLeft;
            cardLayout.childControlWidth = true;
            cardLayout.childControlHeight = true;
            cardLayout.childForceExpandWidth = false;
            cardLayout.childForceExpandHeight = true;

            CreateBadge(cardGO.transform, station, isCompleted, isLocked);
            CreateTexts(cardGO.transform, title, StatusFor(station, isCompleted, isLocked),
                isLocked ? StatusLockedColor : isCompleted ? StatusCompletedColor : StatusAvailableColor);

            var button = cardGO.GetComponent<Button>();
            button.targetGraphic = background;
            // O tint padrão de Button reduz muito a opacidade do prêmio bloqueado e
            // apaga o cartão sobre o fundo ilustrado. O estado já está indicado pelo
            // baú fechado e pelo texto, então preservamos a cor do cartão.
            if (isLocked)
                button.transition = Selectable.Transition.None;
            button.interactable = !isLocked;

            if (isLocked)
                return;

            var stationId = station.stationId;
            var reviewHint = isCompleted;

            button.onClick.AddListener(() =>
            {
                // Estação concluída não refaz a pergunta: reabre a dica que ela deu, para
                // o visitante reler o que já coletou.
                if (reviewHint)
                    GameManager.Instance.ShowStationHint(stationId);
                else
                    GameManager.Instance.LoadStation(stationId);
            });
        }

        private static string StatusFor(StationData station, bool isCompleted, bool isLocked)
        {
            if (isLocked)
            {
                var missing = ProgressManager.Instance.MissingForFinal;
                return missing == 1
                    ? "Falta 1 dica para liberar"
                    : $"Faltam {missing} dicas para liberar";
            }

            if (station.isFinalStation)
                return isCompleted ? "Toque para rever o prêmio" : "Toque para escanear e ver o prêmio";

            return isCompleted ? "Dica coletada — toque para rever" : "Toque para escanear o QR Code";
        }

        private void CreateBadge(Transform parent, StationData station, bool isCompleted,
            bool isLocked)
        {
            var badgeGO = new GameObject("Badge", typeof(RectTransform), typeof(LayoutElement));
            badgeGO.transform.SetParent(parent, false);

            var frameGO = new GameObject("Frame", typeof(Image));
            frameGO.transform.SetParent(badgeGO.transform, false);
            StretchToParent(frameGO.GetComponent<RectTransform>());

            var frame = frameGO.GetComponent<Image>();
            frame.sprite = LoadArtwork("BadgeFrame");
            frame.preserveAspect = true;
            frame.raycastTarget = false;

            var iconGO = new GameObject("Icon", typeof(Image));
            iconGO.transform.SetParent(badgeGO.transform, false);
            var iconRect = iconGO.GetComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0.14f, 0.14f);
            iconRect.anchorMax = new Vector2(0.86f, 0.86f);
            iconRect.offsetMin = Vector2.zero;
            iconRect.offsetMax = Vector2.zero;

            var image = iconGO.GetComponent<Image>();
            image.sprite = BadgeFor(station, isCompleted, isLocked);
            image.preserveAspect = true;
            image.enabled = image.sprite != null;
            image.raycastTarget = false;

            var layoutElement = badgeGO.GetComponent<LayoutElement>();
            layoutElement.preferredWidth = _badgeSize;
            layoutElement.preferredHeight = _badgeSize;
            layoutElement.minWidth = _badgeSize;
        }

        private Sprite BadgeFor(StationData station, bool isCompleted, bool isLocked)
        {
            if (isCompleted && station.puzzlePiece != null)
                return station.puzzlePiece;

            if (station.isFinalStation)
            {
                var prize = LoadArtwork(isLocked ? "PrizeLocked" : "PrizeReady");
                if (prize != null)
                    return prize;
            }
            else
            {
                var stationQr = LoadArtwork($"QRStation_{station.stationId}");
                if (stationQr != null)
                    return stationQr;
            }

            return station.placeholderPiece != null ? station.placeholderPiece : fallbackBadge;
        }

        private static Sprite LoadArtwork(string name)
        {
            return Resources.Load<Sprite>($"Artwork/StationBadges/{name}");
        }

        private static void StretchToParent(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private void CreateTexts(Transform parent, string title, string status, Color statusColor)
        {
            var columnGO = new GameObject("Texts", typeof(VerticalLayoutGroup), typeof(LayoutElement));
            columnGO.transform.SetParent(parent, false);

            var columnLayout = columnGO.GetComponent<VerticalLayoutGroup>();
            columnLayout.childAlignment = TextAnchor.MiddleLeft;
            columnLayout.spacing = 4f;
            columnLayout.childControlWidth = true;
            columnLayout.childControlHeight = true;
            columnLayout.childForceExpandWidth = true;
            columnLayout.childForceExpandHeight = false;

            var columnElement = columnGO.GetComponent<LayoutElement>();
            columnElement.flexibleWidth = 1f;

            CreateText(columnGO.transform, "Title", title, TitleFontSize, TitleColor, FontStyles.Bold);
            CreateText(columnGO.transform, "Status", status, StatusFontSize, statusColor,
                FontStyles.Normal);
        }

        private void CreateText(Transform parent, string name, string content, float size,
            Color color, FontStyles style)
        {
            var textGO = new GameObject(name, typeof(TextMeshProUGUI));
            textGO.transform.SetParent(parent, false);

            var text = textGO.GetComponent<TextMeshProUGUI>();

            if (font != null)
                text.font = font;

            text.text = content;
            text.fontSize = size;
            text.color = color;
            text.fontStyle = style;
            text.alignment = TextAlignmentOptions.Left;
            text.raycastTarget = false;
        }

        private void EnsureLayout()
        {
            if (cardContainer == null)
                return;

            var layout = cardContainer.GetComponent<VerticalLayoutGroup>();

            if (layout == null)
                layout = cardContainer.gameObject.AddComponent<VerticalLayoutGroup>();

            layout.spacing = CardSpacing;
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
        }
    }
}
