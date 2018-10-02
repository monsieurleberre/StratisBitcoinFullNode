namespace Stratis.Bitcoin.Networks.Smash
{
    public class Constants
    {
        /// <summary> Smash maximal value for the calculated time offset. If the value is over this limit, the time syncing feature will be switched off. </summary>
        public const int MaxTimeOffsetSeconds = 25 * 60;

        /// <summary> Smash default value for the maximum tip age in seconds to consider the node in initial block download (2 hours). </summary>
        public const int DefaultMaxTipAgeInSeconds = 2 * 60 * 60;

        /// <summary> The name of the root folder containing the different Smash blockchains (SmashMain, SmashTest, SmashRegTest). </summary>
        public const string RootFolderName = "smash";

        /// <summary> The default name used for the Smash configuration file. </summary>
        public const string DefaultConfigFilename = "smash.conf";

    }
}
