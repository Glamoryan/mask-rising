using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections;

/// <summary>
/// Player'ın farklı tile'lar arasında geçişte hop/atlama efekti
/// </summary>
public class TileHopEffect : MonoBehaviour
{
    [Header("Tilemap References")]
    [SerializeField] private Tilemap groundTilemap; // Ground tilemap referansı
    
    [Header("Hop Effect Settings")]
    [SerializeField] private bool enableHopEffect = false; // HOP KAPALI - Sadece wind efekt
    [SerializeField] private float hopHeight = 0.3f; // Ne kadar yüksek zıplar
    [SerializeField] private float hopDuration = 0.2f; // Zıplama süresi
    [SerializeField] private AnimationCurve hopCurve = AnimationCurve.EaseInOut(0, 0, 1, 1); // Zıplama eğrisi
    
    [Header("Wind Effect")]
    [SerializeField] private bool enableWindEffect = true; // WIND AÇIK
    [SerializeField] private GameObject windEffectPrefab; // Opsiyonel particle efekt
    [SerializeField] private int windEffectCount = 3; // Kaç adet wind efekt
    [SerializeField] private float windEffectRadius = 0.2f; // Karaktere ne kadar yakın
    [SerializeField] private float windParticleLifetime = 1.2f; // Particle'ların ömrü
    [SerializeField] private int windEffectSortingOrder = 100; // Sorting order (karakterin üstünde)
    
    [Header("Tile Detection")]
    [SerializeField] private LayerMask tileLayer;
    [SerializeField] private float checkInterval = 0.1f; // Ne sıklıkla kontrol edilecek
    
    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = true;
    
    private Transform visualTransform; // Karakterin görsel kısmı (child objeler)
    private Vector3Int currentTilePosition;
    private Vector3Int lastTilePosition;
    private string currentTileType;
    private string lastTileType;
    private bool isHopping = false;
    private float checkTimer = 0f;
    
    private void Awake()
    {
        // Ground tilemap'i otomatik bul
        if (groundTilemap == null)
        {
            GameObject groundObj = GameObject.Find("ground");
            if (groundObj != null)
            {
                groundTilemap = groundObj.GetComponent<Tilemap>();
                if (groundTilemap != null)
                {
                    Debug.Log("[TileHopEffect] Ground tilemap bulundu");
                }
            }
            
            // Eğer hala bulunamadıysa, tüm tilemap'leri ara
            if (groundTilemap == null)
            {
                Tilemap[] tilemaps = FindObjectsOfType<Tilemap>();
                if (tilemaps.Length > 0)
                {
                    groundTilemap = tilemaps[0];
                    Debug.Log($"[TileHopEffect] İlk tilemap kullanılıyor: {groundTilemap.gameObject.name}");
                }
            }
        }
        
        // Visual transform - player'ın child'larını içeren parent
        visualTransform = transform;
        
        // İlk tile pozisyonunu kaydet
        UpdateCurrentTile();
        lastTilePosition = currentTilePosition;
        lastTileType = currentTileType;
    }
    
    private void Update()
    {
        if (groundTilemap == null) return;
        if (!enableWindEffect && !enableHopEffect) return; // Hiçbir efekt aktif değilse çalışma
        
        // Belli aralıklarla tile kontrolü yap (performans için)
        checkTimer += Time.deltaTime;
        if (checkTimer >= checkInterval)
        {
            checkTimer = 0f;
            CheckTileChange();
        }
    }
    
    private void CheckTileChange()
    {
        UpdateCurrentTile();
        
        // Tile pozisyonu değişti mi?
        if (currentTilePosition != lastTilePosition)
        {
            // Tile tipi de değişti mi?
            if (currentTileType != lastTileType && !string.IsNullOrEmpty(lastTileType))
            {
                if (showDebugInfo)
                {
                    Debug.Log($"[TileHopEffect] Tile değişti: {lastTileType} → {currentTileType}");
                }
                
                TriggerHopEffect();
            }
            
            lastTilePosition = currentTilePosition;
            lastTileType = currentTileType;
        }
    }
    
    private void UpdateCurrentTile()
    {
        if (groundTilemap == null) return;
        
        // Player'ın world pozisyonunu tile pozisyonuna çevir
        Vector3 worldPos = transform.position;
        currentTilePosition = groundTilemap.WorldToCell(worldPos);
        
        // Tile'ı al
        TileBase tile = groundTilemap.GetTile(currentTilePosition);
        
        if (tile != null)
        {
            currentTileType = tile.name;
        }
        else
        {
            currentTileType = "empty";
        }
    }
    
    private void TriggerHopEffect()
    {
        // Sadece wind effect - karaktere dokunma!
        if (enableWindEffect)
        {
            SpawnWindEffect();
        }
        
        // Hop animasyonu kapalı (isteğe bağlı)
        if (enableHopEffect && !isHopping)
        {
            StartCoroutine(HopCoroutine());
        }
    }
    
    private IEnumerator HopCoroutine()
    {
        isHopping = true;
        
        float elapsed = 0f;
        Vector3 startPos = visualTransform.localPosition;
        Vector3 targetPos = startPos; // Aynı pozisyona dön
        
        while (elapsed < hopDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / hopDuration;
            
            // Eğri değerini al (0-1)
            float curveValue = hopCurve.Evaluate(t);
            
            // Parabolic hop (yukarı çık, aşağı in)
            float height = Mathf.Sin(t * Mathf.PI) * hopHeight;
            
            // Y ekseninde offset uygula
            Vector3 newPos = startPos;
            newPos.y = startPos.y + height;
            visualTransform.localPosition = newPos;
            
            yield return null;
        }
        
        // Pozisyonu sıfırla
        visualTransform.localPosition = startPos;
        
        isHopping = false;
    }
    
    private void SpawnWindEffect()
    {
        // Karakterin tam pozisyonu
        Vector3 playerPos = transform.position;
        
        // Birden fazla wind efekt oluştur - RANDOM yönlerde
        for (int i = 0; i < windEffectCount; i++)
        {
            // Random açı (360 derece)
            float randomAngle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            
            // Random radius (karaktere çok yakın)
            float randomDist = Random.Range(0f, windEffectRadius);
            
            // Random offset hesapla
            Vector3 randomOffset = new Vector3(
                Mathf.Cos(randomAngle) * randomDist,
                Mathf.Sin(randomAngle) * randomDist,
                0f
            );
            
            Vector3 effectPos = playerPos + randomOffset;
            
            if (windEffectPrefab != null)
            {
                GameObject effect = Instantiate(windEffectPrefab, effectPos, Quaternion.identity);
                effect.layer = LayerMask.NameToLayer("Ignore Raycast");
                
                // Sorting order'ı ayarla (karakterin üstünde)
                ParticleSystemRenderer[] renderers = effect.GetComponentsInChildren<ParticleSystemRenderer>();
                foreach (var rend in renderers)
                {
                    rend.sortingOrder = windEffectSortingOrder;
                }
                
                // Tüm Rigidbody ve Collider'ları devre dışı bırak
                Rigidbody2D[] rigidbodies = effect.GetComponentsInChildren<Rigidbody2D>(true);
                foreach (var rb in rigidbodies)
                {
                    Destroy(rb);
                }
                
                Collider2D[] colliders = effect.GetComponentsInChildren<Collider2D>(true);
                foreach (var col in colliders)
                {
                    Destroy(col);
                }
                
                // Particle'lar bitene kadar bekle
                Destroy(effect, windParticleLifetime + 0.5f);
            }
            else
            {
                // Basit particle efekt oluştur - random yön
                CreateSimpleWindEffect(effectPos, randomAngle);
            }
        }
    }
    
    private void CreateSimpleWindEffect(Vector3 position, float directionAngle)
    {
        GameObject effectObj = new GameObject("WindEffect");
        effectObj.transform.position = position;
        effectObj.layer = LayerMask.NameToLayer("Ignore Raycast");
        
        // Random yönde rotation
        effectObj.transform.rotation = Quaternion.Euler(0, 0, directionAngle * Mathf.Rad2Deg);
        
        ParticleSystem ps = effectObj.AddComponent<ParticleSystem>();
        
        // Ana ayarlar
        var main = ps.main;
        main.startLifetime = windParticleLifetime; // Particle ömrü
        main.startSpeed = 0f; // HAREKET YOK - sabit dursun
        main.startSize = new ParticleSystem.MinMaxCurve(0.3f, 0.5f); // Random boyut
        main.startColor = new Color(1f, 1f, 1f, 1f); // Tam opak başlasın
        main.maxParticles = 20;
        main.duration = 0.1f; // Sadece başta spawn et
        main.loop = false;
        main.playOnAwake = true;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.startRotation = new ParticleSystem.MinMaxCurve(0, 360f * Mathf.Deg2Rad); // Random rotasyon
        main.stopAction = ParticleSystemStopAction.None; // Bitince hiçbir şey yapma (destroy etme)
        
        // Emission
        var emission = ps.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new ParticleSystem.Burst[] 
        { 
            new ParticleSystem.Burst(0f, 20)
        });
        
        // Shape - Sphere, karaktere yakın
        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.15f; // Karakterin etrafında
        shape.radiusThickness = 1f;
        
        // Size over lifetime - Smooth büyüyüp küçülme
        var sizeOverLifetime = ps.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        AnimationCurve sizeCurve = new AnimationCurve();
        sizeCurve.AddKey(0f, 0f); // Başta 0
        sizeCurve.AddKey(0.2f, 1f); // Hızlıca büyü
        sizeCurve.AddKey(1f, 0.3f); // Yavaşça küçül
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);
        
        // Color over lifetime - Smooth fade out
        var colorOverLifetime = ps.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] 
            { 
                new GradientColorKey(Color.white, 0f),
                new GradientColorKey(Color.white, 0.5f),
                new GradientColorKey(new Color(0.8f, 0.8f, 0.8f), 1f)
            },
            new GradientAlphaKey[] 
            { 
                new GradientAlphaKey(0f, 0f), // Başta görünmez
                new GradientAlphaKey(1f, 0.15f), // Hızlıca belir
                new GradientAlphaKey(0.8f, 0.5f), // Bir süre tam görünür
                new GradientAlphaKey(0f, 1f) // Yavaşça kaybol
            }
        );
        colorOverLifetime.color = gradient;
        
        // Rotation over lifetime - Yavaş dönme
        var rotationOverLifetime = ps.rotationOverLifetime;
        rotationOverLifetime.enabled = true;
        rotationOverLifetime.z = new ParticleSystem.MinMaxCurve(-90f * Mathf.Deg2Rad, 90f * Mathf.Deg2Rad);
        
        // Collision - KAPALI
        var collision = ps.collision;
        collision.enabled = false;
        
        // Renderer
        var renderer = ps.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        renderer.sortingOrder = windEffectSortingOrder;
        
        ps.Play();
        
        // Particle'lar tamamen bitene kadar GameObject'i tutuyoruz
        Destroy(effectObj, windParticleLifetime + 0.5f);
    }
    
    // Public metodlar
    public void SetHopHeight(float height)
    {
        hopHeight = height;
    }
    
    public void SetHopDuration(float duration)
    {
        hopDuration = duration;
    }
    
    public void ToggleEffect(bool enable)
    {
        enableHopEffect = enable;
    }
    
    public string GetCurrentTileType()
    {
        return currentTileType;
    }
}
