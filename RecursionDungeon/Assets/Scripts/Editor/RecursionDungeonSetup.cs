using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

public class RecursionDungeonSetup : EditorWindow
{
    // ═══════════ RETRO PIXEL PALETTE ═══════════
    static readonly Color C_BG_DARK      = new Color(0.04f, 0.02f, 0.10f, 1f);
    static readonly Color C_PANEL        = new Color(0.12f, 0.07f, 0.22f, 0.95f);
    static readonly Color C_PANEL_BORDER = new Color(0.55f, 0.35f, 0.95f, 1f);
    static readonly Color C_ACCENT_CYAN  = new Color(0f, 0.95f, 1f, 1f);
    static readonly Color C_ACCENT_PINK  = new Color(1f, 0.30f, 0.70f, 1f);
    static readonly Color C_ACCENT_YELLOW= new Color(1f, 0.90f, 0.15f, 1f);
    static readonly Color C_ACCENT_GREEN = new Color(0.30f, 1f, 0.45f, 1f);
    static readonly Color C_ACCENT_RED   = new Color(1f, 0.25f, 0.30f, 1f);
    static readonly Color C_TEXT         = new Color(0.98f, 0.98f, 0.92f, 1f);
    static readonly Color C_TEXT_DIM     = new Color(0.60f, 0.55f, 0.75f, 1f);
    static readonly Color C_TEXT_SHADOW  = new Color(0f, 0f, 0f, 0.9f);

    // ═══════════ MENU ENTRIES ═══════════
    [MenuItem("Tools/Recursion Dungeon/Build FULL Game Scene")]
    static void BuildGameScene()
    {
        if (!EditorUtility.DisplayDialog("Build Game Scene",
            "Build all GameObjects for the game scene?", "Yes", "Cancel"))
            return;

        BuildPlayer();
        BuildRooms();
        BuildManagers();
        BuildCanvas();
        SetupCamera();
        EnsureEventSystem();
        WireReferences();

        Debug.Log("=== Recursion Dungeon Game Scene built! ===");
    }

    [MenuItem("Tools/Recursion Dungeon/Build Main Menu Scene")]
    static void BuildMainMenuScene()
    {
        if (!EditorUtility.DisplayDialog("Build Main Menu",
            "Build Main Menu GameObjects?", "Yes", "Cancel"))
            return;
        BuildMainMenu();
        EnsureEventSystem();
        Debug.Log("=== Main Menu built! ===");
    }

    [MenuItem("Tools/Recursion Dungeon/Add Missing EventSystem (fix unclickable buttons)")]
    static void AddEventSystemMenu()
    {
        EnsureEventSystem();
        EditorUtility.DisplayDialog("Done",
            "EventSystem has been added to the current scene.\nSave the scene and UI buttons will now be clickable.",
            "OK");
    }

    /// <summary>Makes sure the scene contains an EventSystem so UI buttons receive mouse clicks.</summary>
    static void EnsureEventSystem()
    {
        var existing = Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>();
        if (existing != null) return;

        GameObject es = new GameObject("EventSystem");
        es.AddComponent<UnityEngine.EventSystems.EventSystem>();
        es.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        Debug.Log("EventSystem added to scene.");
    }

    [MenuItem("Tools/Recursion Dungeon/Configure Build Settings (add both scenes)")]
    static void ConfigureBuildSettings()
    {
        string[] sceneGuids = AssetDatabase.FindAssets("t:Scene");
        string mainMenuPath = null, gameScenePath = null;
        foreach (var guid in sceneGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            string fileName = System.IO.Path.GetFileNameWithoutExtension(path);
            if (fileName == "MainMenu") mainMenuPath = path;
            else if (fileName == "GameScene") gameScenePath = path;
        }
        if (mainMenuPath == null || gameScenePath == null)
        {
            EditorUtility.DisplayDialog("Scenes Not Found",
                $"MainMenu: {(mainMenuPath ?? "NOT FOUND")}\nGameScene: {(gameScenePath ?? "NOT FOUND")}\n\nMake sure both are saved.",
                "OK");
            return;
        }
        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene(mainMenuPath, true),
            new EditorBuildSettingsScene(gameScenePath, true)
        };
        EditorUtility.DisplayDialog("Build Settings Configured",
            "Scenes added:\n0: MainMenu\n1: GameScene", "OK");
    }

    // ═══════════════════════════════════════════════════
    //                     PLAYER
    // ═══════════════════════════════════════════════════
    static GameObject BuildPlayer()
    {
        GameObject player = new GameObject("Player");
        player.tag = "Player";

        SpriteRenderer sr = player.AddComponent<SpriteRenderer>();
        sr.sprite = Circle();
        sr.color = C_ACCENT_CYAN;
        sr.sortingOrder = 10;
        player.transform.localScale = Vector3.one * 0.75f;

        Rigidbody2D rb = player.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;

        CircleCollider2D col = player.AddComponent<CircleCollider2D>();
        col.radius = 0.45f;

        player.AddComponent<PlayerController>();
        player.AddComponent<PlayerInventory>();
        PlayerCombat combat = player.AddComponent<PlayerCombat>();
        combat.monsterLayer = LayerMask.GetMask("Default");
        combat.attackRange = 1.8f;

        // Detailed knight/hero visual (helmet, plume, armor, boots) built at runtime.
        player.AddComponent<PlayerVisual>();

        // Small facing-direction arrow floats above the hero's head.
        GameObject arrow = new GameObject("DirectionArrow");
        arrow.transform.SetParent(player.transform);
        arrow.transform.localPosition = new Vector3(0, 0.85f, 0);
        arrow.transform.localScale = new Vector3(0.25f, 0.25f, 1f);
        SpriteRenderer arrowSR = arrow.AddComponent<SpriteRenderer>();
        arrowSR.sprite = Circle();
        arrowSR.color = new Color(1f, 1f, 0.5f, 0.7f);
        arrowSR.sortingOrder = 11;

        // Weapon icon
        GameObject weaponIcon = new GameObject("WeaponIcon");
        weaponIcon.transform.SetParent(player.transform);
        weaponIcon.transform.localPosition = new Vector3(0.65f, -0.3f, 0);
        weaponIcon.transform.localScale = Vector3.one * 0.45f;
        SpriteRenderer weaponSR = weaponIcon.AddComponent<SpriteRenderer>();
        weaponSR.sprite = Circle();
        weaponSR.color = Color.gray;
        weaponSR.sortingOrder = 12;
        weaponSR.enabled = false;
        player.GetComponent<PlayerInventory>().weaponIconRenderer = weaponSR;

        AddWorldLabel(player.transform, "YOU", new Vector3(0, -0.7f, 0), C_ACCENT_CYAN, 26);
        return player;
    }

    // ═══════════════════════════════════════════════════
    //                     ROOMS + MAZES
    // ═══════════════════════════════════════════════════
    static void BuildRooms()
    {
        GameObject roomsParent = new GameObject("Rooms");

        string[] roomNames = { "Room0_GrandHall", "Room1_DarkCave", "Room2_Crypt", "Room3_TinyChamber" };
        float[] roomSizes = { 14f, 12f, 10f, 8f };

        Color[] floorColors = {
            new Color(0.22f, 0.18f, 0.14f),
            new Color(0.14f, 0.14f, 0.22f),
            new Color(0.18f, 0.10f, 0.22f),
            new Color(0.10f, 0.08f, 0.14f)
        };
        Color[] wallColors = {
            new Color(0.70f, 0.45f, 0.20f),
            new Color(0.40f, 0.45f, 0.65f),
            new Color(0.65f, 0.30f, 0.70f),
            new Color(0.50f, 0.40f, 0.40f)
        };

        string[] monsterNames = { "Dragon", "Wolf", "Bat", "" };
        Color[] monsterColors = {
            new Color(1f, 0.20f, 0.10f),
            new Color(0.75f, 0.75f, 0.80f),
            new Color(0.85f, 0.30f, 1f),
            Color.clear
        };
        // Monster sizes kept small enough to fit INSIDE the maze corridors
        float[] monsterSizes = { 1.4f, 1.2f, 1.0f, 0f };
        // Monsters placed in a DEAD-END POCKET on the right side of the first corridor
        // so they're visible but do NOT block the player's traversal path through the maze.
        Vector3[] monsterPositions = {
            new Vector3(5f,   -1.5f, 0),   // Room 0: right dead-end past wall 1 gap
            new Vector3(4.5f, -1f,   0),   // Room 1: right dead-end past wall 1 gap
            new Vector3(3f,   -0.75f,0),   // Room 2: right dead-end past wall 1 gap
            Vector3.zero
        };

        string[] requiredWeapons = { "Fire Sword", "Bow", "Dagger", "" };
        string[] rewardWeapons   = { "",           "Fire Sword", "Bow", "" };
        Color[] rewardColors = {
            Color.clear,
            new Color(1f, 0.50f, 0.05f),
            new Color(0.55f, 0.35f, 0.10f),
            Color.clear
        };

        for (int i = 0; i < 4; i++)
        {
            GameObject room = new GameObject(roomNames[i]);
            room.transform.SetParent(roomsParent.transform);
            room.transform.position = Vector3.zero;

            float size = roomSizes[i];
            float half = size / 2f;

            // Floor
            CreateSprite($"Floor_{i}", room.transform, Vector3.zero,
                new Vector3(size, size, 1), floorColors[i], 0);

            // Floor pixel grid (more retro look)
            Color gridDark = floorColors[i] * 0.75f; gridDark.a = 0.6f;
            Color gridLight = floorColors[i] * 1.25f; gridLight.a = 0.4f;
            for (int g = -((int)half) + 1; g < (int)half; g++)
            {
                CreateSprite($"GridH_{g}", room.transform, new Vector3(0, g, 0),
                    new Vector3(size - 0.6f, 0.05f, 1), gridLight, 1);
                CreateSprite($"GridV_{g}", room.transform, new Vector3(g, 0, 0),
                    new Vector3(0.05f, size - 0.6f, 1), gridLight, 1);
            }

            // Perimeter walls
            float wallThick = 0.6f;
            CreateWall($"WallTop_{i}",    room.transform, new Vector3(0, half + wallThick/2f, 0),
                new Vector3(size + wallThick * 2, wallThick, 1), wallColors[i]);
            CreateWall($"WallBot_{i}",    room.transform, new Vector3(0, -half - wallThick/2f, 0),
                new Vector3(size + wallThick * 2, wallThick, 1), wallColors[i]);
            CreateWall($"WallLeft_{i}",   room.transform, new Vector3(-half - wallThick/2f, 0, 0),
                new Vector3(wallThick, size + wallThick * 2, 1), wallColors[i]);
            CreateWall($"WallRight_{i}",  room.transform, new Vector3(half + wallThick/2f, 0, 0),
                new Vector3(wallThick, size + wallThick * 2, 1), wallColors[i]);

            // ── INTERNAL MAZE WALLS ──
            BuildMazeFor(i, room.transform, half, wallColors[i]);

            // Entry point (bottom-left corner of the room so doors at center are reached via maze)
            GameObject entry = new GameObject($"EntryPoint_{i}");
            entry.transform.SetParent(room.transform);
            entry.transform.localPosition = new Vector3(-half + 1.5f, -half + 1.5f, 0);

            // Room name label
            string displayName = roomNames[i].Replace($"Room{i}_", "").Replace("_", " ");
            AddWorldLabel(room.transform, displayName,
                new Vector3(0, half - 1.0f, 0), new Color(1f, 1f, 1f, 0.35f), 45);

            // Enter-deeper door (top center)
            if (i < 3)
                CreateDoor($"DoorDeeper_{i}", room.transform,
                    new Vector3(0, half - 0.35f, 0), false, i + 1,
                    Door.DoorType.EnterDeeper, "ENTER DEEPER >>>");

            // Exit-to-parent door (bottom center)
            if (i > 0)
            {
                bool locked = !string.IsNullOrEmpty(monsterNames[i]);
                CreateDoor($"DoorExit_{i}", room.transform,
                    new Vector3(0, -half + 0.35f, 0), locked, i - 1,
                    Door.DoorType.ExitToParent, "<<< GO BACK");
            }

            // Room 0 victory door
            if (i == 0)
                CreateDoor("DoorVictoryExit", room.transform,
                    new Vector3(0, -half + 0.35f, 0), true, 0,
                    Door.DoorType.ExitToParent, "<<< ESCAPE DUNGEON");

            // Monster (except room 3)
            if (i < 3)
            {
                GameObject monster = CreateMonster($"Monster_{monsterNames[i]}", room.transform,
                    monsterPositions[i], monsterNames[i], monsterColors[i],
                    monsterSizes[i], requiredWeapons[i], rewardWeapons[i], rewardColors[i]);
                Monster mc = monster.GetComponent<Monster>();
                Door exitDoor = (i == 0)
                    ? room.transform.Find("DoorVictoryExit")?.GetComponent<Door>()
                    : room.transform.Find($"DoorExit_{i}")?.GetComponent<Door>();
                if (mc != null && exitDoor != null) mc.exitDoor = exitDoor;
            }

            // Base case item (Room 3)
            if (i == 3)
            {
                CreateItem("Item_Dagger", room.transform, new Vector3(0, 0.5f, 0),
                    "Dagger", new Color(0.85f, 0.90f, 1f), 0.7f);
                AddWorldLabel(room.transform, "~ BASE CASE ~",
                    new Vector3(0, 2.8f, 0), C_ACCENT_GREEN, 34);
                AddWorldLabel(room.transform, "No monster here!\nGrab the Dagger and return.",
                    new Vector3(0, -2f, 0), C_ACCENT_YELLOW, 20);
            }

            room.SetActive(i == 0);
        }
    }

    // ═══════════════════════════════════════════════════
    //                    MAZE BUILDER
    // Each room has a zigzag maze (more complex deeper).
    // Walls are HIGH-CONTRAST stone-colored blocks that stand
    // out against the dark floors.
    // ═══════════════════════════════════════════════════
    static readonly Color[] MAZE_COLORS = {
        new Color(0.95f, 0.70f, 0.35f, 1f),  // Room 0: bright warm sandstone (contrasts brown floor)
        new Color(0.55f, 0.75f, 1.00f, 1f),  // Room 1: bright ice blue (contrasts dark blue floor)
        new Color(1.00f, 0.55f, 0.90f, 1f),  // Room 2: bright pink stone (contrasts dark purple floor)
        new Color(0.85f, 0.85f, 0.90f, 1f)   // Room 3: bright bone white
    };

    // Maze design principle:
    //   - Horizontal walls create a zigzag skeleton with alternating gaps (R/L/R)
    //   - Vertical wall segments inside corridors force ADDITIONAL turns & decisions
    //   - Dead-end branches trick the player into wrong paths
    //   - Monsters live in side pockets off the main traversal route
    //   - Walls are thin (t=0.5) so corridors feel spacious
    //   - Deeper rooms have MORE internal walls (harder mazes)
    static void BuildMazeFor(int depth, Transform parent, float half, Color wallColor)
    {
        Color mazeColor = MAZE_COLORS[depth];
        float t = 0.5f; // thin walls → more playable space

        // Horizontal walls slide horizontally → the gap shifts left/right over time,
        // so player must time their move when the gap aligns with them.
        if (depth == 0)
        {
            // ─── GRAND HALL (14×14) ─── intro: H1 static, upper walls + posts moving
            AddMazeWall(parent, new Vector3(-2f, -3f, 0), new Vector3(10f, t, 1), mazeColor); // H1 static (safe entry)
            AddMovingMazeWall(parent, new Vector3(2f, 0.5f, 0), new Vector3(10f, t, 1),
                MovingWall.MoveAxis.Horizontal, 2f, 0.9f);                              // H2 sliding
            AddMovingMazeWall(parent, new Vector3(-2f, 4f, 0), new Vector3(10f, t, 1),
                MovingWall.MoveAxis.Horizontal, 2f, 0.9f, Mathf.PI);                    // H3 sliding (opposite phase)
            AddMazeWall(parent, new Vector3(0f, -1.25f, 0), new Vector3(t, 1.5f, 1), mazeColor); // V1 static post
            AddMovingMazeWall(parent, new Vector3(0f, 2.25f, 0), new Vector3(t, 1.5f, 1),
                MovingWall.MoveAxis.Horizontal, 3f, 1.3f);                              // V2 sliding post
        }
        else if (depth == 1)
        {
            // ─── DARK CAVE (12×12) ─── every horizontal wall slides, plus 2 moving posts
            AddMovingMazeWall(parent, new Vector3(-1.5f, -2.5f, 0), new Vector3(8f, t, 1),
                MovingWall.MoveAxis.Horizontal, 1.8f, 1.0f);                            // H1 sliding
            AddMovingMazeWall(parent, new Vector3(1.5f, 0f, 0), new Vector3(8f, t, 1),
                MovingWall.MoveAxis.Horizontal, 1.8f, 1.0f, Mathf.PI);                  // H2 sliding (opposite)
            AddMovingMazeWall(parent, new Vector3(-1.5f, 2.5f, 0), new Vector3(8f, t, 1),
                MovingWall.MoveAxis.Horizontal, 1.8f, 1.0f);                            // H3 sliding
            // Moving posts out of phase
            AddMovingMazeWall(parent, new Vector3(-0.5f, -1.25f, 0), new Vector3(t, 1.5f, 1),
                MovingWall.MoveAxis.Horizontal, 2.5f, 1.5f);
            AddMovingMazeWall(parent, new Vector3(0.5f, 1.25f, 0), new Vector3(t, 1.5f, 1),
                MovingWall.MoveAxis.Horizontal, 2.5f, 1.5f, Mathf.PI * 0.5f);
            // Static red-herring dead-ends
            AddMazeWall(parent, new Vector3( 3.5f, -1.5f, 0), new Vector3(3f, t, 1), mazeColor);
            AddMazeWall(parent, new Vector3(-3.5f, 1.25f, 0), new Vector3(t, 2.5f, 1), mazeColor);
        }
        else if (depth == 2)
        {
            // ─── CRYPT (10×10) ─── hardest: all three horizontals sliding + vertical gate
            AddMovingMazeWall(parent, new Vector3(-1.5f, -2f, 0), new Vector3(6f, t, 1),
                MovingWall.MoveAxis.Horizontal, 1.6f, 1.2f);                            // H1 sliding fast
            AddMovingMazeWall(parent, new Vector3(1.5f, 0.5f, 0), new Vector3(6f, t, 1),
                MovingWall.MoveAxis.Horizontal, 1.6f, 1.2f, Mathf.PI);                  // H2 opposite
            AddMovingMazeWall(parent, new Vector3(-1.5f, 3f, 0), new Vector3(6f, t, 1),
                MovingWall.MoveAxis.Horizontal, 1.6f, 1.2f);                            // H3 sliding
            // Moving post + vertical up-down gate
            AddMovingMazeWall(parent, new Vector3(-0.5f, -0.75f, 0), new Vector3(t, 1.2f, 1),
                MovingWall.MoveAxis.Horizontal, 2.5f, 1.8f);
            AddMovingMazeWall(parent, new Vector3(2.5f, 1.75f, 0), new Vector3(2f, t, 1),
                MovingWall.MoveAxis.Vertical, 1.4f, 1.6f, 1f);
            // Static dead-ends
            AddMazeWall(parent, new Vector3(-3f,  1.75f, 0), new Vector3(t, 2f, 1), mazeColor);
            AddMazeWall(parent, new Vector3(-2f,  3.75f, 0), new Vector3(2f, t, 1), mazeColor);
        }
        else
        {
            // ─── TINY CHAMBER (8×8) ─── base case, gentle static maze
            AddMazeWall(parent, new Vector3(-1f, -0.5f, 0), new Vector3(5f, t, 1), mazeColor);
            AddMazeWall(parent, new Vector3( 1f,  2f,   0), new Vector3(5f, t, 1), mazeColor);
            AddMazeWall(parent, new Vector3( 0f,  0.75f, 0), new Vector3(t, 1f, 1), mazeColor);
        }
    }

    static GameObject AddMazeWall(Transform parent, Vector3 localPos, Vector3 scale, Color color)
    {
        // Main wall body — solid bright color, on top of floor & grid
        GameObject w = CreateSprite($"MazeWall_{parent.childCount}", parent, localPos, scale, color, 4);
        BoxCollider2D col = w.AddComponent<BoxCollider2D>();
        col.size = new Vector2(1f, 1f);

        // Dark outline: extend by a constant 0.15 world units on each side regardless of wall size
        Color outlineColor = new Color(color.r * 0.2f, color.g * 0.2f, color.b * 0.2f, 1f);
        GameObject outline = new GameObject("Outline");
        outline.transform.SetParent(w.transform, false);
        outline.transform.localPosition = Vector3.zero;
        outline.transform.localScale = new Vector3(
            (scale.x + 0.3f) / scale.x,
            (scale.y + 0.3f) / scale.y,
            1f);
        SpriteRenderer osr = outline.AddComponent<SpriteRenderer>();
        osr.sprite = Square();
        osr.color = outlineColor;
        osr.sortingOrder = 3;

        // Retro highlight: thin bright stripe. Placed on TOP for horizontal walls,
        // on LEFT side for vertical walls so the "light source" feels consistent.
        bool isHorizontal = scale.x >= scale.y;
        GameObject highlight = new GameObject("Highlight");
        highlight.transform.SetParent(w.transform, false);
        if (isHorizontal)
        {
            highlight.transform.localPosition = new Vector3(0f, 0.32f, 0f);
            highlight.transform.localScale = new Vector3(0.92f, 0.22f, 1f);
        }
        else
        {
            highlight.transform.localPosition = new Vector3(-0.32f, 0f, 0f);
            highlight.transform.localScale = new Vector3(0.22f, 0.92f, 1f);
        }
        SpriteRenderer hsr = highlight.AddComponent<SpriteRenderer>();
        hsr.sprite = Square();
        hsr.color = Color.Lerp(color, Color.white, 0.6f);
        hsr.sortingOrder = 5;

        return w;
    }

    // Golden/yellow tint for moving walls so player visually distinguishes them
    static readonly Color MOVING_WALL_COLOR = new Color(1f, 0.85f, 0.25f, 1f);

    /// <summary>Creates a maze wall that oscillates back and forth.</summary>
    static GameObject AddMovingMazeWall(Transform parent, Vector3 localPos, Vector3 scale,
        MovingWall.MoveAxis axis, float distance, float speed, float phase = 0f)
    {
        GameObject w = AddMazeWall(parent, localPos, scale, MOVING_WALL_COLOR);
        w.name = $"MovingWall_{parent.childCount}";

        MovingWall mw = w.AddComponent<MovingWall>();
        mw.axis = axis;
        mw.distance = distance;
        mw.speed = speed;
        mw.phaseOffset = phase;

        return w;
    }

    // ═══════════════════════════════════════════════════
    //                    MANAGERS
    // ═══════════════════════════════════════════════════
    static void BuildManagers()
    {
        GameObject managers = new GameObject("--- MANAGERS ---");
        GameObject gm = new GameObject("GameManager");
        gm.transform.SetParent(managers.transform);
        gm.AddComponent<GameManager>();

        GameObject bt = new GameObject("BombTimer");
        bt.transform.SetParent(managers.transform);
        BombTimer bomb = bt.AddComponent<BombTimer>();
        bomb.fuseSeconds = 20f;

        GameObject rm = new GameObject("RoomManager");
        rm.transform.SetParent(managers.transform);
        RoomManager roomMgr = rm.AddComponent<RoomManager>();

        Transform roomsParent = GameObject.Find("Rooms")?.transform;
        Transform player = GameObject.Find("Player")?.transform;
        if (roomsParent != null)
        {
            roomMgr.rooms = new GameObject[4];
            roomMgr.entryPoints = new Transform[4];
            for (int i = 0; i < 4; i++)
            {
                roomMgr.rooms[i] = roomsParent.GetChild(i).gameObject;
                roomMgr.entryPoints[i] = roomsParent.GetChild(i).Find($"EntryPoint_{i}");
            }
        }
        if (player != null) roomMgr.player = player;
    }

    // ═══════════════════════════════════════════════════
    //                    RETRO PIXEL CANVAS
    // ═══════════════════════════════════════════════════
    static void BuildCanvas()
    {
        GameObject canvasGO = new GameObject("GameCanvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasGO.AddComponent<GraphicRaycaster>();

        UIManager uiMgr = canvasGO.AddComponent<UIManager>();
        CameraTransition camTrans = canvasGO.AddComponent<CameraTransition>();

        // Fade
        GameObject fadeObj = CreateUIImage("FadeImage", canvasGO.transform, Color.black);
        StretchFull(fadeObj);
        Image fadeImg = fadeObj.GetComponent<Image>();
        fadeImg.color = new Color(0, 0, 0, 0);
        fadeImg.raycastTarget = false;
        camTrans.fadeImage = fadeImg;

        // ── TOP LEFT: Room Info (pixel framed) ──
        GameObject hudLeft = CreatePixelPanel("HUD_TopLeft", canvasGO.transform,
            new Vector2(0, 1), new Vector2(15, -15), new Vector2(340, 100),
            C_PANEL, C_ACCENT_CYAN);

        GameObject depthText = CreatePixelText("DepthText", hudLeft.transform, "DEPTH: 0",
            28, TextAnchor.UpperLeft, new Vector2(20, -12), new Vector2(300, 40), C_ACCENT_CYAN);

        GameObject roomText = CreatePixelText("RoomNameText", hudLeft.transform, "GRAND HALL",
            22, TextAnchor.UpperLeft, new Vector2(20, -52), new Vector2(300, 35), C_ACCENT_YELLOW);

        uiMgr.depthText = depthText.GetComponent<Text>();
        uiMgr.roomNameText = roomText.GetComponent<Text>();

        // ── TOP RIGHT: Weapon ──
        GameObject hudRight = CreatePixelPanel("HUD_TopRight", canvasGO.transform,
            new Vector2(1, 1), new Vector2(-15, -15), new Vector2(340, 80),
            C_PANEL, C_ACCENT_PINK);
        hudRight.GetComponent<RectTransform>().pivot = new Vector2(1, 1);

        GameObject weaponText = CreatePixelText("WeaponText", hudRight.transform, "WEAPON: NONE",
            22, TextAnchor.MiddleRight, new Vector2(-20, 0), new Vector2(250, 40), C_ACCENT_PINK);

        GameObject weaponIcon = CreateUIImage("WeaponIcon", hudRight.transform, Color.gray);
        RectTransform wiRT = weaponIcon.GetComponent<RectTransform>();
        wiRT.anchorMin = new Vector2(0, 0.5f); wiRT.anchorMax = new Vector2(0, 0.5f);
        wiRT.anchoredPosition = new Vector2(35, 0);
        wiRT.sizeDelta = new Vector2(45, 45);
        AddPixelBorder(weaponIcon, Color.white, 2);

        uiMgr.weaponNameText = weaponText.GetComponent<Text>();
        uiMgr.weaponIconImage = weaponIcon.GetComponent<Image>();

        // ── TOP CENTER: Timer ──
        GameObject timerPanel = CreatePixelPanel("TimerPanel", canvasGO.transform,
            new Vector2(0.5f, 1), new Vector2(0, -15), new Vector2(160, 55),
            C_PANEL, C_ACCENT_GREEN);
        timerPanel.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 1);

        GameObject timerText = CreatePixelText("TimerText", timerPanel.transform, "00:00",
            26, TextAnchor.MiddleCenter, Vector2.zero, new Vector2(150, 50), C_ACCENT_GREEN);
        RectTransform ttRT = timerText.GetComponent<RectTransform>();
        ttRT.anchorMin = Vector2.zero; ttRT.anchorMax = Vector2.one;
        ttRT.offsetMin = Vector2.zero; ttRT.offsetMax = Vector2.zero;
        uiMgr.timerText = timerText.GetComponent<Text>();

        // ── BOTTOM CENTER: Dialog ──
        GameObject dialogPanel = CreatePixelPanel("DialogPanel", canvasGO.transform,
            new Vector2(0.5f, 0), new Vector2(0, 50), new Vector2(640, 80),
            C_PANEL, C_ACCENT_YELLOW);
        dialogPanel.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0);

        GameObject dialogText = CreatePixelText("DialogText", dialogPanel.transform, "",
            26, TextAnchor.MiddleCenter, Vector2.zero, new Vector2(610, 70), C_ACCENT_YELLOW);
        RectTransform dlgTRT = dialogText.GetComponent<RectTransform>();
        dlgTRT.anchorMin = Vector2.zero; dlgTRT.anchorMax = Vector2.one;
        dlgTRT.offsetMin = new Vector2(15, 8); dlgTRT.offsetMax = new Vector2(-15, -8);

        uiMgr.dialogPanel = dialogPanel;
        uiMgr.dialogText = dialogText.GetComponent<Text>();
        dialogPanel.SetActive(false);

        // ── BOTTOM RIGHT: Controls ──
        GameObject controls = CreatePixelPanel("ControlsHint", canvasGO.transform,
            new Vector2(1, 0), new Vector2(-15, 15), new Vector2(260, 110),
            new Color(0.08f, 0.05f, 0.15f, 0.85f), C_TEXT_DIM);
        controls.GetComponent<RectTransform>().pivot = new Vector2(1, 0);

        GameObject controlsText = CreatePixelText("ControlsText", controls.transform,
            "WASD - MOVE\nSPACE - ATTACK\nFIND DOORS IN MAZE",
            15, TextAnchor.MiddleLeft, Vector2.zero, new Vector2(240, 100), C_TEXT_DIM);
        RectTransform ctrlRT = controlsText.GetComponent<RectTransform>();
        ctrlRT.anchorMin = Vector2.zero; ctrlRT.anchorMax = Vector2.one;
        ctrlRT.offsetMin = new Vector2(15, 8); ctrlRT.offsetMax = new Vector2(-15, -8);

        // ── LEFT SIDE: Call Stack ──
        GameObject callStackBG = CreatePixelPanel("CallStackPanel", canvasGO.transform,
            new Vector2(0, 0), new Vector2(15, 150), new Vector2(320, 470),
            C_PANEL, C_PANEL_BORDER);
        callStackBG.GetComponent<RectTransform>().pivot = new Vector2(0, 0);

        // Title bar
        GameObject titleBar = CreateUIImage("TitleBar", callStackBG.transform, C_PANEL_BORDER);
        RectTransform tbRT = titleBar.GetComponent<RectTransform>();
        tbRT.anchorMin = new Vector2(0, 1); tbRT.anchorMax = new Vector2(1, 1);
        tbRT.pivot = new Vector2(0.5f, 1);
        tbRT.offsetMin = new Vector2(0, -40); tbRT.offsetMax = Vector2.zero;

        CreatePixelText("CallStackTitle", titleBar.transform, "≡ CALL STACK ≡",
            20, TextAnchor.MiddleCenter, Vector2.zero, new Vector2(300, 35), Color.white);

        // Stack container
        GameObject stackContainer = new GameObject("StackContainer");
        stackContainer.transform.SetParent(callStackBG.transform, false);
        RectTransform scRT = stackContainer.AddComponent<RectTransform>();
        scRT.anchorMin = new Vector2(0, 0); scRT.anchorMax = new Vector2(1, 1);
        scRT.offsetMin = new Vector2(10, 10); scRT.offsetMax = new Vector2(-10, -45);
        VerticalLayoutGroup vlg = stackContainer.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 8;
        vlg.childAlignment = TextAnchor.LowerLeft;
        vlg.childControlWidth = true;
        vlg.childControlHeight = false;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;
        stackContainer.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        CallStackUI csUI = canvasGO.AddComponent<CallStackUI>();
        csUI.stackContainer = stackContainer.transform;

        GameObject framePrefab = CreateCallStackFrame(canvasGO.transform);
        csUI.framePrefab = framePrefab;
        framePrefab.SetActive(false);

        // ── VICTORY PANEL ──
        GameObject victoryPanel = CreatePixelPanel("VictoryPanel", canvasGO.transform,
            new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(620, 430),
            C_PANEL, C_ACCENT_YELLOW);
        victoryPanel.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0.5f);
        AddPixelBorder(victoryPanel, C_ACCENT_YELLOW, 4);

        CreatePixelText("VictoryTitle", victoryPanel.transform, "★ VICTORY ★",
            48, TextAnchor.MiddleCenter, new Vector2(0, 140), new Vector2(600, 70), C_ACCENT_YELLOW);

        CreatePixelText("VictorySubtitle", victoryPanel.transform, "YOU ESCAPED THE DUNGEON!",
            22, TextAnchor.MiddleCenter, new Vector2(0, 80), new Vector2(580, 40), Color.white);

        CreatePixelText("RecursionNote", victoryPanel.transform,
            "~ The recursion has unwound ~",
            18, TextAnchor.MiddleCenter, new Vector2(0, 35), new Vector2(580, 30), C_ACCENT_GREEN);

        GameObject victoryTime = CreatePixelText("VictoryTime", victoryPanel.transform, "TIME: 00:00",
            28, TextAnchor.MiddleCenter, new Vector2(0, -20), new Vector2(580, 45), C_ACCENT_CYAN);

        GameObject playAgainBtn = CreatePixelButton("PlayAgainButton", victoryPanel.transform,
            "PLAY AGAIN  [R]", new Vector2(-115, -95), new Vector2(220, 55),
            new Color(0.15f, 0.5f, 0.2f), C_ACCENT_GREEN);

        GameObject mainMenuBtn = CreatePixelButton("MainMenuButton", victoryPanel.transform,
            "MAIN MENU  [M]", new Vector2(115, -95), new Vector2(220, 55),
            new Color(0.5f, 0.15f, 0.2f), C_ACCENT_RED);

        CreatePixelText("KeyHint", victoryPanel.transform,
            "Press R to replay  ·  M for main menu",
            15, TextAnchor.MiddleCenter, new Vector2(0, -160), new Vector2(560, 25), C_TEXT_DIM);

        uiMgr.victoryPanel = victoryPanel;
        uiMgr.victoryTimeText = victoryTime.GetComponent<Text>();
        uiMgr.playAgainButton = playAgainBtn.GetComponent<Button>();
        uiMgr.mainMenuButton = mainMenuBtn.GetComponent<Button>();
        victoryPanel.SetActive(false);

        // ── BOMB TIMER PANEL (top-center, below the main timer) ──
        Color bombBorder = new Color(1f, 0.3f, 0.1f);
        Color bombBody = new Color(0.12f, 0.04f, 0.06f, 0.95f);

        GameObject bombPanel = CreatePixelPanel("BombPanel", canvasGO.transform,
            new Vector2(0.5f, 1), new Vector2(0, -80), new Vector2(360, 120),
            bombBody, bombBorder);
        bombPanel.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 1);
        AddPixelBorder(bombPanel, bombBorder, 3);

        GameObject bombLabel = CreatePixelText("BombLabel", bombPanel.transform,
            "▼  BOMB TIMER  ▼",
            18, TextAnchor.MiddleCenter, Vector2.zero, new Vector2(340, 28),
            new Color(1f, 0.55f, 0.15f));
        RectTransform blRT = bombLabel.GetComponent<RectTransform>();
        blRT.anchorMin = new Vector2(0, 1); blRT.anchorMax = new Vector2(1, 1);
        blRT.pivot = new Vector2(0.5f, 1);
        blRT.anchoredPosition = new Vector2(0, -6);
        blRT.sizeDelta = new Vector2(-10, 26);

        GameObject bombTimer = CreatePixelText("BombTimerText", bombPanel.transform,
            "20.00",
            52, TextAnchor.MiddleCenter, Vector2.zero, new Vector2(340, 70),
            new Color(1f, 0.85f, 0.25f));
        RectTransform btRT = bombTimer.GetComponent<RectTransform>();
        btRT.anchorMin = new Vector2(0, 0.5f); btRT.anchorMax = new Vector2(1, 0.5f);
        btRT.pivot = new Vector2(0.5f, 0.5f);
        btRT.anchoredPosition = new Vector2(0, 2);
        btRT.sizeDelta = new Vector2(-10, 70);

        // Fuse bar background (dark)
        GameObject fuseBG = CreateUIImage("FuseBarBG", bombPanel.transform,
            new Color(0.08f, 0.02f, 0.02f, 1f));
        RectTransform fuseBGRT = fuseBG.GetComponent<RectTransform>();
        fuseBGRT.anchorMin = new Vector2(0, 0); fuseBGRT.anchorMax = new Vector2(1, 0);
        fuseBGRT.pivot = new Vector2(0.5f, 0);
        fuseBGRT.anchoredPosition = new Vector2(0, 8);
        fuseBGRT.sizeDelta = new Vector2(-24, 12);

        // Fuse bar fill (the actual countdown bar)
        GameObject fuseBar = CreateUIImage("FuseBarFill", fuseBG.transform,
            new Color(1f, 0.55f, 0.1f));
        RectTransform fuseBarRT = fuseBar.GetComponent<RectTransform>();
        fuseBarRT.anchorMin = new Vector2(0, 0); fuseBarRT.anchorMax = new Vector2(1, 1);
        fuseBarRT.offsetMin = new Vector2(2, 2); fuseBarRT.offsetMax = new Vector2(-2, -2);
        Image fuseBarImg = fuseBar.GetComponent<Image>();
        fuseBarImg.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        fuseBarImg.type = Image.Type.Filled;
        fuseBarImg.fillMethod = Image.FillMethod.Horizontal;
        fuseBarImg.fillOrigin = (int)Image.OriginHorizontal.Left;
        fuseBarImg.fillAmount = 1f;

        uiMgr.bombPanel = bombPanel;
        uiMgr.bombTimerText = bombTimer.GetComponent<Text>();
        uiMgr.bombLabelText = bombLabel.GetComponent<Text>();
        uiMgr.bombFuseBar = fuseBarImg;
        uiMgr.bombPanelBackground = bombPanel.transform.Find("Body").GetComponent<Image>();
        bombPanel.SetActive(false);

        // ── GAME OVER PANEL ──
        GameObject gameOverPanel = CreatePixelPanel("GameOverPanel", canvasGO.transform,
            new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(680, 460),
            new Color(0.10f, 0.03f, 0.05f, 0.98f), C_ACCENT_RED);
        gameOverPanel.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0.5f);
        AddPixelBorder(gameOverPanel, C_ACCENT_RED, 5);

        CreatePixelText("GOTitle", gameOverPanel.transform, "☠  DEFEAT  ☠",
            54, TextAnchor.MiddleCenter, new Vector2(0, 155), new Vector2(640, 80), C_ACCENT_RED);

        CreatePixelText("GOBoom", gameOverPanel.transform, "✹  B O O M  ✹",
            32, TextAnchor.MiddleCenter, new Vector2(0, 90), new Vector2(640, 50),
            new Color(1f, 0.55f, 0.15f));

        GameObject goReason = CreatePixelText("GOReason", gameOverPanel.transform,
            "THE BOMB EXPLODED!",
            22, TextAnchor.MiddleCenter, new Vector2(0, 40), new Vector2(640, 40), Color.white);

        CreatePixelText("GOTip", gameOverPanel.transform,
            "The Dagger armed a 20-second fuse.\nEscape the recursion faster next time!",
            17, TextAnchor.MiddleCenter, new Vector2(0, -15), new Vector2(620, 70), C_TEXT_DIM);

        GameObject goRestartBtn = CreatePixelButton("GORestartButton", gameOverPanel.transform,
            "TRY AGAIN  [R]", new Vector2(-130, -110), new Vector2(240, 60),
            new Color(0.5f, 0.15f, 0.2f), C_ACCENT_RED);

        GameObject goMenuBtn = CreatePixelButton("GOMenuButton", gameOverPanel.transform,
            "MAIN MENU  [M]", new Vector2(130, -110), new Vector2(240, 60),
            new Color(0.15f, 0.15f, 0.4f), C_ACCENT_CYAN);

        CreatePixelText("GOHint", gameOverPanel.transform,
            "Press R to retry  ·  M for main menu",
            15, TextAnchor.MiddleCenter, new Vector2(0, -180), new Vector2(560, 25), C_TEXT_DIM);

        uiMgr.gameOverPanel = gameOverPanel;
        uiMgr.gameOverReasonText = goReason.GetComponent<Text>();
        uiMgr.gameOverRestartButton = goRestartBtn.GetComponent<Button>();
        uiMgr.gameOverMenuButton = goMenuBtn.GetComponent<Button>();
        gameOverPanel.SetActive(false);
    }

    // ═══════════════════════════════════════════════════
    //                       CAMERA
    // ═══════════════════════════════════════════════════
    static void SetupCamera()
    {
        Camera cam = Camera.main;
        if (cam == null)
        {
            GameObject camGO = new GameObject("Main Camera");
            camGO.tag = "MainCamera";
            cam = camGO.AddComponent<Camera>();
            camGO.AddComponent<AudioListener>();
            camGO.transform.position = new Vector3(0, 0, -10);
        }
        cam.orthographic = true;
        cam.orthographicSize = 8f;
        cam.backgroundColor = C_BG_DARK;
        cam.clearFlags = CameraClearFlags.SolidColor;
        if (cam.GetComponent<CameraFollow>() == null)
            cam.gameObject.AddComponent<CameraFollow>();
        CameraFollow follow = cam.GetComponent<CameraFollow>();
        Transform player = GameObject.Find("Player")?.transform;
        if (player != null) follow.target = player;
    }

    // ═══════════════════════════════════════════════════
    //                     MAIN MENU
    // ═══════════════════════════════════════════════════
    static void BuildMainMenu()
    {
        Camera cam = Camera.main;
        if (cam == null)
        {
            GameObject camGO = new GameObject("Main Camera");
            camGO.tag = "MainCamera";
            cam = camGO.AddComponent<Camera>();
            camGO.AddComponent<AudioListener>();
            camGO.transform.position = new Vector3(0, 0, -10);
        }
        cam.orthographic = true;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = C_BG_DARK;

        GameObject canvasGO = new GameObject("MenuCanvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasGO.AddComponent<GraphicRaycaster>();

        // Background
        GameObject bg = CreateUIImage("Background", canvasGO.transform, C_BG_DARK);
        StretchFull(bg);

        // Retro grid pattern overlay
        GameObject gridOverlay = CreateUIImage("GridOverlay", canvasGO.transform,
            new Color(0.55f, 0.35f, 0.95f, 0.08f));
        StretchFull(gridOverlay);

        // Title shadow copy (pink)
        GameObject titleShadow = CreatePixelText("TitleShadow", canvasGO.transform,
            "RECURSION\nDUNGEON",
            88, TextAnchor.MiddleCenter, new Vector2(6, 134), new Vector2(1000, 220), C_ACCENT_PINK);
        titleShadow.GetComponent<Text>().fontStyle = FontStyle.Bold;

        // Main title (yellow)
        GameObject title = CreatePixelText("Title", canvasGO.transform,
            "RECURSION\nDUNGEON",
            88, TextAnchor.MiddleCenter, new Vector2(0, 140), new Vector2(1000, 220), C_ACCENT_YELLOW);
        title.GetComponent<Text>().fontStyle = FontStyle.Bold;

        // Decorative line
        GameObject decorLine1 = CreateUIImage("DecorLine1", canvasGO.transform, C_ACCENT_CYAN);
        RectTransform dlRT = decorLine1.GetComponent<RectTransform>();
        dlRT.anchoredPosition = new Vector2(0, 20);
        dlRT.sizeDelta = new Vector2(500, 4);

        // Subtitle
        GameObject subtitle = CreatePixelText("Subtitle", canvasGO.transform,
            "SOLVE THE DEEPEST ROOM FIRST",
            26, TextAnchor.MiddleCenter, new Vector2(0, -20), new Vector2(800, 40), C_ACCENT_CYAN);

        // Code block
        GameObject codeBlock = CreatePixelPanel("CodeBlock", canvasGO.transform,
            new Vector2(0.5f, 0.5f), new Vector2(0, -90), new Vector2(600, 110),
            new Color(0.06f, 0.04f, 0.12f, 0.85f), C_ACCENT_GREEN);
        codeBlock.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0.5f);

        CreatePixelText("CodeText", codeBlock.transform,
            "solve(room) {\n   weapon = solve(room.next);\n   defeat(room.monster, weapon);\n}",
            16, TextAnchor.MiddleCenter, Vector2.zero, new Vector2(580, 100), C_ACCENT_GREEN);

        // Play button
        GameObject playBtn = CreatePixelButton("PlayButton", canvasGO.transform,
            "▶ PLAY", new Vector2(0, -220), new Vector2(280, 70),
            new Color(0.10f, 0.45f, 0.15f), C_ACCENT_GREEN);
        playBtn.GetComponentInChildren<Text>().fontSize = 34;

        // Quit button
        GameObject quitBtn = CreatePixelButton("QuitButton", canvasGO.transform,
            "QUIT", new Vector2(0, -310), new Vector2(220, 55),
            new Color(0.35f, 0.10f, 0.15f), C_ACCENT_RED);
        quitBtn.GetComponentInChildren<Text>().fontSize = 24;

        // Credit / hint bottom
        CreatePixelText("Hint", canvasGO.transform,
            "Use WASD to move · SPACE to attack · Find the path through the maze",
            14, TextAnchor.MiddleCenter, new Vector2(0, 40), new Vector2(900, 30), C_TEXT_DIM);
        RectTransform hintRT = canvasGO.transform.Find("Hint").GetComponent<RectTransform>();
        hintRT.anchorMin = new Vector2(0.5f, 0); hintRT.anchorMax = new Vector2(0.5f, 0);

        MainMenu menu = canvasGO.AddComponent<MainMenu>();
        menu.playButton = playBtn.GetComponent<Button>();
        menu.quitButton = quitBtn.GetComponent<Button>();
    }

    static void WireReferences()
    {
        GameObject prefabHolder = new GameObject("--- PREFAB TEMPLATES (hidden) ---");
        GameObject itemTemplate = new GameObject("ItemPrefab");
        itemTemplate.transform.SetParent(prefabHolder.transform);
        SpriteRenderer itemSR = itemTemplate.AddComponent<SpriteRenderer>();
        itemSR.sprite = Circle();
        itemSR.sortingOrder = 5;
        CircleCollider2D itemCol = itemTemplate.AddComponent<CircleCollider2D>();
        itemCol.isTrigger = true;
        itemCol.radius = 0.45f;
        itemTemplate.AddComponent<Item>();
        itemTemplate.transform.localScale = Vector3.one * 0.6f;

        Monster[] monsters = Object.FindObjectsByType<Monster>(FindObjectsSortMode.None);
        foreach (var m in monsters) m.rewardItemPrefab = itemTemplate;
        prefabHolder.SetActive(false);
    }

    // ═══════════════════════════════════════════════════
    //                WORLD-SPACE HELPERS
    // ═══════════════════════════════════════════════════
    static Sprite Circle() => AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");

    // Built-in Background.psd has 9-slice borders that make the center look faded.
    // We generate a pure solid white 4x4 sprite that tints cleanly to any color.
    static Sprite _solidSquare;
    static Sprite Square()
    {
        if (_solidSquare == null)
        {
            Texture2D tex = new Texture2D(4, 4, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            tex.wrapMode = TextureWrapMode.Clamp;
            Color[] pixels = new Color[16];
            for (int i = 0; i < 16; i++) pixels[i] = Color.white;
            tex.SetPixels(pixels);
            tex.Apply();
            _solidSquare = Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 4f);
            _solidSquare.name = "SolidSquareSprite";
        }
        return _solidSquare;
    }

    static GameObject AddWorldLabel(Transform parent, string text, Vector3 localPos, Color color, int fontSize)
    {
        GameObject labelGO = new GameObject("Label");
        labelGO.transform.SetParent(parent);
        labelGO.transform.localPosition = localPos;
        labelGO.transform.localRotation = Quaternion.identity;
        TextMesh tm = labelGO.AddComponent<TextMesh>();
        tm.text = text;
        tm.fontSize = fontSize;
        tm.characterSize = 0.12f;
        tm.anchor = TextAnchor.MiddleCenter;
        tm.alignment = TextAlignment.Center;
        tm.color = color;
        tm.fontStyle = FontStyle.Bold;
        labelGO.GetComponent<MeshRenderer>().sortingOrder = 15;
        return labelGO;
    }

    static GameObject CreateSprite(string name, Transform parent, Vector3 localPos,
        Vector3 scale, Color color, int sortOrder)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent);
        go.transform.localPosition = localPos;
        go.transform.localScale = scale;
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = Square();
        sr.color = color;
        sr.sortingOrder = sortOrder;
        return go;
    }

    static GameObject CreateWall(string name, Transform parent, Vector3 localPos,
        Vector3 scale, Color color)
    {
        GameObject wall = CreateSprite(name, parent, localPos, scale, color, 2);
        wall.AddComponent<BoxCollider2D>();
        return wall;
    }

    static GameObject CreateDoor(string name, Transform parent, Vector3 localPos,
        bool locked, int targetDepth, Door.DoorType type, string labelText)
    {
        GameObject door = new GameObject(name);
        door.transform.SetParent(parent);
        door.transform.localPosition = localPos;
        door.transform.localScale = new Vector3(2.8f, 0.7f, 1f);

        SpriteRenderer sr = door.AddComponent<SpriteRenderer>();
        sr.sprite = Square();
        sr.sortingOrder = 3;

        BoxCollider2D col = door.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(1f, 1.2f);

        Door doorComp = door.AddComponent<Door>();
        doorComp.doorType = type;
        doorComp.targetRoomDepth = targetDepth;
        doorComp.isLocked = locked;
        doorComp.doorRenderer = sr;
        sr.color = locked ? C_ACCENT_RED : C_ACCENT_GREEN;

        GameObject lockIcon = new GameObject("LockIndicator");
        lockIcon.transform.SetParent(door.transform);
        lockIcon.transform.localPosition = Vector3.zero;
        lockIcon.transform.localScale = new Vector3(0.13f, 0.35f, 1f);
        SpriteRenderer lockSR = lockIcon.AddComponent<SpriteRenderer>();
        lockSR.sprite = Circle();
        lockSR.color = C_ACCENT_YELLOW;
        lockSR.sortingOrder = 4;
        lockIcon.SetActive(locked);
        doorComp.lockIndicator = lockIcon;

        float labelY = (type == Door.DoorType.EnterDeeper) ? -1.1f : 1.1f;
        Color labelColor = locked ? C_ACCENT_RED : C_ACCENT_GREEN;
        AddWorldLabel(door.transform, labelText,
            new Vector3(0, labelY, 0), labelColor, 22);

        return door;
    }

    static GameObject CreateMonster(string name, Transform parent, Vector3 localPos,
        string monsterName, Color color, float size, string required, string reward, Color rewardColor)
    {
        GameObject monster = new GameObject(name);
        monster.transform.SetParent(parent);
        monster.transform.localPosition = localPos;
        monster.transform.localScale = Vector3.one * size;

        SpriteRenderer sr = monster.AddComponent<SpriteRenderer>();
        sr.sprite = Circle();
        sr.color = color;
        sr.sortingOrder = 5;

        CircleCollider2D col = monster.AddComponent<CircleCollider2D>();
        col.radius = 0.5f;

        Monster mc = monster.AddComponent<Monster>();
        mc.monsterName = monsterName;
        mc.monsterColor = color;
        mc.requiredWeapon = required;
        mc.rewardWeaponName = reward;
        mc.rewardWeaponColor = rewardColor;

        // Detailed composite art (face, horns, wings, etc.) is built at runtime.
        monster.AddComponent<MonsterVisual>();

        GameObject glow = new GameObject("GlowRing");
        glow.transform.SetParent(monster.transform);
        glow.transform.localPosition = Vector3.zero;
        glow.transform.localScale = Vector3.one * 1.5f;
        SpriteRenderer glowSR = glow.AddComponent<SpriteRenderer>();
        glowSR.sprite = Circle();
        Color gc = color; gc.a = 0.28f; glowSR.color = gc;
        glowSR.sortingOrder = 4;

        AddWorldLabel(monster.transform, monsterName,
            new Vector3(0, 0.85f, 0), Color.white, 28);
        AddWorldLabel(monster.transform, $"NEEDS: {required}",
            new Vector3(0, -0.85f, 0), C_ACCENT_YELLOW, 20);
        return monster;
    }

    static GameObject CreateItem(string name, Transform parent, Vector3 localPos,
        string weaponName, Color color, float size)
    {
        GameObject item = new GameObject(name);
        item.transform.SetParent(parent);
        item.transform.localPosition = localPos;
        item.transform.localScale = Vector3.one * size;

        SpriteRenderer sr = item.AddComponent<SpriteRenderer>();
        sr.sprite = Circle();
        sr.color = color;
        sr.sortingOrder = 5;

        CircleCollider2D col = item.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.5f;

        Item itemComp = item.AddComponent<Item>();
        itemComp.weaponName = weaponName;
        itemComp.weaponColor = color;

        // Build detailed composite weapon art (blade, guard, handle, flames...) at runtime.
        item.AddComponent<ItemVisual>();

        GameObject glow = new GameObject("Glow");
        glow.transform.SetParent(item.transform);
        glow.transform.localPosition = Vector3.zero;
        glow.transform.localScale = Vector3.one * 1.6f;
        SpriteRenderer glowSR = glow.AddComponent<SpriteRenderer>();
        glowSR.sprite = Circle();
        Color gc = color; gc.a = 0.25f; glowSR.color = gc;
        glowSR.sortingOrder = 4;

        AddWorldLabel(item.transform, weaponName,
            new Vector3(0, -0.9f, 0), C_ACCENT_YELLOW, 26);
        return item;
    }

    // ═══════════════════════════════════════════════════
    //              PIXEL-STYLE UI HELPERS
    // ═══════════════════════════════════════════════════
    static void StretchFull(GameObject go)
    {
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
    }

    /// <summary>Creates a panel with a thick pixel-style border (inner body + outer frame).</summary>
    static GameObject CreatePixelPanel(string name, Transform parent, Vector2 anchor,
        Vector2 anchoredPos, Vector2 size, Color bodyColor, Color borderColor)
    {
        GameObject outer = new GameObject(name);
        outer.transform.SetParent(parent, false);
        RectTransform rt = outer.AddComponent<RectTransform>();
        rt.anchorMin = anchor; rt.anchorMax = anchor;
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = size;
        rt.pivot = anchor;

        Image outerImg = outer.AddComponent<Image>();
        outerImg.color = borderColor;

        // Inner body (slightly smaller)
        GameObject inner = new GameObject("Body");
        inner.transform.SetParent(outer.transform, false);
        RectTransform innerRT = inner.AddComponent<RectTransform>();
        innerRT.anchorMin = Vector2.zero; innerRT.anchorMax = Vector2.one;
        innerRT.offsetMin = new Vector2(4, 4); innerRT.offsetMax = new Vector2(-4, -4);
        Image innerImg = inner.AddComponent<Image>();
        innerImg.color = bodyColor;

        // Corner "pixel" accents for retro feel
        AddCornerPixel(outer, borderColor, new Vector2(0, 0), new Vector2(10, 0));  // BL
        AddCornerPixel(outer, borderColor, new Vector2(1, 0), new Vector2(-10, 0));  // BR
        AddCornerPixel(outer, borderColor, new Vector2(0, 1), new Vector2(10, 0));   // TL
        AddCornerPixel(outer, borderColor, new Vector2(1, 1), new Vector2(-10, 0));  // TR

        return outer;
    }

    static void AddCornerPixel(GameObject parent, Color color, Vector2 anchor, Vector2 offset)
    {
        GameObject corner = new GameObject("CornerPx");
        corner.transform.SetParent(parent.transform, false);
        RectTransform rt = corner.AddComponent<RectTransform>();
        rt.anchorMin = anchor; rt.anchorMax = anchor;
        rt.pivot = anchor;
        rt.anchoredPosition = offset;
        rt.sizeDelta = new Vector2(6, 6);
        Image img = corner.AddComponent<Image>();
        img.color = color;
        img.raycastTarget = false;
    }

    /// <summary>Adds a thick border around an existing UI image.</summary>
    static void AddPixelBorder(GameObject parent, Color borderColor, int thickness)
    {
        GameObject border = new GameObject("Border");
        border.transform.SetParent(parent.transform, false);
        border.transform.SetAsFirstSibling();
        RectTransform rt = border.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = new Vector2(-thickness, -thickness);
        rt.offsetMax = new Vector2(thickness, thickness);
        Image img = border.AddComponent<Image>();
        img.color = borderColor;
        img.raycastTarget = false;
    }

    /// <summary>Pixel-style text: bold, with shadow + outline for retro feel.</summary>
    static GameObject CreatePixelText(string name, Transform parent, string content,
        int fontSize, TextAnchor alignment, Vector2 anchoredPos, Vector2 size, Color color)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = size;
        Text text = go.AddComponent<Text>();
        text.text = content;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = fontSize;
        text.fontStyle = FontStyle.Bold;
        text.color = color;
        text.alignment = alignment;
        text.raycastTarget = false;

        // Shadow for retro pixel feel
        Shadow shadow = go.AddComponent<Shadow>();
        shadow.effectColor = C_TEXT_SHADOW;
        shadow.effectDistance = new Vector2(3, -3);

        // Outline to simulate pixel chunkiness
        Outline outline = go.AddComponent<Outline>();
        outline.effectColor = new Color(0f, 0f, 0f, 0.85f);
        outline.effectDistance = new Vector2(1.5f, 1.5f);

        return go;
    }

    static GameObject CreateUIImage(string name, Transform parent, Color color)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.AddComponent<RectTransform>();
        Image img = go.AddComponent<Image>();
        img.color = color;
        return go;
    }

    /// <summary>Chunky retro button with thick border and color accent.</summary>
    static GameObject CreatePixelButton(string name, Transform parent, string label,
        Vector2 anchoredPos, Vector2 size, Color bodyColor, Color borderColor)
    {
        GameObject outer = new GameObject(name);
        outer.transform.SetParent(parent, false);
        RectTransform rt = outer.AddComponent<RectTransform>();
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = size;

        Image outerImg = outer.AddComponent<Image>();
        outerImg.color = borderColor;

        Button btn = outer.AddComponent<Button>();
        ColorBlock cb = btn.colors;
        cb.normalColor = Color.white;
        cb.highlightedColor = new Color(1.1f, 1.1f, 1.1f);
        cb.pressedColor = new Color(0.7f, 0.7f, 0.7f);
        cb.selectedColor = Color.white;
        btn.colors = cb;
        btn.targetGraphic = outerImg;

        // Inner body
        GameObject inner = new GameObject("Body");
        inner.transform.SetParent(outer.transform, false);
        RectTransform innerRT = inner.AddComponent<RectTransform>();
        innerRT.anchorMin = Vector2.zero; innerRT.anchorMax = Vector2.one;
        innerRT.offsetMin = new Vector2(4, 4); innerRT.offsetMax = new Vector2(-4, -4);
        Image innerImg = inner.AddComponent<Image>();
        innerImg.color = bodyColor;
        innerImg.raycastTarget = false;

        // Highlight stripe (top)
        GameObject highlight = new GameObject("Highlight");
        highlight.transform.SetParent(outer.transform, false);
        RectTransform hRT = highlight.AddComponent<RectTransform>();
        hRT.anchorMin = new Vector2(0, 1); hRT.anchorMax = new Vector2(1, 1);
        hRT.pivot = new Vector2(0.5f, 1);
        hRT.offsetMin = new Vector2(4, -10); hRT.offsetMax = new Vector2(-4, -4);
        Image hImg = highlight.AddComponent<Image>();
        hImg.color = new Color(1f, 1f, 1f, 0.15f);
        hImg.raycastTarget = false;

        // Text
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(outer.transform, false);
        RectTransform textRT = textObj.AddComponent<RectTransform>();
        textRT.anchorMin = Vector2.zero; textRT.anchorMax = Vector2.one;
        textRT.offsetMin = Vector2.zero; textRT.offsetMax = Vector2.zero;
        Text text = textObj.AddComponent<Text>();
        text.text = label;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = 24;
        text.fontStyle = FontStyle.Bold;
        text.color = Color.white;
        text.alignment = TextAnchor.MiddleCenter;
        text.raycastTarget = false;

        Shadow sh = textObj.AddComponent<Shadow>();
        sh.effectColor = new Color(0, 0, 0, 0.9f);
        sh.effectDistance = new Vector2(2, -2);

        return outer;
    }

    static GameObject CreateCallStackFrame(Transform parent)
    {
        GameObject frame = new GameObject("FramePrefab");
        frame.transform.SetParent(parent, false);
        RectTransform rt = frame.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(300, 75);

        Image bg = frame.AddComponent<Image>();
        bg.color = new Color(0.15f, 0.10f, 0.25f, 0.95f);

        LayoutElement le = frame.AddComponent<LayoutElement>();
        le.preferredHeight = 75; le.minHeight = 75;

        // Body (inner)
        GameObject body = new GameObject("Body");
        body.transform.SetParent(frame.transform, false);
        RectTransform bodyRT = body.AddComponent<RectTransform>();
        bodyRT.anchorMin = Vector2.zero; bodyRT.anchorMax = Vector2.one;
        bodyRT.offsetMin = new Vector2(3, 3); bodyRT.offsetMax = new Vector2(-3, -3);
        Image bodyImg = body.AddComponent<Image>();
        bodyImg.color = new Color(0.07f, 0.04f, 0.15f, 0.95f);
        bodyImg.raycastTarget = false;

        // Depth badge (left)
        GameObject badge = new GameObject("DepthBadge");
        badge.transform.SetParent(body.transform, false);
        RectTransform bRT = badge.AddComponent<RectTransform>();
        bRT.anchorMin = new Vector2(0, 0); bRT.anchorMax = new Vector2(0, 1);
        bRT.pivot = new Vector2(0, 0.5f);
        bRT.offsetMin = new Vector2(4, 4); bRT.offsetMax = new Vector2(44, -4);
        Image badgeImg = badge.AddComponent<Image>();
        badgeImg.color = C_ACCENT_PINK;
        badgeImg.raycastTarget = false;

        // Text
        GameObject textObj = new GameObject("FrameText");
        textObj.transform.SetParent(body.transform, false);
        RectTransform textRT = textObj.AddComponent<RectTransform>();
        textRT.anchorMin = Vector2.zero; textRT.anchorMax = Vector2.one;
        textRT.offsetMin = new Vector2(52, 6); textRT.offsetMax = new Vector2(-8, -6);
        Text text = textObj.AddComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = 14;
        text.fontStyle = FontStyle.Bold;
        text.color = Color.white;
        text.alignment = TextAnchor.MiddleLeft;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Truncate;
        text.lineSpacing = 1.2f;
        text.raycastTarget = false;

        Shadow sh = textObj.AddComponent<Shadow>();
        sh.effectColor = new Color(0, 0, 0, 0.9f);
        sh.effectDistance = new Vector2(2, -2);

        return frame;
    }
}
