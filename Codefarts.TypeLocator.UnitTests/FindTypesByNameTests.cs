using System;
using System.IO;
using System.Linq;
using Codefarts.TypeLocator.UnitTests.Mocks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Codefarts.TypeLocator.UnitTests;

[TestClass]
public class FindTypesByNameTests
{
    [TestMethod]
    public void NullTypeName()
    {
       // var locator = new TypeLocator();
        Assert.ThrowsException<ArgumentException>(() => TypeLocator.FindTypesByName(null));
    }

    [TestMethod]
    public void WhitespaceTypeName()
    {
        Assert.ThrowsException<ArgumentException>(() => TypeLocator.FindTypesByName("   "));
    }

    [TestMethod]
    public void EmptyTypeName()
    {
        Assert.ThrowsException<ArgumentException>(() => TypeLocator.FindTypesByName(string.Empty));
    }

    [TestMethod]
    public void FindExistingInExternalAssembly()
    {
        var rootPath = Path.GetDirectoryName(this.GetType().Assembly.Location);
        var assemblyFiles = new[] { Path.Combine(rootPath, "Codefarts.TypeLocator.UnitTests.External.dll") };
        var type = TypeLocator.FindTypesByName("ExternalSimpleMockType", assemblyFiles);
        Assert.AreEqual(1, type.Count());
        var item = type.FirstOrDefault();
        Assert.AreEqual("ExternalSimpleMockType", item.Name);
    }

    [TestMethod]
    public void FindExistingInExternalAssemblyInCache()
    {
        var rootPath = Path.GetDirectoryName(this.GetType().Assembly.Location);
        var assemblyFiles = new[] { Path.Combine(rootPath, "Codefarts.TypeLocator.UnitTests.External.dll") };
        var type = TypeLocator.FindTypesByName("ExternalSimpleMockType", assemblyFiles);
        Assert.AreEqual(1, type.Count());
        var item = type.FirstOrDefault();
        Assert.AreEqual("ExternalSimpleMockType", item.Name);

        var anotherType = TypeLocator.FindTypesByName("ExternalSimpleMockType");
        Assert.AreEqual(1, anotherType.Count());
        item = anotherType.FirstOrDefault();
        Assert.AreEqual("ExternalSimpleMockType", item.Name);
    }

    [TestMethod]
    public void FindMissingInExternalAssembly()
    {
        var rootPath = Path.GetDirectoryName(this.GetType().Assembly.Location);
        var assemblyFiles = new[] { Path.Combine(rootPath, "Codefarts.TypeLocator.UnitTests.External.dll") };

        var type = TypeLocator.FindTypesByName("MissingExternalSimpleMockType", assemblyFiles);
        Assert.AreEqual(0, type.Count());
    }

    [TestMethod]
    public void WillForceDomainScan()
    {
        var type = TypeLocator.FindTypesByName(nameof(SimpleMockType));
        Assert.AreEqual(1, type.Count());
        var item = type.FirstOrDefault();
        Assert.AreEqual(nameof(SimpleMockType), item.Name);
    }

    [TestMethod]
    public void WillUseCache()
    {
        var type = TypeLocator.FindTypesByName(nameof(SimpleMockType));
        Assert.AreEqual(1, type.Count());
        var item = type.FirstOrDefault();
        Assert.AreEqual(nameof(SimpleMockType), item.Name);

        var anotherType = TypeLocator.FindTypesByName(nameof(SimpleMockType));
        Assert.AreEqual(1, anotherType.Count());
        item = anotherType.FirstOrDefault();
        Assert.AreEqual(nameof(SimpleMockType), item.Name);
    }
}