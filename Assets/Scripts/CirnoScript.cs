using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CirnoScript : MonoBehaviour
{
    public AudioSource audioSource;
    public IceSkateSound iceSkateSounds;
    private int nextSound;
    // Start is called before the first frame update
    void Start()
    {
        nextSound = 0;
    }

    // Update is called once per frame
    void Update()
    {

    }


    public void SkateStart()
    {
        audioSource.PlayOneShot(iceSkateSounds.IceSkateSounds[1]);
        nextSound++;
        if (nextSound == 5)
        {
            nextSound = 0;
        } 
    }
}
