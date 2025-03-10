using DG.Tweening;
using HeroicEngine.Components;
using HeroicEngine.Components.Combat;
using HeroicEngine.Systems.DI;
using HeroicEngine.Utils;
using HeroicEngine.Utils.Math;
using HeroicEngine.Utils.Pooling;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyController : Hittable, IInjectable
{
    private readonly int RunAnimHash = Animator.StringToHash("Running");
    private readonly int AttackAnimHash = Animator.StringToHash("Attack");
    private readonly int DeathAnimHash = Animator.StringToHash("Death");
    private readonly int DamagedAnimHash = Animator.StringToHash("Damaged");

    [SerializeField] private Animator animator;

    [Header("Movement")]
    [SerializeField][Min(0f)] private float speed = 5f;
    [SerializeField][Min(0f)] private float patrollingSpeed = 2.5f;
    [SerializeField][Min(0f)] private float resetDistance = 10f;
    [SerializeField] private List<Transform> patrolPoints = new List<Transform>();
    [SerializeField][Min(0f)] private float reachPositionThreshold = 0.2f;

    [Header("Combat")]
    [SerializeField][Min(0f)] private float aggroDistance = 5f;
    [SerializeField][Min(0f)] private float attackDistance = 1.5f;
    [SerializeField][Min(0f)] private float attackPower = 1f;
    [SerializeField][Min(0f)] private float attackSpeed = 1f;
    [SerializeField] private LayerMask obstacleMask;
    [SerializeField] private FlyUpText combatText;

    [Header("Sounds")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip aggroSound;
    [SerializeField] private AudioClip damagedSound;
    [SerializeField] private AudioClip deathSound;

    [Inject] private PlayerController playerController;

    #region Private Params
    private Slider hpBar;
    private Canvas botCanvas;
    private Rigidbody2D rb;
    private EnemyState currentState;
    private Transform movementTarget;
    private Vector2 moveDirection;
    private Vector3 spawnPoint;
    private bool initialized;
    #endregion

    #region Lifetime Methods
    protected override void Awake()
    {
        base.Awake();
        rb = GetComponent<Rigidbody2D>();
        initialized = false;
    }

    protected override void Start()
    {
        base.Start();

        InjectionManager.RegisterObject(this);

        hpBar = transform.GetComponentInChildren<Slider>();
        botCanvas = transform.GetComponentInChildren<Canvas>();

        currHealth = maxHealth;

        spawnPoint = transform.position;

        SubscribeToDeath(PlayDeathAnim);
        SubscribeToDamageGot(OnDamaged);
    }

    public void PostInject()
    {
        initialized = true;
    }

    private void Update()
    {
        if (!IsDead() && initialized)
        {
            UpdateCurrentState();
        }
    }

    private void LateUpdate()
    {
        if (currentState == EnemyState.Attacking)
        {
            Vector3 attackDirection = playerController.transform.position - transform.position;
            transform.rotation = Quaternion.Euler(0f, attackDirection.x >= 0f ? 0f : 180f, 0f);
        }
        else
        {
            transform.rotation = Quaternion.Euler(0f, moveDirection.x >= 0f ? 0f : 180f, 0f);
        }
    }

    private void FixedUpdate()
    {
        float actualSpeed = currentState == EnemyState.Patrolling ? patrollingSpeed : speed;
        // Apply movement
        rb.velocity = new Vector2(moveDirection.x * actualSpeed, rb.velocity.y);
    }

    private void OnDrawGizmos()
    {
        patrolPoints.ForEach(p =>
        {
            Gizmos.color = Color.white;
            Gizmos.DrawSphere(p.position, 0.2f);
            Gizmos.color = new Color(1f, 0f, 0f, 0.1f);
            Gizmos.DrawCube(transform.position + transform.right * aggroDistance * 0.5f + Vector3.up
                , new Vector3(aggroDistance, 2f, 1f));
        });
    }
    #endregion

    private void OnDamaged(float damage)
    {
        audioSource.PlayOneShot(damagedSound);

        if (!IsDead())
        {
            animator.SetTrigger(DamagedAnimHash);
        }

        var damageText = PoolSystem.GetInstanceAtPosition(combatText, combatText.GetName(), botCanvas.transform.position, botCanvas.transform);
        damageText.SetText($"-{Mathf.CeilToInt(damage)}");

        UpdateHealthBar();
        GoToPlayer();
    }

    #region Attack
    private void PerformAttack()
    {
        if (transform.Distance(playerController.transform) < attackDistance)
        {
            playerController.GetDamage(attackPower);
        }
    }

    private void StopAttack()
    {
        animator.SetBool(AttackAnimHash, false);
        animator.speed = 1f;
    }
    #endregion

    private void UpdateHealthBar()
    {
        DOTween.To(() => hpBar.value, (x) => hpBar.value = x, GetHPPercentage(), 0.2f);

        if (IsDead())
        {
            botCanvas.gameObject.SetActive(false);
        }
    }

    private void PlayDeathAnim()
    {
        SwitchState(EnemyState.Dead);
    }

    private bool IsTargetVisible(Transform target)
    {
        if (target == null) return false;

        Vector3 targetCenter = target.position + Vector3.up;
        Vector3 lookFromPoint = transform.position + Vector3.up;

        // Get direction to the target
        Vector2 directionToTarget = targetCenter - lookFromPoint;
        float distanceToTarget = directionToTarget.magnitude;

        // Check if target is within view distance
        if (distanceToTarget > aggroDistance) return false;

        // Get AI facing direction
        float aiFacingDirection = Mathf.Sign(moveDirection.x);
        float targetDirection = Mathf.Sign(directionToTarget.x);

        // Ensure target is in front of AI
        if (aiFacingDirection != targetDirection) return false;

        // Raycast to check line of sight
        RaycastHit2D hit = Physics2D.Raycast(lookFromPoint, directionToTarget.normalized, distanceToTarget, obstacleMask);

        // If ray hits something before reaching the target, the target is blocked
        if (hit.collider != null && hit.collider.transform != target)
        {
            return false;
        }

        return true; // Target is visible
    }

    #region Movement

    private Transform GetNextPatrolPoint()
    {
        return patrolPoints.GetRandomElementExceptOne(movementTarget);
    }

    private void MoveTo(Vector3 position)
    {
        animator.SetBool(RunAnimHash, true);
        animator.speed = currentState == EnemyState.Patrolling ? patrollingSpeed / speed : 1f;
        position.y = transform.position.y;
        moveDirection = (position - transform.position).normalized;
        moveDirection.y = 0f;
    }

    private void StopMovement()
    {
        moveDirection = Vector2.zero;
        animator.SetBool(RunAnimHash, false);
    }
    #endregion

    #region State Machine
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
                moveDirection = Vector2.zero;
                currHealth = 0f;
                UpdateHealthBar();
                StopMovement();
                StopAttack();
                GetComponent<Collider2D>().enabled = false;
                rb.bodyType = RigidbodyType2D.Kinematic;
                animator.SetTrigger(DeathAnimHash);
                audioSource.PlayOneShot(deathSound);
                break;
            case EnemyState.Resetting:
                StopMovement();
                StopAttack();

                isImmuneToDamage = true;

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
        }
    }

    private void UpdateCurrentState()
    {
        if (playerController == null)
        {
            InjectionManager.InjectTo(this);
        }

        switch (currentState)
        {
            case EnemyState.Idle:
                StopMovement();
                StopAttack();

                if (IsTargetVisible(playerController.transform))
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
                    SwitchState(EnemyState.Resetting);
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

                if (IsTargetVisible(playerController.transform))
                {
                    SwitchState(EnemyState.RunToPlayer);
                    return;
                }

                if (patrolPoints.Count == 0)
                {
                    SwitchState(EnemyState.Idle);
                    return;
                }

                if (transform.DistanceXZ(movementTarget) < reachPositionThreshold)
                {
                    movementTarget = GetNextPatrolPoint();
                }

                MoveTo(movementTarget.position);
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
            case EnemyState.Resetting:
                MoveTo(movementTarget.position);

                if (transform.DistanceXZ(movementTarget) < reachPositionThreshold)
                {
                    ResetHealth();
                    UpdateHealthBar();
                    SwitchState(EnemyState.Patrolling);
                }
                break;
        }
    }
    #endregion

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
    Dead = 4,
    Resetting = 5
}
