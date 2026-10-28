namespace Project06.Commands
{
    public class CommandBuffer
    {
        // 현재 이동 입력. 누르고 있는 동안 이어지는 입력이라 소비해도 비우지 않고, 매 Update에 덮어쓴다.
        public MoveCommand moveCommand;
        // 현재 바라보는 방향 입력. 이동 입력처럼 소비해도 비우지 않고, 매 Update에 덮어쓴다.
        public LookCommand lookCommand;
        // 현재 조준 입력. 이동 입력처럼 소비해도 비우지 않고, 매 Update에 덮어쓴다.
        public AimCommand aimCommand;
        // 현재 발사 입력. 누르고 있는 동안 연사하는 입력이라 소비해도 비우지 않고, 매 Update에 덮어쓴다.
        public FireCommand fireCommand;
        // 재장전·취소 입력. 누른 순간의 입력이라 Update마다 누적하고, Tick에서 소비한 뒤 비운다.
        public ReloadCommand reloadCommand;
    }
}
