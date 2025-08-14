using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationController : MonoBehaviour
{
    private Animator anim;

    public void Initalize(Animator inAnimator)
    {
        anim = inAnimator;
    }

    public void PlayAttack()
    {
        anim.SetTrigger("attack");
    }

    public void PlayWalk()
    {
        anim.SetTrigger("walk");
    }

    public void PlayAttacked()
    {
        anim.SetTrigger("attacked");
    }

    public void PlayDeath()
    {
        anim.SetTrigger("death");
    }

    public void PlayAnimDirection(Vector2Int dir)
    {
        anim.SetFloat("xDir", dir.x);
        anim.SetFloat("yDir", dir.y);
    }

    public void SetLookDirection(Vector2 dir)
    {
        anim.SetFloat("xDir", dir.x);
        anim.SetFloat("yDir", dir.y);
    }

    public void SetLookDirection(int xDir, int yDir)
    {
        anim.SetFloat("xDir", xDir);
        anim.SetFloat("yDir", yDir);
    }
}
