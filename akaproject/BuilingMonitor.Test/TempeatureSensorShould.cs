﻿using Xunit;
using Akka.Actor;
using Akka.TestKit.Xunit2;
using BuildingMonitor.Actors;
using BuildingMonitor.Messages;

namespace BuilingMonitor.Test
{
    public class TempeatureSensorShould:TestKit
    {
        [Fact]
        public void InitializeSensorMetaData()
        {
            var probe = CreateTestProbe();
            var sensor = Sys.ActorOf(TemperatureSensor.Props("a", "1"));

            sensor.Tell(new RequestMetadata(1), probe.Ref);

            var received = probe.ExpectMsg<RespondMetadata>();

            Assert.Equal(1, received.RequsteId);
            Assert.Equal("a", received.FloorId);
            Assert.Equal("1", received.SensorId);
        }

        [Fact]
        public void StartWithNoTemperature()
        {
            var probe = CreateTestProbe();
            var sensor = Sys.ActorOf(TemperatureSensor.Props("a", "1"));
            sensor.Tell(new RequestTempeture(1), probe.Ref);
            var received = probe.ExpectMsg<RespondTempeture>();

            Assert.Null(received.Temperature);
        }

        [Fact]
        public void ConfirmTemperatureUpdate()
        {
            var probe = CreateTestProbe();
            var sensor = Sys.ActorOf(TemperatureSensor.Props("a", "1"));

            sensor.Tell(new RequestUpdateTemperature(42, 100), probe.Ref);
            probe.ExpectMsg<RespondTemperatureUpdated>(m => Assert.Equal(42, m.RequestId));
        }

        [Fact]
        public void UpdateNewTemperature()
        {
            var probe = CreateTestProbe();
            var sensor = Sys.ActorOf(TemperatureSensor.Props("a", "1"));

            sensor.Tell(new RequestUpdateTemperature(42, 100));
            sensor.Tell(new RequestTempeture(1), probe.Ref);

            var received =   probe.ExpectMsg<RespondTempeture>();
         
            Assert.Equal(100, received.Temperature);
            Assert.Equal(1, received.RequestId);
        }

        [Fact]
        public void RegisterSensor()
        {
            var probe = CreateTestProbe();
            var sensor = Sys.ActorOf(TemperatureSensor.Props("a", "1"));

            sensor.Tell(new RequestRegisterTemperatureSensor(1, "a", "1"), probe.Ref);
            var received = probe.ExpectMsg<RespondSensorRegistered>();

            Assert.Equal(1, received.RequestId);
            Assert.Equal(sensor, received.SensorReference);
        }

        [Fact]
        public void NotRegisterSensorWhenIncorrectFloorId()
        {
            var probe = CreateTestProbe();
            var eventStreamProbe = CreateTestProbe();

            Sys.EventStream.Subscribe(eventStreamProbe, typeof(Akka.Event.UnhandledMessage));
            var sensor = Sys.ActorOf(TemperatureSensor.Props("a", "1"));
    
            sensor.Tell(new RequestRegisterTemperatureSensor(1, "b", "1"), probe.Ref);
            probe.ExpectNoMsg();

            var unhundle = eventStreamProbe.ExpectMsg<Akka.Event.UnhandledMessage>();

            Assert.IsType<RequestRegisterTemperatureSensor>(unhundle.Message);
        }

        [Fact]
        public void NotRegisterSensorWhenIncorrectSensorId()
        {
            var probe = CreateTestProbe();
            var eventStreamProbe = CreateTestProbe();

            Sys.EventStream.Subscribe(eventStreamProbe, typeof(Akka.Event.UnhandledMessage));

            var sensor = Sys.ActorOf(TemperatureSensor.Props("a", "1"));

            sensor.Tell(new RequestRegisterTemperatureSensor(1, "a", "4"), probe.Ref);
            probe.ExpectNoMsg();

            var unhundle = eventStreamProbe.ExpectMsg<Akka.Event.UnhandledMessage>();
            Assert.IsType<RequestRegisterTemperatureSensor>(unhundle.Message);
        }
    }
}
