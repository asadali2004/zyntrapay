using System;
using System.Collections.Generic;

namespace RewardsService.Models;

public partial class RewardCatalog
{
    public int Id { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public int PointsCost { get; set; }

    public int Stock { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<Redemption> Redemptions { get; set; } = new List<Redemption>();
}
