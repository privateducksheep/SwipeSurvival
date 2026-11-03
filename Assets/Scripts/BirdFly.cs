using UnityEngine;

public class BirdFly : MonoBehaviour
{
    public float speed = 2f;
    //How far past the left edge the bird restarts, so there is a short gap between passes
    public float respawnGap = 8f;

    void Update()
    {
        transform.Translate(Vector2.right * speed * Time.deltaTime);

        // Loop once the bird is past the right edge of the view, whatever the screen width
        Camera viewCamera = Camera.main;
        if (viewCamera == null)
            return;

        float halfWidth = CameraFit.HalfViewAt(viewCamera, transform.position.z).x;
        float birdHalfWidth = GetComponent<Renderer>().bounds.extents.x;
        float rightEdge = viewCamera.transform.position.x + halfWidth + birdHalfWidth;

        if (transform.position.x > rightEdge)
        {
            float leftEdge = viewCamera.transform.position.x - halfWidth - birdHalfWidth;
            transform.position = new Vector3(leftEdge - respawnGap, Random.Range(3f, 6f), transform.position.z);
        }
    }
}
