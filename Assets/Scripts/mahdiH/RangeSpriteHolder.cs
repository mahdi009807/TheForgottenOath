using UnityEngine;

public class RangeSpriteHolder : MonoBehaviour
{
    public int change1 = 0;
    public void changePosition1()
    {
        change1++;
        transform.position += new Vector3(0, 0.1f, 0);
    }
    public void changePosition2()
    {
        if (change1 >= 2)
        {
            transform.position += new Vector3(0, -0.2f, 0);
            change1 = 0;
        }
        else if (change1 <= 1 && change1 > 0)
        {
            transform.position += new Vector3(0, -0.1f, 0);
            change1 = 0;
        }
    }
    
}
