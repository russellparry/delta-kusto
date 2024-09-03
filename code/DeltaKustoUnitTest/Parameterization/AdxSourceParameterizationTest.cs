using DeltaKustoIntegration.Parameterization;
using Xunit;

namespace DeltaKustoUnitTest.Parameterization
{
    public class AdxSourceParameterizationTest : TestBase
    {
        [Fact]
        public void ValidateAllowsHttpWithInsecureFlag()
        {
            var jobParameterization = CreateHttpAdxSourceParameterization();

            var exception = Record.Exception(() => jobParameterization.Validate());
            Assert.NotNull(exception);

            jobParameterization.Current!.Adx!.AllowInsecureHttp = true;
            exception = Record.Exception(() => jobParameterization.Validate());
            Assert.Null(exception);
        }

        [Fact]
        public void ValidateAllowsHttsWithoutInsecureFlag()
        {
            var jobParameterization = CreateHttpAdxSourceParameterization();

            var exception = Record.Exception(() => jobParameterization.Validate());
            Assert.NotNull(exception);

            jobParameterization.Current!.Adx!.ClusterUri = "https://localhost";
            exception = Record.Exception(() => jobParameterization.Validate());
            Assert.Null(exception);
        }

        private JobParameterization CreateHttpAdxSourceParameterization()
        {
            var adxSourceParameterization = new AdxSourceParameterization()
            {
                ClusterUri = "http://localhost",
                Database = "db"
            };
            var sourceParameterization = new SourceParameterization()
            {
                Adx = adxSourceParameterization
            };
            var jobParameterization = new JobParameterization()
            {
                Current = sourceParameterization,
                Target = sourceParameterization,
                Action = new ActionParameterization
                {
                    PushToCurrent = true
                }
            };

            return jobParameterization;
        }
    }


}

