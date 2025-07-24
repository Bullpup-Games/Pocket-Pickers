using System.Collections;
using TMPro;
using UnityEngine;

public class CreditScroll : MonoBehaviour
{
    [SerializeField] private Transform textScroll;
    [SerializeField] private float scrollSpeed;
    [SerializeField] private TextMeshProUGUI winText;
    [SerializeField] private TextMeshProUGUI thanksText;
    private bool _started;
 

    private void Start()
    {
        StartCoroutine(WaitBeforeStart());
    }

    private void Update()
    {
        if (!_started)
            return;
        
        var pos = textScroll.position;
        pos.y = Mathf.Lerp(pos.y, pos.y + 1, Time.deltaTime * scrollSpeed);
        textScroll.position = new Vector3(pos.x, pos.y, pos.z);
    }

    private IEnumerator WaitBeforeStart()
    {
        yield return new WaitForSeconds(2.5f);
        
        // Fade out the win text before starting credit roll
        var originalColor = winText.color;
        var time = 0f;
        const float duration = 2f;

        while (time < duration)
        {
            time += Time.deltaTime;
            var t = time / duration;
            var newAlpha = Mathf.Lerp(100f, 0f, t);
            winText.color = new Color(originalColor.r, originalColor.g, originalColor.b, newAlpha);
            thanksText.color = new Color(originalColor.r, originalColor.g, originalColor.b, newAlpha);
            yield return null;
        }

        winText.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0f);
        thanksText.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0f);

        // After fading start the credit roll
        _started = true;
        StartCoroutine(CreditTimeout());
    }

    private IEnumerator CreditTimeout()
    {
        yield return new WaitForSeconds(28f);
        
        LevelLoader.Instance.LoadLevel(LevelLoader.Instance.menu);
    }
}
