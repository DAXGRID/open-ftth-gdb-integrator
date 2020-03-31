using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using Xunit.Sdk;

namespace OpenFTTH.GDBIntegrator.Subscriber.Tests
{
    public class JsonFileDataAttribute : DataAttribute
    {
        private readonly string _filePath;
        private readonly string _propertyName;

        /// <summary>
        /// Load data from a JSON file as the data source for a theory
        /// </summary>
        /// <param name="filePath">The absolute or relative path to the JSON file to load</param>
        public JsonFileDataAttribute(string filePath)
        {
            _filePath = filePath;
        }

        /// <inheritDoc />
        public override IEnumerable<object[]> GetData(MethodInfo testMethod)
        {
            if (testMethod == null)
                throw new ArgumentNullException(nameof(testMethod));

            var absolutePath = Path.IsPathRooted(_filePath)
                ? _filePath
                : Path.GetRelativePath(Directory.GetCurrentDirectory(), _filePath);

            if (!File.Exists(absolutePath))
                throw new ArgumentException($"Could not find file at path: {absolutePath}");

            var fileData = File.ReadAllText(_filePath);

            return new List<object[]> { new object[] { fileData } };
        }
    }
}
