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
        [SerializeField] private int columns = 3;

        [Header("References")]
        [SerializeField] private GameConfig config;
        [SerializeField] private RectTransform gridContainer;
        [SerializeField] private Sprite placeholderSprite;

        [Header("Cell Settings")]
        [SerializeField] private Vector2 cellSize = new(220f, 220f);
        [SerializeField] private Vector2 cellSpacing = new(4f, 4f);

        private GridLayoutGroup _gridLayout;
        private readonly List<Image> _cells = new();

        private void Awake()
        {
            InitializeGridLayout();
        }

        private void InitializeGridLayout()
        {
            _gridLayout = gridContainer.GetComponent<GridLayoutGroup>();
            if (_gridLayout == null)
                _gridLayout = gridContainer.gameObject.AddComponent<GridLayoutGroup>();

            _gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            _gridLayout.constraintCount = columns;
            _gridLayout.cellSize = cellSize;
            _gridLayout.spacing = cellSpacing;
            _gridLayout.childAlignment = TextAnchor.MiddleCenter;
        }
        
        public void Refresh()
        {
            foreach (var cell in _cells)
                if (cell != null) Destroy(cell.gameObject);

            _cells.Clear();

            var stations = config.stations.OrderBy(s => s.stationId).ToArray();

            foreach (var station in stations)
            {
                var cellGO = new GameObject($"Piece_{station.stationId}", typeof(Image));
                cellGO.transform.SetParent(gridContainer, false);

                var img = cellGO.GetComponent<Image>();
                var isRevealed = ProgressManager.Instance.IsStationComplete(station.stationId);

                img.sprite = isRevealed && station.puzzlePiece != null
                    ? station.puzzlePiece
                    : station.placeholderPiece != null ? station.placeholderPiece : placeholderSprite;

                img.preserveAspect = false;
                _cells.Add(img);
            }
        }
    }
}
