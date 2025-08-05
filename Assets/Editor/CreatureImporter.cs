using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class CreatureImporter : EditorWindow
{
    private const string jsonPath = "Assets/Editor/chaotic_creatures.json";
    private const string savePath = "Assets/Resources/Data/Creatures";

    private static int startingId = 101;
    private static int creaturesPerBatch = 12;

    [MenuItem("Chaotic Tools/Import Creatures")]
    public static void ShowWindow()
    {
        GetWindow<CreatureImporter>("Creature Importer");
    }

    void OnGUI()
    {
        GUILayout.Label("Creature Importer", EditorStyles.boldLabel);

        startingId = EditorGUILayout.IntField("Starting ID:", startingId);
        creaturesPerBatch = EditorGUILayout.IntField("Creatures per Batch:", creaturesPerBatch);

        if (GUILayout.Button("Import Creatures"))
        {
            ImportCreatures();
        }

        if (GUILayout.Button("Import ALL Creatures"))
        {
            ImportAllCreatures();
        }
    }

    public static void ImportCreatures()
    {
        ImportCreaturesInternal(creaturesPerBatch);
    }

    public static void ImportAllCreatures()
    {
        ImportCreaturesInternal(-1);
    }

    private static void ImportCreaturesInternal(int maxCount)
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
            List<CreatureJsonData> creatures = JsonUtilityWrapper.FromJsonArray<CreatureJsonData>(
                jsonText
            );

            if (creatures == null || creatures.Count == 0)
            {
                Debug.LogError("Nenhuma criatura encontrada no JSON!");
                return;
            }

            int count = 0;
            int imported = 0;

            foreach (var data in creatures)
            {
                if (maxCount > 0 && count >= maxCount)
                    break;

                if (string.IsNullOrEmpty(data.name))
                {
                    Debug.LogWarning($"Criatura {count} sem nome, pulando...");
                    count++;
                    continue;
                }

                try
                {
                    Creature creature = CreateCreatureFromData(data, startingId + count);

                    string fileName = SanitizeFileName(creature.cardName);
                    string assetPath = Path.Combine(savePath, $"{creature.cardCode}.asset");

                    if (AssetDatabase.LoadAssetAtPath<Creature>(assetPath) != null)
                    {
                        Debug.LogWarning(
                            $"Criatura {creature.cardName} já existe, sobrescrevendo..."
                        );
                    }

                    AssetDatabase.CreateAsset(creature, assetPath);
                    imported++;

                    Debug.Log($"Criatura importada: {creature.cardName} ({creature.cardCode})");
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Erro ao importar criatura {data.name}: {e.Message}");
                }

                count++;
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log(
                $"Importação finalizada! {imported} criaturas importadas de {count} processadas."
            );
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Erro ao processar JSON: {e.Message}");
        }
    }

    private static Creature CreateCreatureFromData(CreatureJsonData data, int id)
    {
        Creature creature = ScriptableObject.CreateInstance<Creature>();

        creature.cardCode = $"creature-{id:D3}";
        creature.cardName = data.name;
        creature.tribe = ParseTribe(data.tribe);
        creature.rarity = ParseRarity(data.rarity);

        creature.courage = ParseIntSafe(data.courage, 0);
        creature.power = ParseIntSafe(data.power, 0);
        creature.wisdom = ParseIntSafe(data.wisdom, 0);
        creature.speed = ParseIntSafe(data.speed, 0);
        creature.energy = ParseIntSafe(data.energy, 0);
        creature.mugicCounters = ParseIntSafe(data.mugicability, 0);

        creature.elements = ParseElements(data.elements);

        creature.abilityDescription = data.ability ?? "";
        creature.cardType = data.types ?? "";
        creature.cardFlavorText = data.flavortext ?? "";
        creature.flavorText = data.flavortext ?? "";
        creature.cardArtist = data.artist ?? "";

        creature.isBrainwashed = !string.IsNullOrEmpty(data.brainwashed);
        creature.brainwashedEffect = data.brainwashed ?? "";

        creature.abilityTrigger = AbilityTriggerType.Passive;

        creature.cardSet = data.set ?? "";
        creature.cardId = data.id ?? "";

        creature.tribeColor = GetTribeColor(creature.tribe);

        string imagePath = GetCreatureImagePath(data);
        Sprite creatureSprite = LoadCreatureImage(imagePath);

        creature.artworkImage = creatureSprite;
        creature.creaturePortrait = creatureSprite;

        string artworkPath = GetCreatureImagePath(data, "artwork");
        string portraitPath = GetCreatureImagePath(data, "portrait");

        creature.artworkImage = LoadCreatureImage(artworkPath);
        creature.creaturePortrait = LoadCreatureImage(portraitPath);

        return creature;
    }

    private static int ParseIntSafe(string value, int defaultValue)
    {
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

    private static ElementType[] ParseElements(string elements)
    {
        if (string.IsNullOrEmpty(elements))
            return new ElementType[0];

        return elements
            .Split(',')
            .Select(e => e.Trim())
            .Select(ParseElement)
            .Where(e => e != ElementType.None)
            .ToArray();
    }

    private static ElementType ParseElement(string element)
    {
        switch (element?.ToLower())
        {
            case "fire":
                return ElementType.Fire;
            case "air":
                return ElementType.Air;
            case "earth":
                return ElementType.Earth;
            case "water":
                return ElementType.Water;
            default:
                return ElementType.None;
        }
    }

    private static Color GetTribeColor(Tribe tribe)
    {
        switch (tribe)
        {
            case Tribe.Overworld:
                return Color.blue;
            case Tribe.Underworld:
                return Color.red;
            case Tribe.Mipedian:
                return Color.yellow;
            case Tribe.Danian:
                return Color.green;
            case Tribe.Marillian:
                return Color.cyan;
            default:
                return Color.white;
        }
    }

    private static string SanitizeFileName(string name)
    {
        if (string.IsNullOrEmpty(name))
            return "Unknown_Creature";

        string sanitized = name;
        foreach (char c in Path.GetInvalidFileNameChars())
        {
            sanitized = sanitized.Replace(c, '_');
        }

        sanitized = sanitized.Replace(' ', '_');
        sanitized = sanitized.Replace("'", "");
        sanitized = sanitized.Replace("-", "_");

        return sanitized;
    }

    [System.Serializable]
    public class CreatureJsonData
    {
        public string type;
        public string name;
        public string set;
        public string rarity;
        public string id;
        public string tribe;
        public string courage;
        public string power;
        public string wisdom;
        public string speed;
        public string energy;
        public string mugicability;
        public string elements;
        public string types;
        public string ability;
        public string flavortext;
        public string brainwashed;
        public string artist;
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

    private static string GetCreatureImagePath(CreatureJsonData data, string imageType = "artwork")
    {
        string basePath = "Assets/_Project/Art/Cards/";

        string set = SanitizeFolderName(data.set);
        string tribe = SanitizeFolderName(data.tribe);
        string rarity = data.rarity;
        string name = data.name;

        string fullPath = Path.Combine(
            basePath,
            "Creatures",
            set,
            tribe,
            rarity,
            $"{name} - ic.png"
        );
        return fullPath;
    }

    private static Sprite LoadCreatureImage(string path)
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

    private static string SanitizeFolderName(string name)
    {
        if (string.IsNullOrEmpty(name))
            return "Unknown";

        return name.Replace(" ", "").Replace("-", "").Replace("'", "").Trim();
    }
}
