# Strong Naming

This directory provides build logic to strong-name the assembly. The provided key includes the private key.

**IMPORTANT:** Strong names are not considered a secure identity. DO NOT use this key for any kind of secure identity. To provide a secure verification of your assembly identity, use Authenticode.

## Using an existing Signing Key

You can use an existing signing key by changing the `AssemblyOriginatorKeyFile` value in `module.props`
