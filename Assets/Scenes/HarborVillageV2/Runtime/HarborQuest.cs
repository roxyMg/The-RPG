using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace HarborV2
{
    public sealed class HarborQuest : MonoBehaviour
    {
        public GameObject playerPrefab;
        public Player player;
        public HarborView view;
        public HarborPoint[] points;
        public Transform gate;
        public Collider2D gateCollider;
        public Vector2 spawn = new Vector2(3, 5.3f);
        public bool HasPermit { get; private set; }
        public bool GateOpen { get; private set; }
        public bool Complete { get; private set; }
        public bool Paused { get; private set; }
        public int Relics { get; private set; }
        public int Falls { get; private set; }
        public Vector2 Checkpoint { get; private set; }
        public HarborPoint Nearest { get; private set; }
        public bool Replacing { get; private set; }
        private Vector3 gateOrigin;
        private SpriteRenderer gateVisual;
        private float elapsed, noticeUntil, gateLift;
        private string notice;
        private Font font;
        private GUIStyle title, label, small, button;
        private readonly List<InputAction> suspendedActions = new List<InputAction>();

        private void Awake()
        {
            gateOrigin = gate.localPosition; gateVisual = gate.GetComponent<SpriteRenderer>();
            Checkpoint = spawn;
            font = Font.CreateDynamicFontFromOSFont(new[] { "Microsoft YaHei", "SimHei", "Arial" }, 20);
        }

        private void Start()
        {
            view.target = player.transform;
            Notify("小屋前取回航海凭证，沿连续石道前往东码头。Q 交互。", 7);
        }

        private void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard != null)
            {
                if (keyboard.rKey.wasPressedThisFrame) Restart();
                if (keyboard.escapeKey.wasPressedThisFrame) TogglePause();
            }
            if (Paused || Complete || Replacing || player == null) return;
            elapsed += Time.deltaTime;
            gateLift = Mathf.MoveTowards(gateLift, GateOpen ? 5 : 0, Time.deltaTime * 4);
            gate.localPosition = gateOrigin + Vector3.up * gateLift;
            gateVisual.color = new Color(1, 1, 1, 1 - gateLift / 5);
            if (player.transform.position.y < -5 || player.GetComponent<Entity_Health>().IsDead)
            { Respawn(); return; }
            Nearest = null;
            float best = float.MaxValue;
            foreach (var point in points)
            {
                if (point.used && (point.kind == HarborPoint.Kind.Permit || point.kind == HarborPoint.Kind.Relic)) continue;
                float distance = Vector2.Distance(player.transform.position, point.transform.position);
                if (distance < point.radius && distance < best) { best = distance; Nearest = point; }
            }
            if (keyboard != null && keyboard.qKey.wasPressedThisFrame && Nearest != null) Interact(Nearest);
        }

        public void Interact(HarborPoint point)
        {
            if (Replacing || Paused || Complete || player == null || point == null ||
                Vector2.Distance(player.transform.position, point.transform.position) > point.radius || player.execution.IsExecuting) return;
            switch (point.kind)
            {
                case HarborPoint.Kind.Permit:
                    if (point.used) return;
                    HasPermit = true; point.used = true;
                    Notify("航海凭证已收好。沿石道向右，旅店港灯可设置检查点。", 5);
                    break;
                case HarborPoint.Kind.Checkpoint:
                    Checkpoint = new Vector2(point.transform.position.x, 3.3f); point.used = true;
                    Notify("港灯已记录归途。落水或倒下后从此处重新出发。", 4);
                    break;
                case HarborPoint.Kind.Winch:
                    if (!HasPermit) { Notify("需要先取得小屋前宝箱里的航海凭证。", 4); return; }
                    GateOpen = true; point.used = true; gateCollider.enabled = false;
                    Notify("栈桥已开放。沿码头向右，在小船旁按 Q 离港。", 5);
                    break;
                case HarborPoint.Kind.Boat:
                    if (!HasPermit || !GateOpen) { Notify("先取回凭证，并启动码头绞盘。", 4); return; }
                    Complete = true; Nearest = null;
                    // Freeze the round, including every original combat InputAction.
                    FreezeInputs(true);
                    break;
                case HarborPoint.Kind.Relic:
                    if (point.used) return;
                    point.used = true; Relics++;
                    Notify("旧港纪念物 " + Relics + " / 3", 3);
                    break;
            }
            point.Refresh();
        }

        public void Respawn()
        {
            if (Replacing) return;
            Falls++; StartCoroutine(ReplacePlayer(Checkpoint));
            Notify("回到港灯旁。凭证、机关和纪念物进度保留。", 4);
        }

        public void Restart()
        {
            if (Replacing) return;
            Paused = Complete = HasPermit = GateOpen = false; Time.timeScale = 1;
            suspendedActions.Clear();
            Relics = Falls = 0; elapsed = gateLift = 0;
            gate.localPosition = gateOrigin; gateCollider.enabled = true; gateVisual.color = Color.white;
            foreach (var point in points) { point.used = false; point.Refresh(); }
            foreach (var crate in FindObjectsByType<HarborCrate>()) crate.ResetCrate();
            Checkpoint = spawn; StartCoroutine(ReplacePlayer(spawn));
            Notify("新的航程。Q 交互；移动与战斗使用项目原有操作。", 5);
        }

        private IEnumerator ReplacePlayer(Vector2 position)
        {
            Replacing = true;
            if (player != null)
            {
                // Fresh instances also reset FSM, execution, knockback, ammo and input subscriptions.
                player.gameObject.SetActive(false);
                Destroy(player.gameObject);
            }
            yield return null;
            player = Instantiate(playerPrefab, position, Quaternion.identity).GetComponent<Player>();
            player.name = "Player (SampleScene configuration)";
            view.target = player.transform;
            Replacing = false;
        }

        public void TogglePause()
        {
            if (Complete || Replacing) return;
            Paused = !Paused; FreezeInputs(Paused);
        }

        private void FreezeInputs(bool freeze)
        {
            Time.timeScale = freeze ? 0 : 1;
            if (freeze)
            {
                suspendedActions.Clear(); suspendedActions.AddRange(InputSystem.ListEnabledActions());
                foreach (var action in suspendedActions) action.Disable();
            }
            else
            {
                foreach (var action in suspendedActions) action.Enable();
                suspendedActions.Clear();
            }
        }

        private void OnDisable() { Time.timeScale = 1; }
        private void OnDestroy() { if (font != null) Destroy(font); }
        private void Notify(string text, float duration) { notice = text; noticeUntil = Time.time + duration; }
        private static void Panel(Rect rect, Color color)
        { Color previous = GUI.color; GUI.color = color; GUI.DrawTexture(rect, Texture2D.whiteTexture); GUI.color = previous; }

        private void OnGUI()
        {
            if (title == null)
            {
                label = new GUIStyle(GUI.skin.label) { font = font, fontSize = 18, wordWrap = true, normal = { textColor = new Color(.90f,.92f,.94f) } };
                title = new GUIStyle(label) { fontSize = 27, fontStyle = FontStyle.Bold, normal = { textColor = new Color(.96f,.79f,.48f) } };
                small = new GUIStyle(label) { fontSize = 14 };
                button = new GUIStyle(GUI.skin.button) { font = font, fontSize = 20 };
            }
            var old = GUI.matrix;
            float scale = Mathf.Min(Screen.width / 1280f, Screen.height / 720f);
            GUI.matrix = Matrix4x4.TRS(new Vector3((Screen.width-1280*scale)/2, (Screen.height-720*scale)/2), Quaternion.identity, Vector3.one*scale);
            var ink = new Color(.035f,.055f,.085f,.94f);
            Panel(new Rect(851,24,405,137),ink);
            GUI.Label(new Rect(870,33,370,40),"月潮港 · 最后一班船",title);
            GUI.Label(new Rect(870,80,365,30),!HasPermit ? "01  取回小屋前的航海凭证" : !GateOpen ? "02  启动东码头绞盘" : "03  走到小船旁，登船离港",label);
            GUI.Label(new Rect(870,119,365,26),"纪念物 " + Relics + "/3   ·   " + (int)elapsed + " 秒   ·   落水 " + Falls,small);
            Panel(new Rect(24,650,1232,48),ink);
            GUI.Label(new Rect(38,659,1200,35),"AD 移动  空格 跳跃  Shift 冲刺  左键 攻击  右键 枪反  E 处决  F 派生  |  Q 交互  Tab 全景  Esc 暂停  R 重开",small);
            if (!Complete && !Paused && Nearest != null)
            {
                Panel(new Rect(440,585,400,45),ink);
                GUI.Label(new Rect(458,594,365,30),"[ Q ]  " + Nearest.prompt,label);
            }
            if (!Complete && !Paused && Time.time < noticeUntil)
            {
                Panel(new Rect(300,178,680,58),ink);
                GUI.Label(new Rect(316,189,648,45),notice,label);
            }
            if (Complete || Paused)
            {
                Panel(new Rect(0,0,1280,720),new Color(.02f,.03f,.05f,.8f));
                Panel(new Rect(390,238,500,244),ink);
                GUI.Label(new Rect(420,266,440,45),Complete ? "潮声相送 · 航程开启" : "潮汐暂歇 · 已暂停",title);
                GUI.Label(new Rect(420,329,440,55),Complete ? "凭证已交付 · 纪念物 " + Relics + "/3\n用时 " + (int)elapsed + " 秒" : "继续探索，或从小屋前重新出发。",label);
                if (GUI.Button(new Rect(420,410,208,44),Complete ? "再玩一次 [R]" : "继续 [Esc]",button))
                { if (Complete) Restart(); else TogglePause(); }
                if (!Complete && GUI.Button(new Rect(652,410,208,44),"重新开始 [R]",button)) Restart();
            }
            GUI.matrix = old;
        }
    }
}
