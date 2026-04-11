public static class GameEvents
{
    // 공격력, 방어력, 유물/포션 등에 의한 기본 스탯 변화
    public static System.Action OnBaseStatsChanged;
    // 첫 카드 사용 여부 등 게임 진행 상태 변화
    public static System.Action OnPlayStateChanged;
    // 타겟 마우스 오버 등 타겟 변경
    public static System.Action<Enemy> OnTargetChanged;

    public static void NotifyBaseStats() => OnBaseStatsChanged?.Invoke();
    public static void NotifyPlayState() => OnPlayStateChanged?.Invoke();
    public static void NotifyTarget(Enemy enemy) => OnTargetChanged?.Invoke(enemy);
}