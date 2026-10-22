using System;
using System.Collections.Generic;

namespace Notification.Infrastructure.Persistence.Entities;

public partial class EmailDelivery
{
    public Guid Id { get; set; }

    public string MessageId { get; set; } = null!;

    public Guid? UserId { get; set; }

    public string RecipientEmail { get; set; } = null!;

    public string NotificationType { get; set; } = null!;

    public string Status { get; set; } = null!;

    public int AttemptCount { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public DateTime? SentAtUtc { get; set; }

    public string? LastError { get; set; }
}
