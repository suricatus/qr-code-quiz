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
        // Layout fica em código, fora do Inspector: além de não precisar de ajuste,
        // campos serializados aqui já foram desfeitos várias vezes por regravações
        // da cena pelo Unity, e renomeá-los quebrava a serialização no build.
        private const float PieceSpacing = 12f;
        private const float MaxPieceSize = 220f;

        [Header("References")]
        [SerializeField] private GameConfig config;
        [SerializeField] private RectTransform gridContainer;
        [SerializeField] private Sprite placeholderSprite;

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
            _gridLayout.spacing = new Vector2(PieceSpacing, PieceSpacing);
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
        /// Peças em fileira única, com a célula quadrada e do maior tamanho que couber
        /// no container. Assim mudar o número de estações nunca deixa sobra desalinhada.
        /// </summary>
        private void ResizeCellsToFit(int pieceCount)
        {
            var area = gridContainer.rect.size;
            var widthPerPiece = (area.x - PieceSpacing * (pieceCount - 1)) / pieceCount;

            var side = Mathf.Min(widthPerPiece, area.y, MaxPieceSize);
            side = Mathf.Max(side, 1f);

            _gridLayout.constraintCount = pieceCount;
            _gridLayout.cellSize = new Vector2(side, side);
        }
    }
}
