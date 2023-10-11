using ControlCanvas.Runtime;

namespace Playground.Scripts.AI
{
    public class HeroInventoryBlackboard : IBlackboard
    {
        public bool HasSword { get; set; }
        
        public int Coins { get; set; }
    }
}