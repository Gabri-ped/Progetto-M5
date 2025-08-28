using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

public class LifeController : MonoBehaviour
{
    [SerializeField] private UnityEvent<int> OnLivesChanged;
    [SerializeField] private GameObject _gameover;
    [SerializeField] private Transform _respawnPoint;
    [SerializeField] private AudioSource _audio;
    [SerializeField] private AudioClip _clip;
    [SerializeField] private AudioSource _masterAudio;
    
    private int maxLives = 3;
    private float delay = 2f;
    private int currentLives;
    private NavMeshAgent _agent;
    

    private void Awake()
    {
        
        _gameover.SetActive(false);
    }

    void Start()
    {
        _agent = GetComponent<NavMeshAgent>();
        currentLives = maxLives;
        OnLivesChanged?.Invoke(currentLives);
    }

    public void LoseLife()
    {
        currentLives--;

       
        OnLivesChanged?.Invoke(currentLives);

        if(currentLives > 0)
        {
            StartCoroutine(DelayRespawn());
        }
        else if (currentLives <= 0)
        {
            GameOver();
        }
    }

    private IEnumerator DelayRespawn()
    {
        Respawn();
        yield return new WaitForSeconds(delay);

        TriggerWall[] triggers = FindObjectsOfType<TriggerWall>();
        foreach(TriggerWall trigger in triggers)
        {
            trigger.ResetWall();
        }

    }

   private void Respawn()
    {
        _agent.Warp(_respawnPoint.position);
    }

   private void GameOver()
    {
        _masterAudio.Pause();
        _gameover.SetActive(true);
        _audio.PlayOneShot(_clip);
    }
    
}
