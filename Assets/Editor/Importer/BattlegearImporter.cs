using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class BattlegearImporter : EditorWindow
{
    private const string jsonPath = "Assets/Editor/chaotic_battlegear.json";
    private const string savePath = "Assets/Resources/Data/Battlegear";

    private static int startingId = 1;
    private static int battlegearPerBatch = 12;

    [MenuItem("Chaotic Tools/Import Battlegear")]
    public static void ShowWindow()
    {
        GetWindow<BattlegearImporter>("Battlegear Importer");
    }

    void OnGUI()
    {
        GUILayout.Label("Battlegear Importer", EditorStyles.boldLabel);

        startingId = EditorGUILayout.IntField("Starting ID:", startingId);
        battlegearPerBatch = EditorGUILayout.IntField("Battlegear per Batch:", battlegearPerBatch);

        if (GUILayout.Button("Import Battlegear"))
        {
            ImportBattlegear();
        }

        if (GUILayout.Button("Import ALL Battlegear"))
        {
            ImportAllBattlegear();
        }
    }

    public static void ImportBattlegear()
    {
        ImportBattlegearInternal(battlegearPerBatch);
    }

    public static void ImportAllBattlegear()
    {
        ImportBattlegearInternal(-1);
    }

    private static void ImportBattlegearInternal(int maxCount)
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
            List<BattlegearJsonData> battlegears =
                JsonUtilityWrapper.FromJsonArray<BattlegearJsonData>(jsonText);

            if (battlegears == null || battlegears.Count == 0)
            {
                Debug.LogError("Nenhum battlegear encontrado no JSON!");
                return;
            }

            int count = 0;
            int imported = 0;

            foreach (var data in battlegears)
            {
                if (maxCount > 0 && count >= maxCount)
                    break;

                if (string.IsNullOrEmpty(data.name))
                {
                    Debug.LogWarning($"Battlegear {count} sem nome, pulando...");
                    count++;
                    continue;
                }

                try
                {
                    Battlegear battlegear = CreateBattlegearFromData(data, startingId + count);

                    string fileName = SanitizeFileName(battlegear.cardName);
                    string assetPath = Path.Combine(savePath, $"{battlegear.cardCode}.asset");

                    if (AssetDatabase.LoadAssetAtPath<Battlegear>(assetPath) != null)
                    {
                        Debug.LogWarning(
                            $"Battlegear {battlegear.cardName} já existe, sobrescrevendo..."
                        );
                    }

                    AssetDatabase.CreateAsset(battlegear, assetPath);
                    imported++;

                    Debug.Log(
                        $"Battlegear importado: {battlegear.cardName} ({battlegear.cardCode})"
                    );
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Erro ao importar battlegear {data.name}: {e.Message}");
                }

                count++;
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log(
                $"Importação finalizada! {imported} battlegears importados de {count} processados."
            );
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Erro ao processar JSON: {e.Message}");
        }
    }

    private static Battlegear CreateBattlegearFromData(BattlegearJsonData data, int id)
    {
        Battlegear battlegear = ScriptableObject.CreateInstance<Battlegear>();

        battlegear.cardCode = $"battlegear-{id:D3}";
        battlegear.cardName = data.name;
        battlegear.rarity = ParseRarity(data.rarity);

        battlegear.abilityDescription = data.ability ?? "";

        battlegear.cardType = data.types ?? "";
        battlegear.cardFlavorText = data.flavortext ?? "";
        battlegear.cardArtist = data.artist ?? "";

        battlegear.isUnique = data.unique == "1";
        battlegear.isLoyal = data.loyal == "1";
        battlegear.isLegendary = data.legendary == "1";
        battlegear.tags = data.tags ?? "";
        battlegear.exclusive = data.exclusive ?? "";

        battlegear.cardSet = data.set ?? "";
        battlegear.cardId = data.id ?? "";

        string imagePath = GetBattlegearImagePath(data);
        Sprite battlegearSprite = LoadBattlegearImage(imagePath);
        battlegear.artworkImage = battlegearSprite;
        battlegear.cardPortrait = battlegearSprite;

        return battlegear;
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

    private static string GetBattlegearImagePath(BattlegearJsonData data)
    {
        string basePath = "Assets/_Project/Art/Cards/";
        string name = data.name;
        string set = data.set;
        string rarity = data.rarity;

        string fullPath = Path.Combine(basePath, "Battlegear", set, rarity, $"{name} - ic.png");
        return fullPath;
    }

    private static Sprite LoadBattlegearImage(string path)
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
            return "Unknown_Battlegear";

        string sanitized = name;
        foreach (char c in Path.GetInvalidFileNameChars())
        {
            sanitized = sanitized.Replace(c, '_');
        }

        return sanitized;
    }

    [System.Serializable]
    public class BattlegearJsonData
    {
        public string type;
        public string name;
        public string set;
        public string rarity;
        public string id;
        public string types;
        public string ability;
        public string flavortext;
        public string unique;
        public string loyal;
        public string legendary;
        public string artist;
        public string tags;
        public string exclusive;
        public string alt;
        public string animated;
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
