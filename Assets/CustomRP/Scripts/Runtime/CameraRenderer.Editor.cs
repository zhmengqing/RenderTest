using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Rendering;

public partial class CameraRenderer
{
#if UNITY_EDITOR
    static ShaderTagId[] legacyShaderTagIds =
    {
        new ShaderTagId("Always"),
        new ShaderTagId("ForwardBase"),
        new ShaderTagId("PrepassBase"),
        new ShaderTagId("Vertex"),
        new ShaderTagId("VertexLMRGBM"),
        new ShaderTagId("VertexLM")
    };
    //绘制成使用错误材质的粉红色
    static Material errorMat;
    string sampleName { get; set; }
    /// <summary>
    /// 绘制SRP不支持的着色器类型
    /// </summary>
    partial void DrawUnsupportedShaders()
    {
        if (errorMat == null)
        {
            errorMat = new Material(Shader.Find("Hidden/InternalErrorShader"));
        }

        //数组第一个元素用来构造DrawingSettings对象的时候设置
        var drawingSettings = new DrawingSettings(legacyShaderTagIds[0], new SortingSettings(camera))
        {
            overrideMaterial = errorMat
        };
        for (int i = 1; i < legacyShaderTagIds.Length; i++)
        {
            drawingSettings.SetShaderPassName(i, legacyShaderTagIds[i]);
        }
        //使用默认设置即可，反正画出来的都是不支持的
        var filteringSettings = FilteringSettings.defaultValue;
        //绘制不支持的ShaderTag类型的物体
        context.DrawRenderers(cullingResults, ref drawingSettings, ref filteringSettings);
    }

    partial void DrawGizmos()
    {
        if (Handles.ShouldRenderGizmos())
        {
            context.DrawGizmos(camera, GizmoSubset.PreImageEffects);
            context.DrawGizmos(camera, GizmoSubset.PostImageEffects);
        }
    }

    partial void PrepareForSceneWindow()
    {
        if (camera.cameraType == CameraType.SceneView) 
        {
            ScriptableRenderContext.EmitWorldGeometryForSceneView(camera);
        }
    }
    partial void PrepareBuffer()
    {
        Profiler.BeginSample("Editor Only");
        buffer.name = sampleName = camera.name;
        Profiler.EndSample();
    }
#endif
}
