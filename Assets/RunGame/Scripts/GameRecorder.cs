﻿using UnityEngine;
using System.Collections;
using System;
using System.Text;
using System.Collections.Generic;

public class GameRecorder : MonoBehaviour
{

    [Serializable]
    public class GameData
    {
        public GameData(float avgY)
        {
            this.avgY = avgY;
        }
        public float avgY;
    }

    [Serializable]
    public class GameHistory
    {
        public string playerId;
        public string stageName;
        public float gameTime;
        public float frequency;
        public GameData[] gameData;
    }

    private const float Y_MOVE_DEFAULT = 0.15f;    //預設值
    private float yMove;    //震動觸發移動的值
    private float yOld = 0;
    private float timeLeft = 0.5f;
    private float avg = 0;
    private int count = 0;
    private ArrayList gameDataList;
    private bool isUpload = false;

    private const string IP = "140.134.26.86";
    private const string PORT = "3000";
    private const string URL = "http://" + IP + ":" + PORT + "/upload/uploadGameHistory";

    public string StageName = "Default";
    public float Frequency = 0.5f;

    // Use this for initialization
    void Start()
    {
        yMove = PlayerPrefs.GetFloat("Y_MOVE", Y_MOVE_DEFAULT);  //取得震動y值觸發移動
        gameDataList = new ArrayList();
    }

    // Update is called once per frame
    void Update()
    {
        if (GoalTrigger.isClear && !isUpload)
        {
            if (count == 0)
            {
                gameDataList.Add(new GameData(0));
            }
            else
            {
                avg /= count;
                gameDataList.Add(new GameData(avg));
            }

            GameHistory gameHistory = new GameHistory();
            gameHistory.playerId = PlayerPrefs.GetString("PlayerId");
            gameHistory.stageName = StageName;
            gameHistory.gameTime = GameTimer.time;
            gameHistory.frequency = Frequency;
            gameHistory.gameData = (GameData[])(gameDataList.ToArray(typeof(GameData)));
            isUpload = true;
            StartCoroutine(upload(gameHistory));

        }
        else
        {
            float dy = Mathf.Abs(Input.acceleration.y - yOld);

            timeLeft -= Time.deltaTime;
            if (timeLeft <= 0)
            {
                if (count == 0)
                {
                    gameDataList.Add(new GameData(0));
                }
                else
                {
                    avg /= count;
                    gameDataList.Add(new GameData(avg));
                }
                timeLeft = 0.5f;
                avg = 0;
                count = 0;
            }
            else
            {
                if (dy > yMove)
                {
                    avg += dy;
                    count++;
                }
            }

            yOld = Input.acceleration.y;
        }
    }

    private IEnumerator upload(GameHistory gameHistory)
    {
        Dictionary<string, string> postHeader = new Dictionary<string, string>();
        string jsonString = JsonUtility.ToJson(gameHistory);
        Debug.Log(jsonString);
        postHeader.Add("Content-Type", "application/json");
        postHeader.Add("Content-Length", jsonString.Length.ToString());
        WWW www = new WWW(URL, Encoding.UTF8.GetBytes(jsonString), postHeader);
        yield return www;
        print(www.text);
    }

}
