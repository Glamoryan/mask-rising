using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController2D : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;

    private Rigidbody2D rb;
    private Animator animator;
    private Vector2 input;
    private bool isMoving;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        if (input.sqrMagnitude > 1f) input.Normalize();
        
        // Hareket durumunu kontrol et
        isMoving = input.magnitude > 0.01f;
        
        // Animator'ı güncelle (eğer varsa)
        if (animator != null)
        {
            animator.SetBool("IsMoving", isMoving);
            animator.SetFloat("Speed", input.magnitude);
            
            // Alternatif: Animator'ı tamamen aç/kapat
            // animator.enabled = isMoving; // Bu satırı kullanırsanız, sadece hareket ederken animasyon çalışır
        }
    }

    private void FixedUpdate()
    {
        Vector2 next = rb.position + input * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(next);
    }
    
    // Public getter for other scripts
    public bool IsMoving => isMoving;
}
