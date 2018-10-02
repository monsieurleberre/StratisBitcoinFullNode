using System;
using System.Collections.Generic;
using NBitcoin;
using NBitcoin.BouncyCastle.Math;
using NBitcoin.DataEncoders;
using NBitcoin.Protocol;
using Stratis.Bitcoin.Features.SmartContracts.Consensus;
using Stratis.Bitcoin.Networks;
using Stratis.Bitcoin.Networks.Smash;

namespace Stratis.Bitcoin.Features.SmartContracts.Networks
{
    public sealed class SmartContractPosRegTest : SmashPosRegTest
    {
        /// <summary>
        /// Took the 'InitReg' from above and adjusted it slightly (set a static flag + removed the hash check)
        /// </summary>
        public SmartContractPosRegTest()
            : base(new SmartContractPosConsensusFactory(), SmartContractBlockHeader.AddGenesisHashStateRoot)
        {
            this.Name = nameof(SmartContractPosRegTest);
            this.Base58Prefixes[(int)Base58Type.PUBKEY_ADDRESS] = new byte[] { (63) };
            this.Base58Prefixes[(int)Base58Type.SCRIPT_ADDRESS] = new byte[] { (125) };
            this.Base58Prefixes[(int)Base58Type.SECRET_KEY] = new byte[] { (63 + 128) };
        }
    }
}