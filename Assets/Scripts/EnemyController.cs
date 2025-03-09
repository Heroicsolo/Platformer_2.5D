using DG.Tweening;
using HeroicEngine.Components.Combat;
using HeroicEngine.Systems.DI;
using HeroicEngine.Utils;
using HeroicEngine.Utils.Math;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyController : Hittable, IInjectable
{
    private readonly int RunAnimHash = Animator.StringToHash("Running");
    private readonly int AttackAnimHash = Animator.StringToHash("Attack");
    private readonly int DeathAnimHash = Animator.StringToHash("Death");

    [SerializeField] private Animator animator;

    [Header("Movement")]
    [SerializeField][Min(0f)] private float speed = 5f;
    [SerializeField][Min(0f)] private float resetDistance = 10f;
    [SerializeField] private List<Transform> patrolPoints = new List<Transform>();
    [SerializeField][Min(0f)] private float reachPositionThreshold = 0.2f;

    [Header("Combat")]
    [SerializeField][Min(0f)] private float aggroDistance = 5f;
    [SerializeField][Min(0f)] private float attackDistance = 1.5f;
    [SerializeField][Min(0f)] private float attackPower = 1f;
    [SerializeField][Min(0f)] private float attackSpeed = 1f;
    
    [Header("Sounds")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip aggroSound;
    [SerializeField] private AudioClip damagedSound;
    [SerializeField] private AudioClip deathSound;

    [Inject] private PlayerController playerController;

    private Slider hpBar;
    private Rigidbody2D rb;
    private EnemyState currentState;
    private Transform movementTarget;
    private Vector2 moveDirection;
    private Vector3 spawnPoint;

    protected override void Awake()
    {
        base.Awake();
        rb = GetComponent<Rigidbody2D>();
    }

    protected override void Start()
    {
        base.Start();

        InjectionManager.RegisterObject(this);

        hpBar = transform.GetComponentInChildren<Slider>();

        currHealth = maxHealth;

        spawnPoint = transform.position;

        SubscribeToDeath(PlayDeathAnim);
        SubscribeToDamageGot(OnDamaged);
    }

    public void PostInject()
    {
    }

    private void OnDamaged(float damage)
    {
        audioSource.PlayOneShot(damagedSound);
        UpdateHealthBar();
        GoToPlayer();
    }

    private void UpdateHealthBar()
    {
        DOTween.To(() => hpBar.value, (x) => hpBar.value = x, GetHPPercentage(), 0.2f);
    }

    private void PlayDeathAnim()
    {
        //moveDirection = 0f;
        rb.velocity = Vector2.zero;
        animator.SetTrigger(DeathAnimHash);
    }

    private Transform GetNextPatrolPoint()
    {
        return patrolPoints.GetRandomElementExceptOne(movementTarget);
    }

    private void MoveTo(Vector3 position)
    {
        animator.SetBool(RunAnimHash, true);
        position.y = transform.position.y;
        moveDirection = (position - transform.position).normalized;
        moveDirection.y = 0f;
    }

    private void StopMovement()
    {
        moveDirection = Vector2.zero;
        animator.SetBool(RunAnimHash, false);
    }

    private void StopAttack()
    {
        animator.SetBool(AttackAnimHash, false);
        animator.speed = 1f;
    }

    private void SwitchState(EnemyState state)
    {
        if (currentState == state)
        {
            return;
        }

        currentState = state;

        if (playerController == null)
        {
            InjectionManager.InjectTo(this);
        }

        switch (state)
        {
            case EnemyState.Idle:
                StopMovement();

                if (patrolPoints.Count > 0)
                {
                    SwitchState(EnemyState.Patrolling);
                    return;
                }
                break;
            case EnemyState.RunToPlayer:
                movementTarget = playerController.transform;
                MoveTo(movementTarget.position);
                audioSource.PlayOneShot(aggroSound);
                break;
            case EnemyState.Patrolling:
                if (patrolPoints.Count > 0)
                {
                    movementTarget = GetNextPatrolPoint();
                }
                else
                {
                    SwitchState(EnemyState.Idle);
                    return;
                }
                break;
            case EnemyState.Attacking:
                StopMovement();

                animator.SetBool(AttackAnimHash, true);
                break;
            case EnemyState.Dead:
                rb.velocity = Vector2.zero;
                currHealth = 0f;
                UpdateHealthBar();
                StopMovement();
                StopAttack();
                animator.SetTrigger(DeathAnimHash);
                audioSource.PlayOneShot(deathSound);
                break;
        }
    }

    private void UpdateCurrentState()
    {
        switch (currentState)
        {
            case EnemyState.Idle:
                StopMovement();
                StopAttack();

                if (transform.Distance(playerController.transform) < aggroDistance)
                {
                    SwitchState(EnemyState.RunToPlayer);
                    return;
                }

                if (patrolPoints.Count > 0)
                {
                    SwitchState(EnemyState.Patrolling);
                    return;
                }
                break;
            case EnemyState.RunToPlayer:
                StopAttack();

                movementTarget = playerController.transform;
                MoveTo(movementTarget.position);

                if (transform.position.Distance(spawnPoint) > resetDistance)
                {
                    SwitchState(EnemyState.Idle);
                    return;
                }

                if (transform.Distance(movementTarget) < attackDistance)
                {
                    SwitchState(EnemyState.Attacking);
                    return;
                }
                break;
            case EnemyState.Patrolling:
                StopAttack();

                if (transform.Distance(playerController.transform) < aggroDistance)
                {
                    SwitchState(EnemyState.RunToPlayer);
                    return;
                }

                if (patrolPoints.Count == 0)
                {
                    SwitchState(EnemyState.Idle);
                    return;
                }

                if (transform.Distance(movementTarget) < reachPositionThreshold)
                {
                    movementTarget = GetNextPatrolPoint();
                }
                break;
            case EnemyState.Attacking:
                if (transform.Distance(movementTarget) > attackDistance)
                {
                    SwitchState(EnemyState.RunToPlayer);
                    return;
                }

                StopMovement();
                animator.speed = attackSpeed;
                animator.SetBool(AttackAnimHash, true);
                break;
            case EnemyState.Dead:
                StopMovement();
                StopAttack();
                break;
        }
    }

    private void Update()
    {
        if (!IsDead())
        {
            UpdateCurrentState();
        }
    }

    private void LateUpdate()
    {
        transform.rotation = Quaternion.Euler(0f, moveDirection.x >= 0f ? 0f : 180f, 0f);
    }

    private void FixedUpdate()
    {
        // Apply movement
        rb.velocity = new Vector2(moveDirection.x * speed, rb.velocity.y);
    }

    public void GoToPlayer()
    {
        if (currentState == EnemyState.Idle
            || currentState == EnemyState.Patrolling)
        {
            SwitchState(EnemyState.RunToPlayer);
        }
    }

    public bool IsAttacking()
    {
        return currentState == EnemyState.Attacking;
    }
}

public enum EnemyState
{
    Idle = 0,
    RunToPlayer = 1,
    Attacking = 2,
    Patrolling = 3,
    Dead = 4
}
