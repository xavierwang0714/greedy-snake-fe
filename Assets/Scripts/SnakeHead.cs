using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

public class SnakeHead : MonoBehaviour
{
    public List<RectTransform> bodyList = new List<RectTransform>(); // ��������б�
    public float velocity = 0.35f; // ���ƶ����ٶ�
    public int step; // ÿ���ƶ�ʱ��ͷ�ƶ��Ĳ���
    public GameObject dieEffect; // ����ʱ��ʾ����Ч
    public GameObject bodyPrefab; // �������Ԥ�Ƽ�
    public Sprite[] bodySprites = new Sprite[2]; // ������ľ���ͼ������
    public AudioClip eatClip; // ��ʳ��ʱ����Ч
    public AudioClip dieClip; // ����ʱ����Ч
    public GameObject upCollider; // �ϱ߽���ײ��
    public GameObject downCollider; // �±߽���ײ��
    public GameObject leftCollider; // ��߽���ײ��
    public GameObject rightCollider; // �ұ߽���ײ��

    private int x; // ��ͷ��x���ϵ��ƶ�����
    private int y; // ��ͷ��y���ϵ��ƶ�����
    private Vector3 headPos; // ��ͷ��ǰ��λ��
    private Transform canvas; // ��Ϸ����������
    private bool isDie = false; // ������Ƿ��Ѿ�����

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
        headPos = gameObject.transform.localPosition; //������ͷ�ƶ�ǰ��λ��
        gameObject.transform.localPosition = new Vector3(headPos.x + x, headPos.y + y, headPos.z); //��ͷ������λ���ƶ�
        if(bodyList.Count > 0)
        {
            for(int i = bodyList.Count - 2; i>=0;i--) //�Ӻ���ǰ�ƶ�����
            {
                bodyList[i + 1].localPosition = bodyList[i].localPosition; //ÿһ�����ƶ�����ǰ��һ���ڵ��λ��
            }
            bodyList[0].localPosition = headPos; //��һ�������ƶ�����ͷ��λ��
        }
    }

    void Grow()
    {
        AudioSource.PlayClipAtPoint(eatClip, Vector3.zero);

        int index = (bodyList.Count % 2 == 0) ? 0 : 1;
        GameObject body = Instantiate(bodyPrefab);
        RectTransform bodyRectTransform = body.GetComponent<RectTransform>(); // ��ȡRectTransform���
        bodyRectTransform.anchoredPosition = Vector2.zero; // ����RectTransform��λ��
        body.GetComponent<Image>().sprite = bodySprites[index];
        body.transform.SetParent(canvas, false);
        bodyList.Add(bodyRectTransform); // ��RectTransform������ӵ��б���
    }

    void Die()
    {
        AudioSource.PlayClipAtPoint(dieClip, Vector3.zero);

        CancelInvoke();
        isDie = true;
        Instantiate(dieEffect);
        PlayerPrefs.SetInt("lastl", MainUIController.Instance.length);
        PlayerPrefs.SetInt("lasts", MainUIController.Instance.score);

        // ��Ϸ��ʱ1.5ms���ٿ�ʼ
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
