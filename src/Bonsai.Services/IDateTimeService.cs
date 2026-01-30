using System;

namespace Bonsai.Services;

public interface IDateTimeService
{
    DateTime UtcNow { get; }
} 
