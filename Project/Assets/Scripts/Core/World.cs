using Project06.Commands;
using Project06.Entities;

namespace Project06.Core
{
    public class World
    {
        // 플레이어 엔티티
        public PlayerEntity player;
        // Update에서 쌓고 Tick에서 소비하는 입력 명령
        public CommandBuffer commandBuffer = new CommandBuffer();
    }
}
