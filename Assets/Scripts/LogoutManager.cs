using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class LogoutManager : MonoBehaviour
{
    public static LogoutManager Instance;
    private string logoutUrl = "http://snake.abtxw.com/api/player-logout";

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

    public IEnumerator Logout(UserCredentials userCredentials)
    {
        Debug.Log("LogoutManager-Logout: username: " + userCredentials.Username); // 用于调试

        string jsonData = JsonUtility.ToJson(userCredentials);
        byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(jsonData);

        using (UnityWebRequest request = new UnityWebRequest(logoutUrl, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(jsonToSend);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("LogoutManager-Logout: Logout Successful");
            }
            else
            {
                Debug.LogError("LogoutManager-Logout: Logout Error: " + request.error);
            }
        }
    }
}
