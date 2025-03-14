using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arrow : MonoBehaviour
{
    #region Public Fields
    [Tooltip("The prefab of arrow head")]
    public GameObject ArrowHeadPrefab;

    [Tooltip("The prefab of arrow node")]
    public GameObject ArrowNodePrefab;

    [Tooltip("The number of arrow nodes")]
    public int ArrowNodeNum;

    [Tooltip("The scale multiplier for arrow nodes")]
    public float scaleFactor = 1f;

    [Tooltip("The arrow renderer list")]
    public List<SpriteRenderer> arrowRenderer = new();

    [HideInInspector]
    public int ArrowIndex = -1;
    
    #endregion
    
    #region Private Fields
    private Transform origin;
    private List<Transform> arrowNodes = new();
    private List<Vector2> controlPoints = new();
    private readonly Vector2[][] controlPointFactors =
        new Vector2[][]
        {
            new Vector2[2] { new Vector2(-0.3f, 0.8f), new Vector2(0.1f, 1.4f) },
            new Vector2[2] { new Vector2(0f, 0.8f), new Vector2(0f, 1.5f) },
            new Vector2[2] { new Vector2(0f, 0.8f), new Vector2(0f, 1.5f) }
        };
    #endregion
    #region Public Methods
    public void SetStartArrow()
    {
        switch (ArrowIndex)
        {
            case 0:
                this.controlPoints[0] = new Vector2(this.origin.position.x, this.origin.position.y + CardUtils.LargeCardPosY);
                break;
            case 1:
            case 2:
                this.controlPoints[0] = ItemManager.Instance.SettingArrowPos(ArrowIndex);
                break;
            //case 2:
            //    this.controlPoints[0] = PotionManager.Instance.ArrowPotionPos();
            //    break;
        }
        this.controlPoints[3] = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        this.controlPoints[1] = this.controlPoints[0] + (this.controlPoints[3] - this.controlPoints[0]) * this.controlPointFactors[ArrowIndex][0];
        this.controlPoints[2] = this.controlPoints[0] + (this.controlPoints[3] - this.controlPoints[0]) * this.controlPointFactors[ArrowIndex][1];
    }
    #endregion
    #region Private Methods

    private void Awake()
    {
        if (BattleManager.Instance != null)
        {
            Destroy(gameObject);
        }
        this.origin = this.GetComponent<Transform>();

        for (int i = 0; i < this.ArrowNodeNum; ++i)
        {
            this.arrowNodes.Add(Instantiate(this.ArrowNodePrefab, this.transform).GetComponent<Transform>());
            arrowRenderer.Add(arrowNodes[i].GetComponent<SpriteRenderer>());
            arrowRenderer[i].sortingOrder = ArrowNodeNum - i;
        }

        this.arrowNodes.Add(Instantiate(this.ArrowHeadPrefab, this.transform).GetComponent<Transform>());
        arrowRenderer.Add(arrowNodes[^1].GetComponent<SpriteRenderer>());

        //this.arrowNodes.ForEach(a => a.GetComponent<Transform>().position = new Vector2(-0.5f, -0.5f));

        for (int i = 0; i < 4; ++i)
        {
            this.controlPoints.Add(Vector2.zero);
        }
        this.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (EnemyManager.Instance.TargetEnemy != null)
            {
                if (ArrowIndex == 1)
                {
                    ItemManager.Instance.AttackSingleTarget(EnemyManager.Instance.TargetEnemy/*.GetComponent<Enemy>()*/);
                }
                else if (ArrowIndex == 2)
                {
                    ItemManager.Instance.AttackSingleTarget(EnemyManager.Instance.TargetEnemy);
                }
            }
            BattleManager.Instance.SetActiveArrowCursor(false, ArrowIndex);
        }
        //this.controlPoints[0] = new Vector2(this.origin.position.x, this.origin.position.y + CardUtils.LargeCardPosY);

        this.controlPoints[3] = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        this.controlPoints[1] = this.controlPoints[0] + (this.controlPoints[3] - this.controlPoints[0]) * this.controlPointFactors[ArrowIndex][0];
        this.controlPoints[2] = this.controlPoints[0] + (this.controlPoints[3] - this.controlPoints[0]) * this.controlPointFactors[ArrowIndex][1];

        if (MapManager.Instance.currStage.State == Map.StageState.Boss)
        {
            this.controlPoints[1] = new Vector2(this.controlPoints[3].x - 3 * this.controlPoints[1].x, 0.75f * this.controlPoints[1].y);        // Boss용 Card 베지어 곡선
            this.controlPoints[2] = new Vector2(this.controlPoints[3].x + 3 * this.controlPoints[2].x, 0.75f * this.controlPoints[2].y);        // Boss용 Card 베지어 곡선
        }


        for (int i = 0; i < this.arrowNodes.Count; ++i)     // 보스방에서 Node수 적어보이면 그냥 처음에 많이 만들고, 보스방에서는 노드 전부 사용, 일반 적은 일부만 사용 방식 쳬택할 예정.
        {
            //var t = Mathf.Log(1f * i / (this.arrowNodes.Count - 1) + 1f, 2f);
            var t = Mathf.Pow(2, i/(this.arrowNodes.Count - 1f)) - 1f;
            this.arrowNodes[i].position = 
                Mathf.Pow(1 - t, 3) * this.controlPoints[0] +
                3 * Mathf.Pow(1 - t, 2) * t * this.controlPoints[1] +
                3 * (1 - t) * Mathf.Pow(t, 2) * this.controlPoints[2] +
                Mathf.Pow(t, 3) * this.controlPoints[3];

            if (i > 0)
            {
                var euler = new Vector3(0, 0, Vector2.SignedAngle(Vector2.up, this.arrowNodes[i].position - this.arrowNodes[i - 1].position));
                this.arrowNodes[i].rotation = Quaternion.Euler(euler);
            }
            
            var scale = this.scaleFactor * (1f - 0.03f * (this.arrowNodes.Count - 1 - i));

            if (i == this.arrowNodes.Count - 1)
                scale = 1.2f * scale;

            this.arrowNodes[i].localScale = new Vector3(scale, scale, 1f);
        }

        this.arrowNodes[0].transform.rotation = this.arrowNodes[1].transform.rotation;
        //this.arrowNodes[4].transform.position = this.controlPointFactors[1][0];
        //this.arrowNodes[6].transform.position = this.controlPointFactors[1][1];
        //this.arrowNodes[8].transform.position = this.controlPointFactors[2][0];
        //this.arrowNodes[10].transform.position = this.controlPointFactors[2][1];
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Enemy")) return;

        if (CardManager.Instance.isSingleTarget)
        {
            CardManager.Instance.useSingleTargetCard = true;
        }
        //else if (ItemManager.Instance.arrowOn)
        //{
        //    //EnemyManager.Instance.targetEnemy = other.gameObject;
        //    for (int i = 0; i < EnemyManager.Instance.ArrowCursor.arrowRenderer.Count; i++)
        //    {
        //        EnemyManager.Instance.ArrowCursor.arrowRenderer[i].color = Color.red;
        //    }
        //}
        //CardManager.Instance.useSingleTargetCard = true;        // 따로 체크해서 받아주는 거랑 그냥 true 하는 거랑 비슷할 것 같아서 걍 if문 없이 진행 => 근데 그럼 일관성을 해치는 듯 다시 체크해줌.

        //EnemyManager.Instance.TargetEnemy = collision.gameObject;       // Enemy 스크립트를 여기서 받는 건 너무 오바라서 그냥 카드 사용할 때 받기로 함. (Enemy한테 OnTrigger 하는 것보다 이게 좀 더 비용적으로 나을 듯?)
        EnemyManager.Instance.TargetEnemy = EnemyManager.Instance.EnemyDict[collision.gameObject.GetInstanceID()];
        for (int i = 0; i < BattleManager.Instance.ArrowCursor.arrowRenderer.Count; ++i)
        {
            BattleManager.Instance.ArrowCursor.arrowRenderer[i].color = Color.red;
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!collision.CompareTag("Enemy")) return;
        //if (/*CardManager.Instance.isSingleTarget*/EnemyManager.Instance.ArrowCursor.arrowRenderer[0].color == Color.red)
        //{
        //    CardManager.Instance.useSingleTargetCard = false;
        //    EnemyManager.Instance.targetEnemy = null;
        //    for (int i = 0; i < EnemyManager.Instance.ArrowCursor.arrowRenderer.Count; i++)
        //    {
        //        EnemyManager.Instance.ArrowCursor.arrowRenderer[i].color = Color.white;
        //    }
        //}
        //else if (ItemManager.Instance.arrowOn)
        //{
        //    EnemyManager.Instance.targetEnemy = null;
        //    for (int i = 0; i < EnemyManager.Instance.ArrowCursor.arrowRenderer.Count; i++)
        //    {
        //        EnemyManager.Instance.ArrowCursor.arrowRenderer[i].color = Color.white;
        //    }
        //}
        CardManager.Instance.useSingleTargetCard = false;       // 나갈 때 무조건 꺼야하는데, 굳이 조건문 확인해서 체크 꺼주는 것보다 그냥 꺼주는 게 더 나을 듯?
        EnemyManager.Instance.TargetEnemy = null;
        for (int i = 0; i < BattleManager.Instance.ArrowCursor.arrowRenderer.Count; ++i)
        {
            BattleManager.Instance.ArrowCursor.arrowRenderer[i].color = Color.white;
        }
    }
    #endregion
}
