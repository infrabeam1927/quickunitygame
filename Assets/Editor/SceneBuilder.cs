#if UNITY_EDITOR
using System.Collections.Generic;
using Game.Combat;
using Game.Core;
using Game.Enemies;
using Game.Player;
using Game.UI;
using Game.Utils;
using Game.Waves;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Game.EditorTools
{
    // Procedurally assembles the entire vertical-slice arena scene (camera, player,
    // walls, spawn points, prefabs, wave manager, HUD) from code. This avoids hand
    // authoring Unity's scene/prefab YAML and needs no external art files - every
    // sprite is generated at runtime by ShapeSpriteFactory.
    public static class SceneBuilder
    {
        private const string ScenesFolder = "Assets/Scenes";
        private const string PrefabsFolder = "Assets/Prefabs";
        private const string DataFolder = "Assets/Data/Waves";

        private const float ArenaHalfWidth = 8f;
        private const float ArenaHalfHeight = 5f;
        private const float WallThickness = 0.5f;

        [MenuItem("Tools/Vertical Slice/Build Arena Scene")]
        public static void BuildScene()
        {
            EnsureFolders();

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneSetupMode.SingleScene);

            BuildBackground();
            BuildCamera();
            BuildGameManagerObject();
            BuildArenaWalls();
            Transform[] spawnPoints = BuildSpawnPoints();

            GameObject impactVfx = BuildBurstVfxPrefab("ImpactVfx", new Color(1f, 0.95f, 0.6f), 10, 0.6f);
            GameObject deathVfx = BuildBurstVfxPrefab("DeathVfx", new Color(0.85f, 0.3f, 0.9f), 20, 1f);

            GameObject playerProjectile = BuildProjectilePrefab("PlayerProjectile", new Color(0.45f, 0.95f, 1f), 0.16f, impactVfx);
            GameObject enemyProjectile = BuildProjectilePrefab("EnemyProjectile", new Color(1f, 0.4f, 0.55f), 0.15f, impactVfx);

            GameObject chaserPrefab = BuildEnemyPrefab("ChaserEnemy", EnemyBehavior.Chaser, new Color(0.85f, 0.3f, 0.5f), null, deathVfx, 25f);
            GameObject shooterPrefab = BuildEnemyPrefab("ShooterEnemy", EnemyBehavior.Shooter, new Color(1f, 0.7f, 0.2f), enemyProjectile, deathVfx, 20f);

            GameObject player = BuildPlayer(playerProjectile);

            WaveData[] waveData = BuildWaveData();

            var waveManagerGO = new GameObject("WaveManager");
            var waveManager = waveManagerGO.AddComponent<WaveManager>();
            waveManager.Configure(waveData, chaserPrefab, shooterPrefab, spawnPoints, player.transform);

            BuildCanvas(player, waveManager);

            string scenePath = $"{ScenesFolder}/Arena.unity";
            EditorSceneManager.SaveScene(scene, scenePath);
            AddSceneToBuildSettings(scenePath);

            Debug.Log($"Vertical slice arena built and saved to {scenePath}. Press Play to test.");
        }

        private static void EnsureFolders()
        {
            EnsureFolder("Assets", "Scenes");
            EnsureFolder("Assets", "Prefabs");
            EnsureFolder("Assets", "Data");
            EnsureFolder("Assets/Data", "Waves");
        }

        private static void EnsureFolder(string parent, string name)
        {
            string path = $"{parent}/{name}";
            if (!AssetDatabase.IsValidFolder(path))
            {
                AssetDatabase.CreateFolder(parent, name);
            }
        }

        private static void BuildBackground()
        {
            var floor = new GameObject("Floor");
            var sr = floor.AddComponent<SpriteRenderer>();
            sr.sprite = ShapeSpriteFactory.Create(ShapeSpriteFactory.Shape.Square, 64, new Color(0.09f, 0.10f, 0.16f));
            sr.sortingOrder = -10;
            floor.transform.localScale = new Vector3(ArenaHalfWidth * 2f, ArenaHalfHeight * 2f, 1f);
        }

        private static void BuildCamera()
        {
            var camGO = new GameObject("Main Camera");
            camGO.tag = "MainCamera";
            var cam = camGO.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 6f;
            cam.backgroundColor = new Color(0.05f, 0.055f, 0.09f);
            cam.clearFlags = CameraClearFlags.SolidColor;
            camGO.transform.position = new Vector3(0f, 0f, -10f);
            camGO.AddComponent<AudioListener>();
            camGO.AddComponent<ScreenShake>();
        }

        private static void BuildGameManagerObject()
        {
            new GameObject("GameManager").AddComponent<GameManager>();
        }

        private static void BuildArenaWalls()
        {
            CreateWall("Wall_Top", new Vector2(0f, ArenaHalfHeight + WallThickness / 2f),
                new Vector2(ArenaHalfWidth * 2f + WallThickness * 2f, WallThickness));
            CreateWall("Wall_Bottom", new Vector2(0f, -ArenaHalfHeight - WallThickness / 2f),
                new Vector2(ArenaHalfWidth * 2f + WallThickness * 2f, WallThickness));
            CreateWall("Wall_Left", new Vector2(-ArenaHalfWidth - WallThickness / 2f, 0f),
                new Vector2(WallThickness, ArenaHalfHeight * 2f));
            CreateWall("Wall_Right", new Vector2(ArenaHalfWidth + WallThickness / 2f, 0f),
                new Vector2(WallThickness, ArenaHalfHeight * 2f));
        }

        private static void CreateWall(string name, Vector2 position, Vector2 size)
        {
            var wall = new GameObject(name);
            wall.transform.position = position;
            wall.transform.localScale = new Vector3(size.x, size.y, 1f);

            var sr = wall.AddComponent<SpriteRenderer>();
            sr.sprite = ShapeSpriteFactory.Create(ShapeSpriteFactory.Shape.Square, 64, new Color(0.2f, 0.22f, 0.32f));
            sr.sortingOrder = 0;

            wall.AddComponent<BoxCollider2D>();
            wall.AddComponent<Wall>();
        }

        private static Transform[] BuildSpawnPoints()
        {
            var parent = new GameObject("SpawnPoints").transform;
            Vector2[] points =
            {
                new Vector2(0f, 4f), new Vector2(0f, -4f),
                new Vector2(7f, 0f), new Vector2(-7f, 0f),
                new Vector2(7f, 4f), new Vector2(-7f, 4f),
                new Vector2(7f, -4f), new Vector2(-7f, -4f)
            };

            var result = new Transform[points.Length];
            for (int i = 0; i < points.Length; i++)
            {
                var sp = new GameObject($"SpawnPoint_{i}").transform;
                sp.SetParent(parent, false);
                sp.position = points[i];
                result[i] = sp;
            }
            return result;
        }

        private static GameObject BuildProjectilePrefab(string name, Color color, float radius, GameObject impactVfx)
        {
            var go = new GameObject(name);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = ShapeSpriteFactory.Create(ShapeSpriteFactory.Shape.Circle, 64, color);
            sr.sortingOrder = 15;
            go.transform.localScale = Vector3.one * (radius * 2f);

            var rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;

            var col = go.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.5f;

            var projectile = go.AddComponent<Projectile>();
            projectile.SetImpactVfx(impactVfx);

            return SaveAsPrefabAndDestroy(go, $"{PrefabsFolder}/{name}.prefab");
        }

        private static GameObject BuildEnemyPrefab(string name, EnemyBehavior behavior, Color color, GameObject projectilePrefab, GameObject deathVfx, float maxHealth)
        {
            var go = new GameObject(name);

            var shape = behavior == EnemyBehavior.Chaser ? ShapeSpriteFactory.Shape.Diamond : ShapeSpriteFactory.Shape.Ring;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = ShapeSpriteFactory.Create(shape, 64, color);
            sr.sortingOrder = 5;

            var rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;

            var col = go.AddComponent<CircleCollider2D>();
            col.radius = 0.42f;

            var health = go.AddComponent<Health>();
            health.SetMaxHealth(maxHealth);

            var hitFlash = go.AddComponent<HitFlash>();
            var feedback = go.AddComponent<DamageFeedback>();
            feedback.SetHitFlash(hitFlash);
            feedback.SetDeathVfx(deathVfx);

            var ai = go.AddComponent<EnemyAI>();
            ai.SetBehavior(behavior);

            if (behavior == EnemyBehavior.Shooter)
            {
                var firePoint = new GameObject("FirePoint").transform;
                firePoint.SetParent(go.transform, false);
                ai.SetFirePoint(firePoint);
                ai.SetProjectilePrefab(projectilePrefab);
            }

            return SaveAsPrefabAndDestroy(go, $"{PrefabsFolder}/{name}.prefab");
        }

        private static GameObject BuildBurstVfxPrefab(string name, Color color, int burstCount, float sizeMultiplier)
        {
            var go = new GameObject(name);
            var ps = go.AddComponent<ParticleSystem>();

            var main = ps.main;
            main.duration = 0.4f;
            main.loop = false;
            main.startLifetime = 0.35f;
            main.startSpeed = 3f * sizeMultiplier;
            main.startSize = 0.12f * sizeMultiplier;
            main.startColor = color;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.stopAction = ParticleSystemStopAction.Destroy;

            var emission = ps.emission;
            emission.rateOverTime = 0f;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)burstCount) });

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 0.05f;

            var renderer = go.GetComponent<ParticleSystemRenderer>();
            renderer.material = new Material(Shader.Find("Sprites/Default"));

            return SaveAsPrefabAndDestroy(go, $"{PrefabsFolder}/{name}.prefab");
        }

        private static GameObject BuildPlayer(GameObject projectilePrefab)
        {
            var player = new GameObject("Player");
            player.transform.position = Vector3.zero;

            var sr = player.AddComponent<SpriteRenderer>();
            sr.sprite = ShapeSpriteFactory.Create(ShapeSpriteFactory.Shape.Circle, 64, new Color(0.5f, 0.85f, 1f));
            sr.sortingOrder = 10;

            var rb = player.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;

            var col = player.AddComponent<CircleCollider2D>();
            col.radius = 0.5f;

            player.AddComponent<Health>();
            player.AddComponent<PlayerHealth>();

            var hitFlash = player.AddComponent<HitFlash>();
            var feedback = player.AddComponent<DamageFeedback>();
            feedback.SetHitFlash(hitFlash);
            feedback.SetShakeCameraOnHit(true);

            var aimPivot = new GameObject("AimPivot").transform;
            aimPivot.SetParent(player.transform, false);

            var muzzle = new GameObject("MuzzleMarker");
            muzzle.transform.SetParent(aimPivot, false);
            var muzzleSr = muzzle.AddComponent<SpriteRenderer>();
            muzzleSr.sprite = ShapeSpriteFactory.Create(ShapeSpriteFactory.Shape.Triangle, 64, new Color(0.85f, 0.98f, 1f));
            muzzleSr.sortingOrder = 11;
            muzzle.transform.localPosition = new Vector3(0.35f, 0f, 0f);
            muzzle.transform.localScale = Vector3.one * 0.45f;

            var firePoint = new GameObject("FirePoint").transform;
            firePoint.SetParent(aimPivot, false);
            firePoint.localPosition = new Vector3(0.6f, 0f, 0f);

            var controller = player.AddComponent<PlayerController>();
            controller.SetAimPivot(aimPivot);
            controller.SetCamera(Camera.main);

            var weapon = player.AddComponent<WeaponController>();
            weapon.SetAimPivot(aimPivot);
            weapon.SetFirePoint(firePoint);
            weapon.SetProjectilePrefab(projectilePrefab);

            return player;
        }

        private static WaveData[] BuildWaveData()
        {
            var definitions = new (string name, EnemySpawnEntry[] enemies, float interval, float delay, string flavor)[]
            {
                ("Wave_1_FirstBloom", new[] { new EnemySpawnEntry { kind = EnemyKind.Chaser, count = 3 } }, 1.0f, 1.5f,
                    "Spore drifters detected at the airlock. Hold the line, WARD-7."),
                ("Wave_2_Creepers", new[] { new EnemySpawnEntry { kind = EnemyKind.Chaser, count = 5 } }, 0.8f, 3f,
                    "More of them. The Seed-Core's light is drawing them in."),
                ("Wave_3_Turrets", new[]
                {
                    new EnemySpawnEntry { kind = EnemyKind.Chaser, count = 4 },
                    new EnemySpawnEntry { kind = EnemyKind.Shooter, count = 2 }
                }, 0.7f, 3f, "New signature: ranged spore-turrets. Keep moving."),
                ("Wave_4_Bloomrush", new[]
                {
                    new EnemySpawnEntry { kind = EnemyKind.Chaser, count = 6 },
                    new EnemySpawnEntry { kind = EnemyKind.Shooter, count = 3 }
                }, 0.6f, 3f, "The infection is spreading faster."),
                ("Wave_5_Overgrowth", new[]
                {
                    new EnemySpawnEntry { kind = EnemyKind.Chaser, count = 8 },
                    new EnemySpawnEntry { kind = EnemyKind.Shooter, count = 4 }
                }, 0.5f, 3f, "Almost through. Don't let them reach the core."),
                ("Wave_6_FinalBloom", new[]
                {
                    new EnemySpawnEntry { kind = EnemyKind.Chaser, count = 10 },
                    new EnemySpawnEntry { kind = EnemyKind.Shooter, count = 6 }
                }, 0.4f, 4f, "Final bloom. Protect the Seed-Core, WARD-7.")
            };

            var result = new WaveData[definitions.Length];
            for (int i = 0; i < definitions.Length; i++)
            {
                var def = definitions[i];
                var wave = ScriptableObject.CreateInstance<WaveData>();
                wave.waveName = def.name;
                wave.enemies = def.enemies;
                wave.spawnInterval = def.interval;
                wave.delayBeforeWave = def.delay;
                wave.flavorText = def.flavor;

                AssetDatabase.CreateAsset(wave, $"{DataFolder}/{def.name}.asset");
                result[i] = wave;
            }
            return result;
        }

        private static void BuildCanvas(GameObject player, WaveManager waveManager)
        {
            var canvasGO = new GameObject("Canvas");
            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280f, 720f);

            canvasGO.AddComponent<GraphicRaycaster>();

            var eventSystemGO = new GameObject("EventSystem");
            eventSystemGO.AddComponent<EventSystem>();
            eventSystemGO.AddComponent<StandaloneInputModule>();

            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            // --- HUD ---
            var hudGO = new GameObject("HUD");
            hudGO.transform.SetParent(canvasGO.transform, false);

            FillBar healthBar = CreateBar(hudGO.transform, "HealthBar", new Vector2(20f, -20f), new Vector2(240f, 24f),
                new Color(0.15f, 0.16f, 0.22f), new Color(0.4f, 0.9f, 0.5f));

            FillBar heatBar = CreateBar(hudGO.transform, "HeatBar", new Vector2(20f, -52f), new Vector2(240f, 14f),
                new Color(0.15f, 0.16f, 0.22f), new Color(1f, 0.55f, 0.25f));

            Text scoreText = CreateText(hudGO.transform, "ScoreText", "Score: 0", font, 22,
                new Vector2(-20f, -20f), new Vector2(220f, 30f), TextAnchor.UpperRight, AnchorPreset.UpperRight);

            Text waveText = CreateText(hudGO.transform, "WaveText", "Wave 0 / 0", font, 22,
                new Vector2(-20f, -52f), new Vector2(220f, 30f), TextAnchor.UpperRight, AnchorPreset.UpperRight);

            var hudController = hudGO.AddComponent<HUDController>();
            var playerHealth = player.GetComponent<Health>();
            var weapon = player.GetComponent<WeaponController>();
            hudController.Configure(healthBar, heatBar, scoreText, waveText, playerHealth, weapon, waveManager);

            // --- Game Over panel ---
            var panelGO = new GameObject("GameOverPanel");
            panelGO.transform.SetParent(canvasGO.transform, false);
            var panelRect = panelGO.AddComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;
            var panelImage = panelGO.AddComponent<Image>();
            panelImage.color = new Color(0.02f, 0.02f, 0.04f, 0.85f);

            Text titleText = CreateText(panelGO.transform, "TitleText", "Garden Secured", font, 42,
                new Vector2(0f, 40f), new Vector2(600f, 60f), TextAnchor.MiddleCenter, AnchorPreset.MiddleCenter);

            Text finalScoreText = CreateText(panelGO.transform, "FinalScoreText", "Final Score: 0", font, 26,
                new Vector2(0f, -20f), new Vector2(400f, 40f), TextAnchor.MiddleCenter, AnchorPreset.MiddleCenter);

            var buttonGO = new GameObject("RestartButton");
            buttonGO.transform.SetParent(panelGO.transform, false);
            var buttonRect = buttonGO.AddComponent<RectTransform>();
            buttonRect.sizeDelta = new Vector2(200f, 50f);
            buttonRect.anchoredPosition = new Vector2(0f, -90f);
            SetAnchorPreset(buttonRect, AnchorPreset.MiddleCenter);
            var buttonImage = buttonGO.AddComponent<Image>();
            buttonImage.color = new Color(0.25f, 0.55f, 0.9f);
            var button = buttonGO.AddComponent<Button>();

            CreateText(buttonGO.transform, "Label", "Restart", font, 24,
                Vector2.zero, new Vector2(200f, 50f), TextAnchor.MiddleCenter, AnchorPreset.MiddleCenter);

            var gameOverUI = panelGO.AddComponent<GameOverUI>();
            gameOverUI.Configure(panelGO, titleText, finalScoreText, button);
        }

        private enum AnchorPreset { UpperLeft, UpperRight, MiddleCenter }

        private static FillBar CreateBar(Transform parent, string name, Vector2 anchoredPosition, Vector2 size, Color bgColor, Color fillColor)
        {
            var bgGO = new GameObject(name);
            bgGO.transform.SetParent(parent, false);
            var bgRect = bgGO.AddComponent<RectTransform>();
            bgRect.sizeDelta = size;
            bgRect.anchoredPosition = anchoredPosition;
            SetAnchorPreset(bgRect, AnchorPreset.UpperLeft);
            var bgImage = bgGO.AddComponent<Image>();
            bgImage.color = bgColor;

            var fillGO = new GameObject("Fill");
            fillGO.transform.SetParent(bgGO.transform, false);
            var fillRect = fillGO.AddComponent<RectTransform>();
            fillRect.anchorMin = new Vector2(0f, 0f);
            fillRect.anchorMax = new Vector2(1f, 1f);
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;
            var fillImage = fillGO.AddComponent<Image>();
            fillImage.color = fillColor;

            var bar = bgGO.AddComponent<FillBar>();
            bar.SetFillRect(fillRect);
            return bar;
        }

        private static Text CreateText(Transform parent, string name, string content, Font font, int fontSize,
            Vector2 anchoredPosition, Vector2 sizeDelta, TextAnchor alignment, AnchorPreset anchorPreset)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();
            rect.sizeDelta = sizeDelta;
            rect.anchoredPosition = anchoredPosition;
            SetAnchorPreset(rect, anchorPreset);

            var text = go.AddComponent<Text>();
            text.font = font;
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.color = Color.white;
            text.text = content;
            return text;
        }

        private static void SetAnchorPreset(RectTransform rect, AnchorPreset preset)
        {
            switch (preset)
            {
                case AnchorPreset.UpperLeft:
                    rect.anchorMin = new Vector2(0f, 1f);
                    rect.anchorMax = new Vector2(0f, 1f);
                    rect.pivot = new Vector2(0f, 1f);
                    break;
                case AnchorPreset.UpperRight:
                    rect.anchorMin = new Vector2(1f, 1f);
                    rect.anchorMax = new Vector2(1f, 1f);
                    rect.pivot = new Vector2(1f, 1f);
                    break;
                default:
                    rect.anchorMin = new Vector2(0.5f, 0.5f);
                    rect.anchorMax = new Vector2(0.5f, 0.5f);
                    rect.pivot = new Vector2(0.5f, 0.5f);
                    break;
            }
        }

        private static GameObject SaveAsPrefabAndDestroy(GameObject instance, string path)
        {
            var prefab = PrefabUtility.SaveAsPrefabAsset(instance, path);
            Object.DestroyImmediate(instance);
            return prefab;
        }

        private static void AddSceneToBuildSettings(string scenePath)
        {
            var scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            if (!scenes.Exists(s => s.path == scenePath))
            {
                scenes.Add(new EditorBuildSettingsScene(scenePath, true));
                EditorBuildSettings.scenes = scenes.ToArray();
            }
        }
    }
}
#endif
