using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class GameDataSender : MonoBehaviour
{
    public static GameDataSender Instance;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);  // 设置为不在加载新场景时被销毁
        }
        else
        {
            if (Instance != this)
            {
                Destroy(gameObject);  // 如果已存在实例，则销毁新对象
            }
        }
    }

    private string unregApiUrl = "http://snake.abtxw.com/api/unregistered-save-game-data";
    private string regApiUrl = "http://snake.abtxw.com/api/registered-save-game-data";

    public void UnregSendGameData(int lastLength, int lastScore, int bestLength, int bestScore)
    {
        StartCoroutine(UnregSendGameDataCoroutine(lastLength, lastScore, bestLength, bestScore));
    }

    public void RegSendGameData(int lastLength, int lastScore, int bestLength, int bestScore)
    {
        StartCoroutine(RegSendGameDataCoroutine(lastLength, lastScore, bestLength, bestScore));
    }

    private IEnumerator SendGameDataCoroutine(UserGameData data, string apiUrl)
    {
        Debug.Log("UserGameData: " + data); // 用于调试

        string jsonData = JsonUtility.ToJson(data);
        byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(jsonData);

        using (UnityWebRequest request = new UnityWebRequest(apiUrl, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(jsonToSend);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            // 获取并设置cookie
            string cookie = UserSessionManager.GetCookie();
            if (!string.IsNullOrEmpty(cookie))
            {
                request.SetRequestHeader("Cookie", cookie);
            }
            else
            {
                Debug.LogError("The sessionCookie is NULL");
            }

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError(request.error);
            }
            else
            {
                Debug.Log("Game data sent successfully");
            }
        }
    }

    private IEnumerator RegSendGameDataCoroutine(int lastLength, int lastScore, int bestLength, int bestScore)
    {
        RegUserGameData data = new RegUserGameData
        {
            Username = PlayerPrefs.GetString("username", ""),
            LastLength = lastLength,
            LastScore = lastScore,
            BestLength = bestLength,
            BestScore = bestScore
        };

        yield return SendGameDataCoroutine(data, regApiUrl);
    }

    private IEnumerator UnregSendGameDataCoroutine(int lastLength, int lastScore, int bestLength, int bestScore)
    {
        UnregUserGameData data = new UnregUserGameData
        {
            LastLength = lastLength,
            LastScore = lastScore,
            BestLength = bestLength,
            BestScore = bestScore
        };

        yield return SendGameDataCoroutine(data, unregApiUrl);
    }
}

[System.Serializable]
public class UnregUserGameData : UserGameData
{
   
}

[System.Serializable]
public class RegUserGameData : UserGameData
{
    public string Username;
}

[System.Serializable]
public class UserGameData
{
    public int LastLength;
    public int LastScore;
    public int BestLength;
    public int BestScore;
}
