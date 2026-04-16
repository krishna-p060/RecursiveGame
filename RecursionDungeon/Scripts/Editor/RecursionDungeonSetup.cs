using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

/// <summary>
/// One-click automated scene builder for Recursion Dungeon.
/// Use via the menu: Tools > Recursion Dungeon > Build Game Scene / Build Main Menu.
/// </summary>
public class RecursionDungeonSetup : EditorWindow
{
    [MenuItem("Tools/Recursion Dungeon/Build FULL Game Scene")]
    static void BuildGameScene()
    {
        if (!EditorUtility.DisplayDialog("Build Game Scene",
            "This will create all GameObjects for the game scene in the current scene. Continue?",
            "Yes", "Cancel"))
            return;

        BuildPlayer();
        BuildRooms();
        BuildManagers();
        BuildCanvas();
        SetupCamera();
        WireReferences();

        Debug.Log("=== Recursion Dungeon Game Scene built! Check hierarchy. ===");
    }

    [MenuItem("Tools/Recursion Dungeon/Build Main Menu Scene")]
    static void BuildMainMenuScene()
    {
        if (!EditorUtility.DisplayDialog("Build Main Menu",
            "This will create all GameObjects for the Main Menu. Continue?",
            "Yes", "Cancel"))
            return;

        BuildMainMenu();
        Debug.Log("=== Main Menu built! Save this as 'MainMenu' scene. ===");
    }

    // ─────────────────── PLAYER ───────────────────
    static GameObject BuildPlayer()
    {
        GameObject player = new GameObject("Player");
        player.tag = "Player";
        player.layer = LayerMask.NameToLayer("Default");

        SpriteRenderer sr = player.AddComponent<SpriteRenderer>();
        sr.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
        sr.color = new Color(0.2f, 0.7f, 1f);
        sr.sortingOrder = 10;
        player.transform.localScale = Vector3.one * 0.5f;

        Rigidbody2D rb = player.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;

        CircleCollider2D col = player.AddComponent<CircleCollider2D>();
        col.radius = 0.5f;

        player.AddComponent<PlayerController>();
        player.AddComponent<PlayerInventory>();

        PlayerCombat combat = player.AddComponent<PlayerCombat>();
        combat.monsterLayer = LayerMask.GetMask("Default");

        // Weapon icon child
        GameObject weaponIcon = new GameObject("WeaponIcon");
        weaponIcon.transform.SetParent(player.transform);
        weaponIcon.transform.localPosition = new Vector3(0.8f, 0f, 0f);
        weaponIcon.transform.localScale = Vector3.one * 0.5f;
        SpriteRenderer weaponSR = weaponIcon.AddComponent<SpriteRenderer>();
        weaponSR.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
        weaponSR.color = Color.gray;
        weaponSR.sortingOrder = 11;
        weaponSR.enabled = false;

        PlayerInventory inv = player.GetComponent<PlayerInventory>();
        inv.weaponIconRenderer = weaponSR;

        return player;
    }

    // ─────────────────── ROOMS ───────────────────
    static void BuildRooms()
    {
        GameObject roomsParent = new GameObject("Rooms");

        string[] roomNames = { "Room0_GrandHall", "Room1_DarkCave", "Room2_Crypt", "Room3_TinyChamber" };
        float[] roomSizes = { 10f, 8.5f, 7f, 5.5f };
        Color[] floorColors = {
            new Color(0.35f, 0.30f, 0.25f),
            new Color(0.25f, 0.22f, 0.20f),
            new Color(0.18f, 0.15f, 0.18f),
            new Color(0.12f, 0.10f, 0.12f)
        };

        string[] monsterNames = { "Dragon", "Wolf", "Bat", "" };
        Color[] monsterColors = {
            new Color(0.9f, 0.15f, 0.1f),
            new Color(0.5f, 0.5f, 0.5f),
            new Color(0.6f, 0.2f, 0.8f),
            Color.clear
        };
        string[] requiredWeapons = { "Fire Sword", "Bow", "Dagger", "" };
        string[] rewardWeapons = { "", "Fire Sword", "Bow", "" };
        Color[] rewardColors = { Color.clear, new Color(1f, 0.4f, 0f), new Color(0.6f, 0.3f, 0.1f), Color.clear };

        string basePickup = "Dagger";
        Color daggerColor = new Color(0.7f, 0.7f, 0.8f);

        for (int i = 0; i < 4; i++)
        {
            GameObject room = new GameObject(roomNames[i]);
            room.transform.SetParent(roomsParent.transform);
            room.transform.position = Vector3.zero;

            // Floor
            GameObject floor = CreateSprite($"Floor_{i}", room.transform, Vector3.zero,
                new Vector3(roomSizes[i], roomSizes[i], 1), floorColors[i], 0);

            // Walls (4 sides)
            float half = roomSizes[i] / 2f;
            float wallThickness = 0.3f;
            Color wallColor = new Color(0.3f, 0.25f, 0.2f);

            CreateWall($"WallTop_{i}", room.transform, new Vector3(0, half, 0),
                new Vector3(roomSizes[i] + wallThickness, wallThickness, 1), wallColor);
            CreateWall($"WallBot_{i}", room.transform, new Vector3(0, -half, 0),
                new Vector3(roomSizes[i] + wallThickness, wallThickness, 1), wallColor);
            CreateWall($"WallLeft_{i}", room.transform, new Vector3(-half, 0, 0),
                new Vector3(wallThickness, roomSizes[i], 1), wallColor);
            CreateWall($"WallRight_{i}", room.transform, new Vector3(half, 0, 0),
                new Vector3(wallThickness, roomSizes[i], 1), wallColor);

            // Entry point
            GameObject entry = new GameObject($"EntryPoint_{i}");
            entry.transform.SetParent(room.transform);
            entry.transform.localPosition = new Vector3(0, -half + 1.5f, 0);

            // Enter-deeper door (top of room) — not for room 3
            if (i < 3)
            {
                GameObject enterDoor = CreateDoor($"DoorDeeper_{i}", room.transform,
                    new Vector3(0, half - 0.5f, 0), false, i + 1, Door.DoorType.EnterDeeper);
            }

            // Exit-to-parent door (bottom) — not for room 0
            if (i > 0)
            {
                bool exitLocked = (i < 3 && !string.IsNullOrEmpty(monsterNames[i]));
                GameObject exitDoor = CreateDoor($"DoorExit_{i}", room.transform,
                    new Vector3(0, -half + 0.5f, 0), exitLocked, i - 1, Door.DoorType.ExitToParent);
            }

            // Room 0 special: Victory exit door (bottom, locked until dragon defeated)
            if (i == 0)
            {
                GameObject victoryDoor = CreateDoor("DoorVictoryExit", room.transform,
                    new Vector3(0, -half + 0.5f, 0), true, 0, Door.DoorType.ExitToParent);
            }

            // Monster (except room 3)
            if (i < 3)
            {
                GameObject monster = CreateMonster($"Monster_{monsterNames[i]}", room.transform,
                    new Vector3(0, 1.5f, 0), monsterNames[i], monsterColors[i],
                    requiredWeapons[i], rewardWeapons[i], rewardColors[i]);

                // Wire exit door to monster
                Monster monsterComp = monster.GetComponent<Monster>();
                Door exitDoorComp = null;
                if (i == 0)
                    exitDoorComp = room.transform.Find("DoorVictoryExit")?.GetComponent<Door>();
                else
                    exitDoorComp = room.transform.Find($"DoorExit_{i}")?.GetComponent<Door>();

                if (monsterComp != null && exitDoorComp != null)
                    monsterComp.exitDoor = exitDoorComp;
            }

            // Base case item (room 3 only)
            if (i == 3)
            {
                CreateItem("Item_Dagger", room.transform, new Vector3(0, 0.5f, 0),
                    basePickup, daggerColor);
            }

            // Deactivate all rooms except first
            room.SetActive(i == 0);
        }
    }

    // ─────────────────── MANAGERS ───────────────────
    static void BuildManagers()
    {
        GameObject managers = new GameObject("--- MANAGERS ---");

        GameObject gm = new GameObject("GameManager");
        gm.transform.SetParent(managers.transform);
        gm.AddComponent<GameManager>();

        GameObject rm = new GameObject("RoomManager");
        rm.transform.SetParent(managers.transform);
        RoomManager roomMgr = rm.AddComponent<RoomManager>();

        // Wire rooms
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

        if (player != null)
            roomMgr.player = player;
    }

    // ─────────────────── CANVAS ───────────────────
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

        // ── Fade Image (fullscreen) ──
        GameObject fadeObj = CreateUIImage("FadeImage", canvasGO.transform, Color.black);
        RectTransform fadeRT = fadeObj.GetComponent<RectTransform>();
        fadeRT.anchorMin = Vector2.zero; fadeRT.anchorMax = Vector2.one;
        fadeRT.offsetMin = Vector2.zero; fadeRT.offsetMax = Vector2.zero;
        Image fadeImg = fadeObj.GetComponent<Image>();
        fadeImg.color = new Color(0, 0, 0, 0);
        fadeImg.raycastTarget = false;
        camTrans.fadeImage = fadeImg;

        // ── HUD Top Left Panel ──
        GameObject hudLeft = CreateUIPanel("HUD_TopLeft", canvasGO.transform,
            new Vector2(0, 1), new Vector2(0, 1), new Vector2(10, -10),
            new Vector2(300, 80), new Color(0, 0, 0, 0.6f));

        GameObject depthText = CreateUIText("DepthText", hudLeft.transform, "Depth: 0",
            20, TextAnchor.UpperLeft, new Vector2(10, -5), new Vector2(280, 35));
        GameObject roomText = CreateUIText("RoomNameText", hudLeft.transform, "Grand Hall",
            16, TextAnchor.UpperLeft, new Vector2(10, -35), new Vector2(280, 35));

        uiMgr.depthText = depthText.GetComponent<Text>();
        uiMgr.roomNameText = roomText.GetComponent<Text>();

        // ── HUD Top Right Panel ──
        GameObject hudRight = CreateUIPanel("HUD_TopRight", canvasGO.transform,
            new Vector2(1, 1), new Vector2(1, 1), new Vector2(-10, -10),
            new Vector2(280, 60), new Color(0, 0, 0, 0.6f));

        GameObject weaponText = CreateUIText("WeaponText", hudRight.transform, "Weapon: None",
            18, TextAnchor.UpperRight, new Vector2(-10, -10), new Vector2(260, 40));

        GameObject weaponIcon = CreateUIImage("WeaponIcon", hudRight.transform, Color.gray);
        RectTransform weaponIconRT = weaponIcon.GetComponent<RectTransform>();
        weaponIconRT.anchorMin = new Vector2(0, 0.5f); weaponIconRT.anchorMax = new Vector2(0, 0.5f);
        weaponIconRT.anchoredPosition = new Vector2(25, 0);
        weaponIconRT.sizeDelta = new Vector2(30, 30);

        uiMgr.weaponNameText = weaponText.GetComponent<Text>();
        uiMgr.weaponIconImage = weaponIcon.GetComponent<Image>();

        // ── Timer ──
        GameObject timerObj = CreateUIText("TimerText", canvasGO.transform, "00:00",
            18, TextAnchor.UpperCenter, new Vector2(0, -15), new Vector2(120, 40));
        RectTransform timerRT = timerObj.GetComponent<RectTransform>();
        timerRT.anchorMin = new Vector2(0.5f, 1); timerRT.anchorMax = new Vector2(0.5f, 1);
        uiMgr.timerText = timerObj.GetComponent<Text>();

        // ── Dialog Panel (center bottom) ──
        GameObject dialogPanel = CreateUIPanel("DialogPanel", canvasGO.transform,
            new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 80),
            new Vector2(500, 60), new Color(0, 0, 0, 0.8f));

        GameObject dialogText = CreateUIText("DialogText", dialogPanel.transform, "",
            20, TextAnchor.MiddleCenter, Vector2.zero, new Vector2(480, 50));
        RectTransform dlgTextRT = dialogText.GetComponent<RectTransform>();
        dlgTextRT.anchorMin = Vector2.zero; dlgTextRT.anchorMax = Vector2.one;
        dlgTextRT.offsetMin = new Vector2(10, 5); dlgTextRT.offsetMax = new Vector2(-10, -5);

        uiMgr.dialogPanel = dialogPanel;
        uiMgr.dialogText = dialogText.GetComponent<Text>();
        dialogPanel.SetActive(false);

        // ── Call Stack UI (left side) ──
        GameObject callStackPanel = CreateUIPanel("CallStackPanel", canvasGO.transform,
            new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(10, 0),
            new Vector2(280, 400), new Color(0, 0, 0, 0f));

        // Title
        CreateUIText("CallStackTitle", callStackPanel.transform, "CALL STACK",
            16, TextAnchor.UpperCenter, new Vector2(0, -5), new Vector2(260, 30));

        // Stack container with vertical layout
        GameObject stackContainer = new GameObject("StackContainer");
        stackContainer.transform.SetParent(callStackPanel.transform, false);
        RectTransform scRT = stackContainer.AddComponent<RectTransform>();
        scRT.anchorMin = new Vector2(0, 0); scRT.anchorMax = new Vector2(1, 1);
        scRT.offsetMin = new Vector2(5, 5); scRT.offsetMax = new Vector2(-5, -35);
        VerticalLayoutGroup vlg = stackContainer.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 5;
        vlg.childAlignment = TextAnchor.LowerLeft;
        vlg.childControlWidth = true;
        vlg.childControlHeight = false;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;
        ContentSizeFitter csf = stackContainer.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        CallStackUI callStackUI = canvasGO.AddComponent<CallStackUI>();
        callStackUI.stackContainer = stackContainer.transform;

        // Create frame prefab template
        GameObject framePrefab = CreateCallStackFramePrefab(canvasGO.transform);
        callStackUI.framePrefab = framePrefab;
        framePrefab.SetActive(false);

        // ── Victory Panel ──
        GameObject victoryPanel = CreateUIPanel("VictoryPanel", canvasGO.transform,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero,
            new Vector2(500, 350), new Color(0.05f, 0.05f, 0.15f, 0.95f));

        CreateUIText("VictoryTitle", victoryPanel.transform, "You Escaped the Dungeon!",
            32, TextAnchor.MiddleCenter, new Vector2(0, 80), new Vector2(480, 50));

        GameObject victoryTime = CreateUIText("VictoryTime", victoryPanel.transform, "Time: 00:00",
            22, TextAnchor.MiddleCenter, new Vector2(0, 20), new Vector2(480, 40));

        GameObject playAgainBtn = CreateUIButton("PlayAgainButton", victoryPanel.transform,
            "Play Again", new Vector2(-80, -60), new Vector2(150, 45),
            new Color(0.2f, 0.6f, 0.2f));

        GameObject mainMenuBtn = CreateUIButton("MainMenuButton", victoryPanel.transform,
            "Main Menu", new Vector2(80, -60), new Vector2(150, 45),
            new Color(0.6f, 0.2f, 0.2f));

        uiMgr.victoryPanel = victoryPanel;
        uiMgr.victoryTimeText = victoryTime.GetComponent<Text>();
        uiMgr.playAgainButton = playAgainBtn.GetComponent<Button>();
        uiMgr.mainMenuButton = mainMenuBtn.GetComponent<Button>();
        victoryPanel.SetActive(false);
    }

    // ─────────────────── CAMERA ───────────────────
    static void SetupCamera()
    {
        Camera cam = Camera.main;
        if (cam != null)
        {
            cam.orthographic = true;
            cam.orthographicSize = 6f;
            cam.backgroundColor = new Color(0.05f, 0.05f, 0.08f);
            CameraFollow follow = cam.gameObject.AddComponent<CameraFollow>();
            Transform player = GameObject.Find("Player")?.transform;
            if (player != null) follow.target = player;
        }
    }

    // ─────────────────── MAIN MENU ───────────────────
    static void BuildMainMenu()
    {
        Camera cam = Camera.main;
        if (cam != null)
        {
            cam.orthographic = true;
            cam.backgroundColor = new Color(0.05f, 0.03f, 0.1f);
        }

        GameObject canvasGO = new GameObject("MenuCanvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasGO.AddComponent<GraphicRaycaster>();

        // Background panel
        GameObject bg = CreateUIImage("Background", canvasGO.transform, new Color(0.05f, 0.03f, 0.1f));
        RectTransform bgRT = bg.GetComponent<RectTransform>();
        bgRT.anchorMin = Vector2.zero; bgRT.anchorMax = Vector2.one;
        bgRT.offsetMin = Vector2.zero; bgRT.offsetMax = Vector2.zero;

        // Title
        CreateUIText("Title", canvasGO.transform, "RECURSION DUNGEON",
            52, TextAnchor.MiddleCenter, new Vector2(0, 120), new Vector2(800, 80));

        // Subtitle
        GameObject subtitle = CreateUIText("Subtitle", canvasGO.transform,
            "Solve the deepest room first.",
            22, TextAnchor.MiddleCenter, new Vector2(0, 50), new Vector2(600, 40));
        subtitle.GetComponent<Text>().color = new Color(0.6f, 0.6f, 0.8f);

        // Decorative recursion hint
        GameObject hint = CreateUIText("Hint", canvasGO.transform,
            "f(dungeon) = defeat( f(dungeon - 1) )",
            16, TextAnchor.MiddleCenter, new Vector2(0, 10), new Vector2(600, 30));
        hint.GetComponent<Text>().color = new Color(0.4f, 0.8f, 0.4f);
        hint.GetComponent<Text>().fontStyle = FontStyle.Italic;

        // Play button
        GameObject playBtn = CreateUIButton("PlayButton", canvasGO.transform,
            "PLAY", new Vector2(0, -60), new Vector2(220, 55),
            new Color(0.15f, 0.4f, 0.15f));
        playBtn.GetComponentInChildren<Text>().fontSize = 28;

        // Quit button
        GameObject quitBtn = CreateUIButton("QuitButton", canvasGO.transform,
            "QUIT", new Vector2(0, -130), new Vector2(180, 45),
            new Color(0.4f, 0.15f, 0.15f));

        // MainMenu script
        MainMenu menu = canvasGO.AddComponent<MainMenu>();
        menu.playButton = playBtn.GetComponent<Button>();
        menu.quitButton = quitBtn.GetComponent<Button>();
    }

    // ─────────────────── WIRE REFERENCES ───────────────────
    static void WireReferences()
    {
        // Wire item prefab to monsters
        // Since we can't easily create a prefab at edit time without saving,
        // we create a template item under a hidden parent
        GameObject prefabHolder = new GameObject("--- PREFAB TEMPLATES (hidden) ---");

        GameObject itemTemplate = new GameObject("ItemPrefab");
        itemTemplate.transform.SetParent(prefabHolder.transform);
        SpriteRenderer itemSR = itemTemplate.AddComponent<SpriteRenderer>();
        itemSR.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
        itemSR.sortingOrder = 5;
        CircleCollider2D itemCol = itemTemplate.AddComponent<CircleCollider2D>();
        itemCol.isTrigger = true;
        itemCol.radius = 0.4f;
        itemTemplate.AddComponent<Item>();
        itemTemplate.transform.localScale = Vector3.one * 0.35f;

        // Set item template as reward prefab on all monsters
        Monster[] monsters = Object.FindObjectsOfType<Monster>();
        foreach (var m in monsters)
            m.rewardItemPrefab = itemTemplate;

        prefabHolder.SetActive(false);

        Debug.Log("References wired. After building, drag the ItemPrefab to Assets/Prefabs to make it a real prefab.");
    }

    // ═══════════════════ HELPER METHODS ═══════════════════

    static GameObject CreateSprite(string name, Transform parent, Vector3 localPos, Vector3 scale, Color color, int sortOrder)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent);
        go.transform.localPosition = localPos;
        go.transform.localScale = scale;
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
        sr.color = color;
        sr.sortingOrder = sortOrder;
        return go;
    }

    static GameObject CreateWall(string name, Transform parent, Vector3 localPos, Vector3 scale, Color color)
    {
        GameObject wall = CreateSprite(name, parent, localPos, scale, color, 2);
        BoxCollider2D col = wall.AddComponent<BoxCollider2D>();
        return wall;
    }

    static GameObject CreateDoor(string name, Transform parent, Vector3 localPos, bool locked, int targetDepth, Door.DoorType type)
    {
        GameObject door = new GameObject(name);
        door.transform.SetParent(parent);
        door.transform.localPosition = localPos;
        door.transform.localScale = new Vector3(1.2f, 0.4f, 1f);

        SpriteRenderer sr = door.AddComponent<SpriteRenderer>();
        sr.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
        sr.sortingOrder = 3;

        BoxCollider2D col = door.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(1f, 1.5f);

        Door doorComp = door.AddComponent<Door>();
        doorComp.doorType = type;
        doorComp.targetRoomDepth = targetDepth;
        doorComp.isLocked = locked;
        doorComp.doorRenderer = sr;

        sr.color = locked ? new Color(0.8f, 0.2f, 0.2f) : new Color(0.2f, 0.8f, 0.2f);

        // Lock indicator
        GameObject lockIcon = new GameObject("LockIndicator");
        lockIcon.transform.SetParent(door.transform);
        lockIcon.transform.localPosition = Vector3.zero;
        lockIcon.transform.localScale = Vector3.one * 0.3f;
        SpriteRenderer lockSR = lockIcon.AddComponent<SpriteRenderer>();
        lockSR.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
        lockSR.color = Color.yellow;
        lockSR.sortingOrder = 4;
        lockIcon.SetActive(locked);
        doorComp.lockIndicator = lockIcon;

        return door;
    }

    static GameObject CreateMonster(string name, Transform parent, Vector3 localPos,
        string monsterName, Color color, string required, string reward, Color rewardColor)
    {
        GameObject monster = new GameObject(name);
        monster.transform.SetParent(parent);
        monster.transform.localPosition = localPos;
        monster.transform.localScale = Vector3.one * 0.7f;

        SpriteRenderer sr = monster.AddComponent<SpriteRenderer>();
        sr.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
        sr.color = color;
        sr.sortingOrder = 5;

        CircleCollider2D col = monster.AddComponent<CircleCollider2D>();
        col.radius = 0.5f;

        Monster monsterComp = monster.AddComponent<Monster>();
        monsterComp.monsterName = monsterName;
        monsterComp.monsterColor = color;
        monsterComp.requiredWeapon = required;
        monsterComp.rewardWeaponName = reward;
        monsterComp.rewardWeaponColor = rewardColor;

        return monster;
    }

    static GameObject CreateItem(string name, Transform parent, Vector3 localPos,
        string weaponName, Color color)
    {
        GameObject item = new GameObject(name);
        item.transform.SetParent(parent);
        item.transform.localPosition = localPos;
        item.transform.localScale = Vector3.one * 0.35f;

        SpriteRenderer sr = item.AddComponent<SpriteRenderer>();
        sr.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
        sr.color = color;
        sr.sortingOrder = 5;

        CircleCollider2D col = item.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.5f;

        Item itemComp = item.AddComponent<Item>();
        itemComp.weaponName = weaponName;
        itemComp.weaponColor = color;

        return item;
    }

    static GameObject CreateCallStackFramePrefab(Transform parent)
    {
        GameObject frame = new GameObject("FramePrefab");
        frame.transform.SetParent(parent, false);

        RectTransform rt = frame.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(270, 60);

        Image bg = frame.AddComponent<Image>();
        bg.color = new Color(0.15f, 0.15f, 0.25f, 0.9f);

        LayoutElement le = frame.AddComponent<LayoutElement>();
        le.preferredHeight = 60;
        le.minHeight = 60;

        GameObject textObj = new GameObject("FrameText");
        textObj.transform.SetParent(frame.transform, false);
        RectTransform textRT = textObj.AddComponent<RectTransform>();
        textRT.anchorMin = Vector2.zero; textRT.anchorMax = Vector2.one;
        textRT.offsetMin = new Vector2(8, 4); textRT.offsetMax = new Vector2(-8, -4);

        Text text = textObj.AddComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = 13;
        text.color = Color.white;
        text.alignment = TextAnchor.MiddleLeft;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Truncate;

        return frame;
    }

    // ── UI Helpers ──

    static GameObject CreateUIPanel(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax,
        Vector2 anchoredPos, Vector2 size, Color color)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = anchorMin; rt.anchorMax = anchorMax;
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = size;
        rt.pivot = anchorMin;

        Image img = go.AddComponent<Image>();
        img.color = color;
        return go;
    }

    static GameObject CreateUIText(string name, Transform parent, string content,
        int fontSize, TextAnchor alignment, Vector2 anchoredPos, Vector2 size)
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
        text.color = Color.white;
        text.alignment = alignment;
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

    static GameObject CreateUIButton(string name, Transform parent, string label,
        Vector2 anchoredPos, Vector2 size, Color bgColor)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = size;

        Image img = go.AddComponent<Image>();
        img.color = bgColor;

        Button btn = go.AddComponent<Button>();
        ColorBlock cb = btn.colors;
        cb.highlightedColor = bgColor * 1.3f;
        cb.pressedColor = bgColor * 0.7f;
        btn.colors = cb;

        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(go.transform, false);
        RectTransform textRT = textObj.AddComponent<RectTransform>();
        textRT.anchorMin = Vector2.zero; textRT.anchorMax = Vector2.one;
        textRT.offsetMin = Vector2.zero; textRT.offsetMax = Vector2.zero;

        Text text = textObj.AddComponent<Text>();
        text.text = label;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = 20;
        text.color = Color.white;
        text.alignment = TextAnchor.MiddleCenter;

        return go;
    }
}
