using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace TF.AssetsCreator
{

    public class AssetsCreatorWindow : EditorWindow
    {
        [SerializeField] public string targetFolder;
        [SerializeField] private HashSet<Type> _scriptablesTypes;
        [SerializeField] private Vector2 _position;

        private ScriptableObject previewObject;
        private Editor previewObjectEditor;


        [MenuItem("Assets/Create Scriptable Object...", priority = -10000)]
        private static void ShowDialog()
        {
            var path = "Assets";
            var obj = Selection.activeObject;
            if (obj && AssetDatabase.Contains(obj))
            {
                path = AssetDatabase.GetAssetPath(obj);
                if (!Directory.Exists(path))
                {
                    path = Path.GetDirectoryName(path);
                }
            }

            var window = CreateInstance<AssetsCreatorWindow>();
            window.ShowUtility();
            window.titleContent = new GUIContent(path);
            window.targetFolder = path;
        }

        private void OnEnable()
        {
            SetScriptables();
        }

        private void SetScriptables()
        {
            _scriptablesTypes = AssemblyUtilities.GetUserScriptableObjectTypes();
        }

        private void OnGUI()
        {
            if (_scriptablesTypes == null)
                SetScriptables();


            GUILayout.BeginHorizontal();
            {
                _position = GUILayout.BeginScrollView(_position, GUILayout.Width(300));
                {
                    EditorGUILayout.LabelField("Select a scriptable object", EditorStyles.boldLabel);

                    foreach (var type in _scriptablesTypes)
                    {
                        string name = ObjectNames.NicifyVariableName(type.Name);

                        if (GUILayout.Button(name))
                        {
                            previewObject = CreateInstance(type);
                            previewObject.name = name;
                            previewObjectEditor = Editor.CreateEditor(previewObject);
                            //CreateAsset(type, type.Name);
                        }
                    }
                }
                GUILayout.EndScrollView();

                GUILayout.BeginVertical();
                {

                    if (previewObject)
                    {
                        previewObjectEditor.OnInspectorGUI();

                        GUILayout.FlexibleSpace();
                        if (GUILayout.Button("Create Asset", GUILayout.Height(30)))
                        {
                            CreateAsset();
                        }
                    }
                }
                GUILayout.EndVertical();
            }
            GUILayout.EndHorizontal();
        }

        private void CreateAsset()
        {
            if (!previewObject) return;

            var dest = targetFolder + "/" + previewObject.name + ".asset";
            dest = AssetDatabase.GenerateUniqueAssetPath(dest);
            ProjectWindowUtil.CreateAsset(previewObject, dest);
            EditorApplication.delayCall += this.Close;
        }
    }
}