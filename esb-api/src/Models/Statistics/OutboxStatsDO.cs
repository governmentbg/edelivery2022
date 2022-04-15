using System;

namespace ED.EsbApi;

public record OutboxStatsDO(
    DateOnly Month,
    int Value);
