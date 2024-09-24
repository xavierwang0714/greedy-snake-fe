using UnityEngine;

public class AutoSizeBoxCollider2D : MonoBehaviour
{
    public GameObject up;
    public GameObject down;
    public GameObject left;
    public GameObject right;

    private void Start()
    {
        // Ϊÿ���������Ϸ������� BoxCollider2D ��С
        AdjustCollider(up);
        AdjustCollider(down);
        AdjustCollider(left);
        AdjustCollider(right);
    }

    // ����ָ����Ϸ����� BoxCollider2D ��С����Ӧ�� RectTransform
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
