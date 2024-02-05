using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum EnemyState
{
    Spawn,
    Idle,
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

    public EnemyState DefaultState;
    private EnemyState _state;

    public Transform target;
    public float UpdateSpeed = 0.1f;
    public float IdleMaxRoamDistance = 4f;
    public float IdleMovespeedMultiplier = 0.5f;

    //public EnemyLineOfSightChecker lineOfSightChecker;
    public delegate void StateChangeEvent(EnemyState oldState, EnemyState newState);
    public StateChangeEvent OnStateChange;
    public NavMeshTriangulation _triangulation;

    private int _waypointIndex = 0;
    private Vector3[] _waypoints = new Vector3[4];

    //private AgentLinkMover linkMover;
    private NavMeshAgent _agent;
    private Coroutine _stateCoroutine;

    public EnemyState State
    {
        get { return _state; }
        set
        {
            OnStateChange?.Invoke(_state, value);
            _state = value;
        }
    }

    private void Awake()
    {
        _triangulation = NavMesh.CalculateTriangulation();

        _agent = GetComponent<NavMeshAgent>();

        /*linkMover = GetComponent<AgentLinkMover>();

        linkMover.OnLinkStart += HandleLinkStart;
        linkMover.OnLinkEnd += HandleLinkEnd;

        lineOfSightChecker.onGainSight += HandleGainSight;
        lineOfSightChecker.onLoseSight += HandleLoseSight;*/

        OnStateChange += HandleStateChange;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawLineStrip(_waypoints, true);
    }

    private void OnEnable()
    {
        Spawn();
    }

    private void OnDisable()
    {
        _state = DefaultState;
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
        if (oldState == newState)
            return;

        if (_stateCoroutine != null)
            StopCoroutine(_stateCoroutine);

        if (oldState == EnemyState.Idle)
        {
            _agent.speed /= IdleMovespeedMultiplier;
        }

        switch (newState)
        {
            case EnemyState.Idle:
                _stateCoroutine = StartCoroutine(IdleStateUpdate());
                break;
            case EnemyState.Patrol:
                _stateCoroutine = StartCoroutine(PatrolStateUpdate());
                break;
            case EnemyState.Chase:
                _stateCoroutine = StartCoroutine(ChaseStateUpdate());
                break;
        }
    }

    public void StartChasing()
    {
        //followCoroutine = StartCoroutine(followTarget());
        State = EnemyState.Patrol;
    }

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
        OnStateChange?.Invoke(EnemyState.Spawn, DefaultState);
    }

    void Update()
    {
        //animator.SetBool(isMoving, agent.velocity.magnitude > 0.01f);
    }

    private IEnumerator IdleStateUpdate()
    {
        WaitForSeconds wait = new(UpdateSpeed);

        _agent.speed *= IdleMovespeedMultiplier;

        while (true)
        {
            if (!_agent.enabled || !_agent.isOnNavMesh)
            {
                yield return wait;
            }
            else if (_agent.remainingDistance <= _agent.stoppingDistance)
            {
                Vector2 point = Random.insideUnitCircle * IdleMaxRoamDistance;

                if (NavMesh.SamplePosition(_agent.transform.position + new Vector3(point.x, 0, point.y), out NavMeshHit hit, 2f, _agent.areaMask))
                {
                    _agent.SetDestination(hit.position);
                }
            }
            yield return wait;
        }
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

        while (enabled && _agent.enabled)
        {
            _agent.SetDestination(target.transform.position);
            yield return wait;
        }
    }
}
