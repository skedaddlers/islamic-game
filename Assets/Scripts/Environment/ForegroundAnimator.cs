using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class ForegroundAnimator : MonoBehaviour
{
    public float animationDuration = 1f;
    public float xStartOffset = 100f;
    public float yStartOffset = 50f;

    public bool setEase = true;
    public bool isScale = false;

    private Vector3 initialPosition;
    private Vector3 initialScale;

    private void Awake()
    {
        GetComponent<Image>().enabled = false;
        initialPosition = transform.localPosition;
        initialScale = transform.localScale;
    }

    public void AnimateIn()
    {
        GetComponent<Image>().enabled = true;
        if (isScale)
        {
            transform.localScale = Vector3.zero;
            transform.DOScale(initialScale, animationDuration)
                .SetEase(setEase ? Ease.OutBack : Ease.Unset);
        }
        else
        {
            transform.localPosition = new Vector3(initialPosition.x + xStartOffset, initialPosition.y + yStartOffset, initialPosition.z);
            transform.DOLocalMove(initialPosition, animationDuration)
                .SetEase(setEase ? Ease.OutBack : Ease.Unset);
        }
    }

    public Tween AnimateInTween()
    {
        GetComponent<Image>().enabled = true;
        if (isScale)
        {
            transform.localScale = Vector3.zero;
            return transform.DOScale(initialScale, animationDuration)
                .SetEase(setEase ? Ease.OutBack : Ease.Unset);
        }
        else
        {
            GetComponent<Image>().enabled = true;
            transform.localPosition = new Vector3(initialPosition.x + xStartOffset, initialPosition.y + yStartOffset, initialPosition.z);
            return transform.DOLocalMove(initialPosition, animationDuration)
                .SetEase(setEase ? Ease.OutBack : Ease.Unset);
        }
    }
}