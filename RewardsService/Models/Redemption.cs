using System;
using System.Collections.Generic;

namespace RewardsService.Models;

public partial class Redemption
{
    public int Id { get; set; }

    public int AuthUserId { get; set; }

    public int RewardCatalogId { get; set; }

    public int PointsSpent { get; set; }

    public DateTime RedeemedAt { get; set; }

    public virtual RewardCatalog RewardCatalog { get; set; } = null!;
}
