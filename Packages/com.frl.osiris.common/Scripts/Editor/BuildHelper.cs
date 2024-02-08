#if UNITY_EDITOR

using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Osiris {
  public class BuildHelper : MonoBehaviour {
    static void Build()
    {
        string[] args = Environment.GetCommandLineArgs();
        string[] targetScenes = { };
        BuildTarget target = BuildTarget.StandaloneWindows64;
        string targetPath = "";

		BuildOptions options = BuildOptions.None;

        for (int i = 0; i < args.Length - 1; i++)
        {

            if (args[i] == "--targetScenes")
            {
                targetScenes = args[i + 1].Split(',');
            }
            else if (args[i] == "-buildTarget")
            {
                if (args[i + 1] == "Android")
                {
                    target = BuildTarget.Android;
                }
                else if (args[i + 1] == "Win64")
                {
                    target = BuildTarget.StandaloneWindows64;
                }
                else
                {
                    var err = $"Unhandled build target: \"{args[i + 1]}\"";
                    Debug.Log(err);
                    throw new Exception(err);
                }
            }
            else if (args[i] == "--targetPath")
            {
                targetPath = args[i + 1];
            }
            else if (args[i] == "-development") {
              options |= BuildOptions.Development;
            }
        }

        if (targetPath == "")
        {
            var err = "Unspecified target path";
            Debug.Log(err);
            throw new Exception(err);
        }

        var report = BuildPipeline.BuildPlayer(targetScenes, targetPath, target, options);
        switch (report.summary.result)
        {
            case BuildResult.Succeeded:
                Debug.Log("Build succeeded");
                break;
            case BuildResult.Failed:
                var err = "Build failed";
                Debug.Log(err);
                // Throw an exception to ensure that higher-level build systems are alerted to the failure
                throw new Exception(err);
            case BuildResult.Cancelled:
                Debug.Log("Build canceled");
                break;
        }
    }
  }

#if UNITY_ANDROID
    [InitializeOnLoad]
    public class OVRGradleGeneration : UnityEditor.Android.IPostGenerateGradleAndroidProject
    {
        public int callbackOrder { get { return 3; } } // taken from OVRGradleGeneration.cs

        private static void CreateGradlePropertiesForProxy(string path)
        {
            string gradlePropsPath = Path.Combine(path, "gradle.properties");

            string httpHostPart = "fwdproxy";
            string httpPortPart = "8080";

            string httpsHostPart = "fwdproxy";
            string httpsPortPart = "8080";

            StreamWriter writer = File.AppendText(gradlePropsPath);
            writer.WriteLine("org.gradle.jvmargs=-Xmx4096M");
            writer.WriteLine(String.Format("systemProp.http.proxyHost={0}", httpHostPart));
            writer.WriteLine(String.Format("systemProp.http.proxyPort={0}", httpPortPart));
            writer.WriteLine(String.Format("systemProp.https.proxyHost={0}", httpsHostPart));
            writer.WriteLine(String.Format("systemProp.https.proxyPort={0}", httpsPortPart));
            writer.Flush();
            writer.Close();

            UnityEngine.Debug.LogFormat("HTTP Proxy enabled for Gradle: [{0}:{1}]", httpHostPart, httpPortPart);
            UnityEngine.Debug.LogFormat("HTTPS Proxy enabled for Gradle: [{0}:{1}]", httpsHostPart, httpsPortPart);
            UnityEngine.Debug.LogFormat("Gradle properties set to file at: [{0}]", gradlePropsPath);
        }

        public void OnPostGenerateGradleAndroidProject(string path)
        {
            // Check if we want to write gradle props
            string writeGradleEnvVar = Environment.GetEnvironmentVariable("OSIRIS_WRITE_GRADLE_PROXY");
            bool writeGradle = writeGradleEnvVar == "true";

            if (writeGradle)
            {
                UnityEngine.Debug.LogFormat("Configuring Gradle HTTP/HTTPS proxy");
#if UNITY_2019_3_OR_NEWER
                string gradleProjectPath = Path.Combine(path, "../");
#else
                string gradleProjectPath = path;
#endif
                CreateGradlePropertiesForProxy(gradleProjectPath);
            }
        }
    }
#endif
} // namespace Osiris

#endif // UNITY_EDITOR
