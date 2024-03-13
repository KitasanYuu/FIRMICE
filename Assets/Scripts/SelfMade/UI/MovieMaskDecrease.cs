using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovieMaskDecrease : MonoBehaviour
{
    public bool TestButton;
    private CanvasGroup cvg;
    [SerializeField]private float transformRate = 5f;

    void Start()
    {
        cvg = GetComponent<CanvasGroup>();
        cvg.alpha = 1;
    }

    void Update()
    {
        if (TestButton)
        {
            cvg.alpha = 1;
            TestButton = false;
        }

        float cvgalpha = cvg.alpha;

        if (cvgalpha <= 1 && cvgalpha >= 0.85)
            cvg.alpha = Mathf.Lerp(cvgalpha, 0, transformRate * Time.deltaTime);
        else if ((cvgalpha < 0.85 && cvgalpha > 0.1))
            cvg.alpha = Mathf.Lerp(cvgalpha, 0, transformRate * 10 * Time.deltaTime);
        else
            cvg.alpha = 0;
    }
}
