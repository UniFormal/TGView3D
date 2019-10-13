using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingleLine : MonoBehaviour
{
    // Start is called before the first frame update

    public Transform Origin;
    public Transform Target;
    public LineRenderer Line;

    void Start()
    {
        Line = GetComponent<LineRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        Line.SetPosition(1, Target.position);
        Line.SetPosition(0, Origin.position);
    }
}
