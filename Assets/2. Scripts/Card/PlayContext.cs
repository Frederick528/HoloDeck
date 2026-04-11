using System.Collections.Generic;
public class PlayContext
{
    // 1. 드로우 기록 (기존)
    public List<Card> DrawnCards = new List<Card>();

    // [추가] 가장 최근에 발생한 타격의 데미지 수치
    public int LastDamageDealt;

    // 3. 총 데미지 합산 (편의용 프로퍼티)
    public int TotalDamage;

    // 4. 처치 기록 (이번 카드로 죽인 적이 있는지)
    public bool KilledEnemie;
}
