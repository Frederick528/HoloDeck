using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Pool;

[System.Serializable]
public struct CardData
{
    public string Name; // = "이름";
    public int Cost; // = 0;
    public string Descript; // = "카드 종류에 대한 설명";
    public Sprite Sprite; // = "카드 이미지";
}
public enum CardType
{
    Food,
    Water,
    Wood,
    Stone,
    Combination,
    Merchant
}
public class Card : MonoBehaviour
{
    public IObjectPool<GameObject> Pool { get; set; }

    [SerializeField] SpriteRenderer card;
    [SerializeField] SpriteRenderer character;
    [SerializeField] TMP_Text nameText;
    [SerializeField] TMP_Text costText;
    [SerializeField] TMP_Text desText;

    public PRS originPRS;
    //private Animator _anim;

    public CardData Data; /*{ get; private set; }*/
    public CardType cardType;
    public int ID;

    public bool block;

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
        nameText.text = data.Name;
        //costText.text = data.Cost.ToString();
        costText.text = GameManager.Instance.num.ToString();
        GameManager.Instance.num++;
        desText.text = data.Descript;
        character.sprite = data.Sprite;
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

    public void Init(int level)
    {
        //base.Init(level);

        ID = level;

        switch (cardType)
        {
            case CardType.Food:
                ID += 1010;
                GetComponent<MeshRenderer>().material =
                    Resources.Load<Material>($"Prefabs/Materials/Food/{ID}");
                break;
            case CardType.Water:
                ID += 1020;
                GetComponent<MeshRenderer>().material =
                    Resources.Load<Material>($"Prefabs/Materials/Water/{ID}");
                break;
            case CardType.Wood:
                ID += 2010;
                GetComponent<MeshRenderer>().material =
                    Resources.Load<Material>($"Prefabs/Materials/Wood/{ID}");
                break;
            case CardType.Stone:
                ID += 2020;
                GetComponent<MeshRenderer>().material =
                    Resources.Load<Material>($"Prefabs/Materials/Stone/{ID}");
                break;
            case CardType.Combination:
                ID = 3000;
                GetComponent<MeshRenderer>().material =
                    Resources.Load<Material>($"Prefabs/Materials/Combination/{ID}");
                break;
            case CardType.Merchant:
                ID = 5000;
                GetComponent<MeshRenderer>().material =
                    Resources.Load<Material>($"Prefabs/Materials/Merchant/{ID}");
                break;
            default:
                GetComponent<MeshRenderer>().material =
                    Resources.Load<Material>("Prefabs/Materials/Black");
                break;
        }

        //if (!CardDataDeserializer.TryGetData(ID, out _data))
        //    Debug.Log("데이터를 불러오는 도중에 문제가 발생했습니다." +
        //              $"\n카드 ID : {ID}");

        this.GetComponentInChildren<TMP_Text>().text = Data.Name;

    }
    //public void Init(int ID, out bool temp)
    //{
    //    temp = true;
    //    this.ID = ID;
    //    level = ID % 10;

    //    switch (ID / 10)
    //    {
    //        case 101:
    //            GetComponent<MeshRenderer>().material =
    //                Resources.Load<Material>($"Prefabs/Materials/Food/{ID}");
    //            cardType = CardType.Food;
    //            break;
    //        case 102:
    //            GetComponent<MeshRenderer>().material =
    //                Resources.Load<Material>($"Prefabs/Materials/Water/{ID}");
    //            cardType = CardType.Water;
    //            break;
    //        case 201:
    //            GetComponent<MeshRenderer>().material =
    //                Resources.Load<Material>($"Prefabs/Materials/Wood/{ID}");
    //            cardType = CardType.Wood;
    //            break;
    //        case 202:
    //            GetComponent<MeshRenderer>().material =
    //                Resources.Load<Material>($"Prefabs/Materials/Stone/{ID}");
    //            cardType = CardType.Stone;
    //            break;
    //        case 300:
    //            GetComponent<MeshRenderer>().material =
    //                Resources.Load<Material>($"Prefabs/Materials/Combination/{ID}");
    //            cardType = CardType.Combination;
    //            level = 5;
    //            break;
    //        case 500:
    //            GetComponent<MeshRenderer>().material =
    //                Resources.Load<Material>($"Prefabs/Materials/Merchant/{ID}");
    //            cardType = CardType.Merchant;
    //            level = 5;
    //            break;
    //        default:
    //            GetComponent<MeshRenderer>().material =
    //                Resources.Load<Material>("Prefabs/Materials/Black");
    //            break;
    //    }

    //    //if (!CardDataDeserializer.TryGetData(ID, out _data))
    //    //    Debug.Log("데이터를 불러오는 도중에 문제가 발생했습니다." +
    //    //              $"\n카드 ID : {ID}");

    //    this.GetComponentInChildren<TMP_Text>().text = Data.KR;

    //}

    //private void InitCheck()
    //{

    //    var v = from card in CardManager.Cards
    //            where card.ID % 10 == this.ID % 10
    //            select card;

    //}


    //private CardGroup CreateParent(Transform targetPos)
    //{
    //    var emptyParent = new GameObject("CardGroup");
    //    emptyParent.transform.SetParent(CardManager.Instance.transform);
    //    emptyParent.transform.localPosition = targetPos.transform.localPosition;
    //    var temp = emptyParent.AddComponent<CardGroup>();

    //    return temp;
    //}


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
        CardManager.Instance.CardMouseUp(this).Forget();


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

    void CardRelease()
    {
        Pool.Release(this.gameObject);
    }

}
