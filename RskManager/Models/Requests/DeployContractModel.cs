using System;
using System.ComponentModel.DataAnnotations;
using Nethereum.Hex.HexTypes;

namespace RskManager.Models.Requests
{    
    public class DeployContractModel
    {
        [Required]
        public string SenderPrivateKey
        {
            get;
            set;
        }

        [Required]
        public string SenderAddress
        {
            get;
            set;
        }

        [Required]
        public string Abi
        {
            get;
            set;
        }

        [Required]
        public string Bytecode
        {
            get;
            set;
        }

        [Required]
        public string Gas
        {
            get;
            set;
        }

        [Required]
        public string GasPrice
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
