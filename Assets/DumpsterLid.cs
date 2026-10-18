using UnityEngine;

public class DumpsterLid : MonoBehaviour
{
    public Transform playerHead;
    public Transform[] lids;
    public Quaternion[] closedRotations;
    public Quaternion[] openRotations;
    public float openDistance = 3f;
    public float closeDistance = 3.5f;
    public float degreesPerSecond = 120f;
    public bool manualPreview;
    public bool previewOpen;
    private bool open;

    private void Start()
    {
        for (int i = 0; i < lids.Length; i++)
            if (lids[i] != null) lids[i].localRotation = closedRotations[i];
    }

    private void Update()
    {
        if (manualPreview) open = previewOpen;
        else
        {
        if (playerHead == null && Camera.main != null) playerHead = Camera.main.transform;
        if (playerHead == null) return;
        Vector3 delta = playerHead.position - transform.position;
        delta.y = 0f;
        float distance = delta.magnitude;
        if (distance < openDistance) open = true;
        else if (distance > closeDistance) open = false;
        }
        for (int i = 0; i < lids.Length; i++)
            if (lids[i] != null)
                lids[i].localRotation = Quaternion.RotateTowards(lids[i].localRotation,
                    open ? openRotations[i] : closedRotations[i], degreesPerSecond * Time.deltaTime);
    }

    [ContextMenu("Preview Open Lid")]
    private void PreviewOpen() { SetPreview(true); }

    [ContextMenu("Preview Closed Lid")]
    private void PreviewClosed() { SetPreview(false); }

    [ContextMenu("Resume Automatic Lid")]
    private void ResumeAutomatic() { manualPreview = false; }

    private void SetPreview(bool value)
    {
        manualPreview = true;
        previewOpen = value;
        for (int i = 0; i < lids.Length; i++)
            if (lids[i] != null) lids[i].localRotation = value ? openRotations[i] : closedRotations[i];
    }
}
