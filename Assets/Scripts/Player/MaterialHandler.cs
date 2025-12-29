using UnityEngine;
using System.Collections.Generic;

public class MaterialHandler : MonoBehaviour
{
    [SerializeField] private Material translucentMaterial;
    [SerializeField] private LayerMask excludeLayers;

    private SkinnedMeshRenderer[] targets;     
    private Material[][] originalMaterials;    

    private void Awake()
    {
        var all = GetComponentsInChildren<SkinnedMeshRenderer>(true);

        if (all == null || all.Length == 0)
        {
            Debug.LogError("[MaterialHandler] SkinnedMeshRenderer not found in children.");
            return;
        }

        List<SkinnedMeshRenderer> list = new List<SkinnedMeshRenderer>(all.Length);
        foreach (var r in all)
        {
            if (((1 << r.gameObject.layer) & excludeLayers) != 0)
                continue;

            list.Add(r);
        }

        targets = list.ToArray();
        originalMaterials = new Material[targets.Length][];

        for (int i = 0; i < targets.Length; i++)
            originalMaterials[i] = targets[i].sharedMaterials;
    }

    public void ApplyTranslucent()
    {
        if (targets == null || targets.Length == 0) return;
        if (translucentMaterial == null)
        {
            Debug.LogWarning("[MaterialHandler] translucentMaterial is not assigned.");
            return;
        }

        foreach (var r in targets)
        {
            var mats = r.sharedMaterials; 
            for (int i = 0; i < mats.Length; i++)
                mats[i] = translucentMaterial;

            r.sharedMaterials = mats;
        }
    }

    public void ApplyOriginal()
    {
        if (targets == null || targets.Length == 0) return;
        if (originalMaterials == null) return;

        for (int i = 0; i < targets.Length; i++)
            targets[i].sharedMaterials = originalMaterials[i];
    }
}
