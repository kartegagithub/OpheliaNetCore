using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ophelia;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ophelia.Tests
{
    [TestClass()]
    public class UtilityTests
    {
        [TestMethod()]
        public void GenerateRandomPasswordTest()
        {
            Assert.IsNotNull(Utility.GenerateRandomPassword(10));
        }
    }
}