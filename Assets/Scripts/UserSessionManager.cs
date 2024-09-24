using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using System.Runtime.InteropServices;
using TMPro;
using System.Collections.Generic;

public class UserSessionManager : MonoBehaviour
{
    public static UserSessionManager Instance;
    private static string sessionID;
    public TextMeshProUGUI lastText;
    public TextMeshProUGUI bestText;

    // 被 JavaScript 调用的方法
    [DllImport("__Internal")]
    private static extern void GetSessionIDFromJS();

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

    void Start()
    {
        // 获取当前sessionID
        GetSessionIDFromJS();

        // 用于测试
        Debug.Log("UserSessionManager: AuthToken: " + PlayerPrefs.GetString("AuthToken", ""));

        // 若用户未登录，则创建会话
        if (string.IsNullOrEmpty(PlayerPrefs.GetString("AuthToken", "")))
        {
            // 向后端发送请求
            StartCoroutine(CreateOrRestoreSession());
        }
        else
        {
            return;
        }
    }

    //要获取后端传回的游戏数据
    // 退出登录后要调用CreateOrRestoreSession()

    // 向服务器发送会话创建请求或恢复请求
    public IEnumerator CreateOrRestoreSession()
    {
        // 获取当前sessionID
        GetSessionIDFromJS();

        // 向后端发出Get请求
        UnityWebRequest request = UnityWebRequest.Get("http://snake.abtxw.com/api/create-session");
        request.SetRequestHeader("Cookie", sessionID);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            // 在控制台输出错误信息
            Debug.LogError(request.error);
        }
        else
        {
            // 检查请求是否成功完成
            if (request.isDone)
            {
                // 从浏览器cookie中获取sessionID
                GetSessionIDFromJS();

                // 检查是否有响应数据
                if (!string.IsNullOrEmpty(request.downloadHandler.text))
                {
                    // 解析 JSON 数据
                    string jsonResponse = request.downloadHandler.text;
                    Debug.Log("Received JSON response: " + jsonResponse);

                    UserGameData gameData = new UserGameData();
                    gameData = JsonUtility.FromJson<UserGameData>(jsonResponse);

                    Debug.Log("Last Length: " + gameData.LastLength);
                    Debug.Log("Last Score: " + gameData.LastScore);
                    Debug.Log("Best Length: " + gameData.BestLength);
                    Debug.Log("Best Score: " + gameData.BestScore);

                    PlayerPrefs.SetInt("lastl", gameData.LastLength);
                    PlayerPrefs.SetInt("lasts", gameData.LastScore);
                    PlayerPrefs.SetInt("bestl", gameData.BestLength);
                    PlayerPrefs.SetInt("bests", gameData.BestScore);

                    // 更新状态面板数据
                    UpdateUIWithGameData(gameData);
                }
                else
                {
                    Debug.LogError("Empty response received from the server.");
                }
            }
            else
            {
                Debug.LogError("Failed to complete the request.");
            }
        }
    }

    // 供外部调用以获取当前cookie
    public static string GetCookie()
    {
        return sessionID;
    }

    // 从浏览器Cookie中获取sessionID
    public void GetSessionID(string sessionIDFromCookie)
    {
        sessionID = sessionIDFromCookie;
    }

    private void UpdateUIWithGameData(UserGameData gameData)
    {
        if (gameData != null)
        {
            lastText.text = $"Last length: {gameData.LastLength}\nLast score: {gameData.LastScore}";
            bestText.text = $"Best length: {gameData.BestLength}\nBest score: {gameData.BestScore}";
        }
        else
        {
            Debug.LogWarning("UserSessionManager-UpdateUIWithGameData: Failed to parse game data from server response.");
        }
    }
}

[System.Serializable]
public class ServerResponse
{
    public string sessionId;
}
