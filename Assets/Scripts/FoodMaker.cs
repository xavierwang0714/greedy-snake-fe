using UnityEngine;
using UnityEngine.UI;

public class FoodMaker : MonoBehaviour
{
    private static FoodMaker _instance;
    public static FoodMaker Instance
    {
        get
        {
            if (_instance == null)
            {
                Debug.LogError("FoodMaker instance is null.");
            }
            return _instance;
        }
    }

    // 游戏区域限制
    public int xlimit = 20;
    public int ylimit = 13;
    public int xoffset = 12;

    // 食物和奖励预制体
    public GameObject foodPrefab;
    public GameObject rewardPrefab;
    public Sprite[] foodSprites; // 食物的图案数组

    //食物容器
    private Transform foodHolder;

    // 游戏边界碰撞器
    public GameObject upCollider;
    public GameObject downCollider;
    public GameObject leftCollider;
    public GameObject rightCollider;

    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        foodHolder = GameObject.Find("FoodHolder").transform;
        MakeFood(false);
    }

    public void MakeFood(bool isReward)
    {
        // 获取边界坐标
        float leftX = leftCollider.GetComponent<RectTransform>().localPosition.x;
        float rightX = rightCollider.GetComponent<RectTransform>().localPosition.x;
        float upperY = upCollider.GetComponent<RectTransform>().localPosition.y;
        float lowerY = downCollider.GetComponent<RectTransform>().localPosition.y;

        // 随机生成食物
        int index = Random.Range(0, foodSprites.Length);
        GameObject food = Instantiate(foodPrefab);
        food.GetComponent<Image>().sprite = foodSprites[index];
        food.transform.SetParent(foodHolder, false);
        int x = Random.Range((int)(leftX + 50f), (int)(rightX - 50f));
        int y = Random.Range((int)(lowerY + 50f), (int)(upperY - 50f));
        food.transform.localPosition = new Vector3(x, y, 0);

        // 根据isReward决定是否生成奖励
        if (isReward)
        {
            GameObject reward = Instantiate(rewardPrefab);
            reward.transform.SetParent(foodHolder, false);
            x = Random.Range(-xlimit + xoffset, xlimit);
            y = Random.Range(-ylimit, ylimit);
            reward.transform.localPosition = new Vector3(x * 30, y * 30, 0);
        }
    }
}
