using System;
using System.Collections.Generic;
using System.Text;

namespace InterCase.Common
{
    public enum PaymentMethod
    {
        CreditCard = 1,
        BankTransfer = 2,
        Cash = 3
    }

    public enum OrderStatus
    {
        Pending = 1,
        Processing = 2,
        Processed = 3,
        Failed = 4
    }

    public enum CacheProvider
    {
        Redis = 1
    }

    public enum QueueProvider
    {
        RabbitMq = 1
    }
}
