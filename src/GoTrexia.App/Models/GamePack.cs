using System.Collections.Generic;

namespace GoTrexia.Models
{
    public sealed record GamePack
    {
        public GameInfo GameInfo { get; init; } = new();

        public EndScreen EndScreen { get; init; } = new();

        public List<Stage> Stages { get; init; } = [];
    }
}
