using System.Collections;
using UnityEngine;

public class DisappearTrap : TriggerTrapController
{
    [SerializeField] private float destroyTime = 0.6f;
    [SerializeField] private float spawnTime = 3f;

    private Color originColor;
    private Color midColor = Color.yellow;
    private Color finalColor = Color.red;

    protected override void Awake()
    {
        base.Awake();
        if (trapRenderer != null)
            originColor = trapRenderer.material.color;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (isProcessing) return;
        if (!IsTarget(collision.collider)) return;

        OnTrapTriggered(collision.collider);
    }

    protected override void OnTrapTriggered(Collider other)
    {
        if (!isProcessing)
            StartCoroutine(DisappearRoutine());
    }

    private IEnumerator DisappearRoutine()
    {
        isProcessing = true;
        float half = destroyTime * 0.5f;

        trapRenderer.material.color = midColor;
        yield return new WaitForSeconds(half);

        trapRenderer.material.color = finalColor;
        yield return new WaitForSeconds(half);

        trapCollider.enabled = false;
        trapRenderer.enabled = false;

        yield return new WaitForSeconds(spawnTime);

        trapRenderer.material.color = originColor;
        trapCollider.enabled = true;
        trapRenderer.enabled = true;

        isProcessing = false;
    }
}
