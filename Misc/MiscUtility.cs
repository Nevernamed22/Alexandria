using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Alexandria.Misc
{
    public static class MiscUtility
    {
        public class WeightedTypeCollection<T>
        {
            public T SelectByWeight(System.Random generatorRandom)
            {
                List<WeightedType<T>> list = new List<WeightedType<T>>();
                float num = 0f;
                for (int i = 0; i < this.elements.Length; i++)
                {
                    WeightedType<T> weightedInt = this.elements[i];
                    if (weightedInt.weight > 0f)
                    {
                        list.Add(weightedInt);
                        num += weightedInt.weight;
                    }
                }
                float num2 = ((generatorRandom == null) ? UnityEngine.Random.value : ((float)generatorRandom.NextDouble())) * num;
                float num3 = 0f;
                for (int k = 0; k < list.Count; k++)
                {
                    num3 += list[k].weight;
                    if (num3 > num2)
                    {
                        return list[k].value;
                    }
                }
                return list[0].value;
            }

            public T SelectByWeight()
            {
                return this.SelectByWeight(null);
            }

            public WeightedType<T>[] elements;
        }
        public class WeightedType<T>
        {
            public T value;
            public float weight;
        }
    }
}
