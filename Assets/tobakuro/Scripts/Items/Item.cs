using UnityEngine;

public enum ItemType
{
    Speed,      // スピードアップ
    Power,      // パワーアップ
    HP,         // 最大HP増加
    Defense,    // 防御力アップ
    Heal        // HP回復
}

public class Item : MonoBehaviour
{
    [Header("アイテム設定")]
    [SerializeField] private ItemType itemType;
    [SerializeField] private float rotationSpeed = 90f;
    [SerializeField] private float bobSpeed = 2f;
    [SerializeField] private float bobHeight = 0.5f;
    
    [Header("エフェクト")]
    [SerializeField] private GameObject pickupEffectPrefab;
    [SerializeField] private AudioClip pickupSound;
    
    [Header("アイテム別マテリアル")]
    [SerializeField] private Material speedMaterial;    // 青色
    [SerializeField] private Material powerMaterial;    // 赤色
    [SerializeField] private Material hpMaterial;       // 緑色
    [SerializeField] private Material defenseMaterial;  // 黄色
    [SerializeField] private Material healMaterial;     // ピンク色
    
    private Vector3 startPosition;
    private Renderer itemRenderer;
    private AudioSource audioSource;
    
    void Start()
    {
        startPosition = transform.position;
        itemRenderer = GetComponent<Renderer>();
        audioSource = GetComponent<AudioSource>();
        
        // アイテムタイプに応じた見た目を設定
        SetAppearanceByType();
    }
    
    void Update()
    {
        // アイテムの回転
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
        
        // 上下の浮遊アニメーション
        float newY = startPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }
    
    void SetAppearanceByType()
    {
        if (itemRenderer == null) return;
        
        switch (itemType)
        {
            case ItemType.Speed:
                if (speedMaterial != null) itemRenderer.material = speedMaterial;
                break;
            case ItemType.Power:
                if (powerMaterial != null) itemRenderer.material = powerMaterial;
                break;
            case ItemType.HP:
                if (hpMaterial != null) itemRenderer.material = hpMaterial;
                break;
            case ItemType.Defense:
                if (defenseMaterial != null) itemRenderer.material = defenseMaterial;
                break;
            case ItemType.Heal:
                if (healMaterial != null) itemRenderer.material = healMaterial;
                break;
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerStats playerStats = other.GetComponent<PlayerStats>();
            if (playerStats != null)
            {
                ApplyItemEffect(playerStats);
                PlayPickupEffects();
                Destroy(gameObject);
            }
        }
    }
    
    void ApplyItemEffect(PlayerStats playerStats)
    {
        switch (itemType)
        {
            case ItemType.Speed:
                playerStats.ApplySpeedBoost();
                break;
            case ItemType.Power:
                playerStats.ApplyPowerBoost();
                break;
            case ItemType.HP:
                playerStats.ApplyHPBoost();
                break;
            case ItemType.Defense:
                playerStats.ApplyDefenseBoost();
                break;
            case ItemType.Heal:
                playerStats.ApplyHeal();
                break;
        }
    }
    
    void PlayPickupEffects()
    {
        // エフェクト生成
        if (pickupEffectPrefab != null)
        {
            Instantiate(pickupEffectPrefab, transform.position, Quaternion.identity);
        }
        
        // サウンド再生
        if (pickupSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(pickupSound);
        }
    }
    
    // エディター用：アイテムタイプを設定
    public void SetItemType(ItemType type)
    {
        itemType = type;
        SetAppearanceByType();
    }
    
    // ゲッター
    public ItemType GetItemType() => itemType;
}