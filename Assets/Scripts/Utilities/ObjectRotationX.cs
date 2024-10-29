using UnityEngine;

public class ObjectRotationX : MonoBehaviour
{
  public float rotationSpeed = 10f;

  void Update()
  {
    // Handle mouse input for desktop
    if (Input.GetMouseButton(0))
    {
      float rotationX = Input.GetAxis("Mouse X") * rotationSpeed * Time.deltaTime;

      RotateObject(rotationX);
    }

    // Handle touch input for mobile
    if (Input.touchCount > 0)
    {
      Touch touch = Input.GetTouch(0);
      if (touch.phase == TouchPhase.Moved)
      {
        float rotationX = touch.deltaPosition.x * rotationSpeed * Time.deltaTime;

        RotateObject(rotationX);
      }
    }
  }

  private void RotateObject(float rotationX)
  {
    // Rotate the object on the X axis
    transform.Rotate(Vector3.up, -rotationX);
  }
}
