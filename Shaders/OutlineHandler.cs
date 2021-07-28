using System;
using UnityEngine;
using UnityEngine.Rendering;

[DefaultExecutionOrder(-1)]
public class OutlineHandler : MonoBehaviour
{
    public Material WriteObject;
    public Material ApplyOutline;
    private RaycastHit hit;
    private CommandBuffer commandBuffer;

    [Header("Debug")] 
    public Renderer[] OutlinedObject;


    public static OutlineHandler Instance { get; private set; }

    private void Awake()
    {
        OutlinedObject = null;
        // OutlinedObject = new Renderer[];
        // Array.Clear(OutlinedObject,0,OutlinedObject.Length);
        commandBuffer = new CommandBuffer();

        if (Instance == null)
            Instance = this;
        if (Instance != this)
            Destroy(this);
    }


    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        // if (OutlinedObject[0] == null || OutlinedObject == null) return;
        //     Debug.Log("Object is here");
        
        commandBuffer = new CommandBuffer();


        // commandBuffer = new CommandBuffer();
        int selectionBuffer = Shader.PropertyToID("_SelectionBuffer");
        commandBuffer.GetTemporaryRT(selectionBuffer, source.descriptor);


        //setup stuff


        //render selection buffer
        commandBuffer.SetRenderTarget(selectionBuffer);
        commandBuffer.ClearRenderTarget(true, true, Color.clear);

        if (OutlinedObject != null && OutlinedObject[0] != null)
        {
            
            foreach (Renderer outlinedObject in OutlinedObject)
            {
                commandBuffer.DrawRenderer(outlinedObject, WriteObject);
            }

            //apply everything and clean up in commandbuffer
        }
        
        
        commandBuffer.Blit(source, destination, ApplyOutline);

        commandBuffer.ReleaseTemporaryRT(selectionBuffer);


        //execute and clean up commandbuffer itself
        Graphics.ExecuteCommandBuffer(commandBuffer);
        commandBuffer.Dispose();
    }
}