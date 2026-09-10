using System.Collections.Generic;
using System.Linq;
using Core;
using Data;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class PuzzleGridController : MonoBehaviour
    {
        [Header("Grid Layout")]
        [Tooltip("Colunas do grid. Deixe 0 para automático: todas as peças em uma fileira só, " +
                 "sem sobra desalinhada quando o número de estações mudar.")]
        [SerializeField] private int columnCount;

        [Header("References")]
        [SerializeField] private GameConfig config;
        [SerializeField] private RectTransform gridContainer;
        [SerializeField] private Sprite placeholderSprite;

        [Header("Cell Settings")]
        [Tooltip("Tamanho MÁXIMO de cada peça. O tamanho real é calculado para caber no " +
                 "container, sempre quadrado, para não deformar as imagens.")]
        [SerializeField] private Vector2 maxCellSize = new(220f, 220f);
        [SerializeField] private Vector2 pieceSpacing = new(12f, 12f);

        private GridLayoutGroup _gridLayout;
        private readonly List<Image> _cells = new();

        private void Awake()
        {
            EnsureGridLayout();
        }

        private void EnsureGridLayout()
        {
            _gridLayout = gridContainer.GetComponent<GridLayoutGroup>();
            if (_gridLayout == null)
                _gridLayout = gridContainer.gameObject.AddComponent<GridLayoutGroup>();

            _gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            _gridLayout.spacing = pieceSpacing;
            _gridLayout.childAlignment = TextAnchor.MiddleCenter;
        }

        public void Refresh()
        {
            foreach (var cell in _cells)
                if (cell != null) Destroy(cell.gameObject);

            _cells.Clear();

            // A estação final revela o prêmio, não entrega dica — e sua tela nem é a de
            // dica, então a peça dela nunca poderia ser preenchida aqui. Fora do grid,
            // a contagem de peças passa a bater com "Faltam X de Y dicas".
            var stations = config.stations
                .Where(s => !s.isFinalStation)
                .OrderBy(s => s.stationId)
                .ToArray();

            if (stations.Length == 0)
                return;

            ResizeCellsToFit(stations.Length);

            foreach (var station in stations)
            {
                var cellGO = new GameObject($"Piece_{station.stationId}", typeof(Image));
                cellGO.transform.SetParent(gridContainer, false);

                var img = cellGO.GetComponent<Image>();
                var isRevealed = ProgressManager.Instance.IsStationComplete(station.stationId);

                img.sprite = isRevealed && station.puzzlePiece != null
                    ? station.puzzlePiece
                    : station.placeholderPiece != null ? station.placeholderPiece : placeholderSprite;

                // As artes são quadradas (512x512). Sem isso elas são esticadas até
                // preencher a célula e viram elipses achatadas.
                img.preserveAspect = true;
                _cells.Add(img);
            }
        }

        /// <summary>
        /// Calcula uma célula quadrada que caiba no container com o número de peças atual,
        /// para o grid não estourar a área nem deixar uma fileira órfã desalinhada.
        /// </summary>
        private void ResizeCellsToFit(int pieceCount)
        {
            var cols = columnCount > 0 ? Mathf.Min(columnCount, pieceCount) : pieceCount;
            var rows = Mathf.CeilToInt(pieceCount / (float)cols);

            var area = gridContainer.rect.size;
            var widthPerCell = (area.x - pieceSpacing.x * (cols - 1)) / cols;
            var heightPerCell = (area.y - pieceSpacing.y * (rows - 1)) / rows;

            var side = Mathf.Min(widthPerCell, heightPerCell);
            if (maxCellSize.x > 0f) side = Mathf.Min(side, maxCellSize.x);
            if (maxCellSize.y > 0f) side = Mathf.Min(side, maxCellSize.y);
            side = Mathf.Max(side, 1f);

            _gridLayout.constraintCount = cols;
            _gridLayout.cellSize = new Vector2(side, side);
        }
    }
}
