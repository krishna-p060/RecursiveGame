using UnityEngine;

/// <summary>
/// Builds detailed pixel-style composite visuals for monsters and weapons at runtime.
/// Uses three procedurally generated primitive sprites (circle, square, triangle) and
/// layers many small child SpriteRenderers to form a recognisable silhouette.
/// No external art assets are required.
/// </summary>
public static class ArtBuilder
{
    // ─── Cached runtime-generated primitive sprites ────────────────────────
    private static Sprite _triangle;
    private static Sprite _circle;
    private static Sprite _square;

    public static Sprite GetTriangle()
    {
        if (_triangle != null) return _triangle;
        int size = 32;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
        Color t = new Color(0, 0, 0, 0);
        for (int y = 0; y < size; y++)
        {
            float halfWidth = (size - 1 - y) * 0.5f;
            for (int x = 0; x < size; x++)
                tex.SetPixel(x, y, (x >= halfWidth && x <= size - 1 - halfWidth) ? Color.white : t);
        }
        tex.Apply();
        _triangle = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        _triangle.name = "Triangle_Runtime";
        return _triangle;
    }

    public static Sprite GetCircle()
    {
        if (_circle != null) return _circle;
        int size = 32;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
        Color t = new Color(0, 0, 0, 0);
        float cx = (size - 1) / 2f, cy = (size - 1) / 2f;
        float r = size / 2f - 0.5f;
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float dx = x - cx, dy = y - cy;
                tex.SetPixel(x, y, (dx * dx + dy * dy <= r * r) ? Color.white : t);
            }
        tex.Apply();
        _circle = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        _circle.name = "Circle_Runtime";
        return _circle;
    }

    public static Sprite GetSquare()
    {
        if (_square != null) return _square;
        int size = 8;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
                tex.SetPixel(x, y, Color.white);
        tex.Apply();
        _square = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        _square.name = "Square_Runtime";
        return _square;
    }

    // ─── Part composition helper ──────────────────────────────────────────
    public static GameObject AddPart(Transform parent, string name, Sprite sprite,
        Vector2 localPos, float rotationZ, Vector2 localScale, Color color, int sortOrder)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.localPosition = new Vector3(localPos.x, localPos.y, 0);
        go.transform.localRotation = Quaternion.Euler(0, 0, rotationZ);
        go.transform.localScale = new Vector3(localScale.x, localScale.y, 1);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.color = color;
        sr.sortingOrder = sortOrder;
        return go;
    }

    static Color Darker(Color c, float amount = 0.5f) =>
        new Color(c.r * amount, c.g * amount, c.b * amount, c.a);

    static Color Lighter(Color c, float amount = 0.2f) =>
        new Color(Mathf.Min(1, c.r + amount), Mathf.Min(1, c.g + amount), Mathf.Min(1, c.b + amount), c.a);

    // ═══════════════════════════════════════════════════════════════════════
    //                           MONSTER BUILDERS
    // ═══════════════════════════════════════════════════════════════════════
    public static void BuildMonsterArt(Transform parent, string monsterName, Color baseColor)
    {
        switch (monsterName)
        {
            case "Dragon": BuildDragon(parent, baseColor); break;
            case "Wolf":   BuildWolf(parent, baseColor);   break;
            case "Bat":    BuildBat(parent, baseColor);    break;
        }
    }

    static void BuildDragon(Transform p, Color c)
    {
        Sprite circle = GetCircle(), tri = GetTriangle(), sq = GetSquare();
        Color dark = Darker(c, 0.55f);
        Color darker = Darker(c, 0.3f);
        Color horn = new Color(1f, 0.85f, 0.3f);
        Color hornDark = new Color(0.75f, 0.55f, 0.1f);
        Color fang = new Color(1f, 0.95f, 0.85f);
        Color eyeWhite = new Color(1f, 0.95f, 0.75f);
        Color pupil = new Color(1f, 0.15f, 0.15f);

        // Wings behind body (darker red ellipses)
        AddPart(p, "WingL", circle, new Vector2(-0.55f, 0.08f), 25f, new Vector2(0.4f, 0.6f), dark, 3);
        AddPart(p, "WingR", circle, new Vector2(0.55f, 0.08f), -25f, new Vector2(0.4f, 0.6f), dark, 3);
        AddPart(p, "WingInnerL", circle, new Vector2(-0.55f, 0.08f), 25f, new Vector2(0.25f, 0.4f), darker, 4);
        AddPart(p, "WingInnerR", circle, new Vector2(0.55f, 0.08f), -25f, new Vector2(0.25f, 0.4f), darker, 4);

        // Horns (yellow triangles on top, with dark base ring)
        AddPart(p, "HornBaseL", circle, new Vector2(-0.22f, 0.28f), 0, new Vector2(0.12f, 0.08f), hornDark, 6);
        AddPart(p, "HornBaseR", circle, new Vector2(0.22f, 0.28f), 0, new Vector2(0.12f, 0.08f), hornDark, 6);
        AddPart(p, "HornL", tri, new Vector2(-0.22f, 0.42f), -10f, new Vector2(0.18f, 0.38f), horn, 7);
        AddPart(p, "HornR", tri, new Vector2(0.22f, 0.42f), 10f, new Vector2(0.18f, 0.38f), horn, 7);

        // Snout
        AddPart(p, "Snout", circle, new Vector2(0f, -0.22f), 0, new Vector2(0.5f, 0.3f), Lighter(c, 0.08f), 6);
        AddPart(p, "SnoutTop", circle, new Vector2(0f, -0.12f), 0, new Vector2(0.35f, 0.12f), Darker(c, 0.85f), 6);

        // Eyes
        AddPart(p, "EyeWhiteL", circle, new Vector2(-0.2f, 0.06f), 0, new Vector2(0.22f, 0.22f), eyeWhite, 7);
        AddPart(p, "EyeWhiteR", circle, new Vector2(0.2f, 0.06f), 0, new Vector2(0.22f, 0.22f), eyeWhite, 7);
        AddPart(p, "PupilL", circle, new Vector2(-0.2f, 0.06f), 0, new Vector2(0.1f, 0.16f), pupil, 8);
        AddPart(p, "PupilR", circle, new Vector2(0.2f, 0.06f), 0, new Vector2(0.1f, 0.16f), pupil, 8);
        AddPart(p, "EyeGlintL", circle, new Vector2(-0.18f, 0.1f), 0, new Vector2(0.05f, 0.05f), Color.white, 9);
        AddPart(p, "EyeGlintR", circle, new Vector2(0.22f, 0.1f), 0, new Vector2(0.05f, 0.05f), Color.white, 9);

        // Nostrils
        AddPart(p, "NostrilL", circle, new Vector2(-0.08f, -0.2f), 0, new Vector2(0.07f, 0.06f), darker, 7);
        AddPart(p, "NostrilR", circle, new Vector2(0.08f, -0.2f), 0, new Vector2(0.07f, 0.06f), darker, 7);

        // Angry brow
        AddPart(p, "BrowL", sq, new Vector2(-0.2f, 0.22f), 18f, new Vector2(0.28f, 0.07f), darker, 8);
        AddPart(p, "BrowR", sq, new Vector2(0.2f, 0.22f), -18f, new Vector2(0.28f, 0.07f), darker, 8);

        // Fangs peeking from snout
        AddPart(p, "FangL", tri, new Vector2(-0.1f, -0.33f), 180f, new Vector2(0.09f, 0.13f), fang, 9);
        AddPart(p, "FangR", tri, new Vector2(0.1f, -0.33f), 180f, new Vector2(0.09f, 0.13f), fang, 9);

        // Back spikes
        AddPart(p, "SpikeTop", tri, new Vector2(0f, 0.55f), 0, new Vector2(0.12f, 0.2f), horn, 2);
        AddPart(p, "SpikeL", tri, new Vector2(-0.35f, 0.35f), -35f, new Vector2(0.1f, 0.18f), horn, 2);
        AddPart(p, "SpikeR", tri, new Vector2(0.35f, 0.35f), 35f, new Vector2(0.1f, 0.18f), horn, 2);
    }

    static void BuildWolf(Transform p, Color c)
    {
        Sprite circle = GetCircle(), tri = GetTriangle(), sq = GetSquare();
        Color dark = Darker(c, 0.6f);
        Color darker = Darker(c, 0.35f);
        Color muzzle = Lighter(c, 0.25f);
        Color eye = new Color(1f, 0.85f, 0.2f);
        Color fang = new Color(1f, 0.98f, 0.9f);
        Color black = new Color(0.05f, 0.05f, 0.08f);

        // Ears
        AddPart(p, "EarL", tri, new Vector2(-0.3f, 0.42f), -8f, new Vector2(0.28f, 0.4f), c, 4);
        AddPart(p, "EarR", tri, new Vector2(0.3f, 0.42f), 8f, new Vector2(0.28f, 0.4f), c, 4);
        AddPart(p, "EarInL", tri, new Vector2(-0.3f, 0.38f), -8f, new Vector2(0.13f, 0.22f), darker, 5);
        AddPart(p, "EarInR", tri, new Vector2(0.3f, 0.38f), 8f, new Vector2(0.13f, 0.22f), darker, 5);

        // Fur tufts on sides of head
        AddPart(p, "CheekL", circle, new Vector2(-0.32f, -0.08f), 0, new Vector2(0.28f, 0.28f), dark, 6);
        AddPart(p, "CheekR", circle, new Vector2(0.32f, -0.08f), 0, new Vector2(0.28f, 0.28f), dark, 6);

        // Muzzle
        AddPart(p, "Muzzle", circle, new Vector2(0f, -0.18f), 0, new Vector2(0.45f, 0.3f), muzzle, 7);
        AddPart(p, "Nose", tri, new Vector2(0f, -0.1f), 180f, new Vector2(0.14f, 0.11f), black, 8);

        // Eyes (fierce yellow with slit pupils)
        AddPart(p, "EyeL", circle, new Vector2(-0.18f, 0.08f), 0, new Vector2(0.2f, 0.22f), eye, 7);
        AddPart(p, "EyeR", circle, new Vector2(0.18f, 0.08f), 0, new Vector2(0.2f, 0.22f), eye, 7);
        AddPart(p, "PupilL", sq, new Vector2(-0.18f, 0.08f), 0, new Vector2(0.06f, 0.18f), black, 8);
        AddPart(p, "PupilR", sq, new Vector2(0.18f, 0.08f), 0, new Vector2(0.06f, 0.18f), black, 8);
        AddPart(p, "GlintL", circle, new Vector2(-0.16f, 0.12f), 0, new Vector2(0.04f, 0.04f), Color.white, 9);
        AddPart(p, "GlintR", circle, new Vector2(0.2f, 0.12f), 0, new Vector2(0.04f, 0.04f), Color.white, 9);

        // Angry brow
        AddPart(p, "BrowL", sq, new Vector2(-0.18f, 0.22f), 18f, new Vector2(0.25f, 0.06f), darker, 8);
        AddPart(p, "BrowR", sq, new Vector2(0.18f, 0.22f), -18f, new Vector2(0.25f, 0.06f), darker, 8);

        // Fangs below muzzle
        AddPart(p, "FangL", tri, new Vector2(-0.09f, -0.32f), 180f, new Vector2(0.09f, 0.13f), fang, 9);
        AddPart(p, "FangR", tri, new Vector2(0.09f, -0.32f), 180f, new Vector2(0.09f, 0.13f), fang, 9);
    }

    static void BuildBat(Transform p, Color c)
    {
        Sprite circle = GetCircle(), tri = GetTriangle(), sq = GetSquare();
        Color wing = Darker(c, 0.55f);
        Color wingDark = Darker(c, 0.3f);
        Color eye = new Color(1f, 0.2f, 0.25f);
        Color fang = new Color(1f, 0.98f, 0.9f);

        // Jagged bat wings (multiple triangles forming membrane)
        AddPart(p, "WingL_A", tri, new Vector2(-0.55f, 0.18f), 95f,  new Vector2(0.32f, 0.55f), wing, 3);
        AddPart(p, "WingL_B", tri, new Vector2(-0.55f, -0.08f), 115f, new Vector2(0.25f, 0.5f), wing, 3);
        AddPart(p, "WingL_C", tri, new Vector2(-0.4f, -0.2f), 135f,  new Vector2(0.18f, 0.4f), wing, 3);
        AddPart(p, "WingR_A", tri, new Vector2(0.55f, 0.18f), -95f, new Vector2(0.32f, 0.55f), wing, 3);
        AddPart(p, "WingR_B", tri, new Vector2(0.55f, -0.08f), -115f, new Vector2(0.25f, 0.5f), wing, 3);
        AddPart(p, "WingR_C", tri, new Vector2(0.4f, -0.2f), -135f,  new Vector2(0.18f, 0.4f), wing, 3);

        // Wing bones (thin dark lines)
        AddPart(p, "BoneL", sq, new Vector2(-0.4f, 0.05f), 105f, new Vector2(0.03f, 0.45f), wingDark, 4);
        AddPart(p, "BoneR", sq, new Vector2(0.4f, 0.05f), -105f, new Vector2(0.03f, 0.45f), wingDark, 4);

        // Pointy ears
        AddPart(p, "EarL", tri, new Vector2(-0.18f, 0.42f), -5f, new Vector2(0.16f, 0.35f), c, 6);
        AddPart(p, "EarR", tri, new Vector2(0.18f, 0.42f), 5f, new Vector2(0.16f, 0.35f), c, 6);
        AddPart(p, "EarInL", tri, new Vector2(-0.18f, 0.38f), -5f, new Vector2(0.07f, 0.18f), Darker(c, 0.4f), 7);
        AddPart(p, "EarInR", tri, new Vector2(0.18f, 0.38f), 5f, new Vector2(0.07f, 0.18f), Darker(c, 0.4f), 7);

        // Glowing red eyes with glints
        AddPart(p, "EyeL", circle, new Vector2(-0.16f, 0.08f), 0, new Vector2(0.17f, 0.2f), eye, 7);
        AddPart(p, "EyeR", circle, new Vector2(0.16f, 0.08f), 0, new Vector2(0.17f, 0.2f), eye, 7);
        AddPart(p, "EyeGlowL", circle, new Vector2(-0.16f, 0.08f), 0, new Vector2(0.28f, 0.28f), new Color(eye.r, eye.g, eye.b, 0.35f), 6);
        AddPart(p, "EyeGlowR", circle, new Vector2(0.16f, 0.08f), 0, new Vector2(0.28f, 0.28f), new Color(eye.r, eye.g, eye.b, 0.35f), 6);
        AddPart(p, "GlintL", circle, new Vector2(-0.14f, 0.12f), 0, new Vector2(0.05f, 0.05f), Color.white, 9);
        AddPart(p, "GlintR", circle, new Vector2(0.18f, 0.12f), 0, new Vector2(0.05f, 0.05f), Color.white, 9);

        // Vampire fangs
        AddPart(p, "FangL", tri, new Vector2(-0.08f, -0.22f), 180f, new Vector2(0.08f, 0.13f), fang, 9);
        AddPart(p, "FangR", tri, new Vector2(0.08f, -0.22f), 180f, new Vector2(0.08f, 0.13f), fang, 9);
    }

    // ═══════════════════════════════════════════════════════════════════════
    //                           HERO / PLAYER BUILDER
    // ═══════════════════════════════════════════════════════════════════════
    public static void BuildHeroArt(Transform p, Color bodyColor)
    {
        Sprite circle = GetCircle(), tri = GetTriangle(), sq = GetSquare();
        Color skin      = new Color(1f, 0.82f, 0.65f);
        Color skinShade = new Color(0.75f, 0.55f, 0.4f);
        Color hair      = new Color(0.45f, 0.28f, 0.1f);
        Color helmet    = new Color(0.78f, 0.82f, 0.9f);
        Color helmetDark= new Color(0.45f, 0.5f, 0.6f);
        Color plume     = new Color(1f, 0.2f, 0.3f);
        Color armorDark = Darker(bodyColor, 0.55f);
        Color boot      = new Color(0.35f, 0.2f, 0.1f);

        // Torso / armor plate (replaces plain circle look)
        AddPart(p, "ArmorBack", circle, new Vector2(0f, -0.05f), 0, new Vector2(1.05f, 1.05f), armorDark, 4);
        AddPart(p, "ArmorPlate", circle, new Vector2(0f, -0.1f), 0, new Vector2(0.85f, 0.9f), bodyColor, 5);
        AddPart(p, "ArmorShine", sq, new Vector2(-0.18f, 0.05f), 10f, new Vector2(0.06f, 0.35f), Lighter(bodyColor, 0.35f), 6);
        AddPart(p, "BeltBuckle", sq, new Vector2(0f, -0.32f), 0, new Vector2(0.3f, 0.08f), new Color(1f, 0.8f, 0.2f), 7);

        // Head
        AddPart(p, "Head", circle, new Vector2(0f, 0.28f), 0, new Vector2(0.45f, 0.45f), skin, 7);
        AddPart(p, "HeadShadow", circle, new Vector2(0.05f, 0.22f), 0, new Vector2(0.38f, 0.32f), skinShade, 6);

        // Helmet
        AddPart(p, "Helmet", circle, new Vector2(0f, 0.38f), 0, new Vector2(0.5f, 0.38f), helmet, 8);
        AddPart(p, "HelmetBand", sq, new Vector2(0f, 0.3f), 0, new Vector2(0.5f, 0.05f), helmetDark, 9);
        AddPart(p, "HelmetTop", tri, new Vector2(0f, 0.55f), 0, new Vector2(0.22f, 0.15f), helmet, 8);
        AddPart(p, "Plume", tri, new Vector2(0f, 0.68f), 0, new Vector2(0.18f, 0.25f), plume, 9);

        // Hair peeking out
        AddPart(p, "HairL", sq, new Vector2(-0.18f, 0.28f), 0, new Vector2(0.06f, 0.1f), hair, 7);
        AddPart(p, "HairR", sq, new Vector2(0.18f, 0.28f), 0, new Vector2(0.06f, 0.1f), hair, 7);

        // Eyes
        AddPart(p, "EyeL", sq, new Vector2(-0.1f, 0.28f), 0, new Vector2(0.05f, 0.07f), Color.black, 9);
        AddPart(p, "EyeR", sq, new Vector2(0.1f, 0.28f), 0, new Vector2(0.05f, 0.07f), Color.black, 9);

        // Shoulders (pauldrons)
        AddPart(p, "ShoulderL", circle, new Vector2(-0.38f, 0.05f), 0, new Vector2(0.28f, 0.28f), armorDark, 7);
        AddPart(p, "ShoulderR", circle, new Vector2(0.38f, 0.05f), 0, new Vector2(0.28f, 0.28f), armorDark, 7);
        AddPart(p, "ShoulderShineL", circle, new Vector2(-0.4f, 0.1f), 0, new Vector2(0.14f, 0.14f), Lighter(bodyColor, 0.4f), 8);
        AddPart(p, "ShoulderShineR", circle, new Vector2(0.4f, 0.1f), 0, new Vector2(0.14f, 0.14f), Lighter(bodyColor, 0.4f), 8);

        // Boots
        AddPart(p, "BootL", sq, new Vector2(-0.15f, -0.48f), 0, new Vector2(0.2f, 0.15f), boot, 7);
        AddPart(p, "BootR", sq, new Vector2(0.15f, -0.48f), 0, new Vector2(0.2f, 0.15f), boot, 7);
    }

    // ═══════════════════════════════════════════════════════════════════════
    //                           WEAPON BUILDERS
    // ═══════════════════════════════════════════════════════════════════════
    public static void BuildWeaponArt(Transform parent, string weaponName, Color baseColor)
    {
        switch (weaponName)
        {
            case "Dagger":      BuildDagger(parent, baseColor);    break;
            case "Bow":         BuildBow(parent, baseColor);       break;
            case "Fire Sword":
            case "FireSword":   BuildFireSword(parent, baseColor); break;
        }
    }

    static void BuildDagger(Transform p, Color c)
    {
        Sprite circle = GetCircle(), tri = GetTriangle(), sq = GetSquare();
        Color blade = new Color(0.88f, 0.93f, 1f);
        Color bladeEdge = new Color(0.55f, 0.62f, 0.78f);
        Color shine = Color.white;
        Color guard = new Color(1f, 0.78f, 0.25f);
        Color guardDark = new Color(0.7f, 0.5f, 0.1f);
        Color handle = new Color(0.42f, 0.22f, 0.1f);
        Color handleWrap = new Color(0.3f, 0.15f, 0.06f);
        Color pommel = new Color(1f, 0.85f, 0.3f);

        AddPart(p, "BladeEdge", tri, new Vector2(0f, 0.2f), 0, new Vector2(0.32f, 0.62f), bladeEdge, 4);
        AddPart(p, "Blade", tri, new Vector2(0f, 0.2f), 0, new Vector2(0.24f, 0.56f), blade, 5);
        AddPart(p, "BladeShine", sq, new Vector2(-0.04f, 0.28f), 0, new Vector2(0.03f, 0.32f), shine, 6);
        AddPart(p, "GuardShadow", sq, new Vector2(0f, -0.11f), 0, new Vector2(0.5f, 0.05f), guardDark, 6);
        AddPart(p, "Guard", sq, new Vector2(0f, -0.08f), 0, new Vector2(0.45f, 0.08f), guard, 7);
        AddPart(p, "HandleWrap1", sq, new Vector2(0f, -0.19f), 0, new Vector2(0.14f, 0.04f), handleWrap, 8);
        AddPart(p, "HandleWrap2", sq, new Vector2(0f, -0.27f), 0, new Vector2(0.14f, 0.04f), handleWrap, 8);
        AddPart(p, "Handle", sq, new Vector2(0f, -0.23f), 0, new Vector2(0.14f, 0.18f), handle, 7);
        AddPart(p, "Pommel", circle, new Vector2(0f, -0.38f), 0, new Vector2(0.17f, 0.14f), pommel, 8);
    }

    static void BuildBow(Transform p, Color c)
    {
        Sprite circle = GetCircle(), tri = GetTriangle(), sq = GetSquare();
        Color wood = new Color(0.6f, 0.35f, 0.15f);
        Color woodDark = new Color(0.4f, 0.22f, 0.08f);
        Color grip = new Color(0.25f, 0.12f, 0.05f);
        Color stringCol = new Color(0.95f, 0.95f, 0.85f);
        Color shaft = new Color(0.85f, 0.65f, 0.4f);
        Color silver = new Color(0.85f, 0.9f, 0.95f);
        Color fletch = new Color(0.9f, 0.2f, 0.2f);
        Color fletchDark = new Color(0.6f, 0.1f, 0.1f);

        // Bow stave — tall curved body made of 3 segments
        AddPart(p, "StaveTop", tri, new Vector2(0.1f, 0.35f), 90f, new Vector2(0.12f, 0.25f), wood, 3);
        AddPart(p, "StaveMid", sq, new Vector2(0.1f, 0f), -8f, new Vector2(0.11f, 0.6f), wood, 3);
        AddPart(p, "StaveBot", tri, new Vector2(0.1f, -0.35f), -90f, new Vector2(0.12f, 0.25f), wood, 3);

        // Wood grain shadow
        AddPart(p, "StaveShadow", sq, new Vector2(0.14f, 0f), -8f, new Vector2(0.04f, 0.55f), woodDark, 4);

        // Wrapped grip
        AddPart(p, "Grip", sq, new Vector2(0.1f, 0f), -8f, new Vector2(0.16f, 0.2f), grip, 5);

        // Bowstring
        AddPart(p, "String", sq, new Vector2(-0.05f, 0f), 8f, new Vector2(0.025f, 0.7f), stringCol, 3);

        // Arrow (horizontal, drawn back)
        AddPart(p, "ArrowShaft", sq, new Vector2(0f, 0f), 90f, new Vector2(0.05f, 0.55f), shaft, 6);
        AddPart(p, "ArrowTip", tri, new Vector2(0.32f, 0f), 90f, new Vector2(0.13f, 0.16f), silver, 7);
        AddPart(p, "ArrowTipEdge", tri, new Vector2(0.3f, 0f), 90f, new Vector2(0.09f, 0.11f), Color.white, 8);
        AddPart(p, "FletchTop", tri, new Vector2(-0.24f, 0.08f), -60f, new Vector2(0.11f, 0.14f), fletch, 7);
        AddPart(p, "FletchBot", tri, new Vector2(-0.24f, -0.08f), -120f, new Vector2(0.11f, 0.14f), fletchDark, 7);
    }

    static void BuildFireSword(Transform p, Color c)
    {
        Sprite circle = GetCircle(), tri = GetTriangle(), sq = GetSquare();
        Color flameOuter = new Color(1f, 0.4f, 0.05f);
        Color flameInner = new Color(1f, 0.85f, 0.3f);
        Color flameCore  = new Color(1f, 1f, 0.8f);
        Color bladeOuter = new Color(1f, 0.45f, 0.05f);
        Color blade      = new Color(1f, 0.8f, 0.25f);
        Color bladeHot   = new Color(1f, 1f, 0.75f);
        Color guard      = new Color(1f, 0.78f, 0.1f);
        Color guardDark  = new Color(0.7f, 0.5f, 0.0f);
        Color handle     = new Color(0.3f, 0.15f, 0.08f);
        Color pommel     = new Color(1f, 0.65f, 0.1f);

        // Flame aura glow behind everything
        AddPart(p, "AuraOuter", circle, new Vector2(0f, 0.2f), 0, new Vector2(0.85f, 1.1f),
            new Color(flameOuter.r, flameOuter.g, flameOuter.b, 0.3f), 2);
        AddPart(p, "AuraMid", circle, new Vector2(0f, 0.2f), 0, new Vector2(0.55f, 0.8f),
            new Color(flameInner.r, flameInner.g, flameInner.b, 0.4f), 3);

        // Flame tongues licking up from the blade
        AddPart(p, "FlameL", tri, new Vector2(-0.1f, 0.6f), -18f, new Vector2(0.13f, 0.25f), flameOuter, 4);
        AddPart(p, "FlameM", tri, new Vector2(0f, 0.68f), 0, new Vector2(0.15f, 0.32f), flameInner, 5);
        AddPart(p, "FlameR", tri, new Vector2(0.1f, 0.58f), 18f, new Vector2(0.12f, 0.22f), flameOuter, 4);
        AddPart(p, "FlameCore", tri, new Vector2(0f, 0.65f), 0, new Vector2(0.07f, 0.2f), flameCore, 6);

        // Blade with glowing hot core
        AddPart(p, "BladeOuter", tri, new Vector2(0f, 0.18f), 0, new Vector2(0.32f, 0.75f), bladeOuter, 5);
        AddPart(p, "Blade", tri, new Vector2(0f, 0.18f), 0, new Vector2(0.22f, 0.66f), blade, 6);
        AddPart(p, "BladeHot", sq, new Vector2(0f, 0.22f), 0, new Vector2(0.04f, 0.5f), bladeHot, 7);

        // Cross-guard with bead tips
        AddPart(p, "GuardShadow", sq, new Vector2(0f, -0.19f), 0, new Vector2(0.6f, 0.06f), guardDark, 7);
        AddPart(p, "Guard", sq, new Vector2(0f, -0.17f), 0, new Vector2(0.55f, 0.09f), guard, 8);
        AddPart(p, "BeadL", circle, new Vector2(-0.28f, -0.17f), 0, new Vector2(0.13f, 0.13f), pommel, 8);
        AddPart(p, "BeadR", circle, new Vector2(0.28f, -0.17f), 0, new Vector2(0.13f, 0.13f), pommel, 8);

        // Handle with wrap bands
        AddPart(p, "Handle", sq, new Vector2(0f, -0.32f), 0, new Vector2(0.16f, 0.22f), handle, 8);
        AddPart(p, "HandleBand1", sq, new Vector2(0f, -0.27f), 0, new Vector2(0.16f, 0.04f), pommel, 9);
        AddPart(p, "HandleBand2", sq, new Vector2(0f, -0.37f), 0, new Vector2(0.16f, 0.04f), pommel, 9);
        AddPart(p, "Pommel", circle, new Vector2(0f, -0.46f), 0, new Vector2(0.18f, 0.15f), pommel, 9);

        // Floating sparks
        AddPart(p, "Spark1", circle, new Vector2(-0.2f, 0.5f), 0, new Vector2(0.07f, 0.07f), flameInner, 9);
        AddPart(p, "Spark2", circle, new Vector2(0.25f, 0.4f), 0, new Vector2(0.05f, 0.05f), flameOuter, 9);
        AddPart(p, "Spark3", circle, new Vector2(0.2f, 0.08f), 0, new Vector2(0.05f, 0.05f), flameInner, 9);
        AddPart(p, "Spark4", circle, new Vector2(-0.25f, 0.25f), 0, new Vector2(0.04f, 0.04f), flameCore, 9);
    }
}
