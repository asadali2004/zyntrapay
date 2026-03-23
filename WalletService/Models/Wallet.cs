using System;
using System.Collections.Generic;

namespace WalletService.Models;

public partial class Wallet
{
    public int Id { get; set; }

    public int AuthUserId { get; set; }

    public decimal Balance { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<LedgerEntry> LedgerEntries { get; set; } = new List<LedgerEntry>();
}
