using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;


namespace CustomSerialization
{
    [CustomPropertyDrawer(typeof(InterfaceReference<>))]
    [CustomPropertyDrawer(typeof(InterfaceReference<,>))]
    public class InterfaceReferenceDrawer : PropertyDrawer
    {
        const string UnderlyingValueFieldName = "underlyingValue";

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var underlyingProperty = property.FindPropertyRelative(UnderlyingValueFieldName);
            var args = GetArguments(fieldInfo);

            if (args.ObjectType == null || args.InterfaceType == null)
            {
                Debug.LogWarning($"Invalid InterfaceReference field '{property.name}'. Ensure it is of a supported type.");
                EditorGUI.ObjectField(position, label, null, typeof(UnityEngine.Object), true);
                return;
            }

            EditorGUI.BeginProperty(position, label, property);
            var assignedObject = EditorGUI.ObjectField(position, label, underlyingProperty.objectReferenceValue, typeof(Object), true);

            if (assignedObject != null)
            {
                if (assignedObject is GameObject gameObject)
                {
                    ValidateAndAssignObject(underlyingProperty, gameObject.GetComponent(args.InterfaceType), gameObject.name, args.InterfaceType.Name);
                }
                else
                {
                    ValidateAndAssignObject(underlyingProperty, assignedObject, args.InterfaceType.Name);
                }
            }
            else
            {
                underlyingProperty.objectReferenceValue = null;
            }
            EditorGUI.EndProperty();
            InterfaceReferenceUtil.OnGUI(position, underlyingProperty, label, args);
        }

        static InterfaceArgs GetArguments(FieldInfo fieldInfo)
        {
            Type fieldType = fieldInfo.FieldType;

            bool TryGetTypesFromInterfaceReference(Type type, out Type objType, out Type intfType)
            {
                objType = intfType = null;

                if (type?.IsGenericType != true) return false;

                var genericType = type.GetGenericTypeDefinition();
                if (genericType == typeof(InterfaceReference<>)) type = type.BaseType;

                if (type?.GetGenericTypeDefinition() == typeof(InterfaceReference<,>))
                {
                    var types = type.GetGenericArguments();
                    intfType = types[0];
                    objType = types[1];
                    return true;
                }

                return false;
            }

            void GetTypesFromList(Type type, out Type objType, out Type intfType)
            {
                objType = intfType = null;

                var listInterface = type.GetInterfaces().FirstOrDefault(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IList<>));

                if (listInterface != null)
                {
                    var elementType = listInterface.GetGenericArguments()[0];
                    TryGetTypesFromInterfaceReference(elementType, out objType, out intfType);
                }
            }

            if (!TryGetTypesFromInterfaceReference(fieldType, out Type objectType, out Type interfaceType))
            {
                GetTypesFromList(fieldType, out objectType, out interfaceType);
            }

            return new InterfaceArgs(objectType, interfaceType);
        }


        /*static InterfaceArgs GetArguments(FieldInfo fieldInfo)
        {
            Type objectType = null, interfaceType = null;
            Type fieldType = fieldInfo.FieldType;

            bool TryGetTypesFromInterfaceReference(Type type, out Type objType, out Type intfType)
            {
                objType = intfType = null;

                if (type?.IsGenericType != true) return false;

                var genericType = type.GetGenericTypeDefinition();
                if (genericType == typeof(InterfaceReference<>)) type = type.BaseType;

                if (type?.GetGenericTypeDefinition() == typeof(InterfaceReference<,>))
                {
                    var types = type.GetGenericArguments();
                    intfType = types[0];
                    objType = types[1];
                    return true;
                }

                return false;
            }

            void GetTypesFromList(Type type, out Type objType, out Type intfType)
            {
                objType = intfType = null;

                var listInterface = type.GetInterfaces().FirstOrDefault(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IList<>));
                
                if (listInterface != null)
                {
                    var elementType = listInterface.GetGenericArguments()[0];
                    TryGetTypesFromInterfaceReference(elementType, out objType, out intfType);
                }
            }

            if (!TryGetTypesFromInterfaceReference(fieldType, out objectType, out interfaceType))
            {
                GetTypesFromList(fieldType, out objectType, out interfaceType);
            }

            return new InterfaceArgs(objectType, interfaceType);
        }*/

        static void ValidateAndAssignObject(SerializedProperty property, Object targetObject, string componentNameOrType, string interfaceName = null)
        {
            if (targetObject == null)
            {
                Debug.LogWarning($"No object was assigned.");
                property.objectReferenceValue = null;
                return;
            }

            if (interfaceName != null && !(targetObject is GameObject gameObject && gameObject.GetComponent(interfaceName) != null))
            {
                Debug.LogWarning(
                    $"The assigned object ({componentNameOrType}) does not implement the required interface '{interfaceName}'."
                );
                property.objectReferenceValue = null;
                return;
            }

            property.objectReferenceValue = targetObject;
        }

        /*static void ValidateAndAssignObject(SerializedProperty property, Object targetObject, string componentNameOrType, string interfaceName = null)
        {
            if (targetObject != null)
            {
                property.objectReferenceValue = targetObject;
            }
            else
            {
                // Verbaetum string technique
                Debug.LogWarning(
                    @$"The {(interfaceName != null ?
                        $"GameObject '{componentNameOrType}'"
                        : $"assigned object")} does not have a component that implements '{componentNameOrType}.'"
                );
                property.objectReferenceValue = null;
            }
        }*/

    }
}
