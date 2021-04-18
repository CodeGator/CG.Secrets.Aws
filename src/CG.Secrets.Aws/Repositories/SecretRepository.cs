
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using CG.Business.Repositories;
using CG.Diagnostics;
using CG.Secrets.Models;
using CG.Secrets.Repositories;
using CG.Validations;
using Microsoft.AspNetCore.Connections.Features;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CG.Secrets.Aws.Repositories
{
    /// <summary>
    /// This class is an Azure implementation of the <see cref="ISecretRepository"/>
    /// interface.
    /// </summary>
    public class SecretRepository : RepositoryBase, ISecretRepository
    {
        // *******************************************************************
        // Properties.
        // *******************************************************************

        #region Properties

        /// <summary>
        /// This property contain a refernce to an AWS secret client.
        /// </summary>
        protected AmazonSecretsManagerClient SecretClient { get; }

        #endregion

        // *******************************************************************
        // Constructors.
        // *******************************************************************

        #region Constructors

        /// <summary>
        /// This constructor creates a new instance of the <see cref="SecretRepository"/>
        /// class.
        /// </summary>
        public SecretRepository(
            AmazonSecretsManagerClient secretClient
            )
        {
            // Validate the parameters before attempting to use them.
            Guard.Instance().ThrowIfNull(secretClient, nameof(secretClient));

            // Save the references.
            SecretClient = secretClient;
        }

        #endregion

        // *******************************************************************
        // Public methods.
        // *******************************************************************

        #region Public methods

        /// <inheritdoc/>
        public virtual async Task<Secret> GetByNameAsync(
            string name,
            CancellationToken cancellationToken = default
            )
        {
            try
            {
                // Validate the parameters before attempting to use them.
                Guard.Instance().ThrowIfNullOrEmpty(name, nameof(name));

                // Create the request.
                var request = new GetSecretValueRequest
                {
                    SecretId = name
                };

                // Defer to the manager.
                var response = await SecretClient.GetSecretValueAsync(
                    request,
                    cancellationToken
                    ).ConfigureAwait(false);

                // Validate the results.
                if (response.HttpStatusCode != System.Net.HttpStatusCode.OK)
                {
                    // Panic!!
                    throw new RepositoryException(
                        message: $"The call to ASWS failed with status code: {response.HttpStatusCode}"
                        );
                }

                // Wrap the results in our model.
                var model = new Secret()
                {
                    Key = response.ARN,
                    Name = name,
                    Value = response.SecretString
                };

                // Return the result.
                return model;
            }
            catch (Exception ex)
            {
                // Provide better context for the error.
                throw new RepositoryException(
                    message: $"Failed to query the value of a secret, by name!",
                    innerException: ex
                    ).SetCallerInfo()
                     .SetOriginator(nameof(SecretRepository))
                     .SetDateTime();
            }
        }

        // *******************************************************************
        
        /// <inheritdoc/>
        public virtual async Task<Secret> SetByNameAsync(
            string name,
            string value,
            CancellationToken cancellationToken = default
            )
        {
            try
            {
                // Validate the parameters before attempting to use them.
                Guard.Instance().ThrowIfNullOrEmpty(name, nameof(name));

                // Create the request.
                var request = new PutSecretValueRequest
                {
                    SecretId = name,
                    SecretString = value
                };

                // Defer to the manager.
                var response = await SecretClient.PutSecretValueAsync(
                    request,
                    cancellationToken
                    ).ConfigureAwait(false);

                // Validate the results.
                if (response.HttpStatusCode != System.Net.HttpStatusCode.OK)
                {
                    // Panic!!
                    throw new RepositoryException(
                        message: $"The call to ASWS failed with status code: {response.HttpStatusCode}"
                        );
                }

                // Wrap the results in our model.
                var model = new Secret()
                {
                    Key = response.ARN,
                    Name = name,
                    Value = value
                };

                // Return the result.
                return model;
            }
            catch (Exception ex)
            {
                // Provide better context for the error.
                throw new RepositoryException(
                    message: $"Failed to set the value for a secret, by name!",
                    innerException: ex
                    ).SetCallerInfo()
                     .SetOriginator(nameof(SecretRepository))
                     .SetDateTime();
            }
        }

        #endregion
    }
}
