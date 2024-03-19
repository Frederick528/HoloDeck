using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Player : Entity
{
    public int maxHolo { get; private set; }
    public int curHolo { get; private set; }

    [SerializeField] TMP_Text holoValue;
    // Start is called before the first frame update
    void Start()
    {
        SetupPlayer(80, 3);
    }

    // Update is called once per frame
    void Update()
    {

    }

    void SetupPlayer(int hp, int startHoloValue)
    {
        base.SetupEntity(hp);
        maxHolo = startHoloValue;
        curHolo = maxHolo;
        holoValue.text = $"{curHolo} / {maxHolo}";
    }
    public override void TakeDamage(int dmg)
    {
        base.TakeDamage(dmg);
        if (curHp <= 0)
        {
            print("플레이어가 죽었습니다.");
        }
    }

    public void ChangeHoloValue(int chargeOrUse)
    {
        curHolo = Mathf.Clamp(curHolo + chargeOrUse, 0, maxHolo);
        holoValue.text = $"{curHolo} / {maxHolo}";
    }
}
