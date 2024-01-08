using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace CodeWalker.Test
{
    public class FloatUtilTests(ITestOutputHelper testOutputHelper)
    {
        [Fact]
        public void FloatUtilTest()
        {
            float num = 1234.1234128934612983542364891263489125346758128793648972634921783945981234f;
            Assert.Equal(num.ToString(), FloatUtil.ToString(num));
            Assert.Equal(num, float.Parse(FloatUtil.ToString(num)));
            Assert.Equal(num, float.Parse(num.ToString()));
            var random = new Random(42);

            for (int i = 0; i < 100000; i++)
            {
                var number = (float)(random.NextDouble() * random.Next(int.MinValue, int.MaxValue));

                var numToString = number.ToString();

                var floatUtilToString = FloatUtil.ToString(number);

                Assert.Equal(numToString, floatUtilToString);
            }

        }
    }
}
