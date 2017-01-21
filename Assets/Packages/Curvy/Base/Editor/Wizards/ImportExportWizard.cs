// =====================================================================
// Copyright 2013-2016 Fluffy Underware
// All rights reserved
// 
// http://www.fluffyunderware.com
// =====================================================================

using UnityEngine;
using System.Collections;
using UnityEditor;
using FluffyUnderware.DevToolsEditor;
using FluffyUnderware.Curvy;
using FluffyUnderware.Curvy.Utils;
using FluffyUnderware.DevTools.Extensions;
using System.Linq;

namespace FluffyUnderware.CurvyEditor
{

    public class ImportExportWizard : EditorWindow
    {

        string txtSerialized = string.Empty;
        string txtSerializedType = string.Empty;
        string txtFile = string.Empty;
        CurvySerializationSpace Space = CurvySerializationSpace.WorldSpline;
        
        Vector2 scroll;
        bool mNeedRepaint;
        bool mFormat = true;

        IDTInspectorNodeRenderer GUIRenderer = new DTInspectorNodeDefaultRenderer();
        DTGroupNode nSerializedText = new DTGroupNode("Serialized Text");
        DTGroupNode nActions = new DTGroupNode("Actions");

        
        bool SplinesSelected
        {
            get { return DTSelection.HasComponent<CurvySpline>(true); }
        }

        bool ControlPointsSelected
        {
            get { return DTSelection.HasComponent<CurvySplineSegment>(true); }
        }

        bool canDeserialize
        {
            get
            {
                switch (txtSerializedType)
                {
                    case "SerializedCurvySplineCollection":
                        return !DTSelection.HasComponent<CurvySplineSegment>(true);
                    case "SerializedCurvySplineSegmentCollection":
                        return DTSelection.HasComponent<CurvySpline>() || DTSelection.HasComponent<CurvySplineSegment>();
                    default:
                        return false;
                }
            }
        }

        public static void Open()
        {
            var win = EditorWindow.GetWindow<ImportExportWizard>(true, "Import/Export");
            win.minSize = new Vector2(500, 340);
        }

        void OnEnable()
        {
            DTSelection.OnSelectionChange += DTSelection_OnSelectionChange;
        }

        void OnDisable()
        {
            DTSelection.OnSelectionChange -= DTSelection_OnSelectionChange;
        }

         void DTSelection_OnSelectionChange()
        {
            Repaint();
        }

        void OnGUI()
        {
            DTInspectorNode.IsInsideInspector = false;
            // Actions
            GUIRenderer.RenderSectionHeader(nActions);
            if (nActions.ContentVisible)
            {
                GUILayout.BeginHorizontal();
                GUI.enabled = ControlPointsSelected || SplinesSelected;
                if (GUILayout.Button("Serialize", GUILayout.Width(160)))
                {
                    serialize();
                    GUI.FocusControl(null);
                }
                GUI.enabled = true;
                GUILayout.FlexibleSpace();
                float lw = EditorGUIUtility.labelWidth;
                EditorGUIUtility.labelWidth = 50;
                Space=(CurvySerializationSpace)EditorGUILayout.EnumPopup("Space",Space,GUILayout.Width(150));
                EditorGUIUtility.labelWidth = lw;
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUI.enabled = canDeserialize;
                if (GUILayout.Button("Deserialize",GUILayout.Width(160)))
                    deserialize();
                
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUI.enabled = !string.IsNullOrEmpty(txtFile);
                if (GUILayout.Button("Load File",GUILayout.Width(160)))
                {
                    if (System.IO.File.Exists(txtFile))
                        setText(System.IO.File.ReadAllText(txtFile).Replace("\n", ""));
                }
                txtFile = EditorGUILayout.TextField(txtFile);
                GUI.enabled = true;
                if (GUILayout.Button(new GUIContent("...","Select file"),GUILayout.ExpandWidth(false)))
                {
                    txtFile=EditorUtility.OpenFilePanel("Select file to load", Application.dataPath, "");
                }
                GUILayout.EndHorizontal();
                if (GUILayout.Button("Save File", GUILayout.Width(160))) {
                    var file = EditorUtility.SaveFilePanel("Save to...", Application.dataPath, txtSerializedType + ".txt", "txt");
                    if (!string.IsNullOrEmpty(file))
                    {
                        System.IO.File.WriteAllText(file, txtSerialized);
                        AssetDatabase.Refresh();
                    }
                }

            }
            GUIRenderer.RenderSectionFooter(nActions);
            mNeedRepaint = mNeedRepaint || nActions.NeedRepaint;
            // Text
            GUIRenderer.RenderSectionHeader(nSerializedText);
            if (nSerializedText.ContentVisible)
            {
                GUILayout.BeginHorizontal();
                mFormat = EditorGUILayout.Toggle("Formatted Output", mFormat);
                GUILayout.FlexibleSpace();
                GUILayout.Label(txtSerializedType);
                GUILayout.EndHorizontal();
                scroll = EditorGUILayout.BeginScrollView(scroll,GUILayout.MaxHeight(position.height - 100));
                EditorGUI.BeginChangeCheck();
                txtSerialized = EditorGUILayout.TextArea(txtSerialized,EditorStyles.textArea);
                if (EditorGUI.EndChangeCheck())
                    getType();
                EditorGUILayout.EndScrollView();
            }
            GUIRenderer.RenderSectionFooter(nSerializedText);
            GUILayout.Space(5);
            mNeedRepaint = mNeedRepaint || nSerializedText.NeedRepaint;
            if (mNeedRepaint)
            {
                Repaint();
                mNeedRepaint = false;
            }
        }

        void serialize()
        {
            if (ControlPointsSelected)
            {
                setText(new SerializedCurvySplineSegmentCollection(DTSelection.GetAllAs<CurvySplineSegment>(),Space).ToJson());
            }
            
            else if (SplinesSelected)
            {
                setText(new SerializedCurvySplineCollection(DTSelection.GetAllAs<CurvySpline>(), Space).ToJson());
            }
        }

        void deserialize()
        {
            switch (txtSerializedType)
            {
                case "SerializedCurvySplineCollection":
                    var sspl = SerializedCurvySplineCollection.FromJson(txtSerialized);
                    var applyTo = DTSelection.GetAllAs<CurvySpline>().ToArray();
                    if (applyTo.Length>0)
                        sspl.Deserialize(applyTo, Space);
                    else
                        sspl.Deserialize(DTSelection.GetAs<Transform>(), Space);
                    break;
                case "SerializedCurvySplineSegmentCollection":
                    var scp = SerializedCurvySplineSegmentCollection.FromJson(txtSerialized);
                    CurvySplineSegment cp = DTSelection.GetAs<CurvySplineSegment>();
                    if (cp)
                        scp.Deserialize(cp, Space);
                     else
                    {
                        CurvySpline spl = DTSelection.GetAs<CurvySpline>();
                        scp.Deserialize(spl, Space);
                    }
                    break;
            }
        }

        

        void setText(string json)
        {
            Debug.Log("Set");
            txtSerialized = (mFormat) ? FormatJson(json) : json;
            GUI.FocusControl(null);
            getType();
        }

        void getType()
        {
            System.Type t = SerializedCurvyObjectHelper.GetJsonSerializedType(txtSerialized.Substring(0,70));
            txtSerializedType = (t != null) ? t.Name : "Unsupported type";
        }

        

        private const string INDENT_STRING = "    ";
        static string FormatJson(string str)
        {
            var indent = 0;
            var quoted = false;
            var sb = new System.Text.StringBuilder();
            for (var i = 0; i < str.Length; i++)
            {
                var ch = str[i];
                switch (ch)
                {
                    case '{':
                    case '[':
                        sb.Append(ch);
                        if (!quoted)
                        {
                            sb.AppendLine();
                            Enumerable.Range(0, ++indent).ForEach(item => sb.Append(INDENT_STRING));
                        }
                        break;
                    case '}':
                    case ']':
                        if (!quoted)
                        {
                            sb.AppendLine();
                            Enumerable.Range(0, --indent).ForEach(item => sb.Append(INDENT_STRING));
                        }
                        sb.Append(ch);
                        break;
                    case '"':
                        sb.Append(ch);
                        bool escaped = false;
                        var index = i;
                        while (index > 0 && str[--index] == '\\')
                            escaped = !escaped;
                        if (!escaped)
                            quoted = !quoted;
                        break;
                    case ',':
                        sb.Append(ch);
                        if (!quoted)
                        {
                            sb.AppendLine();
                            Enumerable.Range(0, indent).ForEach(item => sb.Append(INDENT_STRING));
                        }
                        break;
                    case ':':
                        sb.Append(ch);
                        if (!quoted)
                            sb.Append(" ");
                        break;
                    default:
                        sb.Append(ch);
                        break;
                }
            }
            return sb.ToString();
        }
    }


}

        

   
