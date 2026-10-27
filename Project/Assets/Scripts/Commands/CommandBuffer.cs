namespace Project06.Commands
{
    public class CommandBuffer
    {
        // 현재 이동 입력. 누르고 있는 동안 이어지는 입력이라 소비해도 비우지 않고, 매 Update에 덮어쓴다.
        public MoveCommand moveCommand;
        // 현재 바라보는 방향 입력. 이동 입력처럼 소비해도 비우지 않고, 매 Update에 덮어쓴다.
        public LookCommand lookCommand;
    }
}
