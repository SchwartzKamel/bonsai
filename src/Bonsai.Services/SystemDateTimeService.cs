using System;

namespace Bonsai.Services;

public class SystemDateTimeService : IDateTimeService
{
    public DateTime UtcNow => DateTime.UtcNow;
}
