using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace HarborV2.Editor
{
    public static class HarborBuilder
    {
        public const string Root = "Assets/Scenes/HarborVillageV2";
        public const string ScenePath = "Assets/Scenes/HarborVillage.unity";
        private static readonly Dictionary<string,Sprite> sprites = new Dictionary<string,Sprite>();
        private static readonly List<HarborPoint> points = new List<HarborPoint>();
        private static Transform environment;
        public static readonly Vector2[] Profile = { new Vector2(0,4),new Vector2(14,4),new Vector2(22,2),new Vector2(42,2),new Vector2(48,0),new Vector2(64,0) };

        [MenuItem("Tools/Harbor Village/Open Scene")]
        public static void Open() { if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) EditorSceneManager.OpenScene(ScenePath); }
        [MenuItem("Tools/Harbor Village/Play Level")]
        public static void Play() { Open(); EditorApplication.delayCall += EditorApplication.EnterPlaymode; }
        [MenuItem("Tools/Harbor Village/Rebuild V2")]
        public static void Rebuild() { if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) Create(); }

        public static void Create()
        {
            foreach (string folder in new[] {"Art/Sprites","Materials","Meshes","Prefabs","Preview"}) Directory.CreateDirectory(Root+"/"+folder);
            AssetDatabase.Refresh(); sprites.Clear(); points.Clear();
            PrepareArt();
            // The source scene is read but never saved. Preserve every gameplay component and setting.
            EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity");
            var source = Object.FindAnyObjectByType<Player>();
            if (source == null) throw new Exception("SampleScene player missing");
            var copy = Object.Instantiate(source.gameObject);
            copy.transform.SetParent(null); copy.transform.position = Vector3.zero; copy.name = "ProjectPlayer";
            // Hide the debug overlay only; original HUD, Animator and gameplay components remain intact.
            foreach (var debug in copy.GetComponents<UI_AttackDebug>()) debug.enabled = false;
            copy.AddComponent<HarborPlayerLifetime>();
            var prefab = PrefabUtility.SaveAsPrefabAsset(copy,Root+"/Prefabs/ProjectPlayer.prefab");
            var parity = VerifyPlayer(source,copy.GetComponent<Player>());
            File.WriteAllText(Root+"/Preview/PlayerParity.txt",parity);
            Object.DestroyImmediate(copy);
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            environment = new GameObject("Harbor V2 - continuous quay / new artwork").transform;
            var backdrop = Art("Panorama",new Vector2(32,4),128,80,-100,false);
            backdrop.name = "Distant coast - full bleed parallax";
            BuildGround();
            Art("Cottage",new Vector2(8,3.96f),11,10,-10);
            Art("Inn",new Vector2(31,1.96f),14,15,-10);
            Art("Tower",new Vector2(45,-.04f),10,13,-10);
            for (int x=1;x<14;x+=4) Art("Fence",new Vector2(x,3.97f),3.9f,1.1f,5);
            for (int x=23;x<42;x+=4) Art("Fence",new Vector2(x,1.97f),3.9f,1.1f,5);
            for (int x=50;x<64;x+=4) Art("Fence",new Vector2(x,-.03f),3.9f,1.1f,5);
            foreach (var p in new[] {new Vector2(1,4),new Vector2(12,4),new Vector2(25,2),new Vector2(37,2),new Vector2(49,0),new Vector2(57,0)})
                Art("Barrel",p,1,1.3f,7);
            Art("Sign",new Vector2(25,5.1f),1.5f,1.8f,7);
            // Optional jump route is tightly supported by the quay, never floating over accidental holes.
            Ledge(18,Surface(18)+1.25f,2.2f);
            Ledge(20,Surface(20)+2.5f,2.2f);
            Art("Piling",new Vector2(18,Surface(18)-.1f),.7f,1.4f,3);
            Art("Piling",new Vector2(20,Surface(20)-.1f),.7f,2.65f,3);
            var quest = new GameObject("Last departure quest").AddComponent<HarborQuest>();
            quest.playerPrefab = prefab;
            quest.player = ((GameObject)PrefabUtility.InstantiatePrefab(prefab)).GetComponent<Player>();
            quest.player.transform.position = quest.spawn;
            var camera = new GameObject("Harbor follow camera").AddComponent<Camera>();
            camera.tag = "MainCamera"; camera.orthographic = true; camera.orthographicSize = 19.125f;
            camera.transform.position = new Vector3(32,6,-10); camera.backgroundColor = new Color(.08f,.12f,.19f);
            camera.clearFlags = CameraClearFlags.SolidColor; camera.gameObject.AddComponent<AudioListener>();
            camera.gameObject.AddComponent<UniversalAdditionalCameraData>();
            quest.view = camera.gameObject.AddComponent<HarborView>(); quest.view.target = quest.player.transform; quest.view.backdrop = backdrop.transform;
            var light = new GameObject("Global light - original player material").AddComponent<Light2D>();
            light.lightType = Light2D.LightType.Global; light.intensity = 1;
            Point(HarborPoint.Kind.Permit,"凭证宝箱","打开宝箱 · 取得航海凭证","Chest",11,4,1.3f,1);
            Point(HarborPoint.Kind.Checkpoint,"旅店港灯","点亮港灯 · 记录检查点","Lamp",33,2,.85f,2);
            Point(HarborPoint.Kind.Winch,"东码头绞盘","启动绞盘 · 开放栈桥","Winch",48,0,2,1.4f);
            Point(HarborPoint.Kind.Boat,"离港小船","登船离港",null,62,0,1,1);
            Art("Boat",new Vector2(62,-1.35f),4.8f,1.3f,12);
            Point(HarborPoint.Kind.Relic,"旧船徽章","拾取旧船徽章","Sign",2,4,.7f,.7f);
            Point(HarborPoint.Kind.Relic,"高处信物","拾取高处信物","Chest",20,Surface(20)+2.5f,.7f,.6f).radius = 1.25f;
            Point(HarborPoint.Kind.Relic,"码头纪念物","拾取码头纪念物","Sign",58,0,.7f,.7f);
            var barrier = Art("Crate",new Vector2(53,0),1.2f,4.3f,15);
            barrier.name = "Winch gate"; barrier.gameObject.layer = LayerMask.NameToLayer("Ground");
            var gateCollider = barrier.gameObject.AddComponent<BoxCollider2D>(); gateCollider.size = barrier.sprite.bounds.size;
            quest.gate = barrier.transform; quest.gateCollider = gateCollider; quest.points = points.ToArray();
            var crate = Art("Crate",new Vector2(38,2),1.25f,1.25f,10);
            crate.name = "Cargo training crate - original attack damage";
            crate.gameObject.layer = LayerMask.NameToLayer("Enemy");
            var crateCollider = crate.gameObject.AddComponent<BoxCollider2D>(); crateCollider.size = crate.sprite.bounds.size; crateCollider.isTrigger = true;
            crate.gameObject.AddComponent<HarborCrate>();
            Wall("West boundary",-1,8); Wall("East boundary",68,8);
            EditorSceneManager.SaveScene(scene,ScenePath); AssetDatabase.SaveAssets();
            Validate(); Render(camera,Root+"/Preview/Overview.png");
            Debug.Log("HARBOR_V2_CREATED");
        }

        public static float Surface(float x)
        {
            for(int i=0;i<Profile.Length-1;i++) if(x <= Profile[i+1].x)
                return Mathf.Lerp(Profile[i].y,Profile[i+1].y,Mathf.InverseLerp(Profile[i].x,Profile[i+1].x,x));
            return 0;
        }

        private static void PrepareArt()
        {
            var a=Load("Architecture");
            Slice(a,"Cottage",new RectInt(8,400,488,485),192,true);
            Slice(a,"Inn",new RectInt(493,240,552,640),240,true);
            Slice(a,"Tower",new RectInt(1046,240,484,640),208,true); Object.DestroyImmediate(a);
            var k=Load("Kit");
            Slice(k,"StoneLip",new RectInt(52,176,470,20),128,false);
            Slice(k,"DockFace",new RectInt(566,176,504,58),128,false);
            Slice(k,"Piling",new RectInt(1134,75,115,277),24,true);
            Slice(k,"Ladder",new RectInt(1325,65,165,289),32,true);
            Slice(k,"Barrel",new RectInt(60,405,222,263),32,true);
            Slice(k,"Crate",new RectInt(382,410,264,254),32,true);
            Slice(k,"Chest",new RectInt(758,468,241,192),40,true);
            Slice(k,"Winch",new RectInt(1098,399,425,264),64,true);
            Slice(k,"Lamp",new RectInt(65,694,177,286),24,true);
            Slice(k,"Boat",new RectInt(297,797,510,149),96,true);
            Slice(k,"Fence",new RectInt(846,760,347,210),64,true);
            Slice(k,"Sign",new RectInt(1262,694,253,286),32,true); Object.DestroyImmediate(k);
            var s=Load("Stone"); Slice(s,"StoneFill",new RectInt(0,0,s.width,s.height),128,false); Object.DestroyImmediate(s);
            var b=Load("Panorama"); Slice(b,"Panorama",new RectInt(0,0,b.width,b.height),960,false); Object.DestroyImmediate(b);
        }

        private static Texture2D Load(string file)
        { var texture=new Texture2D(2,2,TextureFormat.RGBA32,false); texture.LoadImage(File.ReadAllBytes(Root+"/Art/Source/"+file+".png")); return texture; }

        // Crop source margins BEFORE layout. Opaque terrain strips have no transparent border texels.
        private static void Slice(Texture2D source,string name,RectInt rect,int width,bool trim)
        {
            if(trim)
            {
                int left=rect.xMax,right=rect.x,top=rect.yMax,bottom=rect.y;
                for(int y=rect.y;y<rect.yMax;y++) for(int x=rect.x;x<rect.xMax;x++)
                    if(CropPixel(source,name,x,y).a > .5f) { left=Mathf.Min(left,x);right=Mathf.Max(right,x);top=Mathf.Min(top,y);bottom=Mathf.Max(bottom,y); }
                if(right<left) throw new Exception("Empty art: "+name);
                rect=new RectInt(left,top,right-left+1,bottom-top+1);
            }
            int height=Mathf.Max(1,Mathf.RoundToInt((float)rect.height/rect.width*width));
            var texture=new Texture2D(width,height,TextureFormat.RGBA32,false);
            for(int y=0;y<height;y++) for(int x=0;x<width;x++)
            {
                Color c=CropPixel(source,name,rect.x+Mathf.Min(rect.width-1,(int)((x+.5f)*rect.width/width)),rect.y+rect.height-1-Mathf.Min(rect.height-1,(int)((y+.5f)*rect.height/height)));
                // Hard alpha avoids a faint exterior halo. Solid ground is always fully opaque.
                c.a=trim ? (c.a>.5f ? 1:0):1; texture.SetPixel(x,y,c);
            }
            texture.Apply(); string path=Root+"/Art/Sprites/"+name+".png";
            File.WriteAllBytes(path,texture.EncodeToPNG()); Object.DestroyImmediate(texture);
            AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);
            var importer=(TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;
            importer.spritePixelsPerUnit=16;importer.filterMode=FilterMode.Point;importer.mipmapEnabled=false;
            importer.npotScale=TextureImporterNPOTScale.None;importer.textureCompression=TextureImporterCompression.Uncompressed;
            importer.wrapMode=trim ? TextureWrapMode.Clamp:TextureWrapMode.Repeat;importer.alphaIsTransparency=true;
            var settings=new TextureImporterSettings();importer.ReadTextureSettings(settings);settings.spriteMeshType=SpriteMeshType.FullRect;importer.SetTextureSettings(settings);
            importer.SaveAndReimport();sprites[name]=AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        // Exclude neighboring silhouettes that intrude into a rectangular atlas crop.
        private static Color CropPixel(Texture2D source,string name,int x,int topY)
        {
            if(name=="Cottage"&&x>=474&&topY<730)return Color.clear;
            if(name=="Inn"&&x>=1037&&topY<700)return Color.clear;
            return source.GetPixel(x,source.height-1-topY);
        }
        private static SpriteRenderer Art(string sprite,Vector2 position,float w,float h,int order,bool bottom=true)
        {
            var sr=new GameObject(sprite).AddComponent<SpriteRenderer>();sr.transform.SetParent(environment);
            sr.sprite=sprites[sprite];sr.transform.position=position+(bottom?Vector2.up*h/2:Vector2.zero);
            sr.transform.localScale=new Vector3(w/sr.sprite.bounds.size.x,h/sr.sprite.bounds.size.y,1);
            sr.sortingLayerName="Background";sr.sortingOrder=order;
            string matPath=Root+"/Materials/Sprite_"+sprite+".mat";
            var material=AssetDatabase.LoadAssetAtPath<Material>(matPath);
            if(material==null){material=new Material(Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default"));AssetDatabase.CreateAsset(material,matPath);}
            material.mainTexture=sr.sprite.texture;EditorUtility.SetDirty(material);sr.sharedMaterial=material;return sr;
        }

        private static void BuildGround()
        {
            var ground=new GameObject("Continuous ground - ONE closed collision path");ground.transform.SetParent(environment);
            ground.layer=LayerMask.NameToLayer("Ground");
            var polygon=ground.AddComponent<PolygonCollider2D>();
            polygon.points=Profile.Concat(new[]{new Vector2(64,-7),new Vector2(0,-7)}).ToArray();
            StripMesh("Continuous masonry",Profile.Take(5).ToArray(),-7,false,"StoneFill",0);
            StripMesh("Seamless stone coping",Profile.Take(5).ToArray(),.25f,true,"StoneLip",4);
            StripMesh("Seamless dock deck",new[]{new Vector2(47.98f,.006f),new Vector2(64,.006f)},.45f,true,"DockFace",4);
            for(int x=49;x<=64;x+=3) Art("Piling",new Vector2(x,-5),.6f,5,2);
        }

        private static void StripMesh(string name,Vector2[] top,float depth,bool relative,string textureName,int order)
        {
            var vertices=new List<Vector3>();var uv=new List<Vector2>();var triangles=new List<int>();
            for(int i=0;i<top.Length;i++)
            {
                float bottom=relative?top[i].y-depth:depth;
                vertices.Add(new Vector3(top[i].x,top[i].y,0));vertices.Add(new Vector3(top[i].x,bottom,0));
                uv.Add(new Vector2(top[i].x/(relative?4:6),relative?1:top[i].y/6));
                uv.Add(new Vector2(top[i].x/(relative?4:6),relative?0:bottom/6));
                if(i>0){int n=i*2;triangles.AddRange(new[]{n-2,n,n-1,n,n+1,n-1});}
            }
            string meshPath=Root+"/Meshes/"+textureName+".asset";
            var mesh=AssetDatabase.LoadAssetAtPath<Mesh>(meshPath);
            if(mesh==null){mesh=new Mesh();AssetDatabase.CreateAsset(mesh,meshPath);} else mesh.Clear();
            mesh.name=name;mesh.SetVertices(vertices);mesh.SetUVs(0,uv);mesh.SetTriangles(triangles,0);mesh.RecalculateBounds();mesh.RecalculateNormals();EditorUtility.SetDirty(mesh);
            string materialPath=Root+"/Materials/"+textureName+".mat";
            var material=AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            if(material==null){material=new Material(Shader.Find("Sprites/Default"));AssetDatabase.CreateAsset(material,materialPath);}
            material.mainTexture=sprites[textureName].texture;EditorUtility.SetDirty(material);
            var go=new GameObject(name);go.transform.SetParent(environment);go.AddComponent<MeshFilter>().sharedMesh=mesh;
            var renderer=go.AddComponent<MeshRenderer>();renderer.sharedMaterial=material;renderer.sortingLayerName="Background";renderer.sortingOrder=order;
        }

        private static HarborPoint Point(HarborPoint.Kind kind,string name,string prompt,string sprite,float x,float floor,float w,float h)
        {
            var go=new GameObject(name);go.transform.SetParent(environment);go.transform.position=new Vector3(x,floor+1.2f);
            var point=go.AddComponent<HarborPoint>();point.kind=kind;point.prompt=prompt;
            if(sprite!=null)point.visual=Art(sprite,new Vector2(x,floor-.025f),w,h,12);
            points.Add(point);return point;
        }

        private static void Ledge(float x,float y,float width)
        {
            var sr=Art("DockFace",new Vector2(x,y-.28f),width,.28f,6);
            sr.gameObject.layer=LayerMask.NameToLayer("Ground");var c=sr.gameObject.AddComponent<BoxCollider2D>();c.size=sr.sprite.bounds.size;c.usedByEffector=true;
            var effector=sr.gameObject.AddComponent<PlatformEffector2D>();effector.useOneWay=true;effector.surfaceArc=90;effector.useSideFriction=false;
        }
        private static void Wall(string name,float x,float y)
        {var go=new GameObject(name);go.transform.SetParent(environment);go.transform.position=new Vector3(x,y);go.layer=LayerMask.NameToLayer("Ground");go.AddComponent<BoxCollider2D>().size=new Vector2(1,35);}

        public static string VerifyPlayer(Player a,Player b)
        {
            var checks=new List<string>();
            foreach(Type type in new[]{typeof(Player),typeof(Rigidbody2D),typeof(CapsuleCollider2D),typeof(Player_Combat),typeof(Player_Execution),typeof(Player_PropulsionAmmo),typeof(Player_LethalAmmo),typeof(Player_ReloadProgress),typeof(Entity_Health)})
            {
                var ca=a.GetComponent(type);var cb=b.GetComponent(type);if(ca==null||cb==null)throw new Exception("Player component missing: "+type.Name);
                var soA=new SerializedObject(ca);var soB=new SerializedObject(cb);var p=soA.GetIterator();
                while(p.NextVisible(true))
                {
                    if(p.propertyPath.StartsWith("m_")||p.propertyType==SerializedPropertyType.Generic)continue;
                    var q=soB.FindProperty(p.propertyPath);if(q==null)throw new Exception("Player property missing: "+p.propertyPath);
                    string va=PropertyValue(p,a.transform),vb=PropertyValue(q,b.transform);
                    if(va!=vb)throw new Exception("Player differs: "+type.Name+"."+p.propertyPath+" "+va+" != "+vb);
                }
                // Native physics fields are m_-prefixed and need an exact JSON comparison.
                if(type==typeof(Rigidbody2D)||type==typeof(CapsuleCollider2D))
                {
                    var pa=new SerializedObject(ca).GetIterator();var sb=new SerializedObject(cb);
                    while(pa.NextVisible(true))
                    {
                        if(pa.propertyPath=="m_GameObject"||pa.propertyPath.Contains("Prefab")||pa.propertyPath.Contains("Corresponding")||pa.propertyType==SerializedPropertyType.Generic)continue;
                        var pb=sb.FindProperty(pa.propertyPath);
                        if(PropertyValue(pa,a.transform)!=PropertyValue(pb,b.transform))throw new Exception("Physics differs: "+pa.propertyPath);
                    }
                }
                checks.Add("PASS "+type.Name+" serialized configuration matches SampleScene");
            }
            if(a.GetComponentInChildren<Animator>().runtimeAnimatorController!=b.GetComponentInChildren<Animator>().runtimeAnimatorController)throw new Exception("Animator mismatch");
            checks.Add("PASS same original AnimatorController, clips, events and child sensors");
            checks.Add("moveSpeed="+b.moveSpeed+" jumpForce="+b.jumpForce+" dashSpeed="+b.dashSpeed+" dashDuration="+b.dashDuration+" gravity="+b.GetComponent<Rigidbody2D>().gravityScale);
            return string.Join("\n",checks);
        }

        private static string PropertyValue(SerializedProperty p,Transform root)
        {
            if(p.propertyType==SerializedPropertyType.ObjectReference)
            {
                var o=p.objectReferenceValue;if(o==null)return "null";
                string path=AssetDatabase.GetAssetPath(o);if(!string.IsNullOrEmpty(path))return path+"/"+o.name;
                Transform t=o is Component c?c.transform:o is GameObject go?go.transform:null;
                return t!=null?AnimationUtility.CalculateTransformPath(t,root)+"/"+o.GetType().Name:o.name;
            }
            return p.boxedValue?.ToString()??"null";
        }

        public static void Validate()
        {
            Physics2D.SyncTransforms();
            var ground=GameObject.Find("Continuous ground - ONE closed collision path").GetComponent<PolygonCollider2D>();
            for(float x=.125f;x<64;x+=.125f)
            {
                var hit=Physics2D.RaycastAll(new Vector2(x,Surface(x)+.1f),Vector2.down,.2f,LayerMask.GetMask("Ground")).FirstOrDefault(h=>h.collider==ground);
                if(hit.collider!=ground||Mathf.Abs(hit.point.y-Surface(x))>.015f)throw new Exception("Ground crack at "+x);
            }
            foreach(var t in Object.FindObjectsByType<Transform>())if(GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject)>0)throw new Exception("Missing script: "+t.name);
            File.WriteAllText(Root+"/Preview/GroundValidation.txt","PASS 511 ground ray probes across x=0.125..63.875. No collision gaps.\nPASS no missing scene scripts.\n");
            Debug.Log("HARBOR_V2_STATIC_PASS: 511 ground probes; no gaps");
        }

        public static void Render(Camera camera,string path)
        {
            var rt=new RenderTexture(1600,900,24);var previous=RenderTexture.active;camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;
            var texture=new Texture2D(1600,900,TextureFormat.RGB24,false);texture.ReadPixels(new Rect(0,0,1600,900),0,0);texture.Apply();
            File.WriteAllBytes(path,texture.EncodeToPNG());camera.targetTexture=null;RenderTexture.active=previous;rt.Release();Object.DestroyImmediate(texture);Object.DestroyImmediate(rt);
        }
    }
}




