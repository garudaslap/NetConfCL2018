using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.Hex.HexTypes;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using Newtonsoft.Json;
using RskManager.Models;
using RskManager.Models.Requests;

namespace RskManager.Controllers
{
    [Route("api/[controller]")]
    public class RskController : Controller
    {
        readonly static string NodeUrl = "http://localhost:4444";
        readonly Web3 Web3Client = new Web3(NodeUrl);

        public RskController()
        {
        }

        [HttpGet]
        public string Get()
        {
            return "RskController";
        }

		[HttpGet("GetBlockNumber")]
		public IActionResult GetBlockNumber()
		{
			try
            {
                var result = Web3Client.Eth.Blocks.GetBlockNumber.SendRequestAsync();
                return Json(result);
            }
            catch (Exception ex)
            {
                return ReturnError(ex);
            }
		}

		[HttpGet("GetBlock")]
        public IActionResult GetBlock()
        {
            try
            {
				var result = Web3Client.Eth.Blocks.GetBlockWithTransactionsByNumber;
                return Json(result);
            }
            catch (Exception ex)
            {
                return ReturnError(ex);
            }
        }

        [HttpGet("GetAccounts")]
        public async Task<IActionResult> GetAccounts()
        {
            try
            {
                var result = await Web3Client.Eth.Accounts.SendRequestAsync();
                return Json(result);
            }
            catch(Exception ex)
            {
                return ReturnError(ex);
            }
        }

        [HttpGet("Balance")]
        public async Task<IActionResult> Balance(string address)
        {
            try
            {
                if (String.IsNullOrWhiteSpace(address))
                    BadRequest("Address is null");


                var balance = await Web3Client.Eth.GetBalance.SendRequestAsync(address);
                return Json(balance.HexValue);
            }
            catch(Exception ex)
            {
                return ReturnError(ex);
            }
        }

        [HttpGet("CreateNewAccount")]
        public IActionResult CreateNewAccount()
        {
            try
            {
                var ecKey = Nethereum.Signer.EthECKey.GenerateKey();
                var privateKey = ecKey.GetPrivateKeyAsBytes().ToHex();
                var account = new Nethereum.Web3.Accounts.Account(privateKey);

                return Json(new AccountModel(account.Address, account.PrivateKey));
            }
            catch (Exception ex)
            {
                return ReturnError(ex);
            }
        }

        [HttpPost("SendTx")]
        public async Task<IActionResult> SendTx([FromBody]TxModel txModel)
        {
            try
            {
                if (ModelState.IsValid)
                {

                    var transacInpunt = new Nethereum.RPC.Eth.DTOs.TransactionInput
                    {
                        From = txModel.SenderAddress,
                        To = txModel.ToAddress,
                        Value = new HexBigInteger(txModel.Value)
                    };

                    var tx = await Web3Client.Eth.Transactions.SendTransaction.SendRequestAsync(transacInpunt);
                    return Json(tx);
                }

                return BadRequest(ModelState);
            }
            catch (Exception ex)
            {
                return ReturnError(ex);
            }
        }

        [HttpPost("SendOfflineTx")]
        public async Task<IActionResult> SendOfflineTx([FromBody]TxOfflineModel txModel)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var txCount = Web3Client
                        .Eth.Transactions
                        .GetTransactionCount
                        .SendRequestAsync(txModel.SenderAddress).Result;

                    // Entiendo que el gas para la transacción es fijo ahora en 21000. 
                    // Habría que ver si hay una manera de inferirlo.
                    var encoded = Web3.OfflineTransactionSigner
                                      .SignTransaction(txModel.SenderPrivateKey, 
                                                       txModel.ToAddress, 
                                                       new HexBigInteger(txModel.Value), 
                                                       txCount.Value, 
                                                       new HexBigInteger("0x0"), 21000);
                    
                    var send = await Web3Client.Eth
                                               .Transactions
                                               .SendRawTransaction
                                               .SendRequestAsync("0x" + encoded);

                    return Json(send);
                }

                return BadRequest(ModelState);
            }
            catch (Exception ex)
            {
                return ReturnError(ex);
            }
        }

        [HttpPost("DeployContract")]
        public async Task<IActionResult> DeployContract([FromBody]DeployContractModel deployContractModel)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    //new HexBigInteger(0x300000)
                    //gasPrice = 0x0
                    //value = 0x0
                    var gas = await Web3Client.Eth.DeployContract.EstimateGasAsync(deployContractModel.Abi,
                                                                                   deployContractModel.Bytecode,
                                                                                   deployContractModel.SenderAddress,
                                                                                   null);

                    var w = new Web3(new Account(deployContractModel.SenderPrivateKey), NodeUrl);

                    var txHash = await w.Eth.DeployContract
                                        .SendRequestAsync(deployContractModel.Abi,
                                                          deployContractModel.Bytecode,
                                                          deployContractModel.SenderAddress,
                                                          gas,
                                                          new HexBigInteger(deployContractModel.GasPrice),
                                                          new HexBigInteger(deployContractModel.Value));
                    return Json(txHash);
                }

                return BadRequest(ModelState);
            }
            catch (Exception ex)
            {
                return ReturnError(ex);
            }
        }

        [HttpGet("GetContractAddress")]
        public async Task<IActionResult> GetContractAddress(string txHash)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txHash))
                    return BadRequest("txHash is null");
                
                var receipt = await Web3Client.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(txHash);

                if (receipt != null)
                    return Json(receipt.ContractAddress);

                return NotFound();
            }
            catch (Exception ex)
            {
                return ReturnError(ex);
            }
        }

        [HttpPost("CallContractFunction")]
        public async Task<IActionResult> CallContractFunction([FromBody]CallContractFunctionModel callContractFunction)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var contract = Web3Client.Eth.GetContract(callContractFunction.Abi, callContractFunction.ContractAddress);
                    var result = await contract.GetFunction(callContractFunction.FunctionName).CallAsync<string>();

                    return Json(result);
                }

                return BadRequest(ModelState);
            }
            catch (Exception ex)
            {
                return ReturnError(ex);
            }
        }

        [HttpPost("CallContractFunctionTxParameter")]
        public async Task<IActionResult> CallContractFunctionTxParameter([FromBody]CallContractFunctionParameterModel callContractFunction)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var w = new Web3(new Account(callContractFunction.SenderPrivateKey), NodeUrl);

                    var contract = w.Eth.GetContract(callContractFunction.Abi, callContractFunction.ContractAddress);
                    var contractFunction = contract.GetFunction(callContractFunction.FunctionName);
                    var tx = await contractFunction
                        .SendTransactionAsync(callContractFunction.SenderAddress,
                                              new HexBigInteger(callContractFunction.Gas),
                                              new HexBigInteger(callContractFunction.GasPrice),
                                              new HexBigInteger(callContractFunction.Value),
                                              callContractFunction.Parameter);

                    return Json(tx);
                }

                return BadRequest(ModelState);
            }
            catch (Exception ex)
            {
                return ReturnError(ex);
            }
        }

        [HttpGet("about")]
        public ContentResult About()
        {
            return Content("An API for managing RSK node.");
        }

        [HttpGet("version")]
        public string Version()
        {
            return "Version 1.0.0";
        }

        private IActionResult ReturnError(Exception ex)
        {
            var error = new ErrorResultModel(ex.Message, ex.StackTrace);
            return StatusCode(500, JsonConvert.SerializeObject(error));
        }
    }
}
