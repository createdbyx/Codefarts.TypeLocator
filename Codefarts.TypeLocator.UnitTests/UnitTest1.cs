using System;
using System.IO;
using System.Linq;
using Codefarts.TypeLocator.UnitTests.Mocks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Codefarts.TypeLocator.UnitTests;

[TestClass]
public class UnitTest1
{
    [TestMethod]
    public void NullTypeName()
    {
        var locator = new TypeLocator();
        Assert.ThrowsException<ArgumentException>(() => locator.FindTypes(null, false));
    }

    [TestMethod]
    public void WhitespaceTypeName()
    {
        var locator = new TypeLocator();
        Assert.ThrowsException<ArgumentException>(() => locator.FindTypes("   ", false));
    }

    [TestMethod]
    public void EmptyTypeName()
    {
        var locator = new TypeLocator();
        Assert.ThrowsException<ArgumentException>(() => locator.FindTypes(string.Empty, false));
    }

    [TestMethod]
    public void FindExistingInExternalAssembly()
    {
        var locator = new TypeLocator();
        var rootPath = Path.GetDirectoryName(this.GetType().Assembly.Location);
        var assemblyFiles = new[] { Path.Combine(rootPath, "Codefarts.TypeLocator.UnitTests.External.dll") };
        var type = locator.FindTypes("ExternalSimpleType", false, assemblyFiles);
        Assert.AreEqual(1, type.Count());
        var item = type.FirstOrDefault();
        Assert.AreEqual("ExternalSimpleType", item.Name);
    }

    [TestMethod]
    public void FindExistingInExternalAssemblyInCache()
    {
        var locator = new TypeLocator();
        var rootPath = Path.GetDirectoryName(this.GetType().Assembly.Location);
        var assemblyFiles = new[] { Path.Combine(rootPath, "Codefarts.TypeLocator.UnitTests.External.dll") };
        var type = locator.FindTypes("ExternalSimpleType", true, assemblyFiles);
        Assert.AreEqual(1, type.Count());
        var item = type.FirstOrDefault();
        Assert.AreEqual("ExternalSimpleType", item.Name);

        var anotherType = locator.FindTypes("ExternalSimpleType", false);
        Assert.AreEqual(1, anotherType.Count());
        item = anotherType.FirstOrDefault();
        Assert.AreEqual("ExternalSimpleType", item.Name);
    }

    [TestMethod]
    public void FindMissingInExternalAssembly()
    {
        var locator = new TypeLocator();
        var rootPath = Path.GetDirectoryName(this.GetType().Assembly.Location);
        var assemblyFiles = new[] { Path.Combine(rootPath, "Codefarts.TypeLocator.UnitTests.External.dll") };
        Assert.ThrowsException<TypeLoadException>(() =>
        {
            var type = locator.FindTypes("MissingExternalSimpleType", true, assemblyFiles);
            Assert.AreEqual(0, type.Count());
        });
    }

    [TestMethod]
    public void WillForceDomainScan()
    {
        var locator = new TypeLocator();
        var type = locator.FindTypes(nameof(SimpleType), false);
        Assert.AreEqual(1, type.Count());
        var item = type.FirstOrDefault();
        Assert.AreEqual(nameof(SimpleType), item.Name);
    }

    [TestMethod]
    public void WillUseCache()
    {
        var locator = new TypeLocator();
        var type = locator.FindTypes(nameof(SimpleType), true);
        Assert.AreEqual(1, type.Count());
        var item = type.FirstOrDefault();
        Assert.AreEqual(nameof(SimpleType), item.Name);

        var anotherType = locator.FindTypes(nameof(SimpleType), true);
        Assert.AreEqual(1, anotherType.Count());
        item = anotherType.FirstOrDefault();
        Assert.AreEqual(nameof(SimpleType), item.Name);
    }
}