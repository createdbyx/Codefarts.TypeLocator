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
        var locator = new TypeLocator();
        Assert.ThrowsException<ArgumentException>(() => locator.FindTypesByName(null));
    }

    [TestMethod]
    public void WhitespaceTypeName()
    {
        var locator = new TypeLocator();
        Assert.ThrowsException<ArgumentException>(() => locator.FindTypesByName("   "));
    }

    [TestMethod]
    public void EmptyTypeName()
    {
        var locator = new TypeLocator();
        Assert.ThrowsException<ArgumentException>(() => locator.FindTypesByName(string.Empty));
    }

    [TestMethod]
    public void FindExistingInExternalAssembly()
    {
        var locator = new TypeLocator();
        var rootPath = Path.GetDirectoryName(this.GetType().Assembly.Location);
        var assemblyFiles = new[] { Path.Combine(rootPath, "Codefarts.TypeLocator.UnitTests.External.dll") };
        var type = locator.FindTypesByName("ExternalSimpleMockType", assemblyFiles);
        Assert.AreEqual(1, type.Count());
        var item = type.FirstOrDefault();
        Assert.AreEqual("ExternalSimpleMockType", item.Name);
    }

    [TestMethod]
    public void FindExistingInExternalAssemblyInCache()
    {
        var locator = new TypeLocator();
        var rootPath = Path.GetDirectoryName(this.GetType().Assembly.Location);
        var assemblyFiles = new[] { Path.Combine(rootPath, "Codefarts.TypeLocator.UnitTests.External.dll") };
        var type = locator.FindTypesByName("ExternalSimpleMockType", assemblyFiles);
        Assert.AreEqual(1, type.Count());
        var item = type.FirstOrDefault();
        Assert.AreEqual("ExternalSimpleMockType", item.Name);

        var anotherType = locator.FindTypesByName("ExternalSimpleMockType");
        Assert.AreEqual(1, anotherType.Count());
        item = anotherType.FirstOrDefault();
        Assert.AreEqual("ExternalSimpleMockType", item.Name);
    }

    [TestMethod]
    public void FindMissingInExternalAssembly()
    {
        var locator = new TypeLocator();
        var rootPath = Path.GetDirectoryName(this.GetType().Assembly.Location);
        var assemblyFiles = new[] { Path.Combine(rootPath, "Codefarts.TypeLocator.UnitTests.External.dll") };

        var type = locator.FindTypesByName("MissingExternalSimpleMockType", assemblyFiles);
        Assert.AreEqual(0, type.Count());
    }

    [TestMethod]
    public void WillForceDomainScan()
    {
        var locator = new TypeLocator();
        var type = locator.FindTypesByName(nameof(SimpleMockType));
        Assert.AreEqual(1, type.Count());
        var item = type.FirstOrDefault();
        Assert.AreEqual(nameof(SimpleMockType), item.Name);
    }

    [TestMethod]
    public void WillUseCache()
    {
        var locator = new TypeLocator();
        var type = locator.FindTypesByName(nameof(SimpleMockType));
        Assert.AreEqual(1, type.Count());
        var item = type.FirstOrDefault();
        Assert.AreEqual(nameof(SimpleMockType), item.Name);

        var anotherType = locator.FindTypesByName(nameof(SimpleMockType));
        Assert.AreEqual(1, anotherType.Count());
        item = anotherType.FirstOrDefault();
        Assert.AreEqual(nameof(SimpleMockType), item.Name);
    }
}