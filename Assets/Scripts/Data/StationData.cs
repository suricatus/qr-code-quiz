using UnityEngine;

namespace Data
{
    [CreateAssetMenu(fileName = "StationData", menuName = "QR/Data")]
    public class StationData : ScriptableObject
    {
        [Header("Station Configuration")]
        public int stationId;
        
        [Header("Question")]
        [TextArea(2, 4)]
        public string questionText;
        public string[] answerOptions;
        public int correctAnswerIndex;

        [Header("Result Correct")]
        [TextArea(2, 4)]
        public string curiosityText;
        [TextArea(2, 4)]
        public string hintText;
        
        [Header("Puzzle Piece")]
        public Sprite puzzlePiece;
        public Sprite placeholderPiece;

        [Header("Final Station")]
        [Tooltip("Marque verdadeiro na última estação para aparecer a tela de premiação junto com a dica")]
        public bool isFinalStation;
    }
}
