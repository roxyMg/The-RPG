using UnityEngine;

// 开发调试显示：仅观察玩家攻击状态，不读取或消费输入，不控制派生。
[RequireComponent(typeof(Player))]
public class UI_AttackDebug : MonoBehaviour
{
    [SerializeField] private bool showDebug = true;
    [Tooltip("默认仅在 Unity 编辑器的 Play Mode 中显示。")]
    [SerializeField] private bool editorOnly = true;
    private Player player;
    private GUIStyle panelStyle;
    private Font debugFont;
    private string debugText = "派生调试：等待玩家初始化";
    public string DebugText => debugText;
    public bool ShouldDisplay => showDebug && isActiveAndEnabled && Application.isPlaying &&
        (!editorOnly || Application.isEditor);

    private void Awake() => player = GetComponent<Player>();
    private void LateUpdate()
    {
        if (!ShouldDisplay)
            return;
        PlayerAttackState state = player != null ? player.attackState : null;
        if (state == null)
        {
            debugText = "派生调试：等待玩家初始化";
            return;
        }
        bool active = state.IsActive;
        Attack_MoveDefinition move = active ? state.CurrentMove : null;
        string current = !active ? "非攻击状态" : move != null ? move.name : "旧固定连段";
        string phase = !active ? "未在攻击" : state.IsTransitionRequested ? "已提交帧末衔接" :
            state.HasQueuedAttack ? "等待动画结束" : "等待派生输入";
        string queued = !active ? "无" : state.QueuedMove != null ? state.QueuedMove.name : state.HasQueuedAttack ? "下一段（旧固定连段）" : "无";
        string window = !active ? "未在攻击" : !state.UsesBranchWindow ? "未限制" :
            move.BranchWindowStart.ToString("F2") + "～" + move.BranchWindowEnd.ToString("F2") +
            " 秒（" + (state.IsBranchWindowOpen ? "开放" : "关闭") + "）";
        debugText = "攻击派生调试\n" +
            "当前招式：" + current + "\n" +
            "左键派生：" + MoveName(move != null ? move.NormalFollowUp : null) + "\n" +
            "F 键派生：" + MoveName(move != null ? move.AlternativeFollowUp : null) + "\n" +
            "招式计时：" + state.AttackElapsedTime.ToString("F2") + " 秒\n" +
            "输入窗口：" + window + "\n" +
            "最近派生输入：" + state.LastInputDescription + "\n" +
            "缓存招式：" + queued + "\n" +
            "衔接状态：" + phase;
    }
    private static string MoveName(Attack_MoveDefinition move) => move != null ? move.name : "无";
    private void OnGUI()
    {
        if (!ShouldDisplay)
            return;
        if (panelStyle == null)
        {
            debugFont = Font.CreateDynamicFontFromOSFont(new[] { "Microsoft YaHei", "SimHei", "Arial" }, 14);
            panelStyle = new GUIStyle(GUI.skin.box)
            {
                alignment = TextAnchor.UpperLeft,
                fontSize = 14,
                wordWrap = true,
                padding = new RectOffset(12, 12, 10, 10),
                font = debugFont
            };
        }
        float width = Mathf.Min(440, Mathf.Max(1, Screen.width - 24));
        float height = Mathf.Min(250, Mathf.Max(1, Screen.height - 24));
        GUI.Box(new Rect(Mathf.Max(0, Screen.width - width - 12), 12, width, height), debugText, panelStyle);
    }
    private void OnDestroy()
    {
        if (debugFont != null)
            UnityEngine.Object.Destroy(debugFont);
    }
}
