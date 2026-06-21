using System;
using System.Linq;
using NightLum.Elevate.Composition;
using NightLum.Elevate.Core;
using NUnit.Framework;

namespace NightLum.Elevate.Tests
{
    public sealed class IntegrationReadinessTests
    {
        [Test]
        public void RuntimeAssemblyDoesNotReferenceUnity()
        {
            string[] references = typeof(ScalarMap).Assembly
                .GetReferencedAssemblies()
                .Select(assembly => assembly.Name)
                .ToArray();

            CollectionAssert.DoesNotContain(references, "UnityEngine");
            CollectionAssert.DoesNotContain(references, "UnityEditor");
        }

        [Test]
        public void ExternalAdapterCanCreateMapWithoutCoreChanges()
        {
            ScalarMap source = ExampleAdapter.Convert(new[,] { { 2f }, { 4f } });
            ScalarMap result = Composer.Compose(
                new ScalarMap(2, 1),
                new[] { new Layer { Source = source, Mode = BlendMode.Add } },
                new ComposeSettings());

            CollectionAssert.AreEqual(new[] { 2f, 4f }, result.GetRawData());
        }

        private static class ExampleAdapter
        {
            public static ScalarMap Convert(float[,] values)
            {
                int width = values.GetLength(0);
                int height = values.GetLength(1);
                ScalarMap map = new ScalarMap(width, height);

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                        map.Set(x, y, values[x, y]);
                }

                return map;
            }
        }
    }
}
