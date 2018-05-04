using Stratis.Bitcoin.IntegrationTests.TestFramework;
using Xunit;

// Disable warnings about "this" qualifier to make the Specification more readable
// ReSharper disable ArrangeThisQualifier

namespace Stratis.Bitcoin.IntegrationTests.Consensus.PeerBanning
{
    public partial class MutatedBlockGetsBannedSpecification : BddSpecification
    {
        /// <summary>
        /// This test is not ready yet but I don't want to loose the work
        /// </summary>
        [Fact]
        public void MutatedBlockGetsBannedTest()
        {
            Given(two_nodes);
            And(some_coins_to_spend);

            When(a_malicious_node_creates_a_mutated_block);
            And(the_malicious_node_broadcasts_the_bad_block_to_a_peer);
            And(the_honest_peer_tries_to_sync_with_the_malicious_peer);

            Then(the_hash_of_the_rejected_block_should_not_be_banned);
            And(the_block_with_mutated_hash_should_be_ignored); //currently test is inverted
            And(the_malicious_miner_should_get_banned); //currently test is inverted
        }
    }
}