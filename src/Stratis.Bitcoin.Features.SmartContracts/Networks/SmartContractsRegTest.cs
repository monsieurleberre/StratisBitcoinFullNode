using System;
using System.Collections.Generic;
using NBitcoin;
using NBitcoin.DataEncoders;
using NBitcoin.Protocol;
using Stratis.Bitcoin.Features.SmartContracts.Consensus;
using Stratis.Bitcoin.Networks;

namespace Stratis.Bitcoin.Features.SmartContracts.Networks
{
    public sealed class SmartContractsRegTest : SmashRegTest
    {
        /// <summary>
        /// Took the 'InitReg' from above and adjusted it slightly (set a static flag + removed the hash check)
        /// </summary>
        public SmartContractsRegTest()
            : base(new SmartContractPosConsensusFactory(), SmartContractBlockHeader.AddGenesisHashStateRoot)

        {
            this.Name = nameof(SmartContractsRegTest);
            
            this.Base58Prefixes[(int)Base58Type.PUBKEY_ADDRESS] = new byte[] { (111) };
            this.Base58Prefixes[(int)Base58Type.SCRIPT_ADDRESS] = new byte[] { (196) };
            this.Base58Prefixes[(int)Base58Type.SECRET_KEY] = new byte[] { (239) };
        }
    }
}