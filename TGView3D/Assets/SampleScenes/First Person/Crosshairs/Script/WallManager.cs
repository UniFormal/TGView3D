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

public class WallManager : MonoBehaviour {

    /// <summary>
    /// Walls color
    /// </summary>
    Color defaultColor;

    /// <summary>
    /// Use particle for damage effect
    /// </summary>
    public GameObject damageEffectPrefab = null;
	ParticleSystem damageEffect;

	/// <summary>
	/// Variables for flashing objects
	/// </summary>
	public bool onFlashEffect;
    float flashCycle  = 0.0f;
    float timer       = 0.0f;
    float lastTime    = 0.0f;
    bool needDefault  = false;    

    /// <summary>
    /// Start in MonoBehaviour: Instantiate materials 
    /// </summary>
    void Start () 
    {
        if (onFlashEffect)
        {
            this.GetComponent<Renderer>().material = Instantiate(this.GetComponent<Renderer>().material) as Material;
            defaultColor = this.GetComponent<Renderer>().material.color;
        }

		GameObject  damageEffectObj = Instantiate(damageEffectPrefab) as GameObject;
		damageEffectObj.transform.SetParent(this.transform);
		damageEffect = damageEffectObj.GetComponent<ParticleSystem>();
		damageEffect.Stop();
    }


    /// <summary>
    /// Update in MonoBehaviour: Instantiate materials 
    /// </summary>
    void Update()
    {

        // If need to be back to default color, it will set in 0.15 secs.
        if (Time.time - lastTime > 0.15f && needDefault == true && onFlashEffect)
        {
            timer = 0.0f;
            this.GetComponent<Renderer>().material.color = defaultColor;
            needDefault = false;
        }
    }

    /// <summary>
    /// Flash objects while object is hit by crosshair
    /// </summary>
    public void FlashObject(bool hitObject)
    {
        if (hitObject && onFlashEffect)
        {
            timer += Time.deltaTime * 5.0f;
            flashCycle = Mathf.Round(timer);

            if (flashCycle % 2.0f == 0.0f)
            {
                needDefault = true;
                this.GetComponent<Renderer>().material.color = new Color(0.5f, 0.5f, 0.5f);
            }
            else
            {
                this.GetComponent<Renderer>().material.color = defaultColor;
                needDefault = false;
            }
        }

        lastTime = Time.time;
    }

    /// <summary>
    /// Generate hit effect
    /// </summary>
    public void Damage(Vector3 hitPos, float activeTime)
    {
        this.damageEffect.transform.position = hitPos;
        StartCoroutine(DamageEffectProcess(activeTime));       
    }

    /// <summary>
    /// IEnumerator 
    /// </summary>
    IEnumerator DamageEffectProcess(float fireActiveTime)
    {
        damageEffect.Play();

        float deltaTime = 0.0f;
        while (deltaTime < fireActiveTime)
        {
            deltaTime += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        damageEffect.Stop();

        yield return null;
    }

}
