using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Obi;

public class TearCable : MonoBehaviour
{
    // Start is called before the first frame update
    private ObiRope rope;
    public bool torn = false;
    private float max_len = 1.35f;
    void Start()
    {
        rope = GetComponent<ObiRope>();
    }

    // Update is called once per frame
    void Update()
    {
        float len = rope.CalculateLength();
        if (!torn && len > max_len)
        {
            rope.Tear(rope.elements[rope.elements.Count / 2]);
            rope.RebuildConstraintsFromElements();
            torn = true;
            SimData pd = GameObject.Find("Player").GetComponent<SimData>();
            pd.tornCable = true;
            PandaController.PandaController pc = GameObject.Find("panda").GetComponent<PandaController.PandaController>();
            pc.EndTaskEarly();
            // allow the cable to fall to the ground
            GameObject.Find("cable_end").GetComponent<Rigidbody>().useGravity = true;
        }
    }
}
