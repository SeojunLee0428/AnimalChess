using UnityEngine;

public class Animations : MonoBehaviour
{
    Animator ani;   
   
    private void Awake()
    {
        ani = this.GetComponent<Animator>();
    }
 
    public void Walk()   //걷는 모션
    {
        ani.SetBool("Walk", true);
    }
    
    public void Attack1()   //힐러 공격 모션
    {
        ani.Play("Attack1");
    }

    public void Attack2()   //워리어, 매지션 공격 모션
    {
        ani.Play("Attack2");
    }

    public void Attack3()   //아처 공격 모션
    {
        ani.Play("Attack3");
    }

    public void Jump()   //왕 점프 공격 모션
    {
        ani.Play("Jump");
    }

    public void Damage()   //공격받은 모션
    {
        ani.Play("Damage");
    }

    public void Die()   // 죽는 모션
    {
        ani.Play("Die");
    }

    public void Reset()   //애니메이션 초기화
    {
        ani.SetBool("Walk", false);
        ani.SetBool("Attack1", false);
        ani.SetBool("Attack2", false);
        ani.SetBool("Attack3", false);
        ani.SetBool("Damage", false);
        ani.SetBool("Jump", false);
    }
}
