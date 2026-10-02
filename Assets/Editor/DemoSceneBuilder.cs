using System.Linq;
using Core;
using TMPro;
using UI;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace EditorTools
{
    /// <summary>
    /// Monta na cena o que só a demo do site usa: o mapa do estande, que faz o papel do
    /// QR Code, os botões de navegação (no evento o jogador anda escaneando QR, então as
    /// telas de dica e final não têm botão nenhum) e o painel que substitui o formulário
    /// na tela de prêmio.
    ///
    /// É um script de editor em vez de UI montada à mão por dois motivos: o visual é
    /// clonado do que já existe na cena, então nada sai do tom; e rodar o menu de novo
    /// reconstrói tudo igual, sem resíduo da tentativa anterior.
    /// </summary>
    public static class DemoSceneBuilder
    {
        private const string MapScreenName = "MapScreen";
        private const string DemoPanelName = "DemoPanel";
        private const string BackButtonName = "DemoBackButton";
        private const string ContinueButtonName = "DemoContinueButton";
        private const string FormObjectName = "Form";

        private const string MapTitle = "Caça às dicas";
        private const string MapSubtitle =
            "No evento, cada dica fica atrás de um QR Code espalhado pelo estande. " +
            "Aqui, toque no ponto para simular o escaneamento.";

        // Medidas tiradas das telas que já existem: o botão padrão do jogo é 485x158,
        // ancorado embaixo. Mantê-las iguais é o que faz a demo não parecer enxertada.
        private const float StandardButtonWidth = 485f;
        private const float StandardButtonHeight = 158f;
        private const float LogoSizeOnMap = 320f;

        private static readonly Color PanelColor = new(0.10f, 0.11f, 0.20f, 0.92f);
        private static readonly Color TitleColor = Color.white;
        // O fundo atrás do texto é ciano claro: texto branco some, azul-escuro lê.
        private static readonly Color SubtitleColor = new(0.06f, 0.14f, 0.32f);
        private static readonly Color ProgressColor = new(0.95f, 0.3f, 0.4f);

        [MenuItem("Suricatus/Demo/Preparar cena da demo")]
        public static void Build()
        {
            if (!TryBuild(out var error))
                EditorUtility.DisplayDialog("Demo", error, "Ok");
        }

        /// <summary>
        /// Entrada para linha de comando (-executeMethod). Em batch não há cena aberta,
        /// então ela é aberta aqui antes de montar.
        /// </summary>
        public static void PrepareFromCommandLine()
        {
            var scenePath = EditorBuildSettings.scenes.FirstOrDefault(s => s.enabled)?.path;

            if (string.IsNullOrEmpty(scenePath))
            {
                Debug.LogError("[DemoSceneBuilder] Nenhuma cena habilitada em Build Settings.");
                EditorApplication.Exit(1);
                return;
            }

            EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

            if (!TryBuild(out var error))
            {
                Debug.LogError("[DemoSceneBuilder] " + error);
                EditorApplication.Exit(1);
                return;
            }

            EditorApplication.Exit(0);
        }

        private static bool TryBuild(out string error)
        {
            error = null;
            var uiController = FindInScene<UIController>();
            var gameManager = FindInScene<GameManager>();

            if (uiController == null || gameManager == null)
            {
                error = "Abra a cena do jogo (Assets/Scenes/SampleScene.unity) antes de rodar " +
                        "este menu.";
                return false;
            }

            var ui = new SerializedObject(uiController);
            var quizScreen = ScreenOf(ui, "quizScreen");
            var correctScreen = ScreenOf(ui, "correctScreen");
            var hintScreen = ScreenOf(ui, "hintScreen");
            var finalScreen = ScreenOf(ui, "finalScreen");
            var prizeScreen = ScreenOf(ui, "prizeScreen");

            if (quizScreen == null)
            {
                error = "O UIController está sem a tela de quiz atribuída, então não há de " +
                        "onde clonar o visual nem onde encaixar o mapa.";
                return false;
            }

            var template = FindButtonTemplate(correctScreen, prizeScreen, quizScreen);

            if (template == null)
            {
                error = "Não encontrei nenhum botão na cena para usar como modelo visual " +
                        "dos botões da demo.";
                return false;
            }

            var font = SampleFont(quizScreen);
            var mapScreen = BuildMapScreen(quizScreen, gameManager, hintScreen, template, font);

            ui.FindProperty("mapScreen").objectReferenceValue = mapScreen;
            ui.ApplyModifiedProperties();

            // Faixa livre de cada tela, medida na cena com os pivots: na de dica sobra o
            // rodapé abaixo do contador; na final, a arte do prêmio ocupa de 339 a 960
            // (pivot embaixo), então o botão só cabe abaixo dela.
            BuildDemoButton<HintScreenController>(hintScreen, template, "Voltar ao mapa", 100f);
            BuildDemoButton<FinalScreenController>(finalScreen, template, "Ver meu prêmio", 170f);
            BuildQuizBackButton(quizScreen, template);
            BuildPrizeDemoPanel(prizeScreen, template, font);

            var scene = uiController.gameObject.scene;
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            Debug.Log("[DemoSceneBuilder] Cena da demo pronta: mapa criado e telas de quiz, " +
                      "dica, final e prêmio ajustadas. Para testar, ligue 'Debug Demo Mode' " +
                      "no GameManager e dê Play.");
            return true;
        }

        // --- Mapa ------------------------------------------------------------

        private static GameObject BuildMapScreen(GameObject quizScreen, GameManager gameManager,
            GameObject hintScreen, Button template, TMP_FontAsset font)
        {
            var canvas = quizScreen.transform.parent;
            var existing = canvas.Find(MapScreenName);

            if (existing != null)
                Object.DestroyImmediate(existing.gameObject);

            var mapScreen = NewUIObject(MapScreenName, canvas);
            CopyRect(quizScreen.GetComponent<RectTransform>(), mapScreen.GetComponent<RectTransform>());

            CloneBackdrop(quizScreen, mapScreen.transform);

            var title = CreateText(mapScreen.transform, "Title", MapTitle, 76f, TitleColor,
                FontStyles.Bold, font);
            TopAnchored(title.rectTransform, 350f, 100f, 60f);

            var subtitle = CreateText(mapScreen.transform, "Subtitle", MapSubtitle, 40f,
                SubtitleColor, FontStyles.Normal, font);
            TopAnchored(subtitle.rectTransform, 460f, 190f, 80f);

            // Altura para duas linhas: ao coletar tudo a frase cresce e invadia o subtítulo.
            var progress = CreateText(mapScreen.transform, "Progress", "", 42f, ProgressColor,
                FontStyles.Bold, font);
            TopAnchored(progress.rectTransform, 660f, 120f, 60f);

            var container = NewUIObject("Cards", mapScreen.transform).GetComponent<RectTransform>();
            Stretch(container, 70f, 70f, 800f, 240f);

            var restartButton = CloneButton(template, mapScreen.transform, "RestartButton",
                "Começar de novo");
            BottomAnchored(restartButton.GetComponent<RectTransform>(), 100f,
                StandardButtonWidth, StandardButtonHeight);

            var controller = mapScreen.AddComponent<MapScreenController>();
            var map = new SerializedObject(controller);
            map.FindProperty("config").objectReferenceValue = gameManager.config;
            map.FindProperty("cardContainer").objectReferenceValue = container;
            map.FindProperty("progressText").objectReferenceValue = progress;
            map.FindProperty("restartButton").objectReferenceValue = restartButton;
            map.FindProperty("font").objectReferenceValue = font;
            map.FindProperty("cardSprite").objectReferenceValue = SampleButtonSprite(template);
            map.FindProperty("fallbackBadge").objectReferenceValue = PuzzlePlaceholder(hintScreen);
            map.ApplyModifiedProperties();

            mapScreen.SetActive(false);
            return mapScreen;
        }

        /// <summary>
        /// Fundo, véu e logo são iguais em todas as telas; clonar os da tela de quiz é o
        /// que garante que o mapa não pareça de outro jogo. A logo entra menor, senão os
        /// 507px dela comem o espaço dos cartões.
        /// </summary>
        private static void CloneBackdrop(GameObject source, Transform parent)
        {
            var decor = source.GetComponentsInChildren<Image>(true)
                .Where(i => i.transform.parent == source.transform)
                .Where(i => i.name.StartsWith("Background") || i.name.StartsWith("Fade") ||
                            i.name.StartsWith("Logo"))
                .ToArray();

            foreach (var image in decor)
            {
                var clone = Object.Instantiate(image.gameObject, parent);
                clone.name = image.name;
                clone.SetActive(true);

                if (!image.name.StartsWith("Logo"))
                    continue;

                var rect = clone.GetComponent<RectTransform>();
                rect.sizeDelta = new Vector2(LogoSizeOnMap, LogoSizeOnMap);
                rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, -10f);
            }
        }

        private static Sprite PuzzlePlaceholder(GameObject hintScreen)
        {
            if (hintScreen == null)
                return null;

            var grid = hintScreen.GetComponentInChildren<PuzzleGridController>(true);

            if (grid == null)
                return null;

            return new SerializedObject(grid).FindProperty("placeholderSprite")
                .objectReferenceValue as Sprite;
        }

        /// <summary>
        /// A mensagem vai dentro de uma cópia do painel do título: texto solto sobre o
        /// fundo do jogo fica ilegível, e o painel já traz a cor e a fonte certas.
        /// </summary>
        private static void CreateMessage(Transform parent, TextMeshProUGUI titleText,
            TMP_FontAsset font)
        {
            const string content =
                "É assim que o visitante termina a caça no seu evento: com o prêmio na mão " +
                "e o estande inteiro visitado.";

            var titlePanel = titleText != null ? titleText.transform.parent : null;

            if (titlePanel == null)
            {
                var plain = CreateText(parent, "Message", content, 42f, TitleColor,
                    FontStyles.Normal, font);
                Stretch(plain.rectTransform, 20f, 20f, 40f, 360f);
                return;
            }

            var clone = Object.Instantiate(titlePanel.gameObject, parent);
            clone.name = "MessagePanel";
            clone.SetActive(true);
            Stretch(clone.GetComponent<RectTransform>(), 0f, 0f, 40f, 360f);

            var text = clone.GetComponentInChildren<TextMeshProUGUI>(true);

            if (text == null)
                return;

            text.text = content;
            text.alignment = TextAlignmentOptions.Center;
        }

        // --- Botões só da demo -----------------------------------------------

        /// <summary>
        /// Cria o botão que a tela não tem. No evento o jogador sai destas telas
        /// escaneando o próximo QR Code, então o botão nasce desativado e só o
        /// controller o liga, quando o jogo abre em modo demo.
        /// </summary>
        private static void BuildDemoButton<T>(GameObject screen, Button template, string label,
            float fromBottom) where T : MonoBehaviour
        {
            if (screen == null)
                return;

            var controller = screen.GetComponentInChildren<T>(true);

            if (controller == null)
            {
                Debug.LogWarning($"[DemoSceneBuilder] {typeof(T).Name} não encontrado em " +
                                 $"{screen.name}; o botão da demo não foi criado.");
                return;
            }

            ReplaceChild(screen.transform, ContinueButtonName);

            var button = CloneButton(template, screen.transform, ContinueButtonName, label);
            BottomAnchored(button.GetComponent<RectTransform>(), fromBottom,
                StandardButtonWidth, StandardButtonHeight);

            var serialized = new SerializedObject(controller);
            serialized.FindProperty("demoContinueButton").objectReferenceValue = button;
            serialized.ApplyModifiedProperties();

            button.gameObject.SetActive(false);
        }

        /// <summary>
        /// Na demo o jogador pode abrir uma pergunta e querer sair sem responder; no evento
        /// ele simplesmente escaneia outro QR, então este botão só aparece na demo.
        /// </summary>
        private static void BuildQuizBackButton(GameObject quizScreen, Button template)
        {
            var controller = quizScreen.GetComponentInChildren<QuizUIController>(true);

            if (controller == null)
                return;

            ReplaceChild(quizScreen.transform, BackButtonName);

            var button = CloneButton(template, quizScreen.transform, BackButtonName, "Voltar");
            TopLeftAnchored(button.GetComponent<RectTransform>(), 30f, 30f, 260f, 96f);
            ShrinkLabel(button, 38f);

            var serialized = new SerializedObject(controller);
            serialized.FindProperty("demoBackButton").objectReferenceValue = button;
            serialized.ApplyModifiedProperties();

            button.gameObject.SetActive(false);
        }

        // --- Tela de prêmio --------------------------------------------------

        private static void BuildPrizeDemoPanel(GameObject prizeScreen, Button template,
            TMP_FontAsset font)
        {
            if (prizeScreen == null)
                return;

            var controller = prizeScreen.GetComponentInChildren<PrizeScreenController>(true);

            if (controller == null)
                return;

            var form = prizeScreen.GetComponentsInChildren<Transform>(true)
                .FirstOrDefault(t => t.name == FormObjectName);

            ReplaceChild(prizeScreen.transform, DemoPanelName);

            var panel = NewUIObject(DemoPanelName, prizeScreen.transform);

            // Ocupa a faixa que o formulário, o aceite de termos e o botão de enviar
            // deixam livre quando a demo os esconde: do título até a base da tela.
            Stretch(panel.GetComponent<RectTransform>(), 70f, 70f, 850f, 240f);

            CreateMessage(panel.transform, FindTitleText(prizeScreen), font);

            var button = CloneButton(template, panel.transform, "RestartButton", "Jogar de novo");
            BottomAnchored(button.GetComponent<RectTransform>(), 80f, StandardButtonWidth,
                StandardButtonHeight);

            var prize = new SerializedObject(controller);
            prize.FindProperty("formRoot").objectReferenceValue = form != null ? form.gameObject : null;
            prize.FindProperty("demoPanel").objectReferenceValue = panel;
            prize.FindProperty("demoRestartButton").objectReferenceValue = button;
            prize.FindProperty("titleText").objectReferenceValue = FindTitleText(prizeScreen);
            prize.ApplyModifiedProperties();

            panel.SetActive(false);
        }

        /// <summary>
        /// Título da tela de prêmio: o texto que fica fora do formulário, do aceite de
        /// termos, do botão de enviar e do painel de agradecimento.
        /// </summary>
        private static TextMeshProUGUI FindTitleText(GameObject prizeScreen)
        {
            var excluded = new[]
            {
                FormObjectName, "TermsToggle", "Thanks Panel", "Submit Button", DemoPanelName
            };

            return prizeScreen.GetComponentsInChildren<TextMeshProUGUI>(true)
                .Where(t => !IsUnder(t.transform, prizeScreen.transform, excluded))
                .OrderByDescending(t => t.name == "FieldText")
                .FirstOrDefault();
        }

        private static bool IsUnder(Transform node, Transform root, string[] names)
        {
            for (var current = node; current != null && current != root; current = current.parent)
                if (names.Contains(current.name))
                    return true;

            return false;
        }

        // --- Clonagem e estilo -----------------------------------------------

        /// <summary>
        /// Um botão qualquer da cena serve de modelo, desde que tenha texto filho: é dele
        /// que saem sprite, cor, fonte e tamanho dos botões da demo.
        /// </summary>
        private static Button FindButtonTemplate(params GameObject[] screens)
        {
            return screens
                .Where(screen => screen != null)
                .SelectMany(screen => screen.GetComponentsInChildren<Button>(true))
                .FirstOrDefault(b => b.GetComponentInChildren<TextMeshProUGUI>(true) != null);
        }

        private static Button CloneButton(Button template, Transform parent, string name,
            string label)
        {
            var clone = Object.Instantiate(template.gameObject, parent);
            clone.name = name;
            clone.SetActive(true);

            var button = clone.GetComponent<Button>();

            // O clone herda o onClick do original, que aponta para o controller da outra
            // tela. Sem limpar, o botão da demo dispararia a ação errada.
            while (button.onClick.GetPersistentEventCount() > 0)
                UnityEventTools.RemovePersistentListener(button.onClick, 0);

            var text = clone.GetComponentInChildren<TextMeshProUGUI>(true);

            if (text != null)
                text.text = label;

            return button;
        }

        private static void ShrinkLabel(Button button, float fontSize)
        {
            var text = button.GetComponentInChildren<TextMeshProUGUI>(true);

            if (text == null)
                return;

            text.fontSize = fontSize;
            text.enableAutoSizing = false;
        }

        private static Sprite SampleButtonSprite(Button template)
        {
            var image = template.GetComponent<Image>();
            return image != null ? image.sprite : null;
        }

        private static TMP_FontAsset SampleFont(GameObject screen)
        {
            return screen.GetComponentsInChildren<TextMeshProUGUI>(true)
                .Select(t => t.font)
                .FirstOrDefault(f => f != null);
        }

        private static GameObject NewUIObject(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return go;
        }

        private static TextMeshProUGUI CreateText(Transform parent, string name, string content,
            float size, Color color, FontStyles fontStyle, TMP_FontAsset font)
        {
            var text = NewUIObject(name, parent).AddComponent<TextMeshProUGUI>();

            if (font != null)
                text.font = font;

            text.text = content;
            text.fontSize = size;
            text.color = color;
            text.fontStyle = fontStyle;
            text.alignment = TextAlignmentOptions.Center;
            text.raycastTarget = false;
            return text;
        }

        private static void ReplaceChild(Transform parent, string name)
        {
            var existing = parent.Find(name);

            if (existing != null)
                Object.DestroyImmediate(existing.gameObject);
        }

        // --- Layout ----------------------------------------------------------

        private static void CopyRect(RectTransform source, RectTransform target)
        {
            if (source == null || target == null)
                return;

            target.anchorMin = source.anchorMin;
            target.anchorMax = source.anchorMax;
            target.pivot = source.pivot;
            target.anchoredPosition = source.anchoredPosition;
            target.sizeDelta = source.sizeDelta;
        }

        private static void Stretch(RectTransform rect, float left, float right, float top,
            float bottom)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.offsetMin = new Vector2(left, bottom);
            rect.offsetMax = new Vector2(-right, -top);
        }

        private static void TopAnchored(RectTransform rect, float fromTop, float height,
            float margin)
        {
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.offsetMin = new Vector2(margin, -(fromTop + height));
            rect.offsetMax = new Vector2(-margin, -fromTop);
        }

        /// <summary>
        /// Mesma convenção dos botões da cena: ancorado embaixo no centro, pivot no meio.
        /// </summary>
        private static void BottomAnchored(RectTransform rect, float fromBottom, float width,
            float height)
        {
            rect.anchorMin = new Vector2(0.5f, 0f);
            rect.anchorMax = new Vector2(0.5f, 0f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(0f, fromBottom);
            rect.sizeDelta = new Vector2(width, height);
        }

        private static void TopLeftAnchored(RectTransform rect, float fromLeft, float fromTop,
            float width, float height)
        {
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(fromLeft, -fromTop);
            rect.sizeDelta = new Vector2(width, height);
        }

        private static GameObject ScreenOf(SerializedObject uiController, string propertyName)
        {
            return uiController.FindProperty(propertyName).objectReferenceValue as GameObject;
        }

        /// <summary>
        /// Resources.FindObjectsOfTypeAll acha também os objetos desativados, que é o caso
        /// de quase toda tela do jogo; o filtro por cena descarta o que vem de prefab.
        /// </summary>
        private static T FindInScene<T>() where T : MonoBehaviour
        {
            return Resources.FindObjectsOfTypeAll<T>()
                .FirstOrDefault(c => c.gameObject.scene.IsValid());
        }
    }
}
