using UnityEngine;

public class AutoSizeBoxCollider2D : MonoBehaviour
{
    public GameObject up;
    public GameObject down;
    public GameObject left;
    public GameObject right;

    private void Start()
    {
        // 为每个方向的游戏对象调整 BoxCollider2D 大小
        AdjustCollider(up);
        AdjustCollider(down);
        AdjustCollider(left);
        AdjustCollider(right);
    }

    // 调整指定游戏对象的 BoxCollider2D 大小以适应其 RectTransform
    private void AdjustCollider(GameObject gameObject)
    {
        if (gameObject != null)
        {
            BoxCollider2D boxCollider = gameObject.GetComponent<BoxCollider2D>();
            if (boxCollider != null)
            {
                RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
                boxCollider.size = new Vector2(rectTransform.rect.width, rectTransform.rect.height);
            }
        }
    }
}
