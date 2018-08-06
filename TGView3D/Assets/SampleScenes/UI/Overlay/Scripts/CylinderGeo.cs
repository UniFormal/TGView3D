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

//-------------------------------------------------------------------------------------
/// <summary>
/// Create cylinder arc surface geometry.
/// </summary>
/// 
public class CylinderGeo : MonoBehaviour {

	public int widthTess = 100;
	public int heightTess = 2;
	public float cylinderWidthArc = 10;
	public float cylinderHeight = 10;
	public float radius = 10;

	private Vector3[] Vertices;
	private Vector2[] UV;
	private int[] Triangles;

	void CreateCylinderArc()
	{
		int vertexCount = widthTess * heightTess;
		int indexCount = (widthTess - 1) * (heightTess - 1) * 6;

		Vertices = new Vector3[vertexCount];
		UV = new Vector2[vertexCount];
		Triangles = new int[indexCount];

		float arcAngle = cylinderWidthArc / radius;
		for (int j = 0; j < heightTess; j++)
			for (int i = 0; i < widthTess; i++)
			{
				float currentAngle = -arcAngle / 2.0f + arcAngle * i / (float)(widthTess - 1);
				Vertices[j * widthTess + i].x = Mathf.Sin(currentAngle) * radius;
				Vertices[j * widthTess + i].y = 0.5f * cylinderHeight - j / (float)(heightTess - 1) * cylinderHeight;
				Vertices[j * widthTess + i].z = Mathf.Cos(currentAngle) * radius;

				UV[j * widthTess + i].x = (float)i / (float)(widthTess - 1);
				UV[j * widthTess + i].y = 1 - (float)j / (float)(heightTess - 1);
			}

		for (int j = 0; j < heightTess - 1; j++)
			for (int i = 0; i < widthTess - 1; i++)
			{
				Triangles[j * (widthTess - 1) * 6 + i * 6 + 0] = j * widthTess + i;
				Triangles[j * (widthTess - 1) * 6 + i * 6 + 1] = j * widthTess + i + 1;
				Triangles[j * (widthTess - 1) * 6 + i * 6 + 2] = (j + 1) * widthTess + (i + 1);
				Triangles[j * (widthTess - 1) * 6 + i * 6 + 3] = j * widthTess + i;
				Triangles[j * (widthTess - 1) * 6 + i * 6 + 4] = (j + 1) * widthTess + (i + 1);
				Triangles[j * (widthTess - 1) * 6 + i * 6 + 5] = (j + 1) * widthTess + i;
			}
	}
	// Use this for initialization
	void Start ()
	{
		CreateCylinderArc();
		gameObject.GetComponent<MeshFilter>().mesh.vertices = Vertices;
		gameObject.GetComponent<MeshFilter>().mesh.uv = UV;
		gameObject.GetComponent<MeshFilter>().mesh.triangles = Triangles;
	}
}
