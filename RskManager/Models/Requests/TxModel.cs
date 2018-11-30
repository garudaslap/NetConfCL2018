using System;
using System.ComponentModel.DataAnnotations;

namespace RskManager.Models.Requests
{
    public class TxModel
    {
        [Required]
        public string SenderAddress
        {
            get;
            set;
        }

        [Required]
        public string ToAddress
        {
            get;
            set;
        }

        [Required]
        public string Value
        {
            get;
            set;
        }
    }
}
