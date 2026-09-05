using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class DisappearTrap : TriggerTrapController
{
    [SerializeField] private float destroyTime = 0.6f;
    [SerializeField] private float spawnTime = 3f;

    private Color originColor;
    private Color midColor = Color.yellow;
    private Color finalColor = Color.red;

    private Coroutine routine;

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

        if (IsServer)
        {
            StartOnServer();
        }
        else
        {
            RequestStart_ServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestStart_ServerRpc(ServerRpcParams rpcParams = default)
    {
        StartOnServer();
    }
    private void StartOnServer()
    {
        if (isProcessing) return;
        isProcessing = true;

        double t0 = NetworkManager.Singleton != null ? NetworkManager.Singleton.ServerTime.Time : 0;

        StartDisappear_ClientRpc(t0);

        if (IsServer && !IsClient)
            StartLocalSequence(t0);
    }


    [ClientRpc]
    private void StartDisappear_ClientRpc(double serverStartTime)
    {
        StartLocalSequence(serverStartTime);
    }

    private void StartLocalSequence(double serverStartTime)
    {
        if (routine != null) StopCoroutine(routine);

        float elapsed = 0f;
        if (NetworkManager.Singleton != null)
        {
            double now = NetworkManager.Singleton.ServerTime.Time;
            elapsed = Mathf.Max(0f, (float)(now - serverStartTime));
        }

        routine = StartCoroutine(DisappearRoutine(elapsed));
    }

    protected override void OnTrapTriggered(Collider other) { }


    private IEnumerator DisappearRoutine(float elapsed)
    {
        float half = destroyTime * 0.5f;
        float tYellowEnd = half;
        float tRedEnd = destroyTime;
        float tHiddenEnd = destroyTime + spawnTime;

        // Yellow
        if (elapsed < tYellowEnd)
        {
            SetColor(midColor);
            yield return new WaitForSeconds(tYellowEnd - elapsed);
            elapsed = tYellowEnd;
        }

        // Red
        if (elapsed < tRedEnd)
        {
            SetColor(finalColor);
            yield return new WaitForSeconds(tRedEnd - elapsed);
            elapsed = tRedEnd;
        }

        // Hidden
        if (elapsed < tHiddenEnd)
        {
            SetVisible(false);
            yield return new WaitForSeconds(tHiddenEnd - elapsed);
            elapsed = tHiddenEnd;
        }

        // Restore
        SetColor(originColor);
        SetVisible(true);

        isProcessing = false;
        routine = null;
    }
    private void SetVisible(bool on)
    {
        if (trapCollider != null) trapCollider.enabled = on;
        if (trapRenderer != null) trapRenderer.enabled = on;
    }

    private void SetColor(Color c)
    {
        if (trapRenderer == null) return;
        trapRenderer.material.color = c; 
    }
}
