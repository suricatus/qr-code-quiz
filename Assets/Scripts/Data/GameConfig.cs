using System.Linq;
using UnityEngine;

namespace Data
{
    [CreateAssetMenu(fileName = "GameConfig", menuName = "QR/GameConfig")]
    public class GameConfig : ScriptableObject
    {
        [Header("All Stations")]
        public StationData[]  stations;
        
        public StationData GetStation(int stationId)
        {
            return stations.FirstOrDefault(s => s.stationId == stationId);
        }
    }
}