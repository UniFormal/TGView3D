/************************************************************************************

Copyright   :   Copyright 2017 Oculus VR, LLC. All Rights reserved.

Licensed under the Oculus VR Rift SDK License Version 3.4.1 (the "License");
you may not use the Oculus VR Rift SDK except in compliance with the License,
which is provided at the time of installation or download, or which
otherwise accompanies this software in either electronic or hard copy form.

You may obtain a copy of the License at

https://developer.oculus.com/licenses/sdk-3.4.1

Unless required by applicable law or agreed to in writing, the Oculus VR SDK
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.

************************************************************************************/

using UnityEngine;
using System.Collections;

public class OVRDebugDraw : MonoBehaviour
{
    private static OVRDebugDraw Instance;
    public Material LineMaterial;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }


    static public void AddLine(Vector3 start, Vector3 end, float width, Color color, float duration)
    {
        if (Instance == null)
            return;

        LineRenderer lineRenderer;
        var debugdraw = PrepareLine(width, color, out lineRenderer);

        lineRenderer.sharedMaterial.color = color;
        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, start);
        lineRenderer.SetPosition(1, end);
        debugdraw.StartCoroutine(FadeLine(lineRenderer, color, duration));
    }

    static IEnumerator FadeLine(LineRenderer line, Color color, float duration)
    {
        float remaining = duration;
        while (remaining > 0)
        {
            color.a = remaining / duration;
	        remaining -= Time.deltaTime;
	        line.startColor = color;
	        line.endColor = color;
            yield return new WaitForEndOfFrame();
        }
        Destroy(line.gameObject);
    }

    public static void AddCross(Vector3 target, float size, float lineWidth, Color color, float duration)
    {
        if (Instance == null)
            return;

        LineRenderer lineRenderer;
        var debugdraw = PrepareLine(lineWidth, color, out lineRenderer);
        lineRenderer.positionCount = 6;
        lineRenderer.SetPosition(0, target + new Vector3(0, size, 0));
        lineRenderer.SetPosition(1, target - new Vector3(0, size, 0));
        lineRenderer.SetPosition(2, target + new Vector3(size, 0, 0));
        lineRenderer.SetPosition(3, target - new Vector3(size, 0, 0));
        lineRenderer.SetPosition(4, target + new Vector3(0, 0, size));
        lineRenderer.SetPosition(5, target - new Vector3(0, 0, size));
        debugdraw.StartCoroutine(FadeLine(lineRenderer, color, duration));
    }

    private static OVRDebugDraw PrepareLine(float width, Color color, out LineRenderer lineRenderer)
    {
        var go = new GameObject();
        go.transform.SetParent(Instance.transform);
        go.name = "DebugDrawLine";
        var debugdraw = go.AddComponent<OVRDebugDraw>();
        lineRenderer = go.AddComponent<LineRenderer>();

	    lineRenderer.startWidth = width;
	    lineRenderer.endWidth = width;
        lineRenderer.material = new Material(Instance.LineMaterial);
        lineRenderer.sharedMaterial.color = color;
        return debugdraw;
    }
}

