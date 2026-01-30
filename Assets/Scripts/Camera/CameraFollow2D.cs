using UnityEngine;

public class CameraFollow2D : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float smooth = 10f;

    private void LateUpdate()
    {
        if (!target) return;
        var desired = new Vector3(target.position.x, target.position.y, transform.position.z);
        transform.position = Vector3.Lerp(transform.position, desired, 1f - Mathf.Exp(-smooth * Time.deltaTime));
    }
}
