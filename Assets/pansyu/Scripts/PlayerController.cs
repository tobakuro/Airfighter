using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // Start is called before the first frame update
    Rigidbody rb;
    float force = 5.0f;    // オブジェクトを動かす際の力
    float speed_X;  // オブジェクトのX軸における速度
    float speed_Z;  // オブジェクトのZ軸における速度
    float maxSpeed = 4.0f;  // オブジェクトの最大速度

    Vector3 prePos; // 前フレームでのオブジェクトの座標位置
    Quaternion target;  // オブジェクトの回転量

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        prePos = transform.position;    // スタート地点の座標を代入
    }

    void Update()
    {
        // 現在の速度を取得
        speed_X = Mathf.Abs(rb.velocity.x);   // X軸における線速度
        speed_Z = Mathf.Abs(rb.velocity.z);   // Z軸における線速度

        // キー操作による移動処理
        if (speed_X < maxSpeed && speed_Z < maxSpeed)   // 速度制限
        {
            if (Input.GetKey(KeyCode.W))
            {
                rb.AddForce(0, 0, force);
            }
            if (Input.GetKey(KeyCode.S))
            {
                rb.AddForce(0, 0, -force);
            }
            if (Input.GetKey(KeyCode.D))
            {
                rb.AddForce(force, 0, 0);
            }
            if (Input.GetKey(KeyCode.A))
            {
                rb.AddForce(-force, 0, 0);
            }
        }

        // オブジェクトの回転処理
        Vector3 direction = transform.position - prePos;    // フレーム間での座標の差分
        if (direction != Vector3.zero)
        {
            target = Quaternion.LookRotation(direction);    // オブジェクトを回転させたい量
            transform.rotation = Quaternion.RotateTowards(transform.rotation, target, 600f * Time.deltaTime);   // 回転を加える処理
        }
        prePos = transform.position;    // 現在の座標位置を代入しておく
    }
}
