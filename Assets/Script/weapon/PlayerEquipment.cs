using UnityEngine;

public class PlayerEquipment : MonoBehaviour
{
    [SerializeField] private Animator playerAnimator;
    private RuntimeAnimatorController defaultAnimator;
    
    private void Start()
    {
        defaultAnimator = playerAnimator.runtimeAnimatorController;
        BackpackManager.Instance.PlayerEquip = this;
    }
    
    public void EquipWeapon(WeaponData weapon)
    {
        playerAnimator.runtimeAnimatorController = weapon?.weaponAnimator ?? defaultAnimator;
    }
}