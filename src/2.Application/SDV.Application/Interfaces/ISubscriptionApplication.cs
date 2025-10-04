using System;
using SDV.Application.Results;

namespace SDV.Application.Interfaces;

public interface ISubscriptionApplication
{
    Task<OperationResult<string>> Subscribe(string planId, string clientId);
}
