﻿using System;
using System.ComponentModel.DataAnnotations;

namespace RskManager.Models.Requests
{
    public class CallContractFunctionParameterModel
    {
        [Required]
        public string SenderPrivateKey
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
        public string ContractAddress
        {
            get;
            set;
        }

        [Required]
        public string FunctionName
        {
            get;
            set;
        }

        public string Parameter
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
