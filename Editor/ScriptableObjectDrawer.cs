using System.Collections.Generic;
using DreadZitoTools.ScriptableLovers;
using UnityEditor;
using UnityEngine;

namespace ScriptableLovers.Editor
{
    [CustomPropertyDrawer(typeof(ScriptableObject), true)]
    public class ScriptableObjectDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            // Dibujar etiqueta
            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

            // Reservar espacio para el Object Field y el botón
            Rect objectFieldRect = new Rect(position.x, position.y, position.width - 50, position.height);
            Rect buttonRect = new Rect(position.x + position.width - 50, position.y, 50, position.height);

            // Dibujar el campo de objeto
            EditorGUI.ObjectField(objectFieldRect, property, GUIContent.none);

            // Obtener el tipo del campo desde la propiedad serializada
            System.Type fieldType = GetFieldType();

            if (property.objectReferenceValue == null)
            {
                // Mostrar el botón "New" si el campo está vacío
                if (GUI.Button(buttonRect, "New"))
                {
                    CreateNewScriptableObject(property, fieldType);
                }
            }

            EditorGUI.EndProperty();
        }

        private System.Type GetFieldType()
        {
            // Si el tipo del campo es una lista o array, obtener el tipo genérico o el tipo del elemento
            System.Type fieldType = fieldInfo.FieldType;

            if (fieldType.IsArray)
            {
                return fieldType.GetElementType();
            }
            else if (fieldType.IsGenericType && fieldType.GetGenericTypeDefinition() == typeof(List<>))
            {
                return fieldType.GetGenericArguments()[0];
            }

            // Retornar el tipo base si no es un array ni una lista
            return fieldType;
        }

        private void CreateNewScriptableObject(SerializedProperty property, System.Type type)
        {
            string path = null;

            // Intentar obtener el atributo ScriptableObjectPath
            ScriptableObjectPathAttribute pathAttribute = (ScriptableObjectPathAttribute)System.Attribute.GetCustomAttribute(type, typeof(ScriptableObjectPathAttribute));

            if (pathAttribute != null)
            {
                path = pathAttribute.Path;

                Debug.Log($"Attribute path: {path}");
                if (!AssetDatabase.IsValidFolder(path))
                {
                    //Debug.LogWarning($"This path isn't valid: {path}, opening selecting window");
                    path = null; // Limpiar la ruta para abrir la ventana de selección
                }
            }

            var assetName = $"New {type.Name}.asset";
            // Si no se definió un path válido, mostrar un panel para elegir el lugar de guardado
            if (string.IsNullOrEmpty(path))
            {
                string savePath = EditorUtility.SaveFilePanelInProject(
                    "CHOOSE LOCATION (AVOID THIS BY ADDING ScriptableObjectPath Attribute TO THE SO CLASS)",
                    assetName,
                    "asset",
                    "Select the location to save the new ScriptableObject"
                );

                if (string.IsNullOrEmpty(savePath))
                {
                    Debug.LogWarning("No location was selected for the new ScriptableObject.");
                    return; // Si el usuario cancela, salimos del método
                }

                path = System.IO.Path.GetDirectoryName(savePath);
                assetName = System.IO.Path.GetFileName(savePath);
            }

            // Generar una ruta de archivo única
            string assetPath = AssetDatabase.GenerateUniqueAssetPath($"{path}/{assetName}");

            // Crear el nuevo ScriptableObject
            ScriptableObject newAsset = ScriptableObject.CreateInstance(type);
            AssetDatabase.CreateAsset(newAsset, assetPath);
            AssetDatabase.SaveAssets();

            // Asignar el nuevo objeto al campo
            property.objectReferenceValue = newAsset;
            property.serializedObject.ApplyModifiedProperties();

            // Resaltar el nuevo archivo en el Project
            EditorGUIUtility.PingObject(newAsset);
        }
    }
}