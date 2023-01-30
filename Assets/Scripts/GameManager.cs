using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public bool Died;
    public GameObject player;
    // Start is called before the first frame update
    void Start()
    {
        Died = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (Died)
        {
            player.transform.position = new Vector3(215.8846f, 11.157f, 194.4f);
            StartCoroutine(JustDied());
        }

    }

    private IEnumerator JustDied()
    {
        
        yield return new WaitForSeconds(1f);
        Died = false;
    }
}
