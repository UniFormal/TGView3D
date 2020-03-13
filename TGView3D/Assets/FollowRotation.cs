using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowRotation : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField]
    Transform ToFollow;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.rotation = ToFollow.rotation;
    }
}
