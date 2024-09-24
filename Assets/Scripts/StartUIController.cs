using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Runtime.InteropServices;
using UnityEngine.Events;
using System;

public class StartUIController : MonoBehaviour
{
    // ×îºóÒ»´ÎµÄ³¤¶ÈºÍ·ÖÊýÏÔÊ¾
    public TextMeshProUGUI lastText;
    // ×î¼Ñ³¤¶ÈºÍ·ÖÊýÏÔÊ¾
    public TextMeshProUGUI bestText;
    // ÑÕÉ«Ñ¡ÔñµÄÇÐ»»°´Å¥
    public Toggle blueToggle;
    public Toggle yellowToggle;
    // ±ß¿òÑ¡ÔñµÄÇÐ»»°´Å¥
    public Toggle borderToggle;
    public Toggle noBorderToggle;
    // ÓÃ»§µÇÂ¼×´Ì¬ÌáÊ¾ÐÅÏ¢
    public TextMeshProUGUI promptMessage;
    private string username;
    // µÇÂ¼/µÇ³ö°´Å¥
    public Button myButton;
    public Sprite[] buttonSprites = new Sprite[2]; // [0]´æ·ÅloginÍ¼Æ¬£¬[1]´æ·ÅlogoutÍ¼Æ¬

    [DllImport("__Internal")]
    private static extern void CheckRefresh();

    [DllImport("__Internal")]
    private static extern void ForceRefresh();

    private string fetchRegDataAPIUrl = "http://snake.abtxw.com/api/fetch-registered-game-data";

    private void Awake()
    {
    }

    private void Start()
    {
        CheckRefresh();

        // ÉèÖÃÄ¬ÈÏµÄÑÕÉ«ºÍ±ß¿ò
        SetDefaultColor();
        SetDefaultBorder();

        Debug.Log("StartUIController-Start: username: " + PlayerPrefs.GetString("username"));

        // »ñÈ¡ÓÃ»§Ãû
        username = PlayerPrefs.GetString("username", "");

        // ¼ì²éÓÃ»§ÊÇ·ñÒÑµÇÂ¼
        bool isUserLoggedIn = !string.IsNullOrEmpty(PlayerPrefs.GetString("AuthToken", ""));

        // ¸üÐÂÓÃ»§µÇÂ¼×´Ì¬ÌáÊ¾ÐÅÏ¢
        UpdatePromptMessage(isUserLoggedIn);

        // ¸üÐÂµÇÂ¼/µÇ³ö°´Å¥
        Button btn = myButton.GetComponent<Button>(); // Button×é¼þµÄÒýÓÃ
        Image img = myButton.GetComponent<Image>(); // Image×é¼þµÄÒýÓÃ
        if(isUserLoggedIn)
        {
            if(img != null && btn != null)
            {
                img.sprite = buttonSprites[1];
                btn.onClick.AddListener(GoToLogout);
                btn.onClick.AddListener(GoToForceRefresh);
            }
            else
            {
                Debug.LogError("StartUIController-Start: img or btn is null");
            }
        }
        else
        {
            if(img != null && btn != null)
            {
                img.sprite = buttonSprites[0];
                btn.onClick.AddListener(GoToLogin);
            }
            else
            {
                Debug.LogError("StartUIController-Start: img or btn is null");
            }
        }

        if (isUserLoggedIn) // Èç¹ûÓÃ»§ÒÑµÇÂ¼£¬Ôò´ÓÊý¾Ý¿â»ñÈ¡²¢»Ö¸´Êý¾Ý
        {
            Debug.Log("StartUIController-Start: Logged in, use user data for game data");

            UserCredentials userCredentials = new UserCredentials
            {
                Username = PlayerPrefs.GetString("username"),
                Password = PlayerPrefs.GetString("password"),
            };

            StartCoroutine(FetchRegUserGameData(userCredentials));
        }
        else // Èç¹ûÓÃ»§Î´µÇÂ¼£¬Ôò´ÓPlayerPrefs»ñÈ¡²¢»Ö¸´Êý¾Ý
        {
            Debug.Log("StartUIController-Start: No logged in, use PlayerPrefs for game data.");

            // »ñÈ¡µ±Ç°ÓÎÏ·Êý¾Ý
            int lastLength = PlayerPrefs.GetInt("lastl", 0);
            int lastScore = PlayerPrefs.GetInt("lasts", 0);
            int bestLength = PlayerPrefs.GetInt("bestl", 0);
            int bestScore = PlayerPrefs.GetInt("bests", 0);

            // ÉèÖÃ×îºóÒ»´ÎºÍ×î¼ÑµÄ³¤¶È¼°·ÖÊý
            lastText.text = $"Last length: {lastLength}\nLast score: {lastScore}";
            bestText.text = $"Best length: {bestLength}\nBest score: {bestScore}";
        }
    }

    private void SetDefaultColor()
    {
        if (PlayerPrefs.GetString("sh", "sh01") == "sh01")
        {
            blueToggle.isOn = true;
        }
        else
        {
            yellowToggle.isOn = true;
        }
    }

    private void SetDefaultBorder()
    {
        if (PlayerPrefs.GetInt("border", 1) == 1)
        {
            borderToggle.isOn = true;
        }
        else
        {
            noBorderToggle.isOn = true;
        }
    }

    public void BlueSelected(bool isOn)
    {
        if (isOn)
        {
            PlayerPrefs.SetString("sh", "sh01");
            PlayerPrefs.SetString("sb01", "sb0101");
            PlayerPrefs.SetString("sb02", "sb0102");
        }
    }

    public void YellowSelected(bool isOn)
    {
        if (isOn)
        {
            PlayerPrefs.SetString("sh", "sh02");
            PlayerPrefs.SetString("sb01", "sb0201");
            PlayerPrefs.SetString("sb02", "sb0202");
        }
    }

    public void BorderSelected(bool isOn)
    {
        PlayerPrefs.SetInt("border", isOn ? 1 : 0);
    }

    public void NoBorderSelected(bool isOn)
    {
        PlayerPrefs.SetInt("border", isOn ? 0 : 1);
    }

    public void StartGame()
    {
        // ¼ÓÔØÓÎÏ·Ö÷³¡¾°
        UnityEngine.SceneManagement.SceneManager.LoadScene(1);
    }

    public void GoToLogin()
    {
        // Ê¹ÓÃ³¡¾°Ë÷Òý¼ÓÔØLoginScene
        UnityEngine.SceneManagement.SceneManager.LoadScene(2);
    }

    public void UpdateUIWithGameData(UserGameData gameData)
    {
        if (gameData != null)
        {
            // ¸üÐÂUIÏÔÊ¾
            lastText.text = $"Last length: {gameData.LastLength}\nLast score: {gameData.LastScore}";
            bestText.text = $"Best length: {gameData.BestLength}\nBest score: {gameData.BestScore}";
        }
        else
        {
            Debug.LogWarning("StartUIController-UpdateUIWithGameData: Failed to parse game data from server response.");
        }
    }

    // »ñÈ¡ÒÑµÇÂ¼ÓÃ»§µÄÓÎÏ·Êý¾Ý
    private IEnumerator FetchRegUserGameData(UserCredentials userCredentials)
    {
        Debug.Log("StartUIController-FetchRegUserGameData: userCredentials: " + userCredentials); //ÓÃÓÚ²âÊÔ

        if (!string.IsNullOrEmpty(userCredentials.Username))
        {
            string jsonData = JsonUtility.ToJson(userCredentials);
            byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(jsonData);

            using (UnityWebRequest request = new UnityWebRequest(fetchRegDataAPIUrl, "POST"))
            {
                request.uploadHandler = new UploadHandlerRaw(jsonToSend);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");

                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.LogError(request.error);
                }
                else
                {
                    // ÇëÇó³É¹¦£¬½âÎöºó¶Ë·µ»ØµÄJSONÊý¾Ý
                    string jsonResponse = request.downloadHandler.text;
                    UserGameData gameData = JsonUtility.FromJson<UserGameData>(jsonResponse);

                    PlayerPrefs.SetInt("lastl", gameData.LastLength);
                    PlayerPrefs.SetInt("lasts", gameData.LastScore);
                    PlayerPrefs.SetInt("bestl", gameData.BestLength);
                    PlayerPrefs.SetInt("bests", gameData.BestScore);

                    // ¸üÐÂ¿ØÖÆÃæ°åÊý¾Ý
                    UpdateUIWithGameData(gameData);

                    Debug.Log("StartUIController-FetchRegUserGameData: Fetch game data successfully");
                }
            }
        }
        else
        {
            Debug.LogWarning("StartUIController-FetchRegUserGameData: Username not found in PlayerPrefs.");
        }
    }

    // ¸üÐÂÓÃ»§µÇÂ¼×´Ì¬ÌáÊ¾ÐÅÏ¢
    private void UpdatePromptMessage(bool isUserLoggedIn)
    {
        if (isUserLoggedIn)
        {
            promptMessage.text = $"Hi, {username}!";
        }
        else
        {
            promptMessage.text = "Hi, Game Player!";
        }
    }

    public void GoToLogout()
    {
        LogoutManager logoutManager = new LogoutManager();

        UserCredentials userCredentials = new UserCredentials
        {
            Username = PlayerPrefs.GetString("username", ""),
            Password = PlayerPrefs.GetString("password", "")
        };

        Debug.Log("StartUIController-GoToLogout: username: " + userCredentials.Username);

        if(!string.IsNullOrEmpty(userCredentials.Username))
        {
            // Ïòºó¶Ë·¢ËÍÇëÇó½«¸ÃÓÃ»§ÒÆ³ýÔÚÏßÁÐ±í
            StartCoroutine(GoToLogoutCoroutine(userCredentials, logoutManager));
        }
        else
        {
            Debug.LogError("StartUIController-GoToLogout: userCredentials is null");
        }
    }

    private IEnumerator GoToLogoutCoroutine(UserCredentials userCredentials, LogoutManager logoutManager)
    {
        Debug.Log("StartUIController-GoToLogout: method has been called");

        // Í£Ö¹·¢ËÍÐÄÌøÐÅºÅ
        GameObject loginManagerInstance = GameObject.Find("LoginManager");
        if (loginManagerInstance != null)
        {
            LoginManager loginManager = loginManagerInstance.GetComponent<LoginManager>();

            if(loginManager != null)
            {
                yield return StartCoroutine(loginManager.StopSendHeartbeatCoroutine());
            }
            else
            {
                Debug.LogError("StartUIController-GoToLogoutCoroutine: loginManager component not found on the LoginManager Instance.");
            }
        }
        else
        {
            Debug.LogError("StartUIController-GoToLogoutCoroutine: LoginManager Instance not found.");
        }

        yield return logoutManager.Logout(userCredentials);

        // ½«Ç°¶Ë´æ´¢µÄÓÃ»§ÐÅÏ¢Çå¿Õ
        PlayerPrefs.SetString("AuthToken", "");
        PlayerPrefs.SetString("username", "");
        PlayerPrefs.SetString("password", "");
     
        // ÖØÐÂÔØÈëÊ×Ò³³¡¾°
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }

    private IEnumerator WaitAndDoSomething()
    {
        // 等待2秒钟
        yield return new WaitForSeconds(2.0f);

        // 等待结束后执行的操作
        Debug.Log("Waited for 2 seconds!");
    }

    private void GoToForceRefresh()
    {
        WaitAndDoSomething();

        PlayerPrefs.SetString("AuthToken", "");
        PlayerPrefs.SetString("username", "");
        PlayerPrefs.SetString("password", "");

        ForceRefresh();
    }
}