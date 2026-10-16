// Menú "Leyendas" en la barra de Unity (versión nativa: las escenas ya están guardadas; nada aquí las reconstruye).
//  2. Restablecer importación del arte → vuelve a aplicar los ajustes nítidos a las imágenes.
//  3. Configurar para Android / iOS  → orientación horizontal, nombre e identificador.
//  Borrar partida guardada.
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace Leyendas.Editor
{
    public static class MenuLeyendas
    {
        const string DATOS = "Assets/Resources/DatosJuego.asset";

        [MenuItem("Leyendas/Datos/Crear o abrir DatosJuego", false, 20)]
        public static void AbrirDatos() { Selection.activeObject = CrearDatos(false); }

        [MenuItem("Leyendas/Datos/Restablecer DatosJuego a los valores originales", false, 21)]
        public static void RestablecerDatos()
        {
            if (!EditorUtility.DisplayDialog("Restablecer datos", "Se perderán los cambios que hiciste en textos, códigos y pistas. ¿Continuar?", "Restablecer", "Cancelar")) return;
            Selection.activeObject = CrearDatos(true);
        }

        public static DatosJuego CrearDatos(bool reiniciar)
        {
            Directory.CreateDirectory("Assets/Resources");
            var d = AssetDatabase.LoadAssetAtPath<DatosJuego>(DATOS);
            if (d == null)
            {
                d = ScriptableObject.CreateInstance<DatosJuego>();
                DatosPorDefecto.Llenar(d);
                AssetDatabase.CreateAsset(d, DATOS);
            }
            else if (reiniciar || d.version < DatosJuego.VERSION)
            {
                DatosPorDefecto.Llenar(d);
                EditorUtility.SetDirty(d);
            }
            AssetDatabase.SaveAssets();
            return d;
        }

        [MenuItem("Leyendas/Borrar partida guardada", false, 40)]
        public static void BorrarPartida()
        {
            Estado.BorrarTodas();
            Debug.Log("Leyendas: partidas guardadas borradas (las 3 ranuras).");
        }
    }

    // Ajustes de importación del arte: nítido (sin mipmaps), sprites, y bordes 9-slice para la interfaz.
    public class ImportadorArte : AssetPostprocessor
    {
        static bool forzar;

        void OnPreprocessTexture()
        {
            if (!assetPath.StartsWith("Assets/Resources/Arte/")) return;
            var imp = (TextureImporter)assetImporter;
            if (!forzar && !imp.importSettingsMissing) return; // respeta lo que cambies a mano
            Aplicar(imp, assetPath);
        }

        public static void Aplicar(TextureImporter imp, string ruta)
        {
            imp.textureType = TextureImporterType.Sprite;
            imp.spriteImportMode = SpriteImportMode.Single;
            imp.mipmapEnabled = false;
            imp.alphaIsTransparency = true;
            imp.filterMode = FilterMode.Bilinear;
            imp.wrapMode = TextureWrapMode.Clamp;
            imp.maxTextureSize = 2048;
            bool ui = ruta.Contains("/Arte/UI/");
            bool grande = ruta.Contains("/Arte/Escenas/") || ruta.Contains("/Arte/Capas/") || ruta.EndsWith("Portada.jpg");
            // Escenarios y capas: comprimidas con crunch (pesan poco en disco; son pintura suave, no se nota).
            // Ojo: solo se comprimen si sus lados son múltiplos de 4 (el generador de arte ya las deja así).
            imp.textureCompression = grande ? TextureImporterCompression.Compressed : TextureImporterCompression.Uncompressed;
            imp.crunchedCompression = grande;
            imp.compressionQuality = 60;
            imp.spritePixelsPerUnit = ui ? 200 : 100;
            var s = new TextureImporterSettings();
            imp.ReadTextureSettings(s);
            s.spriteMeshType = SpriteMeshType.FullRect;
            s.spriteAlignment = (int)SpriteAlignment.Center;
            if (ruta.EndsWith("UI/patron.png")) s.wrapMode = TextureWrapMode.Repeat;
            imp.SetTextureSettings(s);
            imp.spriteBorder = Borde(Path.GetFileNameWithoutExtension(ruta), ui);
        }

        // Bordes 9-slice (izquierda, abajo, derecha, arriba) en píxeles del arte al doble.
        static Vector4 Borde(string n, bool ui)
        {
            if (!ui) return Vector4.zero;
            switch (n)
            {
                case "panel": return new Vector4(96, 96, 96, 96);
                case "papel": return new Vector4(88, 88, 88, 88);
                case "boton": return new Vector4(60, 68, 60, 60);
                case "ranura": return new Vector4(56, 56, 56, 56);
                case "marco": return new Vector4(92, 92, 92, 92);
                case "redondo": return new Vector4(44, 44, 44, 44);
            }
            return Vector4.zero;
        }

        [MenuItem("Leyendas/2. Restablecer importación del arte", false, 3)]
        public static void Restablecer()
        {
            var guids = AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets/Resources/Arte" });
            try
            {
                forzar = true;
                AssetDatabase.StartAssetEditing();
                foreach (var g in guids)
                {
                    var ruta = AssetDatabase.GUIDToAssetPath(g);
                    var imp = AssetImporter.GetAtPath(ruta) as TextureImporter;
                    if (imp == null) continue;
                    Aplicar(imp, ruta);
                    imp.SaveAndReimport();
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
                forzar = false;
            }
            Debug.Log("Leyendas: " + guids.Length + " imágenes con ajustes nítidos.");
        }
    }

    public static class ConfigurarProyecto
    {
        [MenuItem("Leyendas/3. Configurar para Android e iOS", false, 4)]
        public static void Menu() { Aplicar(true); }

        public static void Aplicar(bool avisar)
        {
            PlayerSettings.companyName = "UTNG";
            PlayerSettings.productName = "Leyendas de Dolores Hidalgo";
            PlayerSettings.SetApplicationIdentifier(UnityEditor.Build.NamedBuildTarget.Android, "com.utng.leyendasdolores");
            PlayerSettings.SetApplicationIdentifier(UnityEditor.Build.NamedBuildTarget.iOS, "com.utng.leyendasdolores");
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.AutoRotation;
            PlayerSettings.allowedAutorotateToPortrait = false;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.allowedAutorotateToLandscapeLeft = true;
            PlayerSettings.allowedAutorotateToLandscapeRight = true;
            PlayerSettings.bundleVersion = "0.6.0";
            if (avisar) EditorUtility.DisplayDialog("Leyendas", "Listo: horizontal, nombre del juego e identificador com.utng.leyendasdolores.", "OK");
        }
    }
}
