using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RemoveShadow : MonoBehaviour
{
    float  storedShadowDistance;

    private void OnPreRender()
    {
        storedShadowDistance = QualitySettings.shadowDistance;
        QualitySettings.shadowDistance = 0;
    }

    private void OnPostRender()
    {
        QualitySettings.shadowDistance = storedShadowDistance;
    }
}
