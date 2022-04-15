using System;
using EDelivery.SEOS.DataContracts;

namespace EDelivery.SEOS.Models
{
    public class MessageCreationResult
    {
        public static MessageCreationResult Empty 
        {
            get
            {
                return new MessageCreationResult
                {
                    Result = String.Empty,
                    Type = null
                };
            }
        }

        public string Result { get; set; }

        public MessageType? Type { get; set; }
    }
}
