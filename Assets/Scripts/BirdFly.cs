using UnityEngine;

public class BirdFly : MonoBehaviour
{
    public float speed = 2f;

    void Update()
    {
        transform.Translate(Vector2.right * speed * Time.deltaTime);

        // Loop when off screen
        if (transform.position.x > 12) // Adjust based on your screen size
        {
            transform.position = new Vector2(-20, Random.Range(3f, 6f));
        }
    }
}

