using UnityEngine;

public class LidarTestEnvironment : MonoBehaviour
{
    [Header("Test Environment")]
    public bool createTestEnvironment = true;
    public float groundSize = 100f;
    public int obstacleCount = 10;
    public float obstacleRadius = 50f;

    void Start()
    {
        if (createTestEnvironment)
        {
            CreateTestEnvironment();
        }
    }

    void CreateTestEnvironment()
    {
        // Create ground
        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "LidarTestGround";
        ground.transform.position = Vector3.zero;
        ground.transform.localScale = new Vector3(groundSize / 10f, 1, groundSize / 10f);
        ground.GetComponent<Renderer>().material.color = Color.green;

        // Create some obstacles around the lidar
        for (int i = 0; i < obstacleCount; i++)
        {
            float angle = i * (360f / obstacleCount);
            float distance = Random.Range(5f, obstacleRadius);
            
            Vector3 position = new Vector3(
                Mathf.Cos(angle * Mathf.Deg2Rad) * distance,
                Random.Range(0.5f, 3f),
                Mathf.Sin(angle * Mathf.Deg2Rad) * distance
            );

            GameObject obstacle = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obstacle.name = $"LidarTestObstacle_{i}";
            obstacle.transform.position = position;
            obstacle.transform.localScale = new Vector3(
                Random.Range(1f, 3f),
                Random.Range(1f, 5f),
                Random.Range(1f, 3f)
            );
            obstacle.GetComponent<Renderer>().material.color = Random.ColorHSV();
        }

        // Create some walls
        CreateWall("North", new Vector3(0, 2, 25), new Vector3(50, 4, 1));
        CreateWall("South", new Vector3(0, 2, -25), new Vector3(50, 4, 1));
        CreateWall("East", new Vector3(25, 2, 0), new Vector3(1, 4, 50));
        CreateWall("West", new Vector3(-25, 2, 0), new Vector3(1, 4, 50));

        Debug.Log("[LIDAR TEST] Test environment created with ground, obstacles, and walls.");
    }

    void CreateWall(string name, Vector3 position, Vector3 scale)
    {
        GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.name = $"LidarTestWall_{name}";
        wall.transform.position = position;
        wall.transform.localScale = scale;
        wall.GetComponent<Renderer>().material.color = Color.gray;
    }
}