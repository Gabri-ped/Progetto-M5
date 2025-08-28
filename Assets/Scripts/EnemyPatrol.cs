using UnityEngine;
using UnityEngine.AI;

public class  EnemyPatrol : MonoBehaviour
{
    [SerializeField] private GameObject _gameover;
    [SerializeField] float chaseSpeed = 3.5f;

    [Header("Visione")]
    [SerializeField] private float viewAngle = 45f;
    [SerializeField] private float viewDistance = 10f;
    [SerializeField] private float eyeHeight = 1f;
    [SerializeField] private LayerMask obstacleMask;
    [SerializeField] private LineRenderer visionLine;
    [SerializeField] private int coneSegments = 6;

    private Quaternion targetRotation;
    private Quaternion startRotation;
    private Vector3 startPosition;
    private NavMeshAgent agent;
    private Transform player;
    private LifeController life;
    private Animator _anim;
    private enum EnemyState { Idle, Turning, Chasing, Returning }
    private EnemyState currentState = EnemyState.Idle;

    private float turnInterval = 3f;
    private float turnSpeed = 90f;
    private float turnAngle = 90f;
    private float timer;


    private void Awake()
    {
        _gameover.SetActive(false);
    }
    void Start()
    {
        _anim = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
       

        startPosition = transform.position;
        startRotation = transform.rotation;
        timer = turnInterval;
        targetRotation = startRotation;

        if (visionLine != null)
        {
            visionLine.positionCount = coneSegments + 2; 
            visionLine.loop = true; 
        }

        SetAgentStoppedState();
    }

    void Update()
    {
        DrawVisionCone();
         

        switch (currentState)
        {
            case EnemyState.Idle:
                SetAgentStoppedState();
                UpdateAnimations(idle:true,walk:false,run:false);
                HandleIdle();
                DetectPlayer();
                break;

            case EnemyState.Turning:
                SetAgentStoppedState();
                UpdateAnimations(idle:true,walk: false, run:false);
                HandleTurning();
                DetectPlayer();
                break;

            case EnemyState.Chasing:
                SetAgentActiveState();
                UpdateAnimations(idle:false,walk:false, run:true);
                HandleChasing();
                DetectPlayer();
                break;

            case EnemyState.Returning:
                SetAgentActiveState();
                UpdateAnimations(idle:false,walk:true,run:false);
                HandleReturning();
                DetectPlayer();
                break;
        }
    }

    void UpdateAnimations(bool idle,bool walk, bool run)
    {
        if(_anim != null)
        {
            _anim.SetBool("isIdle", idle);
            _anim.SetBool("isWalk", walk);
            _anim.SetBool("isRun", run);
        }
    }
    void HandleIdle()
    {
        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            targetRotation = Quaternion.Euler(0f, transform.eulerAngles.y + turnAngle, 0f);
            currentState = EnemyState.Turning;
        }
    }

    void HandleTurning()
    {
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);

        if (Quaternion.Angle(transform.rotation, targetRotation) < 0.1f)
        {
            transform.rotation = targetRotation;
            timer = turnInterval;
            currentState = EnemyState.Idle;
        }
    }

    void HandleChasing()
    {
        agent.speed = chaseSpeed * 2f;
        agent.SetDestination(player.position);

        if (!IsPlayerVisible())
        {
            currentState = EnemyState.Returning;
            agent.SetDestination(startPosition);
        }
    }

    void HandleReturning()
    {
        if (IsPlayerVisible())
        {
            currentState = EnemyState.Chasing;
            agent.SetDestination(player.position);
        }

        else if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.1f)
        {
            agent.speed = chaseSpeed;
            agent.ResetPath();
            agent.Warp(startPosition);   
            transform.rotation = startRotation;
            targetRotation = startRotation;
            timer = turnInterval;
            currentState = EnemyState.Idle;
            SetAgentStoppedState();
        }
    }

    void DetectPlayer()
    {
        if (IsPlayerVisible())
        {
            currentState = EnemyState.Chasing;
            SetAgentActiveState();
            agent.SetDestination(player.position);
        }
    }

    bool IsPlayerVisible()
    {
        Vector3 eyePos = transform.position + Vector3.up * eyeHeight;
        Vector3 dirToPlayer = (player.position - eyePos).normalized;
        float distToPlayer = Vector3.Distance(eyePos, player.position);

        if (Vector3.Angle(transform.forward, dirToPlayer) < viewAngle && distToPlayer <= viewDistance)
        {
            if (!Physics.Raycast(eyePos, dirToPlayer, distToPlayer, obstacleMask))
            {
                return true;
            }
        }
        return false;
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

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (life == null)
                life = other.GetComponent<LifeController>();
           life.LoseLife();
        }
    }

    void SetAgentStoppedState()
    {
        agent.isStopped = true;
        agent.updateRotation = false;
        agent.updatePosition = false;
    }

    void SetAgentActiveState()
    {
        agent.isStopped = false;
        agent.updateRotation = true;
        agent.updatePosition = true;
    }
}
