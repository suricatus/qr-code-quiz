namespace Core
{
    /// <summary>
    /// Como o jogo foi aberto. O mesmo build atende os dois: o QR do evento traz
    /// "?station=N" na URL, e a demo do site abre sem parâmetro nenhum.
    /// </summary>
    public enum GameMode
    {
        /// <summary>Evento real: cada estação é um QR Code impresso no estande.</summary>
        Event,

        /// <summary>Demo do site: não existe QR para escanear, a navegação é pelo mapa.</summary>
        Demo
    }
}
