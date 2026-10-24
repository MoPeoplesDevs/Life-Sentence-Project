using TMPro;
using UnityEngine;

public class AmmoHUD : MonoBehaviour
{
    [SerializeField] private Playercontroller playerController;
    [SerializeField] private TMP_Text ammoText;
    [SerializeField] private TMP_Text weaponNameText;

    private void Update()
    {
        if (playerController == null || ammoText == null)
        {
            return;
        }

        IWeapon weapon = playerController.GetCurrentWeapon();

        if (weapon is DefaultWeapon)
        {
            ammoText.text = "Unlimited";

            if (weaponNameText != null)
            {
                weaponNameText.text = "PISTOL";
            }

            return;
        }

        if (weapon == null)
        {
            ammoText.text = "No Ammo";

            if (weaponNameText != null)
            {
                weaponNameText.text = "No Weapon";
            }

            return;
        }

        ammoText.text = weapon.CurrentAmmo + " / " + weapon.ReserveAmmo;

        if (weaponNameText != null)
        {
            if (weapon is ShotgunWeapon)
            {
                weaponNameText.text = "SHOTGUN";
            }
            else if (weapon is MachineGun)
            {
                weaponNameText.text = "MACHINE GUN";
            }
            else if (weapon is SniperWeapon)
            {
                weaponNameText.text = "SNIPER RIFLE";
            }
            else
            {
                weaponNameText.text = "PISTOL";
            }
        }
    }
}