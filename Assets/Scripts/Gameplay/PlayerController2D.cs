using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController2D : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;

    private Rigidbody2D rb;
    private Vector2 input;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        if (input.sqrMagnitude > 1f) input.Normalize();
        
        // Debug - Geçici olarak input kontrolü
        if (input.magnitude > 0)
            Debug.Log($"Input: {input}, Position: {rb.position}");
    }

    private void FixedUpdate()
    {
        Vector2 next = rb.position + input * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(next);
    }
}
