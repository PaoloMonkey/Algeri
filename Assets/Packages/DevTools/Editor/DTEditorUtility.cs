// =====================================================================
// Copyright 2013-2015 Fluffy Underware
// All rights reserved
// 
// http://www.fluffyunderware.com
// =====================================================================

using UnityEngine;
using UnityEditor;
using System.Reflection;

namespace FluffyUnderware.DevToolsEditor
{
    public static class DTEditorUtility
    {
        public static Camera ActiveCamera
        {
            get
            {
                return (SceneView.currentDrawingSceneView) ? SceneView.currentDrawingSceneView.camera : Camera.current;
            }
        }


       
    }
}
