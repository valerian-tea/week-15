using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    public float sensX;
    public float sensY;
    public Transform orientation;
    float xRotation;
    float yRotation;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Initialize rotations to 0
        xRotation = 0f;
        yRotation = 0f;
        
        // Apply initial rotation to both camera and orientation
        transform.rotation = Quaternion.Euler(xRotation, yRotation, 0);
        orientation.rotation = Quaternion.Euler(0, yRotation, 0);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // Update is called once per frame
    void Update()
    {
        bool shiftHeld = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        
        if (shiftHeld)
        {
            // Get horizontal input (left/right arrows or A/D keys)
            float horizontalInput = Input.GetAxis("Horizontal");
            
            // Only rotate if there's horizontal input
            if (horizontalInput != 0)
            {
                // Apply rotation based on horizontal input
                yRotation += horizontalInput * Time.deltaTime * sensX;
                transform.rotation = Quaternion.Euler(xRotation, yRotation, 0);
                orientation.rotation = Quaternion.Euler(0, yRotation, 0);
            }
        }
    }
}
