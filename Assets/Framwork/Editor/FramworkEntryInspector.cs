using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.Reflection;
using UnityEditorInternal;

namespace Framwork
{
    [CustomEditor(typeof(FramworkEntry))]
    public class FramworkEntryInspector : Editor
    {
        FramworkEntry framworkEntry;
        SerializedProperty useFguiProp;
        SerializedProperty loadDataTableProp;
        SerializedProperty loadJsonDataProp;
        SerializedProperty injectLocalDataProp;
        SerializedProperty dataTableIsNullProp;
        SerializedProperty jsonDataIsNullProp;
        SerializedProperty localDataIsNullProp;
        SerializedProperty ignoreDataTableTypeNameProp;
        SerializedProperty ignoreJsonDataTypeNameProp;
        SerializedProperty ignoreLocalDataTypeNameProp;
        SerializedProperty fguiConfigLoadCallbackProp;
        SerializedProperty mainUITypeFullNameProp;
        SerializedProperty mainUIShowCallbackProp;
        SerializedProperty loadSingleDataTableCallbackProp;
        SerializedProperty loadAllDataTableCallbackProp;
        SerializedProperty loadSingleJsonDataCallbackProp;
        SerializedProperty loadAllJsonDataCallbackProp;
        SerializedProperty injectSingleLocalDataCallbackProp;
        SerializedProperty injectAllLocalDataCallbackProp;
        SerializedProperty framworkReadyCallbackProp;
        SerializedProperty runTimeSequenceProp;
        ReorderableList runTimeSequenceList;
        string[] singleFguiTypeName;
        string[] singleFguiFullTypeName;
        string[] dataTableTypeNames;
        string[] dataTableNames;
        string[] jsonDataTypeNames;
        string[] jsonDataNames;
        string[] localDataTypeNames;
        Type[] localDataTypes;
        bool[] unfoldLocalDataView;
        Dictionary<string, ReorderableList> listDic;
        Dictionary<string, bool> viewDic;
        Dictionary<string, object> objCacheDic;
        Dictionary<string, float> heightDic;

        string[] layerMaskNames;
        GUIContent toggleContent;
        GUIContent toggleOnContent;
        GUIContent boxContent;
        GUIContent emptyBoxContent;
        GUIContent foldContent;
        GUIContent unfoldContent;
        GUIContent addContent;
        GUIContent subContent;
        MethodInfo dynamicArrayAddInfo;
        MethodInfo dynamicArrayRemoveInfo;
        MethodInfo getKeysFromDictionaryInfo;
        MethodInfo getValuesFromDictionaryInfo;
        MethodInfo checkDictionaryKeyInfo;
        MethodInfo addDictionaryValueInfo;
        MethodInfo removeKeyFromDictionaryInfo;
        MethodInfo updateDictionaryKeyInfo;
        MethodInfo updateDictionaryValueInfo;

        private void OnEnable()
        {
            framworkEntry = (FramworkEntry)target;
            useFguiProp = serializedObject.FindProperty("useFgui");
            loadDataTableProp = serializedObject.FindProperty("loadDataTable");
            loadJsonDataProp = serializedObject.FindProperty("loadJsonData");
            injectLocalDataProp = serializedObject.FindProperty("injectLocalData");
            dataTableIsNullProp = serializedObject.FindProperty("dataTableIsNull");
            jsonDataIsNullProp = serializedObject.FindProperty("jsonDataIsNull");
            localDataIsNullProp = serializedObject.FindProperty("localDataIsNull");
            ignoreDataTableTypeNameProp = serializedObject.FindProperty("ignoreDataTableTypeName");
            ignoreJsonDataTypeNameProp = serializedObject.FindProperty("ignoreJsonDataTypeName");
            ignoreLocalDataTypeNameProp = serializedObject.FindProperty("ignoreLocalDataTypeName");
            fguiConfigLoadCallbackProp = serializedObject.FindProperty("fguiConfigLoadCallback");
            mainUITypeFullNameProp = serializedObject.FindProperty("mainUITypeFullName");
            mainUIShowCallbackProp = serializedObject.FindProperty("mainUIShowCallback");
            loadSingleDataTableCallbackProp = serializedObject.FindProperty("loadSingleDataTableCallback");
            loadAllDataTableCallbackProp = serializedObject.FindProperty("loadAllDataTableCallback");
            loadSingleJsonDataCallbackProp = serializedObject.FindProperty("loadSingleJsonDataCallback");
            loadAllJsonDataCallbackProp = serializedObject.FindProperty("loadAllJsonDataCallback");
            injectSingleLocalDataCallbackProp = serializedObject.FindProperty("injectSingleLocalDataCallback");
            injectAllLocalDataCallbackProp = serializedObject.FindProperty("injectAllLocalDataCallback");
            framworkReadyCallbackProp = serializedObject.FindProperty("framworkReadyCallback");
            runTimeSequenceProp = serializedObject.FindProperty("runTimeSequence");
            toggleContent = EditorGUIUtility.IconContent("ol toggle act");
            toggleOnContent = EditorGUIUtility.IconContent("ol toggle on act");
            boxContent = EditorGUIUtility.IconContent("white");
            emptyBoxContent = EditorGUIUtility.IconContent("toolbar back");
            foldContent = EditorGUIUtility.IconContent("IN foldout");
            unfoldContent = EditorGUIUtility.IconContent("IN foldout on");
            addContent = EditorGUIUtility.IconContent("d_Toolbar Plus");
            subContent = EditorGUIUtility.IconContent("d_Toolbar Minus");
            List<string> layerNames = new List<string>();
            for (int i = 0; i < 32; i++)
            {
                string name = LayerMask.LayerToName(i);
                if (!string.IsNullOrEmpty(name))
                    layerNames.Add(name);
            }
            layerMaskNames = layerNames.ToArray();
            listDic = new Dictionary<string, ReorderableList>();
            viewDic = new Dictionary<string, bool>();
            objCacheDic = new Dictionary<string, object>();
            heightDic = new Dictionary<string, float>();

            Type type = GetType();
            dynamicArrayAddInfo = type.GetMethod("dynamicArrayAdd", BindingFlags.NonPublic | BindingFlags.Static);
            dynamicArrayRemoveInfo = type.GetMethod("dynamicArrayRemove", BindingFlags.NonPublic | BindingFlags.Static);
            getKeysFromDictionaryInfo = type.GetMethod("getKeysFromDictionary", BindingFlags.NonPublic | BindingFlags.Static);
            getValuesFromDictionaryInfo = type.GetMethod("getValuesFromDictionary", BindingFlags.NonPublic | BindingFlags.Static);
            updateDictionaryKeyInfo = type.GetMethod("updateDictionaryKey", BindingFlags.NonPublic | BindingFlags.Static);
            updateDictionaryValueInfo = type.GetMethod("updateDictionaryValue", BindingFlags.NonPublic | BindingFlags.Static);
            checkDictionaryKeyInfo = type.GetMethod("checkDictionaryKey", BindingFlags.NonPublic | BindingFlags.Static);
            addDictionaryValueInfo = type.GetMethod("addDictionaryValue", BindingFlags.NonPublic | BindingFlags.Static);
            removeKeyFromDictionaryInfo = type.GetMethod("removeKeyFromDictionary", BindingFlags.NonPublic | BindingFlags.Static);

            serializedObject.Update();
            Type[] singleFguiTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(x => x.GetTypes().Where(y => typeof(SingleFgui).IsAssignableFrom(y) && y.IsClass && !y.IsAbstract)).ToArray();
            singleFguiTypeName = new string[singleFguiTypes.Length + 1];
            singleFguiFullTypeName = new string[singleFguiTypes.Length + 1];
            singleFguiTypeName[0] = "None";
            for (int i = 0; i < singleFguiTypes.Length; i++)
            {
                Type singleFguiType = singleFguiTypes[i];
                singleFguiTypeName[i + 1] = singleFguiType.Name;
                singleFguiFullTypeName[i + 1] = singleFguiType.FullName;
            }

            Type[] dataTableTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(x => x.GetTypes().Where(y => typeof(DataTableUtility).IsAssignableFrom(y) && y.IsClass && !y.IsAbstract)).ToArray();
            dataTableTypeNames = new string[dataTableTypes.Length];
            dataTableNames = new string[dataTableTypes.Length];
            for (int i = 0; i < dataTableTypes.Length; i++)
            {
                Type item = dataTableTypes[i];
                DataTableUtility dataTable = (DataTableUtility)Activator.CreateInstance(item);
                string assetType = item.GetProperty("AssetType", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(dataTable).ToString();
                dataTableTypeNames[i] = dataTableTypes[i].Name;
                dataTableNames[i] = $"{dataTable.TableAssetName}  [{assetType}]";
            }
            dataTableIsNullProp.boolValue = dataTableTypes.Length == 0;
            for (int i = 0; i < ignoreDataTableTypeNameProp.arraySize; i++)
            {
                string select = ignoreDataTableTypeNameProp.GetArrayElementAtIndex(i).stringValue;
                if (!dataTableTypeNames.Contains(select))
                {
                    ignoreDataTableTypeNameProp.DeleteArrayElementAtIndex(i);
                    i--;
                }
            }

            Type[] jsonDataTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(x => x.GetTypes().Where(y => typeof(JsonDataUntility).IsAssignableFrom(y) && y.IsClass && !y.IsAbstract)).ToArray();
            jsonDataTypeNames = new string[jsonDataTypes.Length];
            jsonDataNames = new string[jsonDataTypes.Length];
            for (int i = 0; i < jsonDataTypes.Length; i++)
            {
                Type item = jsonDataTypes[i];
                JsonDataUntility jsonData = (JsonDataUntility)Activator.CreateInstance(item);
                string assetType = item.GetProperty("AssetType", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(jsonData).ToString();
                jsonDataTypeNames[i] = jsonDataTypes[i].Name;
                jsonDataNames[i] = $"{jsonData.JsonAssetName}  [{assetType}]";
            }
            jsonDataIsNullProp.boolValue = jsonDataTypes.Length == 0;
            for (int i = 0; i < ignoreJsonDataTypeNameProp.arraySize; i++)
            {
                string select = ignoreJsonDataTypeNameProp.GetArrayElementAtIndex(i).stringValue;
                if (!jsonDataTypeNames.Contains(select))
                {
                    ignoreJsonDataTypeNameProp.DeleteArrayElementAtIndex(i);
                    i--;
                }
            }

            localDataTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(x => x.GetTypes().Where(y => typeof(LocalSaveUtility).IsAssignableFrom(y) && y.IsClass && !y.IsAbstract)).ToArray();
            localDataTypeNames = new string[localDataTypes.Length];
            unfoldLocalDataView = new bool[localDataTypes.Length];
            for (int i = 0; i < localDataTypes.Length; i++)
            {
                LocalSaveUtility dataTable = (LocalSaveUtility)Activator.CreateInstance(localDataTypes[i]);
                localDataTypeNames[i] = localDataTypes[i].Name;
            }
            localDataIsNullProp.boolValue = localDataTypes.Length == 0;
            for (int i = 0; i < ignoreLocalDataTypeNameProp.arraySize; i++)
            {
                string select = ignoreLocalDataTypeNameProp.GetArrayElementAtIndex(i).stringValue;
                if (!localDataTypeNames.Contains(select))
                {
                    ignoreLocalDataTypeNameProp.DeleteArrayElementAtIndex(i);
                    i--;
                }
            }
            serializedObject.ApplyModifiedProperties();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();

            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(20);
            EditorGUILayout.LabelField("Fgui configuration:", GUILayout.Width(120));
            FguiConfiguration configuration = (FguiConfiguration)EditorGUILayout.ObjectField(framworkEntry.FguiConfiguration, typeof(FguiConfiguration), false);
            EditorGUILayout.EndHorizontal();

            GUIStyle labelStyle = GUI.skin.GetStyle("MiniBoldLabel");
            if (configuration != null)
            {
                EditorGUILayout.Space();
                fguiDrawer(labelStyle, configuration);
            }
            EditorGUILayout.Space();
            dataTableDrawer(labelStyle);
            EditorGUILayout.Space();
            jsonDataDrawer(labelStyle);
            EditorGUILayout.Space();
            localDataDrawer(labelStyle);
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            GUILayout.Label("", "IN Title");
            checkRunTimeSequenceList();
            runTimeSequenceList.DoLayoutList();
            EditorGUILayout.PropertyField(framworkReadyCallbackProp);

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(framworkEntry, "FramworkEntry Change");
                framworkEntry.FguiConfiguration = configuration;
                LocalSaveUtility.SaveAll();
            }
            serializedObject.ApplyModifiedProperties();
        }

        void fguiDrawer(GUIStyle labelStyle, FguiConfiguration configuration)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(20);
            if (GUILayout.Button(useFguiProp.boolValue ? toggleOnContent : toggleContent, "AboutWIndowLicenseLabel") || GUILayout.Button("Use Fgui:", "IN Title"))
                useFguiProp.boolValue = !useFguiProp.boolValue;
            EditorGUILayout.EndHorizontal();

            if (useFguiProp.boolValue)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(20);
                EditorGUILayout.BeginVertical("ColorPickerSliderBackground");

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("FguiDesignScreenSize:", labelStyle, GUILayout.Width(150));
                EditorGUILayout.LabelField($"{configuration.FguiDesignScreenSize.x}x{configuration.FguiDesignScreenSize.y}", labelStyle);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                GUIContent fontContent = new GUIContent("FguiFontAssetName:", "Font asset path: \"[Resources]\" or \"[Resources]/Fonts\"");
                EditorGUILayout.LabelField(fontContent, labelStyle, GUILayout.Width(150));
                EditorGUILayout.LabelField(configuration.FguiFontAssetName, labelStyle);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                GUIContent assetTypeContent = new GUIContent("FguiAssetType:", "\"Resources\" or \"Addressable\"");
                EditorGUILayout.LabelField(assetTypeContent, labelStyle, GUILayout.Width(150));
                EditorGUILayout.LabelField(configuration.FguiAssetType.ToString(), labelStyle);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("CommonPackName:", labelStyle, GUILayout.Width(150));
                EditorGUILayout.LabelField(configuration.CommonPackName, labelStyle);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("LanguageAssetName:", labelStyle, GUILayout.Width(150));
                EditorGUILayout.LabelField(configuration.LanguageAssetName, labelStyle);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                GUIContent mainUIContent = new GUIContent("MainUI:", "Game enter ui");
                EditorGUILayout.LabelField(mainUIContent, labelStyle, GUILayout.Width(150));
                string selectTypeFullName = mainUITypeFullNameProp.stringValue;
                int index = Array.IndexOf(singleFguiFullTypeName, selectTypeFullName);
                if (index == -1) index = 0;
                index = EditorGUILayout.Popup(index, singleFguiTypeName, GUILayout.MinWidth(150));
                mainUITypeFullNameProp.stringValue = singleFguiFullTypeName[index];
                EditorGUILayout.LabelField(configuration.LanguageAssetName, labelStyle);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(50);
                EditorGUILayout.PropertyField(fguiConfigLoadCallbackProp);
                EditorGUILayout.EndHorizontal();

                if (index > 0)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(50);
                    EditorGUILayout.PropertyField(mainUIShowCallbackProp);
                    EditorGUILayout.EndHorizontal();
                }
            }
        }

        void dataTableDrawer(GUIStyle labelStyle)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(20);
            if (GUILayout.Button(loadDataTableProp.boolValue ? toggleOnContent : toggleContent, "AboutWIndowLicenseLabel") || GUILayout.Button("Load DataTable:", "IN Title"))
                loadDataTableProp.boolValue = !loadDataTableProp.boolValue;
            EditorGUILayout.EndHorizontal();

            if (loadDataTableProp.boolValue)
            {
                if (!dataTableIsNullProp.boolValue)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(20);
                    EditorGUILayout.BeginVertical("ColorPickerSliderBackground");
                    GUIContent typeContent = new GUIContent("", "Class name");
                    GUIContent assetContent = new GUIContent("", "Asset name  [Asset type]");
                    for (int i = 0; i < dataTableTypeNames.Length; i++)
                    {
                        EditorGUILayout.BeginHorizontal();
                        string typeName = dataTableTypeNames[i];
                        typeContent.text = $"{typeName}:";
                        assetContent.text = dataTableNames[i].ToString();
                        int index = -1;
                        for (int j = 0; j < ignoreDataTableTypeNameProp.arraySize; j++)
                        {
                            if (ignoreDataTableTypeNameProp.GetArrayElementAtIndex(j).stringValue == typeName)
                            {
                                index = j;
                                break;
                            }
                        }
                        bool ignore = index > -1;
                        if (GUILayout.Button(ignore ? emptyBoxContent : boxContent, "AboutWIndowLicenseLabel", GUILayout.Width(10)))
                            ignore = !ignore;
                        EditorGUILayout.LabelField(typeContent, labelStyle, GUILayout.Width(150));
                        EditorGUILayout.LabelField(assetContent, labelStyle);
                        if (ignore)
                        {
                            if (index == -1)
                            {
                                ignoreDataTableTypeNameProp.InsertArrayElementAtIndex(ignoreDataTableTypeNameProp.arraySize);
                                ignoreDataTableTypeNameProp.GetArrayElementAtIndex(ignoreDataTableTypeNameProp.arraySize - 1).stringValue = typeName;
                            }
                        }
                        else
                        {
                            if (index > -1)
                                ignoreDataTableTypeNameProp.DeleteArrayElementAtIndex(index);
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(50);
                    EditorGUILayout.PropertyField(loadSingleDataTableCallbackProp);
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(50);
                    EditorGUILayout.PropertyField(loadAllDataTableCallbackProp);
                    EditorGUILayout.EndHorizontal();
                }
                else
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(20);
                    EditorGUILayout.BeginVertical("ColorPickerSliderBackground");
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Null", labelStyle, GUILayout.Width(150));
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndHorizontal();
                }
            }
        }

        void jsonDataDrawer(GUIStyle labelStyle)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(20);
            if (GUILayout.Button(loadJsonDataProp.boolValue ? toggleOnContent : toggleContent, "AboutWIndowLicenseLabel") || GUILayout.Button("Load JsonData:", "IN Title"))
                loadJsonDataProp.boolValue = !loadJsonDataProp.boolValue;
            EditorGUILayout.EndHorizontal();

            if (loadJsonDataProp.boolValue)
            {
                if (!jsonDataIsNullProp.boolValue)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(20);
                    EditorGUILayout.BeginVertical("ColorPickerSliderBackground");
                    GUIContent typeContent = new GUIContent("", "Class name");
                    GUIContent assetContent = new GUIContent("", "Asset name  [Asset type]");
                    for (int i = 0; i < jsonDataTypeNames.Length; i++)
                    {
                        EditorGUILayout.BeginHorizontal();
                        string typeName = jsonDataTypeNames[i];
                        typeContent.text = $"{typeName}:";
                        assetContent.text = jsonDataNames[i].ToString();
                        int index = -1;
                        for (int j = 0; j < ignoreJsonDataTypeNameProp.arraySize; j++)
                        {
                            if (ignoreJsonDataTypeNameProp.GetArrayElementAtIndex(j).stringValue == typeName)
                            {
                                index = j;
                                break;
                            }
                        }
                        bool ignore = index > -1;
                        if (GUILayout.Button(ignore ? emptyBoxContent : boxContent, "AboutWIndowLicenseLabel", GUILayout.Width(10)))
                            ignore = !ignore;
                        EditorGUILayout.LabelField(typeContent, labelStyle, GUILayout.Width(150));
                        EditorGUILayout.LabelField(assetContent, labelStyle);
                        if (ignore)
                        {
                            if (index == -1)
                            {
                                ignoreJsonDataTypeNameProp.InsertArrayElementAtIndex(ignoreJsonDataTypeNameProp.arraySize);
                                ignoreJsonDataTypeNameProp.GetArrayElementAtIndex(ignoreJsonDataTypeNameProp.arraySize - 1).stringValue = typeName;
                            }
                        }
                        else
                        {
                            if (index > -1)
                                ignoreJsonDataTypeNameProp.DeleteArrayElementAtIndex(index);
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(50);
                    EditorGUILayout.PropertyField(loadSingleJsonDataCallbackProp);
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(50);
                    EditorGUILayout.PropertyField(loadAllJsonDataCallbackProp);
                    EditorGUILayout.EndHorizontal();
                }
                else
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(20);
                    EditorGUILayout.BeginVertical("ColorPickerSliderBackground");
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Null", labelStyle, GUILayout.Width(150));
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndHorizontal();
                }
            }
        }

        void localDataDrawer(GUIStyle labelStyle)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(20);
            if (GUILayout.Button(injectLocalDataProp.boolValue ? toggleOnContent : toggleContent, "AboutWIndowLicenseLabel") || GUILayout.Button("Inject LocalData:", "IN Title"))
                injectLocalDataProp.boolValue = !injectLocalDataProp.boolValue;
            EditorGUILayout.EndHorizontal();

            if (injectLocalDataProp.boolValue)
            {
                if (!localDataIsNullProp.boolValue)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(20);
                    EditorGUILayout.BeginVertical("ColorPickerSliderBackground");
                    GUIContent typeContent = new GUIContent("", "Class name");
                    for (int i = 0; i < localDataTypeNames.Length; i++)
                    {
                        EditorGUILayout.BeginHorizontal();
                        string typeName = localDataTypeNames[i];
                        typeContent.text = localDataTypeNames[i].ToString();
                        int index = -1;
                        for (int j = 0; j < ignoreLocalDataTypeNameProp.arraySize; j++)
                        {
                            if (ignoreLocalDataTypeNameProp.GetArrayElementAtIndex(j).stringValue == typeName)
                            {
                                index = j;
                                break;
                            }
                        }
                        bool ignore = index > -1;
                        if (GUILayout.Button(ignore ? emptyBoxContent : boxContent, "AboutWIndowLicenseLabel", GUILayout.Width(10)))
                            ignore = !ignore;
                        EditorGUILayout.LabelField(typeContent, labelStyle, GUILayout.Width(150));
                        if (ignore)
                        {
                            if (index == -1)
                            {
                                ignoreLocalDataTypeNameProp.InsertArrayElementAtIndex(ignoreLocalDataTypeNameProp.arraySize);
                                ignoreLocalDataTypeNameProp.GetArrayElementAtIndex(ignoreLocalDataTypeNameProp.arraySize - 1).stringValue = typeName;
                            }
                        }
                        else
                        {
                            if (index > -1)
                                ignoreLocalDataTypeNameProp.DeleteArrayElementAtIndex(index);
                        }
                        if (GUILayout.Button(unfoldLocalDataView[i] ? unfoldContent : foldContent, "AboutWIndowLicenseLabel"))
                            unfoldLocalDataView[i] = !unfoldLocalDataView[i];
                        EditorGUILayout.EndHorizontal();

                        if (unfoldLocalDataView[i])
                        {
                            Type type = localDataTypes[i];
                            LocalSaveUtility data = LocalSaveUtility.InjectGetLocalData(type);
                            FieldInfo[] fieldInfos = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
                            for (int j = 0; j < fieldInfos.Length; j++)
                            {
                                FieldInfo item = fieldInfos[j];
                                if (item.IsLiteral)
                                    continue;
                                if (item.IsDefined(typeof(WaitingFreeToSaveAttribute), true))
                                {
                                    WaitingFreeToSaveAttribute attribute = item.GetCustomAttribute(typeof(WaitingFreeToSaveAttribute), true) as WaitingFreeToSaveAttribute;
                                    string key = attribute.SaveName;
                                    fieldDrawer(key, item, data, 30);
                                }
                            }
                        }
                    }
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(50);
                    EditorGUILayout.PropertyField(injectSingleLocalDataCallbackProp);
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(50);
                    EditorGUILayout.PropertyField(injectAllLocalDataCallbackProp);
                    EditorGUILayout.EndHorizontal();
                }
                else
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(20);
                    EditorGUILayout.BeginVertical("ColorPickerSliderBackground");
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Null", labelStyle, GUILayout.Width(150));
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndHorizontal();
                }
            }
        }

        object fieldDrawer(string saveName, FieldInfo fieldInfo, object instance, float space, float space2 = 120)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(space);

            Type fieldType = fieldInfo.FieldType;
            string isStatic = fieldInfo.IsStatic ? "static " : "";
            GUIContent label = new GUIContent($"{fieldInfo.Name}:", $"{isStatic}{getTypeName(fieldType)} {fieldInfo.Name}");
            GUILayout.Label(label, GUILayout.Width(space2));
            EditorGUILayout.BeginVertical();
            object value = valDrawer(fieldInfo.GetValue(instance), fieldType);
            if (value != null)
                fieldInfo.SetValue(instance, value);
            else if (fieldType.IsArray)
            {
                EditorGUILayout.BeginVertical();
                Type genericType = fieldType.GetElementType();
                object obj = fieldInfo.GetValue(instance);
                if (obj == null)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("null", GUI.skin.GetStyle("LODRendererAddButton"), GUILayout.Width(25));
                    if (GUILayout.Button(addContent, "LODRendererAddButton", GUILayout.Width(25)))
                        fieldInfo.SetValue(instance, Array.CreateInstance(genericType, 0));
                    EditorGUILayout.EndHorizontal();
                }
                else
                {
                    bool listView = true;
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(obj.ToString(), GUI.skin.GetStyle("HeaderLabel"));
                    if (GUILayout.Button(subContent, "LODRendererAddButton", GUILayout.Width(25)))
                    {
                        fieldInfo.SetValue(instance, null);
                        listView = false;
                    }
                    EditorGUILayout.EndHorizontal();
                    if (listView)
                    {
                        ReorderableList reorderableList;
                        IList list = (IList)obj;
                        string key = $"{saveName}-{getTypeName(genericType)}-array";
                        if (listDic.TryGetValue(key, out reorderableList))
                            reorderableList.list = list;
                        else
                        {
                            bool noAddButton = LocalSaveUtility.NoDefaultConstructorTppeName(fieldType) != "";
                            reorderableList = new ReorderableList(list, genericType, true, false, !noAddButton, true);
                            reorderableList.onAddCallback = l =>
                            {
                                object newVal;
                                if (genericType.Name == "String")
                                    newVal = "";
                                else if (genericType.IsArray)
                                    newVal = Array.CreateInstance(genericType.GetElementType(), 0);
                                else
                                    newVal = Activator.CreateInstance(genericType);
                                l.list = (IList)dynamicArrayAddInfo.MakeGenericMethod(genericType).Invoke(this, new object[] { l.list, newVal });
                            };
                            reorderableList.onRemoveCallback = l =>
                            {
                                object val = l.list[l.index];
                                l.list = (IList)dynamicArrayRemoveInfo.MakeGenericMethod(genericType).Invoke(this, new object[] { l.list, val });
                            };
                            listCallback(key, reorderableList, genericType);
                            listDic.Add(key, reorderableList);
                            viewDic.Add(key, false);
                        }
                        reorderableList.DoLayoutList();
                        fieldInfo.SetValue(instance, reorderableList.list);
                    }
                }
                EditorGUILayout.EndVertical();
            }
            else if (fieldType.Name == "List`1")
            {
                EditorGUILayout.BeginVertical();
                object obj = fieldInfo.GetValue(instance);
                if (obj == null)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("null", GUI.skin.GetStyle("LODRendererAddButton"), GUILayout.Width(25));
                    if (LocalSaveUtility.NoDefaultConstructorTppeName(fieldType) == "")
                    {
                        if (GUILayout.Button(addContent, "LODRendererAddButton", GUILayout.Width(25)))
                            fieldInfo.SetValue(instance, Activator.CreateInstance(fieldType));
                    }
                    EditorGUILayout.EndHorizontal();
                }
                else
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(obj.ToString(), GUI.skin.GetStyle("HeaderLabel"));
                    if (GUILayout.Button(subContent, "LODRendererAddButton", GUILayout.Width(25)))
                        fieldInfo.SetValue(instance, null);
                    EditorGUILayout.EndHorizontal();
                    ReorderableList reorderableList;
                    IList list = (IList)obj;
                    Type genericType = fieldType.GetGenericArguments()[0];
                    string key = $"{saveName}-{getTypeName(genericType)}-list";
                    if (listDic.TryGetValue(key, out reorderableList))
                        reorderableList.list = list;
                    else
                    {
                        bool noAddButton = LocalSaveUtility.NoDefaultConstructorTppeName(genericType) != "";
                        reorderableList = new ReorderableList(list, genericType, true, false, !noAddButton, true);
                        reorderableList.onAddCallback = l =>
                        {
                            if (genericType.Name == "String")
                                l.list.Add("");
                            else if (genericType.IsArray)
                                l.list.Add(Array.CreateInstance(genericType.GetElementType(), 0));
                            else
                                l.list.Add(Activator.CreateInstance(genericType));
                        };
                        listCallback(key, reorderableList, genericType);
                        listDic.Add(key, reorderableList);
                        viewDic.Add(key, false);
                    }
                    reorderableList.DoLayoutList();
                }
                EditorGUILayout.EndVertical();
            }
            else if (fieldType.Name == "Dictionary`2")
            {
                object obj = fieldInfo.GetValue(instance);
                if (obj == null)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("null", GUI.skin.GetStyle("LODRendererAddButton"), GUILayout.Width(25));
                    if (LocalSaveUtility.NoDefaultConstructorTppeName(fieldType) == "")
                    {
                        if (GUILayout.Button(addContent, "LODRendererAddButton", GUILayout.Width(25)))
                            fieldInfo.SetValue(instance, Activator.CreateInstance(fieldType));
                    }
                    EditorGUILayout.EndHorizontal();
                }
                else
                {
                    Type keyType = fieldType.GetGenericArguments()[0];
                    Type valType = fieldType.GetGenericArguments()[1];
                    string keyName = $"{saveName}-{getTypeName(keyType)}-{getTypeName(valType)}-dic";
                    if (!viewDic.TryGetValue(keyName, out bool view))
                        viewDic.Add(keyName, view);
                    Array keys = (Array)getKeysFromDictionaryInfo.MakeGenericMethod(new[] { keyType, valType }).Invoke(null, new object[] { obj });
                    Array vals = (Array)getValuesFromDictionaryInfo.MakeGenericMethod(new[] { keyType, valType }).Invoke(null, new object[] { obj });
                    EditorGUILayout.BeginHorizontal();
                    if (GUILayout.Button(view ? unfoldContent : foldContent, "AboutWIndowLicenseLabel"))
                    {
                        view = !view;
                        viewDic[keyName] = view;
                    }
                    EditorGUILayout.LabelField(obj.ToString(), GUI.skin.GetStyle("HeaderLabel"));
                    if (GUILayout.Button(subContent, "LODRendererAddButton", GUILayout.Width(25)))
                        fieldInfo.SetValue(instance, null);
                    EditorGUILayout.EndHorizontal();
                    if (view)
                    {
                        Type dicType = typeof(ObjDictionary<,>);
                        dicType = dicType.MakeGenericType(new Type[] { keyType, valType });
                        FieldInfo keyInfo = dicType.GetField("Key", BindingFlags.Public | BindingFlags.Instance);
                        FieldInfo valInfo = dicType.GetField("Value", BindingFlags.Public | BindingFlags.Instance);
                        for (int i = 0; i < keys.Length; i++)
                        {
                            string key_name = $"{keyName}-{i}";
                            object key = keys.GetValue(i);
                            object val = vals.GetValue(i);
                            object dicVal = Activator.CreateInstance(dicType, new object[] { key, val });
                            if (!viewDic.TryGetValue(key_name, out bool keyView))
                                viewDic.Add(key_name, keyView);
                            EditorGUILayout.BeginHorizontal();
                            GUILayout.Space(10);
                            if (GUILayout.Button("", "ToggleMixed", GUILayout.Width(15)))
                            {
                                if ((bool)removeKeyFromDictionaryInfo.MakeGenericMethod(new Type[] { keyType, valType }).Invoke(null, new object[] { obj, key }))
                                    continue;
                            }
                            if (GUILayout.Button(keyView ? unfoldContent : foldContent, "AboutWIndowLicenseLabel"))
                            {
                                keyView = !keyView;
                                viewDic[key_name] = keyView;
                            }
                            if (keyType.IsClass)
                                fieldDrawer(key_name, keyInfo, dicVal, 0, 50);
                            else
                                dicVal = fieldDrawer(key_name, keyInfo, dicVal, 0, 50);
                            EditorGUILayout.EndHorizontal();
                            object newKey = keyInfo.GetValue(dicVal);
                            if ((bool)updateDictionaryKeyInfo.MakeGenericMethod(new Type[] { keyType, valType }).Invoke(null, new object[] { obj, key, newKey }))
                                key = newKey;
                            if (keyView)
                            {
                                if (valType.IsClass)
                                    fieldDrawer(key_name, valInfo, dicVal, 40, 50);
                                else
                                    dicVal = fieldDrawer(key_name, valInfo, dicVal, 40, 50);
                                object newValue = valInfo.GetValue(dicVal);
                                updateDictionaryValueInfo.MakeGenericMethod(new Type[] { keyType, valType }).Invoke(null, new object[] { obj, key, newValue });
                            }
                        }
                        if (LocalSaveUtility.NoDefaultConstructorTppeName(keyType) == "")
                        {
                            EditorGUILayout.BeginHorizontal();
                            GUILayout.Space(80);
                            EditorGUILayout.BeginVertical("SelectionRect");
                            string newDicName = $"{keyName}-New";
                            if (!objCacheDic.TryGetValue(newDicName, out object newDicKey))
                            {
                                if (keyType.Name == "String")
                                    newDicKey = "";
                                else if (keyType.IsArray)
                                    newDicKey = Array.CreateInstance(keyType, 0);
                                else
                                    newDicKey = Activator.CreateInstance(keyType);
                                objCacheDic.Add(newDicName, newDicKey);
                            }
                            object newDic = Activator.CreateInstance(dicType, new object[] { newDicKey });
                            bool containsKey = (bool)checkDictionaryKeyInfo.MakeGenericMethod(new Type[] { keyType, valType }).Invoke(null, new object[] { obj, newDicKey });
                            bool add = false;
                            EditorGUILayout.BeginHorizontal();
                            EditorGUI.BeginDisabledGroup(containsKey);
                            if (GUILayout.Button("New", containsKey ? "PreButtonRed" : "PreButtonGreen", GUILayout.Width(37)))
                            {
                                if ((bool)addDictionaryValueInfo.MakeGenericMethod(new Type[] { keyType, valType }).Invoke(null, new object[] { obj, newDicKey, null }))
                                {
                                    objCacheDic.Remove(newDicName);
                                    add = true;
                                }
                            }
                            EditorGUI.EndDisabledGroup();
                            GUILayout.Space(3);
                            if (!add)
                            {
                                if (keyType.IsClass)
                                    fieldDrawer(newDicName, keyInfo, newDic, 0, 50);
                                else
                                    newDic = fieldDrawer(newDicName, keyInfo, newDic, 0, 50);
                                objCacheDic[newDicName] = keyInfo.GetValue(newDic);
                            }
                            EditorGUILayout.EndHorizontal();
                            EditorGUILayout.EndVertical();
                            EditorGUILayout.EndHorizontal();
                        }
                    }
                }
            }
            else
            {
                object obj = fieldInfo.GetValue(instance);
                if (obj == null)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("null", GUI.skin.GetStyle("LODRendererAddButton"), GUILayout.Width(25));
                    if (LocalSaveUtility.NoDefaultConstructorTppeName(fieldType) == "")
                    {
                        if (GUILayout.Button(addContent, "LODRendererAddButton", GUILayout.Width(25)))
                            fieldInfo.SetValue(instance, Activator.CreateInstance(fieldType));
                    }
                    EditorGUILayout.EndHorizontal();
                }
                else
                {
                    string key = $"{saveName}-{getTypeName(fieldType)}";
                    FieldInfo[] fieldInfos;
                    if (ES3.UnsafeTypeList.Contains(fieldType))
                        fieldInfos = fieldType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
                    else
                        fieldInfos = fieldType.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
                    if (!viewDic.TryGetValue(key, out bool view))
                        viewDic.Add(key, view);
                    EditorGUILayout.BeginHorizontal();
                    if (fieldInfos.Count(x => !x.IsLiteral) > 0)
                    {
                        if (GUILayout.Button(view ? unfoldContent : foldContent, "AboutWIndowLicenseLabel"))
                        {
                            view = !view;
                            viewDic[key] = view;
                        }
                    }
                    try
                    {
                        EditorGUILayout.LabelField(obj.ToString(), GUI.skin.GetStyle("HeaderLabel"));
                    }
                    catch
                    {
                        EditorGUILayout.LabelField("Error", GUI.skin.GetStyle("ErrorLabel"));
                    }
                    if (fieldType.IsClass && GUILayout.Button(subContent, "LODRendererAddButton", GUILayout.Width(25)))
                        fieldInfo.SetValue(instance, null);
                    EditorGUILayout.EndHorizontal();
                    if (view)
                    {
                        for (int i = 0; i < fieldInfos.Length; i++)
                        {
                            FieldInfo item = fieldInfos[i];
                            if (item.IsLiteral)
                                continue;
                            if (fieldType.IsClass)
                                fieldDrawer(key, item, obj, 0, 60);
                            else
                                fieldInfo.SetValue(instance, fieldDrawer(key, item, obj, 0, 60));
                        }
                    }
                }
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
            return instance;
        }

        object valDrawer(object value, Type type)
        {
            if (type == typeof(string))
            {
                if (value == null)
                    value = "";
                return GUILayout.TextField((string)value);
            }
            else if (type == typeof(int))
                return EditorGUILayout.IntField((int)value);
            else if (type == typeof(long))
                return EditorGUILayout.LongField((long)value);
            else if (type == typeof(float))
                return EditorGUILayout.FloatField((float)value);
            else if (type == typeof(double))
                return EditorGUILayout.DoubleField((double)value);
            else if (type == typeof(bool))
                return EditorGUILayout.Toggle((bool)value);
            else if (type == typeof(Vector2))
                return EditorGUILayout.Vector2Field("", (Vector2)value);
            else if (type == typeof(Vector2Int))
                return EditorGUILayout.Vector2IntField("", (Vector2Int)value);
            else if (type == typeof(Vector3))
                return EditorGUILayout.Vector3Field("", (Vector3)value);
            else if (type == typeof(Vector3Int))
                return EditorGUILayout.Vector3IntField("", (Vector3Int)value);
            else if (type == typeof(Vector4))
                return EditorGUILayout.Vector4Field("", (Vector4)value);
            else if (type == typeof(Quaternion))
            {
                Quaternion quaternion = (Quaternion)value;
                Vector4 val = new Vector4(quaternion.x, quaternion.y, quaternion.z, quaternion.w);
                val = EditorGUILayout.Vector4Field("", val);
                quaternion.x = val.x;
                quaternion.y = val.y;
                quaternion.z = val.z;
                quaternion.w = val.w;
                return quaternion;
            }
            else if (type == typeof(Color))
                return EditorGUILayout.ColorField((Color)value);
            else if (type == typeof(Bounds))
            {
                return EditorGUILayout.BoundsField((Bounds)value);
            }
            else if (type == typeof(BoundsInt))
            {
                return EditorGUILayout.BoundsIntField((BoundsInt)value);
            }
            else if (type == typeof(LayerMask))
                return (LayerMask)EditorGUILayout.MaskField((LayerMask)value, layerMaskNames);
            else if (type == typeof(Rect))
                return EditorGUILayout.RectField((Rect)value);
            else if (type == typeof(RectInt))
                return EditorGUILayout.RectIntField((RectInt)value);
            else if (type.IsEnum)
                return EditorGUILayout.EnumPopup((Enum)value);
            else if (type == typeof(AnimationCurve))
            {
                AnimationCurve curve = (AnimationCurve)value;
                if (curve == null)
                    curve = new AnimationCurve();
                return EditorGUILayout.CurveField(curve);
            }
            else if (type == typeof(Gradient))
            {
                Gradient gradient = (Gradient)value;
                if (gradient == null)
                    gradient = new Gradient();
                return EditorGUILayout.GradientField(gradient);
            }
            else
            {
                return null;
            }
        }

        object valDrawer(Rect rect, object value, Type type, ref float line)
        {
            rect.height = 17;
            line = 20;
            if (type == typeof(string))
            {
                if (value == null)
                    value = "";
                return EditorGUI.TextField(rect, (string)value);
            }
            else if (type == typeof(int))
                return EditorGUI.IntField(rect, (int)value);
            else if (type == typeof(long))
                return EditorGUI.LongField(rect, (long)value);
            else if (type == typeof(float))
                return EditorGUI.FloatField(rect, (float)value);
            else if (type == typeof(double))
                return EditorGUI.DoubleField(rect, (double)value);
            else if (type == typeof(bool))
                return EditorGUI.Toggle(rect, (bool)value);
            else if (type == typeof(Vector2))
                return EditorGUI.Vector2Field(rect, "", (Vector2)value);
            else if (type == typeof(Vector2Int))
                return EditorGUI.Vector2IntField(rect, "", (Vector2Int)value);
            else if (type == typeof(Vector3))
                return EditorGUI.Vector3Field(rect, "", (Vector3)value);
            else if (type == typeof(Vector3Int))
                return EditorGUI.Vector3IntField(rect, "", (Vector3Int)value);
            else if (type == typeof(Vector4))
                return EditorGUI.Vector4Field(rect, "", (Vector4)value);
            else if (type == typeof(Quaternion))
            {
                Quaternion quaternion = (Quaternion)value;
                Vector4 val = new Vector4(quaternion.x, quaternion.y, quaternion.z, quaternion.w);
                val = EditorGUI.Vector4Field(rect, "", val);
                quaternion.x = val.x;
                quaternion.y = val.y;
                quaternion.z = val.z;
                quaternion.w = val.w;
                return quaternion;
            }
            else if (type == typeof(Color))
                return EditorGUI.ColorField(rect, (Color)value);
            else if (type == typeof(Bounds))
            {
                line = 35;
                return EditorGUI.BoundsField(rect, (Bounds)value);
            }
            else if (type == typeof(BoundsInt))
            {
                line = 35;
                return EditorGUI.BoundsIntField(rect, (BoundsInt)value);
            }
            else if (type == typeof(LayerMask))
                return (LayerMask)EditorGUI.MaskField(rect, (LayerMask)value, layerMaskNames);
            else if (type == typeof(Rect))
            {
                line = 35;
                return EditorGUI.RectField(rect, (Rect)value);
            }
            else if (type == typeof(RectInt))
            {
                line = 35;
                return EditorGUI.RectIntField(rect, (RectInt)value);
            }
            else if (type.IsEnum)
                return EditorGUI.EnumPopup(rect, (Enum)value);
            else if (type == typeof(AnimationCurve))
            {
                AnimationCurve curve = (AnimationCurve)value;
                if (curve == null)
                    curve = new AnimationCurve();
                return EditorGUI.CurveField(rect, curve);
            }
            else if (type == typeof(Gradient))
            {
                Gradient gradient = (Gradient)value;
                if (gradient == null)
                    gradient = new Gradient();
                return EditorGUI.GradientField(rect, gradient);
            }
            else
            {
                line = 0;
                return null;
            }
        }

        object listElementDrawer(Rect rect, object value, Type type, string key, ref float line, bool parentIsIList = true)
        {
            rect.height = 17;
            object Val = valDrawer(rect, value, type, ref line);
            if (line != 0)
                return Val;
            else if (type.IsArray)
            {
                Type genericType = type.GetElementType();
                if (value == null)
                {
                    rect.width = 25;
                    EditorGUI.LabelField(rect, "null", GUI.skin.GetStyle("LODRendererAddButton"));
                    rect.x += 25;
                    if (GUI.Button(rect, addContent, "LODRendererAddButton"))
                        value = Array.CreateInstance(genericType, 0);
                    line = 20;
                    return value;
                }
                else
                {
                    float rect_x = rect.x;
                    float width = rect.width;
                    rect.width -= 25;
                    EditorGUI.LabelField(rect, value.ToString(), GUI.skin.GetStyle("HeaderLabel"));
                    rect.x += rect.width;
                    rect.width = 25;
                    if (!parentIsIList && GUI.Button(rect, subContent, "LODRendererAddButton"))
                        value = null;
                    if (value != null)
                    {
                        rect.height += 20;
                        rect.x = rect_x;
                        rect.width = width;
                        ReorderableList reorderableList;
                        IList list = (IList)value;
                        if (listDic.TryGetValue(key, out reorderableList))
                            reorderableList.list = list;
                        else
                        {
                            bool noAddButton = LocalSaveUtility.NoDefaultConstructorTppeName(type) != "";
                            reorderableList = new ReorderableList(list, genericType, true, false, !noAddButton, true);
                            reorderableList.onAddCallback = l =>
                            {
                                object newVal;
                                if (genericType.Name == "String")
                                    newVal = "";
                                else if (genericType.IsArray)
                                    newVal = Array.CreateInstance(genericType.GetElementType(), 0);
                                else
                                    newVal = Activator.CreateInstance(genericType);
                                l.list = (IList)dynamicArrayAddInfo.MakeGenericMethod(genericType).Invoke(this, new object[] { l.list, newVal });
                            };
                            reorderableList.onRemoveCallback = l =>
                            {
                                object val = l.list[l.index];
                                l.list = (IList)dynamicArrayRemoveInfo.MakeGenericMethod(genericType).Invoke(this, new object[] { l.list, val });
                            };
                            listCallback(key, reorderableList, genericType);
                            listDic.Add(key, reorderableList);
                            viewDic.Add(key, false);
                        }
                        rect.y += 17;
                        reorderableList.DoList(rect);
                        line = 45;
                        return reorderableList.list;
                    }
                    else
                    {
                        line = 20;
                        return value;
                    }
                }
            }
            else if (type.Name == "List`1")
            {
                if (value == null)
                {
                    rect.width = 25;
                    EditorGUI.LabelField(rect, "null", GUI.skin.GetStyle("LODRendererAddButton"));
                    if (LocalSaveUtility.NoDefaultConstructorTppeName(type) == "")
                    {
                        rect.x += 25;
                        if (GUI.Button(rect, addContent, "LODRendererAddButton"))
                            value = Activator.CreateInstance(type);
                    }
                    line = 20;
                    return value;
                }
                else
                {
                    float rect_x = rect.x;
                    float width = rect.width;
                    rect.width -= 25;
                    EditorGUI.LabelField(rect, value.ToString(), GUI.skin.GetStyle("HeaderLabel"));
                    rect.x += rect.width;
                    rect.width = 25;
                    if (!parentIsIList && GUI.Button(rect, subContent, "LODRendererAddButton"))
                        value = null;
                    if (value != null)
                    {
                        rect.height += 20;
                        rect.x = rect_x;
                        rect.width = width;
                        ReorderableList reorderableList;
                        IList list = (IList)value;
                        Type genericType = type.GetGenericArguments()[0];
                        if (listDic.TryGetValue(key, out reorderableList))
                            reorderableList.list = list;
                        else
                        {
                            bool noAddButton = LocalSaveUtility.NoDefaultConstructorTppeName(genericType) != "";
                            reorderableList = new ReorderableList(list, genericType, true, false, !noAddButton, true);
                            reorderableList.onAddCallback = l =>
                            {
                                if (genericType.Name == "String")
                                    l.list.Add("");
                                else if (genericType.IsArray)
                                    l.list.Add(Array.CreateInstance(genericType.GetElementType(), 0));
                                else
                                    l.list.Add(Activator.CreateInstance(genericType));
                            };
                            listCallback(key, reorderableList, genericType);
                            listDic.Add(key, reorderableList);
                            viewDic.Add(key, false);
                        }
                        rect.y += 17;
                        reorderableList.DoList(rect);
                        line = 45;
                        return value;
                    }
                    else
                    {
                        line = 20;
                        return value;
                    }
                }
            }
            else if (type.Name == "Dictionary`2")
            {
                if (value == null)
                {
                    rect.width = 25;
                    EditorGUI.LabelField(rect, "null", GUI.skin.GetStyle("LODRendererAddButton"));
                    if (LocalSaveUtility.NoDefaultConstructorTppeName(type) == "")
                    {
                        rect.x += 25;
                        if (GUI.Button(rect, addContent, "LODRendererAddButton"))
                            value = Activator.CreateInstance(type);
                    }
                    line = 20;
                    return value;
                }
                else
                {
                    Type keyType = type.GetGenericArguments()[0];
                    Type valType = type.GetGenericArguments()[1];
                    if (!viewDic.TryGetValue(key, out bool view))
                        viewDic.Add(key, view);
                    Array keys = (Array)getKeysFromDictionaryInfo.MakeGenericMethod(new[] { keyType, valType }).Invoke(null, new object[] { value });
                    Array vals = (Array)getValuesFromDictionaryInfo.MakeGenericMethod(new[] { keyType, valType }).Invoke(null, new object[] { value });

                    float rect_x = rect.x;
                    float width = rect.width;
                    rect.width = 17;
                    if (GUI.Button(rect, view ? unfoldContent : foldContent, "AboutWIndowLicenseLabel"))
                    {
                        view = !view;
                        viewDic[key] = view;
                    }
                    rect.x += rect.width;
                    rect.width = width - 17;
                    if (!parentIsIList)
                        rect.width -= 25;
                    EditorGUI.LabelField(rect, value.ToString(), GUI.skin.GetStyle("HeaderLabel"));
                    if (!parentIsIList)
                    {
                        rect.x += rect.width;
                        rect.width = 25;
                        if (GUI.Button(rect, subContent, "LODRendererAddButton"))
                            value = null;
                    }
                    if (value != null && view)
                    {
                        rect_x += 10;
                        width -= 10;
                        float fieldHeight = 0;
                        Type dicType = typeof(ObjDictionary<,>);
                        dicType = dicType.MakeGenericType(new Type[] { keyType, valType });
                        FieldInfo keyInfo = dicType.GetField("Key", BindingFlags.Public | BindingFlags.Instance);
                        FieldInfo valInfo = dicType.GetField("Value", BindingFlags.Public | BindingFlags.Instance);
                        GUIContent keyContent = new GUIContent($"{keyInfo.Name}:", $"{keyType.Name} {keyInfo.Name}");
                        GUIContent valueContent = new GUIContent($"{valInfo.Name}:", $"{valType.Name} {valInfo.Name}");
                        for (int i = 0; i < keys.Length; i++)
                        {
                            string key_name = $"{key}-{i}";
                            object _key = keys.GetValue(i);
                            object _val = vals.GetValue(i);
                            object dicVal = Activator.CreateInstance(dicType, new object[] { _key, _val });
                            if (!viewDic.TryGetValue(key_name, out bool keyView))
                                viewDic.Add(key_name, keyView);

                            rect.width = width;
                            rect.x = rect_x;
                            rect.y += 20;
                            rect.width = 15;
                            if (GUI.Button(rect, "", "ToggleMixed"))
                            {
                                if ((bool)removeKeyFromDictionaryInfo.MakeGenericMethod(new Type[] { keyType, valType }).Invoke(null, new object[] { value, _key }))
                                    continue;
                            }
                            rect.x += rect.width + 3;
                            rect.width = 17;
                            if (GUI.Button(rect, keyView ? unfoldContent : foldContent, "AboutWIndowLicenseLabel"))
                            {
                                keyView = !keyView;
                                viewDic[key_name] = keyView;
                            }
                            rect.x += rect.width;
                            rect.width = 50;
                            float _height = 0;
                            EditorGUI.LabelField(rect, keyContent);
                            rect.x += rect.width;
                            rect.width = width - 85;
                            object newKey = listElementDrawer(rect, keyInfo.GetValue(dicVal), keyType, key_name, ref _height, false);
                            keyInfo.SetValue(dicVal, newKey);
                            fieldHeight += _height;
                            if ((bool)updateDictionaryKeyInfo.MakeGenericMethod(new Type[] { keyType, valType }).Invoke(null, new object[] { value, _key, newKey }))
                                _key = newKey;
                            if (keyView)
                            {
                                _height = 0;
                                rect.y += 20;
                                rect.x -= 54;
                                rect.width = 50;
                                EditorGUI.LabelField(rect, valueContent);
                                rect.x += rect.width;
                                rect.width = width - 81;
                                object newValue = listElementDrawer(rect, valInfo.GetValue(dicVal), valType, key_name, ref _height, false);
                                valInfo.SetValue(dicVal, newValue);
                                fieldHeight += _height;
                                updateDictionaryValueInfo.MakeGenericMethod(new Type[] { keyType, valType }).Invoke(null, new object[] { value, _key, newValue });
                            }
                        }
                        if (LocalSaveUtility.NoDefaultConstructorTppeName(keyType) == "")
                        {
                            rect.y += 20;
                            rect.x = rect_x + 70;
                            rect.width = width - 70;
                            rect.width += 4;
                            rect.height += 4;
                            rect.x -= 2;
                            rect.y += 2;
                            GUI.BeginGroup(rect, GUI.skin.GetStyle("SelectionRect"));
                            rect.x = 2;
                            rect.y = 2;
                            string newDicName = $"{key}-New";
                            if (!objCacheDic.TryGetValue(newDicName, out object newDicKey))
                            {
                                if (keyType.Name == "String")
                                    newDicKey = "";
                                else if (keyType.IsArray)
                                    newDicKey = Array.CreateInstance(keyType, 0);
                                else
                                    newDicKey = Activator.CreateInstance(keyType);
                                objCacheDic.Add(newDicName, newDicKey);
                            }
                            object newDic = Activator.CreateInstance(dicType, new object[] { newDicKey });
                            bool containsKey = (bool)checkDictionaryKeyInfo.MakeGenericMethod(new Type[] { keyType, valType }).Invoke(null, new object[] { value, newDicKey });
                            bool add = false;
                            EditorGUI.BeginDisabledGroup(containsKey);
                            rect.width = 37;
                            if (GUI.Button(rect, "New", containsKey ? "PreButtonRed" : "PreButtonGreen"))
                            {
                                if ((bool)addDictionaryValueInfo.MakeGenericMethod(new Type[] { keyType, valType }).Invoke(null, new object[] { value, newDicKey, null }))
                                {
                                    objCacheDic.Remove(newDicName);
                                    add = true;
                                }
                            }
                            EditorGUI.EndDisabledGroup();
                            if (!add)
                            {
                                float _height = 0;
                                rect.x += 40;
                                rect.width = 50;
                                EditorGUI.LabelField(rect, keyContent);
                                rect.x += 50;
                                rect.width = width - 160;
                                object newVal = listElementDrawer(rect, keyInfo.GetValue(newDic), keyType, newDicName, ref _height, false);
                                objCacheDic[newDicName] = newVal;
                                fieldHeight += _height;
                            }
                            fieldHeight += 5;
                            GUI.EndGroup();
                        }
                        heightDic[key] = fieldHeight + 20;
                    }
                    else
                        heightDic[key] = 0;
                    line = 20;
                    return value;
                }
            }
            else
            {
                if (value == null)
                {
                    rect.width = 25;
                    EditorGUI.LabelField(rect, "null", GUI.skin.GetStyle("LODRendererAddButton"));
                    if (LocalSaveUtility.NoDefaultConstructorTppeName(type) == "")
                    {
                        rect.x += 25;
                        if (GUI.Button(rect, addContent, "LODRendererAddButton"))
                            value = Activator.CreateInstance(type);
                    }
                    line = 20;
                    return value;
                }
                else
                {
                    float width = rect.width;
                    float rect_x = rect.x;
                    FieldInfo[] fieldInfos;
                    if (ES3.UnsafeTypeList.Contains(type))
                        fieldInfos = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
                    else
                        fieldInfos = type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
                    if (!viewDic.TryGetValue(key, out bool view))
                        viewDic.Add(key, view);
                    if (fieldInfos.Count(x => !x.IsLiteral) > 0)
                    {
                        rect.width = 20;
                        if (GUI.Button(rect, view ? unfoldContent : foldContent, "AboutWIndowLicenseLabel"))
                        {
                            view = !view;
                            viewDic[key] = view;
                        }
                        rect.x += rect.width;
                        rect.width = width - 20;
                    }
                    if (!parentIsIList && type.IsClass)
                        rect.width -= 25;
                    try
                    {
                        EditorGUI.LabelField(rect, value.ToString(), GUI.skin.GetStyle("HeaderLabel"));
                    }
                    catch
                    {
                        EditorGUI.LabelField(rect, "Error", GUI.skin.GetStyle("ErrorLabel"));
                    }
                    if (!parentIsIList && type.IsClass)
                    {
                        rect.x += rect.width;
                        rect.width = 25;
                        if (GUI.Button(rect, subContent, "LODRendererAddButton"))
                            value = null;
                    }
                    if (value != null && view)
                    {
                        float fieldHeight = 0;
                        for (int i = 0; i < fieldInfos.Length; i++)
                        {
                            FieldInfo item = fieldInfos[i];
                            if (item.IsLiteral)
                                continue;
                            rect.x = rect_x;
                            rect.width = 100;
                            rect.y += 20;
                            string isStatic = item.IsStatic ? "static " : "";
                            GUIContent label = new GUIContent($"{item.Name}:", $"{isStatic}{getTypeName(item.FieldType)} {item.Name}");
                            EditorGUI.LabelField(rect, label);
                            rect.x += 100;
                            rect.width = width - 100;
                            float _height = 0;
                            string itemKey = $"{key}-{item.Name}";
                            object itemVal = listElementDrawer(rect, item.GetValue(value), item.FieldType, itemKey, ref _height, false);
                            item.SetValue(value, itemVal);
                            if (itemVal != null && viewDic.TryGetValue(itemKey, out bool itemView))
                            {
                                if (itemView)
                                {
                                    if (listDic.TryGetValue(itemKey, out ReorderableList itemList))
                                    {
                                        if (itemList.count > 0)
                                        {
                                            float height = 0;
                                            for (int j = 0; j < itemList.count; j++)
                                                height += itemList.elementHeightCallback.Invoke(j);
                                            height += 57;
                                            fieldHeight += height;
                                            rect.y += height - 20;
                                        }
                                        else
                                        {
                                            fieldHeight += 74;
                                            rect.y += 55;
                                        }
                                    }
                                    else
                                    {
                                        float itemViewHeight = heightDic[itemKey];
                                        fieldHeight += itemViewHeight;
                                        rect.y += itemViewHeight - 20;
                                    }
                                }
                                else
                                {
                                    fieldHeight += _height;
                                    rect.y += _height - 20;
                                }
                            }
                            else
                            {
                                fieldHeight += _height;
                                rect.y += _height - 20;
                            }
                        }
                        heightDic[key] = fieldHeight + 20;
                    }
                    else
                        heightDic[key] = 0;
                    line = 20;
                    return value;
                }
            }
        }

        void listCallback(string key, ReorderableList reorderableList, Type elementType)
        {
            string typeName = getTypeName(elementType);
            bool canAdd = reorderableList.displayAdd;
            heightDic[key] = 20;
            reorderableList.elementHeightCallback = index =>
            {
                if (viewDic[key])
                {
                    float height = heightDic[key];
                    string name = $"{key}-{index}";
                    if (viewDic.TryGetValue(name, out bool elementView))
                    {
                        if (elementView)
                        {
                            if (listDic.TryGetValue(name, out ReorderableList list))
                            {
                                if (list.count > 0)
                                {
                                    for (int i = 0; i < list.count; i++)
                                        height += list.elementHeightCallback.Invoke(i);
                                    height += 15;
                                }
                                else
                                    height += 32;
                            }
                            else
                            {
                                height += heightDic[name] - 17;
                            }
                        }
                    }
                    return height;
                }
                else
                    return 0;
            };
            reorderableList.drawHeaderCallback = rect =>
            {
                rect.x += 10f;
                bool res = viewDic[key];
                rect.width = 100;
                res = EditorGUI.Foldout(rect, res, typeName);
                if (res)
                {
                    if (reorderableList.count == 0)
                        reorderableList.elementHeight = 17;
                    reorderableList.footerHeight = 13;
                    reorderableList.displayAdd = canAdd;
                    reorderableList.displayRemove = true;
                    reorderableList.draggable = true;
                }
                else
                {
                    if (reorderableList.count == 0)
                        reorderableList.elementHeight = 0;
                    reorderableList.footerHeight = 0;
                    reorderableList.displayAdd = false;
                    reorderableList.displayRemove = false;
                    reorderableList.draggable = false;
                }
                viewDic[key] = res;
            };

            reorderableList.drawElementCallback = (rect, index, isActive, isFocused) =>
            {
                if (!viewDic[key])
                    return;
                object element = reorderableList.list[index];
                float line = 0;
                object newVal = listElementDrawer(rect, element, elementType, $"{key}-{index}", ref line);
                reorderableList.list[index] = newVal;
                heightDic[key] = line;
            };
        }

        static TKey[] getKeysFromDictionary<TKey, TValue>(Dictionary<TKey, TValue> dic)
        {
            return dic.Keys.ToArray();
        }

        static TValue[] getValuesFromDictionary<TKey, TValue>(Dictionary<TKey, TValue> dic)
        {
            return dic.Values.ToArray();
        }

        static bool checkDictionaryKey<TKey, TValue>(Dictionary<TKey, TValue> dic, TKey key)
        {
            if (key == null)
                return true;
            return dic.ContainsKey(key);
        }

        static bool addDictionaryValue<TKey, TValue>(Dictionary<TKey, TValue> dic, TKey key, TValue val)
        {
            if (key == null)
                return false;
            if (dic.ContainsKey(key))
                return false;
            dic.Add(key, val);
            return true;
        }

        static bool removeKeyFromDictionary<TKey, TValue>(Dictionary<TKey, TValue> dic, TKey key)
        {
            return dic.Remove(key);
        }

        static bool updateDictionaryKey<TKey, TValue>(Dictionary<TKey, TValue> dic, TKey originKey, TKey newKey)
        {
            if (newKey == null)
            {
                dic.Remove(originKey);
                return true;
            }
            if (dic.ContainsKey(newKey))
                return false;
            if (dic.TryGetValue(originKey, out TValue val))
            {
                dic.Remove(originKey);
                dic.Add(newKey, val);
                return true;
            }
            return false;
        }

        static void updateDictionaryValue<TKey, TValue>(Dictionary<TKey, TValue> dic, TKey key, TValue value)
        {
            if (key == null)
                return;
            if (dic.ContainsKey(key))
                dic[key] = value;
            else
                dic.Add(key, value);
        }

        static Array dynamicArrayAdd<T>(IList l, object val)
        {
            List<T> cloneList = new List<T>((IEnumerable<T>)l);
            cloneList.Add((T)val);
            return cloneList.ToArray();
        }

        static Array dynamicArrayRemove<T>(IList l, object val)
        {
            List<T> cloneList = new List<T>((IEnumerable<T>)l);
            T value = (T)val;
            if (cloneList.Contains(value))
                cloneList.Remove(value);
            return cloneList.ToArray();
        }

        string getTypeName(Type type)
        {
            string name = type.Name;
            if (name == "List`1")
                name = $"List<{getTypeName(type.GetGenericArguments()[0])}>";
            else if (name == "Dictionary`2")
            {
                Type[] genericTypes = type.GetGenericArguments();
                name = $"Dictionary<{getTypeName(genericTypes[0])},{getTypeName(genericTypes[1])}>";
            }
            return name;
        }

        void checkRunTimeSequenceList()
        {
            for (int i = 0; i < runTimeSequenceProp.arraySize; i++)
            {
                int enumIndex = runTimeSequenceProp.GetArrayElementAtIndex(i).enumValueIndex;
                bool res = false;
                if (enumIndex == 0 && (!loadDataTableProp.boolValue || dataTableIsNullProp.boolValue))
                    res = true;
                if (enumIndex == 1 && (!loadJsonDataProp.boolValue || jsonDataIsNullProp.boolValue))
                    res = true;
                if (enumIndex == 2 && (!injectLocalDataProp.boolValue || localDataIsNullProp.boolValue))
                    res = true;
                if (enumIndex == 3 && (!useFguiProp.boolValue || framworkEntry.FguiConfiguration == null))
                    res = true;
                if (res)
                {
                    runTimeSequenceProp.DeleteArrayElementAtIndex(i);
                    i--;
                }
            }
            if (runTimeSequenceList == null)
            {
                runTimeSequenceList = new ReorderableList(serializedObject, runTimeSequenceProp);
                runTimeSequenceList.drawHeaderCallback = rect =>
                {
                    EditorGUI.LabelField(rect, "RunTime load sequence");
                };
                runTimeSequenceList.drawElementCallback = (rect, index, isActive, isFocused) =>
                {
                    SerializedProperty property = runTimeSequenceProp.GetArrayElementAtIndex(index);
                    EditorGUI.LabelField(rect, property.enumNames[property.enumValueIndex].ToString());
                };
                runTimeSequenceList.onAddDropdownCallback = (rect, list) =>
                {
                    serializedObject.Update();
                    var menu = new GenericMenu();
                    bool isNull = true;
                    bool fguiRes = false;
                    bool dataTableRes = false;
                    bool jsonDataRes = false;
                    bool localSaveDataRes = false;
                    for (int i = 0; i < runTimeSequenceProp.arraySize; i++)
                    {
                        int typeIndex = runTimeSequenceProp.GetArrayElementAtIndex(i).enumValueIndex;
                        if (typeIndex == 0)
                            dataTableRes = true;
                        else if (typeIndex == 1)
                            jsonDataRes = true;
                        else if (typeIndex == 2)
                            localSaveDataRes = true;
                        else if (typeIndex == 3)
                            fguiRes = true;
                    }
                    if (loadDataTableProp.boolValue && !dataTableIsNullProp.boolValue)
                    {
                        if (dataTableRes)
                        {
                            menu.AddDisabledItem(new GUIContent("DataTable"));
                        }
                        else
                        {
                            menu.AddItem(new GUIContent("DataTable"), false, () =>
                            {
                                runTimeSequenceProp.InsertArrayElementAtIndex(runTimeSequenceProp.arraySize);
                                SerializedProperty str = runTimeSequenceProp.GetArrayElementAtIndex(runTimeSequenceProp.arraySize - 1);
                                str.enumValueIndex = 0;
                                serializedObject.ApplyModifiedProperties();
                            });
                        }
                        isNull = false;
                    }
                    if (loadJsonDataProp.boolValue && !jsonDataIsNullProp.boolValue)
                    {
                        if (jsonDataRes)
                        {
                            menu.AddDisabledItem(new GUIContent("JsonData"));
                        }
                        else
                        {
                            menu.AddItem(new GUIContent("JsonData"), false, () =>
                            {
                                runTimeSequenceProp.InsertArrayElementAtIndex(runTimeSequenceProp.arraySize);
                                SerializedProperty str = runTimeSequenceProp.GetArrayElementAtIndex(runTimeSequenceProp.arraySize - 1);
                                str.enumValueIndex = 1;
                                serializedObject.ApplyModifiedProperties();
                            });
                        }
                        isNull = false;
                    }
                    if (injectLocalDataProp.boolValue && !localDataIsNullProp.boolValue)
                    {
                        if (localSaveDataRes)
                        {
                            menu.AddDisabledItem(new GUIContent("LocalData"));
                        }
                        else
                        {
                            menu.AddItem(new GUIContent("LocalData"), false, () =>
                            {
                                runTimeSequenceProp.InsertArrayElementAtIndex(runTimeSequenceProp.arraySize);
                                SerializedProperty str = runTimeSequenceProp.GetArrayElementAtIndex(runTimeSequenceProp.arraySize - 1);
                                str.enumValueIndex = 2;
                                serializedObject.ApplyModifiedProperties();
                            });
                        }
                        isNull = false;
                    }
                    if (useFguiProp.boolValue && framworkEntry.FguiConfiguration != null)
                    {
                        if (fguiRes)
                        {
                            menu.AddDisabledItem(new GUIContent("Fgui"));
                        }
                        else
                        {
                            menu.AddItem(new GUIContent("Fgui"), false, () =>
                            {
                                runTimeSequenceProp.InsertArrayElementAtIndex(runTimeSequenceProp.arraySize);
                                SerializedProperty str = runTimeSequenceProp.GetArrayElementAtIndex(runTimeSequenceProp.arraySize - 1);
                                str.enumValueIndex = 3;
                                serializedObject.ApplyModifiedProperties();
                            });
                        }
                        isNull = false;
                    }
                    if (isNull)
                    {
                        menu.AddDisabledItem(new GUIContent("null"));
                    }
                    menu.DropDown(rect);
                    serializedObject.ApplyModifiedProperties();
                };
            }
        }
    }

    class ObjDictionary<TKey, TValue>
    {
        public TKey Key;
        public TValue Value;

        public ObjDictionary(TKey key)
        {
            Key = key;
        }

        public ObjDictionary(TKey key, TValue value)
        {
            Key = key;
            Value = value;
        }
    }
}