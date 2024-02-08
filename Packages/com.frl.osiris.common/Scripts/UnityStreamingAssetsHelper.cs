using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;

namespace Osiris {

    public static class UnityStreamingAssetsHelper
    {
        // Looks for StreamingAssets/relPath in all StreamingAssets folders
        // Return absolute path if found, otherwise null.
        public static string FindStreamingAsset(string relPath) {
#if !UNITY_EDITOR && UNITY_ANDROID
            // Android runtime has only a single, constant streamingAssetsPath
            return Path.Combine(EnsurePaths()[0], relPath);
#else
            relPath = relPath.TrimStart('/', '\\');
            foreach (var root in EnsurePaths()) {
                var filepath = Path.Combine(root, relPath);
                if (System.IO.File.Exists(filepath)) {
                    return filepath;
                }
            }

            return null;
#endif
        }


        // Private fields
        static List<string> _streamingAssetsPaths = null;


        // Methods
        public static List<string> EnsurePaths() {
            // Paths already exist
            if (_streamingAssetsPaths != null) {
                return _streamingAssetsPaths;
            }

            _streamingAssetsPaths = new List<string>();

#if !UNITY_EDITOR && UNITY_ANDROID
        _streamingAssetsPaths.Add(Application.dataPath + "!/assets/");
        return _streamingAssetsPaths;
#else
#if UNITY_EDITOR
        var root = Path.Combine(Application.dataPath, "..");
#else
            var root = Application.dataPath;
#endif

            // Recursively iterate dirs from root
            _streamingAssetsPaths =
                Directory.EnumerateDirectories(Path.GetFullPath(root), "StreamingAssets", SearchOption.AllDirectories)
                    .ToList();

            // Print results
            string tmp = string.Join("\n", _streamingAssetsPaths);
            Debug.Log($"UnityStreamingAssetsHelper found: [\n{tmp}]");

            return _streamingAssetsPaths;
#endif
        }
    }

}
