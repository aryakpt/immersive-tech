using UnityEngine;

public class ObjectRotation : MonoBehaviour
{
  public float rotationSpeed = 10f;

  void Update()
  {
    // Handle mouse input for desktop
    if (Input.GetMouseButton(0))
    {
      float rotationX = Input.GetAxis("Mouse X") * rotationSpeed * Time.deltaTime;
      float rotationY = Input.GetAxis("Mouse Y") * rotationSpeed * Time.deltaTime;

      RotateObject(rotationX, rotationY);
    }

    // Handle touch input for mobile
    if (Input.touchCount > 0)
    {
      Touch touch = Input.GetTouch(0);
      if (touch.phase == TouchPhase.Moved)
      {
        float rotationX = touch.deltaPosition.x * rotationSpeed * Time.deltaTime;
        float rotationY = touch.deltaPosition.y * rotationSpeed * Time.deltaTime;

        RotateObject(rotationX, rotationY);
      }
    }
  }

  private void RotateObject(float rotationX, float rotationY)
  {
    // Rotate the object
    transform.Rotate(Vector3.up, -rotationX);
    transform.Rotate(Vector3.right, rotationY);
  }
}
