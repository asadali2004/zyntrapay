using System;
using System.Collections.Generic;

namespace RewardsService.Models;

public partial class RewardAccount
{
    public int Id { get; set; }

    public int AuthUserId { get; set; }

    public int TotalPoints { get; set; }

    public string Tier { get; set; } = null!;

    public DateTime CreatedAt { get; set; }
}
