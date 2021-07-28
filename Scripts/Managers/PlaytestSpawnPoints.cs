using System;
using System.Collections;
using System.Collections.Generic;
using GP2_Team7.Managers;
using UnityEngine;

public class PlaytestSpawnPoints : MonoBehaviour
{

    [Header("keys F1-F9, one per button can be added")]
    public Transform[] SpawnPoints;


    
    // Update is called once per frame
    void Update()
    {
        if (SpawnPoints.Length == 0) return;
        
        if(Input.GetKeyDown(KeyCode.F1) && SpawnPoints.Length > 0)
        {
            GameManager.Player.transform.position = SpawnPoints[0].position;
        }
        if(Input.GetKeyDown(KeyCode.F2) && SpawnPoints.Length > 1){
            GameManager.Player.transform.position = SpawnPoints[1].position;        
        }
        if(Input.GetKeyDown(KeyCode.F3)&& SpawnPoints.Length > 2){
            GameManager.Player.transform.position = SpawnPoints[2].position;
        }
        if(Input.GetKeyDown(KeyCode.F4)&& SpawnPoints.Length > 3){
            GameManager.Player.transform.position = SpawnPoints[3].position;
        }
        if(Input.GetKeyDown(KeyCode.F5)&& SpawnPoints.Length > 4){
            GameManager.Player.transform.position = SpawnPoints[4].position;
        }
        if(Input.GetKeyDown(KeyCode.F6)&& SpawnPoints.Length > 5){
            GameManager.Player.transform.position = SpawnPoints[5].position;
        }
        if(Input.GetKeyDown(KeyCode.F7)&& SpawnPoints.Length > 6){
            GameManager.Player.transform.position = SpawnPoints[6].position;
        }
        if(Input.GetKeyDown(KeyCode.F8)&& SpawnPoints.Length > 7){
            GameManager.Player.transform.position = SpawnPoints[7].position;
        }
        if(Input.GetKeyDown(KeyCode.F9)&& SpawnPoints.Length > 8){
            GameManager.Player.transform.position = SpawnPoints[8].position;
        }


    }
}
