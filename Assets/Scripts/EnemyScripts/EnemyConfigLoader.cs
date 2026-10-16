using System;
using UnityEngine;
using System.Collections.Generic;

public class EnemySprites
{
    public Sprite Idle;
    public Sprite WalkA;
    public Sprite WalkB;
    public Sprite Attack;
}

public class EnemyDefinition
{
    public EnemySprites Sprite;
    public Dictionary<string, EnemySprites> Variants = new Dictionary<string, EnemySprites>();
}

public static class EnemyConfigLoader
{
    public static Dictionary<string, EnemyDefinition> Load(string path = "Data/EnemyConfig")
    {
        TextAsset file = Resources.Load<TextAsset>(path);
        if (file == null)
            throw new InvalidOperationException($"Missing Resources/{path}.json");

        ConfigData config = JsonUtility.FromJson<ConfigData>(file.text);
        if (config == null || config.Enemies == null || config.TileSize <= 0 || config.Columns <= 0 || config.Rows <= 0 || config.PixelsPerUnit <= 0)
            throw new InvalidOperationException("Invalid enemy sprite configuration.");

        Texture2D texture = Resources.Load<Texture2D>(config.TexturePath);
        if (texture == null)
            throw new InvalidOperationException($"Missing texture at Resources/{config.TexturePath}");

        int width = config.Columns * config.TileSize;
        int height = config.Rows * config.TileSize;
        if (texture.width != width || texture.height != height)
            throw new InvalidOperationException($"Expected a {width}x{height} texture, got {texture.width}x{texture.height}. Check Max Size and platform overrides. Use Sprite texture type, or disable Non Power of 2 scaling.");

        Dictionary<string, EnemyDefinition> result = new Dictionary<string, EnemyDefinition>();
        Dictionary<int, Sprite> sprites = new Dictionary<int, Sprite>();

        foreach (EnemyData enemy in config.Enemies)
        {
            if (enemy == null || string.IsNullOrEmpty(enemy.Name) || enemy.Variants == null)
                throw new InvalidOperationException("Each enemy needs a Name and Variants array.");

            EnemyDefinition definition = new EnemyDefinition();
            foreach (VariantData variant in enemy.Variants)
            {
                if (variant == null || string.IsNullOrEmpty(variant.Name) || variant.Sprite == null)
                    throw new InvalidOperationException($"Invalid variant for {enemy.Name}.");

                SpriteIndices frames = variant.Sprite;
                definition.Variants.Add(variant.Name, new EnemySprites
                {
                    Idle = GetSprite(frames.Idle, texture, config, sprites),
                    WalkA = GetSprite(frames.WalkA, texture, config, sprites),
                    WalkB = GetSprite(frames.WalkB, texture, config, sprites),
                    Attack = GetSprite(frames.Attack, texture, config, sprites)
                });
            }

            if (string.IsNullOrEmpty(enemy.DefaultVariant) || !definition.Variants.TryGetValue(enemy.DefaultVariant, out definition.Sprite))
                throw new InvalidOperationException($"Missing default variant for {enemy.Name}.");

            result.Add(enemy.Name, definition);
        }

        return result;
    }

    private static Sprite GetSprite(int index, Texture2D texture, ConfigData config, Dictionary<int, Sprite> sprites)
    {
        if (index < 0 || index >= config.Columns * config.Rows)
            throw new InvalidOperationException($"Sprite tile index {index} is outside the sheet.");

        if (sprites.TryGetValue(index, out Sprite existing)) return existing;

        int x = (index % config.Columns) * config.TileSize;
        int y = texture.height - ((index / config.Columns) + 1) * config.TileSize;
        Rect rect = new Rect(x, y, config.TileSize, config.TileSize);

        Sprite sprite = Sprite.Create(texture, rect, new Vector2(0.5f, 0.5f), config.PixelsPerUnit, 0, SpriteMeshType.FullRect);
        sprite.name = $"EnemyTile_{index}";
        sprites.Add(index, sprite);
        return sprite;
    }

    public static void Release(Dictionary<string, EnemyDefinition> config)
    {
        if (config == null) return;
        HashSet<Sprite> sprites = new HashSet<Sprite>();
        foreach (EnemyDefinition enemy in config.Values)
            foreach (EnemySprites variant in enemy.Variants.Values)
            {
                sprites.Add(variant.Idle);
                sprites.Add(variant.WalkA);
                sprites.Add(variant.WalkB);
                sprites.Add(variant.Attack);
            }

        foreach (Sprite sprite in sprites)
            if (sprite != null) UnityEngine.Object.Destroy(sprite);
        config.Clear();
    }

    [Serializable]
    private class ConfigData
    {
        public string TexturePath;
        public int TileSize;
        public int Columns;
        public int Rows;
        public float PixelsPerUnit;
        public EnemyData[] Enemies;
    }

    [Serializable]
    private class EnemyData
    {
        public string Name;
        public string DefaultVariant;
        public VariantData[] Variants;
    }

    [Serializable]
    private class VariantData
    {
        public string Name;
        public SpriteIndices Sprite;
    }

    [Serializable]
    private class SpriteIndices
    {
        public int Idle;
        public int WalkA;
        public int WalkB;
        public int Attack;
    }
}