using Core;
using UnityEngine;

namespace UI
{
    public class WrongScreenController : MonoBehaviour
    {
        public void OnTryAgainClicked()
        {
            // Repete a estação que o jogador errou. Ler de novo a URL funcionava no
            // evento, mas na demo não há "?station=" para reler.
            GameManager.Instance.ReloadCurrentStation();
        }
    }
}