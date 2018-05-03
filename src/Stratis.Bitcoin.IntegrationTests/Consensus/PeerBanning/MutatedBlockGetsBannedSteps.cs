using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NBitcoin;
using Stratis.Bitcoin.Base;
using Stratis.Bitcoin.Base.Deployments;
using Stratis.Bitcoin.BlockPulling;
using Stratis.Bitcoin.Configuration;
using Stratis.Bitcoin.Connection;
using Stratis.Bitcoin.Features.Consensus;
using Stratis.Bitcoin.Features.Consensus.CoinViews;
using Stratis.Bitcoin.Features.Consensus.Interfaces;
using Stratis.Bitcoin.Features.Consensus.Rules;
using Stratis.Bitcoin.Features.Miner;
using Stratis.Bitcoin.Features.Wallet;
using Stratis.Bitcoin.Features.Wallet.Notifications;
using Stratis.Bitcoin.Features.WatchOnlyWallet.Notifications;
using Stratis.Bitcoin.IntegrationTests.Builders;
using Stratis.Bitcoin.IntegrationTests.EnvironmentMockUpHelpers;
using Stratis.Bitcoin.IntegrationTests.TestFramework;
using Stratis.Bitcoin.P2P.Peer;
using Stratis.Bitcoin.Signals;
using Stratis.Bitcoin.Tests.Common;
using Stratis.Bitcoin.Utilities;
using Xunit.Abstractions;

namespace Stratis.Bitcoin.IntegrationTests.Consensus.PeerBanning
{
    public partial class MutatedBlockGetsBannedSpecification : BddSpecification
    {
        private const string MaliciousNodeName = "malicious";
        private const string ValidatorNodeName = "validator";
        private const string MaliciousWalletName = "maliciousWallet";
        private const string ReceivingWalletName = "receivingWallet";
        private const string Password = "P@ssw0rd";
        private const string AccountName = "account 0";

        private readonly Network network = Network.RegTest;
        private StaticFlagIsolator staticFlagIsolator;
        private NodeGroupBuilder builder;
        private SharedSteps sharedSteps;
        private IDictionary<string, CoreNode> nodeGroup;
        private CoreNode maliciousNode;
        private CoreNode validatorNode;
        private int coinbaseMaturity;
        private Money powReward;
        private ConsensusLoop validatorConsensusLoop;
        private IPeerBanning peerBanning;
        private LookaheadResult pendingValidation;
        private ChainedBlock latestChainedBlock;
        private Block mutatedBlock;
        private int observedCount;
        private IDisposable subscription;

        public MutatedBlockGetsBannedSpecification(ITestOutputHelper output) : base(output)
        {
        }

        protected override void BeforeTest()
        {
            this.staticFlagIsolator = new StaticFlagIsolator(this.network);
            this.builder = new NodeGroupBuilder(this.CurrentTest.DisplayName);
            this.sharedSteps = new SharedSteps();
        }

        protected override void AfterTest()
        {
            this.staticFlagIsolator?.Dispose();
            this.builder?.Dispose();
        }

        public void two_nodes()
        {
            this.nodeGroup = this.builder
                .StratisPowNode(MaliciousNodeName).Start().NotInIBD()
                .WithWallet(MaliciousWalletName, Password)
                .StratisPowNode(ValidatorNodeName).Start().NotInIBD()
                .WithWallet(ReceivingWalletName, Password)
                .WithConnections()
                .Connect(MaliciousNodeName, ValidatorNodeName)
                .Connect(ValidatorNodeName, MaliciousNodeName)
                .AndNoMoreConnections()
                .Build();

            this.maliciousNode = this.nodeGroup[MaliciousNodeName];
            this.validatorNode = this.nodeGroup[ValidatorNodeName];

        }

        public void some_coins_to_spend()
        {

            this.coinbaseMaturity = (int)this.maliciousNode.FullNode
                .Network.Consensus.Option<PowConsensusOptions>().CoinbaseMaturity;
            this.powReward = this.maliciousNode.FullNode
                .Network.Consensus.Option<PowConsensusOptions>().ProofOfWorkReward;
            this.sharedSteps.MineBlocks(this.coinbaseMaturity + 1, this.maliciousNode, AccountName, MaliciousWalletName, Password);
        }

        public async Task a_miner_creates_a_mutated_block_and_broadcasts_it()
        {
            this.sharedSteps.WaitForNodeToSync(this.maliciousNode, this.validatorNode);

            this.latestChainedBlock = this.maliciousNode.FullNode.BlockStoreManager().BlockRepository.HighestPersistedBlock;
            var latestBlock = await this.maliciousNode.FullNode.BlockStoreManager().BlockRepository
                .GetAsync(this.latestChainedBlock.HashBlock);

            var coinBaseDepositAddress = this.maliciousNode.FullNode.WalletManager()
                .GetUnusedAddress(new WalletAccountReference(MaliciousWalletName, AccountName));
            var bitcoinAddress = BitcoinScriptAddress.Create(coinBaseDepositAddress.Address, this.network);
            this.mutatedBlock = latestBlock.CreateNextBlockWithCoinbase(bitcoinAddress, this.latestChainedBlock.Height + 1);

            var transaction = Tests.Common.Transactions.BuildNewTransactionFromExistingTransaction(latestBlock.Transactions.First());
            this.mutatedBlock.AddTransaction(transaction);
            var duplicateTransaction = Tests.Common.Transactions.BuildNewTransactionFromExistingTransaction(this.mutatedBlock.Transactions.First());
            this.mutatedBlock.AddTransaction(duplicateTransaction);
            this.mutatedBlock.AddTransaction(duplicateTransaction);
            this.mutatedBlock.UpdateMerkleRoot();
            //this.mutatedBlock.BlockSignatur.
            var mutatedBlock = maliciousNode.GenerateStratis(1,
                new List<Transaction>() {transaction, duplicateTransaction, duplicateTransaction}, broadcast: false);

            await ForcePublishBlock(this.maliciousNode, mutatedBlock.First());
            //this.maliciousNode.FullNode.Chain.SetTip(this.mutatedBlock.Header);
            //await this.maliciousNode.BroadcastBlocksAsync(new []{this.mutatedBlock});
        }

       

        private async Task ForcePublishBlock(CoreNode node, Block block)
        {
            var chainedBlock = new ChainedBlock(block.Header, block.Header.GetHash(), this.latestChainedBlock);
            node.FullNode.ChainBehaviorState.ConsensusTip = chainedBlock;
            var consensusLoop = node.FullNode.ConsensusLoop();
            //await consensusLoop.FlushAsync(true);
            //consensusLoop.Chain.SetTip(consensusLoop.Tip);
            //consensusLoop.Puller.SetLocation(consensusLoop.Tip);
            //consensusLoop.Tip.CheckProofOfWorkAndTarget(this.network);
            //this.subscription = this.validatorNode.FullNode.Signals.SubscribeForBlocks(
            //    Observer.Create<Block>(b => this.observedCount++));
            // this.maliciousNode.FullNode.Signals.SignalBlock(block);
            //this.maliciousNode.FullNode.Services.
            //this.maliciousNode.BroadcastBlocksAsync()
            //this.maliciousNode.FullNode.
            var peer = this.validatorNode.CreateNetworkPeerClient();
            await this.maliciousNode.BroadcastBlocksAsync(new[] {block}, peer);
        }

        public async Task another_miner_tries_to_validate_it()
        {
            TestHelper.TriggerSync(this.validatorNode);
            this.sharedSteps.WaitForNodeToSync(this.validatorNode);
            this.validatorConsensusLoop = this.validatorNode.FullNode.ConsensusLoop();
            //this.validatorConsensusLoop.Puller.AssignDownloadTaskToPeer(new BlockPullerBehavior(), out bool peerDisconnected);
            //this.validatorNode.FullNode.ConnectionManager.ConnectedPeers.Count().Should().Be(1);
            //this.observedCount.Should().Be(1);
            //await this.validatorConsensusLoop.StartAsync();

            //var validatorPeer = this.validatorNode.CreateNetworkPeerClient();

            //
            var cancelPullingBlock = new CancellationTokenSource(TimeSpan.FromSeconds(100));
            this.pendingValidation = this.validatorConsensusLoop.Puller.NextBlock(cancelPullingBlock.Token);
            //
            var validationContext = new BlockValidationContext()
            {
                Block = this.pendingValidation.Block,
                Peer = this.pendingValidation.Peer,
                BanDurationSeconds = BlockValidationContext.BanDurationDefaultBan
            };

            await this.validatorConsensusLoop.ValidateAndExecuteBlockAsync(validationContext.RuleContext);

            //this.sharedSteps.WaitForNodeToSync(this.maliciousNode, this.validatorNode);
        }

        public void the_block_with_mutated_hash_should_be_ignored()
        {
            //which means the chain has not progressed
            this.validatorNode.FullNode.Chain.Height.Should().Be(this.latestChainedBlock.Height);
        }

        public void the_hash_of_the_rejected_block_should_not_be_banned()
        {
            this.validatorNode.FullNode.ChainBehaviorState.IsMarkedInvalid(this.mutatedBlock.GetHash())
                .Should().BeFalse();
        }

        public void the_malicious_miner_should_get_banned()
        {
            this.peerBanning = this.validatorNode.FullNode.NodeService<IPeerBanning>();

            //this.peerBanning.IsBanned(pendingValidation.Peer).Should().BeTrue();
            this.peerBanning.IsBanned(this.maliciousNode.Endpoint).Should().BeTrue();
        }
    }
}