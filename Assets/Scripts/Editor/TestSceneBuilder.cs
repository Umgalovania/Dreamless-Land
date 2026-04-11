#if UNITY_EDITOR
using DreamlessLand;
using DreamlessLand.Common;
using DreamlessLand.Enemy;
using DreamlessLand.Player;
using DreamlessLand.Terrain;
using DreamlessLand.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

namespace DreamlessLand.EditorTools
{
    public static class TestSceneBuilder
    {
        const string GameScenePath = "Assets/Scenes/Level1.unity";
        const string MenuScenePath = "Assets/Scenes/MainMenu.unity";

        // ===== 颜色 =====
        static readonly Color ColTerrain = new Color(0.35f, 0.35f, 0.38f);
        static readonly Color ColWall = new Color(0.28f, 0.28f, 0.32f);
        static readonly Color ColPlatform = new Color(0.85f, 0.75f, 0.2f);
        static readonly Color ColPlayer = new Color(0.2f, 0.85f, 0.3f);
        static readonly Color ColEnemy = new Color(0.9f, 0.2f, 0.2f);
        static readonly Color ColSoldier = new Color(0.85f, 0.45f, 0.1f);
        static readonly Color ColDoor = new Color(0.2f, 0.4f, 1f);
        static readonly Color ColRestPoint = new Color(1f, 0.25f, 0.25f);
        static readonly Color ColLadder = new Color(0.95f, 0.9f, 0.5f);
        static readonly Color ColPickup = new Color(0.3f, 0.55f, 1f);
        static readonly Color ColBtnNormal = new Color(0.22f, 0.22f, 0.28f, 0.95f);
        static readonly Color ColBtnHover = new Color(0.35f, 0.35f, 0.45f, 1f);
        static readonly Color ColBtnPress = new Color(0.15f, 0.15f, 0.2f, 1f);
        static readonly Color ColPanel = new Color(0.05f, 0.05f, 0.08f, 0.85f);

        static Font _font;
        static Font GetFont()
        {
            if (_font == null)
                _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            return _font;
        }

        // ============================================================
        //  菜单入口
        // ============================================================

        [MenuItem("Dreamless Land/生成完整项目（主菜单 + 第一关）")]
        public static void BuildAll()
        {
            if (!EditorUtility.DisplayDialog("生成完整项目",
                "将生成：\n• MainMenu.unity（主菜单）\n• Level1.unity（第一关）\n\n已有文件将被覆盖。",
                "继续", "取消"))
                return;

            EnsureFolder("Assets/Scenes");

            BuildLevel1Scene();
            BuildMainMenuScene();
            SetupBuildSettings();

            EditorSceneManager.OpenScene(GameScenePath);

            EditorUtility.DisplayDialog("完成",
                "已生成两个场景！\n\n从 MainMenu 场景开始游戏，\n或直接 Play 当前的 Level1 场景测试。\n\n" +
                "A/D - 移动 | K - 跳跃 | J - 攻击\nL - 闪避 | W/S - 梯子攀爬\n" +
                "F - 交互/拾取 | S+K - 穿越平台\nTab - 属性 | ESC - 暂停",
                "确定");
        }

        // ============================================================
        //  第一关场景
        // ============================================================
        //
        //  两层房间（15宽 x 6高 x 2层）
        //  Floor 1: y=0 ~ y=6,  玩家出生在右侧，左墙有梯子
        //  Floor 2: y=6 ~ y=12, 左4单位地板为平台，右侧有门通向外界
        //  门外向右延伸地面，放置两个巡逻小兵和一个休息点
        //

        const float RoomW = 15f;
        const float FloorH = 6f;
        const float WallT = 1f;
        const float DoorH = 2.5f;

        static void BuildLevel1Scene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var player = CreatePlayer();
            CreateCamera(player);
            CreateEventSystem();
            CreateLevel1Terrain();
            CreateLevel1Ladder();
            CreateLevel1Doors();
            CreateLevel1RestPoints();
            CreateLevel1Pickups();
            CreateLevel1Enemies();
            CreateGameCanvas(player);
            CreateGameManagerObj();

            EditorSceneManager.SaveScene(scene, GameScenePath);
        }

        // ---- 摄像机 ----

        static void CreateCamera(GameObject player)
        {
            var go = new GameObject("Main Camera");
            go.tag = "MainCamera";
            var cam = go.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 10f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.08f, 0.08f, 0.12f);
            cam.cullingMask = -1;
            go.AddComponent<AudioListener>();
            go.AddComponent<UniversalAdditionalCameraData>();
            go.transform.position = new Vector3(13f, 5f, -10f);

            var follow = go.AddComponent<CameraFollow>();
            var so = new SerializedObject(follow);
            so.FindProperty("target").objectReferenceValue = player.transform;
            so.FindProperty("offset").vector3Value = new Vector3(0f, 2f, -10f);
            so.FindProperty("smoothSpeed").floatValue = 8f;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        // ---- 地形 ----

        static void CreateLevel1Terrain()
        {
            // 地面（从房间左侧延伸到右侧远处）
            MakeWall("Ground", new Vector3(40f, -0.5f, 0f), new Vector3(200f, 1f, 1f));

            // 左墙（全高 y=0~12）
            MakeWall("LeftWall", new Vector3(-0.5f, 6f, 0f), new Vector3(WallT, 12f, 1f));

            // 右墙 - Floor 1（y=0~6，门在 Floor 2）
            MakeWall("RightWall_F1", new Vector3(15.5f, 3f, 0f), new Vector3(WallT, 6f, 1f));

            // 右墙 - Floor 2 门上方（y=8.5~12）
            MakeWall("RightWall_F2_Top", new Vector3(15.5f, 10.25f, 0f), new Vector3(WallT, 3.5f, 1f));

            // 天花板
            MakeWall("Ceiling", new Vector3(7.5f, 12.5f, 0f), new Vector3(17f, WallT, 1f));

            // 楼层分隔 - 实体部分（x=4~15）
            MakeWall("FloorDivider_Solid", new Vector3(9.5f, 5.85f, 0f), new Vector3(11f, 0.3f, 1f));

            // 楼层分隔 - 平台部分（x=0~4，单向平台）
            var platGo = MakeBlock("FloorDivider_Platform", new Vector3(2f, 5.85f, 0f),
                new Vector3(4f, 0.3f, 1f), "Platform", ColPlatform);
            var box = platGo.AddComponent<BoxCollider2D>();
            box.usedByEffector = true;
            var pe = platGo.AddComponent<PlatformEffector2D>();
            pe.useOneWay = true;
            pe.surfaceArc = 170f;

            // 边界墙 - 左
            MakeWall("BoundaryLeft", new Vector3(-10.5f, 19f, 0f), new Vector3(WallT, 40f, 1f));

            // 边界墙 - 右
            MakeWall("BoundaryRight", new Vector3(90.5f, 19f, 0f), new Vector3(WallT, 40f, 1f));
        }

        // ---- 梯子 ----

        static void CreateLevel1Ladder()
        {
            // 左墙内侧梯子，从 y=0 到 y=7（穿过平台层到 Floor 2）
            var go = MakeBlock("Ladder", new Vector3(0.5f, 3.5f, 0f),
                new Vector3(0.2f, 7f, 1f), "Terrain", ColLadder);

            var ladder = go.AddComponent<Ladder>();

            // 触发碰撞体比视觉宽，方便玩家抓住
            var ladderBox = go.AddComponent<BoxCollider2D>();
            ladderBox.isTrigger = true;
            ladderBox.size = new Vector2(5f, 1f); // effective: 0.2*5=1 wide, 7*1=7 tall
        }

        // ---- 门 ----

        static void CreateLevel1Doors()
        {
            // Floor 2 右墙的门（y=6~8.5）
            MakeDoor("Door_F2_Right", new Vector3(15.5f, 7.25f, 0f), new Vector3(WallT, DoorH, 1f));
        }

        // ---- 休息点 ----

        static void CreateLevel1RestPoints()
        {
            // Floor 2 中间
            MakeRestPoint("RestPoint_F2", new Vector3(5f, 6.3f, 0f));

            // 门外远处（距门 60 单位 = x=75.5）
            MakeRestPoint("RestPoint_Outside", new Vector3(75.5f, 0.5f, 0f));
        }

        // ---- 掉落物 ----

        static void CreateLevel1Pickups()
        {
            // 文件-模糊的祷文（Floor 2 休息点旁边）
            MakeFilePickup("Pickup_Prayer",
                new Vector3(7.5f, 6.55f, 0f),
                "模糊的祷文",
                "来自彼岸的勇……我们需要你……\n将众神从梦……起，\n拯救这个频临破……界。");
        }

        // ---- 敌人 ----

        static void CreateLevel1Enemies()
        {
            // 巡逻小兵 1（门右侧 15 单位 = x=30.5）
            MakeSoldier("Soldier_1", new Vector3(30.5f, 2f, 0f));

            // 巡逻小兵 2（门右侧 45 单位 = x=60.5）
            MakeSoldier("Soldier_2", new Vector3(60.5f, 2f, 0f));
        }

        // ---- 玩家 ----

        static GameObject CreatePlayer()
        {
            var go = new GameObject("Player");
            go.transform.position = new Vector3(13f, 2f, 0f);
            go.transform.localScale = new Vector3(1f, 2f, 1f);
            go.tag = "Player";
            SetLayer(go, "Player");
            SetupSprite(go, ColPlayer);

            var rb = go.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.freezeRotation = true;
            rb.gravityScale = 3f;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            var cap = go.AddComponent<CapsuleCollider2D>();
            cap.size = new Vector2(1f, 1f);
            cap.direction = CapsuleDirection2D.Vertical;

            var gcGo = new GameObject("GroundCheck");
            gcGo.transform.SetParent(go.transform);
            gcGo.transform.localPosition = new Vector3(0f, -0.525f, 0f);

            go.AddComponent<PlayerUnit>();
            SetProps(go.GetComponent<PlayerUnit>(),
                ("maxHealth", 5), ("invincibleDuration", 1f), ("flashInterval", 0.1f), ("knockbackForce", 5f));

            go.AddComponent<PlayerController2D>();
            var ctrlSo = new SerializedObject(go.GetComponent<PlayerController2D>());
            ctrlSo.FindProperty("moveSpeed").floatValue = 6.5f;
            ctrlSo.FindProperty("jumpForce").floatValue = 15.4f;
            ctrlSo.FindProperty("maxJumpCount").intValue = 1;
            ctrlSo.FindProperty("dashSpeed").floatValue = 15f;
            ctrlSo.FindProperty("dashDuration").floatValue = 0.15f;
            ctrlSo.FindProperty("dashCooldown").floatValue = 1f;
            ctrlSo.FindProperty("groundCheck").objectReferenceValue = gcGo.transform;
            ctrlSo.FindProperty("groundCheckRadius").floatValue = 0.18f;
            ctrlSo.FindProperty("groundCheckWidth").floatValue = 0.5f;
            ctrlSo.FindProperty("groundLayers").intValue = LayerMask.GetMask("Terrain", "Platform");
            ctrlSo.FindProperty("platformLayerOnly").intValue = LayerMask.GetMask("Platform");
            ctrlSo.FindProperty("platformIgnoreDuration").floatValue = 0.4f;
            ctrlSo.FindProperty("climbSpeed").floatValue = 4f;
            ctrlSo.ApplyModifiedPropertiesWithoutUndo();

            go.AddComponent<PlayerCombat>();
            SetProps(go.GetComponent<PlayerCombat>(),
                ("attackDamage", 4), ("attackRange", 2f), ("attackAngle", 60f),
                ("attackCooldown", 0.4f));
            var combatSo = new SerializedObject(go.GetComponent<PlayerCombat>());
            combatSo.FindProperty("enemyLayers").intValue = LayerMask.GetMask("Enemy");
            combatSo.ApplyModifiedPropertiesWithoutUndo();

            return go;
        }

        // ---- 工厂方法 ----

        static void MakeSoldier(string name, Vector3 pos)
        {
            var go = MakeBlock(name, pos, new Vector3(1f, 1.5f, 1f), "Enemy", ColSoldier);
            go.tag = "Enemy";

            var rb = go.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.freezeRotation = true;
            rb.gravityScale = 3f;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            var box = go.AddComponent<BoxCollider2D>();
            box.size = new Vector2(0.9f, 0.9f);

            go.AddComponent<EnemyUnit>();
            SetProps(go.GetComponent<EnemyUnit>(), ("maxHealth", 20));

            go.AddComponent<EnemySoldier>();
            var sSo = new SerializedObject(go.GetComponent<EnemySoldier>());
            sSo.FindProperty("moveSpeed").floatValue = 2.5f;
            sSo.FindProperty("patrolHalfWidth").floatValue = 10f;
            sSo.FindProperty("detectionRange").floatValue = 10f;
            sSo.FindProperty("hearingRange").floatValue = 3f;
            sSo.FindProperty("terrainLayers").intValue = LayerMask.GetMask("Terrain");
            sSo.FindProperty("attackRange").floatValue = 1.5f;
            sSo.FindProperty("attackDamage").intValue = 1;
            sSo.FindProperty("attackWindup").floatValue = 0.5f;
            sSo.FindProperty("attackRecovery").floatValue = 1f;
            sSo.ApplyModifiedPropertiesWithoutUndo();

            go.AddComponent<EnemyContactDamage>();
            SetProps(go.GetComponent<EnemyContactDamage>(),
                ("contactDamage", 1), ("damageCooldown", 0.5f));
        }

        static void MakeFilePickup(string name, Vector3 pos, string itemName, string desc)
        {
            var go = MakeBlock(name, pos, new Vector3(0.7f, 0.7f, 1f), "Default", ColPickup);

            go.AddComponent<Pickup>();
            var so = new SerializedObject(go.GetComponent<Pickup>());
            so.FindProperty("itemName").stringValue = itemName;
            so.FindProperty("description").stringValue = desc;
            so.FindProperty("effect").enumValueIndex = 0; // None
            so.FindProperty("interactionRange").floatValue = 2f;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static void MakeWall(string name, Vector3 pos, Vector3 scale)
        {
            var go = MakeBlock(name, pos, scale, "Terrain", ColWall);
            go.AddComponent<BoxCollider2D>();
        }

        static void MakeDoor(string name, Vector3 pos, Vector3 scale)
        {
            var go = MakeBlock(name, pos, scale, "Door", ColDoor);
            go.AddComponent<BoxCollider2D>();
            var door = go.AddComponent<Door>();
            var so = new SerializedObject(door);
            so.FindProperty("isOpen").boolValue = false;
            so.FindProperty("interactionRange").floatValue = 3f;
            so.FindProperty("closedColor").colorValue = ColDoor;
            so.FindProperty("openColor").colorValue = new Color(0.2f, 0.4f, 1f, 0.3f);
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static void MakeRestPoint(string name, Vector3 pos)
        {
            var go = MakeBlock(name, pos, new Vector3(1.5f, 0.6f, 1f), "Terrain", ColRestPoint);
            go.AddComponent<BoxCollider2D>();
            var rp = go.AddComponent<RestPoint>();
            var so = new SerializedObject(rp);
            so.FindProperty("interactionRange").floatValue = 2.5f;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static void CreateGameManagerObj()
        {
            var go = new GameObject("GameManager");
            go.AddComponent<GameManager>();
        }

        // ============================================================
        //  游戏场景 UI
        // ============================================================

        static void CreateGameCanvas(GameObject player)
        {
            var canvas = MakeCanvas("Canvas");

            // --- 血量 ---
            var hpGo = MakeText(canvas, "HealthDisplay", "♥ ♥ ♥ ♥ ♥",
                36, Color.red, TextAnchor.UpperLeft,
                new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1),
                new Vector2(30, -20), new Vector2(400, 60));
            var healthUI = hpGo.AddComponent<HealthUI>();
            SetRef(healthUI, "healthText", hpGo.GetComponent<Text>());

            // --- 操作提示 ---
            MakeText(canvas, "ControlHints",
                "A/D - 移动 | K - 跳跃 | J - 攻击\n" +
                "L - 闪避 | W/S - 梯子攀爬\n" +
                "F - 交互/拾取 | S+K - 穿越平台\n" +
                "Tab - 属性 | ESC - 暂停",
                16, new Color(1, 1, 1, 0.5f), TextAnchor.UpperRight,
                new Vector2(1, 1), new Vector2(1, 1), new Vector2(1, 1),
                new Vector2(-15, -15), new Vector2(320, 120));

            // --- 交互提示 ---
            MakeText(canvas, "InteractionPrompt", "按 F 交互",
                28, Color.white, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0),
                new Vector2(0, 80), new Vector2(400, 50));

            // --- Game Over 面板 ---
            BuildGameOverPanel(canvas);

            // --- 暂停面板 ---
            BuildPausePanel(canvas);

            // --- 属性面板 ---
            BuildStatsPanel(canvas);

            // --- 掉落物信息面板 ---
            BuildPickupPanel(canvas);
        }

        static void BuildGameOverPanel(Transform canvas)
        {
            var panel = MakePanel(canvas, "GameOverPanel", ColPanel, true);
            MakeText(panel, "GameOverText", "GAME OVER", 72, Color.red, TextAnchor.MiddleCenter,
                V(.5f, .6f), V(.5f, .6f), V(.5f, .5f), V(0, 0), V(600, 100));

            var (_, retryBtn) = MakeButton(panel, "RetryButton", "再次尝试",
                new Vector2(-120, -40), new Vector2(200, 55));
            var (_, menuBtn) = MakeButton(panel, "MenuButton", "返回主菜单",
                new Vector2(120, -40), new Vector2(200, 55));

            panel.gameObject.SetActive(false);

            var goUI = canvas.gameObject.AddComponent<GameOverUI>();
            var so = new SerializedObject(goUI);
            so.FindProperty("gameOverPanel").objectReferenceValue = panel.gameObject;
            so.FindProperty("retryButton").objectReferenceValue = retryBtn;
            so.FindProperty("menuButton").objectReferenceValue = menuBtn;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static void BuildPausePanel(Transform canvas)
        {
            var panel = MakePanel(canvas, "PausePanel", ColPanel, true);
            MakeText(panel, "PauseTitle", "暂停", 48, Color.white, TextAnchor.MiddleCenter,
                V(.5f, .7f), V(.5f, .7f), V(.5f, .5f), V(0, 0), V(300, 70));

            var (_, contBtn) = MakeButton(panel, "ContinueButton", "继续游戏", V(0, 30), V(260, 50));
            var (_, setBtn) = MakeButton(panel, "SettingsButton", "设置", V(0, -30), V(260, 50));
            var (_, retBtn) = MakeButton(panel, "ReturnMenuButton", "返回主菜单", V(0, -90), V(260, 50));

            var confirm = MakePanel(panel, "ConfirmPanel", new Color(0.1f, 0.1f, 0.14f, 0.95f), false);
            var crt = confirm.GetComponent<RectTransform>();
            crt.anchorMin = V(0.2f, 0.2f); crt.anchorMax = V(0.8f, 0.6f);
            crt.offsetMin = Vector2.zero; crt.offsetMax = Vector2.zero;

            MakeText(confirm, "ConfirmText", "您目前未保存的进度将会丢失！\n确认返回？",
                24, Color.white, TextAnchor.MiddleCenter,
                V(.5f, .65f), V(.5f, .65f), V(.5f, .5f), V(0, 0), V(400, 80));

            var (_, yesBtn) = MakeButton(confirm, "YesButton", "是", V(-80, -30), V(120, 45));
            var (_, noBtn) = MakeButton(confirm, "NoButton", "否", V(80, -30), V(120, 45));
            confirm.gameObject.SetActive(false);

            panel.gameObject.SetActive(false);

            var pauseUI = canvas.gameObject.AddComponent<PauseMenuUI>();
            var so = new SerializedObject(pauseUI);
            so.FindProperty("pausePanel").objectReferenceValue = panel.gameObject;
            so.FindProperty("confirmPanel").objectReferenceValue = confirm.gameObject;
            so.FindProperty("continueButton").objectReferenceValue = contBtn;
            so.FindProperty("settingsButton").objectReferenceValue = setBtn;
            so.FindProperty("returnMenuButton").objectReferenceValue = retBtn;
            so.FindProperty("confirmYesButton").objectReferenceValue = yesBtn;
            so.FindProperty("confirmNoButton").objectReferenceValue = noBtn;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static void BuildStatsPanel(Transform canvas)
        {
            var panel = MakePanel(canvas, "StatsPanel", new Color(0.08f, 0.08f, 0.12f, 0.92f), false);
            var prt = panel.GetComponent<RectTransform>();
            prt.anchorMin = V(0.3f, 0.25f); prt.anchorMax = V(0.7f, 0.75f);
            prt.offsetMin = Vector2.zero; prt.offsetMax = Vector2.zero;

            MakeText(panel, "StatsTitle", "角色属性", 32, Color.white, TextAnchor.MiddleCenter,
                V(.5f, .85f), V(.5f, .85f), V(.5f, .5f), V(0, 0), V(300, 50));
            var hpGo = MakeText(panel, "HPText", "HP:  5 / 5", 26, Color.white, TextAnchor.MiddleLeft,
                V(.15f, .6f), V(.15f, .6f), V(0, .5f), V(0, 0), V(350, 40));
            var atkGo = MakeText(panel, "ATKText", "攻击力:  4", 26, Color.white, TextAnchor.MiddleLeft,
                V(.15f, .45f), V(.15f, .45f), V(0, .5f), V(0, 0), V(350, 40));
            var eqGo = MakeText(panel, "EqText", "装备栏:  (空)", 26, new Color(0.6f, 0.6f, 0.6f),
                TextAnchor.MiddleLeft,
                V(.15f, .3f), V(.15f, .3f), V(0, .5f), V(0, 0), V(350, 40));

            panel.gameObject.SetActive(false);

            var statsUI = canvas.gameObject.AddComponent<StatsUI>();
            var so = new SerializedObject(statsUI);
            so.FindProperty("statsPanel").objectReferenceValue = panel.gameObject;
            so.FindProperty("hpText").objectReferenceValue = hpGo.GetComponent<Text>();
            so.FindProperty("attackText").objectReferenceValue = atkGo.GetComponent<Text>();
            so.FindProperty("equipmentText").objectReferenceValue = eqGo.GetComponent<Text>();
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static void BuildPickupPanel(Transform canvas)
        {
            var panel = MakePanel(canvas, "PickupPanel", new Color(0.06f, 0.06f, 0.1f, 0.92f), false);
            var prt = panel.GetComponent<RectTransform>();
            prt.anchorMin = V(0.2f, 0.25f); prt.anchorMax = V(0.8f, 0.75f);
            prt.offsetMin = Vector2.zero; prt.offsetMax = Vector2.zero;

            var nameGo = MakeText(panel, "PickupName", "物品名", 34, new Color(1f, 0.85f, 0.4f),
                TextAnchor.MiddleCenter,
                V(.5f, .8f), V(.5f, .8f), V(.5f, .5f), V(0, 0), V(500, 50));

            var descGo = MakeText(panel, "PickupDesc", "物品描述", 22, new Color(0.85f, 0.85f, 0.9f),
                TextAnchor.MiddleCenter,
                V(.5f, .45f), V(.5f, .45f), V(.5f, .5f), V(0, 0), V(500, 150));

            var hintGo = MakeText(panel, "PickupHint", "", 20, new Color(0.6f, 0.6f, 0.65f),
                TextAnchor.MiddleCenter,
                V(.5f, .12f), V(.5f, .12f), V(.5f, .5f), V(0, 0), V(400, 35));

            panel.gameObject.SetActive(false);

            var pickupUI = canvas.gameObject.AddComponent<PickupUI>();
            var so = new SerializedObject(pickupUI);
            so.FindProperty("pickupPanel").objectReferenceValue = panel.gameObject;
            so.FindProperty("nameText").objectReferenceValue = nameGo.GetComponent<Text>();
            so.FindProperty("descriptionText").objectReferenceValue = descGo.GetComponent<Text>();
            so.FindProperty("hintText").objectReferenceValue = hintGo.GetComponent<Text>();
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        // ============================================================
        //  主菜单场景
        // ============================================================

        static void BuildMainMenuScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            var cam = camGo.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 5f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.06f, 0.06f, 0.1f);
            camGo.AddComponent<AudioListener>();
            camGo.AddComponent<UniversalAdditionalCameraData>();

            CreateEventSystem();

            var canvas = MakeCanvas("Canvas");

            MakeText(canvas, "TitleEN", "Dreamless Land", 56, Color.white,
                TextAnchor.MiddleCenter,
                V(.5f, .78f), V(.5f, .78f), V(.5f, .5f), V(0, 0), V(700, 70));
            MakeText(canvas, "TitleCN", "无梦之界", 36, new Color(0.7f, 0.7f, 0.8f),
                TextAnchor.MiddleCenter,
                V(.5f, .68f), V(.5f, .68f), V(.5f, .5f), V(0, 0), V(400, 50));

            var (_, newBtn) = MakeButton(canvas, "NewGameButton", "新游戏", V(0, 60), V(280, 55), 28);
            var (_, contBtn) = MakeButton(canvas, "ContinueButton", "继续游戏", V(0, 0), V(280, 55), 28);
            var (_, instrBtn) = MakeButton(canvas, "InstructionsButton", "游戏说明", V(0, -60), V(280, 55), 28);
            var (_, quitBtn) = MakeButton(canvas, "QuitButton", "退出游戏", V(0, -120), V(280, 55), 28);

            // 说明面板
            var instrPanel = MakePanel(canvas, "InstructionsPanel",
                new Color(0.08f, 0.08f, 0.12f, 0.95f), false);
            var irt = instrPanel.GetComponent<RectTransform>();
            irt.anchorMin = V(0.15f, 0.1f); irt.anchorMax = V(0.85f, 0.9f);
            irt.offsetMin = Vector2.zero; irt.offsetMax = Vector2.zero;

            MakeText(instrPanel, "InstrTitle", "操作说明", 32, Color.white,
                TextAnchor.MiddleCenter,
                V(.5f, .9f), V(.5f, .9f), V(.5f, .5f), V(0, 0), V(300, 45));

            MakeText(instrPanel, "InstrBody",
                "A / D  或  ← / →  —  水平移动\n" +
                "K  —  跳跃\n" +
                "J  —  近战攻击\n" +
                "L  —  闪避（短暂无敌）\n" +
                "W / S  —  梯子上下攀爬\n" +
                "U  —  技能（待开发）\n" +
                "F  —  交互 / 拾取物品\n" +
                "S + K  —  从平台向下穿越\n" +
                "Tab  —  查看角色属性\n" +
                "ESC  —  暂停菜单\n\n" +
                "在休息点按 F 可回满血量、存档并刷新敌人。",
                20, new Color(0.85f, 0.85f, 0.9f), TextAnchor.UpperLeft,
                V(.1f, .5f), V(.9f, .82f), V(0, 1), V(0, 0), V(0, 0));
            var bodyRt = instrPanel.Find("InstrBody").GetComponent<RectTransform>();
            bodyRt.offsetMin = new Vector2(30, 60);
            bodyRt.offsetMax = new Vector2(-30, -50);

            var (_, closeBtn) = MakeButton(instrPanel, "CloseButton", "关闭", V(0, 0), V(160, 45), 24);
            var closeRt = instrPanel.Find("CloseButton").GetComponent<RectTransform>();
            closeRt.anchorMin = V(.5f, .08f); closeRt.anchorMax = V(.5f, .08f);

            instrPanel.gameObject.SetActive(false);

            var menuUI = canvas.gameObject.AddComponent<MainMenuUI>();
            var so = new SerializedObject(menuUI);
            so.FindProperty("newGameButton").objectReferenceValue = newBtn;
            so.FindProperty("continueButton").objectReferenceValue = contBtn;
            so.FindProperty("instructionsButton").objectReferenceValue = instrBtn;
            so.FindProperty("quitButton").objectReferenceValue = quitBtn;
            so.FindProperty("instructionsPanel").objectReferenceValue = instrPanel.gameObject;
            so.FindProperty("closeInstructionsButton").objectReferenceValue = closeBtn;
            so.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.SaveScene(scene, MenuScenePath);
        }

        // ============================================================
        //  UI 工具方法
        // ============================================================

        static Vector2 V(float x, float y) => new Vector2(x, y);

        static Transform MakeCanvas(string name)
        {
            var go = new GameObject(name);
            var c = go.AddComponent<Canvas>();
            c.renderMode = RenderMode.ScreenSpaceOverlay;
            c.sortingOrder = 100;
            var scaler = go.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            go.AddComponent<GraphicRaycaster>();
            return go.transform;
        }

        static void CreateEventSystem()
        {
            var go = new GameObject("EventSystem");
            go.AddComponent<UnityEngine.EventSystems.EventSystem>();
            go.AddComponent<StandaloneInputModule>();
        }

        static GameObject MakeText(Transform parent, string name, string content,
            int fontSize, Color color, TextAnchor align,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot,
            Vector2 anchoredPos, Vector2 sizeDelta)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = pivot;
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = sizeDelta;

            var t = go.AddComponent<Text>();
            t.text = content;
            t.fontSize = fontSize;
            t.color = color;
            t.font = GetFont();
            t.alignment = align;
            t.horizontalOverflow = HorizontalWrapMode.Overflow;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            return go;
        }

        static Transform MakePanel(Transform parent, string name, Color color, bool fullScreen)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            if (fullScreen)
            {
                rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
                rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
            }
            var img = go.AddComponent<UnityEngine.UI.Image>();
            img.color = color;
            return go.transform;
        }

        static (GameObject go, Button btn) MakeButton(Transform parent, string name,
            string label, Vector2 anchoredPos, Vector2 size, int fontSize = 24)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = V(.5f, .5f); rt.anchorMax = V(.5f, .5f);
            rt.pivot = V(.5f, .5f);
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = size;

            var img = go.AddComponent<UnityEngine.UI.Image>();
            img.color = ColBtnNormal;

            var btn = go.AddComponent<Button>();
            var cb = btn.colors;
            cb.normalColor = ColBtnNormal;
            cb.highlightedColor = ColBtnHover;
            cb.pressedColor = ColBtnPress;
            cb.selectedColor = ColBtnNormal;
            btn.colors = cb;

            var txtGo = new GameObject("Text");
            txtGo.transform.SetParent(go.transform, false);
            var trt = txtGo.AddComponent<RectTransform>();
            trt.anchorMin = Vector2.zero; trt.anchorMax = Vector2.one;
            trt.offsetMin = Vector2.zero; trt.offsetMax = Vector2.zero;
            var txt = txtGo.AddComponent<Text>();
            txt.text = label;
            txt.fontSize = fontSize;
            txt.color = Color.white;
            txt.font = GetFont();
            txt.alignment = TextAnchor.MiddleCenter;

            return (go, btn);
        }

        // ============================================================
        //  通用工具方法
        // ============================================================

        static GameObject MakeBlock(string name, Vector3 pos, Vector3 scale,
            string layer, Color color)
        {
            var go = new GameObject(name);
            go.transform.position = pos;
            go.transform.localScale = scale;
            SetLayer(go, layer);
            SetupSprite(go, color);
            return go;
        }

        static void SetLayer(GameObject go, string layerName)
        {
            int l = LayerMask.NameToLayer(layerName);
            if (l >= 0) go.layer = l;
        }

        static void SetupSprite(GameObject go, Color color)
        {
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = SpriteVisualHelper.CreateUnitSquareSprite();
            sr.color = color;

            var pv = go.AddComponent<PlaceholderVisual>();
            var so = new SerializedObject(pv);
            so.FindProperty("spriteColor").colorValue = color;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static void SetProps(Component comp, params (string name, object value)[] props)
        {
            var so = new SerializedObject(comp);
            foreach (var (n, v) in props)
            {
                var p = so.FindProperty(n);
                if (p == null) continue;
                switch (v)
                {
                    case int i: p.intValue = i; break;
                    case float f: p.floatValue = f; break;
                    case bool b: p.boolValue = b; break;
                    case string s: p.stringValue = s; break;
                    case Color c: p.colorValue = c; break;
                }
            }
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static void SetRef(Component comp, string propName, Object reference)
        {
            var so = new SerializedObject(comp);
            so.FindProperty(propName).objectReferenceValue = reference;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static void EnsureFolder(string path)
        {
            if (!AssetDatabase.IsValidFolder(path))
            {
                var parts = path.Split('/');
                string current = parts[0];
                for (int i = 1; i < parts.Length; i++)
                {
                    string next = current + "/" + parts[i];
                    if (!AssetDatabase.IsValidFolder(next))
                        AssetDatabase.CreateFolder(current, parts[i]);
                    current = next;
                }
            }
        }

        static void SetupBuildSettings()
        {
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(MenuScenePath, true),
                new EditorBuildSettingsScene(GameScenePath, true)
            };
        }
    }
}
#endif
