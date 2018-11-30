using System;
using System.ComponentModel.DataAnnotations;

namespace RskManager.Models.Requests
{
    public class CallContractFunctionModel
    {
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
    }
}
