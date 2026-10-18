using UnityEngine;

public class RatMovementVisual : MonoBehaviour
{
    public Transform visual;
    public Animator animator;
    public float turnSpeed = 540f;
    public float facingOffset;
    private Vector3 lastPosition;

    private void Start() { lastPosition = transform.position; }
    private void LateUpdate()
    {
        Vector3 movement = transform.position - lastPosition;
        lastPosition = transform.position;
        movement.y = 0f;
        bool moving = movement.sqrMagnitude > 0.0000001f;
        if (animator != null) animator.speed = moving ? 1f : 0f;
        if (moving && visual != null)
            visual.rotation = Quaternion.RotateTowards(visual.rotation,
                Quaternion.LookRotation(movement) * Quaternion.Euler(0f, facingOffset, 0f), turnSpeed * Time.deltaTime);
    }
}
