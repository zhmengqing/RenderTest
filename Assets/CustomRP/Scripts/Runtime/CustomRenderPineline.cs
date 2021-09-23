using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class CustomRenderPineline : RenderPipeline
{
    CameraRenderer renderer = new CameraRenderer();
    bool useDynamicBatching, useGPUInstancing;
    protected override void Render(ScriptableRenderContext context, Camera[] cameras)
    {
        foreach (Camera cam in cameras)
        {
            renderer.Render(context, cam, useDynamicBatching, useGPUInstancing);
        }
    }

    public CustomRenderPineline(bool useDynamicBatching, bool useGPUInstancing, bool useSRPBatcher)
    {
        this.useDynamicBatching = useDynamicBatching;
        this.useGPUInstancing = useGPUInstancing;
        GraphicsSettings.useScriptableRenderPipelineBatching = useSRPBatcher;
    }
}
