using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateIce : MonoBehaviour
{
    public float currentIceMeter;
    [SerializeField] ThirdPersonMovement player;
    public bool wasGrounded;
    public Vector3 LastPlaceOnGround;
    public GameObject iceBlock;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log(LastPlaceOnGround);
        LastPlaceOnGround = transform.position;
        LastPlaceOnGround = LastPlaceOnGround + (Vector3.down * 1.2f);
        Vector3 downwards = transform.TransformDirection(Vector3.down);
        RaycastHit hit;
        Ray laserRay = new Ray(transform.position, downwards * 2);

        //Debug.DrawRay(transform.position, downwards * 2, Color.green);
        if (Physics.Raycast(laserRay, out hit, 1.5f))
        {
            if (hit.collider.tag != "Ice" && hit.collider.tag != "Water")
            {
                wasGrounded = false;
            }
            if (hit.collider.tag == "Water" && wasGrounded == true && currentIceMeter > 0)
            {
                Instantiate(iceBlock, hit.point + transform.forward, Quaternion.identity);
                currentIceMeter--;
            }
            else if (hit.collider.tag == "Water" && wasGrounded == false && currentIceMeter > 0)
            {
                currentIceMeter--;
                Instantiate(iceBlock, hit.point, Quaternion.identity);
            }
            if (hit.collider.tag == "Ice")
            {
                wasGrounded = true;
            }
        }
    }
}
