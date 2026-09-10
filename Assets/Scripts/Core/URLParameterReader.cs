using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Core
{
    public static class URLParameterReader 
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern string GetURLParameters();

        [DllImport("__Internal")]
        private static extern void ReloadGame();
#endif

#if UNITY_EDITOR
        /// <summary>
        /// Query string simulada no Editor (ex.: "?station=1"). Só é usada quando a URL real está vazia,
        /// que é sempre o caso no Play Mode.
        /// </summary>
        public static string EditorQueryStringOverride { get; set; }
#endif

        /// <summary>
        /// Recarrega a página mantendo a query string do QR. Só tem efeito em build WebGL.
        /// </summary>
        public static void Reload()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            ReloadGame();
#endif
        }

        public static string GetParameter(string key)
        {
            var queryString = GetQueryString();
            if (string.IsNullOrEmpty(queryString))
                return null;
            
            var parameters = ParseQueryString(queryString);
            parameters.TryGetValue(key.ToLowerInvariant(), out var value);
            return value;
        }

        private static string GetQueryString()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            return GetURLParameters();
#else
            var url = Application.absoluteURL ?? string.Empty;
            var index = url.IndexOf('?');
            if (index >= 0)
                return url.Substring(index);
#if UNITY_EDITOR
            return EditorQueryStringOverride;
#else
            return string.Empty;
#endif
#endif
        }

        private static Dictionary<string, string> ParseQueryString(string queryString)
        {
            var result = new Dictionary<string, string>();

            if (queryString.StartsWith("?"))
                queryString = queryString.Substring(1);

            // Alguns leitores de QR devolvem a URL com o fragmento junto (".../?station=1#algo").
            var fragment = queryString.IndexOf('#');
            if (fragment >= 0)
                queryString = queryString.Substring(0, fragment);

            foreach (var part in queryString.Split('&'))
            {
                if (string.IsNullOrEmpty(part))
                    continue;

                var pair = part.Split(new[] { '=' }, 2);
                var key = Uri.UnescapeDataString(pair[0]).ToLowerInvariant();

                // "?prize" sem valor conta como flag ligada, igual a "?prize=1".
                result[key] = pair.Length == 2 ? Uri.UnescapeDataString(pair[1]) : "1";
            }

            return result;
        }
    }
}