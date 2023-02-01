using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public bool Died;
    public GameObject playerToSpawn;
    public GameObject currentPlayerInScene;
    [SerializeField] CreateIce createIce;
    public GameObject CMFreecam;
    public Text IceCreamText;
    public Text PFText;
    public Text CakeText;
    public float cakesLeft;
    public float iceCreamLeft;
    float perfectFreeze;
    public Color red;
    public Color blue;
    public bool readyForIce = true;
    //BDFFFE FF001B 255 0 27 255
    // Start is called before the first frame update
    //display.text = countDown.ToString();
    void Start()
    {
        cakesLeft = 10;
        iceCreamLeft = 10;
        Died = false;
    }

    // Update is called once per frame
    void Update()
    {
        perfectFreeze = createIce.currentIceMeter;
        IceCreamText.text = "x " + iceCreamLeft;
        CakeText.text = "x " + cakesLeft;
        PFText.text = "x " + perfectFreeze;
        if (perfectFreeze < 6)
        {
            PFText.color = red;
        }
        else
        {
            PFText.color = blue;
        }
        if (Died)
        {
            //player.transform.position = new Vector3(215.8846f, 11.157f, 194.4f);
            //StartCoroutine(JustDied());
            Destroy(currentPlayerInScene);
            currentPlayerInScene = Instantiate(playerToSpawn, new Vector3(215.8846f, 15.157f, 194.4f), Quaternion.identity);
            currentPlayerInScene.SetActive(true);
            createIce = currentPlayerInScene.GetComponent<CreateIce>();
            CMFreecam.GetComponent<CinemachineFreeLook>().Follow = currentPlayerInScene.transform;
            CMFreecam.GetComponent<CinemachineFreeLook>().LookAt = currentPlayerInScene.transform;
            //Instantiate(iceBlock, hit.point, Quaternion.identity);
            Died = false;
        }

    }
}
