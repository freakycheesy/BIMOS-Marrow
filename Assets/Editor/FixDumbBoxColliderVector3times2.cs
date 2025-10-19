using UnityEngine;
using UnityEditor;
using System.Linq;
public class FixDumbBoxColliderVector3times2 : EditorWindow
{
    [MenuItem("Marrow uhh/Fix Dumb BoxCollider")]
    public static void ShowWindow() {
        GetWindow<FixDumbBoxColliderVector3times2>().Show();
    }

    private void OnGUI() {
        if (GUILayout.Button("Fix")) {
            try {
                var boxColliders = Resources.FindObjectsOfTypeAll<BoxCollider>().ToList();
                foreach (var boxCollider in boxColliders.ToList()) {
                    try {
                        if (boxCollider.size == Vector3.one * 2) {
                            boxCollider.size = Vector3.one;
                        }
                        EditorUtility.SetDirty(boxCollider);
                        AssetDatabase.SaveAssetIfDirty(boxCollider);
                    }
                    catch {

                    }
                }
            }
            catch {
                
            }
        }
    }
}
