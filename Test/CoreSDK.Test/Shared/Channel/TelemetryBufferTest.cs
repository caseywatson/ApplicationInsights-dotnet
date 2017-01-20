﻿namespace Microsoft.ApplicationInsights.Channel
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using DataContracts;
    using Microsoft.ApplicationInsights.Extensibility;
    using Microsoft.ApplicationInsights.Extensibility.Implementation;
    using Microsoft.ApplicationInsights.Extensibility.Implementation.Platform;
    using Microsoft.ApplicationInsights.TestFramework;
#if NET40 || NET45 || NET46
    using Microsoft.VisualStudio.TestTools.UnitTesting;
#else
    using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
#endif
    using Assert = Xunit.Assert;

    [TestClass]
    public class TelemetryBufferTest
    {
        [TestMethod]
        public void DefaultValueIsAppropriateForProductionEnvironmentAndUnitTests()
        {
            var buffer = new TelemetryBuffer();
            Assert.Equal(500, buffer.Capacity);
            Assert.Equal(1000000, buffer.MaximumUnsentBacklogSize);
        }

        [TestMethod]
        public void CanBeSetByChannelToTunePerformance()
        {
            var buffer = new TelemetryBuffer();
            buffer.Capacity = 42;
            buffer.MaximumUnsentBacklogSize = 9999;
            Assert.Equal(42, buffer.Capacity);
            Assert.Equal(9999, buffer.MaximumUnsentBacklogSize);
        }

        [TestMethod]
        public void WhenNewValueIsLessThanOneSetToDefault()
        {
            var buffer = new TelemetryBuffer();
            buffer.Capacity = 0;
            buffer.MaximumUnsentBacklogSize = 0;

            Assert.Equal(buffer.Capacity, 500);
            Assert.Equal(buffer.MaximumUnsentBacklogSize, 1000000);
        }

        [TestMethod]
        public void TelemetryBufferCallingOnFullActionWhenBufferCapacityReached()
        {
            IEnumerable<ITelemetry> items = null;
            TelemetryBuffer buffer = new TelemetryBuffer { Capacity = 2 };
            buffer.OnFull = () => { items = buffer.Dequeue(); };

            buffer.Enqueue(new EventTelemetry("Event1"));
            buffer.Enqueue(new EventTelemetry("Event2"));

            Assert.NotNull(items);
            Assert.Equal(2, items.Count());
        }

        [TestMethod]
        public void TelemetryBufferDoNotGrowBeyondMaxBacklogSize()
        {            
            TelemetryBuffer buffer = new TelemetryBuffer { Capacity = 2, MaximumUnsentBacklogSize= 5};
            buffer.OnFull = () => { //intentionaly blank to simulate situation where buffer
                                    //is not emptied.
                                  };

            // Add more items to buffer than the max backlog size
            buffer.Enqueue(new EventTelemetry("Event1"));
            buffer.Enqueue(new EventTelemetry("Event2"));
            buffer.Enqueue(new EventTelemetry("Event3"));
            buffer.Enqueue(new EventTelemetry("Event4"));
            buffer.Enqueue(new EventTelemetry("Event5"));
            buffer.Enqueue(new EventTelemetry("Event6"));

            // validate that items are not added after maxunsentbacklogsize is reached.
            // this also validate that items can still be added after Capacity is reached as it is only a soft limit.
            int bufferItemCount = buffer.Dequeue().Count();
            Assert.Equal(5, bufferItemCount);

        }
    }
}
