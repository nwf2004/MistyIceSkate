using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CirnoScript : MonoBehaviour
{

    private float horizontal;
    private float vertical;

    public float turnSmoothTime = 1f;
    float turnSmoothVelocity;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        horizontal = Input.GetAxisRaw("Horizontal");
        vertical = Input.GetAxisRaw("Vertical");
    }

    void LateUpdate()
    {
        //.normalized is so that when going diagonal we don't move extra fast
        Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;

        if (direction.magnitude >= 0.1f)
        {
            //Returns the angle between the x axis and a vector starting at 0 and terminating at x,z direction (its kind of confusing)
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg; //rad2 degrees returns radian to degrees

            //smooth movement
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            //time.Deltatime to make it frame rate independent
            //Cirno.transform.Rotate(Quaternion.Euler(0f, angle, 0f));
        }

    }
}
