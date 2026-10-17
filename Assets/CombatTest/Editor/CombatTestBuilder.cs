using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace CombatTest.Editor
{
    public static class CombatTestBuilder
    {
        public const string Root = "Assets/CombatTest";
        public const string ScenePath = Root + "/Scenes/CombatTestRoom.unity";
        public const string Delivery = "D:/BaiduNetdiskDownload/CombatTestRoom_v0.1";

        [MenuItem("Tools/Combat Test/Open Test Room")]
        public static void Open()
        {
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                EditorSceneManager.OpenScene(ScenePath);
        }

        // Batch entry point. Does not change EditorBuildSettings or save SampleScene.
        public static void Create()
        {
            foreach (string folder in new[] { "Scenes", "Prefabs", "Animations" })
                Directory.CreateDirectory(Root + "/" + folder);
            AssetDatabase.Refresh();
            foreach (string file in Directory.GetFiles(Root + "/Art", "*.png"))
            {
                var importer = (TextureImporter)AssetImporter.GetAtPath(file.Replace('\\','/'));
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.spritePixelsPerUnit = 16;
                importer.filterMode = FilterMode.Point;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.mipmapEnabled = false;
                importer.SaveAndReimport();
            }
            string markerPath = Root + "/Art/Marker.png";
            if (!File.Exists(markerPath))
            {
                var texture = new Texture2D(1,1);
                texture.SetPixel(0,0,Color.white);
                texture.Apply();
                File.WriteAllBytes(markerPath,texture.EncodeToPNG());
                Object.DestroyImmediate(texture);
            }
            AssetDatabase.ImportAsset(markerPath);
            var markerImporter = (TextureImporter)AssetImporter.GetAtPath(markerPath);
            markerImporter.textureType = TextureImporterType.Sprite;
            markerImporter.spritePixelsPerUnit = 1;
            markerImporter.filterMode = FilterMode.Point;
            markerImporter.textureCompression = TextureImporterCompression.Uncompressed;
            markerImporter.SaveAndReimport();

            var source = EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity");
            var originalPlayer = Object.FindFirstObjectByType<Player>();
            var originalEnemy = Object.FindFirstObjectByType<Enemy_Skeleton>();
            if (originalPlayer == null || originalEnemy == null) throw new Exception("Source actors missing");
            var p = Object.Instantiate(originalPlayer.gameObject);
            p.transform.SetParent(null);
            p.name = "TestPlayer";
            p.transform.SetPositionAndRotation(Vector3.zero,Quaternion.identity);
            foreach (var canvas in p.GetComponentsInChildren<Canvas>(true)) Object.DestroyImmediate(canvas.gameObject);
            foreach (var debug in p.GetComponents<UI_AttackDebug>()) Object.DestroyImmediate(debug);
            p.AddComponent<TestPlayerInputLifetime>();
            p.GetComponent<Rigidbody2D>().collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            var playerPrefab = PrefabUtility.SaveAsPrefabAsset(p,Root + "/Prefabs/TestPlayer.prefab");
            Object.DestroyImmediate(p);

            var e = Object.Instantiate(originalEnemy.gameObject);
            e.transform.SetParent(null);
            e.name = "GraveWarden";
            e.transform.SetPositionAndRotation(Vector3.zero,Quaternion.identity);
            e.transform.localScale = Vector3.one * 1.2f;
            foreach (var canvas in e.GetComponentsInChildren<Canvas>(true)) Object.DestroyImmediate(canvas.gameObject);
            var previous = e.GetComponent<Enemy_Skeleton>();
            string properties = EditorJsonUtility.ToJson(previous);
            Object.DestroyImmediate(e.GetComponent<Enemy_ExecutionTarget>());
            Object.DestroyImmediate(previous);
            var elite = e.AddComponent<Enemy_TestElite>();
            EditorJsonUtility.FromJsonOverwrite(properties,elite);
            BindEliteSensors(elite);
            elite.battleMoveSpeed = 2.7f;
            elite.minRetreatDistance = 0;
            elite.stunnedDuration = 2.8f;
            elite.deathFadeDuration = 1.2f;
            elite.markerSprite = AssetDatabase.LoadAssetAtPath<Sprite>(markerPath);
            e.AddComponent<Enemy_ExecutionTarget>();
            var hp = new SerializedObject(e.GetComponent<Enemy_Health>());
            hp.FindProperty("maxHP").floatValue = 240;
            hp.ApplyModifiedPropertiesWithoutUndo();
            var animator = e.GetComponentInChildren<Animator>();
            var originalController = animator.runtimeAnimatorController;
            var overrides = new AnimatorOverrideController(originalController);
            foreach (var clip in originalController.animationClips.Distinct())
            {
                if (clip.events.Length == 0) continue;
                var copy = Object.Instantiate(clip);
                copy.name = "Elite_" + clip.name;
                AnimationUtility.SetAnimationEvents(copy,Array.Empty<AnimationEvent>());
                AssetDatabase.CreateAsset(copy,Root + "/Animations/" + copy.name + ".anim");
                overrides[clip.name] = copy;
            }
            AssetDatabase.CreateAsset(overrides,Root + "/Animations/GraveWarden.overrideController");
            animator.runtimeAnimatorController = overrides;
            animator.GetComponent<SpriteRenderer>().color = new Color(.88f,.76f,.58f);
            var elitePrefab = PrefabUtility.SaveAsPrefabAsset(e,Root + "/Prefabs/GraveWarden.prefab");
            Object.DestroyImmediate(e);

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            var camera = new GameObject("Arena camera").AddComponent<Camera>();
            camera.tag = "MainCamera";
            camera.orthographic = true;
            camera.orthographicSize = 9;
            camera.transform.position = new Vector3(0,4.5f,-10);
            camera.backgroundColor = new Color(.075f,.065f,.12f);
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.gameObject.AddComponent<UniversalAdditionalCameraData>();
            camera.gameObject.AddComponent<AudioListener>();
            var light = new GameObject("Global 2D light").AddComponent<Light2D>();
            light.lightType = Light2D.LightType.Global;
            light.intensity = 1;
            var scenery = new GameObject("GothicVania - dusk courtyard (visual only)").transform;
            var bg = Art("background",Vector2.zero,-20,scenery);
            bg.transform.position = new Vector3(0,6,0);
            bg.transform.localScale = new Vector3(36 / bg.sprite.bounds.size.x,19 / bg.sprite.bounds.size.y,1);
            var mid = Art("middleground",new Vector2(0,5.2f),-15,scenery);
            mid.transform.localScale = Vector3.one * (34 / mid.sprite.bounds.size.x);
            mid.color = new Color(.6f,.64f,.77f);
            for (int i = -18; i < 18; i++)
            {
                // This source tile has nine transparent pixels above the walkable lip.
                Art("ground",new Vector2(i + .5f,-.9375f),-1,scenery);
                for (int y = 1; y < 5; y++) Art("ground-wall",new Vector2(i + .5f,-y-.5f),-2,scenery);
            }
            foreach (int x in new[] { -12, 12 })
            {
                var lamp = Art("street-lamp",new Vector2(x,0),-3,scenery);
                lamp.transform.position += Vector3.up * lamp.sprite.bounds.extents.y;
                var house = Art("house-b",new Vector2(x,0),-10,scenery);
                house.transform.position += Vector3.up * house.sprite.bounds.extents.y;
                house.color = new Color(.72f,.7f,.78f);
            }
            foreach (int x in new[] { -14, 13 })
            {
                var prop = Art(x < 0 ? "crate" : "barrel",new Vector2(x,.5f),-2,scenery);
            }
            Solid("Floor",new Vector2(0,-1),new Vector2(32,2));
            Solid("Left boundary",new Vector2(-15,5),new Vector2(1,14));
            Solid("Right boundary",new Vector2(15,5),new Vector2(1,14));
            Solid("Ceiling",new Vector2(0,15),new Vector2(32,1));
            var room = new GameObject("Combat Test Room - v0.1").AddComponent<CombatTestRoom>();
            room.playerPrefab = playerPrefab;
            room.elitePrefab = elitePrefab;
            // OS font is resolved at runtime so the repository does not redistribute a system font.
            EditorSceneManager.SaveScene(scene,ScenePath);
            AssetDatabase.SaveAssets();
            Validate();
            Debug.Log("COMBAT_TEST_SCENE_CREATED " + ScenePath);
        }

        private static SpriteRenderer Art(string name,Vector2 position,int order,Transform parent)
        {
            var sr = new GameObject(name).AddComponent<SpriteRenderer>();
            sr.transform.SetParent(parent);
            sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(Root + "/Art/" + name + ".png");
            sr.transform.position = position;
            sr.sortingLayerName = "Background";
            sr.sortingOrder = order;
            sr.sharedMaterial = AssetDatabase.GetBuiltinExtraResource<Material>("Sprites-Default.mat");
            return sr;
        }

        private static void Solid(string name,Vector2 position,Vector2 size)
        {
            var go = new GameObject(name);
            go.layer = LayerMask.NameToLayer("Ground");
            go.transform.position = position;
            go.AddComponent<BoxCollider2D>().size = size;
        }

        public static void Validate()
        {
            foreach (var path in new[] { Root + "/Prefabs/TestPlayer.prefab",Root + "/Prefabs/GraveWarden.prefab" })
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                foreach (var t in prefab.GetComponentsInChildren<Transform>(true))
                    if (GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject) != 0)
                        throw new Exception("Missing script: " + path + " / " + t.name);
                var entity = new SerializedObject(prefab.GetComponent<Entity>());
                foreach (string field in new[] { "groundCheck", "primaryWallCheck", "secondaryWallCheck" })
                    if (entity.FindProperty(field).objectReferenceValue == null)
                        throw new Exception("Missing sensor: " + path + " / " + field);
            }
            Debug.Log("COMBAT_TEST_STATIC_VALIDATION_PASSED");
        }

        private static void BindEliteSensors(Enemy_TestElite elite)
        {
            var serialized = new SerializedObject(elite);
            serialized.FindProperty("groundCheck").objectReferenceValue = elite.transform.Find("GroundCheck");
            foreach (string name in new[] { "primaryWallCheck", "secondaryWallCheck", "playerCheck" })
                serialized.FindProperty(name).objectReferenceValue = elite.transform;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        public static void RepairSensors()
        {
            string path = Root + "/Prefabs/GraveWarden.prefab";
            var go = PrefabUtility.LoadPrefabContents(path);
            BindEliteSensors(go.GetComponent<Enemy_TestElite>());
            PrefabUtility.SaveAsPrefabAsset(go,path);
            PrefabUtility.UnloadPrefabContents(go);
            Validate();
        }

        public static void Build()
        {
            Validate();
            Directory.CreateDirectory(Delivery + "/Windows");
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions {
                scenes = new[] { ScenePath },locationPathName = Delivery + "/Windows/CombatTestRoom.exe",
                target = BuildTarget.StandaloneWindows64,options = BuildOptions.Development });
            if (report.summary.result != BuildResult.Succeeded)
                throw new Exception("Combat test build failed: " + report.summary.result);
            Debug.Log("COMBAT_TEST_BUILD_PASSED " + report.summary.totalSize);
        }
    }
}
