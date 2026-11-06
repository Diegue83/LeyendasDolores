// Compilación del juego para PC (Windows 64 bits) y Android (APK), con el ícono y los ajustes de publicación.
// Menú: Leyendas > Compilar > ...   ·   Consola:
//   Unity -batchmode -projectPath ... -buildTarget Win64   -executeMethod Leyendas.Editor.Compilar.WindowsDesdeConsola
//   Unity -batchmode -projectPath ... -buildTarget Android -executeMethod Leyendas.Editor.Compilar.AndroidDesdeConsola
// Las compilaciones quedan en C:/Users/52418/LeyendasDolores_Builds (fuera del proyecto).
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Leyendas.Editor
{
    public static class Compilar
    {
        public const string CARPETA = "C:/Users/52418/LeyendasDolores_Builds";
        const string ICONO = "Assets/Iconos/icono_juego.png";
        const string VERSION = "1.0.0";
        const int VERSION_ANDROID = 1;

        [MenuItem("Leyendas/Compilar/PC (Windows)", false, 200)] static void MenuWindows() { Windows(); }
        [MenuItem("Leyendas/Compilar/Android (APK)", false, 201)] static void MenuAndroid() { Android(); }

        public static void WindowsDesdeConsola() { EditorApplication.Exit(Windows() ? 0 : 1); }
        public static void AndroidDesdeConsola() { EditorApplication.Exit(Android() ? 0 : 1); }

        static void Comunes()
        {
            PlayerSettings.companyName = "UTNG";
            PlayerSettings.productName = "Leyendas de Dolores Hidalgo";
            PlayerSettings.bundleVersion = VERSION;
            AssetDatabase.ImportAsset(ICONO);
            var icono = AssetDatabase.LoadAssetAtPath<Texture2D>(ICONO);
            if (icono != null) PlayerSettings.SetIcons(NamedBuildTarget.Unknown, new[] { icono }, IconKind.Any);
            else Debug.LogWarning("Leyendas: falta el ícono " + ICONO);
            ComprimirArte();
        }

        // Escenarios y capas con compresión crunch (sin esto el juego pesa ~700 MB). Respeta todo lo demás de cada imagen.
        public static void ComprimirArte()
        {
            int n = 0;
            var guids = AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets/Resources/Arte/Escenas", "Assets/Resources/Arte/Capas" });
            foreach (var ruta in guids.Select(AssetDatabase.GUIDToAssetPath).Concat(new[] { "Assets/Resources/Arte/Portada.jpg" }))
            {
                var imp = AssetImporter.GetAtPath(ruta) as TextureImporter;
                if (imp == null) continue;
                if (imp.textureCompression == TextureImporterCompression.Compressed && imp.crunchedCompression && imp.compressionQuality == 60) continue;
                imp.textureCompression = TextureImporterCompression.Compressed;
                imp.crunchedCompression = true;
                imp.compressionQuality = 60;
                imp.ClearPlatformTextureSettings("Standalone");
                imp.ClearPlatformTextureSettings("Android");
                imp.SaveAndReimport();
                n++;
            }
            Debug.Log("Leyendas: " + n + " imágenes pasadas a compresión crunch.");
        }

        static string[] Escenas() { return EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray(); }

        public static bool Windows()
        {
            Comunes();
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Standalone, "com.utng.leyendasdolores");
            PlayerSettings.fullScreenMode = FullScreenMode.FullScreenWindow;
            PlayerSettings.resizableWindow = true;
            PlayerSettings.defaultScreenWidth = 1280; PlayerSettings.defaultScreenHeight = 720;
            var destino = CARPETA + "/Windows/Leyendas de Dolores Hidalgo.exe";
            if (Directory.Exists(CARPETA + "/Windows")) Directory.Delete(CARPETA + "/Windows", true);
            return Construir(destino, BuildTarget.StandaloneWindows64);
        }

        public static bool Android()
        {
            Comunes();
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, "com.utng.leyendasdolores");
            PlayerSettings.Android.bundleVersionCode = VERSION_ANDROID;
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64 | AndroidArchitecture.ARMv7;
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.AutoRotation;
            PlayerSettings.allowedAutorotateToLandscapeLeft = true; PlayerSettings.allowedAutorotateToLandscapeRight = true;
            PlayerSettings.allowedAutorotateToPortrait = false; PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            EditorUserBuildSettings.buildAppBundle = false;   // APK para instalar directo (itch.io); para Google Play se usa .aab con firma propia
            Directory.CreateDirectory(CARPETA + "/Android");
            return Construir(CARPETA + "/Android/LeyendasDoloresHidalgo-" + VERSION + ".apk", BuildTarget.Android);
        }

        static bool Construir(string destino, BuildTarget objetivo)
        {
            var op = new BuildPlayerOptions { scenes = Escenas(), locationPathName = destino, target = objetivo, options = BuildOptions.None };
            var r = BuildPipeline.BuildPlayer(op);
            var s = r.summary;
            Debug.Log("Leyendas: compilación " + objetivo + " → " + s.result + " · " + (s.totalSize / 1048576f).ToString("0.0") + " MB · " + s.totalErrors + " errores · " + destino);
            return s.result == BuildResult.Succeeded;
        }
    }
}
