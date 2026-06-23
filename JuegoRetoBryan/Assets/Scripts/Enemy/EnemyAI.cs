using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// IA basica de enemigo (Etapa 3).
/// Usa NavMeshAgent para perseguir al jugador cuando entra en el rango de
/// deteccion y lo ataca cuando esta a rango de golpe.
///
/// Maquina de estados simple: Idle -> Chase -> Attack.
/// El dano se aplica a traves de la interfaz IDamageable, por lo que el
/// enemigo no necesita conocer la clase concreta del jugador (codigo escalable).
///
/// Requiere: NavMeshAgent en el mismo GameObject y un NavMesh horneado en la
/// escena (Window > AI > Navigation). El jugador debe tener el tag "Player".
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(EnemyHealth))]
public class EnemyAI : MonoBehaviour
{
    public enum EnemyState { Idle, Chase, Attack }

    [Header("Deteccion")]
    [Tooltip("Distancia a la que el enemigo detecta y empieza a perseguir.")]
    [SerializeField] private float detectionRange = 12f;
    [Tooltip("Si el jugador se aleja mas de esto, el enemigo deja de perseguir.")]
    [SerializeField] private float loseRange = 16f;

    [Header("Ataque")]
    [Tooltip("Distancia a la que el enemigo puede golpear al jugador.")]
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private float attackDamage = 10f;
    [Tooltip("Segundos entre golpes.")]
    [SerializeField] private float attackCooldown = 1.5f;

    [Header("Objetivo")]
    [Tooltip("Tag del objetivo a perseguir. Por defecto el jugador.")]
    [SerializeField] private string targetTag = "Player";

    public EnemyState CurrentState { get; private set; } = EnemyState.Idle;

    private NavMeshAgent _agent;
    private EnemyHealth  _health;
    private Transform    _target;
    private IDamageable  _targetDamageable;
    private Animator     _animator;
    private float        _nextAttackTime;
    private bool         _isDead;

    private static readonly int SpeedHash  = Animator.StringToHash("Speed");
    private static readonly int AttackHash = Animator.StringToHash("Attack");

    private void Awake()
    {
        _agent   = GetComponent<NavMeshAgent>();
        _health  = GetComponent<EnemyHealth>();
        _animator = GetComponentInChildren<Animator>();
    }

    private void OnEnable()
    {
        _health.OnDeath += HandleDeath;
    }

    private void OnDisable()
    {
        _health.OnDeath -= HandleDeath;
    }

    private void Start()
    {
        AcquireTarget();
    }

    private void AcquireTarget()
    {
        GameObject go = GameObject.FindGameObjectWithTag(targetTag);
        if (go == null)
            return;

        _target = go.transform;
        go.TryGetComponent(out _targetDamageable);
    }

    private void Update()
    {
        if (_isDead)
            return;

        // El jugador pudo haber aparecido despues (re-spawn / carga tardia)
        if (_target == null)
        {
            AcquireTarget();
            return;
        }

        float distance = Vector3.Distance(transform.position, _target.position);
        UpdateState(distance);

        switch (CurrentState)
        {
            case EnemyState.Idle:
                _agent.isStopped = true;
                break;

            case EnemyState.Chase:
                _agent.isStopped = false;
                _agent.SetDestination(_target.position);
                break;

            case EnemyState.Attack:
                _agent.isStopped = true;
                FaceTarget();
                TryAttack();
                break;
        }

        // Alimenta el Animator del enemigo si existe (locomocion + golpe)
        if (_animator != null)
            _animator.SetFloat(SpeedHash, _agent.velocity.magnitude);
    }

    private void UpdateState(float distance)
    {
        switch (CurrentState)
        {
            case EnemyState.Idle:
                if (distance <= detectionRange)
                    CurrentState = EnemyState.Chase;
                break;

            case EnemyState.Chase:
                if (distance <= attackRange)
                    CurrentState = EnemyState.Attack;
                else if (distance > loseRange)
                    CurrentState = EnemyState.Idle;
                break;

            case EnemyState.Attack:
                if (distance > attackRange)
                    CurrentState = EnemyState.Chase;
                break;
        }
    }

    private void TryAttack()
    {
        if (Time.time < _nextAttackTime || _targetDamageable == null)
            return;

        _nextAttackTime = Time.time + attackCooldown;

        if (_animator != null)
            _animator.SetTrigger(AttackHash);

        _targetDamageable.TakeDamage(attackDamage);
    }

    private void FaceTarget()
    {
        Vector3 dir = _target.position - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.001f)
            return;

        Quaternion look = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.Slerp(transform.rotation, look, 10f * Time.deltaTime);
    }

    private void HandleDeath()
    {
        _isDead = true;
        if (_agent != null && _agent.isOnNavMesh)
            _agent.isStopped = true;
        enabled = false;
    }

    // Visualiza los rangos en el editor para ajustarlos comodamente.
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Gizmos.color = new Color(1f, 0.5f, 0f);
        Gizmos.DrawWireSphere(transform.position, loseRange);
    }
}
