using UnityEngine;

public class Enemy_VFX : Entity_VFX
{
    [Header("反击窗口")]
    [SerializeField] private GameObject attackAlert;



    public void EnableAttackAlert(bool enable) => attackAlert.SetActive(enable);
}
