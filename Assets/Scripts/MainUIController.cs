using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainUIController : MonoBehaviour
{
    private static MainUIController _instance;
    public static MainUIController Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<MainUIController>();
            }
            return _instance;
        }
    }

    public bool hasBorder = true;
    public bool isPause;
    public int score = 0;
    public int length = 0;
    public TextMeshProUGUI msgText;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI lengthText;
    public Image pauseImage;
    public Image bgImage;
    public Sprite[] pauseSprites;
    private Color tempColor;

    private void Awake()
    {
        _instance = this;
        isPause = false;
        Time.timeScale = 1;
    }

    private void Start()
    {
        if (PlayerPrefs.GetInt("border", 1) == 0)
        {
            hasBorder = false;
        }
    }

    //根据分段更新背景颜色
    private void Update()
    {
        int level = score / 100;
        switch (level)
        {
            case 3:
                ColorUtility.TryParseHtmlString("#CCEEFFFF", out tempColor);
                bgImage.color = tempColor;
                msgText.text = "Level\n" + 2;
                break;
            case 5:
                ColorUtility.TryParseHtmlString("#CCFFDBFF", out tempColor);
                bgImage.color = tempColor;
                msgText.text = "Level\n" + 3;
                break;
            case 7:
                ColorUtility.TryParseHtmlString("#EBFFCCFF", out tempColor);
                bgImage.color = tempColor;
                msgText.text = "Level\n" + 4;
                break;
            case 9:
                ColorUtility.TryParseHtmlString("#FFF3CCFF", out tempColor);
                bgImage.color = tempColor;
                msgText.text = "Level\n" + 5;
                break;
            case 11:
                ColorUtility.TryParseHtmlString("#FFDACCFF", out tempColor);
                bgImage.color = tempColor;
                msgText.text = "Endless Mode";
                break;
        }
    }

    //更新控制面板数据
    public void UpdateUI(int s = 5, int l = 1)
    {
        score += s;
        length += l;
        scoreText.text = "Score:\n" + score;
        lengthText.text = "Length:\n" + length;

        // 检查用户是否已登录
        bool isUserLoggedIn = !string.IsNullOrEmpty(PlayerPrefs.GetString("AuthToken", ""));
        
        if (PlayerPrefs.GetInt("bests", 0) < Instance.score && PlayerPrefs.GetInt("bestl", 0) < Instance.length)
        {
            PlayerPrefs.SetInt("bestl", Instance.length);
            PlayerPrefs.SetInt("bests", Instance.score);

            // 根据登录状态发送游戏数据
            if (isUserLoggedIn)
            {
                // 已登录用户
                GameDataSender.Instance.RegSendGameData(length, score, length, score);
            }
            else
            {
                // 未登录用户
                GameDataSender.Instance.UnregSendGameData(length, score, length, score);
            }
        }
        else if (PlayerPrefs.GetInt("bests", 0) < Instance.score && PlayerPrefs.GetInt("bestl", 0) >= Instance.length)
        {
            PlayerPrefs.SetInt("bests", Instance.score);

            // 根据登录状态发送游戏数据
            if (isUserLoggedIn)
            {
                // 已登录用户
                GameDataSender.Instance.RegSendGameData(length, score, -1, score);
            }
            else
            {
                // 未登录用户
                GameDataSender.Instance.UnregSendGameData(length, score, -1, score);
            }
        }
        else if (PlayerPrefs.GetInt("bests", 0) >= Instance.score && PlayerPrefs.GetInt("bestl", 0) < Instance.length)
        {
            PlayerPrefs.SetInt("bestl", Instance.length);

            // 根据登录状态发送游戏数据
            if (isUserLoggedIn)
            {
                // 已登录用户
                GameDataSender.Instance.RegSendGameData(length, score, length, -1);
            }
            else
            {
                // 未登录用户
                GameDataSender.Instance.UnregSendGameData(length, score, length, -1);
            }
        }
        else
        {
            // 根据登录状态发送游戏数据
            if (isUserLoggedIn)
            {
                // 已登录用户
                GameDataSender.Instance.RegSendGameData(length, score, -1, -1);
            }
            else
            {
                // 未登录用户
                GameDataSender.Instance.UnregSendGameData(length, score, -1, -1);
            }
        }
    }

     //暂停
    public void Pause()
    {
        isPause = !isPause;
        Time.timeScale = isPause ? 0 : 1;
        pauseImage.sprite = pauseSprites[isPause ? 1 : 0];
    }

    //返回首页
    public void Home()
    {
        isPause = false;
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }
}
