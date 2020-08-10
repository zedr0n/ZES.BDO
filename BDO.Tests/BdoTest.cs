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
            {
                regs.Add(c =>
                {
                    Config.RegisterAll(c);
                    Enhancement.Config.RegisterAll(c);
                });
            }
            else
            {
                regs.Add(c =>
                {
                    Config.RegisterWithoutSagas(c);
                    Enhancement.Config.RegisterWithoutSagas(c);
                });
            }

            if (registrations != null)
                regs.AddRange(registrations);

            return base.CreateContainer(regs);
        }
    }
}