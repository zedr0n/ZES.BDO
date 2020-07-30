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
        
        protected Container CreateContainer(List<Action<Container>> registrations = null, bool useSagas = true)
        {
            var regs = new List<Action<Container>>();
            if (useSagas)
                regs.Add(Config.RegisterAll);
            else
                regs.Add(Config.RegisterWithoutSagas);
            
            if (registrations != null)
                regs.AddRange(registrations);

            return base.CreateContainer(regs);
        }
    }
}