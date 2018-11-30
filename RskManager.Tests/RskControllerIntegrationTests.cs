using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Newtonsoft.Json;
using Xunit;
using RskManager.Models;
using RskManager.Models.Requests;

[assembly: CollectionBehavior(DisableTestParallelization = true)]
namespace RskManager.Tests
{
    /// <summary>
    /// Rsk controller integration tests.
    /// It is necessary to have the RSK node running!
    /// </summary>
    public class RskControllerIntegrationTests
    {
        private readonly TestServer _server;
        private readonly HttpClient _client;

        public RskControllerIntegrationTests()
        {
            // Arrange
            _server = new TestServer(new WebHostBuilder()
                .UseStartup<Startup>());
            _client = _server.CreateClient();

            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
        }

        [Fact]
        public async Task GetBlockNumberTest()
        {
            var response = await _client.GetAsync("api/rsk/getBlockNumber");

            // Assert
            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            var blockNumber = JsonConvert.DeserializeObject<IEnumerable<string>>(responseString);
            blockNumber.Count().Should().Be(1000);
        }

        [Fact]
        public async Task GetAccountsTest()
        {
            var response = await _client.GetAsync("api/rsk/getAccounts");

            // Assert
            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            var accounts = JsonConvert.DeserializeObject<IEnumerable<string>>(responseString);
            accounts.Count().Should().Be(11);
        }

        [Fact]
        public async Task CheckIfCowAccountExistTest()
        {
            var response = await _client.GetAsync("api/rsk/getAccounts");
            // Assert
            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            var accounts = JsonConvert.DeserializeObject<IEnumerable<string>>(responseString);

            accounts.Should().Contain(TestSettings.CowAccountAddress);
        }

        [Fact]
        public async Task CheckBalanceOfCowAccountTest()
        {
            var response = await _client.GetAsync("api/rsk/balance?address=" + TestSettings.CowAccountAddress);

            // Assert
            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            var balance = JsonConvert.DeserializeObject<string>(responseString);

            balance.Should().Contain(TestSettings.CowAvailableBalance);
        }

        [Fact]
        public async Task CreateNewAccountTest()
        {
            var response = await _client.GetAsync("api/rsk/CreateNewAccount");

            // Assert
            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            var createdAccount = JsonConvert.DeserializeObject<AccountModel>(responseString);

            createdAccount.Address.Should().NotBeNullOrEmpty();
            createdAccount.PrivateKey.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task CheckBalanceOfCreatedAccountTest()
        {
            var response = await _client.GetAsync(string.Format("api/rsk/balance?address={0}", TestSettings.NewAccountAddress));

            // Assert
            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            var balance = JsonConvert.DeserializeObject<string>(responseString);

            //The account should not have gas!
            balance.Should().Contain("0x0");
        }

        [Fact]
        public async Task SendGasFromCowAccountCreatedTest()
        {
            // Act
            var request = new TxModel()
            {
                SenderAddress = TestSettings.CowAccountAddress,
                ToAddress = TestSettings.NewAccountAddress,
                Value = TestSettings.GasAmountToSend
            };

            var content = JsonConvert.SerializeObject(request);
            var stringContent = new StringContent(content, Encoding.UTF8, "application/json");
            var responseSendTx = await _client.PostAsync("api/Rsk/SendTx", stringContent);

            // Assert
            responseSendTx.EnsureSuccessStatusCode();
            var responseSendTxString = await responseSendTx.Content.ReadAsStringAsync();
            var responseTx = JsonConvert.DeserializeObject<string>(responseSendTxString);
            responseTx.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task CheckNewBalanceOfCreatedAccountTest()
        {
            var response = await _client.GetAsync(string.Format("api/rsk/balance?address={0}", TestSettings.NewAccountAddress));

            // Assert
            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            var balance = JsonConvert.DeserializeObject<string>(responseString);

            balance.Should().Contain(TestSettings.GasAmountToSend);
        }

        [Fact]
        public async Task DeployContractFromCreatedAccount()
        {           
            var deployContractModel = new DeployContractModel()
            {
                Abi = TestSettings.ContractAbi,
                Bytecode = TestSettings.ContractByteCode,
                SenderAddress = TestSettings.NewAccountAddress,
                SenderPrivateKey = TestSettings.NewAccountPrivateKey,
                Gas = TestSettings.GasAmountToSend,
                GasPrice = TestSettings.GasPrice,
                Value = TestSettings.TxValue
            };

            var content = JsonConvert.SerializeObject(deployContractModel);
            var stringContent = new StringContent(content, Encoding.UTF8, "application/json");
            var deployContractResponse = await _client.PostAsync("api/Rsk/DeployContract", stringContent);

            // Assert
            deployContractResponse.EnsureSuccessStatusCode();
            var deployContractResponseStr = await deployContractResponse.Content.ReadAsStringAsync();
            var responseTx = JsonConvert.DeserializeObject<string>(deployContractResponseStr);

            responseTx.Should().NotBeNullOrWhiteSpace();

            //ContractDeployedTxHash = responseTx;
        }

        [Fact]
        public async Task GetContractAddress()
        {
            var response = await _client.GetAsync(string.Format("api/rsk/GetContractAddress?txHash={0}", TestSettings.ContractTxHash));

            // Assert
            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            var contractAddress = JsonConvert.DeserializeObject<string>(responseString);

            contractAddress.Should().NotBeNullOrWhiteSpace();
            //ContractAddress = contractAddress;
        }

        [Fact]
        public async Task CallContract()
        {
            // Act
            var callContractFunctionModel = new CallContractFunctionModel()
            {
                Abi = TestSettings.ContractAbi,
                ContractAddress = TestSettings.ContractAddress,
                FunctionName = TestSettings.ContractMethod,

            };

            var content = JsonConvert.SerializeObject(callContractFunctionModel);
            var stringContent = new StringContent(content, Encoding.UTF8, "application/json");
            var contractResponse = await _client.PostAsync("api/Rsk/CallContractFunction", stringContent);

            // Assert
            contractResponse.EnsureSuccessStatusCode();
            var contractResponseStr = await contractResponse.Content.ReadAsStringAsync();
            var response = JsonConvert.DeserializeObject<string>(contractResponseStr);

            response.Should().Contain(TestSettings.ContractMethodResponse);
        }

    }
}