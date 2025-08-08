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

    public void PlayMoveAnim(Vector2Int dir)
    {
        anim.SetFloat("xDir", dir.x);
        anim.SetFloat("yDir", dir.y);
    }

    public void PlayMoveAnim(int xDir, int yDir)
    {
        anim.SetFloat("xDir", xDir);
        anim.SetFloat("yDir", yDir);
    }
}
