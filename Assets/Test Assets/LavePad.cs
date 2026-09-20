using UnityEngine;

public class LavePad : MonoBehaviour
{
    [Range(0, 5)][SerializeField] float dmgOverTime;
    int burnDamage = 1;

    float dmgTime;

    private void OnTriggerStay2D(Collider2D collision)
    {
        IDamage damage = collision.GetComponent<IDamage>();

        if(damage != null )
        {
            damage.TakeDamage(burnDamage);
            dmgTime += Time.deltaTime;
            //dmgTime >= dmgOverTime;
        }
    }

}
