using UnityEngine;
using UnityEngine.EventSystems;

public class TouchLook : MonoBehaviour
{
    public float sensitivity = 0.1f;
    public float minPitch = -60f;
    public float maxPitch = 60f;
    public bool invertY = false;

    private Transform cam;
    private float pitch = 0f;
    private int lookFingerId = -1;

    void Start()
    {
        cam = Camera.main.transform;
        pitch = cam.localEulerAngles.x;
    }

    void Update()
    {
        for (int i = 0; i < Input.touchCount; i++)
        {
            Touch t = Input.GetTouch(i);

            // Skip if over UI
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(t.fingerId))
                continue;

            // Start look: touch on right side
            if (t.phase == TouchPhase.Began && t.position.x > Screen.width / 2f)
            {
                lookFingerId = t.fingerId;
            }

            // Stop look: finger lifted
            if (t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled)
            {
                if (t.fingerId == lookFingerId)
                    lookFingerId = -1;
            }

            // Only rotate with the assigned look finger
            if (t.fingerId == lookFingerId && t.phase == TouchPhase.Moved)
            {
                Vector2 delta = t.deltaPosition * sensitivity;

                transform.Rotate(0f, delta.x, 0f); // Yaw

                pitch += invertY ? delta.y : -delta.y;
                pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

                cam.localEulerAngles = new Vector3(pitch, cam.localEulerAngles.y, 0f);
            }
        }
    }
}
