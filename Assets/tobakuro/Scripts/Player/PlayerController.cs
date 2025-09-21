using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("移動設定")]
    [SerializeField] private float baseMoveSpeed = 8f;    // 基本移動速度
    [SerializeField] private float currentMoveSpeed = 8f; // 現在の移動速度（アイテムで変化）
    [SerializeField] private float acceleration = 20f;    // 加速度（高めで反応良く）
    [SerializeField] private float deceleration = 25f;    // 減速度（高めで慣性弱く）
    
    [Header("ダッシュ設定")]
    [SerializeField] private float dashSpeed = 15f;
    [SerializeField] private float dashDuration = 0.3f;
    [SerializeField] private float dashCooldown = 0.5f;
    [SerializeField] private float doubleTapWindow = 0.3f; // 二回連打の判定時間
    
    [Header("射撃設定")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float bulletSpeed = 20f;
    [SerializeField] private float fireRate = 0.2f; // 連射間隔
    [SerializeField] private float basePower = 10f; // 基本攻撃力
    [SerializeField] private float currentPower = 10f; // 現在の攻撃力（アイテムで変化）
    
    // コンポーネント参照
    private Rigidbody rb;
    private Camera playerCamera;
    
    // 移動関連
    private Vector3 currentVelocity;
    private Vector3 inputDirection;
    
    // ダッシュ関連
    private bool isDashing = false;
    private float dashTimer = 0f;
    private float dashCooldownTimer = 0f;
    private Vector3 dashDirection;
    
    // 二回連打検出用
    private float[] lastKeyPressTime = new float[4]; // W, A, S, D
    private KeyCode[] movementKeys = { KeyCode.W, KeyCode.A, KeyCode.S, KeyCode.D };
    
    // 射撃関連
    private float lastFireTime = 0f;
    
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        playerCamera = Camera.main;
        
        // Rigidbodyの設定（スピード感を重視）
        rb.drag = 0f; // 物理的な抵抗は使わず、スクリプトで制御
        rb.freezeRotation = true; // 回転は制御しない
        
        // 射撃ポイントが設定されていない場合、プレイヤーの前方に設定
        if (firePoint == null)
        {
            GameObject firePointObj = new GameObject("FirePoint");
            firePointObj.transform.SetParent(transform);
            firePointObj.transform.localPosition = Vector3.forward;
            firePoint = firePointObj.transform;
        }
    }
    
    void Update()
    {
        HandleInput();
        HandleShooting();
        UpdateDash();
    }
    
    void FixedUpdate()
    {
        HandleMovement();
    }
    
    void HandleInput()
    {
        // 基本的な移動入力
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        
        // カメラ基準の移動方向を計算
        Vector3 cameraForward = playerCamera.transform.forward;
        Vector3 cameraRight = playerCamera.transform.right;
        
        // Y軸を無視して地上での移動に限定
        cameraForward.y = 0f;
        cameraRight.y = 0f;
        cameraForward.Normalize();
        cameraRight.Normalize();
        
        inputDirection = (cameraForward * vertical + cameraRight * horizontal).normalized;
        
        // 二回連打の検出
        CheckForDoubleTap();
        
        // プレイヤーの向きを移動方向に合わせる
        if (inputDirection != Vector3.zero && !isDashing)
        {
            transform.rotation = Quaternion.LookRotation(inputDirection);
        }
    }
    
    void CheckForDoubleTap()
    {
        for (int i = 0; i < movementKeys.Length; i++)
        {
            if (Input.GetKeyDown(movementKeys[i]))
            {
                float currentTime = Time.time;
                
                // 二回連打の判定
                if (currentTime - lastKeyPressTime[i] <= doubleTapWindow)
                {
                    // ダッシュ可能かチェック
                    if (!isDashing && dashCooldownTimer <= 0f)
                    {
                        StartDash(GetDirectionFromKeyIndex(i));
                    }
                }
                
                lastKeyPressTime[i] = currentTime;
            }
        }
    }
    
    Vector3 GetDirectionFromKeyIndex(int keyIndex)
    {
        Vector3 cameraForward = playerCamera.transform.forward;
        Vector3 cameraRight = playerCamera.transform.right;
        cameraForward.y = 0f;
        cameraRight.y = 0f;
        cameraForward.Normalize();
        cameraRight.Normalize();
        
        switch (keyIndex)
        {
            case 0: return cameraForward;  // W
            case 1: return -cameraRight;   // A
            case 2: return -cameraForward; // S
            case 3: return cameraRight;    // D
            default: return Vector3.zero;
        }
    }
    
    void StartDash(Vector3 direction)
    {
        isDashing = true;
        dashTimer = dashDuration;
        dashDirection = direction;
        dashCooldownTimer = dashCooldown;
    }
    
    void UpdateDash()
    {
        // ダッシュタイマーの更新
        if (isDashing)
        {
            dashTimer -= Time.deltaTime;
            if (dashTimer <= 0f)
            {
                isDashing = false;
            }
        }
        
        // クールダウンタイマーの更新
        if (dashCooldownTimer > 0f)
        {
            dashCooldownTimer -= Time.deltaTime;
        }
    }
    
    void HandleMovement()
    {
        Vector3 targetVelocity;
        
        if (isDashing)
        {
            // ダッシュ中の移動
            targetVelocity = dashDirection * dashSpeed;
        }
        else if (inputDirection != Vector3.zero)
        {
            // 通常の移動
            targetVelocity = inputDirection * currentMoveSpeed;
        }
        else
        {
            // 入力がない場合は停止
            targetVelocity = Vector3.zero;
        }
        
        // スムーズな加速・減速（スピード感重視で高い値）
        float lerpSpeed = (targetVelocity != Vector3.zero) ? acceleration : deceleration;
        currentVelocity = Vector3.Lerp(currentVelocity, targetVelocity, lerpSpeed * Time.fixedDeltaTime);
        
        // Y軸の速度は保持（重力など）
        Vector3 finalVelocity = new Vector3(currentVelocity.x, rb.velocity.y, currentVelocity.z);
        rb.velocity = finalVelocity;
    }
    
    void HandleShooting()
    {
        if (Input.GetMouseButton(0) && Time.time >= lastFireTime + fireRate)
        {
            Shoot();
            lastFireTime = Time.time;
        }
    }
    
    void Shoot()
    {
        if (bulletPrefab != null)
        {
            // 弾を生成
            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
            
            // 弾に速度を与える
            Rigidbody bulletRb = bullet.GetComponent<Rigidbody>();
            if (bulletRb != null)
            {
                bulletRb.velocity = firePoint.forward * bulletSpeed;
            }
            
            // 弾にダメージ値を設定
            Bullet bulletScript = bullet.GetComponent<Bullet>();
            if (bulletScript != null)
            {
                bulletScript.SetDamage(currentPower);
            }
            
            // 5秒後に弾を破棄
            Destroy(bullet, 5f);
        }
    }
    
    // ステータス更新用メソッド（PlayerStatsから呼び出される）
    public void SetMoveSpeed(float newMoveSpeed)
    {
        currentMoveSpeed = newMoveSpeed;
    }
    
    public void SetPower(float newPower)
    {
        currentPower = newPower;
    }
    
    // ゲッター
    public float GetCurrentMoveSpeed() => currentMoveSpeed;
    public float GetCurrentPower() => currentPower;
    
    // デバッグ用：現在の状態を表示
    void OnGUI()
    {
        if (Application.isEditor)
        {
            GUILayout.Label($"速度: {currentVelocity.magnitude:F1} (最大: {currentMoveSpeed:F1})");
            GUILayout.Label($"攻撃力: {currentPower:F0}");
            GUILayout.Label($"ダッシュ中: {isDashing}");
            GUILayout.Label($"クールダウン: {dashCooldownTimer:F1}");
        }
    }
}