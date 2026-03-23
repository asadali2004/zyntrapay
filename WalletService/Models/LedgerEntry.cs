using System;
using System.Collections.Generic;

namespace WalletService.Models;

public partial class LedgerEntry
{
    public int Id { get; set; }

    public int WalletId { get; set; }

    public string Type { get; set; } = null!;

    public decimal Amount { get; set; }

    public string Description { get; set; } = null!;

    public string? ReferenceId { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Wallet Wallet { get; set; } = null!;
}
