using System;
using Randomous.EntitySystem.Implementations;
using Xunit;

namespace Randomous.EntitySystem.test
{
    public class TestGeneralHelper : UnitTestBase
    {
        protected GeneralHelper helper;

        public TestGeneralHelper()
        {
            helper = new GeneralHelper();
        }

        [Fact]
        public void MagicSort()
        {
            var list = new [] {0,1,2,3,4,5,6,7,8,9};

            var result = helper.MagicSort(list, new Func<int, int>((v) => v / 2));

            for(int i = 0; i < 5; i++)
            {
                Assert.True(result.ContainsKey(i));
                AssertResultsEqual(result[i], new[] {i * 2, i * 2 + 1});
            }
        }
    }
}