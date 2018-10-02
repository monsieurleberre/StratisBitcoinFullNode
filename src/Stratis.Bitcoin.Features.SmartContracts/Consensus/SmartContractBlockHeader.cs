using NBitcoin;
using Stratis.SmartContracts.Core;
using Block = NBitcoin.Block;

namespace Stratis.Bitcoin.Features.SmartContracts.Consensus
{
    public class SmartContractBlockHeader : BlockHeader
    {
        /// <summary>
        /// Root of the state trie after execution of this block. 
        /// </summary>
        private uint256 hashStateRoot;
        public uint256 HashStateRoot { get { return this.hashStateRoot; } set { this.hashStateRoot = value; } }

        /// <summary>
        /// Root of the receipt trie after execution of this block.
        /// </summary>
        private uint256 receiptRoot;
        public uint256 ReceiptRoot { get { return this.receiptRoot; } set { this.receiptRoot = value; }  }

        /// <summary>
        /// Bitwise-OR of all the blooms generated from all of the smart contract transactions in the block.
        /// </summary>
        private Bloom logsBloom;
        public Bloom LogsBloom { get { return this.logsBloom; } set { this.logsBloom = value; } }

        public SmartContractBlockHeader() : base()
        {
            this.hashStateRoot = 0;
            this.receiptRoot = 0;
            this.logsBloom = new Bloom();
        }

        /// <summary>
        /// <see cref="ReadWrite(BitcoinStream)"/> overridden so that we can write the <see cref="hashStateRoot"/>.
        /// </summary>
        public override void ReadWrite(BitcoinStream stream)
        {
            base.ReadWrite(stream);
            stream.ReadWrite(ref this.hashStateRoot);
            stream.ReadWrite(ref this.receiptRoot);
            stream.ReadWrite(ref this.logsBloom);
        }

        /// <summary>
        /// Append the HashStateRoot to the genesis block of smart contract networks.
        /// </summary>
        /// <param name="genesisBlock">The genesis block of the smart contract network being generated.</param>
        internal static void AddGenesisHashStateRoot(Block genesisBlock)
        {
            ((SmartContractBlockHeader)genesisBlock.Header).HashStateRoot = new uint256(
                "21B463E3B52F6201C0AD6C991BE0485B6EF8C092E64583FFA655CC1B171FE856");
        }
    }
}