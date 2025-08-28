using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TriggerWall : MonoBehaviour
{
   [SerializeField] GameObject canvasUI;      
   [SerializeField] GameObject wallPrefab;    
   [SerializeField] Transform spawnPoint;

    private List<GameObject> spawnedWalls = new List<GameObject>();
    private bool playerInRange = false;
    private bool wallSpawned = false;


    private void Awake()
    {
        canvasUI.SetActive(false); 
    }
    void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E) && !wallSpawned)
        {
           StartCoroutine(SpawnWallDelay(2f));
        }
    }
    private IEnumerator SpawnWallDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        GameObject wall = Instantiate(wallPrefab, spawnPoint.position, spawnPoint.rotation);
        spawnedWalls.Add(wall);
        wallSpawned = true;
        canvasUI.SetActive(false); 
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;

            if(!wallSpawned)
                canvasUI.SetActive(true); 
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            canvasUI.SetActive(false); 
        }
    }
    public void ResetWall()
    {
        if(spawnedWalls.Count > 0)
        {
            foreach (GameObject wall in spawnedWalls)
            {
                Destroy(wall);
            }
            spawnedWalls.Clear();
        }
        wallSpawned = false;         
        canvasUI.SetActive(false);
    }
}
