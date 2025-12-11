using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrinagleTest : MonoBehaviour
{
    [Header("Triangle Settings")]
    public float sideLength = 5f;
    public float delay = 0.01f;   // Time between new points
    public int iterations = 10000;
    public float dotSize = 0.03f;

    [Header("Colors")]
    public Color dotColor = Color.white;
    public Color triangleColor = Color.yellow;

    private Vector3[] corners;
    private List<Vector3> points = new List<Vector3>();
    private Vector3 currentDot;
    private bool running = false;

    void Start()
    {
        GenerateTriangle();
        StartCoroutine(RunChaosGame());
    }

    public void Restart()
    {
        points.Clear();
        GenerateTriangle();
        StartCoroutine(RunChaosGame());
    }
    void GenerateTriangle()
    {
        float height = Mathf.Sqrt(3f) / 2f * sideLength;

        corners = new Vector3[3];
        corners[0] = new Vector3(-sideLength / 2f, 0, 0);
        corners[1] = new Vector3(sideLength / 2f, 0, 0);
        corners[2] = new Vector3(0, height, 0);
    }

    IEnumerator RunChaosGame()
    {
        running = true;
        currentDot = RandomPointInTriangle(corners[0], corners[1], corners[2]);
        points.Add(currentDot);

        for (int i = 0; i < iterations; i++)
        {
            Vector3 corner = corners[Random.Range(0, 3)];
            currentDot = Vector3.Lerp(currentDot, corner, 0.5f);
            points.Add(currentDot);

            yield return new WaitForSeconds(delay);
        }

        running = false;
    }

    Vector3 RandomPointInTriangle(Vector3 a, Vector3 b, Vector3 c)
    {
        float r1 = Random.value;
        float r2 = Random.value;
        if (r1 + r2 > 1f)
        {
            r1 = 1 - r1;
            r2 = 1 - r2;
        }

        return a + r1 * (b - a) + r2 * (c - a);
    }

    void OnDrawGizmos()
    {
        if (corners == null || corners.Length < 3)
            GenerateTriangle();

        // Draw triangle outline
        Gizmos.color = triangleColor;
        Gizmos.DrawLine(transform.position + corners[0], transform.position + corners[1]);
        Gizmos.DrawLine(transform.position + corners[1], transform.position + corners[2]);
        Gizmos.DrawLine(transform.position + corners[2], transform.position + corners[0]);

        // Draw points
        Gizmos.color = dotColor;
        foreach (var p in points)
            Gizmos.DrawSphere(transform.position + p, dotSize);
    }
}
