using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.AI;
using static UnityEngine.Rendering.DebugUI;

public enum EnemyState
{
    Spawn,
    Idle,
    Wander,
    ApproachAlpha,
    MakeDistance,
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
    private bool _showDebugText = true;
    [SerializeField]
    private TextMeshProUGUI _enemyStateText;
    [SerializeField]
    private LayerMask _enemyLayerMask;

    //private AgentLinkMover linkMover;
    private NavMeshAgent _agent;
    private Coroutine _stateCoroutine;
    private Collider _enemyCollider;
    private float _nearestDistance;
    private Collider _nearestEnemy;

    public EnemyState State
    {
        get { return _state; }
        set
        {
            StateChangeEvent.Invoke(_state, value);
            _state = value;
            if (_showDebugText) Debug.Log($"{gameObject.name} is now in state {_state}");
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

        //if not alpha or currently approaching one, and distance from nearest alpha too far, start approaching alpha
        if (!_isAlpha)
            if (_state != EnemyState.ApproachAlpha && Vector3.Distance(transform.position, _alphaMember.transform.position) > _alphaMember.MaxDistanceFromAlpha)
                State = EnemyState.ApproachAlpha;
    }

    private void HandleGainSight(Player player)
    {
        if (_showDebugText) Debug.Log("Player Spotted!");
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
        if (_showDebugText) Debug.Log($"{gameObject.name} is switching from {oldState} to {newState}");

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
                _stateCoroutine = StartCoroutine(IdleUpdate());
                break;
            case EnemyState.Wander:
                _stateCoroutine = StartCoroutine(WanderUpdate());
                break;
            case EnemyState.ApproachAlpha:
                _stateCoroutine = StartCoroutine(ApproachAlphaUpdate());
                break;
            case EnemyState.MakeDistance:
                _stateCoroutine = StartCoroutine(MakeDistanceUpdate());
                break;
            case EnemyState.Patrol:
                _stateCoroutine = StartCoroutine(PatrolUpdate());
                break;
            case EnemyState.Chase:
                _stateCoroutine = StartCoroutine(ChaseUpdate());
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

        //for every enemy in aggro range, aggro those not currently aggro'd
        foreach (Collider collider in colliders)
        {
            if (collider.TryGetComponent(out EnemyMovement enemy))
            {
                if(enemy.State != EnemyState.Chase)
                    enemy.Aggro();
            }
        }
    }
    
    //update for idle state
    private IEnumerator IdleUpdate()
    {
        WaitForSeconds wait = new(UpdateSpeed);

        yield return wait;
    }

    //update for idle state
    private IEnumerator WanderUpdate()
    {
        WaitForSeconds wait = new(UpdateSpeed);      

        while (true)
        {
            //if this don't exist or smthn then keep waiting til it does
            if (!_agent.enabled || !_agent.isOnNavMesh)
            {
                yield return wait;
                continue;
            }

            GetTooCloseEnemy();

            //if distance to nearest enemy too close, set escape point exact opposite direction to enemy and set state
            if (_nearestDistance < _minDistanceFromEnemy)
            {
                //State = EnemyState.MakeDistance;
                Vector2 escapePoint = (_agent.transform.position - _nearestEnemy.transform.position).normalized * WanderMaxRoamDistance;

                if (NavMesh.SamplePosition(_agent.transform.position + new Vector3(escapePoint.x, 0, escapePoint.y), out NavMeshHit hit, 2f, _agent.areaMask))
                {
                    _agent.SetDestination(hit.position);
                    if (_showDebugText) Debug.Log($"{gameObject.name} detects enemy too near: {_nearestEnemy.name}, setting new position to {hit.position} (current position: {transform.position})");
                    State = EnemyState.MakeDistance;
                }
              
            }
                
            //new wander "waypoint" once old one is reached
            if (_agent.remainingDistance <= _agent.stoppingDistance)
            {
                Vector2 point = Random.insideUnitCircle * WanderMaxRoamDistance;

                if (NavMesh.SamplePosition(_agent.transform.position + new Vector3(point.x, 0, point.y), out NavMeshHit hit, 2f, _agent.areaMask))
                {
                    _agent.SetDestination(hit.position);
                }
            }

            yield return wait;
        }
    }

    //update for aproach alpha state
    private IEnumerator ApproachAlphaUpdate()
    {
        WaitForSeconds wait = new(UpdateSpeed);

        //while distance > resetdistance
        while (Vector3.Distance(transform.position, _alphaMember.transform.position) > _minimumResetDistance)
        {
            _agent.SetDestination(_alphaMember.transform.position);
            yield return wait;
        }

        State = DefaultState;
    }

    //update for make distance state
    private IEnumerator MakeDistanceUpdate()
    {
        /*Vector2 escapePoint = (_agent.transform.position - _nearestEnemy.transform.position).normalized * WanderMaxRoamDistance;

        if (NavMesh.SamplePosition(_agent.transform.position + new Vector3(escapePoint.x, 0, escapePoint.y), out NavMeshHit hit, 2f, _agent.areaMask))
        {
            _agent.SetDestination(hit.position);
            Debug.Log($"{gameObject.name} detects enemy too near: {_nearestEnemy.name}, setting new position to {hit.position} (current position: {transform.position})");
        }

        Debug.Log($"{gameObject.name} is running to {_agent.destination} from {transform.position}, current state is {_state}");*/

        WaitForSeconds wait = new(UpdateSpeed);

        //save nearest enemy
        Collider currentNearestEnemy = _nearestEnemy;

        while (_agent.remainingDistance > _agent.stoppingDistance)
        {
            if (_showDebugText) Debug.Log($"remaining distance: {_agent.remainingDistance}, stopping distance: {_agent.stoppingDistance}");

            GetTooCloseEnemy();

            //if nearest enemy is a different one, set escape point exact opposite direction to new enemy and set state
            if (currentNearestEnemy != _nearestEnemy && _nearestEnemy != null)
            {
                currentNearestEnemy = _nearestEnemy;

                Vector2 escapePoint = (_agent.transform.position - _nearestEnemy.transform.position).normalized * WanderMaxRoamDistance;

                if (NavMesh.SamplePosition(_agent.transform.position + new Vector3(escapePoint.x, 0, escapePoint.y), out NavMeshHit hit2, 2f, _agent.areaMask))
                {
                    _agent.SetDestination(hit2.position);
                }

                if (_showDebugText) Debug.Log($"{gameObject.name} detects new nearest enemy {_nearestEnemy}, setting new position to {hit2.position}");
            }

            yield return wait;
        }

        //when agent hits escape point, reset state
        if (_showDebugText) Debug.Log($"{gameObject.name} has successfully escaped");
        State = DefaultState;
    }

    //update for patrol state
    private IEnumerator PatrolUpdate()
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

    //update for chase state
    private IEnumerator ChaseUpdate()
    {
        WaitForSeconds wait = new(UpdateSpeed);

        _agent.speed *= ChaseMovespeedMultiplier;

        while (enabled && _agent.enabled)
        {
            _agent.SetDestination(_target.transform.position);
            yield return wait;
        }
    }

    //get collider of nearest enemy that's too close
    private void GetTooCloseEnemy()
    {
        Collider[] nearestEnemies = Physics.OverlapSphere(transform.position, _minDistanceFromEnemy, _enemyLayerMask);
        Collider nearestEnemy = null;

        //work thru array til nearest distance and enemy are found
        float nearestDistance = float.MaxValue;
        foreach (Collider enemy in nearestEnemies)
        {
            //ignore this enemy
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

        _nearestDistance = nearestDistance;
        _nearestEnemy = nearestEnemy;
    }
}
