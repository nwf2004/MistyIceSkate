using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Billboard : MonoBehaviour
{
    void OnEnable()
    {
        UnityEngine.Rendering.RenderPipelineManager.beginCameraRendering += LookAtCamera;
    }

    void OnDisable()
    {
        UnityEngine.Rendering.RenderPipelineManager.beginCameraRendering -= LookAtCamera;
    }

    void LookAtCamera(UnityEngine.Rendering.ScriptableRenderContext context, Camera camera)
    {
        transform.LookAt(camera.transform.position);
    }
}
