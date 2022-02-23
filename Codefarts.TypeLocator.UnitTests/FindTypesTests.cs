using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Codefarts.TypeLocator.UnitTests;

[TestClass]
public class FindTypesTests
{
    [TestMethod]
    public void NullTypeName()
    {
        var results = TypeLocator.FindTypes(null);
        //Assert.ThrowsException<ArgumentException>(() => TypeLocator.FindTypes(null));
    }
}