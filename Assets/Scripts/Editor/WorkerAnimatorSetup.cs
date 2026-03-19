using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Editor utility that:
///   1. Builds WorkerAnimatorController with a 1D Speed blend tree
///      (Idle → Walk → Run) using newcharidle / newcharwalk / newcharrun FBX clips.
///   2. Rebuilds Worker.prefab from Ch17_nonPBR.fbx (mesh + skin)
///      and attaches NavMeshAgent, CapsuleCollider, AgentController.
///
/// Run via: Tools → Worker → Setup Animator + Prefab
/// </summary>
public static class WorkerAnimatorSetup
{
    // ── Asset paths ──────────────────────────────────────────────────────────
    private const string ControllerPath = "Assets/Animations/WorkerAnimatorController.controller";

    // Animation source FBXes — each contains one clip named "mixamo.com"
    private const string IdleFbxPath   = "Assets/newcharidle.fbx";
    private const string WalkFbxPath   = "Assets/newcharwalk.fbx";
    private const string RunFbxPath    = "Assets/newcharrun.fbx";

    // Mesh / skin source FBX — Ch17_nonPBR provides the rigged character
    private const string MeshFbxPath   = "Assets/Ch17_nonPBR.fbx";

    private const string PrefabPath    = "Assets/Prefabs/Worker.prefab";

    [MenuItem("Tools/Worker/Setup Animator + Prefab")]
    public static void Setup()
    {
        if (!BuildController()) return;
        if (!BuildPrefab())     return;

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[WorkerAnimatorSetup] Done — Worker.prefab and WorkerAnimatorController rebuilt.");
    }

    // ─────────────────────────────────────────────────────────────────────────
    // 1. Animator Controller
    // ─────────────────────────────────────────────────────────────────────────

    private static bool BuildController()
    {
        // Load the first AnimationClip sub-asset from each FBX (the raw "mixamo.com" clip).
        AnimationClip idleClip = LoadFirstClip(IdleFbxPath);
        AnimationClip walkClip = LoadFirstClip(WalkFbxPath);
        AnimationClip runClip  = LoadFirstClip(RunFbxPath);

        if (idleClip == null || walkClip == null || runClip == null)
        {
            Debug.LogError("[WorkerAnimatorSetup] One or more animation clips not found. " +
                           "Ensure newcharidle.fbx, newcharwalk.fbx, newcharrun.fbx are imported.");
            return false;
        }

        // Create or overwrite the controller asset.
        AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
        if (controller == null)
            controller = AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);

        // Ensure Speed float parameter.
        if (!controller.parameters.Any(p => p.name == "Speed"))
            controller.AddParameter("Speed", AnimatorControllerParameterType.Float);

        // Clear all existing states.
        AnimatorStateMachine rootSM = controller.layers[0].stateMachine;
        foreach (ChildAnimatorState s in rootSM.states.ToArray())
            rootSM.RemoveState(s.state);

        // Create Locomotion state with a 1D blend tree driven by Speed.
        AnimatorState locomotionState = controller.CreateBlendTreeInController("Locomotion", out BlendTree tree);
        tree.blendType              = BlendTreeType.Simple1D;
        tree.blendParameter         = "Speed";
        tree.useAutomaticThresholds = false;
        tree.AddChild(idleClip, 0f);    // Speed == 0   → Idle
        tree.AddChild(walkClip, 2f);    // Speed == 2   → Walk
        tree.AddChild(runClip,  5.5f);  // Speed == 5.5 → Run

        rootSM.defaultState = locomotionState;

        EditorUtility.SetDirty(controller);
        Debug.Log($"[WorkerAnimatorSetup] Controller built: {ControllerPath}");
        return true;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // 2. Prefab
    // ─────────────────────────────────────────────────────────────────────────

    private static bool BuildPrefab()
    {
        GameObject meshSource = AssetDatabase.LoadAssetAtPath<GameObject>(MeshFbxPath);
        if (meshSource == null)
        {
            Debug.LogError($"[WorkerAnimatorSetup] Could not load mesh FBX at {MeshFbxPath}.");
            return false;
        }

        AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);

        // Load the Avatar sub-asset from Ch17_nonPBR.fbx.
        Avatar avatar = null;
        foreach (Object obj in AssetDatabase.LoadAllAssetRepresentationsAtPath(MeshFbxPath))
        {
            if (obj is Avatar a) { avatar = a; break; }
        }
        if (avatar == null)
            Debug.LogWarning($"[WorkerAnimatorSetup] No Avatar found in {MeshFbxPath}. " +
                             "Set 'Avatar Definition → Create From This Model' in the FBX importer.");

        // ── Create a plain root GameObject ────────────────────────────────────
        // We must NOT use PrefabUtility.InstantiatePrefab on an FBX because model
        // prefab instances are read-only — AddComponent on them fails at runtime.
        // Instead: create a blank root, instantiate the FBX mesh as a child.
        GameObject root = new GameObject("Worker");

        try
        {
            // Instantiate the FBX model as a child so we get the skinned mesh/skeleton.
            GameObject meshChild = (GameObject)PrefabUtility.InstantiatePrefab(meshSource);
            meshChild.transform.SetParent(root.transform, false);
            meshChild.transform.localPosition = Vector3.zero;
            meshChild.transform.localRotation = Quaternion.identity;
            meshChild.transform.localScale    = Vector3.one;

            // ── NavMeshAgent on the root ──────────────────────────────────────
            NavMeshAgent nav      = root.AddComponent<NavMeshAgent>();
            nav.radius            = 0.3f;
            nav.height            = 1.8f;
            nav.speed             = 2f;
            nav.acceleration      = 8f;
            nav.angularSpeed      = 180f;
            nav.stoppingDistance  = 0.1f;
            nav.autoBraking       = true;
            nav.autoRepath        = true;
            nav.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;
            nav.avoidancePriority = 50;
            nav.baseOffset        = 0f;

            // ── CapsuleCollider on the root ───────────────────────────────────
            CapsuleCollider col = root.AddComponent<CapsuleCollider>();
            col.center    = new Vector3(0f, 0.9f, 0f);
            col.radius    = 0.3f;
            col.height    = 1.8f;
            col.direction = 1; // Y-axis

            // ── Animator on the root ──────────────────────────────────────────
            // Remove any Animator on the FBX child to avoid double-animating.
            Animator childAnim = meshChild.GetComponent<Animator>();
            if (childAnim != null) Object.DestroyImmediate(childAnim);

            Animator anim = root.AddComponent<Animator>();
            anim.runtimeAnimatorController = controller;
            anim.avatar          = avatar;
            anim.applyRootMotion = false;
            // AlwaysAnimate: bones keep updating even when the renderer is off-screen.
            anim.cullingMode     = AnimatorCullingMode.AlwaysAnimate;

            // ── AgentController on the root ───────────────────────────────────
            root.AddComponent<AgentController>();

            // ── Save as prefab ────────────────────────────────────────────────
            PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            Debug.Log($"[WorkerAnimatorSetup] Prefab saved at {PrefabPath}.");
            return true;
        }
        finally
        {
            Object.DestroyImmediate(root);
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Helpers
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>Returns the first AnimationClip sub-asset found in the given FBX path.</summary>
    private static AnimationClip LoadFirstClip(string fbxPath)
    {
        foreach (Object obj in AssetDatabase.LoadAllAssetRepresentationsAtPath(fbxPath))
        {
            if (obj is AnimationClip clip) return clip;
        }
        Debug.LogWarning($"[WorkerAnimatorSetup] No AnimationClip in '{fbxPath}'.");
        return null;
    }
}
