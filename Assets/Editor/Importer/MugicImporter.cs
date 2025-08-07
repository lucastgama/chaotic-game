using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class MugicImporter : EditorWindow
{
    private const string jsonPath = "Assets/Editor/chaotic_mugic.json";
    private const string savePath = "Assets/Resources/Data/Mugic";

    private static int startingId = 1;
    private static int mugicPerBatch = 12;

    [MenuItem("Chaotic Tools/Import Mugic")]
    public static void ShowWindow()
    {
        GetWindow<MugicImporter>("Mugic Importer");
    }

    void OnGUI()
    {
        GUILayout.Label("Mugic Importer", EditorStyles.boldLabel);

        startingId = EditorGUILayout.IntField("Starting ID:", startingId);
        mugicPerBatch = EditorGUILayout.IntField("Mugic per Batch:", mugicPerBatch);

        if (GUILayout.Button("Import Mugic"))
        {
            ImportMugic();
        }

        if (GUILayout.Button("Import ALL Mugic"))
        {
            ImportAllMugic();
        }
    }

    public static void ImportMugic()
    {
        ImportMugicInternal(mugicPerBatch);
    }

    public static void ImportAllMugic()
    {
        ImportMugicInternal(-1);
    }

    private static void ImportMugicInternal(int maxCount)
    {
        if (!File.Exists(jsonPath))
        {
            Debug.LogError("Arquivo JSON não encontrado em: " + jsonPath);
            return;
        }

        if (!Directory.Exists(savePath))
        {
            Directory.CreateDirectory(savePath);
            AssetDatabase.Refresh();
        }

        string jsonText = File.ReadAllText(jsonPath);

        try
        {
            List<MugicJsonData> mugics = JsonUtilityWrapper.FromJsonArray<MugicJsonData>(jsonText);

            if (mugics == null || mugics.Count == 0)
            {
                Debug.LogError("Nenhum mugic encontrado no JSON!");
                return;
            }

            int count = 0;
            int imported = 0;

            foreach (var data in mugics)
            {
                if (maxCount > 0 && count >= maxCount)
                    break;

                if (string.IsNullOrEmpty(data.name))
                {
                    Debug.LogWarning($"Mugic {count} sem nome, pulando...");
                    count++;
                    continue;
                }

                try
                {
                    Mugic mugic = CreateMugicFromData(data, startingId + count);

                    string fileName = SanitizeFileName(mugic.cardName);
                    string assetPath = Path.Combine(savePath, $"{mugic.cardCode}.asset");

                    if (AssetDatabase.LoadAssetAtPath<Mugic>(assetPath) != null)
                    {
                        Debug.LogWarning($"Mugic {mugic.cardName} já existe, sobrescrevendo...");
                    }

                    AssetDatabase.CreateAsset(mugic, assetPath);
                    imported++;

                    Debug.Log($"Mugic importado: {mugic.cardName} ({mugic.cardCode})");
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Erro ao importar mugic {data.name}: {e.Message}");
                }

                count++;
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log(
                $"Importação finalizada! {imported} mugics importados de {count} processados."
            );
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Erro ao processar JSON: {e.Message}");
        }
    }

    private static Mugic CreateMugicFromData(MugicJsonData data, int id)
    {
        Mugic mugic = ScriptableObject.CreateInstance<Mugic>();

        mugic.cardCode = $"mugic-{id:D3}";
        mugic.cardName = data.name;
        mugic.rarity = ParseRarity(data.rarity);

        mugic.tribe = ParseTribe(data.tribe);
        mugic.cost = ParseIntSafe(data.cost, 0);

        mugic.abilityDescription = data.ability ?? "";
        mugic.notes = data.notes ?? "";
        mugic.shownotes = data.shownotes ?? "";

        mugic.cardType = data.types ?? "";
        mugic.cardFlavorText = data.flavortext ?? "";
        mugic.cardArtist = data.artist ?? "";

        mugic.isUnique = data.unique == "1";
        mugic.tags = data.tags ?? "";
        mugic.exclusive = data.exclusive ?? "";
        mugic.altVersion = data.alt ?? "";
        mugic.isAnimated = data.animated == "1";

        mugic.cardSet = data.set ?? "";
        mugic.cardId = data.id ?? "";

        string imagePath = GetMugicImagePath(data);
        Sprite mugicSprite = LoadMugicImage(imagePath);
        mugic.artworkImage = mugicSprite;
        mugic.cardPortrait = mugicSprite;

        return mugic;
    }

    private static int ParseIntSafe(string value, int defaultValue)
    {
        if (string.IsNullOrEmpty(value))
            return defaultValue;
        if (int.TryParse(value, out int result))
            return result;
        return defaultValue;
    }

    private static Tribe ParseTribe(string tribe)
    {
        switch (tribe?.ToLower())
        {
            case "overworld":
                return Tribe.Overworld;
            case "underworld":
                return Tribe.Underworld;
            case "mipedian":
                return Tribe.Mipedian;
            case "danian":
                return Tribe.Danian;
            case "marillian":
                return Tribe.Marillian;
            default:
                return Tribe.Generic;
        }
    }

    private static Rarity ParseRarity(string rarity)
    {
        switch (rarity?.ToLower())
        {
            case "common":
                return Rarity.Common;
            case "uncommon":
                return Rarity.Uncommon;
            case "rare":
                return Rarity.Rare;
            case "super rare":
                return Rarity.SuperRare;
            case "ultra rare":
                return Rarity.UltraRare;
            case "promo":
                return Rarity.Promo;
            default:
                return Rarity.Common;
        }
    }

    private static string GetMugicImagePath(MugicJsonData data)
    {
        string basePath = "Assets/_Project/Art/Cards/";
        string name = data.name;
        string set = data.set;
        string rarity = data.rarity;
        string tribe = data.tribe;
        string fullPath = Path.Combine(basePath, "Mugic", set, tribe, rarity, $"{name} - ic.png");
        return fullPath;
    }

    private static Sprite LoadMugicImage(string path)
    {
        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);

        if (sprite != null)
        {
            Debug.Log($"Sprite encontrado: {path}");
            return sprite;
        }

        Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);

        if (tex != null)
        {
            ConfigureTextureAsSprite(path);
            AssetDatabase.Refresh();
            sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);

            if (sprite != null)
            {
                Debug.Log($"Texture2D configurada como Sprite: {path}");
                return sprite;
            }
        }

        Debug.LogWarning($"Imagem não encontrada: {path}");
        return null;
    }

    private static void ConfigureTextureAsSprite(string texturePath)
    {
        TextureImporter textureImporter = AssetImporter.GetAtPath(texturePath) as TextureImporter;

        if (textureImporter != null)
        {
            textureImporter.textureType = TextureImporterType.Sprite;
            textureImporter.spriteImportMode = SpriteImportMode.Single;
            textureImporter.alphaSource = TextureImporterAlphaSource.FromInput;
            textureImporter.alphaIsTransparency = true;
            textureImporter.mipmapEnabled = false;
            textureImporter.filterMode = FilterMode.Bilinear;
            textureImporter.wrapMode = TextureWrapMode.Clamp;
            textureImporter.textureCompression = TextureImporterCompression.Compressed;

            textureImporter.SaveAndReimport();
            Debug.Log($"Textura configurada como Sprite: {texturePath}");
        }
        else
        {
            Debug.LogWarning($"Não foi possível obter TextureImporter para: {texturePath}");
        }
    }

    private static string SanitizeFileName(string name)
    {
        if (string.IsNullOrEmpty(name))
            return "Unknown_Mugic";

        string sanitized = name;
        foreach (char c in Path.GetInvalidFileNameChars())
        {
            sanitized = sanitized.Replace(c, '_');
        }

        return sanitized;
    }

    [System.Serializable]
    public class MugicJsonData
    {
        public string type;
        public string name;
        public string set;
        public string rarity;
        public string id;
        public string tribe;
        public string cost;
        public string types;
        public string ability;
        public string flavortext;
        public string unique;
        public string artist;
        public string tags;
        public string exclusive;
        public string alt;
        public string animated;
        public string notes;
        public string shownotes;
        public string ic;
    }

    public static class JsonUtilityWrapper
    {
        public static List<T> FromJsonArray<T>(string json)
        {
            try
            {
                string wrappedJson = "{\"items\":" + json + "}";
                Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(wrappedJson);
                return wrapper.items ?? new List<T>();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Erro ao parsear JSON: {e.Message}");
                return new List<T>();
            }
        }

        [System.Serializable]
        private class Wrapper<T>
        {
            public List<T> items;
        }
    }
}
