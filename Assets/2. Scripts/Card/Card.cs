using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Text;
using System.Threading;
using TMPro;
using UniRx;
using UnityEngine;
using System.Collections.Generic;


//public struct CardData
//{
//    public string Name; // = "이름";
//    public int Cost; // = 0;
//    public string Descript; // = "카드 종류에 대한 설명";
//    public Sprite Sprite; // = "카드 이미지";
//}
public class Card : MonoBehaviour
{

    [SerializeField] SpriteRenderer _card;
    [SerializeField] SpriteRenderer _character;
    [SerializeField] SpriteRenderer _descBG;

    [SerializeField] SpriteRenderer[] _rararityBG;
    [SerializeField] TMP_Text _nameText;
    [SerializeField] TMP_Text _costText;
    [SerializeField] TMP_Text _descText;
    [SerializeField] TMP_Text _tagText;
    [SerializeField] SpriteRenderer _outline;
    private Tween curOutlineTween;


    public PRS OriginPRS;
    public Order CardOrder;
    //private Animator _anim;
    CardData _defaultData = null;
    public CardData DefaultData => _defaultData;
    //string _defaultDesc = null;
    public CardData Data { get; private set; }
    public string Desc;
    public int ID;      // 일단 혹시 몰라서 만들었으나, Data.Id로 받을 수 있음.

    public bool Block;

    public bool Used;

    public bool Selected;       // SelectCard = 내가 지금 들고있는 카드 (카드 사용 용), selectedCard = 내가 선택한 카드 (선택해서 버리기 용)

    public bool IsEnqueued;

    public CardAbility CardAbility = new();

    public Action ImmediatelyUseCard { get; private set; }
    public Action FailureBeforeUseCard { get; private set; }

    public List<Func<UniTask<bool>>> UseConditions { get; private set; }

    //public Action<Card> CardAction { get; private set; }
    //public Action CardAction { get; private set; }
    public Func<UniTask> CardTask { get; private set; }
    //public AsyncLazy CardLazy { get; private set; }

    public bool Enhanced = false;

    public Enemy TargetEnemy { get; private set; } = null;        // 카드 사용 시, 타겟에너미를 받아옴. (나중에 큐에서 체크하기 위함.)

    private ReactiveProperty<bool> _isCardUseTiming = new();

    public IReadOnlyReactiveProperty<bool> IsCardUseTiming => _isCardUseTiming;

    public bool CardUseTiming { get; private set; }
    public bool RepeatEffect { get; private set; }
    public bool AllEnemies { get; private set; }

    //public AsyncLazy PlayEffect = null;

    //BoxCollider2D _boxCollider2;

    // Start is called before the first frame update

    //private void OnEnable()
    //{
    //    var a = this.GetComponent<Animator>();
    //    //if (CardManager.Instance.sortBtn != null)
    //    //    CardManager.Instance.sortBtn.interactable = false;
    //    //Destroy(a);
    //}
    public void SetDefaultDate(CardData data)
    {
        _defaultData = data;
    }
    public void Setup(CardData data)
    {
        CardOrder = GetComponent<Order>();
        //_boxCollider2 = GetComponent<BoxCollider2D>();

        SetDefaultDate(data);
        Data = DefaultData.Clone();
        
        //StringBuilder sb = new StringBuilder(_defaultData.Descript);
        //sb.Replace("{Damage}", (_defaultData.Damage + InGameManager.Instance.player.AttackPower.Value).ToString());
        //sb.Replace("{Shield}", (_defaultData.Shield + InGameManager.Instance.player.DefencePower.Value).ToString());
        //sb.Replace("{Count}", (_defaultData.Count).ToString());
        //sb.Replace("{Draw}", (_defaultData.Draw).ToString());
        //sb.Replace("{Discard}", (_defaultData.Discard).ToString());
        //_defaultDesc = sb.ToString();
        //Desc = sb.ToString();


        _nameText.text = Data.Name;
        _character.sprite = Data.Sprite;
        _character.size = new Vector2(7.8f, 4.6f);
        switch (Data.CardTag)
        {
            case CardTag.SingleAttack:
            case CardTag.AllAttack:
            case CardTag.RandomAttack:
                _tagText.text = "Attack";
                break;
            case CardTag.SkillTargetSelf:
            case CardTag.SkillTargetSingle:
            case CardTag.SkillTargetAll:
            case CardTag.SkillTargetRandom:
                _tagText.text = "Skill";
                break;
        }
        switch (Data.CardRarity)
        {
            case CardRarity.Common:
                for (int i = 0; i < _rararityBG.Length; ++i)
                    _rararityBG[i].sprite = CardManager.Instance.CommonSprites[i];
                break;
            case CardRarity.Rare:
                for (int i = 0; i < _rararityBG.Length; ++i)
                    _rararityBG[i].sprite = CardManager.Instance.RareSprites[i];
                break;
            case CardRarity.Epic:
                for (int i = 0; i < _rararityBG.Length; ++i)
                    _rararityBG[i].sprite = CardManager.Instance.EpicSprites[i];
                break;
            case CardRarity.Legendary:
                for (int i = 0; i < _rararityBG.Length; ++i)
                    _rararityBG[i].sprite = CardManager.Instance.LegendarySprites[i];
                break;
        }
        CardAbility.SetCardAbility(this);
        //CardAction = CardAbility.SetCardActionAbility(this);     // Action<Card> 버전 (드로우 시간 체크 때문에 일단 사용하지 않음.)
        //CardTask = CardAbility.SetCardAbility(this);              // 그냥 카드어빌리티 실행하면 Task 바꾸도록 함.
        //CardAbility.SetCardAbility(this);     // 사용 전에 받기 때문에 굳이 사용 안 해도 됨. 나중에 따로 필요하면 킬 것.
        //CardLazy = CardAbility.SetCardLazyAbility(this);        // 중복 해결을 위해 Lazy를 써봄.

        //SendAnimEvent sendAnimEvent = Data.Effect.GetComponent<SendAnimEvent>();
        if (Data.Effect != null && Data.Effect.TryGetComponent<SendAnimEvent>(out SendAnimEvent sendAnimEvent))     // 나중에 모든 이펙트에 SendAnimEvent 넣으면 그냥 GetComponent 하면 됨.
        {
            RepeatEffect = sendAnimEvent.RepeatEffect;
            AllEnemies = sendAnimEvent.AllEnemies;
        }

        CardDataReset();        // 글(string) 데이터만 초기화
        //Data = _defaultData;
        ////Data.Name = data.Name;
        ////Data.ID = data.ID;
        ////Data.Cost = data.Cost;
        ////Data.Damage = data.Damage;
        ////Data.EnhancedDamage = data.EnhancedDamage;
        ////Data.Shield = data.Shield;
        ////Data.EnhancedDefence = data.EnhancedDefence;
        ////Data.Count = data.Count;
        ////Data.EnhancedCount = data.EnhancedCount;
        ////Data.Draw = data.Draw;
        ////Data.EnhancedDraw = data.EnhancedDraw;
        ////Data.cardUseDelay = data.cardUseDelay;
        ////Data.Descript = data.Descript;
        ////Data.Sprite = data.Sprite;
        ////Data.CardTag = data.CardTag;

        //nameText.text = Data.Name;
        //costText.text = Data.Cost.ToString();
        //desText.text = Data.Descript;
        //character.sprite = Data.Sprite;

        //CardTask = CardAbility.SetCardTaskAbility(Data.ID);
    }
    public void SetCardImmediately((Action immediately, Action failure)? action)
    {
        this.ImmediatelyUseCard = action?.immediately;
        this.FailureBeforeUseCard = action?.failure;
    }

    public void SetCardTask(Func<UniTask> cardTask)
    {
        this.CardTask = cardTask;
    }

    public void SetUseConditions(List<Func<UniTask<bool>>> condition)
    {
        this.UseConditions = condition;
    }

    public async UniTask<bool> CheckUseConditions()
    {
        //CardAbility.SetCardAbility(this);
        if (UseConditions == null || UseConditions.Count == 0)
        {
            return true;
        }

        foreach (var task in UseConditions)
        {
            if (task == null) continue;

            // 실행 후 결과 확인
            var result = await task();

            // 하나라도 실패하면 즉시 false 반환
            if (!result) return false;
        }
        return true;
    }

    public async UniTask UseTask()
    {
        //CardAbility.SetCardAbility(this);   // checkUseConditions에서 받게 되면 이건 사용 안 할 예정
        //UniTask uniTask = UniTask.Create(() => CardTask);
        //await CardAbility.SetCardAbility(this);     // 다른 방식이 있는지 찾아봐야할 듯
        if (CardTask == null) return;
        await CardTask();
        if (DefaultData.Damage != 0)
        {
            InGameManager.Instance.Player.ApplyStatusEffect(StatusEffect.ATKUp, out _);     // 공격 이후 공격력 감소 효과 적용되는 경우
        }
        if (DefaultData.Shield != 0)        // 기존 데이터값에서 쉴드값이 0이 아닌 경우 -> 방어 관련 카드라는 뜻
        {
            InGameManager.Instance.Player.ApplyStatusEffect(StatusEffect.DEFUp, out _);     // 마찬가지
        }
    }
    //public async UniTask UseLazy()
    //{
    //    await CardLazy.Task;
    //}
    //public void UseAction()
    //{
    //    CardAction?.Invoke();
    //}

    public void CardDataReset(/*bool release = false*/)
    {
        //if (release)
        //{
        //    Data.Damage = _defaultData.Damage;
        //    Data.Shield = _defaultData.Shield;
        //    Data.Count = _defaultData.Count;
        //    Data.Draw = _defaultData.Draw;
        //    costText.text = _defaultData.Cost.ToString();
        //    desText.text = _defaultDesc;
        //}
        //else
        //{
        if (DefaultData.Damage != 0)
        {
            Data.Damage = Mathf.Max(0, DefaultData.Damage + InGameManager.Instance.Player.AttackPower.Value);
        }
        if (DefaultData.Shield != 0)
        {
            Data.Shield = Mathf.Max(0, DefaultData.Shield + InGameManager.Instance.Player.DefensePower.Value);
        }

        Data.Count = DefaultData.Count + 0;
        Data.Draw = DefaultData.Draw + 0;
        Data.Discard = DefaultData.Discard + 0;

        string GetColorValue(int current, int original)
        {
            if (current > original)
                // 차분한 딥 그린 (성장/버프 느낌)
                return $"<color=#4CAF50>{current}</color>";
            else if (current < original)
                // 묵직한 다크 레드 (상처/디버프 느낌)
                return $"<color=#B71C1C>{current}</color>";
            else                         // 동일: 검정색 (기본 색상이 검정이라면 태그를 빼도 됩니다)
                return $"{current}";
        }
        
        StringBuilder sb = new StringBuilder(DefaultData.Descript);
        sb.Replace("{Damage}", GetColorValue(Data.Damage, DefaultData.Damage));
        sb.Replace("{Shield}", GetColorValue(Data.Shield, DefaultData.Shield));
        sb.Replace("{Count}", GetColorValue(Data.Count, DefaultData.Count));
        sb.Replace("{Draw}", GetColorValue(Data.Draw, DefaultData.Draw));
        sb.Replace("{Discard}", GetColorValue(Data.Discard, DefaultData.Discard));
        sb.Replace("{Remove}", GetColorValue(Data.Remove, DefaultData.Remove));
        sb.Replace("{HP}", GetColorValue(Data.HP, DefaultData.HP));
        Desc = sb.ToString();
        
        if (DefaultData.Cost == -1)
        {
            _costText.text = "X";
        }
        else
        {
            _costText.text = (DefaultData.Cost + 0).ToString();
        }
        _descText.text = Desc;
        //}

        //nameText.text = Data.Name;
        //costText.text = Data.Cost.ToString();
        ////desText.text = release? _defaultDesc : Desc;
        //character.sprite = Data.Sprite;
    }

    //public void ChangeCardDesc(/*string data*/)
    //{
    //    StringBuilder sb = new StringBuilder(_defaultData.Descript);
    //    sb.Replace("{Damage}", (_defaultData.Damage + InGameManager.Instance.player.AttackPower.Value).ToString());
    //    sb.Replace("{Shield}", (_defaultData.Shield + InGameManager.Instance.player.DefencePower.Value).ToString());
    //    sb.Replace("{Count}", (_defaultData.Count).ToString());
    //    sb.Replace("{Draw}", (_defaultData.Draw).ToString());
    //    sb.Replace("{Discard}", (_defaultData.Discard).ToString());
    //    Desc = sb.ToString();
    //    CardDataReset();
    //    //switch (data)
    //    //{
    //    //    case "Attack":
    //    //        sb.Replace("{Damage}", (_defaultData.Damage+InGameManager.Instance.player.AttackPower.Value).ToString());
    //    //        Desc = sb.ToString();
    //    //        break;
    //    //    case "Shield":
    //    //        sb.Replace("{Shield}", (_defaultData.Shield + InGameManager.Instance.player.DefencePower.Value).ToString());
    //    //        Desc = sb.ToString();
    //    //        break;
    //    //    case "Count":
    //    //        break;
    //    //    case "Draw":
    //    //        break;
    //    //}
    //}

    //public void Setup(int id)
    //{
    //    Data = InGameManager.Instance.FindCardData(id);

    //    _nameText.text = Data.Name;
    //    _costText.text = Data.Cost.ToString();
    //    _descText.text = Data.Descript;
    //    _character.sprite = Data.Sprite;

    //    //CardAction = CardAbility.SetCardActionAbility(this);
    //    //CardAbility.SetCardAbility(this);
    //    //CardLazy = CardAbility.SetCardLazyAbility(this);

    //}

    //public void EnhancedCard()
    //{
    //    if (Enhanced) return;
    //    Enhanced = true;
    //    Setup(Data.ID * 10);
    //}

    public void ResetCard()
    {
        _outline.gameObject.SetActive(false);
    }

    public void Target(Enemy enemy)     // 이거 필요없음. 죽는 적은 애초에 지정이 안 되기 때문에 따로 타켓 안 해도 됨.
    {
        TargetEnemy = enemy;
    }

    public async UniTask TurnOnOutline(bool isOn)
    {
        if (isOn)
        {
            curOutlineTween?.Kill();
            _outline.material.SetFloat("_Thickness", 0.9f);
            _outline.gameObject.SetActive(true);
            curOutlineTween = _outline.material.DOFloat(1f, "_Thickness", 0.3f).SetUpdate(true);
        }
        else
        {
            curOutlineTween?.Kill();
            curOutlineTween = _outline.material.DOFloat(0.9f, "_Thickness", 0.5f).SetUpdate(true);
            await curOutlineTween.ToUniTask(cancellationToken: TurnManager.Instance.CancelSource.Token).SuppressCancellationThrow();
            _outline.gameObject.SetActive(false);
        }
    }

    public async UniTask TaskMoveTransform(PRS prs, CancellationToken cancellationToken, float dotweenTime = 0, Ease ease = Ease.OutQuad)
    {
        //AutoSyncTr(dotweenTime).Forget();           // 그냥 여기서 켰다가 밑에서 꺼도 되지만, 그냥 함수 하나로 퉁치기
        await UniTask.WhenAll(
        //AutoSyncTr(dotweenTime),
            transform.DOMove(prs.pos, dotweenTime).SetUpdate(true).SetEase(ease).WithCancellation(cancellationToken)/*.SuppressCancellationThrow()*/,
            transform.DORotateQuaternion(prs.rot, dotweenTime).SetUpdate(true).SetEase(ease).WithCancellation(cancellationToken)/*.SuppressCancellationThrow()*/,
            transform.DOScale(prs.scale, dotweenTime).SetUpdate(true).SetEase(ease).WithCancellation(cancellationToken)/*.SuppressCancellationThrow()*/
            );
        //if (battleCancel)
        //{
        //    //AutoSyncTr(dotweenTime).Forget();           // 그냥 여기서 켰다가 밑에서 꺼도 되지만, 그냥 함수 하나로 퉁치기
        //    await UniTask.WhenAll(
        //    //AutoSyncTr(dotweenTime),
        //    transform.DOMove(prs.pos, dotweenTime).SetUpdate(true).SetEase(ease).WithCancellation(TurnManager.Instance.CancelSource.Token)/*.SuppressCancellationThrow()*/,
        //    transform.DORotateQuaternion(prs.rot, dotweenTime).SetUpdate(true).SetEase(ease).WithCancellation(TurnManager.Instance.CancelSource.Token)/*.SuppressCancellationThrow()*/,
        //    transform.DOScale(prs.scale, dotweenTime).SetUpdate(true).SetEase(ease).WithCancellation(TurnManager.Instance.CancelSource.Token)/*.SuppressCancellationThrow()*/
        //    );
        //}
        //else
        //{
        //    await UniTask.WhenAll(
        //    //AutoSyncTr(dotweenTime),
        //    transform.DOMove(prs.pos, dotweenTime).SetUpdate(true).SetEase(ease).WithCancellation(this.GetCancellationTokenOnDestroy()),
        //    transform.DORotateQuaternion(prs.rot, dotweenTime).SetUpdate(true).SetEase(ease).WithCancellation(this.GetCancellationTokenOnDestroy()),
        //    transform.DOScale(prs.scale, dotweenTime).SetUpdate(true).SetEase(ease).WithCancellation(this.GetCancellationTokenOnDestroy())
        //    );
        //}
    }
    public void MoveTransform(PRS prs, bool useDotween = false, float dotweenTime = 0, Ease ease = Ease.OutQuad/*, bool ignoreTimeScale = false*/)
    {
        //Physics2D.SyncTransforms();
        //Physics2D.autoSyncTransforms = true;
        if (useDotween)
        {
            //AutoSyncTr(dotweenTime).Forget();
            transform.DOMove(prs.pos, dotweenTime).SetUpdate(true).SetEase(ease);
            transform.DORotateQuaternion(prs.rot, dotweenTime).SetUpdate(true).SetEase(ease);
            transform.DOScale(prs.scale, dotweenTime).SetUpdate(true).SetEase(ease);
        }
        else
        {
            transform.position = prs.pos;
            transform.rotation = prs.rot;
            transform.localScale = prs.scale;
            //if (InGameManager.Instance.PauseInt != 0)
            //{
            //    Physics2D.SyncTransforms();
            //    //SetPRSCollider();
            //}
        }
    }

    public void CardTiming()
    {
        //CardUseTiming = true;
        _isCardUseTiming.Value = true;
    }

    public void UseTimingReset()
    {
        //CardUseTiming = false;
        _isCardUseTiming.Value = false;
    }

    public async UniTaskVoid WaitUnblock(float waitTime)
    {
        Block = true;
        await UniTask.WaitForSeconds(waitTime, true);       // 카드를 가져오기 위해 블락하는 거라, TimeScale은 무시함.
        Block = false;
    }

    public void BlockCard()
    {
        Block = true;
    }
    public void UnblockCard()
    {
        Block = false;
    }

    public void FailedUseCard()
    {
        Block = false;
        Used = false;
    }

    //public void CheckEnemyDead()
    //{
    //    int count = (Data.Count == 0) ? 1 : Data.Count;
    //    switch (Data.CardTag)
    //    {
    //        case CardTag.SingleAttack:
    //            if (InGameManager.Instance.Player.GetStatusEffect(StatusEffect.UseCritical, out _))
    //                TargetEnemy.CheckIfDead(Mathf.RoundToInt(Data.Damage * InGameManager.Instance.Player.CriticalDamage.Value * 0.01f +0.0001f), count);
    //            else
    //            {
    //                TargetEnemy.CheckIfDead(Data.Damage, count);
    //            }
    //            break;
    //        case CardTag.AllAttack:
    //            foreach (Enemy enemy in EnemyManager.Instance.EnemyList)
    //            {
    //                if (InGameManager.Instance.Player.GetStatusEffect(StatusEffect.UseCritical, out _))
    //                    enemy.CheckIfDead(Mathf.RoundToInt(Data.Damage * InGameManager.Instance.Player.CriticalDamage.Value * 0.01f + 0.0001f), count);
    //                else
    //                {
    //                    enemy.CheckIfDead(Data.Damage, count);
    //                }
    //            }
    //            break;
    //        default: break;
    //    }
    //}

    public async UniTask<bool> BeforeUsingCard()
    {
        if (DefaultData.Cost == -1)
        {
            if (InGameManager.Instance.Player.CurHolo > 0)
            {
                Data.Cost = InGameManager.Instance.Player.CurHolo;
            }
            else
            {
                Target(null);
                return false;
            }
        }
        else
        {
            if (InGameManager.Instance.Player.CurHolo < Data.Cost)
            {
                Target(null);
                return false;
            }
        }

        if ((Data.CardTag == CardTag.SingleAttack || Data.CardTag == CardTag.SkillTargetSingle) && TargetEnemy == null)
        {
            Target(null);
            return false;
        }

        //MoveTransform(new PRS(Vector3.zero, Quaternion.identity, CardUtils.CardScale * 0.8f), true, CardUtils.CardAlignmentDelay);
        if (!await CheckUseConditions())
        {
            Target(null);
            return false;
        }

        InGameManager.Instance.Player.AddCurHolo(-Data.Cost);

        Used = true;
        //CardOrder.SetOriginOrder(-10);

        //CheckEnemyDead();

        return true;
    }

    public async UniTask AfterCardAbility(bool endBattle = false)
    {
        await TaskMoveTransform(new PRS(CardManager.Instance.CardDummyTr.position, Quaternion.identity, CardUtils.CardScale * 0.5f), this.GetCancellationTokenOnDestroy(), CardUtils.ThrowAwayCardDelay);

        if (!endBattle)
        {
            CardManager.Instance.CardDummy.Add(this);
            InGameUIManager.Instance.SetDummyCount();
        }
        Block = false;
        Used = false;
    }



    //async UniTask AutoSyncTr(float time)              // TimeScale = 0 되는 곳에서 그냥 true함.
    //{
    //    Physics2D.autoSyncTransforms = true;
    //    await UniTask.WaitForSeconds(time, true).SuppressCancellationThrow();
    //    Physics2D.autoSyncTransforms = false;
    //}

    void OnMouseOver()
    {
        if (Block)
            return;
        CardManager.Instance.CardMouseOver(this);
    }

    void OnMouseExit()
    {
        if (Block)
            return;
        CardManager.Instance.CardMouseExit(this);
    }

    void OnMouseDown()
    {
        if (Block)
            return;
        CardManager.Instance.CardMouseDown(this);
        //if (InGameManager.Instance.blockClick || TurnManager.Instance.IsLoading)
        //    return;
        //comeBackCard = false;
        //draggable = true;
        //InGameManager.Instance.blockClick = true;
    }

    void OnMouseUp()
    {
        CardManager.Instance.CardMouseUp(this);


        //if (comeBackCard || TurnManager.Instance.IsLoading)
        //    return;
        
        //_rigid.isKinematic = false;


        //CollisionChecker(RayCastToken);

        //SoundManager.Instance.Play("Sounds/Effect/CardHoldSound");
        //CardManager.Instance.sortBtn.interactable = false;


        //CardManager.Instance.sortBtn.interactable = true;
    }
    void OnMouseDrag()
    {
        CardManager.Instance.CardDrag(this);

        ////if (!draggable)
        ////    return;
        ////float distance = Camera.main.WorldToScreenPoint(transform.position).z;
        ////print(distance);
        //Vector2 _temp = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        //transform.position = _temp/*new Vector3(_temp.x, _temp.y, -5f)*/;

    }

    public void CardRelease()
    {
        PoolManager.Instance.ReleaseCard(/*this.gameObject, */this);
    }

    //private void OnEnable()
    //{
    //    CardAbility.CancelSource = new();
    //}

    //private void OnDisable()
    //{
    //    CardAbility.CancelSource.Cancel();
    //}

    //private void OnDestroy()
    //{
    //    CardAbility.CancelSource.Cancel();
    //    CardAbility.CancelSource.Dispose();
    //}
}
