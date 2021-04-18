using Amazon;
using Amazon.SecretsManager;
using CG.DataProtection;
using CG.Secrets.Aws.Repositories;
using CG.Secrets.Aws.Repositories.Options;
using CG.Secrets.Repositories;
using CG.Validations;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// This class contains extension methods related to the <see cref="IServiceCollection"/>
    /// type, for registering types from the <see cref="CG.Secrets.Aws"/> library.
    /// </summary>
    public static partial class SecretsAwsServiceCollectionExtensions
    {
        // *******************************************************************
        // Public methods.
        // *******************************************************************

        #region Public methods

        /// <summary>
        /// This method adds AWS repositories for the CG.Secrets library.
        /// </summary>
        /// <param name="serviceCollection">The service collection to use for
        /// the operation.</param>
        /// <param name="configuration">The configuration to use for the operation.</param>
        /// <param name="serviceLifetime">The service lifetime to use for the operation.</param>
        /// <returns>The value of the <paramref name="serviceCollection"/> parameter,
        /// for chaining calls together.</returns>
        public static IServiceCollection AddAwsRepositories(
            this IServiceCollection serviceCollection,
            IConfiguration configuration,
            ServiceLifetime serviceLifetime = ServiceLifetime.Scoped
            )
        {
            // Call the overload with the default data protector.
            return serviceCollection.AddAwsRepositories(
                DataProtector.Instance(),
                configuration,
                serviceLifetime
                );
        }

        // *******************************************************************

        /// <summary>
        /// This method adds AWS repositories for the CG.Secrets library.
        /// </summary>
        /// <param name="serviceCollection">The service collection to use for
        /// the operation.</param>
        /// <param name="dataProtector">The data protector to use for the operation.</param>
        /// <param name="configuration">The configuration to use for the operation.</param>
        /// <param name="serviceLifetime">The service lifetime to use for the operation.</param>
        /// <returns>The value of the <paramref name="serviceCollection"/> parameter,
        /// for chaining calls together.</returns>
        public static IServiceCollection AddAwsRepositories(
            this IServiceCollection serviceCollection,
            IDataProtector dataProtector,
            IConfiguration configuration,
            ServiceLifetime serviceLifetime = ServiceLifetime.Scoped
            )
        {
            // Validate the parameters before attempting to use them.
            Guard.Instance().ThrowIfNull(serviceCollection, nameof(serviceCollection))
                .ThrowIfNull(dataProtector, nameof(dataProtector))
                .ThrowIfNull(configuration, nameof(configuration));

            // Register the repository options.
            serviceCollection.ConfigureOptions<SecretRepositoryOptions>(
                dataProtector,
                configuration
                );

            // Register the secret manager.
            serviceCollection.Add<AmazonSecretsManagerClient>(serviceProvider =>
            {
                // Get the repository options.
                var options = serviceProvider.GetRequiredService<IOptions<SecretRepositoryOptions>>();

                // Create the client instance.
                var client = new AmazonSecretsManagerClient(
                    options.Value.AccessKeyId,
                    options.Value.SecretAccessKey,
                    RegionEndpoint.GetBySystemName(
                        options.Value.Region
                        )
                    );

                // Return the client.
                return client;
            },
            serviceLifetime
            );

            // Register the repository.
            serviceCollection.Add<ISecretRepository, SecretRepository>(serviceLifetime);

            // Return the service collection.
            return serviceCollection;
        }

        #endregion
    }
}
