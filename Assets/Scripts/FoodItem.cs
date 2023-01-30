using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodItem : MonoBehaviour
{
    //Power up = 0
    //Cake = 1
    //Ice Cream = 2
    // Start is called before the first frame update
    float toRotateBy;
    void Start()
    {
        toRotateBy = 0;
    }

    // Update is called once per frame
    void Update()
    {
        
        transform.Rotate(0.0f, transform.rotation.y + 1.2f, 0.0f);
        toRotateBy += 0.5f * Time.deltaTime;
    }
}
