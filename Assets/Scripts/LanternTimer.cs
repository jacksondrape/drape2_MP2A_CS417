using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class LanternTimer : MonoBehaviour
{
    [SerializeField, Min(0.1f)] private float initialDuration = 60f;
    [SerializeField] private GameObject fireObject;
    [SerializeField] private Collider resetObjectCollider;

    private float remainingTime;
    private bool timerExpired;

    public float RemainingTime => remainingTime;
    public bool IsExpired => timerExpired;

    private void Awake()
    {
        remainingTime = initialDuration;

        if (fireObject != null)
            fireObject.SetActive(true);
    }

    private void Update()
    {
        if (timerExpired) return;

        // Explicit Euler integration:
        // t(n+1) = t(n) - deltaTime
        remainingTime -= Time.deltaTime;

        if (remainingTime <= 0f)
        {
            remainingTime = 0f;
            timerExpired = true;

            if (fireObject != null)
                fireObject.SetActive(false);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (MatchesResetObject(collision.collider))
            ResetTimer();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (MatchesResetObject(other))
            ResetTimer();
    }

    private bool MatchesResetObject(Collider other)
    {
        if (other == null || resetObjectCollider == null)
            return false;

        return other == resetObjectCollider ||
               other.transform.IsChildOf(
                   resetObjectCollider.transform);
    }

    public void ResetTimer()
    {
        remainingTime = initialDuration;
        timerExpired = false;

        if (fireObject != null)
            fireObject.SetActive(true);
    }
}