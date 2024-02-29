using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using TMPro;
using UnityEngine;

public enum CardType
{
    Food,
    Water,
    Wood,
    Stone,
    Combination,
    Merchant
}
public class Card : Draggable
{
    [SerializeField] SpriteRenderer card;
    [SerializeField] SpriteRenderer character;
    [SerializeField] TMP_Text nameText;
    [SerializeField] TMP_Text costText;
    [SerializeField] TMP_Text desText;

    //private Animator _anim;

    public CardData Data { get; private set; }
    public CardType cardType;
    public int ID;

    public static CancellationTokenSource RayCastToken = new();
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
        nameText.text = data.KR;
        costText.text = data.Date.ToString();
        desText.text = data.Descript;
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

        this.GetComponentInChildren<TMP_Text>().text = Data.KR;

    }
    public void Init(int ID, out bool temp)
    {
        temp = true;
        this.ID = ID;
        level = ID % 10;

        switch (ID / 10)
        {
            case 101:
                GetComponent<MeshRenderer>().material =
                    Resources.Load<Material>($"Prefabs/Materials/Food/{ID}");
                cardType = CardType.Food;
                break;
            case 102:
                GetComponent<MeshRenderer>().material =
                    Resources.Load<Material>($"Prefabs/Materials/Water/{ID}");
                cardType = CardType.Water;
                break;
            case 201:
                GetComponent<MeshRenderer>().material =
                    Resources.Load<Material>($"Prefabs/Materials/Wood/{ID}");
                cardType = CardType.Wood;
                break;
            case 202:
                GetComponent<MeshRenderer>().material =
                    Resources.Load<Material>($"Prefabs/Materials/Stone/{ID}");
                cardType = CardType.Stone;
                break;
            case 300:
                GetComponent<MeshRenderer>().material =
                    Resources.Load<Material>($"Prefabs/Materials/Combination/{ID}");
                cardType = CardType.Combination;
                level = 5;
                break;
            case 500:
                GetComponent<MeshRenderer>().material =
                    Resources.Load<Material>($"Prefabs/Materials/Merchant/{ID}");
                cardType = CardType.Merchant;
                level = 5;
                break;
            default:
                GetComponent<MeshRenderer>().material =
                    Resources.Load<Material>("Prefabs/Materials/Black");
                break;
        }

        //if (!CardDataDeserializer.TryGetData(ID, out _data))
        //    Debug.Log("데이터를 불러오는 도중에 문제가 발생했습니다." +
        //              $"\n카드 ID : {ID}");

        this.GetComponentInChildren<TMP_Text>().text = Data.KR;

    }

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

    public override void OnMouseUp()
    {
        //_rigid.isKinematic = false;


        //CollisionChecker(RayCastToken);
        if (this.transform.parent.TryGetComponent(out CardGroup hg))
        {
            if (hg.IndexOf(this).Equals(hg.Count - 1))
            {
                hg.RemoveCard(this);
                return;
            }
        }
        //SoundManager.instance.Play("Sounds/Effect/CardHoldSound");
        var results = Physics.OverlapSphere(transform.position, 7f);
        //CardManager.Instance.sortBtn.interactable = false;

        for (int i = 1; i < results.Length; i++)
        {
            var target = results[i].gameObject;
            if (target.transform.parent == null) continue;
            if (target.Equals(this.gameObject)) continue;
            if (!target.transform.TryGetComponent(out Card c)) continue;

            //foreach (var rule in CardDataDeserializer.CraftRules)
            //{
            //    if (rule.Contains(this.ID) && rule.Contains(c.ID))
            //    {
            //        Debug.Log(true);
            //        OnMerge(this.gameObject, c.gameObject, true);
            //        return;
            //    }
            //}

            //var emptyParent = CreateParent(target.transform);
            //emptyParent.AddCardRange(new[] { c, this });
        }
        //CardManager.Instance.sortBtn.interactable = true;
    }
    protected override void OnMouseDrag()
    {
        //float distance = Camera.main.WorldToScreenPoint(transform.position).z;
        //print(distance);
        Vector2 _temp = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        transform.position = _temp;

        //if (this.transform.parent.TryGetComponent(out CardGroup cardGroup))
        //{
        //    if (cardGroup.IndexOf(this) == 0)
        //    {
        //        cardGroup.transform.position = crntPos;
        //    }

        //    else if (cardGroup.IndexOf(this) == cardGroup.Count - 1)
        //        this.transform.position = crntPos;

        //    //     else
        //    //     {
        //    //         if (_tempGroup == null)
        //    //         {
        //    //             var temp = new GameObject("CardGroup");
        //    //             temp.transform.SetParent(CardManager.Instance.transform, true);
        //    //             _tempGroup = temp.AddComponent<CardGroup>();
        //    //             for (int i = cardGroup.IndexOf(this); i < cardGroup.Count;)
        //    //             {
        //    //                 var c = cardGroup.RemoveCard(i);
        //    //                 _tempGroup.AddCard(c);
        //    //             }
        //    //             cardGroup.Sort();
        //    //         }
        //    //     }
        //    // }
        //}
        //else
        //{
        //    this.transform.position = crntPos;
        //}
    }
    protected override void OnMouseDown()
    {
        //SoundManager.instance.Play("Sounds/Effect/CardDropSound");
        if (transform.parent.TryGetComponent(out CardGroup g))
        {
            var idx = g.IndexOf(this);
            if (idx != 0)
            {
                if (idx != g.Count - 1)
                {
                    List<Card> targets = new();
                    for (int i = g.IndexOf(this); i < g.Count;)
                    {
                        var v = g.RemoveCard(i);
                        //temp.AddCard(v);
                    }

                    //temp.Sort();
                }
                else
                {
                    g.RemoveCard(this);
                }
            }
        }

        // if (transform.parent.TryGetComponent(out CardGroup cardGroup))
        // {
        //     cardGroup.transform.position = new Vector3()
        //     {
        //         x = cardGroup.transform.position.x,
        //         y = 5,
        //         z = cardGroup.transform.position.z,
        //     };
        // }
        // else
        // {
        //     this.transform.position = new Vector3()
        //     {
        //         x = transform.position.x,
        //         y = 5,
        //         z = transform.position.z,
        //
        //     };
        // }
        base.OnMouseDown();
    }


    //public bool Lapse()
    //{
    //    var result = true;

    //    _data.Date -= 1;
    //    if (_data.Date <= 0)
    //        result = false;

    //    return result;
    //}

    //public void AnimEvt()
    //{
    //    var a = this.GetComponent<Animator>();
    //    CardManager.CreateQueue.Dequeue();
    //    CardManager.Instance.sortBtn.interactable =
    //        CardManager.CreateQueue.Count == 0;
    //    Destroy(a);
    //}

    ////카드 분해 기능
    //public void OnDecomposition(out Card[] createdCards)
    //{
    //    if (this.level == 0)
    //    {
    //        createdCards = null;
    //        return;
    //    }

    //    CardManager.DestroyCard(this);

    //    var v = new Card[2];
    //    v[0] = CardManager.CreateCard(this.level - 1, Random.Range(0, 4));
    //    v[1] = CardManager.CreateCard(this.level - 1, Random.Range(0, 4));

    //    createdCards = v;
    //}

    //public static void MoveToLerp(GameObject targetObj, Vector3 targetPos)
    //{
    //    GameManager.Instance.StartCoroutine(Move(targetObj, targetPos));
    //}
    //static IEnumerator Move(GameObject targetObj, Vector3 targetPos)
    //{
    //    while (true)
    //    {
    //        try
    //        {
    //            targetObj.transform.position =
    //                Vector3.Lerp(targetObj.transform.position, targetPos, 0.4f);

    //            if (Vector3.Distance(targetObj.transform.position, targetPos) <= 1f)
    //            {
    //                targetObj.transform.position = targetPos;
    //                break;
    //            }
    //        }
    //        catch
    //        {
    //            break;
    //        }
    //        yield return new WaitForSeconds(0.02f);
    //    }
    //    yield return null;
    //}

    //private async UniTaskVoid CollisionChecker(CancellationTokenSource tokenSource)
    //{
    //    while (true)
    //    {
    //        if (tokenSource.Token.IsCancellationRequested)
    //            break;
    //        if (this.gameObject == null)
    //            break;
    //        if (Physics.Raycast(this.transform.position, Vector3.down, 1f, LayerMask.NameToLayer("Floor")))
    //        {
    //            //카드를 내려놓았을 때 바닥으로 레이를 쏴서 닿으면 hit
    //            Camera.main.transform.position += Vector3.down;
    //            await UniTask.Delay(100, cancellationToken: tokenSource.Token);
    //            Camera.main.transform.position += Vector3.up;
    //            break;
    //        }
    //        await UniTask.Delay(100, cancellationToken: tokenSource.Token);
    //    }
    //    RayCastToken.Cancel();
    //    RayCastToken.Dispose();

    //    RayCastToken = new CancellationTokenSource();
    //}
}
