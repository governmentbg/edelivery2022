using System;

namespace ED.EsbApi;

public record InboxStatsDO(
    DateOnly Month,
    int Value);
