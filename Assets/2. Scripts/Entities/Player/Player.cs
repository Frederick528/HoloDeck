using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UniRx;
using System;

public class Player : Entity
{
    //public static Player Instance { get; private set; }
    public int MaxHolo { get; private set; }
    public int CurHolo { get; private set; }

    public ReactiveProperty<int> Coin { get; private set; } = new();
    //public ReactiveProperty<int> AttackPower { get; private set; } = new();
    //public ReactiveProperty<int> DefensePower { get; private set; } = new();
    //public ReactiveProperty<int> HealPower { get; private set; } = new();

    readonly int _battleAnimBool = Animator.StringToHash("Battle");
    readonly int _runningAnim = Animator.StringToHash("Running");
    readonly int _enterAnimBool = Animator.StringToHash("Enter");

    //bool _resurrection = false;
    //public int _attackPower;
    //public int _defensePower;
    //public int _healPower;

    //[SerializeField] TMP_Text holoValue;
    // Start is called before the first frame update

    //private void Awake()
    //{
    //    if (Instance == null)
    //    {
    //        Instance = this;
    //        GameManager.Instance.AddInGameDontDestroy(transform.root.gameObject);
    //        //DontDestroyOnLoad(transform.root.gameObject);
    //    }
    //    else
    //    {
    //        Destroy(transform.root.gameObject);
    //    }
    //}
    public void SpawnPlayer()
    {
        PlayerSubScribe();
        if (GameManager.Instance.PlayerInt == 0)
        {
            SetupPlayer(150, 5, 130);
            AddStatusEffect((StatusEffect.Reflection, StatusEffectType.Perpetual), 3);
        }
        else
        {
            SetupPlayer(80, 25, 200);
            AddStatusEffect((StatusEffect.Vampire, StatusEffectType.Perpetual), 3);
        }
    }

    void PlayerSubScribe()
    {
        EntitySubScribe();
        MaxHP.Subscribe(maxHP => InGameUIManager.Instance.ChangeStatus(0, CurHP.Value, maxHP)).AddTo(this);
        CurHP.Subscribe(curHP => InGameUIManager.Instance.ChangeStatus(0, curHP, MaxHP.Value)).AddTo(this);

        Coin.Subscribe(coin =>
        {
            InGameUIManager.Instance.ChangeStatus(7, coin);
            InGameUIManager.Instance.SetCoin(coin);
        }).AddTo(this);

        AttackPower.Subscribe(atk =>
        {
            InGameUIManager.Instance.ChangeStatus(1, atk);
            CardManager.Instance.ChangeTotalCardDesc();
            ItemManager.Instance.ActiveItemDataReset();
        }).AddTo(this);

        DefensePower.Subscribe(def =>
        {
            InGameUIManager.Instance.ChangeStatus(2, def);
            CardManager.Instance.ChangeTotalCardDesc();
            ItemManager.Instance.ActiveItemDataReset();
        }).AddTo(this);

        HealPower.Subscribe(heal =>
        {
            InGameUIManager.Instance.ChangeStatus(3, heal);
            CardManager.Instance.ChangeTotalCardDesc();
            ItemManager.Instance.ActiveItemDataReset();
        }).AddTo(this);

        _criticalChance.Subscribe(criChance =>
        {
            InGameUIManager.Instance.ChangeStatus(4, criChance);
        }).AddTo(this);

        CriticalDamage.Subscribe(criDamage => InGameUIManager.Instance.ChangeStatus(5, criDamage)).AddTo(this);

        _useCritical.Subscribe(useCri => InGameUIManager.Instance.ChangeStatus(6, _curCritical.Value, useCri)).AddTo(this);

        _curCritical.Subscribe(curCri => InGameUIManager.Instance.ChangeStatus(6, curCri, _useCritical.Value)).AddTo(this);

    }

    void SetupPlayer(int hp, int criticalChance = 10, int criticalDamage = 150)
    {
        //SetupEntity(hp + GameManager.Instance.AddMaxHP, criticalChance + GameManager.Instance.AddCriticalChance, criticalDamage + GameManager.Instance.AddCriticalDamage);
        MaxHP.Value = hp/* + GameManager.Instance.AddMaxHP*/;
        _criticalChance.Value = criticalChance/* + GameManager.Instance.AddCriticalChance*/;
        CriticalDamage.Value = criticalDamage/* + GameManager.Instance.AddCriticalDamage*/;
        //AttackPower.Value = GameManager.Instance.AddAttackPower;
        //DefensePower.Value = GameManager.Instance.AddDefensePower;
        //HealPower.Value = GameManager.Instance.AddHealPower;

        AddStatusEffect((StatusEffect.HPUp, StatusEffectType.Perpetual), GameManager.Instance.AddMaxHP);
        AddStatusEffect((StatusEffect.ATKUp, StatusEffectType.Perpetual), GameManager.Instance.AddAttackPower);
        AddStatusEffect((StatusEffect.DEFUp, StatusEffectType.Perpetual), GameManager.Instance.AddDefensePower);
        AddStatusEffect((StatusEffect.HealUp, StatusEffectType.Perpetual), GameManager.Instance.AddHealPower);
        AddStatusEffect((StatusEffect.CriticalChanceUp, StatusEffectType.Perpetual), GameManager.Instance.AddCriticalChance);
        AddStatusEffect((StatusEffect.CriticalDamageUp, StatusEffectType.Perpetual), GameManager.Instance.AddCriticalDamage);
        AddStatusEffect((StatusEffect.Resurrection, StatusEffectType.UseAmountPerpetual), GameManager.Instance.Resurrection);
        AddStatusEffect((StatusEffect.CoinGained, StatusEffectType.Perpetual), GameManager.Instance.AddCoinGained);
        
        CurHP.Value = MaxHP.Value;

        MaxHolo = 3;
        CurHolo = MaxHolo;
        InGameUIManager.Instance.SetHolo(CurHolo, MaxHolo);

        //AddAttackPower(GameManager.Instance.AddAttackPower);
        //AddDefencePower(GameManager.Instance.AddDefensePower);
        //AddHealPower(GameManager.Instance.AddHealPower);
    }

    public async override UniTask<bool> TakeDamage(int dmg, Entity attacker = null)
    {
        if (!await base.TakeDamage(dmg, attacker))
        {
            return false;
        }
        TurnManager.Instance.EndBattle().Forget();
        await base.DieAnimation(true);
        print("플레이어가 죽었습니다.");
        InGameUIManager.Instance.SetActiveCanvas(InGameUIManager.CanvasName.GameOver, true);
        return true;

    }


    //public async UniTaskVoid TakeDamagePlayer(int dmg)
    //{
    //    if (!TakeDamage(dmg))
    //        return;
    //    if (GameManager.Instance.Resurrection/* && !_resurrection*/)
    //    {
    //        _curHP.Value = (int)(_maxHP.Value * 0.5f);
    //        //_resurrection = true;
    //        return;
    //    }
    //    canvas.gameObject.SetActive(false);
    //    TurnManager.Instance.EndBattle().Forget();
    //    await base.DieAnimation();
    //    print("플레이어가 죽었습니다.");
    //    InGameUIManager.Instance.SetActiveCanvas(InGameUIManager.CanvasName.GameOver, true);
    //    //return true;
    //}

    public void AddCurHolo(int chargeOrUse)
    {
        CurHolo = Mathf.Clamp(CurHolo + chargeOrUse, 0, MaxHolo);
        InGameUIManager.Instance.SetHolo(CurHolo, MaxHolo);
    }

    public void AddMaxHealth(int value)
    {
        MaxHP.Value += value;
        Heal(value).Forget();
    }

    public void AddMaxHolo(int value)
    {
        MaxHolo += value;
        if (MaxHolo < 0)
            MaxHolo = 0;
        //InGameUIManager.Instance.SetHolo(CurHolo, MaxHolo);
    }
    //public void AddAttackPower(int value)
    //{
    //    AttackPower.Value += value;
    //}
    //public void AddDefencePower(int value)
    //{
    //    DefensePower.Value += value;
    //}
    //public void AddHealPower(int value)
    //{
    //    HealPower.Value += value;
    //}

    public void StartOrEndBattle(bool isBattleStart)
    {
        animator.SetBool(_battleAnimBool, isBattleStart);
    }

    public async UniTask EnterChapterDoor(Action isEnter = null, bool changeScene = false)
    {
        Quaternion defaultRot = animator.transform.rotation;
        Vector3 defaultPos = animator.transform.localPosition;
        MapManager.Instance.StopMove = true;

        animator.Play(_runningAnim);
        animator.SetBool(_enterAnimBool, true);
        await RotationTask(0.2f, defaultRot, Quaternion.Euler(defaultRot.x, 90, defaultRot.z));
        await MoveTask(0.45f, defaultPos, new Vector3(4, 0));
        animator.SetBool(_enterAnimBool, false);
        await RotationTask(0.2f, Quaternion.Euler(defaultRot.x, 90, defaultRot.z), Quaternion.identity);
        await OutGameUIManager.Instance.FadeOut(0.55f);
        //InGameUIManager.Instance.SetActiveCanvas(InGameUIManager.CanvasName.RewardBox, false);          // 방 생성 후, 몇몇 UI 비활성화 (상자)
        int? boxIdx = MapManager.Instance.CurShowBoxIdx;
        if (boxIdx != null)
        {
            MapManager.Instance.ShowBox(boxIdx.Value, false);
            MapManager.Instance.CurShowBoxIdx = null;

        }
        MapManager.Instance.ShowBox((int)Map.BoxType.Drop, false);
        InGameUIManager.Instance.SetActiveCanvas(InGameUIManager.CanvasName.Shop, false);               // 방 생성 후, 몇몇 UI 비활성화 (상점 보상)
        //MapManager.Instance.HideReward(MapManager.Instance.currStage);

        if (changeScene)
        {
            await GameManager.Instance.ChangeScene();
        }
        isEnter?.Invoke();
        animator.Play(_runningAnim);
        animator.SetBool(_enterAnimBool, true);
        animator.transform.rotation = Quaternion.Euler(defaultRot.x, 90, defaultRot.z);
        OutGameUIManager.Instance.FadeIn(0.85f).Forget();
        await MoveTask(0.65f, new Vector3(-4, 0), defaultPos);
        animator.SetBool(_enterAnimBool, false);

        await RotationTask(0.2f, animator.transform.rotation, defaultRot);


        MapManager.Instance.StopMove = false;
    }

    public async UniTask ExitAndEnterStage(Action isEnter)
    {
        Quaternion defaultRot = animator.transform.rotation;
        Vector3 defaultPos = animator.transform.localPosition;
        MapManager.Instance.StopMove = true;

        animator.Play(_runningAnim);
        animator.SetBool(_enterAnimBool, true);
        await RotationTask(0.2f, defaultRot, Quaternion.Euler(defaultRot.x, -90, defaultRot.z));
        OutGameUIManager.Instance.FadeOut(0.35f).Forget();
        MapManager.Instance.HideReward(MapManager.Instance.currStage);
        await MoveTask(0.45f, defaultPos, new Vector3(-4, 0));
        isEnter();
        animator.transform.rotation = Quaternion.Euler(defaultRot.x, 90, defaultRot.z);
        //await EnterStage(defaultPos);
        OutGameUIManager.Instance.FadeIn(0.55f).Forget();
        await MoveTask(0.65f, new Vector3(-4, 0), defaultPos);
        animator.SetBool(_enterAnimBool, false);

        await RotationTask(0.2f, animator.transform.rotation, defaultRot);


        MapManager.Instance.StopMove = false;
        //animator.transform.rotation = defaultRot;


        //if (isEnter)
        //{
        //    animator.Play("Running");
        //    animator.SetBool("Enter", true);
        //    await MoveTask(1f, new Vector3(-4, 0), animator.transform.localPosition);
        //    animator.SetBool("Enter", false);
        //}
        //else
        //{
        //    animator.Play("Running");
        //    await MoveTask(1f, animator.transform.localPosition, new Vector3(-4, 0));
        //    animator.SetTrigger("Exit");
        //}
    }
    /// <summary>
    /// 맨 처음 시작할 때와 ExitAndEnterStage 함수에서만 사용함.
    /// </summary>
    /// <param name="canMove"></param>
    /// <param name="defaultPos"></param>
    /// <returns></returns>
    public async UniTask EnterStage(Vector3 defaultPos = default)
    {
        OutGameUIManager.Instance.FadeIn(0.55f).Forget();
        await MoveTask(0.65f, new Vector3(-4, 0), defaultPos);
        animator.SetBool(_enterAnimBool, false);
    }

    public async UniTask MoveTask(float time, Vector3 startPos, Vector3 endPos)
    {
        animator.transform.localPosition = startPos;
        //float elapsedTime = 0f;

        //while (elapsedTime < time)
        //{
        //    elapsedTime += Time.deltaTime;
        //    float t = elapsedTime / time;
        //    animator.transform.localPosition = Vector3.Lerp(animator.transform.localPosition, endPos, t);
        //    await UniTask.Yield(); // 다음 프레임으로 넘김
        //}
        float moveSpeed = Vector3.Distance(animator.transform.localPosition, endPos) / time;

        while (Vector3.Distance(animator.transform.localPosition, endPos) > 0.01f)
        {
            animator.transform.localPosition = Vector3.MoveTowards(animator.transform.localPosition, endPos, moveSpeed * Time.deltaTime);
            await UniTask.Yield(); // 다음 프레임으로 넘김
        }

        animator.transform.localPosition = endPos; // 정확한 위치 고정
    }

    public async UniTask RotationTask(float time, Quaternion startRot, Quaternion endRot)
    {
        animator.transform.localRotation = startRot;
        float rotateSpeed = Quaternion.Angle(animator.transform.localRotation, endRot) / time;

        while (Quaternion.Angle(animator.transform.localRotation, endRot) > 0.1f)
        {

            animator.transform.localRotation = Quaternion.RotateTowards(animator.transform.localRotation, endRot, rotateSpeed * Time.deltaTime);
            await UniTask.Yield(); // 다음 프레임으로 넘김
        }

        animator.transform.localRotation = endRot; // 정확한 위치 고정
    }

}
