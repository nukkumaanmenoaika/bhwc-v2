using Microsoft.VisualStudio.TestTools.UnitTesting;
using sotrudniki.Model;
using sotrudniki.ViewModel;
using System;

namespace UnitTest
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void RoleTest()
        {
            RoleViewModel v = new RoleViewModel();
            Role r = new Role { 
                Id = 5,
                NameRole = "Сварщик"
            };
            v.ListRole.Add(r);
            v.SaveChanges(v.ListRole);

        }
    }
}
