using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpecialEnemyMarker : MonoBehaviour
{
    public int LevelNum;
   
    // Start is called before the first frame update
    void Start()
    {
        LevelNum = 1;
    }

    public bool CheckWeaponType(int weaponType)
    {
        if (this.gameObject.tag == "LevelSpecialEnemy")
        {
            if (weaponType == LevelNum)
            {
                return true;
            }
        }
        return false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Weapon")
        {
            if (other.GetComponent<SpecialWeaponMarker>() == null)
                return;
            if (other.GetComponent<SpecialWeaponMarker>().weaponType == LevelNum)
            {
                other.GetComponent<SpecialWeaponMarker>().isEnhanced = true;
            }
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Weapon")
        {
            if (other.GetComponent<SpecialWeaponMarker>() == null)
                return;
            if (other.GetComponent<SpecialWeaponMarker>().weaponType == LevelNum)
            {
                other.GetComponent<SpecialWeaponMarker>().isEnhanced = false;
            }
        }
    }
}
