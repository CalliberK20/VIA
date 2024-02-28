using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;

public enum EnemyState
{
    Spawn,
    Idle,
    Wander,
    Patrol,
    Combat
}

public enum EnemyAggression
{
    AGGRESSIVE,
    NORMAL,
    PEACEFUL
}

[RequireComponent(typeof(LineOfSightChecker))]
public class EnemyMovement : MonoBehaviour
{
    //private const string isMoving = "isMoving";
    //private const string Jump = "Jump";
    //private const string Landed = "Landed";

    //[SerializeField]
    //private Animator animator;  

    //public EnemyLineOfSightChecker lineOfSightChecker;
    [Header("State")]
    private EnemyState _state = EnemyState.Spawn;
    public delegate void StateChangeEvent(EnemyState oldState, EnemyState newState);
    public StateChangeEvent OnStateChange;

    private Vector3[] _waypoints = new Vector3[4];
    public NavMeshTriangulation _triangulation;
    private int _waypointIndex = 0;
    public Transform _target;

    [Space()]

    [SerializeField]
    private EnemyStats _enemyStats;

    [Space()]
    [Header("Debug")]
    [SerializeField]
    private bool _showDebugText = true;
    [SerializeField]
    private TextMeshProUGUI _enemyStateText;
    [SerializeField]
    private LayerMask _enemyLayerMask;

    //private AgentLinkMover linkMover;
    private NavMeshAgent _agent;
    private Coroutine _stateCoroutine;
    private LineOfSightChecker _lineOfSightChecker;

    public EnemyState State
    {
        get { return _state; }
        set
        {
            OnStateChange?.Invoke(_state, value);
            _state = value;
            if (_showDebugText) Debug.Log($"{gameObject.name} is now in state {_state}");
        }
    }

    private void Awake()
    {
        _triangulation = NavMesh.CalculateTriangulation();

        _agent = GetComponent<NavMeshAgent>();

        _lineOfSightChecker = GetComponent<LineOfSightChecker>();

        /*linkMover = GetComponent<AgentLinkMover>();

        linkMover.OnLinkStart += HandleLinkStart;
        linkMover.OnLinkEnd += HandleLinkEnd;*/

        _lineOfSightChecker.OnGainSight += HandleGainSight;
        _lineOfSightChecker.OnLoseSight += HandleLoseSight;

        OnStateChange += HandleStateChange;
    }

    private void Start()
    {
        _lineOfSightChecker.LineOfSightTags = _enemyStats.AggroTags;
        _lineOfSightChecker.SphereCollider.radius = _enemyStats.AggroRange;
    }

    private void OnDrawGizmosSelected()
    {
        //waypoints
        //Gizmos.color = Color.yellow;
        //Gizmos.DrawLineStrip(_waypoints, true);

        //wander roam distance
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _enemyStats.MaxWanderDistance);

        //alert range
        Gizmos.color = new Color(255, 140, 0);
        Gizmos.DrawWireSphere(transform.position, _enemyStats.AlertRange);

        //aggro range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _enemyStats.AggroRange);
    }

    private void OnEnable()
    {
        Spawn();
    }

    private void OnDisable()
    {
        _state = _enemyStats.DefaultState;
    }

    void Update()
    {
        _enemyStateText.text = _state.ToString();

        //animator.SetBool(isMoving, agent.velocity.magnitude > 0.01f);
    }

    private void HandleGainSight(Transform target)
    {
        if (_showDebugText) Debug.Log("Player Spotted!");
        _target = target;
        State = EnemyState.Combat;
    }

    private void HandleLoseSight(Transform target)
    {
        State = _enemyStats.DefaultState;
        _target = null;
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
        if (oldState == newState)
            return;

        if (_stateCoroutine != null)
            StopCoroutine(_stateCoroutine);

        //reset combat stat modifiers
        if (oldState == EnemyState.Combat)
        {
            _agent.speed /= _enemyStats.CombatSpeedMultiplier;
            _agent.stoppingDistance = _enemyStats.StoppingDistance;
        }


        switch (newState)
        {
            case EnemyState.Idle:
                _stateCoroutine = StartCoroutine(IdleUpdate());
                break;
            case EnemyState.Wander:
                _stateCoroutine = StartCoroutine(WanderUpdate());
                break;
            case EnemyState.Patrol:
                _stateCoroutine = StartCoroutine(PatrolUpdate());
                break;
            case EnemyState.Combat:
                _stateCoroutine = StartCoroutine(CombatUpdate());
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
        State = _enemyStats.DefaultState;
    }

    public void Aggro()
    {
        State = EnemyState.Combat;

        Collider[] colliders = Physics.OverlapSphere(transform.position, _enemyStats.AlertRange);

        //for every enemy in aggro range, aggro those not currently aggro'd
        foreach (Collider collider in colliders)
        {
            if (collider.TryGetComponent(out EnemyMovement enemy))
            {
                if(enemy.State != EnemyState.Combat)
                    enemy.Aggro();
            }
        }
    }
    
    //update for idle state
    private IEnumerator IdleUpdate()
    {
        WaitForSeconds wait = new(_enemyStats.UpdateSpeed);

        yield return wait;
    }

    //update for idle state
    private IEnumerator WanderUpdate()
    {
        WaitForSeconds wait = new(_enemyStats.UpdateSpeed);      

        while (true)
        {
            //if this don't exist or smthn then keep waiting til it does
            if (!_agent.enabled || !_agent.isOnNavMesh)
            {
                yield return wait;
                continue;
            }
                
            //new wander "waypoint" once old one is reached
            if (_agent.remainingDistance <= _agent.stoppingDistance)
            {
                Vector2 point = Random.insideUnitCircle * _enemyStats.MaxWanderDistance;

                if (NavMesh.SamplePosition(_agent.transform.position + new Vector3(point.x, 0, point.y), out NavMeshHit hit, 2f, _agent.areaMask))
                {
                    _agent.SetDestination(hit.position);
                }
            }

            yield return wait;
        }
    }

    //update for patrol state
    private IEnumerator PatrolUpdate()
    {
        WaitForSeconds wait = new(_enemyStats.UpdateSpeed);

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

    //update for chase state
    private IEnumerator CombatUpdate()
    {
        WaitForSeconds wait = new(_enemyStats.UpdateSpeed);

        _agent.speed *= _enemyStats.CombatSpeedMultiplier;
        _agent.stoppingDistance = _enemyStats.CombatStoppingDistance;

        while (enabled && _agent.enabled)
        {
            _agent.SetDestination(_target.transform.position);
            yield return wait;
        }
    }
}
