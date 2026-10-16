using UnityEngine;
using System.Collections;

public class EnemyAnimator : MonoBehaviour
{
    private const float SWAP_DEBOUNCE = 0.5f;
    
    private Enemy attachedEnemy;
    private EnemySprites sprites;

    private Sprite lastWalkSprite;
    private float lastAnimStateChange = 0;

    private bool attacking = false;

    public void Initialize(string variationPack, Enemy enemy, string type)
    {
        attachedEnemy = enemy;
        sprites = WaveManager.Instance.GetEnemySprites(type, variationPack);

        attachedEnemy.spriteRenderer.sprite = sprites.Idle;

        attachedEnemy.Moving += UpdateAnimation;
        attachedEnemy.Attacking += OnAttack;
    }

    private void UpdateAnimation()
    {
        if (attacking) return;
        if (Time.time < lastAnimStateChange + SWAP_DEBOUNCE) return;

        Sprite newSprite;
        if (lastWalkSprite == null || lastWalkSprite == sprites.WalkB)
            newSprite = sprites.WalkA;
        else
            newSprite = sprites.WalkB;

        lastWalkSprite = newSprite;
        lastAnimStateChange = Time.time;
        attachedEnemy.spriteRenderer.sprite = newSprite;
    }

    private void OnAttack()
    {
        if (attacking) return;
        StartCoroutine(AttackAnimation());
    }

    private IEnumerator AttackAnimation()
    {
        attacking = true;
        attachedEnemy.spriteRenderer.sprite = sprites.Attack;
        yield return new WaitForSeconds(0.25f);
        attachedEnemy.spriteRenderer.sprite = sprites.Idle;

        yield return new WaitForSeconds(attachedEnemy.fireRate);
        attacking = false;
    }

    private void OnDestroy()
    {
        if (attachedEnemy == null) return;

        attachedEnemy.Moving -= UpdateAnimation;
        attachedEnemy.Attacking -= OnAttack;
    }
}