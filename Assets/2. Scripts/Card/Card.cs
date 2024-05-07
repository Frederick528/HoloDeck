using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Pool;

//public struct CardData
//{
//    public string name; // = "이름";
//    public int cost; // = 0;
//    public string descript; // = "카드 종류에 대한 설명";
//    public Sprite Sprite; // = "카드 이미지";
//}
public class Card : MonoBehaviour
{
    public IObjectPool<GameObject> CardPool { get; set; }

    [SerializeField] SpriteRenderer card;
    [SerializeField] SpriteRenderer character;
    [SerializeField] TMP_Text nameText;
    [SerializeField] TMP_Text costText;
    [SerializeField] TMP_Text desText;

    public PRS originPRS;
    //private Animator _anim;

    public CardData Data;
    public int ID;

    public bool block;

    CardAbility cardAbility = new();

    public Action<Card> cardAction { get; private set; }

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
        Data = data;
        //Data.name = data.name;
        //Data.id = data.id;
        //Data.cost = data.cost;
        //Data.damage = data.damage;
        //Data.enhancedDamage = data.enhancedDamage;
        //Data.defence = data.defence;
        //Data.enhancedDefence = data.enhancedDefence;
        //Data.count = data.count;
        //Data.enhancedCount = data.enhancedCount;
        //Data.draw = data.draw;
        //Data.enhancedDraw = data.enhancedDraw;
        //Data.cardUseDelay = data.cardUseDelay;
        //Data.descript = data.descript;
        //Data.sprite = data.sprite;
        //Data.cardTag = data.cardTag;

        nameText.text = Data.name;
        costText.text = Data.cost.ToString();
        desText.text = Data.descript;
        character.sprite = Data.sprite;

        cardAction = cardAbility.SetCardAbility(Data.id);
    }

    public async UniTask TaskMoveTransform(PRS prs, bool useDotween, float dotweenTime = 0)
    {
        if (useDotween)
        {
            await UniTask.WhenAll(
            transform.DOMove(prs.pos, dotweenTime).WithCancellation(this.GetCancellationTokenOnDestroy()),
            transform.DORotateQuaternion(prs.rot, dotweenTime).WithCancellation(this.GetCancellationTokenOnDestroy()),
            transform.DOScale(prs.scale, dotweenTime).WithCancellation(this.GetCancellationTokenOnDestroy())
                );
        }
        else
        {
            transform.position = prs.pos;
            transform.rotation = prs.rot;
            transform.localScale = prs.scale;
        }
    }

    public void MoveTransform(PRS prs, bool useDotween, float dotweenTime = 0)
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
        if (block)
            return;
        CardManager.Instance.CardMouseOver(this);
    }

    void OnMouseExit()
    {
        if (block)
            return;
        CardManager.Instance.CardMouseExit(this);
    }

    void OnMouseDown()
    {
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

        //SoundManager.instance.Play("Sounds/Effect/CardHoldSound");
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
        CardPool.Release(this.gameObject);
    }

}
