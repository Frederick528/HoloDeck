public class EnumTypes
{
    public enum CardTypes
    {
        Attack, Defence, Counterattack, Tactic
    }

    public enum CardRanks
    {
        Common, Rare, Epic, Legend
    }

    public enum CardHowToUses
    {
        Normal, /*TargetGround*/ TargetEntity
    }

    public enum CardAfterUses
    {
        Discard, Destruct
    }

    //public enum GameFlowState
    //{
    //    InitGame, SelectStage, Setting, Wave, EventFlow, Ending
    //}
}
