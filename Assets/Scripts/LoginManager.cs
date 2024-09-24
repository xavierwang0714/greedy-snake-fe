using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using TMPro;
using System.Runtime.InteropServices;

public class LoginManager : MonoBehaviour
{
    public static LoginManager Instance;

    // 文本输入框实例
    public TMP_InputField usernameInputField;
    public TMP_InputField passwordInputField;
    // 登录状态提示词
    public TMP_Text promptMessage;
    // 后端URL
    private string loginUrl = "http://snake.abtxw.com/api/player-login";
    // 用于标识是否开启心跳信号
    private bool isSendingHeartbeat = true;
    // 发送心跳信号Coroutine
    private Coroutine sendHeartbeatCoroutine;


    void Awake()
    {
        if (Instance != null)
        {
            // 如果已存在实例，则销毁旧对象
            Destroy(Instance.gameObject);
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);  // 设置为不在加载新场景时被销毁
    }

    public void Cancel()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }

    public void GetUsername()
    {
        if (usernameInputField != null)
        {
            PlayerPrefs.SetString("username", usernameInputField.text);
        }
        else
        {
            Debug.LogError("LoginManager-GetUsername: InputField is null");
        }
    }

    public void GetPassword()
    {
        if (passwordInputField != null)
        {
            PlayerPrefs.SetString("password", passwordInputField.text);
        }
        else
        {
            Debug.LogError("LoginManager-GetPassword: InputField is null");
        }
    }

    public void ConfirmLogin()
    {
        if(!string.IsNullOrEmpty(PlayerPrefs.GetString("username")) && !string.IsNullOrEmpty(PlayerPrefs.GetString("password")))
        {
           UserCredentials userCredentials = new UserCredentials
            {
                Username = PlayerPrefs.GetString("username"),
                Password = PlayerPrefs.GetString("password"),
            }; 

            StartCoroutine(SendLoginRequest(userCredentials));
        }
        else
        {
            promptMessage.text = "Please enter the username and password in full.";
            Debug.LogError("LoginManager-ConfirmLogin: username or password is empty.");
        }
    }

    private IEnumerator SendLoginRequest(UserCredentials userCredentials)
    {
        Debug.Log("SendLoginRequest-userCredentials: " + userCredentials);

        string jsonData = JsonUtility.ToJson(userCredentials);
        byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(jsonData);

        using (UnityWebRequest request = new UnityWebRequest(loginUrl, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(jsonToSend);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("LoginManager-SendLoginRequest: Login Successful");

                // 标识用户已登录，实际应改为后端返回的Token令牌
                PlayerPrefs.SetString("AuthToken","sdadadacasc");
                
                // 处理登录成功后的逻辑，可以通过request.downloadHandler.text 获取后端返回的信息
                string responseText = request.downloadHandler.text;
                Debug.Log("LoginManager-SendLoginRequest: Server Response: " + responseText);

                // 启动心跳信号
                // BeginSendHeartbeat();

                promptMessage.text = "Login successfully!";

                // 等待1.5秒后返回开始页面
                Invoke("BackToStartPage", 1.5f);
            }
            else
            {
                Debug.LogError("LoginManager-SendLoginRequest: Login Error: " + request.error);

                string responseText = request.downloadHandler.text;

                // 使用 JsonUtility 解析 JSON 数据
                ErrorResponse errorResponse = JsonUtility.FromJson<ErrorResponse>(responseText);

                Debug.Log("LoginManager-SendLoginRequest: LoginError responseText: " + responseText);

                if(errorResponse != null)
                {
                    switch(errorResponse.message)
                    {
                        case "User is already logged in":
                        {
                            promptMessage.text = $"The player {usernameInputField.text} is already logged in!";
                            break;
                        }
                        case "Incorrect password":
                        {
                            promptMessage.text = "Login failed! Please check your username and password.";
                            break;
                        }
                        default:
                        {
                            Debug.LogError("LoginManager-SendLoginRequest: Unknow error message");
                            break;
                        }
                    }
                }
                else
                {
                    Debug.LogError("LoginManager-SendLoginRequest: Failed to parse JSON");
                }
            }
        }
    }

    private void BackToStartPage()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }

    // 开始发送心跳信号
    private void BeginSendHeartbeat()
    {
        UserCredentials userCredentials = new UserCredentials
        {
            Username = PlayerPrefs.GetString("username", ""),
            Password = PlayerPrefs.GetString("password", ""),
        };

        // 启动心跳定时器
        isSendingHeartbeat = true;
        sendHeartbeatCoroutine = StartCoroutine(SendHeartbeat(userCredentials));
    }

    // 停止发送心跳信号
    public IEnumerator StopSendHeartbeatCoroutine()
    {
        Debug.Log("Stopping heartbeat...");

        // 终止正在进行的发送心跳信号Coroutine
        if (sendHeartbeatCoroutine != null)
        {
            StopCoroutine(sendHeartbeatCoroutine);
            sendHeartbeatCoroutine = null;
        }

        // 将标志变量设置为 false，停止心跳
        isSendingHeartbeat = false;

        // 等待一段时间，确保心跳已经停止
        yield return new WaitForSeconds(1f);

        Debug.Log("Heartbeat stopped.");
    }

    private IEnumerator SendHeartbeat(UserCredentials userCredentials)
    {
        while (isSendingHeartbeat)
        {
            yield return new WaitForSeconds(10f); // 每隔10秒发送一次心跳

            string apiUrl = "http://snake.abtxw.com/api/receive-heartbeat";

            string jsonData = JsonUtility.ToJson(userCredentials);
            byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(jsonData);

            using (UnityWebRequest request = new UnityWebRequest(apiUrl, "POST"))
            {
                request.uploadHandler = new UploadHandlerRaw(jsonToSend);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");

                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    Debug.Log("LoginManager-SendHeatbeat: Heartbeat sent successfully");
                }
                else
                {
                    Debug.LogError($"LoginManager-SendHeatbeat: Heartbeat failed with error: {request.error}");
                }
            }
        }
    }

    [System.Serializable]
    public class ErrorResponse
    {
        public string message;
    }
}

[System.Serializable]
public class UserCredentials
{
    public string Username;
    public string Password;
}
