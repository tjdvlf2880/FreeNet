using UnityEngine;
using Unity.Netcode.Editor;
using UnityEditor;

[CustomEditor(typeof(NgoManager), true)]
public class NgoManagerEditor : NetworkManagerEditor
{
    // Serialized Properties
    private SerializedProperty _localBufferSec;
    private SerializedProperty _serverBufferSec;
    private SerializedProperty _useEpicOnlineTransport;

    private SerializedProperty _jitterRange;

    private SerializedProperty _virtualRtt;

    private SerializedProperty _fixedRtt;


    private new void OnEnable()
    {
        _localBufferSec = serializedObject.FindProperty("_localBufferSec");
        _serverBufferSec = serializedObject.FindProperty("_serverBufferSec");
        _useEpicOnlineTransport = serializedObject.FindProperty("_useEpicOnlineTransport");
        _jitterRange = serializedObject.FindProperty("_jitterRange");
        _virtualRtt = serializedObject.FindProperty("_virtualRtt");
        _fixedRtt = serializedObject.FindProperty("_fixedRtt");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        base.OnInspectorGUI();

        EditorGUI.BeginChangeCheck(); 

        EditorGUILayout.PropertyField(_localBufferSec);
        EditorGUILayout.PropertyField(_serverBufferSec);
        EditorGUILayout.PropertyField(_useEpicOnlineTransport);

        if (EditorGUI.EndChangeCheck()) 
        {
            serializedObject.ApplyModifiedProperties();
            OnValueChanged();
        }
        else
        {
            serializedObject.ApplyModifiedProperties();
        }
    }


    private void OnValueChanged()
    {
        NgoManager manager = (NgoManager)target; // target을 캐스팅

        if (manager != null)
        {
            manager.SetNetworkValue(); // NgoManager의 실제 함수 호출
        }
    }
}
