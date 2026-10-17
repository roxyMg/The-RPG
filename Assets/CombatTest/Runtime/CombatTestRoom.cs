using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CombatTest
{
    public sealed class CombatTestRoom : MonoBehaviour
    {
        public GameObject playerPrefab;
        public GameObject elitePrefab;
        public Font uiFont;
        public Player Player { get; private set; }
        public Enemy_TestElite Elite { get; private set; }
        public Entity_Health PlayerHealth { get; private set; }
        public Entity_Health EliteHealth { get; private set; }
        public int Round { get; private set; }
        public string Result { get; private set; }
        public bool Paused { get; private set; }
        private GameObject roundRoot;
        private float started;
        private bool restarting;
        private readonly List<InputAction> pausedActions = new List<InputAction>();
        private GUIStyle text, title, small, button;
        private readonly Color gold = new Color(.87f, .71f, .43f);

        private void Awake()
        {
            if (uiFont == null)
                uiFont = Font.CreateDynamicFontFromOSFont(new[] { "Microsoft YaHei", "SimHei", "Arial" },20);
        }
        private void Start() => RestartRound();

        public void RestartRound()
        {
            if (restarting) return;
            StartCoroutine(Restart());
        }

        private IEnumerator Restart()
        {
            restarting = true;
            Paused = false;
            pausedActions.Clear();
            Time.timeScale = 1;
            if (roundRoot != null)
            {
                roundRoot.SetActive(false); // Unsubscribe death listeners before creating replacements.
                Destroy(roundRoot);
                yield return null;
            }
            roundRoot = new GameObject("Round actors");
            roundRoot.transform.SetParent(transform);
            var p = Instantiate(playerPrefab, new Vector3(-7, 1.3f), Quaternion.identity, roundRoot.transform);
            var e = Instantiate(elitePrefab, new Vector3(6, 1.8f), Quaternion.identity, roundRoot.transform);
            p.name = "Player (test copy)";
            e.name = "墓地守卫 / Grave Warden";
            Player = p.GetComponent<Player>();
            PlayerHealth = p.GetComponent<Entity_Health>();
            Elite = e.GetComponent<Enemy_TestElite>();
            EliteHealth = e.GetComponent<Entity_Health>();
            Round++;
            Result = null;
            started = Time.time;
            restarting = false;
        }

        private void Update()
        {
            if (Camera.main != null)
                Camera.main.orthographicSize = Mathf.Max(9,16 / Camera.main.aspect);
            if (Keyboard.current != null)
            {
                if (Keyboard.current.rKey.wasPressedThisFrame) RestartRound();
                if (Keyboard.current.escapeKey.wasPressedThisFrame) TogglePause();
            }
            if (restarting || PlayerHealth == null || Result != null) return;
            if (PlayerHealth.IsDead) Result = "挑战失败 / DEFEAT";
            else if (EliteHealth == null || EliteHealth.IsDead) Result = "挑战完成 / VICTORY";
            else if (Player.transform.position.y < -5)
                PlayerHealth.TakeDamage(10000, transform, 0);
            if (Elite != null && !EliteHealth.IsDead && Elite.transform.position.y < -5)
                RestartRound(); // Test safety net: a lost actor must never strand the round.
        }

        public void TogglePause()
        {
            if (restarting || Player == null) return;
            Paused = !Paused;
            Time.timeScale = Paused ? 0 : 1;
            if (Paused)
            {
                pausedActions.Clear();
                pausedActions.AddRange(InputSystem.ListEnabledActions());
                foreach (var action in pausedActions) action.Disable();
            }
            else
            {
                foreach (var action in pausedActions) action.Enable();
                pausedActions.Clear();
            }
        }

        private void OnDisable() { Time.timeScale = 1; }

        private void Styles()
        {
            if (text != null) return;
            text = new GUIStyle(GUI.skin.label) { font = uiFont, fontSize = 20, normal = { textColor = new Color(.92f,.89f,.81f) } };
            title = new GUIStyle(text) { fontSize = 30, fontStyle = FontStyle.Bold };
            small = new GUIStyle(text) { fontSize = 16 };
            button = new GUIStyle(GUI.skin.button) { font = uiFont, fontSize = 20 };
        }

        private void RectFill(Rect rect, Color color)
        {
            Color previous = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = previous;
        }

        private void Bar(float x, float y, float w, float fill, Color color)
        {
            RectFill(new Rect(x,y,w,9), new Color(.05f,.06f,.09f,.9f));
            RectFill(new Rect(x,y,w * Mathf.Clamp01(fill),9), color);
        }

        private void OnGUI()
        {
            Styles();
            var oldMatrix = GUI.matrix;
            float scale = Mathf.Min(Screen.width / 1280f, Screen.height / 720f);
            GUI.matrix = Matrix4x4.TRS(new Vector3((Screen.width-1280*scale)*.5f,(Screen.height-720*scale)*.5f,0),Quaternion.identity,Vector3.one*scale);
            RectFill(new Rect(0,0,1280,128), new Color(.035f,.045f,.07f,.93f));
            RectFill(new Rect(30,26,3,76),gold);
            GUI.Label(new Rect(48,18,430,42), "暮色庭院 · 战斗试炼",title);
            GUI.Label(new Rect(48,63,450,28), "COMBAT LAB  /  v0.1  /  仅供测试",small);
            GUI.Label(new Rect(48,91,390,28), $"第 {Round:00} 回合     {(Time.time-started):0.0}s",small);
            GUI.Label(new Rect(650,21,450,30), "墓地守卫  /  GRAVE WARDEN",text);
            Bar(650,62,580,EliteHealth != null ? EliteHealth.NormalizedHealth : 0,new Color(.73f,.28f,.24f));
            string phase = Elite == null ? "已击败" : Elite.IsStunned ? "失衡：靠近按 E 处决" : Elite.CurrentPhase switch {
                Enemy_TestElite.Phase.Windup => Elite.MoveLabel + (Elite.CanBeCountered ? " · 金色前摇可枪反" : " · 红色前摇：跳跃躲避"),
                Enemy_TestElite.Phase.Active => Elite.MoveLabel + " · 攻击判定中",
                Enemy_TestElite.Phase.Recovery => "收招硬直 · 反击机会", _ => "接近目标，准备交战" };
            GUI.Label(new Rect(650,83,600,30),phase,small);
            RectFill(new Rect(0,605,1280,115),new Color(.035f,.045f,.07f,.94f));
            if (Player != null && PlayerHealth != null)
            {
                GUI.Label(new Rect(30,615,245,30),$"生命  {Mathf.Max(0,PlayerHealth.CurrentHealth):0} / 100",text);
                Bar(30,650,235,PlayerHealth.NormalizedHealth,new Color(.64f,.76f,.62f));
                GUI.Label(new Rect(300,615,460,30),$"枪反弹  {Player.lethalAmmo.CurrentAmmo}/3     推进弹  {Player.propulsionAmmo.CurrentAmmo}/3",text);
                var reload = Player.GetComponent<Player_ReloadProgress>();
                Bar(300,650,350,reload != null ? reload.NormalizedProgress : 0,new Color(.35f,.63f,.76f));
            }
            GUI.Label(new Rect(30,677,1120,28),"A / D 移动    空格 跳跃    左键 攻击    F 派生    右键 枪反    Shift 冲刺    E 处决    Esc 暂停",small);
            if (GUI.Button(new Rect(1050,620,190,44),"R  重开回合",button)) RestartRound();
            if (Paused || Result != null)
            {
                RectFill(new Rect(0,128,1280,477),new Color(.02f,.025f,.04f,.72f));
                GUI.Label(new Rect(455,265,500,55),Paused ? "已暂停 / PAUSED" : Result,title);
                GUI.Label(new Rect(455,325,500,35),"重开会恢复生命、弹药与敌人状态",text);
                if (GUI.Button(new Rect(455,384,350,48),"R  再来一局",button)) RestartRound();
                if (Paused && GUI.Button(new Rect(455,445,350,40),"Esc  继续",button)) TogglePause();
            }
            GUI.matrix = oldMatrix;
        }
    }
}
