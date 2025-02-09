using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Pool;

//public struct CardData
//{
//    public string Name; // = "이름";
//    public int Cost; // = 0;
//    public string Descript; // = "카드 종류에 대한 설명";
//    public Sprite Sprite; // = "카드 이미지";
//}
public class Card : MonoBehaviour
{
    public IObjectPool<Tuple<GameObject, Card>> CardPool { get; set; }

    [SerializeField] SpriteRenderer _card;
    [SerializeField] SpriteRenderer _character;
    [SerializeField] SpriteRenderer _descBG;

    [SerializeField] SpriteRenderer[] _rararityBG;
    [SerializeField] TMP_Text _nameText;
    [SerializeField] TMP_Text _costText;
    [SerializeField] TMP_Text _descText;
    [SerializeField] TMP_Text _tagText;
    [SerializeField] SpriteRenderer _outline;

    public PRS OriginPRS;
    public Order CardOrder;
    //private Animator _anim;

    CardData _defaultData = null;
    string _defaultDesc = null;
    public CardData Data;
    public string Desc;
    public int ID;      // 일단 혹시 몰라서 만들었으나, Data.Id로 받을 수 있음.

    public bool Block;

    public bool Used;

    public bool Selected;       // _selectCard = 내가 지금 들고있는 카드 (카드 사용 용), selectedCard = 내가 선택한 카드 (선택해서 버리기 용)

    public CardAbility CardAbility = new();

    public UniTask<bool>? UseConditions;

    //public Action<Card> CardAction { get; private set; }
    //public Action CardAction { get; private set; }
    public UniTask CardTask;
    //public AsyncLazy CardLazy { get; private set; }

    public bool Enhanced = false;

    public Enemy TargetEnemy { get; private set; } = null;        // 카드 사용 시, 타겟에너미를 받아옴. (나중에 큐에서 체크하기 위함.)

    //BoxCollider2D _boxCollider2;

    // Start is called before the first frame update

    //private void OnEnable()
    //{
    //    var a = this.GetComponent<Animator>();
    //    //if (CardManager.Instance.sortBtn != null)
    //    //    CardManager.Instance.sortBtn.interactable = false;
    //    //Destroy(a);
    //}

    public void Setup(CardData data)
    {
        CardOrder = GetComponent<Order>();
        //_boxCollider2 = GetComponent<BoxCollider2D>();

        _defaultData = data;
        StringBuilder sb = new StringBuilder(_defaultData.Descript);
        sb.Replace("{Damage}", (_defaultData.Damage + InGameManager.Instance.player.AttackPower.Value).ToString());
        sb.Replace("{Shield}", (_defaultData.Shield + InGameManager.Instance.player.DefencePower.Value).ToString());
        sb.Replace("{Count}", (_defaultData.Count).ToString());
        sb.Replace("{Draw}", (_defaultData.Draw).ToString());
        sb.Replace("{Reduce}", (_defaultData.Reduce).ToString());
        _defaultDesc = sb.ToString();
        Desc = sb.ToString();

        Data = (CardData)_defaultData.Clone();

        _nameText.text = Data.Name;
        _character.sprite = Data.Sprite;
        switch (Data.CardTag)
        {
            case CardTag.SingleAttack:
            case CardTag.MultiAttack:
                _tagText.text = "Attack";
                break;
            case CardTag.Skill:
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
        //CardAction = CardAbility.SetCardActionAbility(this);     // Action<Card> 버전 (드로우 시간 체크 때문에 일단 사용하지 않음.)
        //CardTask = CardAbility.SetCardAbility(this);              // 그냥 카드어빌리티 실행하면 Task 바꾸도록 함.
        //CardAbility.SetCardAbility(this);     // 사용 전에 받기 때문에 굳이 사용 안 해도 됨. 나중에 따로 필요하면 킬 것.
        //CardLazy = CardAbility.SetCardLazyAbility(this);        // 중복 해결을 위해 Lazy를 써봄.

        CardDataReset();
        //Data = _defaultData;
        ////Data.Name = data.Name;
        ////Data.Id = data.Id;
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

        //CardTask = CardAbility.SetCardTaskAbility(Data.Id);
    }

    public async UniTask<bool> CheckUseConditions()
    {
        CardAbility.SetCardAbility(this);
        if (!UseConditions.HasValue)
        {
            return true;
        }
        return await UseConditions.Value;
    }

    public async UniTask UseTask()
    {
        //CardAbility.SetCardAbility(this);   // checkUseConditions에서 받게 되면 이건 사용 안 할 예정
        //UniTask uniTask = UniTask.Create(() => CardTask);
        //await CardAbility.SetCardAbility(this);     // 다른 방식이 있는지 찾아봐야할 듯
        await CardTask;
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
            Data.Damage = _defaultData.Damage + InGameManager.Instance.player.AttackPower.Value;
            Data.Shield = _defaultData.Shield + InGameManager.Instance.player.DefencePower.Value;
            Data.Count = _defaultData.Count + 0;
            Data.Draw = _defaultData.Draw + 0;
            Data.Reduce = _defaultData.Reduce + 0;
            _costText.text = (_defaultData.Cost + 0).ToString();
            _descText.text = Desc;
        //}

        //nameText.text = Data.Name;
        //costText.text = Data.Cost.ToString();
        ////desText.text = release? _defaultDesc : Desc;
        //character.sprite = Data.Sprite;
    }

    public void ChangeCardDesc(/*string data*/)
    {
        StringBuilder sb = new StringBuilder(_defaultData.Descript);
        sb.Replace("{Damage}", (_defaultData.Damage + InGameManager.Instance.player.AttackPower.Value).ToString());
        sb.Replace("{Shield}", (_defaultData.Shield + InGameManager.Instance.player.DefencePower.Value).ToString());
        sb.Replace("{Count}", (_defaultData.Count).ToString());
        sb.Replace("{Draw}", (_defaultData.Draw).ToString());
        sb.Replace("{Reduce}", (_defaultData.Reduce).ToString());
        Desc = sb.ToString();
        CardDataReset();
        //switch (data)
        //{
        //    case "Attack":
        //        sb.Replace("{Damage}", (_defaultData.Damage+InGameManager.Instance.player.AttackPower.Value).ToString());
        //        Desc = sb.ToString();
        //        break;
        //    case "Shield":
        //        sb.Replace("{Shield}", (_defaultData.Shield + InGameManager.Instance.player.DefencePower.Value).ToString());
        //        Desc = sb.ToString();
        //        break;
        //    case "Count":
        //        break;
        //    case "Draw":
        //        break;
        //}
    }

    public void Setup(int id)
    {
        Data = InGameManager.Instance.FindCardData(id);

        _nameText.text = Data.Name;
        _costText.text = Data.Cost.ToString();
        _descText.text = Data.Descript;
        _character.sprite = Data.Sprite;

        //CardAction = CardAbility.SetCardActionAbility(this);
        //CardAbility.SetCardAbility(this);
        //CardLazy = CardAbility.SetCardLazyAbility(this);

    }

    public void EnhancedCard()
    {
        if (Enhanced) return;
        Enhanced = true;
        Setup(Data.Id * 10);
    }

    public void ResetCard()
    {
        _outline.gameObject.SetActive(false);
    }

    public void Target(Enemy enemy)
    {
        TargetEnemy = enemy;
    }

    public void TurnOnOutline(bool isOn)
    {
        _outline.gameObject.SetActive(isOn);
    }

    public async UniTask TaskMoveTransform(PRS prs, bool battleCancel, float dotweenTime = 0)
    {
        if (battleCancel)
        {
            //AutoSyncTr(dotweenTime).Forget();           // 그냥 여기서 켰다가 밑에서 꺼도 되지만, 그냥 함수 하나로 퉁치기
            await UniTask.WhenAll(
            //AutoSyncTr(dotweenTime),
            transform.DOMove(prs.pos, dotweenTime).SetUpdate(true).WithCancellation(TurnManager.Instance.CancelSource.Token)/*.SuppressCancellationThrow()*/,
            transform.DORotateQuaternion(prs.rot, dotweenTime).SetUpdate(true).WithCancellation(TurnManager.Instance.CancelSource.Token)/*.SuppressCancellationThrow()*/,
            transform.DOScale(prs.scale, dotweenTime).SetUpdate(true).WithCancellation(TurnManager.Instance.CancelSource.Token)/*.SuppressCancellationThrow()*/
            );
        }
        else
        {
            await UniTask.WhenAll(
            //AutoSyncTr(dotweenTime),
            transform.DOMove(prs.pos, dotweenTime).SetUpdate(true).WithCancellation(this.GetCancellationTokenOnDestroy()),
            transform.DORotateQuaternion(prs.rot, dotweenTime).SetUpdate(true).WithCancellation(this.GetCancellationTokenOnDestroy()),
            transform.DOScale(prs.scale, dotweenTime).SetUpdate(true).WithCancellation(this.GetCancellationTokenOnDestroy())
            );
        }
    }
    public void MoveTransform(PRS prs, bool useDotween = false, float dotweenTime = 0/*, bool ignoreTimeScale = false*/)
    {
        //Physics2D.SyncTransforms();
        //Physics2D.autoSyncTransforms = true;
        if (useDotween)
        {
            //AutoSyncTr(dotweenTime).Forget();
            transform.DOMove(prs.pos, dotweenTime).SetUpdate(true);
            transform.DORotateQuaternion(prs.rot, dotweenTime).SetUpdate(true);
            transform.DOScale(prs.scale, dotweenTime).SetUpdate(true);
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
        CardManager.Instance.CardMouseUp(this).Forget();


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
        PoolManager.Instance.ReleaseCard(this.gameObject, this);
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
