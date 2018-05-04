using System;
using System.Linq;
using FluentAssertions;
using NBitcoin;
using Stratis.Bitcoin.Features.Consensus.Rules.CommonRules;
using Stratis.Bitcoin.Features.Miner;

namespace Stratis.Bitcoin.Tests.Common
{
    public static class BlockUtils
    {
        public static void CheckBlockIsMutated(Block block)
        {
            var transactionHashes = block.Transactions.Select(t => t.GetHash()).ToList();
            BlockMerkleRootRule.ComputeMerkleRoot(transactionHashes, out bool isMutated);
            isMutated.Should().Be(true);
        }
    }
}
