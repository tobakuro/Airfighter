using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("弾丸設定")]
    [SerializeField] private float damage = 10f;
    [SerializeField] private LayerMask enemyLayerMask = -1; // 敵のレイヤー
    
    [Header("エフェクト")]
    [SerializeField] private GameObject hitEffectPrefab;
    
    // ダメージ値を外部から設定可能にする
    public void SetDamage(float newDamage)
    {
        damage = newDamage;
    }
    
    private void OnTriggerEnter(Collider other)
    {
        // 敵レイヤーかチェック
        if (((1 << other.gameObject.layer) & enemyLayerMask) != 0)
        {
            // 敵にダメージを与える
            IDamageable damageable = other.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(damage);
            }
            
            // ヒットエフェクトを生成
            if (hitEffectPrefab != null)
            {
                Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
            }
            
            // 弾を破棄
            Destroy(gameObject);
        }
        // 地面や壁に当たった場合も破棄
        else if (other.CompareTag("Ground") || other.CompareTag("Wall"))
        {
            Destroy(gameObject);
        }
    }
}

// ダメージを受けられるオブジェクト用のインターフェース
public interface IDamageable
{
    void TakeDamage(float damage);
}