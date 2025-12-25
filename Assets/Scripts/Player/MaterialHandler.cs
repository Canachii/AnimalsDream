using UnityEngine;

public class MaterialHandler : MonoBehaviour
{
    [SerializeField] private Material translucentMaterial;

    private SkinnedMeshRenderer[] smr;
    private Material originalMaterial;

    private void Awake()
    {
        smr = GetComponentsInChildren<SkinnedMeshRenderer>();
        
        if (smr == null)
        {
            Debug.LogError("[MaterialHandler] SkinnedMeshRenderer not found in children.");
            return;
        }

        originalMaterial = smr[0].sharedMaterial;
    }

    public void ApplyTranslucent()
    {
        if (smr == null) return;
        if (translucentMaterial == null)
        {
            Debug.LogWarning("[MaterialHandler] translucentMaterial is not assigned.");
            return;
        }

        foreach (var t in smr)
        {
            var mats = t.materials;
            for (int i = 0; i < mats.Length; i++)
                mats[i] = translucentMaterial;
            t.materials = mats;
        }
    }
    public void ApplyOriginal()
    {
        if (smr == null) return;
        if (originalMaterial == null ) return;

        foreach (var t in smr)
        {
            var mats = t.materials;
            for (int i = 0; i < mats.Length; i++)
                mats[i] = originalMaterial;
            t.materials = mats;
        }
    }
}
