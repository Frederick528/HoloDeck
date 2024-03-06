using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolManager : MonoBehaviour
{
    [SerializeField] GameObject cardPrefab;
    [SerializeField] Transform cardSpawnPoint;
    [SerializeField] Transform HandCard;
    [SerializeField] CardSO cardSO;
    Queue<GameObject> CardPool = new Queue<GameObject>(); //카드 담을 큐
    public static PoolManager instance = null;

    void Awake()
    {
        if (null == instance)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
            for (int i = 0; i < cardSO.cards.Length; i++)
            {
                GameObject cardObject = CreateCard(/*cardSO.cards[i]*/); //초기에 카드 생성
                CardPool.Enqueue(cardObject);
            }
        }
        else
        {
            Destroy(this.gameObject);
        }
    }
    GameObject CreateCard(/*CardData cardData*/) //초기 OR 카드 풀에 남은 카드가 부족할 때, 카드를 생성하기위해 호출되는 함수
    {
        GameObject cardObject = Instantiate(cardPrefab, cardSpawnPoint.position, Quaternion.identity, HandCard);
        //Card card = cardObject.GetComponent<Card>();
        //card.Setup(cardData);
        //CardManager.Instance.MainDeck.Add(card);
        cardObject.gameObject.SetActive(false);

        return cardObject;
    }
    public GameObject GetCard(/*CardData cardData*/) //카드가 필요할 때 다른 스크립트에서 호출되는 함수
    {
        if (CardPool.Count > 0) //현재 큐에 남아있는 카드가 있다면,
        {
            GameObject cardPool = CardPool.Dequeue();

            cardPool.gameObject.SetActive(true);
            //cardPool.transform.SetParent(null);
            return cardPool;
        }
        else //큐에 남아있는 카드가 없을 때 새로 만들어서 사용
        {
            GameObject cardPool = CreateCard(/*cardData*/);

            cardPool.gameObject.SetActive(true);
            //cardPool.transform.SetParent(null);
            return cardPool;
        }
    }
    public void ReturnObjectToQueue(GameObject cardObject) //사용이 완료 된 카드를 다시 큐에 넣을때 호출 파라미터->비활성화 할 카드
    {
        cardObject.gameObject.SetActive(false);
        //cardObject.transform.SetParent(instance.transform);
        CardPool.Enqueue(cardObject); //다시 큐에 넣음
    }
}