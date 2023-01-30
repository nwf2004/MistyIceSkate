using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CirnoScript : MonoBehaviour
{
    public AudioSource audioSource;
    public IceSkateSound iceSkateSounds;
    private int nextSound;
    bool canPlayNewSound;
    // Start is called before the first frame update
    void Start()
    {
        nextSound = 0;
        canPlayNewSound = true;
    }

    // Update is called once per frame
    void Update()
    {

    }
   

    public void SkateStart()
    {
        if (canPlayNewSound)
        {
            audioSource.PlayOneShot(iceSkateSounds.IceSkateSounds[1]);
            nextSound++;
            if (nextSound == 5)
            {
                nextSound = 0;
            }
            StartCoroutine(JustPlayedSound());
        }
    }
    private IEnumerator JustPlayedSound()
    {
        canPlayNewSound = false;
        yield return new WaitForSeconds(.1f);
        canPlayNewSound = true;
    }
}
