using UnityEngine;
using UnityEngine.InputSystem;
public class Spawner : MonoBehaviour
{

    public GameObject circle;
    public GameObject square;
    public Camera camera;
    public int instances;

    // Use this for initialization
    void Start()
    {
        for (int x = 6; x < 30; x += 1)
        {
            for (int y = 6; y < 30; y += 1)
            {
                Instantiate(circle, new Vector3(x, 0, y), Quaternion.identity);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Mouse.current.leftButton.isPressed)
        {
            Vector3 pos = camera.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            pos.y = 0;
            Instantiate(circle, pos, Quaternion.identity);
        }

        if (Mouse.current.rightButton.isPressed)
        {
            Vector3 pos = camera.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            pos.y = 0;
            Instantiate(square, pos, Quaternion.identity);
        }
    }
}
