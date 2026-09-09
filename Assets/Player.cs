using UnityEngine;
public class Player : MonoBehaviour
{
    public Animator anim{ get; private set;}
    public Rigidbody2D rb{ get; private set;}
    private PlayerInputSet Input;
    public StateMachine stateMachine{ get; private set; }
    public PlayerIdleState idleState { get; private set; }
    public PlayerMoveState moveState { get; private set; }
    public Vector2 moveInput { get; private set; }
    [Header("Movement details")]
    public float moveSpeed;
    private bool facingRight = true;//定义面朝方向

    private void Awake()
    {
        anim = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody2D>();
        Input = new PlayerInputSet();
        stateMachine = new StateMachine();
        idleState = new PlayerIdleState(this, stateMachine,"Idle");
        moveState = new PlayerMoveState(this, stateMachine, "Move");
    }
    private void OnEnable()
    {
        Input.Enable();
        Input.Player.Movement.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        Input.Player.Movement.canceled += ctx => moveInput = Vector2.zero;
    }
    private void OnDisable()
    {
        Input.Disable();
    }
    private void Start()
    {
        stateMachine.Initialize(idleState);
    }
    private void Update()
    {
        stateMachine.UpdateActiveState();
    }
    public void SetVelocity(float xVelocity, float yVelocity)//设置速度
    {
        rb.linearVelocity = new Vector2(xVelocity, yVelocity);
        HandFlip(xVelocity);
    }
    private void HandFlip(float xVelocity)//角色如果向某方向移动，判断是否需要转向
    {
        if (xVelocity > 0 && facingRight == false)
        Flip();
        else if(xVelocity < 0 && facingRight)
        Flip();
        
    }
    private void Flip()//角色翻转
    {
        transform.Rotate(0,180,0);
        facingRight = !facingRight;
       
    }



}
