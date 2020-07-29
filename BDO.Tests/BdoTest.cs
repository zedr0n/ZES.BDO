using System;
using System.Collections.Generic;
using BDO.Core;
using SimpleInjector;
using Xunit.Abstractions;
using ZES.Tests;

namespace BDO.Tests
{
    public class BdoTest : Test
    {
        protected BdoTest(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {
        }
        
        protected override Container CreateContainer(List<Action<Container>> registrations = null)
        {
            var regs = new List<Action<Container>>
            {
                Config.RegisterAll,
            };
            if (registrations != null)
                regs.AddRange(registrations);

            return base.CreateContainer(regs);
        }
    }
}