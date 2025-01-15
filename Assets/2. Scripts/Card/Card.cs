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

    [SerializeField] SpriteRenderer card;
    [SerializeField] SpriteRenderer character;
    [SerializeField] TMP_Text nameText;
    [SerializeField] TMP_Text costText;
    [SerializeField] TMP_Text desText;

    public PRS OriginPRS;
    //private Animator _anim;

    CardData _defaultData = null;
    string _defaultDesc = null;
    public CardData Data;
    public string Desc;
    public int ID;      // 일단 혹시 몰라서 만들었으나, Data.Id로 받을 수 있음.

    public bool Block;

    public bool Used;

    public CardAbility CardAbility = new();

    //public Action<Card> CardAction { get; private set; }
    public UniTask CardAction { get; private set; }

    public bool Enhanced = false;

    public Enemy TargetEnemy = null;

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
        _defaultData = data;
        StringBuilder sb = new StringBuilder(_defaultData.Descript);
        sb.Replace("{Damage}", (_defaultData.Damage + GameManager.Instance.player.AttackPower.Value).ToString());
        sb.Replace("{Defence}", (_defaultData.Defence + GameManager.Instance.player.DefencePower.Value).ToString());
        sb.Replace("{Count}", (_defaultData.Count).ToString());
        sb.Replace("{Draw}", (_defaultData.Draw).ToString());
        _defaultDesc = sb.ToString();
        Desc = sb.ToString();

        Data = (CardData)_defaultData.Clone();

        nameText.text = Data.Name;
        character.sprite = Data.Sprite;
        //CardAction = CardAbility.SetCardAbility(Data.Id);     // Action<Card> 버전 (드로우 시간 체크 때문에 일단 사용하지 않음.)
        CardAction = CardAbility.SetCardAbility(this);        // UniTask 중복 사용 불가로 인해 일단 사용 불가
        Debug.Log(TurnManager.Instance.CancelSource.Token);

        CardDataReset();
        //Data = _defaultData;
        ////Data.Name = data.Name;
        ////Data.Id = data.Id;
        ////Data.Cost = data.Cost;
        ////Data.Damage = data.Damage;
        ////Data.EnhancedDamage = data.EnhancedDamage;
        ////Data.Defence = data.Defence;
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

        //CardAction = CardAbility.SetCardAbility(Data.Id);
    }

    public void CardDataReset(bool release = false)
    {
        if (release)
        {
            Data.Damage = _defaultData.Damage;
            Data.Defence = _defaultData.Defence;
            Data.Count = _defaultData.Count;
            Data.Draw = _defaultData.Draw;
            costText.text = _defaultData.Cost.ToString();
            desText.text = _defaultDesc;
        }
        else
        {
            Data.Damage = _defaultData.Damage + GameManager.Instance.player.AttackPower.Value;
            Data.Defence = _defaultData.Defence + GameManager.Instance.player.DefencePower.Value;
            Data.Count = _defaultData.Count + 0;
            Data.Draw = _defaultData.Draw + 0;
            costText.text = (_defaultData.Cost + 0).ToString();
            desText.text = Desc;
        }

        //nameText.text = Data.Name;
        //costText.text = Data.Cost.ToString();
        ////desText.text = release? _defaultDesc : Desc;
        //character.sprite = Data.Sprite;
    }

    public void ChangeCardDesc(/*string data*/)
    {
        StringBuilder sb = new StringBuilder(_defaultData.Descript);
        sb.Replace("{Damage}", (_defaultData.Damage + GameManager.Instance.player.AttackPower.Value).ToString());       //  나중에 여기 부분 고치자. 위에 else랑 겹침.
        sb.Replace("{Defence}", (_defaultData.Defence + GameManager.Instance.player.DefencePower.Value).ToString());
        sb.Replace("{Count}", (_defaultData.Count).ToString());
        sb.Replace("{Draw}", (_defaultData.Draw).ToString());
        Desc = sb.ToString();
        CardDataReset();
        //switch (data)
        //{
        //    case "Attack":
        //        sb.Replace("{Damage}", (_defaultData.Damage+GameManager.Instance.player.AttackPower.Value).ToString());
        //        Desc = sb.ToString();
        //        break;
        //    case "Defence":
        //        sb.Replace("{Defence}", (_defaultData.Defence + GameManager.Instance.player.DefencePower.Value).ToString());
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
        Data = CardManager.Instance.FindCardData(id);

        nameText.text = Data.Name;
        costText.text = Data.Cost.ToString();
        desText.text = Data.Descript;
        character.sprite = Data.Sprite;

        //CardAction = CardAbility.SetCardAbility(Data.Id);
        CardAction = CardAbility.SetCardAbility(this);

    }

    public void EnhancedCard()
    {
        if (Enhanced) return;
        Enhanced = true;
        Setup(Data.Id * 10);
    }

    public void Target(Enemy enemy)
    {
        TargetEnemy = enemy;
    }

    public async UniTask TaskMoveTransform(PRS prs, bool battleCancel, float dotweenTime = 0)
    {
        if (battleCancel)
        {
            await UniTask.WhenAll(
            transform.DOMove(prs.pos, dotweenTime).WithCancellation(TurnManager.Instance.CancelSource.Token)/*.SuppressCancellationThrow()*/,
            transform.DORotateQuaternion(prs.rot, dotweenTime).WithCancellation(TurnManager.Instance.CancelSource.Token)/*.SuppressCancellationThrow()*/,
            transform.DOScale(prs.scale, dotweenTime).WithCancellation(TurnManager.Instance.CancelSource.Token)/*.SuppressCancellationThrow()*/
                );
        }
        else
        {
            await UniTask.WhenAll(
            transform.DOMove(prs.pos, dotweenTime).WithCancellation(this.GetCancellationTokenOnDestroy()),
            transform.DORotateQuaternion(prs.rot, dotweenTime).WithCancellation(this.GetCancellationTokenOnDestroy()),
            transform.DOScale(prs.scale, dotweenTime).WithCancellation(this.GetCancellationTokenOnDestroy())
                );
        }
    }

    public void MoveTransform(PRS prs, bool useDotween = false, float dotweenTime = 0)
    {
        if (useDotween)
        {
            transform.DOMove(prs.pos, dotweenTime);
            transform.DORotateQuaternion(prs.rot, dotweenTime);
            transform.DOScale(prs.scale, dotweenTime);
        }
        else
        {
            transform.position = prs.pos;
            transform.rotation = prs.rot;
            transform.localScale = prs.scale;
        }
    }

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
        //if (GameManager.Instance.blockClick || TurnManager.Instance.isLoading)
        //    return;
        //comeBackCard = false;
        //draggable = true;
        //GameManager.Instance.blockClick = true;
    }

    void OnMouseUp()
    {
        CardManager.Instance.CardMouseUp(this);


        //if (comeBackCard || TurnManager.Instance.isLoading)
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
