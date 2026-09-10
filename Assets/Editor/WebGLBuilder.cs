using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace EditorTools
{
    /// <summary>
    /// Gera a build WebGL direto na pasta que o GitHub Pages publica, já com os
    /// ajustes que o Pages exige.
    /// </summary>
    public static class WebGLBuilder
    {
        private const string OutputFolderName = "docs";

        [MenuItem("Suricatus/Build WebGL para GitHub Pages")]
        public static void BuildForPages()
        {
            var projectRoot = Directory.GetParent(Application.dataPath).FullName;
            var outputPath = Path.Combine(projectRoot, OutputFolderName);

            var scenes = EditorBuildSettings.scenes
                .Where(scene => scene.enabled)
                .Select(scene => scene.path)
                .ToArray();

            if (scenes.Length == 0)
            {
                Debug.LogError("[WebGLBuilder] Nenhuma cena habilitada em File > Build Settings.");
                return;
            }

            // O GitHub Pages não manda o header Content-Encoding, então arquivos .br/.gz
            // não carregam. Sem compressão é a combinação que funciona lá.
            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Disabled;
            PlayerSettings.WebGL.decompressionFallback = false;

            // Nomes previsíveis entre builds, para o cache do celular não servir arquivo velho.
            PlayerSettings.WebGL.nameFilesAsHashes = false;

            // Template próprio: o "Default" do Unity não reage ao teclado do celular.
            PlayerSettings.WebGL.template = "PROJECT:SuricatusMobile";

            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.WebGL)
                EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.WebGL, BuildTarget.WebGL);

            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = outputPath,
                target = BuildTarget.WebGL,
                options = BuildOptions.None
            });

            if (report.summary.result != BuildResult.Succeeded)
            {
                Debug.LogError($"[WebGLBuilder] Build falhou: {report.summary.result}");
                return;
            }

            // Sem isso o Pages roda o Jekyll e ignora pastas iniciadas por "_".
            File.WriteAllText(Path.Combine(outputPath, ".nojekyll"), string.Empty);

            Debug.Log($"[WebGLBuilder] Build pronta em {outputPath}. Agora commite a pasta '{OutputFolderName}' e dê push.");
        }
    }
}
