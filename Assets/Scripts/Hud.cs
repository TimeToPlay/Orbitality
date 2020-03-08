using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class Hud : MonoBehaviour, IPoolable<IMemoryPool>
{
    private IMemoryPool _pool;
    private int _maxHp;
    private int _cooldown;
    [SerializeField] private Image hpBar;
    [SerializeField] private Image cooldownBar;
    [SerializeField] private TextMeshProUGUI nicknameText;
    private RectTransform _rectTransform;
    private Camera _camera;

    [Inject]
    void Construct(Camera cam)
    {
        _camera = cam;
    }
    private void Start()
    {
        _rectTransform = GetComponent<RectTransform>();
    }

    public void Configure(string nickname, bool isPlayer, int maxHp, int currentHP)
    {
        nicknameText.text = nickname;
        hpBar.color = isPlayer ? Color.green : Color.red;
        cooldownBar.gameObject.SetActive(isPlayer);
        cooldownBar.fillAmount = 0;
        _maxHp = maxHp;
        SetNewHp(currentHP);
    }

    public void SetNewHp(int hp)
    {
        hpBar.fillAmount = (float) hp / _maxHp;
    }

    public void TransformWorldPosition(Vector3 worldPosition)
    {
         _rectTransform.position = _camera.WorldToScreenPoint(worldPosition);
    }
    public void OnDespawned()
    {
    }

    public void OnSpawned(IMemoryPool pool)
    {
        _pool = pool;
    }
    public class Factory : PlaceholderFactory<Hud>{}

    public void Despawn()
    {
        _pool.Despawn(this);
    }

    public void SetCooldown(float cooldown)
    {
        cooldownBar.fillAmount = cooldown;
    }
}
