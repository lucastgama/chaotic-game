using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class LocationImporter : EditorWindow
{
    private const string jsonPath = "Assets/Editor/chaotic_locations.json";
    private const string savePath = "Assets/Resources/Data/Locations";

    private static int startingId = 1;
    private static int locationsPerBatch = 12;

    [MenuItem("Chaotic Tools/Import Locations")]
    public static void ShowWindow()
    {
        GetWindow<LocationImporter>("Location Importer");
    }

    void OnGUI()
    {
        GUILayout.Label("Location Importer", EditorStyles.boldLabel);

        startingId = EditorGUILayout.IntField("Starting ID:", startingId);
        locationsPerBatch = EditorGUILayout.IntField("Locations per Batch:", locationsPerBatch);

        if (GUILayout.Button("Import Locations"))
        {
            ImportLocations();
        }

        if (GUILayout.Button("Import ALL Locations"))
        {
            ImportAllLocations();
        }
    }

    public static void ImportLocations()
    {
        ImportLocationsInternal(locationsPerBatch);
    }

    public static void ImportAllLocations()
    {
        ImportLocationsInternal(-1);
    }

    private static void ImportLocationsInternal(int maxCount)
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
            List<LocationJsonData> locations = JsonUtilityWrapper.FromJsonArray<LocationJsonData>(
                jsonText
            );

            if (locations == null || locations.Count == 0)
            {
                Debug.LogError("Nenhuma localização encontrada no JSON!");
                return;
            }

            int count = 0;
            int imported = 0;

            foreach (var data in locations)
            {
                if (maxCount > 0 && count >= maxCount)
                    break;

                if (string.IsNullOrEmpty(data.name))
                {
                    Debug.LogWarning($"Localização {count} sem nome, pulando...");
                    count++;
                    continue;
                }

                try
                {
                    Location location = CreateLocationFromData(data, startingId + count);

                    string fileName = SanitizeFileName(location.cardName);
                    string assetPath = Path.Combine(savePath, $"{location.cardCode}.asset");

                    if (AssetDatabase.LoadAssetAtPath<Location>(assetPath) != null)
                    {
                        Debug.LogWarning(
                            $"Localização {location.cardName} já existe, sobrescrevendo..."
                        );
                    }

                    AssetDatabase.CreateAsset(location, assetPath);
                    imported++;

                    Debug.Log($"Localização importada: {location.cardName} ({location.cardCode})");
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Erro ao importar localização {data.name}: {e.Message}");
                }

                count++;
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log(
                $"Importação finalizada! {imported} localizações importadas de {count} processadas."
            );
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Erro ao processar JSON: {e.Message}");
        }
    }

    private static Location CreateLocationFromData(LocationJsonData data, int id)
    {
        Location location = ScriptableObject.CreateInstance<Location>();

        location.cardCode = $"location-{id:D3}";
        location.cardName = data.name;
        location.rarity = ParseRarity(data.rarity);

        location.initiative = ParseInitiative(data.initiative);

        location.abilityDescription = data.ability ?? "";
        location.cardType = data.types ?? "";
        location.cardFlavorText = data.flavortext ?? "";
        location.cardArtist = data.artist ?? "";

        location.isUnique = data.unique == "1";
        location.tags = data.tags ?? "";
        location.exclusive = data.exclusive ?? "";

        location.cardSet = data.set ?? "";
        location.cardId = data.id ?? "";

        string imagePath = GetLocationImagePath(data);
        Sprite locationSprite = LoadLocationImage(imagePath);
        location.artworkImage = locationSprite;
        location.cardPortrait = locationSprite;

        return location;
    }

    private static InitiativeType ParseInitiative(string initiative)
    {
        switch (initiative?.ToLower())
        {
            case "power":
                return InitiativeType.Power;
            case "courage":
                return InitiativeType.Courage;
            case "wisdom":
                return InitiativeType.Wisdom;
            case "speed":
                return InitiativeType.Speed;
            default:
                return InitiativeType.None;
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

    private static string GetLocationImagePath(LocationJsonData data)
    {
        string basePath = "Assets/_Project/Art/Cards/";
        string name = data.name;
        string set = data.set;
        string rarity = data.rarity;
        string fullPath = Path.Combine(basePath, "Locations", set, rarity, $"{name} - ic.png");
        return fullPath;
    }

    private static Sprite LoadLocationImage(string path)
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
            return "Unknown_Location";

        string sanitized = name;
        foreach (char c in Path.GetInvalidFileNameChars())
        {
            sanitized = sanitized.Replace(c, '_');
        }

        return sanitized;
    }

    [System.Serializable]
    public class LocationJsonData
    {
        public string type;
        public string name;
        public string set;
        public string rarity;
        public string id;
        public string initiative;
        public string types;
        public string ability;
        public string flavortext;
        public string unique;
        public string artist;
        public string tags;
        public string exclusive;
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
