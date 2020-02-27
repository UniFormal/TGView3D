using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InvertCols : MonoBehaviour
{

    public void Invert()
    {
        var texts = GetComponentsInChildren<TMPro.TextMeshProUGUI>();

        foreach (var text in texts)
        {

            var col = Color.white - new Color(text.color.r, text.color.g, text.color.b, 0);
            col.a = text.color.a;
            text.color = col;
        }
    }


}
