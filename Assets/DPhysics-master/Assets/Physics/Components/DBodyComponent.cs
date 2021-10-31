using UnityEngine;
using System.Collections;
using FixedMath;
using UnityEngine.InputSystem;
/// <summary>
/// Monobehaviour component used to define a new physics object.
/// </summary>
public class DBodyComponent : MonoBehaviour
{

    public float speed;

    public float mass;
    public float restitution;
    public float drag;

    private ColliderComponent colliderComponent;
    private DBody body;

    //TODO: remove this temporary code
    void Start()
    {
        this.colliderComponent = GetComponent<ColliderComponent>();
        body = new DBody(
            colliderComponent.RequireCollider(),
            new Vector2F(transform.position),
            (Fix32)mass,
            (Fix32)restitution,
            (Fix32)drag
            );
        DWorld.Instance.AddObject(body);

        //update position
        StartCoroutine(UpdatePosition());
    }

    void Update()
    {

        float v = (Keyboard.current[Key.W].isPressed) ? 1f : (Keyboard.current[Key.S].isPressed) ? -1f : 0f;
        float h = (Keyboard.current[Key.D].isPressed) ? 1f : (Keyboard.current[Key.A].isPressed) ? -1f : 0f;
        Vector2 direction = new Vector2(h, v);
        if (direction != Vector2.zero)
        {
            direction *= 2;
            body.AddForce(new Vector2F(direction));
        }

        //commented this part because of the coroutine
        /*if (body.IsSleeping() || body.IsFixed())
            return;

        this.transform.position = body.InterpolatedPosition();
        */
    }

    void OnDrawGizmos()
    {
        /*if (physicsObject == null)
            return;
        Gizmos.color = (physicsObject.IsSleeping()) ? Color.green : Color.white;
        Gizmos.DrawCube(transform.position, Vector3.one * 2);*/
    }

    private IEnumerator UpdatePosition()
    {
        while (true)
        {
            if (body.IsFixed())
                yield return null;

            this.transform.position = body.InterpolatedPosition();
            yield return null;
        }
    }
}
