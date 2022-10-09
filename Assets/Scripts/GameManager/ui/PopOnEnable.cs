using UnityEngine;
using DG.Tweening;
using System.Collections;

public class PopOnEnable : MonoBehaviour
{
    [SerializeField] float delay = 0, duration = 0.3f;
    private void OnEnable()
    {
        if (delay == 0)
        {
            animate();
        }
        else
        {
            StartCoroutine(delayAndPop());
        }
    }

    IEnumerator delayAndPop()
    {
        transform.localScale = Vector3.zero;
        yield return new WaitForSecondsRealtime(delay);
        animate();
        yield break;
    }

    void animate()
    {
        transform.localScale = Vector3.zero;
        transform.DOScale(Vector3.one, duration).SetEase(Ease.OutBack);
    }
}
