using System;
using System.IO;
using System.Linq;
using Codefarts.TypeLocator.UnitTests.Mocks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Codefarts.TypeLocator.UnitTests;

[TestClass]
public class UnitTest1
{
    // [TestMethod]
    // public void NullTypeName()
    // {
    //     var locator = new TypeLocator();
    //     Assert.ThrowsException<ArgumentException>(() => locator.FindTypes(new string(), false));
    // }

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
        var type = locator.FindTypesByName("ExternalSimpleType", assemblyFiles);
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
        var type = locator.FindTypesByName("ExternalSimpleType", assemblyFiles);
        Assert.AreEqual(1, type.Count());
        var item = type.FirstOrDefault();
        Assert.AreEqual("ExternalSimpleType", item.Name);

        var anotherType = locator.FindTypesByName("ExternalSimpleType");
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
            var type = locator.FindTypesByName("MissingExternalSimpleType", assemblyFiles);
            Assert.AreEqual(0, type.Count());
        });
    }

    [TestMethod]
    public void WillForceDomainScan()
    {
        var locator = new TypeLocator();
        var type = locator.FindTypesByName(nameof(SimpleType));
        Assert.AreEqual(1, type.Count());
        var item = type.FirstOrDefault();
        Assert.AreEqual(nameof(SimpleType), item.Name);
    }

    [TestMethod]
    public void WillUseCache()
    {
        var locator = new TypeLocator();
        var type = locator.FindTypesByName(nameof(SimpleType));
        Assert.AreEqual(1, type.Count());
        var item = type.FirstOrDefault();
        Assert.AreEqual(nameof(SimpleType), item.Name);

        var anotherType = locator.FindTypesByName(nameof(SimpleType));
        Assert.AreEqual(1, anotherType.Count());
        item = anotherType.FirstOrDefault();
        Assert.AreEqual(nameof(SimpleType), item.Name);
    }
}