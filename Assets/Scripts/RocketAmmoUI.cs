using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RocketAmmoUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI ammoText;
    [SerializeField] private GameObject selectedBorder;

    public void SelectEnabled(bool enabled)
    {
        selectedBorder.SetActive(enabled);
    }

    public void SetAmmo(int ammo)
    {
        ammoText.text = ammo.ToString();
    }
}