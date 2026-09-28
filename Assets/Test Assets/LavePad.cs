using UnityEngine;

public class LavePad : MonoBehaviour
{
    [Range(0, 5)][SerializeField] float dmgOverTime;
    [Range(1, 5)][SerializeField] int burnDamage;

    float dmgTime;

    private void OnTriggerEnter2D(Collider2D collision)
    {
		if (collision.transform.tag != "Player") return;
        
        IDamage damage = collision.GetComponent<IDamage>();

        if (damage != null )
        {
            damage.TakeDamage(burnDamage);
            dmgTime = 0;
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
		if (collision.transform.tag != "Player") return;
        
        IDamage damage = collision.GetComponent<IDamage>();

        if(damage != null )
        {
            dmgTime += Time.deltaTime;
            if(dmgTime >= dmgOverTime)
            {
                damage.TakeDamage(burnDamage);
                dmgTime = 0;
            }
        }
    }

    private void OnTriggerExit2D()
    {
        dmgTime = 0;
    }

}
