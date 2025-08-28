using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Victory : MonoBehaviour
{ 
    [SerializeField] private GameObject _victory;
    [SerializeField] private AudioSource _audio;
    [SerializeField] private AudioClip _clip;
    [SerializeField] private AudioSource _masterAudio;


    private void Awake()
    {
       
        _victory.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _masterAudio.Pause();
            _victory.SetActive(true);
            _audio.PlayOneShot(_clip);
        }
    }
   
}
