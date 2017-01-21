using UnityEngine;
using UnityEditor;
using System;
using System.Collections;

public class WOAWaterEditor : ShaderGUI {
    
    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        
        // render the default gui
        //base.OnGUI(materialEditor, properties);

        //Material targetMat = materialEditor.target as Material;

        var prop_debugnoise = FindProperty("_WavesShowNoise",properties);
        var prop_debugfoam = FindProperty("_FoamShowNoise", properties);
        var clamping = FindProperty("_Clamping",properties).vectorValue;
        float min,max = 0;

        EditorGUI.BeginChangeCheck();

        var style_title = new GUIStyle(GUI.skin.GetStyle("Label"));
        style_title.fontStyle = FontStyle.Bold;
        style_title.fontSize = 10;
        var style_bold = new GUIStyle(GUI.skin.GetStyle("Label"));
        style_bold.fontStyle = FontStyle.Bold;
        //style_title.fontSize = 13;

        EditorGUILayout.LabelField("Reflection",style_title);
        EditorGUI.indentLevel++;
        materialEditor.TexturePropertySingleLine(new GUIContent("Bump Map"), FindProperty("_BumpMap", properties));
        materialEditor.RangeProperty(FindProperty("_WaveScale",properties),"Bump Map Scale");
        materialEditor.RangeProperty(FindProperty("_ReflDistort",properties),"Bump Distortion");
        materialEditor.RangeProperty(FindProperty("_WavesReflDistort",properties),"Waves Distortion");
        materialEditor.ColorProperty(FindProperty("_ReflectionTint",properties),"Reflection tint");
        /*
        EditorGUILayout.BeginHorizontal();
        var wavespeed = FindProperty("_WavesReflDistort",properties).vectorValue;
        EditorGUILayout.LabelField("Bump speed (1 x,y)");
        wavespeed.x = EditorGUILayout.FloatField("x",wavespeed.x);
        wavespeed.y = EditorGUILayout.FloatField("y",wavespeed.y);
        targetMat.SetVector("_WavesReflDistort",wavespeed);
        EditorGUILayout.EndHorizontal();
        */
        EditorGUI.indentLevel--;

        EditorGUILayout.LabelField("Lighting",style_title);
        EditorGUI.indentLevel++;
        materialEditor.ColorProperty(FindProperty("_SkyTint",properties),"Water Color");
        materialEditor.RangeProperty(FindProperty("_LightingOpacity",properties),"Global Lighting");
        materialEditor.RangeProperty(FindProperty("_ShadowOpacity",properties),"Shadow Opacity");
        EditorGUI.indentLevel--;



        EditorGUILayout.LabelField("Wave Noise",style_title);
        EditorGUI.indentLevel++;
        bool wasDebuggingNoise = prop_debugnoise.floatValue == 1;
        prop_debugnoise.floatValue = EditorGUILayout.ToggleLeft(" Visualize", prop_debugnoise.floatValue == 1, style_bold) ? 1 : 0;
        if (!wasDebuggingNoise && prop_debugnoise.floatValue == 1) prop_debugfoam.floatValue = 0;

        var noise_visibility = FindProperty("_WavesVisibility",properties).vectorValue;
        noise_visibility.x = EditorGUILayout.FloatField("Blending", noise_visibility.x);
        min = noise_visibility.y;
        max = noise_visibility.z;
        EditorGUILayout.MinMaxSlider(new GUIContent("Range"), ref min, ref max,0,1);
        noise_visibility.y = min;
        noise_visibility.z = max;

        min = clamping.x;
        max = clamping.y;
        EditorGUILayout.MinMaxSlider(new GUIContent("Clamp"), ref min, ref max,0,1);
        clamping.x = min;
        clamping.y = max;

        noise_visibility.w = (float)EditorGUILayout.IntSlider("Resolution",(int)noise_visibility.w,1,8);
        FindProperty("_WavesVisibility",properties).vectorValue = noise_visibility;

        var noise_scale = FindProperty("_WavesScale",properties).vectorValue;
        noise_scale.x = EditorGUILayout.FloatField("Zoom",noise_scale.x);
        noise_scale.y = EditorGUILayout.Slider("Dual noise scale", noise_scale.y, 0, 3);
        FindProperty("_WavesScale",properties).vectorValue = noise_scale;

        var noise_speed = FindProperty("_WavesSpeed",properties).vectorValue;
        noise_speed.x = EditorGUILayout.FloatField("Speed (Horizontal)",noise_speed.x);
        noise_speed.y = EditorGUILayout.FloatField("Speed (Vertical)",noise_speed.y);
        noise_speed.z = EditorGUILayout.FloatField("Noise Movement",noise_speed.z);
        FindProperty("_WavesSpeed",properties).vectorValue = noise_speed;



        materialEditor.ColorProperty(FindProperty("_WavesColor1",properties),"Color (from)");
        materialEditor.ColorProperty(FindProperty("_WavesColor2",properties),"Color (to)");
        EditorGUI.indentLevel--;

        EditorGUILayout.LabelField("Foam",style_title);
        EditorGUI.indentLevel++;
        bool wasDebuggingFoam = prop_debugfoam.floatValue == 1;
        prop_debugfoam.floatValue = EditorGUILayout.ToggleLeft(" Visualize", prop_debugfoam.floatValue == 1, style_bold) ? 1 : 0;
        if (!wasDebuggingFoam && prop_debugfoam.floatValue == 1) prop_debugnoise.floatValue = 0;

        var foam_visibility = FindProperty("_FoamVisibility",properties).vectorValue;
        foam_visibility.x = EditorGUILayout.FloatField("Blending", foam_visibility.x);
        min = foam_visibility.y;
        max = foam_visibility.z;
        EditorGUILayout.MinMaxSlider(new GUIContent("Range"), ref min, ref max,0,1);
        foam_visibility.y = min;
        foam_visibility.z = max;

        min = clamping.z;
        max = clamping.w;
        EditorGUILayout.MinMaxSlider(new GUIContent("Clamp"), ref min, ref max,0,1);
        clamping.z = min;
        clamping.w = max;

        foam_visibility.w = (float)EditorGUILayout.IntSlider("Resolution",(int)foam_visibility.w,1,8);

        FindProperty("_FoamVisibility",properties).vectorValue = foam_visibility;

        materialEditor.RangeProperty(FindProperty("_FoamScale",properties),"Scale");

        materialEditor.ColorProperty(FindProperty("_FoamTint",properties),"Color");
        EditorGUI.indentLevel--;

        FindProperty("_Clamping",properties).vectorValue = clamping;

        if (EditorGUI.EndChangeCheck())
        {
            
        }
    }
}
