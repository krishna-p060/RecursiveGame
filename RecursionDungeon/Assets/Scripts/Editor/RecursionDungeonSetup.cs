using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

public class RecursionDungeonSetup : EditorWindow
{
    [MenuItem("Tools/Recursion Dungeon/Build FULL Game Scene")]
    static void BuildGameScene()
    {
        if (!EditorUtility.DisplayDialog("Build Game Scene",
            "This will create all GameObjects for the game scene. Continue?",
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
            "This will create Main Menu GameObjects. Continue?",
            "Yes", "Cancel"))
            return;

        BuildMainMenu();
        Debug.Log("=== Main Menu built! Save as 'MainMenu' scene. ===");
    }

    [MenuItem("Tools/Recursion Dungeon/Configure Build Settings (add both scenes)")]
    static void ConfigureBuildSettings()
    {
        string[] sceneGuids = AssetDatabase.FindAssets("t:Scene");
        string mainMenuPath = null;
        string gameScenePath = null;

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
                $"Could not find both scenes:\nMainMenu: {(mainMenuPath ?? "NOT FOUND")}\nGameScene: {(gameScenePath ?? "NOT FOUND")}\n\nMake sure you've saved both scenes in Assets/Scenes/",
                "OK");
            return;
        }

        var scenes = new System.Collections.Generic.List<EditorBuildSettingsScene>
        {
            new EditorBuildSettingsScene(mainMenuPath, true),
            new EditorBuildSettingsScene(gameScenePath, true)
        };

        EditorBuildSettings.scenes = scenes.ToArray();

        Debug.Log($"Build Settings configured:\n  0: {mainMenuPath}\n  1: {gameScenePath}");
        EditorUtility.DisplayDialog("Build Settings Configured",
            $"Scenes added to Build Settings:\n\n0: MainMenu\n1: GameScene\n\nThe Play Again and Main Menu buttons will now work!",
            "OK");
    }

    // ═══════════════════════════════════════════════════
    //                     PLAYER
    // ═══════════════════════════════════════════════════
    static GameObject BuildPlayer()
    {
        GameObject player = new GameObject("Player");
        player.tag = "Player";

        SpriteRenderer sr = player.AddComponent<SpriteRenderer>();
        sr.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
        sr.color = new Color(0.3f, 0.85f, 1f);
        sr.sortingOrder = 10;
        player.transform.localScale = Vector3.one * 0.8f;

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

        // Direction arrow child
        GameObject arrow = new GameObject("DirectionArrow");
        arrow.transform.SetParent(player.transform);
        arrow.transform.localPosition = new Vector3(0, 0.6f, 0);
        arrow.transform.localScale = new Vector3(0.3f, 0.4f, 1f);
        SpriteRenderer arrowSR = arrow.AddComponent<SpriteRenderer>();
        arrowSR.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
        arrowSR.color = Color.white;
        arrowSR.sortingOrder = 11;

        // Weapon icon child
        GameObject weaponIcon = new GameObject("WeaponIcon");
        weaponIcon.transform.SetParent(player.transform);
        weaponIcon.transform.localPosition = new Vector3(0.7f, -0.3f, 0);
        weaponIcon.transform.localScale = Vector3.one * 0.5f;
        SpriteRenderer weaponSR = weaponIcon.AddComponent<SpriteRenderer>();
        weaponSR.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
        weaponSR.color = Color.gray;
        weaponSR.sortingOrder = 12;
        weaponSR.enabled = false;

        player.GetComponent<PlayerInventory>().weaponIconRenderer = weaponSR;

        // "YOU" label
        AddWorldLabel(player.transform, "YOU", new Vector3(0, -0.7f, 0),
            new Color(0.3f, 0.85f, 1f), 28);

        return player;
    }

    // ═══════════════════════════════════════════════════
    //                     ROOMS
    // ═══════════════════════════════════════════════════
    static void BuildRooms()
    {
        GameObject roomsParent = new GameObject("Rooms");

        string[] roomNames = { "Room0_GrandHall", "Room1_DarkCave", "Room2_Crypt", "Room3_TinyChamber" };
        float[] roomSizes = { 12f, 10.5f, 9f, 7.5f };

        // Brighter, more distinct floor colors
        Color[] floorColors = {
            new Color(0.28f, 0.22f, 0.18f),
            new Color(0.18f, 0.18f, 0.22f),
            new Color(0.20f, 0.14f, 0.22f),
            new Color(0.14f, 0.12f, 0.16f)
        };

        // Bright, distinct wall colors per room
        Color[] wallColors = {
            new Color(0.50f, 0.35f, 0.20f),
            new Color(0.30f, 0.30f, 0.40f),
            new Color(0.40f, 0.25f, 0.45f),
            new Color(0.35f, 0.30f, 0.30f)
        };

        string[] monsterNames = { "Dragon", "Wolf", "Bat", "" };
        Color[] monsterColors = {
            new Color(1f, 0.2f, 0.1f),       // bright red dragon
            new Color(0.6f, 0.6f, 0.65f),    // silver wolf
            new Color(0.75f, 0.3f, 1f),      // bright purple bat
            Color.clear
        };
        float[] monsterSizes = { 1.8f, 1.4f, 1.2f, 0f };

        string[] requiredWeapons = { "Fire Sword", "Bow", "Dagger", "" };
        string[] rewardWeapons = { "", "Fire Sword", "Bow", "" };
        Color[] rewardColors = {
            Color.clear,
            new Color(1f, 0.5f, 0.05f),      // orange fire sword
            new Color(0.5f, 0.3f, 0.1f),     // brown bow
            Color.clear
        };

        string basePickup = "Dagger";
        Color daggerColor = new Color(0.8f, 0.85f, 1f);

        for (int i = 0; i < 4; i++)
        {
            GameObject room = new GameObject(roomNames[i]);
            room.transform.SetParent(roomsParent.transform);
            room.transform.position = Vector3.zero;

            float size = roomSizes[i];
            float half = size / 2f;

            // ── Floor ──
            CreateSprite($"Floor_{i}", room.transform, Vector3.zero,
                new Vector3(size, size, 1), floorColors[i], 0);

            // ── Floor grid lines for visual depth ──
            for (int g = -((int)half) + 1; g < (int)half; g++)
            {
                Color gridColor = floorColors[i] * 1.15f;
                gridColor.a = 0.3f;
                CreateSprite($"GridH_{g}", room.transform, new Vector3(0, g, 0),
                    new Vector3(size - 0.6f, 0.02f, 1), gridColor, 1);
                CreateSprite($"GridV_{g}", room.transform, new Vector3(g, 0, 0),
                    new Vector3(0.02f, size - 0.6f, 1), gridColor, 1);
            }

            // ── Walls (thick, visible) ──
            float wallThick = 0.5f;
            CreateWall($"WallTop_{i}", room.transform,
                new Vector3(0, half + wallThick / 2f, 0),
                new Vector3(size + wallThick * 2, wallThick, 1), wallColors[i]);
            CreateWall($"WallBot_{i}", room.transform,
                new Vector3(0, -half - wallThick / 2f, 0),
                new Vector3(size + wallThick * 2, wallThick, 1), wallColors[i]);
            CreateWall($"WallLeft_{i}", room.transform,
                new Vector3(-half - wallThick / 2f, 0, 0),
                new Vector3(wallThick, size + wallThick * 2, 1), wallColors[i]);
            CreateWall($"WallRight_{i}", room.transform,
                new Vector3(half + wallThick / 2f, 0, 0),
                new Vector3(wallThick, size + wallThick * 2, 1), wallColors[i]);

            // ── Entry point ──
            GameObject entry = new GameObject($"EntryPoint_{i}");
            entry.transform.SetParent(room.transform);
            entry.transform.localPosition = new Vector3(0, -half + 2f, 0);

            // ── Room name label on floor ──
            string displayName = roomNames[i].Replace($"Room{i}_", "").Replace("_", " ");
            AddWorldLabel(room.transform, displayName, new Vector3(0, half - 1.5f, 0),
                Color.white * 0.4f, 40);

            // ── Enter-deeper door (top) ──
            if (i < 3)
            {
                CreateDoor($"DoorDeeper_{i}", room.transform,
                    new Vector3(0, half - 0.3f, 0), false, i + 1,
                    Door.DoorType.EnterDeeper, "GO DEEPER >>>");
            }

            // ── Exit-to-parent door (bottom) ──
            if (i > 0)
            {
                bool locked = (i < 3 && !string.IsNullOrEmpty(monsterNames[i]));
                CreateDoor($"DoorExit_{i}", room.transform,
                    new Vector3(0, -half + 0.3f, 0), locked, i - 1,
                    Door.DoorType.ExitToParent, "<<< GO BACK");
            }

            // Room 0: victory exit door
            if (i == 0)
            {
                CreateDoor("DoorVictoryExit", room.transform,
                    new Vector3(0, -half + 0.3f, 0), true, 0,
                    Door.DoorType.ExitToParent, "<<< EXIT DUNGEON");
            }

            // ── Monster ──
            if (i < 3)
            {
                GameObject monster = CreateMonster($"Monster_{monsterNames[i]}", room.transform,
                    new Vector3(0, 1.5f, 0), monsterNames[i], monsterColors[i],
                    monsterSizes[i], requiredWeapons[i], rewardWeapons[i], rewardColors[i]);

                Monster mc = monster.GetComponent<Monster>();
                Door exitDoor = null;
                if (i == 0)
                    exitDoor = room.transform.Find("DoorVictoryExit")?.GetComponent<Door>();
                else
                    exitDoor = room.transform.Find($"DoorExit_{i}")?.GetComponent<Door>();
                if (mc != null && exitDoor != null)
                    mc.exitDoor = exitDoor;
            }

            // ── Base case item (Room 3) ──
            if (i == 3)
            {
                CreateItem("Item_Dagger", room.transform, new Vector3(0, 0.5f, 0),
                    basePickup, daggerColor, 0.7f);

                AddWorldLabel(room.transform, "BASE CASE",
                    new Vector3(0, 2.5f, 0), new Color(0.2f, 1f, 0.4f), 36);
                AddWorldLabel(room.transform, "No monster here!\nPick up the Dagger!",
                    new Vector3(0, -1.5f, 0), new Color(1f, 1f, 0.5f), 22);
            }

            room.SetActive(i == 0);
        }
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
        if (player != null)
            roomMgr.player = player;
    }

    // ═══════════════════════════════════════════════════
    //                    CANVAS / UI
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

        // ── Fade overlay ──
        GameObject fadeObj = CreateUIImage("FadeImage", canvasGO.transform, Color.black);
        StretchFull(fadeObj);
        Image fadeImg = fadeObj.GetComponent<Image>();
        fadeImg.color = new Color(0, 0, 0, 0);
        fadeImg.raycastTarget = false;
        camTrans.fadeImage = fadeImg;

        // ── HUD: Top Left (Room Info) ──
        GameObject hudLeft = CreateUIPanel("HUD_TopLeft", canvasGO.transform,
            new Vector2(0, 1), new Vector2(0, 1), new Vector2(15, -15),
            new Vector2(320, 90), new Color(0.05f, 0.05f, 0.15f, 0.85f));

        GameObject depthText = CreateUIText("DepthText", hudLeft.transform, "Depth: 0",
            26, TextAnchor.UpperLeft, new Vector2(15, -8), new Vector2(290, 40));
        depthText.GetComponent<Text>().color = new Color(0.4f, 0.9f, 1f);

        GameObject roomText = CreateUIText("RoomNameText", hudLeft.transform, "Grand Hall",
            20, TextAnchor.UpperLeft, new Vector2(15, -45), new Vector2(290, 35));
        roomText.GetComponent<Text>().color = new Color(0.9f, 0.85f, 0.7f);

        uiMgr.depthText = depthText.GetComponent<Text>();
        uiMgr.roomNameText = roomText.GetComponent<Text>();

        // ── HUD: Top Right (Weapon) ──
        GameObject hudRight = CreateUIPanel("HUD_TopRight", canvasGO.transform,
            new Vector2(1, 1), new Vector2(1, 1), new Vector2(-15, -15),
            new Vector2(300, 70), new Color(0.05f, 0.05f, 0.15f, 0.85f));

        GameObject weaponText = CreateUIText("WeaponText", hudRight.transform, "Weapon: None",
            22, TextAnchor.MiddleRight, new Vector2(-15, 0), new Vector2(230, 40));
        weaponText.GetComponent<Text>().color = new Color(1f, 0.9f, 0.5f);

        GameObject weaponIcon = CreateUIImage("WeaponIcon", hudRight.transform, Color.gray);
        RectTransform wiRT = weaponIcon.GetComponent<RectTransform>();
        wiRT.anchorMin = new Vector2(0, 0.5f); wiRT.anchorMax = new Vector2(0, 0.5f);
        wiRT.anchoredPosition = new Vector2(30, 0);
        wiRT.sizeDelta = new Vector2(40, 40);

        uiMgr.weaponNameText = weaponText.GetComponent<Text>();
        uiMgr.weaponIconImage = weaponIcon.GetComponent<Image>();

        // ── Timer (top center) ──
        GameObject timerPanel = CreateUIPanel("TimerPanel", canvasGO.transform,
            new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -15),
            new Vector2(140, 45), new Color(0.05f, 0.05f, 0.15f, 0.85f));
        timerPanel.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 1);

        GameObject timerText = CreateUIText("TimerText", timerPanel.transform, "00:00",
            24, TextAnchor.MiddleCenter, Vector2.zero, new Vector2(130, 40));
        timerText.GetComponent<Text>().color = new Color(0.8f, 1f, 0.8f);
        RectTransform ttRT = timerText.GetComponent<RectTransform>();
        ttRT.anchorMin = Vector2.zero; ttRT.anchorMax = Vector2.one;
        ttRT.offsetMin = Vector2.zero; ttRT.offsetMax = Vector2.zero;
        uiMgr.timerText = timerText.GetComponent<Text>();

        // ── Dialog Panel (center bottom) ──
        GameObject dialogPanel = CreateUIPanel("DialogPanel", canvasGO.transform,
            new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 40),
            new Vector2(600, 70), new Color(0.1f, 0.05f, 0.2f, 0.92f));
        dialogPanel.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0);

        // Border for dialog
        Outline dlgOutline = dialogPanel.AddComponent<Outline>();
        dlgOutline.effectColor = new Color(0.5f, 0.3f, 0.8f, 0.8f);
        dlgOutline.effectDistance = new Vector2(2, 2);

        GameObject dialogText = CreateUIText("DialogText", dialogPanel.transform, "",
            24, TextAnchor.MiddleCenter, Vector2.zero, new Vector2(580, 60));
        RectTransform dlgTRT = dialogText.GetComponent<RectTransform>();
        dlgTRT.anchorMin = Vector2.zero; dlgTRT.anchorMax = Vector2.one;
        dlgTRT.offsetMin = new Vector2(10, 5); dlgTRT.offsetMax = new Vector2(-10, -5);
        dialogText.GetComponent<Text>().color = new Color(1f, 1f, 0.7f);

        uiMgr.dialogPanel = dialogPanel;
        uiMgr.dialogText = dialogText.GetComponent<Text>();
        dialogPanel.SetActive(false);

        // ── Controls hint (bottom right) ──
        GameObject controlsPanel = CreateUIPanel("ControlsHint", canvasGO.transform,
            new Vector2(1, 0), new Vector2(1, 0), new Vector2(-15, 15),
            new Vector2(220, 90), new Color(0, 0, 0, 0.6f));
        controlsPanel.GetComponent<RectTransform>().pivot = new Vector2(1, 0);

        GameObject controlsText = CreateUIText("ControlsText", controlsPanel.transform,
            "WASD - Move\nSPACE - Attack\nWalk into doors to enter",
            14, TextAnchor.MiddleLeft, Vector2.zero, new Vector2(200, 80));
        RectTransform ctrlRT = controlsText.GetComponent<RectTransform>();
        ctrlRT.anchorMin = Vector2.zero; ctrlRT.anchorMax = Vector2.one;
        ctrlRT.offsetMin = new Vector2(10, 5); ctrlRT.offsetMax = new Vector2(-10, -5);
        controlsText.GetComponent<Text>().color = new Color(0.6f, 0.6f, 0.7f);

        // ── Call Stack UI (left side) ──
        GameObject callStackBG = CreateUIPanel("CallStackPanel", canvasGO.transform,
            new Vector2(0, 0), new Vector2(0, 0), new Vector2(15, 120),
            new Vector2(300, 450), new Color(0.05f, 0.05f, 0.1f, 0.75f));
        callStackBG.GetComponent<RectTransform>().pivot = new Vector2(0, 0);

        // Title bar
        GameObject csTitleBar = CreateUIPanel("TitleBar", callStackBG.transform,
            new Vector2(0, 1), new Vector2(1, 1), Vector2.zero,
            new Vector2(0, 35), new Color(0.15f, 0.1f, 0.3f, 0.9f));
        RectTransform csTitleRT = csTitleBar.GetComponent<RectTransform>();
        csTitleRT.pivot = new Vector2(0.5f, 1);
        csTitleRT.anchorMin = new Vector2(0, 1);
        csTitleRT.anchorMax = new Vector2(1, 1);
        csTitleRT.offsetMin = new Vector2(0, -35);
        csTitleRT.offsetMax = Vector2.zero;

        CreateUIText("CallStackTitle", csTitleBar.transform, "CALL STACK",
            18, TextAnchor.MiddleCenter, Vector2.zero, new Vector2(280, 30));

        // Stack container
        GameObject stackContainer = new GameObject("StackContainer");
        stackContainer.transform.SetParent(callStackBG.transform, false);
        RectTransform scRT = stackContainer.AddComponent<RectTransform>();
        scRT.anchorMin = new Vector2(0, 0); scRT.anchorMax = new Vector2(1, 1);
        scRT.offsetMin = new Vector2(8, 8); scRT.offsetMax = new Vector2(-8, -40);
        VerticalLayoutGroup vlg = stackContainer.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 6;
        vlg.childAlignment = TextAnchor.LowerLeft;
        vlg.childControlWidth = true;
        vlg.childControlHeight = false;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;
        ContentSizeFitter csf = stackContainer.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        CallStackUI callStackUI = canvasGO.AddComponent<CallStackUI>();
        callStackUI.stackContainer = stackContainer.transform;

        // Frame prefab
        GameObject framePrefab = CreateCallStackFramePrefab(canvasGO.transform);
        callStackUI.framePrefab = framePrefab;
        framePrefab.SetActive(false);

        // ── Victory Panel ──
        GameObject victoryPanel = CreateUIPanel("VictoryPanel", canvasGO.transform,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero,
            new Vector2(550, 380), new Color(0.05f, 0.02f, 0.12f, 0.97f));
        victoryPanel.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0.5f);
        Outline victoryOutline = victoryPanel.AddComponent<Outline>();
        victoryOutline.effectColor = new Color(1f, 0.8f, 0.2f, 0.6f);
        victoryOutline.effectDistance = new Vector2(3, 3);

        GameObject vicTitle = CreateUIText("VictoryTitle", victoryPanel.transform,
            "You Escaped the Dungeon!",
            36, TextAnchor.MiddleCenter, new Vector2(0, 100), new Vector2(500, 60));
        vicTitle.GetComponent<Text>().color = new Color(1f, 0.85f, 0.2f);

        GameObject vicSubtitle = CreateUIText("VictorySubtitle", victoryPanel.transform,
            "The recursion has been unwound!",
            20, TextAnchor.MiddleCenter, new Vector2(0, 55), new Vector2(500, 35));
        vicSubtitle.GetComponent<Text>().color = new Color(0.7f, 0.9f, 0.7f);
        vicSubtitle.GetComponent<Text>().fontStyle = FontStyle.Italic;

        GameObject victoryTime = CreateUIText("VictoryTime", victoryPanel.transform, "Time: 00:00",
            26, TextAnchor.MiddleCenter, new Vector2(0, 5), new Vector2(500, 40));
        victoryTime.GetComponent<Text>().color = Color.white;

        GameObject playAgainBtn = CreateUIButton("PlayAgainButton", victoryPanel.transform,
            "Play Again", new Vector2(-90, -70), new Vector2(170, 50),
            new Color(0.15f, 0.5f, 0.15f));
        playAgainBtn.GetComponentInChildren<Text>().fontSize = 22;

        GameObject mainMenuBtn = CreateUIButton("MainMenuButton", victoryPanel.transform,
            "Main Menu", new Vector2(90, -70), new Vector2(170, 50),
            new Color(0.5f, 0.15f, 0.15f));
        mainMenuBtn.GetComponentInChildren<Text>().fontSize = 22;

        uiMgr.victoryPanel = victoryPanel;
        uiMgr.victoryTimeText = victoryTime.GetComponent<Text>();
        uiMgr.playAgainButton = playAgainBtn.GetComponent<Button>();
        uiMgr.mainMenuButton = mainMenuBtn.GetComponent<Button>();
        victoryPanel.SetActive(false);
    }

    // ═══════════════════════════════════════════════════
    //                    CAMERA
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
        cam.orthographicSize = 7f;
        cam.backgroundColor = new Color(0.03f, 0.03f, 0.06f);
        cam.clearFlags = CameraClearFlags.SolidColor;

        if (cam.GetComponent<CameraFollow>() == null)
            cam.gameObject.AddComponent<CameraFollow>();

        CameraFollow follow = cam.GetComponent<CameraFollow>();
        Transform player = GameObject.Find("Player")?.transform;
        if (player != null) follow.target = player;
    }

    // ═══════════════════════════════════════════════════
    //                  MAIN MENU
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
        cam.backgroundColor = new Color(0.04f, 0.02f, 0.08f);

        GameObject canvasGO = new GameObject("MenuCanvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasGO.AddComponent<GraphicRaycaster>();

        // Background
        GameObject bg = CreateUIImage("Background", canvasGO.transform, new Color(0.04f, 0.02f, 0.08f));
        StretchFull(bg);

        // Title
        GameObject title = CreateUIText("Title", canvasGO.transform, "RECURSION\nDUNGEON",
            64, TextAnchor.MiddleCenter, new Vector2(0, 140), new Vector2(900, 160));
        title.GetComponent<Text>().color = new Color(1f, 0.85f, 0.2f);

        // Subtitle
        GameObject subtitle = CreateUIText("Subtitle", canvasGO.transform,
            "Solve the deepest room first.",
            26, TextAnchor.MiddleCenter, new Vector2(0, 40), new Vector2(700, 40));
        subtitle.GetComponent<Text>().color = new Color(0.6f, 0.6f, 0.85f);

        // Code hint
        GameObject hint = CreateUIText("Hint", canvasGO.transform,
            "solve(room) {\n    weapon = solve(room.next);\n    defeat(room.monster, weapon);\n}",
            18, TextAnchor.MiddleCenter, new Vector2(0, -30), new Vector2(600, 80));
        hint.GetComponent<Text>().color = new Color(0.4f, 0.85f, 0.4f);
        hint.GetComponent<Text>().fontStyle = FontStyle.Italic;

        // Play button
        GameObject playBtn = CreateUIButton("PlayButton", canvasGO.transform,
            "PLAY", new Vector2(0, -120), new Vector2(250, 60),
            new Color(0.1f, 0.4f, 0.1f));
        playBtn.GetComponentInChildren<Text>().fontSize = 32;

        // Quit button
        GameObject quitBtn = CreateUIButton("QuitButton", canvasGO.transform,
            "QUIT", new Vector2(0, -200), new Vector2(200, 48),
            new Color(0.4f, 0.12f, 0.12f));
        quitBtn.GetComponentInChildren<Text>().fontSize = 22;

        MainMenu menu = canvasGO.AddComponent<MainMenu>();
        menu.playButton = playBtn.GetComponent<Button>();
        menu.quitButton = quitBtn.GetComponent<Button>();
    }

    // ═══════════════════════════════════════════════════
    //                WIRE REFERENCES
    // ═══════════════════════════════════════════════════
    static void WireReferences()
    {
        GameObject prefabHolder = new GameObject("--- PREFAB TEMPLATES (hidden) ---");

        GameObject itemTemplate = new GameObject("ItemPrefab");
        itemTemplate.transform.SetParent(prefabHolder.transform);
        SpriteRenderer itemSR = itemTemplate.AddComponent<SpriteRenderer>();
        itemSR.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
        itemSR.sortingOrder = 5;
        CircleCollider2D itemCol = itemTemplate.AddComponent<CircleCollider2D>();
        itemCol.isTrigger = true;
        itemCol.radius = 0.45f;
        itemTemplate.AddComponent<Item>();
        itemTemplate.transform.localScale = Vector3.one * 0.6f;

        Monster[] monsters = Object.FindObjectsByType<Monster>(FindObjectsSortMode.None);
        foreach (var m in monsters)
            m.rewardItemPrefab = itemTemplate;

        prefabHolder.SetActive(false);
        Debug.Log("References wired.");
    }

    // ═══════════════════════════════════════════════════
    //              HELPER: WORLD-SPACE LABEL
    // ═══════════════════════════════════════════════════
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

        MeshRenderer mr = labelGO.GetComponent<MeshRenderer>();
        mr.sortingOrder = 15;

        return labelGO;
    }

    // ═══════════════════════════════════════════════════
    //              HELPER: SPRITES & WALLS
    // ═══════════════════════════════════════════════════
    static GameObject CreateSprite(string name, Transform parent, Vector3 localPos,
        Vector3 scale, Color color, int sortOrder)
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

    static GameObject CreateWall(string name, Transform parent, Vector3 localPos,
        Vector3 scale, Color color)
    {
        GameObject wall = CreateSprite(name, parent, localPos, scale, color, 2);
        wall.AddComponent<BoxCollider2D>();
        return wall;
    }

    // ═══════════════════════════════════════════════════
    //              HELPER: DOORS
    // ═══════════════════════════════════════════════════
    static GameObject CreateDoor(string name, Transform parent, Vector3 localPos,
        bool locked, int targetDepth, Door.DoorType type, string labelText)
    {
        GameObject door = new GameObject(name);
        door.transform.SetParent(parent);
        door.transform.localPosition = localPos;
        door.transform.localScale = new Vector3(2.5f, 0.6f, 1f);

        SpriteRenderer sr = door.AddComponent<SpriteRenderer>();
        sr.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
        sr.sortingOrder = 3;

        BoxCollider2D col = door.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(1f, 1.2f);

        Door doorComp = door.AddComponent<Door>();
        doorComp.doorType = type;
        doorComp.targetRoomDepth = targetDepth;
        doorComp.isLocked = locked;
        doorComp.doorRenderer = sr;

        Color unlockedCol = new Color(0.1f, 0.9f, 0.2f);
        Color lockedCol = new Color(0.9f, 0.15f, 0.1f);
        sr.color = locked ? lockedCol : unlockedCol;

        // Lock indicator
        GameObject lockIcon = new GameObject("LockIndicator");
        lockIcon.transform.SetParent(door.transform);
        lockIcon.transform.localPosition = Vector3.zero;
        lockIcon.transform.localScale = new Vector3(0.15f, 0.4f, 1f);
        SpriteRenderer lockSR = lockIcon.AddComponent<SpriteRenderer>();
        lockSR.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
        lockSR.color = Color.yellow;
        lockSR.sortingOrder = 4;
        lockIcon.SetActive(locked);
        doorComp.lockIndicator = lockIcon;

        // Door label
        Color labelColor = locked ? new Color(1f, 0.4f, 0.4f) : new Color(0.4f, 1f, 0.5f);
        float labelY = type == Door.DoorType.EnterDeeper ? -1.2f : 1.2f;
        AddWorldLabel(door.transform, labelText,
            new Vector3(0, labelY, 0), labelColor, 24);

        return door;
    }

    // ═══════════════════════════════════════════════════
    //              HELPER: MONSTERS
    // ═══════════════════════════════════════════════════
    static GameObject CreateMonster(string name, Transform parent, Vector3 localPos,
        string monsterName, Color color, float size, string required, string reward, Color rewardColor)
    {
        GameObject monster = new GameObject(name);
        monster.transform.SetParent(parent);
        monster.transform.localPosition = localPos;
        monster.transform.localScale = Vector3.one * size;

        SpriteRenderer sr = monster.AddComponent<SpriteRenderer>();
        sr.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
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

        // Glow ring behind monster
        GameObject glow = new GameObject("GlowRing");
        glow.transform.SetParent(monster.transform);
        glow.transform.localPosition = Vector3.zero;
        glow.transform.localScale = Vector3.one * 1.4f;
        SpriteRenderer glowSR = glow.AddComponent<SpriteRenderer>();
        glowSR.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
        Color glowColor = color;
        glowColor.a = 0.25f;
        glowSR.color = glowColor;
        glowSR.sortingOrder = 4;

        // Monster name label
        AddWorldLabel(monster.transform, monsterName,
            new Vector3(0, 0.8f, 0), Color.white, 30);

        // "Needs X" label
        AddWorldLabel(monster.transform, $"needs: {required}",
            new Vector3(0, -0.8f, 0), new Color(1f, 0.7f, 0.3f), 22);

        return monster;
    }

    // ═══════════════════════════════════════════════════
    //              HELPER: ITEMS
    // ═══════════════════════════════════════════════════
    static GameObject CreateItem(string name, Transform parent, Vector3 localPos,
        string weaponName, Color color, float size)
    {
        GameObject item = new GameObject(name);
        item.transform.SetParent(parent);
        item.transform.localPosition = localPos;
        item.transform.localScale = Vector3.one * size;

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

        // Glow
        GameObject glow = new GameObject("Glow");
        glow.transform.SetParent(item.transform);
        glow.transform.localPosition = Vector3.zero;
        glow.transform.localScale = Vector3.one * 1.6f;
        SpriteRenderer glowSR = glow.AddComponent<SpriteRenderer>();
        glowSR.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
        Color gc = color; gc.a = 0.2f;
        glowSR.color = gc;
        glowSR.sortingOrder = 4;

        AddWorldLabel(item.transform, weaponName,
            new Vector3(0, -0.9f, 0), new Color(1f, 1f, 0.6f), 26);

        return item;
    }

    // ═══════════════════════════════════════════════════
    //          HELPER: CALL STACK FRAME PREFAB
    // ═══════════════════════════════════════════════════
    static GameObject CreateCallStackFramePrefab(Transform parent)
    {
        GameObject frame = new GameObject("FramePrefab");
        frame.transform.SetParent(parent, false);

        RectTransform rt = frame.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(284, 70);

        Image bg = frame.AddComponent<Image>();
        bg.color = new Color(0.12f, 0.12f, 0.22f, 0.92f);

        LayoutElement le = frame.AddComponent<LayoutElement>();
        le.preferredHeight = 70;
        le.minHeight = 70;

        Outline outline = frame.AddComponent<Outline>();
        outline.effectColor = new Color(0.3f, 0.3f, 0.6f, 0.5f);
        outline.effectDistance = new Vector2(1, 1);

        GameObject textObj = new GameObject("FrameText");
        textObj.transform.SetParent(frame.transform, false);
        RectTransform textRT = textObj.AddComponent<RectTransform>();
        textRT.anchorMin = Vector2.zero; textRT.anchorMax = Vector2.one;
        textRT.offsetMin = new Vector2(10, 5); textRT.offsetMax = new Vector2(-10, -5);

        Text text = textObj.AddComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = 14;
        text.color = Color.white;
        text.alignment = TextAnchor.MiddleLeft;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Truncate;
        text.lineSpacing = 1.1f;

        return frame;
    }

    // ═══════════════════════════════════════════════════
    //              UI HELPERS
    // ═══════════════════════════════════════════════════
    static void StretchFull(GameObject go)
    {
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
    }

    static GameObject CreateUIPanel(string name, Transform parent, Vector2 anchorMin,
        Vector2 anchorMax, Vector2 anchoredPos, Vector2 size, Color color)
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

        Outline ol = go.AddComponent<Outline>();
        ol.effectColor = Color.white * 0.3f;
        ol.effectDistance = new Vector2(1, 1);

        Button btn = go.AddComponent<Button>();
        ColorBlock cb = btn.colors;
        cb.highlightedColor = bgColor * 1.4f;
        cb.pressedColor = bgColor * 0.6f;
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
