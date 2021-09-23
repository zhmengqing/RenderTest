using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public partial class CameraRenderer
{
    static ShaderTagId unlitShaderTagId = new ShaderTagId("SRPDefaultUnlit");
    static ShaderTagId litShaderTagId = new ShaderTagId("CustomLit");
    const string bufferName = "Render Camera";
#if !UNITY_EDITOR
    const string sampleName = bufferName;
#endif
    //存储剔除后的结果数据
    CullingResults cullingResults;
    ScriptableRenderContext context;
    Camera camera;
    public void Render(ScriptableRenderContext context, Camera camera, bool useDynamicBatching, bool useGPUInstancing)
    {
        this.context = context;
        this.camera = camera;
        PrepareBuffer();
        //在GAME视图绘制的几何体也绘制到SCENE视图中
        PrepareForSceneWindow();
        if (!Cull())
        {
            return;
        }

        Setup();
        DrawVisibleGeomery(useDynamicBatching, useGPUInstancing);
        DrawUnsupportedShaders();
        DrawGizmos();
        Submit();
    }
    void Setup()
    {
        context.SetupCameraProperties(camera);
        //得到相机的clear flags
        CameraClearFlags flags = camera.clearFlags;
        //设置相机清除状态
        buffer.ClearRenderTarget(flags <= CameraClearFlags.Depth, flags == CameraClearFlags.Color, flags == CameraClearFlags.Color ? camera.backgroundColor.linear : Color.clear);
        buffer.BeginSample(sampleName);
        ExcuteBuffer();
    }
    void DrawVisibleGeomery()
    {
        var sortingSettings = new SortingSettings(camera)
        {
            criteria = SortingCriteria.CommonOpaque
        };

        var drawingSettings = new DrawingSettings(unlitShaderTagId, sortingSettings);
        var filteringSettings = new FilteringSettings(RenderQueueRange.all);
        //1、绘制不透明物体
        context.DrawRenderers(cullingResults, ref drawingSettings, ref filteringSettings);
        //2、绘制天空盒
        context.DrawSkybox(camera);

        sortingSettings.criteria = SortingCriteria.CommonTransparent;
        drawingSettings.sortingSettings = sortingSettings;
        filteringSettings.renderQueueRange = RenderQueueRange.transparent;
        //3、绘制透明物体
        context.DrawRenderers(cullingResults, ref drawingSettings, ref filteringSettings);
    }
    void DrawVisibleGeomery(bool useDynamicBatching, bool useGPUInstancing)
    {
        var sortingSettings = new SortingSettings(camera)
        {
            criteria = SortingCriteria.CommonOpaque
        };
        var drawingSettings = new DrawingSettings(unlitShaderTagId, sortingSettings)
        {
            enableDynamicBatching = useDynamicBatching,
            enableInstancing = useGPUInstancing
        };

        var filteringSettings = new FilteringSettings(RenderQueueRange.all);
        //1、绘制不透明物体
        context.DrawRenderers(cullingResults, ref drawingSettings, ref filteringSettings);
        //2、绘制天空盒
        context.DrawSkybox(camera);

        sortingSettings.criteria = SortingCriteria.CommonTransparent;
        drawingSettings.sortingSettings = sortingSettings;
        drawingSettings.SetShaderPassName(1, litShaderTagId);
        filteringSettings.renderQueueRange = RenderQueueRange.transparent;
        //3、绘制透明物体
        context.DrawRenderers(cullingResults, ref drawingSettings, ref filteringSettings);
    }

    void Submit()
    {
        buffer.EndSample(sampleName);
        ExcuteBuffer();
        context.Submit();
    }

    void ExcuteBuffer()
    {
        context.ExecuteCommandBuffer(buffer);
        buffer.Clear();
    }

    CommandBuffer buffer = new CommandBuffer
    {
        name = bufferName
    };

    bool Cull()
    {
        ScriptableCullingParameters p;
        if(camera.TryGetCullingParameters(out p))
        {
            cullingResults = context.Cull(ref p);
            return true;
        }
        return false;
    }


    /// <summary>
    /// 绘制SRP不支持的着色器类型
    /// </summary>
    partial void DrawUnsupportedShaders();

    partial void DrawGizmos();
    partial void PrepareForSceneWindow();
    partial void PrepareBuffer();
}
