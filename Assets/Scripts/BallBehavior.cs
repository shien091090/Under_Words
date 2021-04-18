//球的控制腳本

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallBehavior : MonoBehaviour
{
    //-----------變數宣告------------------------------------------------
    public static bool sensor_crash = false; //訊號：撞擊方塊
    public static bool sensor_touchBottom = false; //訊號：觸底
    public static List<string> s_PWords = new List<string>(); //激活詞彙(正面)
    public static List<string> s_NWords = new List<string>(); //激活詞彙(負面)
    public int type; //球的類型 0=普通 / 1=不受控語言(穿透)
    public bool staying = true; //靜止狀態
    public float initialVelocity; //初速
    public float maxSpeed; //最大速度
    public Vector2 moveSpeed; //目前移動速度
    public float rotateAddition; //轉速加成

    private CircleCollider2D circleCollider; //碰撞器

    //-----------內建方法------------------------------------------------
    void Start()
    {

    }

    void FixedUpdate()
    {
        if (!StaticScript.pause && !staying) //非暫停狀態及非靜止狀態時,按物理模擬模式移動
        {
            Move();
            Rotate();
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        ContactPoint2D[] point = collision.contacts;
        if (collision.gameObject.CompareTag("PlayerBar")) //觸碰橫條
        {
            if (type == 1)
            {
                Physics2D.IgnoreCollision(circleCollider, collision.collider, true);
            }
            else
            {
                float speedX = new float();
                float speedY = point[0].normal.y == 0 ? moveSpeed.y : ( point[0].normal.y > 0 ? initialVelocity : -initialVelocity ); //Y軸速度為全速反彈

                if (point[0].normal.x != 0) //X軸撞角速度計算(計算碰撞角度+拖曳速度)
                {
                    speedX = ( Mathf.Abs(moveSpeed.x) * point[0].normal.x ) + PlayerBarBehavior.Instance.dragSpeed;
                }
                else if (point[0].normal.x == 0) //X軸撞面速度計算(只計算拖曳速度)
                {
                    speedX = moveSpeed.x + PlayerBarBehavior.Instance.dragSpeed;
                }

                AudioManagerScript.Instance.PlayAudioClip("SE_HITBAR");

                ParticleEffectController.Instance.OneShotEffect(ParticleEffectType.球互撞, point[0].point, true);

                moveSpeed = new Vector2(speedX, speedY);
            }

        }

        else if (collision.gameObject.CompareTag("Wall_up") || collision.gameObject.CompareTag("Wall_right") || collision.gameObject.CompareTag("Wall_left") || collision.gameObject.CompareTag("Wall_down")) //觸碰牆
        {
            if (collision.gameObject.CompareTag("Wall_down"))
            {
                sensor_touchBottom = true;
            }

            AudioManagerScript.Instance.PlayAudioClip("SE_HITWALL");

            moveSpeed = new Vector2(moveSpeed.x * point[0].normal.x == 0 ? moveSpeed.x : moveSpeed.x * -1, moveSpeed.y * point[0].normal.y == 0 ? moveSpeed.y : moveSpeed.y * -1);
        }

        else if (collision.gameObject.CompareTag("Brick")) //觸碰磚塊
        {
            collision.gameObject.SendMessage("Smash"); //擊毀磚塊

            float speedX = Mathf.Abs(point[0].normal.x) <= 0.3f ? moveSpeed.x : Mathf.Abs(moveSpeed.x) * point[0].normal.x; //X軸撞面與撞角速度
            float speedY = Mathf.Abs(point[0].normal.y) <= 0.3f ? moveSpeed.y : ( point[0].normal.y > 0 ? initialVelocity : -initialVelocity ); //Y軸速度為全速反彈
            BrickBehavior brick = collision.gameObject.GetComponent<BrickBehavior>();
            if (brick.type) //激活正面詞語時
            {
                s_PWords.Add(brick.content.text);

                ParticleEffectController.Instance.OneShotEffect(ParticleEffectType.打擊正面方塊, point[0].point, true);
            }
            else //激活負面詞語時
            {
                s_NWords.Add(brick.content.text);

                ParticleEffectController.Instance.OneShotEffect(ParticleEffectType.打擊負面方塊, point[0].point, true);
            }

            AudioManagerScript.Instance.PlayAudioClip("SE_HITBRICK");

            moveSpeed = new Vector2(speedX, speedY); //改變速度向量
            sensor_crash = true;
        }

        else if (collision.gameObject.CompareTag("Ball")) //撞擊球
        {

            if (type == 2)
            {
                Physics2D.IgnoreCollision(circleCollider, collision.collider, true);
            }
            else
            {
                float speedX = point[0].normal.x == 0 ? moveSpeed.x : initialVelocity * point[0].normal.x; //X軸撞面與撞角速度
                float speedY = point[0].normal.y == 0 ? moveSpeed.y : ( point[0].normal.y > 0 ? initialVelocity : -initialVelocity ); //Y軸速度為全速反彈

                AudioManagerScript.Instance.PlayAudioClip("SE_HITBAR");

                ParticleEffectController.Instance.OneShotEffect(ParticleEffectType.球互撞, point[0].point, true);

                moveSpeed = new Vector2(speedX, speedY);
            }
        }

        else if (collision.gameObject.CompareTag("Brick_Space")) //撞擊空白磚塊
        {
            collision.gameObject.GetComponent<Animator>().Play("SpaceDisappear", 0, 0);
        }
    }

    //-----------自訂方法------------------------------------------------

    //慣性移動
    private void Move()
    {
        moveSpeed = new Vector2(Mathf.Abs(moveSpeed.x) > maxSpeed ? maxSpeed * ( moveSpeed.x / Mathf.Abs(moveSpeed.x) ) : moveSpeed.x, moveSpeed.y);
        this.transform.position += new Vector3(moveSpeed.x * Time.deltaTime, moveSpeed.y * Time.deltaTime);
    }

    //自轉
    private void Rotate()
    {
        this.transform.Rotate(Vector3.forward * Time.fixedDeltaTime * -moveSpeed.x * rotateAddition);
    }

    //初始化移動
    public void InitialMove(Vector2 inter)
    {
        moveSpeed = new Vector2(inter.x, this.transform.position.y > PlayerBarBehavior.Instance.transform.position.y ? initialVelocity + inter.y : -initialVelocity - inter.y); //賦予初速
        staying = false; //解除靜止狀態
        circleCollider = this.GetComponent<CircleCollider2D>();
        circleCollider.enabled = true; //開啟碰撞器
    }

}
