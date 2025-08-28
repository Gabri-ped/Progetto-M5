using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PlayerController : MonoBehaviour
{
    private float runSpeed = 7f;
    private float walkSpeed = 3f;
    private NavMeshAgent agent;
    private Animator _anim;
    

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        _anim = GetComponent<Animator>();
    }

   
    void Update()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        Vector3 direction = new Vector3(h, 0f, v).normalized;

        bool isRunning = Input.GetKey(KeyCode.LeftShift);
        float _speed = isRunning ? runSpeed : walkSpeed;
        float _animSpeed = direction.magnitude * (isRunning ? 1f : 0.5f);
        _anim.SetFloat("Speed", _animSpeed);
        agent.velocity = direction * _speed;
       
    }
}
