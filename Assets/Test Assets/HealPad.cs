using UnityEngine;

public class HealPad : MonoBehaviour
{
    int healing = 1;
    float healTime = 0;
    float healOverTime = 1;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        IHeal heal = collision.GetComponent<IHeal>();

        if(heal != null)
        {
            heal.Heal(healing);
            healTime = 0;
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        IHeal heal = collision.GetComponent<IHeal>();

        if (heal != null)
        {
            healTime += Time.deltaTime;
            if (healTime >= healOverTime)
            {
                heal.Heal(healing);
                healTime = 0;
            }
        }
    }

    private void OnTrigerExit2D(Collider2D collision)
    {
        healTime = 0;
    }

}
