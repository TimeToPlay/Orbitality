using TMPro;
using UnityEngine;

public class RocketAmmoUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI ammoText;
    [SerializeField] private GameObject selectedBorder;

    public void SelectEnabled(bool b)
    {
        selectedBorder.SetActive(b);
    }

    public void SetAmmo(int ammo)
    {
        ammoText.text = ammo.ToString();
    }
}