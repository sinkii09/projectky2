using System.Collections.Generic;
using UnityEngine;

public class ScoringSystem 
{
    public void UpdateRatingsForSession(List<UserData> players)
    {
        int K = 32;
        for (int i = 0; i < players.Count; i++)
        {
            for (int j = 0; j < players.Count; j++)
            {
                if (i != j)
                {
                    UserData playerA = players[i];
                    UserData playerB = players[j];

                    if(playerB.Rating > 2400)
                    {

                        K = 10;
                    }
                    else if(playerB.Rating > 2000)
                    {
                        K = 20;
                    }

                    int scoreA = CalculateScore(playerA);
                    int scoreB = CalculateScore(playerB);

                    UpdateRatings(playerA, playerB, scoreA, scoreB,K);
                }
            }
        }
    }
    public int CalculateScore(UserData userData)
    {
        return userData.playerKill*2 - userData.playerDead;
    }
    public void UpdateRatings(UserData playerA, UserData playerB, int scoreA, int scoreB, int K = 32)
    {
        float expectedScoreA = CalculateExpectedScore(playerA,playerB);
        float expectedScoreB = CalculateExpectedScore(playerB, playerA);

        float normScoreA = scoreA > scoreB ? 1.0f : (scoreA == scoreB ? 0.5f : 0.0f);
        float normScoreB = 1 - expectedScoreA;

        playerA.Rating = (int)(playerA.Rating + K * (normScoreA - expectedScoreA));
        playerB.Rating = (int)(playerB.Rating + K * (normScoreB - expectedScoreB));

        playerA.playerScore = (int)(playerA.playerScore + K * (normScoreA - expectedScoreA));
        playerB.playerScore = (int)(playerB.playerScore + K * (normScoreB - expectedScoreB));
    }
    public float CalculateExpectedScore(UserData playerA, UserData playerB)
    {
        return 1.0f / (1.0f + Mathf.Pow(10, (playerB.Rating - playerA.Rating) / 400.0f));
    }
    public int CalculateRankPoints(UserData player)
    {
        player.playerScore += CalculateScore(player) ;
        if(player.playerScore > 150)
        {
            player.playerScore = 150;
        }
        else if(player.playerScore < -50)
        {
            player.playerScore = -50;
        }
        return player.playerScore;
    }    
    public void UpdateRankPoints(List<UserData> players)
    {
        foreach (var player in players)
        {
            int rankPointsEarned = CalculateRankPoints(player);
            player.RankPoints += rankPointsEarned;
            if (player.RankPoints < 0)
            {
                player.RankPoints = 0;
            }
            Debug.Log($"Player {player.networkId} earned {player.playerScore} rank points. Total rank points: {player.RankPoints} with rating : {player.Rating}");
        }
    }
    public void CalculatePlayerPlace(List<UserData> players)
    {
        players.Sort((p1,p2) =>
        {
            int result = p2.playerKill.CompareTo(p1.playerKill);
            if(result == 0)
            {
                result = p1.playerDead.CompareTo(p2.playerDead);
                if(result == 0)
                {
                    result = p2.Rating.CompareTo(p1.Rating);
                }
            }
            return result;
        });
        
        for(int i = 0; i < players.Count; i++)
        {
            players[i].playerPlace = i + 1;
        }
    }
}
