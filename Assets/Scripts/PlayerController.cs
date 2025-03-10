using DG.Tweening;
using HeroicEngine.Components.Combat;
using HeroicEngine.Enums;
using HeroicEngine.Systems;
using HeroicEngine.Systems.Audio;
using HeroicEngine.Systems.DI;
using HeroicEngine.Systems.Gameplay;
using HeroicEngine.Systems.Inputs;
using HeroicEngine.Systems.UI;
using HeroicEngine.Utils.Math;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : Hittable, IInjectable
{
    private readonly int RunAnimHash = Animator.StringToHash("Running");
    private readonly int AttackAnimHash = Animator.StringToHash("Attack");
    private readonly int DeathAnimHash = Animator.StringToHash("Death");
    private readonly int JumpAnimHash = Animator.StringToHash("Jump");
    private readonly int AnimVariationHash = Animator.StringToHash("AnimVariation");

    [SerializeField] private Animator animator;

    [Header("Movement")]
    [SerializeField] [Min(0f)] private float speed = 5f;
    [SerializeField] [Min(0f)] private float jumpForce = 10f;
    [SerializeField] private Transform groundCheckPivot;
    [SerializeField] private LayerMask groundCheckLayers;
    [SerializeField][Min(0f)] private float groundCheckRadius = 0.1f;

    [Header("Combat")]
    [SerializeField] [Min(0f)] private float attackDistance = 1.5f;
    [SerializeField] [Min(0f)] private float attackPower = 1f;

    [Header("Sounds")]
    [SerializeField] private List<AudioClip> attackSounds;
    [SerializeField] private List<AudioClip> getDamageSounds;
    [SerializeField] private AudioClip jumpSound;

    [Inject] private IInputManager inputManager;
    [Inject] private CameraController cameraController;
    [Inject] private IHittablesManager hittablesManager;
    [Inject] private IUIController uiController;
    [Inject] private ISoundsManager soundsManager;

    #region Private Params
    private Rigidbody2D rb;
    private bool isGrounded;
    private float moveDirection;
    private float lastDirection;
    private Collider2D[] groundCheckResults = new Collider2D[1];
    private Slider hpBar;
    #endregion

    #region Lifetime Methods

    public void PostInject()
    {
    }

    protected override void Awake()
    {
        base.Awake();
        rb = GetComponent<Rigidbody2D>();
    }

    protected override void Start()
    {
        base.Start();

        InjectionManager.RegisterObject(this);

        uiController.ShowUIParts(UIPartType.PlayerHealthBar);

        hpBar = uiController
            .GetUIPartsOfType(UIPartType.PlayerHealthBar)[0]
            .GetComponent<Slider>();

        currHealth = maxHealth;

        hpBar.value = GetHPPercentage();

        cameraController.SetPlayerTransform(transform);

        inputManager.AddKeyDownListener(KeyCode.Space, Jump);
        inputManager.AddKeyDownListener(KeyCode.Mouse0, StartAttack);

        SubscribeToDeath(PlayDeathAnim);
        SubscribeToDamageGot(OnDamaged);
        SubscribeToHealingGot(UpdateHealthBar);
    }

    private void Update()
    {
        if (IsDead())
        {
            return;
        }

        moveDirection = inputManager.GetMovementDirection().x;
        rb.velocity = new Vector2(moveDirection * speed, rb.velocity.y);

        if (Mathf.Abs(moveDirection) > 0f)
        {
            animator.SetBool(RunAnimHash, true);
            lastDirection = moveDirection;
        }
        else
        {
            animator.SetBool(RunAnimHash, false);
        }

        CheckGround();

        ChangeAnimVariation();
    }

    private void LateUpdate()
    {
        transform.rotation = Quaternion.Euler(0f, lastDirection >= 0f ? 0f : 180f, 0f);
    }

    private void OnDisable()
    {
        inputManager.RemoveKeyDownListener(KeyCode.Space, Jump);
    }
    #endregion

    private void ChangeAnimVariation()
    {
        if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.99f)
        {
            animator.SetFloat(AnimVariationHash, Random.value);
        }
    }

    private void Jump()
    {
        if (isGrounded && !IsDead())
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            animator.SetTrigger(JumpAnimHash);
            soundsManager.PlayClip(jumpSound);
        }
    }

    private void OnDamaged(float damage)
    {
        soundsManager.PlayClip(getDamageSounds.GetRandomElement());
        UpdateHealthBar(damage);
    }

    private void UpdateHealthBar(float change)
    {
        DOTween.To(() => hpBar.value, (x) => hpBar.value = x, GetHPPercentage(), 0.2f);
    }

    private void PlayDeathAnim()
    {
        moveDirection = 0f;
        rb.velocity = Vector2.zero;
        animator.SetTrigger(DeathAnimHash);
    }

    #region Attack

    private void StartAttack()
    {
        animator.SetTrigger(AttackAnimHash);

        soundsManager.PlayClip(attackSounds.GetRandomElement());
    }

    private void PerformAttack()
    {
        var enemiesInRadius = hittablesManager.GetTeamHittablesInRadius(transform.position, attackDistance, TeamType.Enemies);

        enemiesInRadius.ForEach(enemy =>
        {
            enemy.GetDamage(attackPower);
        });
    }
    #endregion

    private void CheckGround()
    {
        bool wasGrounded = isGrounded;

        isGrounded = Physics2D.OverlapCircleNonAlloc(groundCheckPivot.position, groundCheckRadius, groundCheckResults, groundCheckLayers.value) > 0;

        if (!wasGrounded && isGrounded)
        {
            animator.ResetTrigger(JumpAnimHash);
        }
    }
}
