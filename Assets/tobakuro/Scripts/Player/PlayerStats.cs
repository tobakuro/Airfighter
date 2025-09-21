using UnityEngine;
using UnityEngine.UI;

public class PlayerStats : MonoBehaviour, IDamageable
{
    [Header("基本ステータス")]
    [SerializeField] private float baseMaxHP = 100f;
    [SerializeField] private float baseMoveSpeed = 8f;
    [SerializeField] private float basePower = 10f;
    [SerializeField] private float baseDefense = 0f;
    
    [Header("現在のステータス")]
    [SerializeField] private float currentHP;
    [SerializeField] private float maxHP;
    [SerializeField] private float moveSpeed;
    [SerializeField] private float power;
    [SerializeField] private float defense;
    
    [Header("アイテム効果")]
    [SerializeField] private float speedBoostAmount = 2f;
    [SerializeField] private float powerBoostAmount = 5f;
    [SerializeField] private float hpBoostAmount = 20f;
    [SerializeField] private float defenseBoostAmount = 5f;
    [SerializeField] private float healAmount = 30f;
    
    [Header("UI参照")]
    [SerializeField] private Slider hpBar;
    [SerializeField] private Text statsText;
    
    [Header("視覚エフェクト")]
    [SerializeField] private GameObject levelUpEffectPrefab;
    [SerializeField] private AudioClip levelUpSound;
    [SerializeField] private AudioClip healSound;
    [SerializeField] private AudioClip damageSound;
    
    // 他のコンポーネントへの参照
    private PlayerController playerController;
    private AudioSource audioSource;
    
    // イベント
    public System.Action<float> OnHPChanged;
    public System.Action OnPlayerDeath;
    public System.Action<PlayerStats> OnStatsChanged;
    
    void Start()
    {
        // 初期ステータスを設定
        maxHP = baseMaxHP;
        currentHP = maxHP;
        moveSpeed = baseMoveSpeed;
        power = basePower;
        defense = baseDefense;
        
        // コンポーネント取得
        playerController = GetComponent<PlayerController>();
        audioSource = GetComponent<AudioSource>();
        
        // AudioSourceがない場合は追加
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // 初期ステータスを適用
        ApplyStatsToComponents();
        UpdateUI();
    }
    
    // アイテム効果の適用
    public void ApplySpeedBoost()
    {
        moveSpeed += speedBoostAmount;
        ApplyStatsToComponents();
        UpdateUI();
        PlayUpgradeEffects();
        OnStatsChanged?.Invoke(this);
        
        Debug.Log($"スピードアップ！現在の移動速度: {moveSpeed}");
    }
    
    public void ApplyPowerBoost()
    {
        power += powerBoostAmount;
        ApplyStatsToComponents();
        UpdateUI();
        PlayUpgradeEffects();
        OnStatsChanged?.Invoke(this);
        
        Debug.Log($"パワーアップ！現在の攻撃力: {power}");
    }
    
    public void ApplyHPBoost()
    {
        maxHP += hpBoostAmount;
        currentHP += hpBoostAmount; // 最大HPが上がった分、現在HPも回復
        ApplyStatsToComponents();
        UpdateUI();
        PlayUpgradeEffects();
        OnStatsChanged?.Invoke(this);
        
        Debug.Log($"最大HP増加！現在の最大HP: {maxHP}");
    }
    
    public void ApplyDefenseBoost()
    {
        defense += defenseBoostAmount;
        ApplyStatsToComponents();
        UpdateUI();
        PlayUpgradeEffects();
        OnStatsChanged?.Invoke(this);
        
        Debug.Log($"防御力アップ！現在の防御力: {defense}");
    }
    
    public void ApplyHeal()
    {
        float healedAmount = Mathf.Min(healAmount, maxHP - currentHP);
        currentHP += healedAmount;
        currentHP = Mathf.Clamp(currentHP, 0, maxHP);
        
        UpdateUI();
        PlayHealEffects();
        OnHPChanged?.Invoke(currentHP);
        
        Debug.Log($"HP回復！回復量: {healedAmount}, 現在HP: {currentHP}/{maxHP}");
    }
    
    // ダメージを受ける（IDamageableの実装）
    public void TakeDamage(float damage)
    {
        // 防御力を考慮したダメージ計算
        float actualDamage = Mathf.Max(1f, damage - defense);
        currentHP -= actualDamage;
        currentHP = Mathf.Clamp(currentHP, 0, maxHP);
        
        UpdateUI();
        PlayDamageEffects();
        OnHPChanged?.Invoke(currentHP);
        
        Debug.Log($"ダメージを受けた！ダメージ: {actualDamage}, 残りHP: {currentHP}");
        
        if (currentHP <= 0)
        {
            Die();
        }
    }
    
    void Die()
    {
        Debug.Log("プレイヤーが倒れました！");
        OnPlayerDeath?.Invoke();
        
        // ここでゲームオーバー処理やリスポーン処理を呼び出す
        // 例: GameManager.Instance.GameOver();
    }
    
    // ステータスを他のコンポーネントに適用
    void ApplyStatsToComponents()
    {
        if (playerController != null)
        {
            playerController.SetMoveSpeed(moveSpeed);
            playerController.SetPower(power);
        }
    }
    
    // UI更新
    void UpdateUI()
    {
        if (hpBar != null)
        {
            hpBar.value = currentHP / maxHP;
        }
        
        if (statsText != null)
        {
            statsText.text = $"HP: {currentHP:F0}/{maxHP:F0}\n" +
                            $"スピード: {moveSpeed:F1}\n" +
                            $"パワー: {power:F0}\n" +
                            $"防御: {defense:F0}";
        }
    }
    
    // エフェクト再生
    void PlayUpgradeEffects()
    {
        // レベルアップエフェクト
        if (levelUpEffectPrefab != null)
        {
            Instantiate(levelUpEffectPrefab, transform.position + Vector3.up, Quaternion.identity);
        }
        
        // レベルアップサウンド
        if (levelUpSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(levelUpSound);
        }
    }
    
    void PlayHealEffects()
    {
        // 回復サウンド
        if (healSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(healSound);
        }
    }
    
    void PlayDamageEffects()
    {
        // ダメージサウンド
        if (damageSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(damageSound);
        }
    }
    
    // ゲッター（他のスクリプトから参照用）
    public float GetCurrentHP() => currentHP;
    public float GetMaxHP() => maxHP;
    public float GetMoveSpeed() => moveSpeed;
    public float GetPower() => power;
    public float GetDefense() => defense;
    public float GetHPPercentage() => currentHP / maxHP;
    
    // セッター（デバッグ用）
    public void SetHP(float hp)
    {
        currentHP = Mathf.Clamp(hp, 0, maxHP);
        UpdateUI();
    }
    
    // ステータス情報を文字列で取得
    public string GetStatsString()
    {
        return $"HP: {currentHP:F0}/{maxHP:F0}, Speed: {moveSpeed:F1}, Power: {power:F0}, Defense: {defense:F0}";
    }
    
    // リセット機能（新しいゲーム開始時など）
    public void ResetStats()
    {
        maxHP = baseMaxHP;
        currentHP = maxHP;
        moveSpeed = baseMoveSpeed;
        power = basePower;
        defense = baseDefense;
        
        ApplyStatsToComponents();
        UpdateUI();
        OnStatsChanged?.Invoke(this);
        
        Debug.Log("ステータスをリセットしました");
    }
    
    // ステータスの総合スコアを計算（デバッグ・バランス調整用）
    public float GetTotalScore()
    {
        return (currentHP / baseMaxHP) * 25f +
               (moveSpeed / baseMoveSpeed) * 25f +
               (power / basePower) * 25f +
               (defense / 5f) * 25f; // 防御力は基本0なので固定値で割る
    }
}