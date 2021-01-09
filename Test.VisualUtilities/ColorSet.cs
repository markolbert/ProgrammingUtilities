using System.Collections;
using System.Collections.Generic;
using System.Drawing;

namespace Test.VisualUtilities
{
    public class ColorSet : IEnumerable<object[]>
    {
        public IEnumerator<object[]> GetEnumerator()
        {
            yield return new object[] { Color.Blue };
            yield return new object[] { Color.Red };
            yield return new object[] { Color.Green };
            yield return new object[] { Color.Violet };
            yield return new object[] { Color.DarkTurquoise };
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}