using UnityEngine;

// アイテム生成用のデータ構造
[System.Serializable]
public class ItemSpawnData
{
    [Header("アイテム設定")]
    public ItemType itemType;           // スポーンするアイテムの種類
    public float spawnWeight = 1f;      // 出現確率の重み（高いほど出やすい）
    
    [Header("説明（エディター用）")]
    [TextArea(2, 3)]
    public string description = "";     // エディターでの説明文
}

public class ItemSpawner : MonoBehaviour
{
    [Header("アイテムプレファブ")]
    [SerializeField] private GameObject itemPrefab;
    
    [Header("スポーン設定")]
    [SerializeField] private float spawnRadius = 10f;       // スポーン範囲の半径
    [SerializeField] private int maxItemsOnField = 20;      // フィールド上の最大アイテム数
    [SerializeField] private float spawnInterval = 3f;     // スポーン間隔（秒）
    [SerializeField] private LayerMask groundLayerMask = 1; // 地面のレイヤーマスク
    
    [Header("アイテム出現設定")]
    [SerializeField] private ItemSpawnData[] spawnData = new ItemSpawnData[]
    {
        new ItemSpawnData { itemType = ItemType.Speed, spawnWeight = 1f },
        new ItemSpawnData { itemType = ItemType.Power, spawnWeight = 1f },
        new ItemSpawnData { itemType = ItemType.HP, spawnWeight = 0.7f },
        new ItemSpawnData { itemType = ItemType.Defense, spawnWeight = 0.8f },
        new ItemSpawnData { itemType = ItemType.Heal, spawnWeight = 1.5f }
    };
    
    [Header("デバッグ情報")]
    [SerializeField] private bool showDebugInfo = true;
    [SerializeField] private bool autoSpawn = true;         // 自動スポーンのON/OFF
    
    // 内部変数
    private float spawnTimer = 0f;
    private int currentItemCount = 0;
    private Transform playerTransform;  // プレイヤーの位置参照用
    
    void Start()
    {
        // プレイヤーの位置を取得（プレイヤー周辺にアイテムをスポーンするため）
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
        
        // 初期アイテムをいくつかスポーン
        for (int i = 0; i < 5; i++)
        {
            SpawnRandomItem();
        }
    }
    
    void Update()
    {
        if (!autoSpawn) return;
        
        // スポーンタイマーの更新
        spawnTimer += Time.deltaTime;
        
        // 条件を満たしたらアイテムをスポーン
        if (spawnTimer >= spawnInterval && currentItemCount < maxItemsOnField)
        {
            SpawnRandomItem();
            spawnTimer = 0f;
        }
    }
    
    /// <summary>
    /// ランダムなアイテムを生成する
    /// </summary>
    public void SpawnRandomItem()
    {
        if (itemPrefab == null || spawnData.Length == 0) 
        {
            Debug.LogWarning("ItemSpawner: アイテムプレファブまたはスポーンデータが設定されていません");
            return;
        }
        
        // ランダムな位置を生成
        Vector3 spawnPosition = GetRandomSpawnPosition();
        
        // スポーン位置が有効かチェック
        if (spawnPosition == Vector3.zero)
        {
            Debug.LogWarning("ItemSpawner: 適切なスポーン位置が見つかりませんでした");
            return;
        }
        
        // アイテムを生成
        GameObject newItem = Instantiate(itemPrefab, spawnPosition, Quaternion.identity);
        
        // ランダムなアイテムタイプを設定
        ItemType randomType = GetRandomItemType();
        Item itemComponent = newItem.GetComponent<Item>();
        if (itemComponent != null)
        {
            itemComponent.SetItemType(randomType);
        }
        
        // アイテムカウントを増加
        currentItemCount++;
        
        // アイテムが破棄されたときの処理を設定
        ItemDestroyNotifier notifier = newItem.GetComponent<ItemDestroyNotifier>();
        if (notifier == null)
        {
            notifier = newItem.AddComponent<ItemDestroyNotifier>();
        }
        notifier.SetSpawner(this);
        
        if (showDebugInfo)
        {
            Debug.Log($"アイテム生成: {randomType} at {spawnPosition} (現在のアイテム数: {currentItemCount})");
        }
    }
    
    /// <summary>
    /// ランダムなスポーン位置を取得
    /// </summary>
    Vector3 GetRandomSpawnPosition()
    {
        Vector3 centerPosition = (playerTransform != null) ? playerTransform.position : transform.position;
        
        // 最大10回試行してスポーン位置を見つける
        for (int attempts = 0; attempts < 10; attempts++)
        {
            // 円形範囲内のランダムな位置
            Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
            Vector3 candidatePosition = centerPosition + new Vector3(randomCircle.x, 20f, randomCircle.y);
            
            // 地面にレイキャストして適切な高さを見つける
            if (Physics.Raycast(candidatePosition, Vector3.down, out RaycastHit hit, 40f, groundLayerMask))
            {
                Vector3 spawnPos = hit.point + Vector3.up * 0.5f; // 地面から少し浮かせる
                
                // 既存のアイテムと重複しないかチェック
                if (!IsPositionOccupied(spawnPos))
                {
                    return spawnPos;
                }
            }
        }
        
        return Vector3.zero; // 適切な位置が見つからなかった場合
    }
    
    /// <summary>
    /// 指定位置に既にアイテムが存在するかチェック
    /// </summary>
    bool IsPositionOccupied(Vector3 position)
    {
        Collider[] overlapping = Physics.OverlapSphere(position, 1f);
        foreach (var collider in overlapping)
        {
            if (collider.GetComponent<Item>() != null)
            {
                return true; // アイテムが存在する
            }
        }
        return false;
    }
    
    /// <summary>
    /// 重み付きランダムでアイテムタイプを選択
    /// </summary>
    ItemType GetRandomItemType()
    {
        if (spawnData.Length == 0) return ItemType.Heal;
        
        // 総重みを計算
        float totalWeight = 0f;
        foreach (var data in spawnData)
        {
            totalWeight += data.spawnWeight;
        }
        
        // ランダム値を生成
        float randomValue = Random.Range(0f, totalWeight);
        float currentWeight = 0f;
        
        // 重み付きで選択
        foreach (var data in spawnData)
        {
            currentWeight += data.spawnWeight;
            if (randomValue <= currentWeight)
            {
                return data.itemType;
            }
        }
        
        // フォールバック
        return spawnData[0].itemType;
    }
    
    /// <summary>
    /// 指定位置に特定のアイテムを生成
    /// </summary>
    public void SpawnSpecificItem(ItemType itemType, Vector3 position)
    {
        if (itemPrefab == null) return;
        
        GameObject newItem = Instantiate(itemPrefab, position, Quaternion.identity);
        Item itemComponent = newItem.GetComponent<Item>();
        if (itemComponent != null)
        {
            itemComponent.SetItemType(itemType);
        }
        
        currentItemCount++;
        
        // 破棄通知を設定
        ItemDestroyNotifier notifier = newItem.GetComponent<ItemDestroyNotifier>();
        if (notifier == null)
        {
            notifier = newItem.AddComponent<ItemDestroyNotifier>();
        }
        notifier.SetSpawner(this);
        
        if (showDebugInfo)
        {
            Debug.Log($"特定アイテム生成: {itemType} at {position}");
        }
    }
    
    /// <summary>
    /// アイテムが破棄されたときに呼び出される
    /// </summary>
    public void OnItemDestroyed()
    {
        currentItemCount--;
        currentItemCount = Mathf.Max(0, currentItemCount);
        
        if (showDebugInfo)
        {
            Debug.Log($"アイテム破棄: 残りアイテム数 {currentItemCount}");
        }
    }
    
    /// <summary>
    /// 全てのアイテムを削除
    /// </summary>
    public void ClearAllItems()
    {
        GameObject[] items = GameObject.FindGameObjectsWithTag("Item");
        foreach (GameObject item in items)
        {
            Destroy(item);
        }
        currentItemCount = 0;
    }
    
    /// <summary>
    /// スポーン設定をリセット
    /// </summary>
    public void ResetSpawner()
    {
        spawnTimer = 0f;
        currentItemCount = 0;
    }
    
    // ゲッター
    public int GetCurrentItemCount() => currentItemCount;
    public int GetMaxItemsOnField() => maxItemsOnField;
    public float GetSpawnInterval() => spawnInterval;
    
    // デバッグ情報表示
    void OnGUI()
    {
        if (showDebugInfo && Application.isEditor)
        {
            GUI.Label(new Rect(10, 200, 300, 20), $"アイテム数: {currentItemCount}/{maxItemsOnField}");
            GUI.Label(new Rect(10, 220, 300, 20), $"次のスポーンまで: {(spawnInterval - spawnTimer):F1}秒");
        }
    }
    
    // シーンビューでスポーン範囲を表示
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector3 center = (playerTransform != null) ? playerTransform.position : transform.position;
        Gizmos.DrawWireSphere(center, spawnRadius);
    }
}

// アイテム破棄通知用の補助コンポーネント
public class ItemDestroyNotifier : MonoBehaviour
{
    private ItemSpawner spawner;
    
    public void SetSpawner(ItemSpawner itemSpawner)
    {
        spawner = itemSpawner;
    }
    
    void OnDestroy()
    {
        if (spawner != null)
        {
            spawner.OnItemDestroyed();
        }
    }
}