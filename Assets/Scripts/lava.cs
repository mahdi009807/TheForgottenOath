using UnityEngine;
using UnityEngine.U2D;

public class lava : MonoBehaviour
{
    public SpriteShapeController controller;
    public float waveHeight = 0.1f;
    public float waveFrequency = 1f;
    public float waveSpeed = 2f;

    private Spline spline;
    private float[] originalY;

    void Start()
    {
        spline = controller.spline;
        originalY = new float[spline.GetPointCount()];

        for (int i = 0; i < spline.GetPointCount(); i++)
            originalY[i] = spline.GetPosition(i).y;
    }

    void Update()
    {
        for (int i = 0; i < spline.GetPointCount(); i++)
        {
            Vector3 pos = spline.GetPosition(i);
            pos.y = originalY[i] + Mathf.Sin(Time.time * waveSpeed + i * waveFrequency) * waveHeight;
            spline.SetPosition(i, pos);
        }

        controller.BakeMesh();
    }
}