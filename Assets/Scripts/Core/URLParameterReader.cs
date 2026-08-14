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
#endif

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
            var url = Application.absoluteURL;
            var index = url.IndexOf("?");
            return index >= 0 ? url.Substring(index) : string.Empty;
#endif
        }

        private static Dictionary<string, string> ParseQueryString(string queryString)
        {
            var result = new Dictionary<string, string>();

            if (queryString.StartsWith("?"))
                queryString = queryString.Substring(1);

            foreach (var part in queryString.Split('&'))
            {
                var pair = part.Split('=');
                if (pair.Length == 2)
                    result[pair[0].ToLowerInvariant()] = Uri.UnescapeDataString(pair[1]);
            }
            
            return result;
        }
    }
}