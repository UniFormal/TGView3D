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
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class ShaderDemo : OVRDiscomfortWarningSource 
{
    List<GameObject> Objects;
    List<Texture> BumpTextures;
    List<Texture> ParallaxTextures;
    

    public Material[] ParallaxMaterials;
    public Material[] DisplacementMaterials;


    bool NormalMappingOn = true;
	bool SpecularOn = true;
    bool ParallaxOn = false;
    bool DisplacementOn = true;

    public Toggle normalToggle;
    public Toggle parallaxToggle;


	// Use this for initialization
	void Start () {
        Objects = new List<GameObject>();
        Objects.Add(GameObject.Find("Wall"));
        Objects.Add(GameObject.Find("Floor"));
        Objects.Add(GameObject.Find("Plane"));
        
        BumpTextures = new List<Texture>();
        ParallaxTextures = new List<Texture>();
        
        // Save texture references so we can turn them on and off
        for (int i = 0; i < ParallaxMaterials.Length; i++)
        {
            BumpTextures.Add(ParallaxMaterials[i].GetTexture("_BumpMap"));
            ParallaxTextures.Add(ParallaxMaterials[i].GetTexture("_ParallaxMap"));
        }

       
	} 


    public void SetNormalMapping(bool on)
    {
        NormalMappingOn = on;
        UpdateMaterials();
    }
    public void SetSpecular(bool on)
    {
        SpecularOn = on;
        UpdateMaterials();
    }
  
    public void SetParallax(bool on)
    {
        ParallaxOn = on;
        UpdateMaterials();

        
    }
    public void SetDisplacement(bool on)
    {
        DisplacementOn = on;
        UpdateMaterials();

        normalToggle.interactable = !on;
        parallaxToggle.interactable = !on;
    }


    void UpdateMaterials()
    {
        for (int i = 0; i < Objects.Count; i++)
        {
            if (DisplacementOn)
            {
                Objects[i].GetComponent<Renderer>().material = DisplacementMaterials[i];

                if (SpecularOn)
                {
                    Objects[i].GetComponent<Renderer>().material.SetColor("_SpecColor", Color.white);
                }
                else
                {
                    Objects[i].GetComponent<Renderer>().material.SetColor("_SpecColor", Color.black);
                }
            }
            else
            {
                Objects[i].GetComponent<Renderer>().material = ParallaxMaterials[i];
                if (NormalMappingOn)
                {
                    Objects[i].GetComponent<Renderer>().material.SetTexture("_BumpMap", BumpTextures[i]);
                }
                else
                {
                    Objects[i].GetComponent<Renderer>().material.SetTexture("_BumpMap", null);
                }
                if (SpecularOn)
                {
                    Objects[i].GetComponent<Renderer>().material.SetColor("_SpecColor", Color.white);
                }
                else
                {
                    Objects[i].GetComponent<Renderer>().material.SetColor("_SpecColor", Color.black);
                }
                if (ParallaxOn)
                {
                    Objects[i].GetComponent<Renderer>().material.SetTexture("_ParallaxMap", ParallaxTextures[i]);
                }
                else
                {
                    Objects[i].GetComponent<Renderer>().material.SetTexture("_ParallaxMap", null);
                }
            }
        }
    }

    override public IEnumerable<OVRDiscomfortWarning.DiscomfortWarning> GetWarnings()
    {
        if (SpecularOn)
        {
            yield return new OVRDiscomfortWarning.DiscomfortWarning(OVRDiscomfortWarning.DiscomfortWarningType.Aliasing);
        }
    }
  
}
