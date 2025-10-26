using SLZ.Interaction;
using SLZ.Marrow;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Grip), true)]
public class GripGizmo : Editor
{
    // Credits
    // Maranara and GreasedScottsman - Original GripGizmo.
    // notnotnotswipez - Modification and porting to Bonelab.

    public static GameObject brettPrefab;
    static GameObject brettInstance;
    static Animator animatorInstance;
    static Transform wrist;
    static Transform HAND;
    static Transform gripPoint;
    public const string animHandednessPrefix = "R_";
    public const string rigHandednessPrefix = "r_";
    string lastHandPose;

    public bool showGizmoLabels = true;
    public bool destroyHandGizmoOnDeselection = true;
    public bool showHandGizmo = true;

    // Barrel Grip positioning
    public float barrelPosition = 0f;
    public float cylinderGripSlide = 0f;

    // When migrating this system from Boneworks to Bonelab, some of the grips are offset a bit from their original Boneworks counterpart. That or this system just never properly
    // Worked with target grips.
    public GripOffset gripOffset = new GripOffset();

    private void SetTargetGripOffset(string gripName)
    {
        switch (gripName)
        {
            case "ChargingHandle":
                gripOffset.posOffset = new Vector3(0, 0.67f, 0.02f);
                gripOffset.rotOffset = new Vector3(180, -100, 0);
                break;
            case "Glock":
                gripOffset.posOffset = new Vector3(0.01f, 0.63f, -0.006f);
                gripOffset.rotOffset = new Vector3(180, 0, 4);
                break;
            case "GlockSlidePar":
                gripOffset.posOffset = new Vector3(-0.014f, 0.34f, 0.35f);
                gripOffset.rotOffset = new Vector3(229.57f, 90.4f, -83.3f);
                break;
            case "GlockMagPalmed":
                gripOffset.posOffset = new Vector3(0.01f, 0.62f, -0.02f);
                gripOffset.rotOffset = new Vector3(180, 0, 0);
                break;
            case "GlockSlidePerp":
                gripOffset.posOffset = new Vector3(-0.17f, 0.03f, -0.1f);
                gripOffset.rotOffset = new Vector3(200, 0, -149.7f);
                break;
            case "OffGlock":
                gripOffset.posOffset = new Vector3(0.04f, 0.61f, -0.04f);
                gripOffset.rotOffset = new Vector3(180, 4, 6);
                break;
            case "RifleMag":
                gripOffset.posOffset = new Vector3(0, 0.64f, -0.012f);
                gripOffset.rotOffset = new Vector3(180, 0, 0);
                break;
            default:
                gripOffset.posOffset = new Vector3(0.01f, 0.615f, -0.017f);
                gripOffset.rotOffset = new Vector3(180, 0, 0);
                break;
        }
    }

    private void SetCylinderGripOffset(string gripName)
    {
        switch (gripName)
        {
            case "R_cylinder":
                gripOffset.posOffset = new Vector3(-0.24f, -1.31f, 0);
                gripOffset.rotOffset = new Vector3(0, 0, 0);
                break;
            case "R_halfCylinder":
                gripOffset.posOffset = new Vector3(-0.38f, -1f, 0);
                gripOffset.rotOffset = new Vector3(0, 0, 0);
                break;
            case "R_ball":
                gripOffset.posOffset = new Vector3(-0.38f, -1f, 0);
                gripOffset.rotOffset = new Vector3(0, 0, 0);
                break;
            case "R_barrelGrip":
                gripOffset.posOffset = new Vector3(0.3f, 0.58f, 0.1f);
                gripOffset.rotOffset = new Vector3(-18.37f, 0, 0);
                break;
            default:
                gripOffset.posOffset = new Vector3(0f, 0f, 0f);
                gripOffset.rotOffset = new Vector3(0f, 0f, 0f);
                break;
        }
    }

    public override void OnInspectorGUI()
    {       
        Grip grip = (Grip)target;

        GUILayout.Label("Grip Hand Gizmo Options", EditorStyles.boldLabel);
        showHandGizmo = EditorGUILayout.Toggle("Show Hand Gizmo", showHandGizmo);
        destroyHandGizmoOnDeselection = EditorGUILayout.Toggle("Deselect Removes Hand Gizmo", destroyHandGizmoOnDeselection);

        // DEBUG FOR DEVELOPMENT
        //gripOffset.posOffset = EditorGUILayout.Vector3Field("Grip pos offset", gripOffset.posOffset);
        //gripOffset.rotOffset = EditorGUILayout.Vector3Field("Grip rot offset", gripOffset.rotOffset);
        CylinderGrip cylinderGrip = grip.GetComponent<CylinderGrip>();
        if (cylinderGrip)
        {
            GUILayout.Label("Hand Gizmo Slide Amount (Visual Only)", EditorStyles.boldLabel);
            cylinderGripSlide = EditorGUILayout.Slider(cylinderGripSlide, -cylinderGrip.limit, cylinderGrip.limit);
        }

        GUILayout.Space(10f);
        bool handled = false;

        
        if (cylinderGrip)
        {
            handled = true;
            if (animatorInstance != null)
            {
                animatorInstance.SetFloat(animHandednessPrefix + "Radius", grip.radius);
                animatorInstance.Update(Time.deltaTime);
            }

            try
            {
                if (animatorInstance != null && lastHandPose != GetRemappedName(grip.handPose.name))
                {
                    animatorInstance.ResetTrigger(lastHandPose);
                    animatorInstance.SetTrigger(GetRemappedName(grip.handPose.name));
                    lastHandPose = GetRemappedName(grip.handPose.name);
                    SetCylinderGripOffset(GetRemappedName(grip.handPose.name));
                }
            }
            catch
            {

            }


            if (brettInstance != null)
            {
                HideOtherHand();

                Transform target = grip.targetTransform;
                if (target == null)
                    target = grip.transform;

                Vector3 regularOffsetPosition = new Vector3(gripOffset.posOffset.x * grip.radius,
                    gripOffset.posOffset.y * grip.radius, cylinderGripSlide + gripOffset.posOffset.z);

                wrist.transform.position = target.transform.TransformPoint(regularOffsetPosition) + (target.right * grip.radius) - (target.up * 0.3f);
                wrist.transform.rotation = (target.rotation * Quaternion.Euler(gripOffset.rotOffset) * Quaternion.Euler(new Vector3(0f, 0f, 90f)));
            }
        }
        // New for Bonelab version of GripGizmo
        else if (grip.GetComponent<TargetGrip>())
        {
            handled = true;
            if (animatorInstance != null)
            {
                animatorInstance.SetFloat(animHandednessPrefix + "Radius", grip.radius);
                animatorInstance.Update(Time.deltaTime);
            }

            try
            {

                if (animatorInstance != null && lastHandPose != GetRemappedName(grip.handPose.name))
                {
                    animatorInstance.ResetTrigger(lastHandPose);
                    animatorInstance.SetTrigger(GetRemappedName(grip.handPose.name));
                    lastHandPose = GetRemappedName(grip.handPose.name);
                    SetTargetGripOffset(grip.handPose.name);
                }
            }
            catch
            {

            }


            if (brettInstance != null)
            {
                HideOtherHand();

                Transform target = grip.targetTransform;
                if (target == null)
                    target = grip.transform;

                if (gripOffset != null)
                {
                    wrist.transform.position = target.transform.TransformPoint(gripOffset.posOffset) + (target.right * grip.radius) - (target.up * 0.3f);
                    wrist.transform.rotation = (target.rotation * Quaternion.Euler(gripOffset.rotOffset) * Quaternion.Euler(new Vector3(0f, 0f, 90f)));
                }
            }
        }

        

        if (grip.GetComponent<SphereGrip>())
        {
            handled = true;
            if (animatorInstance != null)
            {
                animatorInstance.SetFloat(animHandednessPrefix + "Radius", grip.radius);
                animatorInstance.Update(Time.deltaTime);
            }

            if (animatorInstance != null && lastHandPose != GetRemappedName(grip.handPose.name))
            {
                animatorInstance.ResetTrigger(lastHandPose);
                animatorInstance.SetTrigger(GetRemappedName(grip.handPose.name));
                lastHandPose = GetRemappedName(grip.handPose.name);
            }

            if (brettInstance != null)
            {
                HideOtherHand();

                wrist.transform.position = grip.transform.position + (grip.transform.right * grip.radius) - (grip.transform.up * 0.3f);
                wrist.transform.rotation = grip.transform.rotation * Quaternion.Euler(new Vector3(0f, 0f, 90f));
            }
        }

        if (grip.GetComponent<BarrelGrip>())
        {
            handled = true;
            CapsuleCollider capCol = grip.GetComponent<CapsuleCollider>();
            float barrelHalfHeight = (capCol.height / 2);
            float barrelRadius = capCol.radius;
            BarrelGrip barrelGrip = grip.GetComponent<BarrelGrip>();
            barrelPosition = EditorGUILayout.Slider("Hand Position:", barrelPosition, 0, capCol.height);
            GUILayout.Space(10f);
            float distanceFromCenterAlongSpine = barrelGrip.transform.position.y * barrelPosition;
            float distanceFromSpineAlongCurve = barrelGrip.heightAndRadiusCurve.Evaluate(distanceFromCenterAlongSpine) * barrelRadius;

            if (animatorInstance != null)
            {
                if (!barrelGrip.isCurveOverride)
                {
                    animatorInstance.SetFloat(animHandednessPrefix + "Radius", grip.radius);
                    animatorInstance.Update(Time.deltaTime);
                }
            }

            try
            {
                if (animatorInstance != null && lastHandPose != GetRemappedName(barrelGrip.edgeHandPose.name))
                {
                    animatorInstance.ResetTrigger(lastHandPose);
                    animatorInstance.SetTrigger(GetRemappedName(barrelGrip.edgeHandPose.name));
                    lastHandPose = GetRemappedName(barrelGrip.edgeHandPose.name);
                }
            } catch
            {

            }

            

            if (brettInstance != null)
            {
                HideOtherHand();

                if (barrelGrip.isCurveOverride)
                {
                    wrist.transform.position = (grip.transform.position - (grip.transform.up * barrelHalfHeight / 2)) + (grip.transform.up * distanceFromCenterAlongSpine) + (grip.transform.right * distanceFromSpineAlongCurve * 2.2f) - (grip.transform.forward * 0.3f);
                    wrist.transform.rotation = grip.transform.rotation * Quaternion.Euler(new Vector3(90f, 0f, 90f));
                }
                else
                {
                    wrist.transform.position = grip.transform.position + (grip.transform.right * grip.radius) - (grip.transform.up * 0.3f);
                    wrist.transform.rotation = grip.transform.rotation * Quaternion.Euler(new Vector3(0f, 0f, 90f));
                }
            }

        }

        if (grip.GetComponent<BoxGrip>())
        {
            handled = false;
        }

        if (brettInstance)
        {
            brettInstance.SetActive(true);
            animatorInstance.ResetTrigger(lastHandPose);

            if (grip.handPose)
            {
                animatorInstance.SetTrigger(GetRemappedName(grip.handPose.name));
            }

            if (showHandGizmo == true)
            {
                brettInstance.SetActive(handled);
            }
            else
            {
                brettInstance.SetActive(false);
            }
        }

        base.OnInspectorGUI();

    }

    private string GetRemappedName(string original)
    {
        switch (original)
        {
            case "AngleForeGrip":
                return "R_angledForeGrip";
            case "ballHandPose":
                return "R_ball";
            case "SphereGrip":
                return "R_ball";
            case "BarrelGrip":
                return "R_barrelGrip";
            case "PinchGrip":
                return "R_pinch";
            case "ChargingHandle":
                return "R_M16_ChargingHandle";
            case "CylinderGripSmall":
                return "R_cylinder";
            case "CylinderGripLarge":
                return "R_cylinder";
            case "Glock":
                return "R_glock";
            case "GlockMag":
                return "R_glockMag";
            case "GlockMagPalmed":
                return "R_glockMag_Palmed";
            case "GlockSlidePar":
                return "R_glockSlide_Para";
            case "GlockSlidePerp":
                return "R_glockSlide_Perp";
            case "HalfCylinder":
                return "R_halfCylinder";
            case "LargeHalfCylinder":
                return "R_halfCylinder";
            case "M16":
                return "R_M16";
            case "M16ForwardHandMag":
                return "R_M16_ForwardHandMag";
            case "OffGlock":
                return "R_offGlock";
            case "RifleMag":
                return "R_rifleMag";
            case "SoftGrab":
                return "R_softGrab";
        }

        return "R_Neutral";
    }

    private void OnSceneGUI()
    {
        Grip grip = (Grip)target;

        Color cyanAlpha2 = Color.cyan;
        cyanAlpha2.a = 0.2f;

        Color yellowAlpha2 = Color.yellow;
        yellowAlpha2.a = 0.2f;

        if (grip.GetComponent<CylinderGrip>())
        {
            CylinderGrip cylinderGrip = (CylinderGrip)target;

            Handles.color = Color.cyan;

            if (cylinderGrip != null)
            {
                float scaleFactor = 1.0f;
                InteractableHost host = cylinderGrip.GetComponentInParent<InteractableHost>();
                if (host)
                {
                    scaleFactor = host.transform.localScale.y;
                }

                Transform target = grip.targetTransform;
                if (target == null)
                    target = grip.transform;

                Vector3 limit = (target.forward * (cylinderGrip.limit) * scaleFactor);
                Handles.DrawWireDisc(target.position - limit, target.forward, grip.radius);
                Handles.DrawWireDisc(target.position + limit, target.forward, grip.radius);
                Handles.color = Color.Lerp(Color.cyan, Color.white, 0.6f);
                Handles.DrawLine(target.position + limit, target.position - limit);
                Handles.color = Color.Lerp(Color.cyan, Color.red, 0.6f);
                Handles.DrawWireDisc(target.position, target.forward, grip.radius);
            }
            else
            {
                Handles.DrawWireDisc(grip.targetTransform.position, grip.targetTransform.forward, grip.radius);
                Handles.DrawWireDisc(grip.targetTransform.position, grip.targetTransform.up, grip.radius);
            }
        }

        if (grip.GetComponent<SphereGrip>())
        {
            SphereGrip sphereGrip = (SphereGrip)target;

            Handles.color = Color.cyan;

            if (sphereGrip != null)
            {                
                Handles.DrawWireDisc(grip.transform.position, grip.transform.forward, grip.radius);
            }
        }

        SceneView.RepaintAll();
    }

    private void OnEnable()
    {
        if (brettPrefab == null)
        {
            brettPrefab = Resources.Load<GameObject>("GripGizmos/GripHands");
        }
        if (brettInstance == null)
        {
            brettInstance = Instantiate(brettPrefab);
            animatorInstance = brettInstance.GetComponent<Animator>();
        }

        wrist = brettInstance.transform.Find("SHJntGrp/MAINSHJnt/ROOTSHJnt/Spine_01SHJnt/Spine_02SHJnt/Spine_TopSHJnt/" + rigHandednessPrefix+ "Arm_ClavicleSHJnt/" + rigHandednessPrefix + "AC_AuxSHJnt/" + rigHandednessPrefix + "Arm_ShoulderSHJnt/" + rigHandednessPrefix + "Arm_Elbow_CurveSHJnt/");
        HAND = wrist.transform.Find(rigHandednessPrefix + "WristSHJnt");
        gripPoint = HAND.transform.Find(rigHandednessPrefix + "Hand_1SHJnt/" + rigHandednessPrefix + "Hand_2SHJnt/" + rigHandednessPrefix+ "GripPoint_AuxSHJnt");
    }

    private void OnDisable()
    {
        if (brettInstance != null && destroyHandGizmoOnDeselection == true)
        {
            DestroyImmediate(brettInstance);
        }            
    }

    private void HideOtherHand()
    {
        Transform otherHandGeo;

        if (animHandednessPrefix == "R_")
        {
            otherHandGeo = brettInstance.transform.Find("geoGrp/brett_l_hand");
        }
        else
        {
            otherHandGeo = brettInstance.transform.Find("geoGrp/brett_r_hand");
        }

        otherHandGeo.gameObject.SetActive(false);
    }

}

public class GripOffset
{
    public Vector3 posOffset = new Vector3(0, 0.61f, -0.017f);
    public Vector3 rotOffset = new Vector3(180, 0, 0);
}
