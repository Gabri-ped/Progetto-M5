using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using UnityEngine;
using UnityEngine.AI;

public class EnemyPath : MonoBehaviour
{
    [SerializeField] private float _waitTime = 1.5f;
    [SerializeField] private LayerMask _playerLayerMask;
    [SerializeField] private Transform[] _destinations;
    [SerializeField] private GameObject _gameover;
    
    [Header("Visione")]
    [SerializeField] private float viewAngle = 45f;
    [SerializeField] private float viewDistance = 10f;
    [SerializeField] private float eyeHeight;
    [SerializeField] private LayerMask obstacleMask;
    [SerializeField] private LineRenderer visionLine;
    [SerializeField] private int coneSegments = 6;

    
    private int _destinationIndex; 
    private enum enemyState { patrolling, waiting, allert }
    private enemyState currentState = enemyState.patrolling;
    private Transform _playerLastPosition;
    private NavMeshAgent _agent;
    private LifeController life;
    private Animator _anim;

    void Awake()
    {
        _gameover.SetActive(false);
    }
    void Start()
    {
        _anim = GetComponent<Animator>();
        _agent = GetComponent<NavMeshAgent>();
        SetDestinationAtIndex(0);
        if (visionLine != null)
        {
            visionLine.positionCount = coneSegments + 2;
            visionLine.loop = true;
        }
    }


    void Update()
    {
        DrawVisionCone();

        CheckStatus();

        if (CheckPlayer())
            {
               SetAnimState(idle: false, walking: false, running: true);
               currentState = enemyState.allert;
            }
          else
            {
               if (!_agent.isStopped)

                   SetAnimState(idle: false, walking: true, running: false);
                   currentState = enemyState.patrolling;
            }
           
    }

    public void SetDestinationAtNextIndex() => SetDestinationAtIndex(_destinationIndex + 1);

    public void SetDestinationAtPreviousIndex() => SetDestinationAtIndex(_destinationIndex - 1);

    public void SetDestinationAtIndex(int index)
    {
        if( index < 0)
        {
            while (index < 0)
            {
                index += _destinations.Length;
            }
        }
        else if (index >= _destinations.Length)
        {
            index = index % _destinations.Length;
        }
        _destinationIndex = index;

        _agent.SetDestination(_destinations[_destinationIndex].position);
    }

    private void Patrolling()
    {
        if (IsCloseEnoughToDestination())
        {
            SetDestinationAtNextIndex();
            StartCoroutine(StopEnemy());
            
        }
    }

    public bool IsCloseEnoughToDestination()
    {
        float sqrStoppingDistance = _agent.stoppingDistance * _agent.stoppingDistance;
        Vector3 toDestination = _destinations[_destinationIndex].position - transform.position;
        toDestination.y = 0;
        float sqrDistance = toDestination.sqrMagnitude;
        if(sqrDistance <= sqrStoppingDistance + 0.1f)
        {
            return true;
        }
        _agent.SetDestination(_destinations[_destinationIndex].position);
        return false;
    }

    IEnumerator StopEnemy()
    {
        currentState = enemyState.waiting;
        SetAnimState(idle: true, walking: false, running: false);
        _agent.isStopped = true;

        

        yield return new WaitForSeconds(_waitTime);

        _agent.isStopped = false;
        SetAnimState(idle: false, walking: true, running: false);
        currentState = enemyState.patrolling;

    }

    private bool CheckPlayer()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, viewDistance, _playerLayerMask);

        foreach (Collider collider in colliders)
        {
            if (collider.CompareTag("Player"))
            {
                
                Vector3 dirToPlayer = (collider.transform.position - transform.position).normalized;
                dirToPlayer.y = 0; 

                
                if (Vector3.Angle(transform.forward, dirToPlayer) < viewAngle)
                {
                   
                    if (!Physics.Raycast(transform.position + Vector3.up * eyeHeight, dirToPlayer, out RaycastHit hit, viewDistance,obstacleMask))
                    {
                        _playerLastPosition = collider.transform;
                        return true;
                    }
                }
            }
        }
        return false;
    }
    private void SetAnimState(bool idle = false,bool walking = false, bool running = false)
    {
        if (_anim != null)
        {
            _anim.SetBool("isWalk", walking);
            _anim.SetBool("isRun", running);
            _anim.SetBool("isIdle", idle);
        }
    }

    private void CheckStatus()
    {
        
       switch (currentState)
        {
            case enemyState.patrolling:
                Patrolling();
                break;
            case enemyState.waiting:
                break;
            case enemyState.allert:
                ChaseFollowPlayer();
                break; 
        }
    }

    private void ChaseFollowPlayer()
    {
        _agent.SetDestination(_playerLastPosition.position);
        _agent.isStopped = false;
        _agent.speed = 7f;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (life == null)
                life = other.GetComponent<LifeController>();
            life.LoseLife();
        }
    }
    void DrawVisionCone()
    {
        if (visionLine == null) return;

        Vector3 eyePos = transform.position + Vector3.up * 0.05f;

        visionLine.positionCount = coneSegments + 2;
        visionLine.SetPosition(0, eyePos);

        for (int i = 0; i <= coneSegments; i++)
        {
            float angle = -viewAngle + (viewAngle * 2) * (i / (float)coneSegments);
            Vector3 dir = Quaternion.Euler(0f, angle, 0f) * transform.forward;

            if (Physics.Raycast(eyePos, dir, out RaycastHit hit, viewDistance, obstacleMask))
            {
                visionLine.SetPosition(i + 1, hit.point);
            }
            else
            {
                visionLine.SetPosition(i + 1, eyePos + dir * viewDistance);
            }
        }
    }

}

