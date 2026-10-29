namespace Project06.Components
{
    public enum GameResult
    {
        // 승패가 나지 않아 판이 진행 중이다.
        Playing,
        // 보스를 처치했다. 판의 시뮬레이션이 멈춘다.
        Victory,
        // 보스를 처치하기 전에 쉘터가 파괴되었다. 판의 시뮬레이션이 멈춘다.
        Defeat,
    }
}
