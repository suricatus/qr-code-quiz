# Demo do site (suricatusgames.com/demos/)

O mesmo build atende os dois usos. Quem decide é a URL:

| URL | O que abre |
|---|---|
| `.../?station=2` … `?station=7` | **Evento**: a estação do QR Code, exatamente como sempre foi. |
| `.../?prize=1` | **Evento**: tela de premiação (o QR do estande). |
| `.../?demo=1` | **Demo**: mapa do estande, sem QR nenhum. |
| `.../` (sem parâmetro) | **Demo** também — antes essa URL abria uma tela vazia. |

Nenhum QR Code impresso precisa ser refeito.

## O que a demo faz de diferente

- **Tela de mapa** no lugar do QR: o visitante toca em "Dica 1…5" para simular o
  escaneamento daquela estação. A estação do prêmio fica travada até as 5 dicas saírem.
- **Dica já coletada pode ser relida**: tocar num cartão concluído reabre a dica.
- **Botão "Voltar ao mapa"** na pergunta e na tela de dica (no evento continua
  "escaneie o próximo QR").
- **Tela de prêmio sem formulário**: nenhum dado é pedido ou coletado na demo. No
  evento o formulário continua igual.
- **Progresso não é salvo**: cada visitante começa limpo, e jogar a demo não apaga o
  progresso de quem estiver no evento usando o mesmo navegador.

## Publicar

### 1. Preparar a cena — já feito, só refaça se mexer no visual

A cena no repositório já vem montada. Para reconstruir (por exemplo, depois de mudar
o visual das outras telas), no Unity com `Assets/Scenes/SampleScene.unity` aberta:

**Suricatus > Demo > Preparar cena da demo**

Isso recria a tela de mapa, os botões da demo e o painel da tela de prêmio, clonando
fonte, sprites e tamanhos do que já existe na cena, e salva. Rodar de novo reconstrói
tudo do zero — pode repetir sem medo.

Para testar no Editor: no objeto `GameManager`, ligue **Debug Demo Mode** e dê Play.

### 2. Buildar

**Suricatus > Build WebGL para GitHub Pages** — gera em `docs/`.

> **Tamanho do download:** o build sai sem compressão (54 MB) porque o GitHub Pages não
> manda o header `Content-Encoding`. Ligando `compressionFormat = Gzip` com
> `decompressionFallback = true` no `WebGLBuilder.cs`, o próprio loader do Unity
> descomprime no navegador e o download cai para ~19 MB, sem configurar nada no
> servidor. Vale a pena para quem abre a demo no 4G.

### 3. Subir

```bash
git add -A
git commit -m "Build da demo"
git push
```

O workflow `deploy-pages.yml` publica a pasta `docs/`. A URL fica na aba **Actions**
do repositório, no passo "Publicar" (algo como
`https://suricatus.github.io/qr-code-quiz/`) — confirme ali antes de divulgar.

### 4. Embutir na página do WordPress

Na página `/demos/`, use um bloco **HTML personalizado** (Custom HTML):

```html
<div style="max-width:420px;margin:0 auto;">
  <div style="position:relative;padding-top:177.8%;">
    <iframe
      src="https://suricatus.github.io/qr-code-quiz/?demo=1"
      title="Demo: Caça ao QR Code — Suricatus Games"
      allowfullscreen
      loading="lazy"
      style="position:absolute;top:0;left:0;width:100%;height:100%;border:0;border-radius:18px;">
    </iframe>
  </div>
  <p style="text-align:center;margin-top:12px;">
    <a href="https://suricatus.github.io/qr-code-quiz/?demo=1" target="_blank" rel="noopener">
      Abrir em tela cheia
    </a>
  </p>
</div>
```

O `padding-top: 177.8%` mantém a proporção 9:16 do jogo, que é vertical. O link de
tela cheia importa no celular: dentro do iframe a tela útil fica pequena, e é por ele
que a maioria vai jogar de verdade.

Dois cuidados do WordPress: plugins de segurança e alguns temas removem `<iframe>` do
conteúdo — se o bloco sumir ao salvar, é isso. E a página precisa ser **https**, senão
o navegador bloqueia o conteúdo do iframe.

## Se um dia vocês tiverem acesso às pastas do servidor

Dá para hospedar no próprio domínio, sem iframe: copie o conteúdo de `docs/` para algo
como `/demos/qr-quiz/` e aponte o link para lá. O build sai sem compressão
(`.wasm`/`.data` crus), então funciona em qualquer servidor sem configurar header
nenhum — em troca o download é maior. Com acesso ao servidor, vale trocar para Gzip ou
Brotli em `WebGLBuilder.cs` e servir os headers `Content-Encoding` correspondentes.
