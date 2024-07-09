using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VehicleRandomMovement : MonoBehaviour
{
    public float speed = 3.0f;
    public float reverseSpeed = -2.0f;
    public float rotationAngle = 90.0f;
    public float rotationTime = 2.0f; // time it takes to rotate 90 degrees
    private bool reversing = false;
    private bool rotating = false;
    private Quaternion rotationTarget;

    void Update()
    {
        if (reversing)
        {
            // Move the vehicle backwards
            transform.Translate(Vector3.forward * reverseSpeed * Time.deltaTime);
        }
        else
        {
            // Move the vehicle forward
            transform.Translate(Vector3.forward * speed * Time.deltaTime);
        }

        if (rotating)
        {
            // Rotate the vehicle towards the rotation target smoothly
            transform.rotation = Quaternion.Lerp(transform.rotation, rotationTarget, Time.deltaTime / rotationTime);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        // When the vehicle collides with something, it starts to reverse and rotate
        StartCoroutine(ReverseAndRotate());
    }

    IEnumerator ReverseAndRotate()
    {
        reversing = true;
        yield return new WaitForSeconds(2); // Reverse for 2 seconds

        // Set the rotation target 90 degrees to the right
        rotationTarget = Quaternion.Euler(transform.eulerAngles + new Vector3(0, rotationAngle, 0));
        rotating = true;

        yield return new WaitForSeconds(rotationTime); // Wait for the rotation time

        rotating = false; // Stop rotating
        reversing = false; // Stop reversing and continue moving forward
    }
}
