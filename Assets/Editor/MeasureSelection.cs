using UnityEditor;
using UnityEngine;

// Put this file in Assets/Editor.
// Select a scene object, then run Tools > PV > Measure Selection.
public static class MeasureSelection
{
    private static Transform measuredRoot;
    private static Bounds measuredBounds;

    [MenuItem("Tools/PV/Measure Selection")]
    private static void Measure()
    {
        Transform root = Selection.activeTransform;
        if (root == null || !root.gameObject.scene.IsValid())
        {
            Debug.LogWarning("Select an object in the scene Hierarchy first.");
            return;
        }

        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            Debug.LogWarning("Stop Play Mode before measuring.");
            return;
        }

        Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
        bool hasBounds = false;
        Bounds total = new Bounds();
        int included = 0;
        int skipped = 0;

        foreach (Renderer renderer in renderers)
        {
            if (!renderer.enabled || !renderer.gameObject.activeInHierarchy)
            {
                skipped++;
                continue;
            }

            Bounds current = renderer.bounds; // Already in world space.
            if (!hasBounds)
            {
                total = current;
                hasBounds = true;
            }
            else
            {
                total.Encapsulate(current);
            }

            included++;
        }

        if (!hasBounds)
        {
            Debug.LogWarning("No active Renderer found under: " + root.name, root);
            return;
        }

        measuredRoot = root;
        measuredBounds = total;
        SceneView.duringSceneGui -= DrawBounds;
        SceneView.duringSceneGui += DrawBounds;
        SceneView.RepaintAll();

        string path = root.name;
        for (Transform parent = root.parent; parent != null; parent = parent.parent)
            path = parent.name + "/" + path;

        Debug.Log(
            $"[MEASURE] {path}\n" +
            $"World Size XYZ (units): {total.size.ToString("F4")}\n" +
            $"World Min: {total.min.ToString("F4")}\n" +
            $"World Max: {total.max.ToString("F4")}\n" +
            $"Root Local Scale: {root.localScale.ToString("F4")}\n" +
            $"Root World Scale (lossy): {root.lossyScale.ToString("F4")}\n" +
            $"Root World Rotation: {root.eulerAngles.ToString("F2")}\n" +
            $"Renderers: included={included}, skipped={skipped}\n" +
            "World-axis renderer bounds. Includes visible hats/accessories. " +
            "Room bounds include contents, not just walkable space.", root);
    }

    private static void DrawBounds(SceneView sceneView)
    {
        if (measuredRoot == null || Selection.activeTransform != measuredRoot)
            return;

        Color previousColor = Handles.color;
        Handles.color = Color.yellow;
        Handles.DrawWireCube(measuredBounds.center, measuredBounds.size);
        Handles.color = previousColor;
    }
}
