using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

public class SnakeHead : MonoBehaviour
{
    public List<RectTransform> bodyList = new List<RectTransform>(); // 蛇身体的列表
    public float velocity = 0.35f; // 蛇移动的速度
    public int step; // 每次移动时蛇头移动的步长
    public GameObject dieEffect; // 死亡时显示的特效
    public GameObject bodyPrefab; // 蛇身体的预制件
    public Sprite[] bodySprites = new Sprite[2]; // 蛇身体的精灵图像数组
    public AudioClip eatClip; // 吃食物时的音效
    public AudioClip dieClip; // 死亡时的音效
    public GameObject upCollider; // 上边界碰撞器
    public GameObject downCollider; // 下边界碰撞器
    public GameObject leftCollider; // 左边界碰撞器
    public GameObject rightCollider; // 右边界碰撞器

    private int x; // 蛇头在x轴上的移动方向
    private int y; // 蛇头在y轴上的移动方向
    private Vector3 headPos; // 蛇头当前的位置
    private Transform canvas; // 游戏画布的引用
    private bool isDie = false; // 标记蛇是否已经死亡

    private void Awake()
    {
        canvas = GameObject.Find("Canvas").transform;
        LoadSprites();
    }

    void Start()
    {
        InvokeRepeating("Move", 0, velocity);
        x = 0; y = step;
    }

    void Update()
    {
        HandleInput();
    }

    private void LoadSprites()
    {
        gameObject.GetComponent<Image>().sprite = Resources.Load<Sprite>(PlayerPrefs.GetString("sh", "sh01"));
        bodySprites[0] = Resources.Load<Sprite>(PlayerPrefs.GetString("sb01", "sb0101"));
        bodySprites[1] = Resources.Load<Sprite>(PlayerPrefs.GetString("sb02", "sb0102"));
    }

    private void HandleInput()
    {
        if (MainUIController.Instance.isPause || isDie) return;

        bool spaceDown = Input.GetKeyDown(KeyCode.Space);
        bool spaceUp = Input.GetKeyUp(KeyCode.Space);
        bool moveUp = Input.GetKey(KeyCode.W) && y != -step;
        bool moveDown = Input.GetKey(KeyCode.S) && y != step;
        bool moveLeft = Input.GetKey(KeyCode.A) && x != step;
        bool moveRight = Input.GetKey(KeyCode.D) && x != -step;

        if (spaceDown)
        {
            UpdateMoveFrequency(velocity - 0.10f);
        }
        else if (spaceUp)
        {
            UpdateMoveFrequency(velocity);
        }

        if (moveUp)
        {
            UpdateDirection(Quaternion.Euler(0, 0, 0), 0, step);
        }
        else if (moveDown)
        {
            UpdateDirection(Quaternion.Euler(0, 0, 180), 0, -step);
        }
        else if (moveLeft)
        {
            UpdateDirection(Quaternion.Euler(0, 0, 90), -step, 0);
        }
        else if (moveRight)
        {
            UpdateDirection(Quaternion.Euler(0, 0, -90), step, 0);
        }
    }

    private void UpdateMoveFrequency(float newVelocity)
    {
        CancelInvoke();
        InvokeRepeating("Move", 0, newVelocity);
    }

    private void UpdateDirection(Quaternion rotation, int newX, int newY)
    {
        gameObject.transform.localRotation = rotation;
        x = newX; y = newY;
    }

    private float FetchColliderPosition(GameObject go, string axis)
    {
        if (go != null)
        {
            RectTransform rectTransform = go.GetComponent<RectTransform>();
            switch (axis)
            {
                case "x":
                    return rectTransform.localPosition.x;
                case "y":
                    return rectTransform.localPosition.y;
                case "z":
                    return rectTransform.localPosition.z;
                default:
                    return 0;
            }
        }
        return 0;
    }

    void Move()
    {
        headPos = gameObject.transform.localPosition; //保存蛇头移动前的位置
        gameObject.transform.localPosition = new Vector3(headPos.x + x, headPos.y + y, headPos.z); //蛇头向期望位置移动
        if(bodyList.Count > 0)
        {
            for(int i = bodyList.Count - 2; i>=0;i--) //从后向前移动蛇身
            {
                bodyList[i + 1].localPosition = bodyList[i].localPosition; //每一个都移动到它前面一个节点的位置
            }
            bodyList[0].localPosition = headPos; //第一个蛇身移动到蛇头的位置
        }
    }

    void Grow()
    {
        AudioSource.PlayClipAtPoint(eatClip, Vector3.zero);

        int index = (bodyList.Count % 2 == 0) ? 0 : 1;
        GameObject body = Instantiate(bodyPrefab);
        RectTransform bodyRectTransform = body.GetComponent<RectTransform>(); // 获取RectTransform组件
        bodyRectTransform.anchoredPosition = Vector2.zero; // 设置RectTransform的位置
        body.GetComponent<Image>().sprite = bodySprites[index];
        body.transform.SetParent(canvas, false);
        bodyList.Add(bodyRectTransform); // 将RectTransform对象添加到列表中
    }

    void Die()
    {
        AudioSource.PlayClipAtPoint(dieClip, Vector3.zero);

        CancelInvoke();
        isDie = true;
        Instantiate(dieEffect);
        PlayerPrefs.SetInt("lastl", MainUIController.Instance.length);
        PlayerPrefs.SetInt("lasts", MainUIController.Instance.score);

        // 游戏延时1.5ms后再开始
        StartCoroutine(GameOver(1.5f));
    }

    IEnumerator GameOver(float t)
    {
        yield return new WaitForSeconds(t);
        UnityEngine.SceneManagement.SceneManager.LoadScene(1);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Food"))
        {
            Destroy(collision.gameObject);
            MainUIController.Instance.UpdateUI();
            Grow();
            FoodMaker.Instance.MakeFood(Random.Range(0, 100) < 20 ? true : false);
        } 
        else if (collision.gameObject.CompareTag("Reward"))
        {
            Destroy(collision.gameObject);
            MainUIController.Instance.UpdateUI(Random.Range(5, 15) * 10);
            Grow();
        }
        else if (collision.gameObject.CompareTag("Body"))
        {
            Die();
        }
        else
        {
            if (MainUIController.Instance.hasBorder)
            {
                Die();
            }
            else
            {
                switch (collision.gameObject.name)
                {
                    case "UpCollider":
                        transform.localPosition = new Vector3(transform.localPosition.x, FetchColliderPosition(downCollider, "y") + 50f, transform.localPosition.z);
                        break;
                    case "DownCollider":
                        transform.localPosition = new Vector3(transform.localPosition.x, FetchColliderPosition(upCollider, "y") - 50f, transform.localPosition.z);
                        break;
                    case "LeftCollider":
                        transform.localPosition = new Vector3(FetchColliderPosition(rightCollider, "x") - 50f, transform.localPosition.y, transform.localPosition.z);
                        break;
                    case "RightCollider":
                        transform.localPosition = new Vector3(FetchColliderPosition(leftCollider, "x") + 50f, transform.localPosition.y, transform.localPosition.z);
                        break;
                }
            }
        }
    }
}
