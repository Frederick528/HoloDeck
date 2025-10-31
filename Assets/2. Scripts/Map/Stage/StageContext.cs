using Cysharp.Threading.Tasks;
using System;

public class StageContext
{
    //public
    IStage CurrentStage;
    //{
    //    get; set;
    //}
    readonly Map _map;
    public StageContext(Map map)
    {
        _map = map;
    }
    // 움직임 없이 해당 맵으로 바로 이동(처음 스타트 또는 계속하기 등에서 사용할 예정)
    public void ImmediateTransition(IStage stage)
    {
        MapManager.Instance.EnterStage(_map);
        CurrentStage = stage;
        CurrentStage.Enter(_map);
    }
    public async UniTask Transition()
    {
        Action action = () =>
        {
            MapManager.Instance.EnterStage(_map);
            CurrentStage.Enter(_map);
        };
        await InGameManager.Instance.Player.ExitAndEnterStage(action);
        //await InGameManager.Instance.Player.ExitAndEnterStage();
        //CurrentStage.Enter(_map);
    }
    public async UniTask Transition(IStage stage)
    {
        Action action = () =>
        {
            MapManager.Instance.EnterStage(_map);
            CurrentStage = stage;
            CurrentStage.Enter(_map);
        };
        await InGameManager.Instance.Player.ExitAndEnterStage(action);
        //await InGameManager.Instance.Player.ExitAndEnterStage();
        //CurrentStage = stage;
        //CurrentStage.Enter(_map);
    }

    public async UniTask LoadTransition(IStage stage, bool changeScene)
    {
        Action action = () =>
        {
            MapManager.Instance.EnterStage(_map);
            CurrentStage = stage;
            CurrentStage.Enter(_map);
        };
        await InGameManager.Instance.Player.EnterChapterDoor(action, changeScene);
    }
}
