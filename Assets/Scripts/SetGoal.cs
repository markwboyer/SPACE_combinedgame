using UnityEngine;

public class SetGoal : MonoBehaviour
{
    public Rigidbody2D goal;
    public float maxX = 200;
    public float maxY = 100;
    // Start is called before the first frame update
    void Start()
    {
        //float rando = Random.Range(0, 2);
        //float xint = Random.Range(0, maxX);
        //xint = rando < 0.5 ? Random.Range(maxX/4, maxX) : Random.Range(-maxX, -maxX/4);

        //float yint = Random.Range(0, maxY);
        //transform.position = new Vector3(xint, yint, 0);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
