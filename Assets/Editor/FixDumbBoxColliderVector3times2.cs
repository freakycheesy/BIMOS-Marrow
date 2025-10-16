using UnityEngine;
using UnityEditor;
public class FixDumbBoxColliderVector3times2 : EditorWindow
{
    [MenuItem("Marrow uhh/Fix Dumb BoxCollider")]
    public static void ShowWindow() {
        GetWindow<FixDumbBoxColliderVector3times2>().Show();
    }

    private void OnGUI() {
        if (GUILayout.Button("Fix")) {
            var boxColliders = Resources.FindObjectsOfTypeAll<BoxCollider>();
            foreach (var boxCollider in boxColliders) {
                if (boxCollider.size == Vector3.one * 2) {
                    boxCollider.size = Vector3.one;
                }
            }
        }
    }
}
