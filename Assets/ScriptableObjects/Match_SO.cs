using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MatchData", menuName = "ScriptableObjects/Match_SO", order = 1)]
public class Match_SO : ScriptableObject
{
    public enum GameState
    {
        Waiting,
        Playing,
        Ending
    }

    public int killsToWin = 3;
    public GameObject mapCameraPoint;
    public GameState currentState = GameState.Waiting;
    public float waitTimeAfterRound = 5f;
}