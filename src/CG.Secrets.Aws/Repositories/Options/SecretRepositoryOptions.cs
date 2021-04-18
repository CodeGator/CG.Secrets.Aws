using CG.Business.Repositories.Options;
using CG.DataProtection;
using System;

namespace CG.Secrets.Aws.Repositories.Options
{
    /// <summary>
    /// This class represents configuration options for the <see cref="SecretRepository"/>
    /// class.
    /// </summary>
    public class SecretRepositoryOptions : RepositoryOptions
    {
        // *******************************************************************
        // Properties.
        // *******************************************************************

        #region Properties

        /// <summary>
        /// This property contains an AWS access key identifier.
        /// </summary>
        [ProtectedProperty]
        public string AccessKeyId { get; set; }

        /// <summary>
        /// This property contains an AWS secret access key.
        /// </summary>
        [ProtectedProperty]
        public string SecretAccessKey { get; set; }

        /// <summary>
        /// This property contains an AWS region name.
        /// </summary>
        public string Region { get; set; }

        #endregion
    }
}
