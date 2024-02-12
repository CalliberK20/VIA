using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.AI;

public enum EnemyState
{
    Spawn,
    Idle,
    Wander,
    Reset,
    Patrol,
    Chase
}

public class EnemyMovement : MonoBehaviour
{
    //private const string isMoving = "isMoving";
    //private const string Jump = "Jump";
    //private const string Landed = "Landed";

    //[SerializeField]
    //private Animator animator;  

    //public EnemyLineOfSightChecker lineOfSightChecker;
    [Header("State")]
    public EnemyState DefaultState;
    private EnemyState _state = EnemyState.Spawn;
    //<old state, new state>
    public UnityEvent<EnemyState, EnemyState> StateChangeEvent;
    public NavMeshTriangulation _triangulation;
    public float UpdateSpeed = 0.1f;
    public float WanderMaxRoamDistance = 4f;
    public float ChaseMovespeedMultiplier = 1.2f;
    private int _waypointIndex = 0;
    private Vector3[] _waypoints = new Vector3[4];
    public Transform _target;

    [Space()]

    [Header("Pack Behavior")]
    [SerializeField]
    [Tooltip("Minimum distance from other enemies")]
    private float _minDistanceFromEnemy = 1.5f;
    [SerializeField]
    [Tooltip("Determines if this Enemy is a pack Alpha")]
    private bool _isAlpha;
    [SerializeField]
    [Tooltip("(Alpha Only) Maximum distance members of the pack can be from the Alpha")]
    public float MaxDistanceFromAlpha = 3f;
    [SerializeField]
    [Tooltip("Minimum distance needed to break off from reset state")]
    private float _minimumResetDistance = 1f;
    [SerializeField]
    [Tooltip("Maximum distance to aggro nearby enemies")]
    private float _alertDistance = 4f;
    public EnemyMovement _alphaMember;

    [Space()]
    [Header("Debug")]
    [SerializeField]
    private TextMeshProUGUI _enemyStateText;
    [SerializeField]
    private LayerMask _enemyLayerMask;

    //private AgentLinkMover linkMover;
    private NavMeshAgent _agent;
    private Coroutine _stateCoroutine;
    private Collider _enemyCollider;

    public EnemyState State
    {
        get { return _state; }
        set
        {
            StateChangeEvent.Invoke(_state, value);
            _state = value;
        }
    }

    private void Awake()
    {
        _triangulation = NavMesh.CalculateTriangulation();

        _agent = GetComponent<NavMeshAgent>();

        _enemyCollider = GetComponent<Collider>();

        /*linkMover = GetComponent<AgentLinkMover>();

        linkMover.OnLinkStart += HandleLinkStart;
        linkMover.OnLinkEnd += HandleLinkEnd;

        lineOfSightChecker.onGainSight += HandleGainSight;
        lineOfSightChecker.onLoseSight += HandleLoseSight;*/

        StateChangeEvent.AddListener(HandleStateChange);
    }

    private void OnDrawGizmosSelected()
    {
        //waypoints
        //Gizmos.color = Color.yellow;
        //Gizmos.DrawLineStrip(_waypoints, true);

        //wander roam distance
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, WanderMaxRoamDistance);

        //minimum distance from enemy
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, _minDistanceFromEnemy);

        Gizmos.color = Color.cyan;        
        if (!_isAlpha)
            //minimum reset range
            Gizmos.DrawWireSphere(transform.position, _minimumResetDistance);
        else
            //max range from alpha
            Gizmos.DrawWireSphere(transform.position, MaxDistanceFromAlpha);

        //alert range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _alertDistance);
    }

    private void OnEnable()
    {
        Spawn();
    }

    private void OnDisable()
    {
        _state = DefaultState;
    }

    void Update()
    {
        _enemyStateText.text = _state.ToString();

        //animator.SetBool(isMoving, agent.velocity.magnitude > 0.01f);
        if (!_isAlpha)
            if (_state != EnemyState.Reset && Vector3.Distance(transform.position, _alphaMember.transform.position) > _alphaMember.MaxDistanceFromAlpha)
                State = EnemyState.Reset;
    }

    private void HandleGainSight(Player player)
    {
        Debug.Log("Player Spotted!");
        State = EnemyState.Chase;
    }

    private void HandleLoseSight(Player player)
    {
        State = DefaultState;
    }

    /*private void HandleLinkStart()
    {
        animator.SetTrigger(Jump);
    }

    private void HandleLinkEnd()
    {
        animator.SetTrigger(Landed);
    }*/

    private void HandleStateChange(EnemyState oldState, EnemyState newState)
    {
        Debug.Log($"{gameObject.name} is switching from {oldState} to {newState}");

        if (oldState == newState)
            return;

        if (_stateCoroutine != null)
            StopCoroutine(_stateCoroutine);

        if (oldState == EnemyState.Chase)
        {
            _agent.speed /= ChaseMovespeedMultiplier;
        }

        switch (newState)
        {
            case EnemyState.Idle:
                _stateCoroutine = StartCoroutine(IdleStateUpdate());
                break;
            case EnemyState.Wander:
                _stateCoroutine = StartCoroutine(WanderStateUpdate());
                break;
            case EnemyState.Reset:
                _stateCoroutine = StartCoroutine(ResetStateUpdate());
                break;
            case EnemyState.Patrol:
                _stateCoroutine = StartCoroutine(PatrolStateUpdate());
                break;
            case EnemyState.Chase:
                _stateCoroutine = StartCoroutine(ChaseStateUpdate());
                break;
            
        }
    }

    /*public void StartChasing()
    {
        followCoroutine = StartCoroutine(followTarget());
        State = EnemyState.Patrol;
    }*/

    public void Spawn()
    {
        /*for (int i = 0; i < waypoints.Length; i++)
        {
            NavMeshHit hit;
            if (NavMesh.SamplePosition(triangulation.vertices[Random.Range(0, triangulation.vertices.Length)], out hit, 2f, agent.areaMask))
            {
                waypoints[i] = hit.position;
            }
            else
            {
                Debug.LogError("Unable to find position for navmesh newr Triangulation vertex!");
            }

            Debug.Log("New Waypoint Set at " + waypoints[i]);
        }*/
        //StateChangeEvent.Invoke(EnemyState.Spawn, DefaultState);
        State = DefaultState;
    }

    public void Aggro()
    {
        State = EnemyState.Chase;

        Collider[] colliders = Physics.OverlapSphere(transform.position, _alertDistance);

        foreach (Collider collider in colliders)
        {
            if (collider.TryGetComponent(out EnemyMovement enemy))
            {
                if(enemy.State != EnemyState.Chase)
                    enemy.Aggro();
            }
        }
    }

    private IEnumerator IdleStateUpdate()
    {
        WaitForSeconds wait = new(UpdateSpeed);

        yield return wait;
    }

    private IEnumerator WanderStateUpdate()
    {
        WaitForSeconds wait = new(UpdateSpeed);      

        while (true)
        {
            if (!_agent.enabled || !_agent.isOnNavMesh)
            {
                yield return wait;
            }
            else
            {
                Collider[] nearestEnemies = Physics.OverlapSphere(transform.position, _minDistanceFromEnemy, _enemyLayerMask);
                Collider nearestEnemy = null;

                float nearestDistance = float.MaxValue;
                foreach (Collider enemy in nearestEnemies)
                {
                    if (enemy == _enemyCollider)
                        continue;

                    Vector3 vectorToEnemy = enemy.transform.position - _agent.transform.position;
                    float distanceToEnemy = Vector3.SqrMagnitude(vectorToEnemy);

                    if (distanceToEnemy < nearestDistance)
                    {
                        nearestDistance = distanceToEnemy;
                        nearestEnemy = enemy;
                    }
                }

                if (_agent.remainingDistance <= _agent.stoppingDistance || ( nearestEnemy != null && nearestDistance < _minDistanceFromEnemy))
                {
                    Vector2 point = Random.insideUnitCircle * WanderMaxRoamDistance;

                    if (NavMesh.SamplePosition(_agent.transform.position + new Vector3(point.x, 0, point.y), out NavMeshHit hit, 2f, _agent.areaMask))
                    {
                        _agent.SetDestination(hit.position);
                    }
                }
            }
            yield return wait;
        }
    }

    private IEnumerator ResetStateUpdate()
    {
        WaitForSeconds wait = new(UpdateSpeed);

        while (Vector3.Distance(transform.position, _alphaMember.transform.position) > _minimumResetDistance)
        {
            _agent.SetDestination(_alphaMember.transform.position);
            yield return wait;
        }

        State = DefaultState;
    }

    private IEnumerator PatrolStateUpdate()
    {
        WaitForSeconds wait = new(UpdateSpeed);

        yield return new WaitUntil(() => _agent.enabled && _agent.isOnNavMesh);
        _agent.SetDestination(_waypoints[_waypointIndex]);

        while (true)
        {
            if (_agent.isOnNavMesh && _agent.enabled && _agent.remainingDistance <= _agent.stoppingDistance)
            {
                _waypointIndex++;

                if (_waypointIndex >= _waypoints.Length)
                    _waypointIndex = 0;
                _agent.SetDestination(_waypoints[_waypointIndex]);
            }
            yield return wait;
        }
    }

    private IEnumerator ChaseStateUpdate()
    {
        WaitForSeconds wait = new(UpdateSpeed);

        _agent.speed *= ChaseMovespeedMultiplier;

        while (enabled && _agent.enabled)
        {
            _agent.SetDestination(_target.transform.position);
            yield return wait;
        }
    }
}
