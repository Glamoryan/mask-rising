using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController2D : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float sprintMultiplier = 2f; // Koşma hız çarpanı
    [SerializeField] private bool facingRight = false; // Model başlangıçta sola bakıyorsa false

    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private Vector2 input;
    private bool isMoving;
    private bool isRunning; // Normal hareket (sprint değil)
    private bool isSprinting;
    private bool needsFlip = false; // Flip gerekiyor mu?

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        
        // SpriteRenderer'ı bul (hem parent'ta hem de child'larda ara)
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            Debug.Log("[PlayerController] SpriteRenderer child objesinde bulundu");
        }
        
        if (spriteRenderer == null)
        {
            Debug.LogWarning("[PlayerController] SpriteRenderer bulunamadı!");
        }
        else
        {
            Debug.Log($"[PlayerController] SpriteRenderer bulundu: {spriteRenderer.gameObject.name}");
        }
    }

    private void Update()
    {
        input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        if (input.sqrMagnitude > 1f) input.Normalize();
        
        // Sprint kontrolü (Shift tuşu)
        isSprinting = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        
        // Hareket durumunu kontrol et
        isMoving = input.magnitude > 0.01f;
        isRunning = isMoving && !isSprinting; // Normal hareket (sprint değil)
        
        // Sprite yönünü hareket yönüne göre çevir
        if (input.x != 0)
        {
            bool shouldFaceRight = input.x > 0;
            if (shouldFaceRight != facingRight)
            {
                facingRight = shouldFaceRight;
                needsFlip = true; // LateUpdate'de flip yapılacak
                Debug.Log($"[PlayerController] Yön değişecek: {(facingRight ? "SAĞ" : "SOL")}");
            }
        }
        
        // Animator'ı güncelle (eğer varsa)
        if (animator != null)
        {
            animator.SetBool("IsMoving", isMoving);
            animator.SetBool("IsRunning", isRunning);
            animator.SetBool("IsSprinting", isSprinting && isMoving);
            animator.SetFloat("Speed", input.magnitude * (isSprinting ? sprintMultiplier : 1f));
            
            // Alternatif: Animator'ı tamamen aç/kapat
            // animator.enabled = isMoving; // Bu satırı kullanırsanız, sadece hareket ederken animasyon çalışır
        }
    }
    
    private void LateUpdate()
    {
        // Animator'dan sonra çalışır, scale'i override eder
        if (needsFlip)
        {
            needsFlip = false;
            
            // Yöntem 1: Scale ile (daha yaygın)
            // Vector3 scale = transform.localScale;
            // scale.x = facingRight ? Mathf.Abs(scale.x) : -Mathf.Abs(scale.x);
            // transform.localScale = scale;
            
            // Yöntem 2: Rotation ile (Animator scale'i override ediyorsa bu çalışır)
            Vector3 rotation = transform.eulerAngles;
            rotation.y = facingRight ? 180f : 0f;
            transform.eulerAngles = rotation;
            
            Debug.Log($"[PlayerController] LateUpdate - Player rotation.y = {rotation.y}");
        }
    }

    private void FixedUpdate()
    {
        // Sprint durumuna göre hızı ayarla
        float currentSpeed = moveSpeed * (isSprinting ? sprintMultiplier : 1f);
        Vector2 next = rb.position + input * currentSpeed * Time.fixedDeltaTime;
        rb.MovePosition(next);
    }
    
    
    // Public getter for other scripts
    public bool IsMoving => isMoving;
    public bool IsRunning => isRunning;
    public bool IsSprinting => isSprinting;
    public bool FacingRight => facingRight;
}
