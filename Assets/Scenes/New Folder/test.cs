using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class test : MonoBehaviour
{
    [SerializeField] ProjectileInfo[] projectileInfoList;
    [SerializeField] Transform go;
    [SerializeField] float speed;
    [SerializeField] float x, y, z;

    [SerializeField] GameObject testObject;
    private void Start()
    {
        List<UserData> players = new List<UserData>()
        {
            new UserData("PlayerA",1,1000, 14) { playerKill = 25, playerDead = 2 },
            new UserData("PlayerB",2, 1000, 19) { playerKill = 5, playerDead = 15 },
            new UserData("PlayerC",3, 1000, 23) { playerKill = 10, playerDead = 10 },
            new UserData("PlayerD",4, 1000, 23) { playerKill = 10, playerDead = 20 },
            new UserData("PlayerD",5, 1000, 23) { playerKill = 20, playerDead = 10 },
        };
        ScoringSystem scoringSystem = new ScoringSystem();
        scoringSystem.UpdateRatingsForSession(players);
        scoringSystem.UpdateRankPoints(players);
        scoringSystem.CalculatePlayerPlace(players);
        //foreach (var player in players)
        //{
        //    Debug.Log($"{player.userName} - Id: {player.networkId}, rating: {player.Rating}, rank:{player.RankPoints}, score: {player.playerScore}, place: {player.playerPlace}, k/d: {player.playerKill}/{player.playerDead}");
        //}
        //for (int i = 0; i < players.Count; i++)
        //{
        //    for (int j = 0; j < players.Count; j++)
        //    {
        //        if (i != j)
        //        {
        //            UserData playerA = players[i];
        //            UserData playerB = players[j];

        //            float expectedScoreA = scoringSystem.CalculateExpectedScore(playerA, playerB);
        //            float expectedScoreB = scoringSystem.CalculateExpectedScore(playerB, playerA);

        //            Debug.Log($"{playerA.userName}/{playerB.userName}: {expectedScoreA} , B: {expectedScoreB}");
        //        }
        //    }
        //}

        
    }
    public void launchporjectile()
    {
        var projectileInfo = GetProjectileInfo();
        NetworkObject networkObject = NetworkObjectPool.Singleton.GetNetworkObject(projectileInfo.Prefab, projectileInfo.Prefab.transform.position, projectileInfo.Prefab.transform.rotation);
        networkObject.transform.forward = transform.forward;
        networkObject.transform.position = transform.position;
        //networkObject.GetComponent<BoomerangProjectile>().Initialize(networkObject.transform.position, new Vector3(10,0,10), projectileInfo);
        networkObject.Spawn(true);
    }
    protected virtual ProjectileInfo GetProjectileInfo()
    {
        foreach (var projectileInfo in projectileInfoList)
        {
            if (projectileInfo.Prefab && projectileInfo.Prefab.GetComponent<BoomerangProjectile>())
                return projectileInfo;
        }
        throw new System.Exception($"Action {name} has no usable Projectiles!");
    }
    private void Update()
    {
        //if(testObject != null)
        //{
        //    testObject.transform.position = Vector3.Lerp(transform.position, transform.position + Vector3.up, Mathf.PingPong(Time.time, 1));
        //    testObject.GetComponentInChildren<Transform>().SetParent(transform);
        //    testObject.transform.Rotate(Vector3.up,2);
        //}
        //else if(Input.GetMouseButtonDown(0))
        //{
        //    testObject = go.gameObject;
        //}
        if (Input.GetMouseButtonDown(0))
        {
            
        }
        testObject.transform.Rotate(Vector3.forward * speed);
    }
}

