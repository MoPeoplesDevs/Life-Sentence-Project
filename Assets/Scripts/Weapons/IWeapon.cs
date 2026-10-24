using UnityEngine;

public interface IWeapon
{

    void Shoot();

    int CurrentAmmo { get; }
    int ReserveAmmo { get; }

}
