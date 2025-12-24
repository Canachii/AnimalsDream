using UnityEngine;

public class MaterialHandler : MonoBehaviour
{
    [SerializeField] private Material translucentMaterial;

    private SkinnedMeshRenderer smr;
    private Material originalMaterial;

    private void Awake()
    {
        smr = GetComponentInChildren<SkinnedMeshRenderer>();
        
        if (smr == null)
        {
            Debug.LogError("[MaterialHandler] SkinnedMeshRenderer not found in children.");
            return;
        }

        originalMaterial = smr.sharedMaterial;
    }

    public void ApplyTranslucent()
    {
        if (smr == null) return;
        if (translucentMaterial == null)
        {
            Debug.LogWarning("[MaterialHandler] translucentMaterial is not assigned.");
            return;
        }

        smr.sharedMaterial = translucentMaterial;
    }
    public void ApplyOriginal()
    {
        if (smr == null) return;
        if (originalMaterial == null ) return;

        smr.sharedMaterial = originalMaterial;
    }


}
