using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class AttackImporter : EditorWindow
{
    private const string jsonPath = "Assets/Editor/chaotic_attacks.json";
    private const string savePath = "Assets/Resources/Data/Attacks";

    private static int startingId = 1;
    private static int attacksPerBatch = 12;

    [MenuItem("Chaotic Tools/Import Attacks")]
    public static void ShowWindow()
    {
        GetWindow<AttackImporter>("Attack Importer");
    }

    void OnGUI()
    {
        GUILayout.Label("Attack Importer", EditorStyles.boldLabel);

        startingId = EditorGUILayout.IntField("Starting ID:", startingId);
        attacksPerBatch = EditorGUILayout.IntField("Attacks per Batch:", attacksPerBatch);

        if (GUILayout.Button("Import Attacks"))
        {
            ImportAttacks();
        }

        if (GUILayout.Button("Import ALL Attacks"))
        {
            ImportAllAttacks();
        }
    }

    public static void ImportAttacks()
    {
        ImportAttacksInternal(attacksPerBatch);
    }

    public static void ImportAllAttacks()
    {
        ImportAttacksInternal(-1);
    }

    private static void ImportAttacksInternal(int maxCount)
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
            List<AttackJsonData> attacks = JsonUtilityWrapper.FromJsonArray<AttackJsonData>(
                jsonText
            );

            if (attacks == null || attacks.Count == 0)
            {
                Debug.LogError("Nenhum ataque encontrado no JSON!");
                return;
            }

            int count = 0;
            int imported = 0;

            foreach (var data in attacks)
            {
                if (maxCount > 0 && count >= maxCount)
                    break;

                if (string.IsNullOrEmpty(data.name))
                {
                    Debug.LogWarning($"Ataque {count} sem nome, pulando...");
                    count++;
                    continue;
                }

                try
                {
                    Attack attack = CreateAttackFromData(data, startingId + count);

                    string fileName = SanitizeFileName(attack.cardName);
                    string assetPath = Path.Combine(savePath, $"{attack.cardCode}.asset");

                    if (AssetDatabase.LoadAssetAtPath<Attack>(assetPath) != null)
                    {
                        Debug.LogWarning($"Ataque {attack.cardName} já existe, sobrescrevendo...");
                    }

                    AssetDatabase.CreateAsset(attack, assetPath);
                    imported++;

                    Debug.Log($"Ataque importado: {attack.cardName} ({attack.cardCode})");
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Erro ao importar ataque {data.name}: {e.Message}");
                }

                count++;
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log(
                $"Importação finalizada! {imported} ataques importados de {count} processados."
            );
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Erro ao processar JSON: {e.Message}");
        }
    }

    private static Attack CreateAttackFromData(AttackJsonData data, int id)
    {
        Attack attack = ScriptableObject.CreateInstance<Attack>();

        attack.cardCode = $"attack-{id:D3}";
        attack.cardName = data.name;
        attack.rarity = ParseRarity(data.rarity);

        attack.buildPoints = ParseIntSafe(data.bp, 0);
        attack.baseDamage = ParseIntSafe(data.@base, 0);

        attack.fireDamage = ParseIntSafe(data.fire, 0);
        attack.airDamage = ParseIntSafe(data.air, 0);
        attack.earthDamage = ParseIntSafe(data.earth, 0);
        attack.waterDamage = ParseIntSafe(data.water, 0);

        attack.abilityDescription = data.ability ?? "";
        attack.cardType = data.types ?? "";
        attack.cardFlavorText = data.flavortext ?? "";
        attack.cardArtist = data.artist ?? "";

        attack.isUnique = data.unique == "1";
        attack.tags = data.tags ?? "";
        attack.exclusive = data.exclusive ?? "";

        attack.cardSet = data.set ?? "";
        attack.cardId = data.id ?? "";

        string imagePath = GetAttackImagePath(data);
        Sprite attackSprite = LoadAttackImage(imagePath);
        attack.artworkImage = attackSprite;
        attack.cardPortrait = attackSprite;

        return attack;
    }

    private static int ParseIntSafe(string value, int defaultValue)
    {
        if (string.IsNullOrEmpty(value))
            return defaultValue;
        if (int.TryParse(value, out int result))
            return result;
        return defaultValue;
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

    private static string GetAttackImagePath(AttackJsonData data)
    {
        string basePath = "Assets/_Project/Art/Cards/";
        string set = data.set;
        string rarity = data.rarity;
        string name = data.name;

        string fullPath = Path.Combine(basePath, "Attacks", set, rarity, $"{name} - ic.png");
        return fullPath;
    }

    private static Sprite LoadAttackImage(string path)
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
            return "Unknown_Attack";

        string sanitized = name;
        foreach (char c in Path.GetInvalidFileNameChars())
        {
            sanitized = sanitized.Replace(c, '_');
        }

        return sanitized;
    }

    [System.Serializable]
    public class AttackJsonData
    {
        public string type;
        public string name;
        public string set;
        public string rarity;
        public string id;
        public string bp;
        public string @base;
        public string fire;
        public string air;
        public string earth;
        public string water;
        public string types;
        public string ability;
        public string flavortext;
        public string unique;
        public string artist;
        public string tags;
        public string exclusive;
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
