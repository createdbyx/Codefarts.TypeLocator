using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Codefarts.TypeLocator.UnitTests;

[TestClass]
public class FindTypesTests
{
    [TestMethod]
    public void NullFilter()
    {
        Assert.ThrowsException<ArgumentNullException>(() => TypeLocator.FindTypes(null));
    }
}