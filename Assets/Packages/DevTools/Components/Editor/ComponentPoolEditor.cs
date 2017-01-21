// =====================================================================
// Copyright 2013-2015 Fluffy Underware
// All rights reserved
// 
// http://www.fluffyunderware.com
// =====================================================================

using UnityEngine;
using UnityEditor;
using FluffyUnderware.DevTools;

namespace FluffyUnderware.DevToolsEditor
{
    [CustomEditor(typeof(ComponentPool))]
    public class ComponentPoolEditor : DTEditor<ComponentPool>
    {
        protected override void OnCustomInspectorGUIBefore()
        {
            PoolManagerEditor.showBar(Target);
        }
    }
}
