using Xunit;

[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace LLMGameCreator.Tests;

[CollectionDefinition("UnityAlphaProductSmoke", DisableParallelization = true)]
public sealed class UnityAlphaProductSmokeCollection
{
}
