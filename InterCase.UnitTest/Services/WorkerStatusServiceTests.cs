using Hangfire;
using Hangfire.Storage;
using Hangfire.Storage.Monitoring;
using InterCase.Common;
using InterCase.Service.Services.OtherServices;
using Microsoft.Extensions.Time.Testing;
using NSubstitute;

namespace InterCase.UnitTest.Services
{
    [TestClass]
    public sealed class WorkerStatusServiceTests
    {
        private static readonly DateTime Now = new(2026, 11, 23, 15, 0, 0, DateTimeKind.Utc);

        private static WorkerStatusService ServiceWith(params DateTime?[] heartbeats)
        {
            var storage = Substitute.For<JobStorage>();
            var monitoring = Substitute.For<IMonitoringApi>();
            storage.GetMonitoringApi().Returns(monitoring);
            monitoring.Servers().Returns(heartbeats
                .Select((heartbeat, i) => new ServerDto { Name = $"intercase-worker:{i}", Heartbeat = heartbeat })
                .ToList());

            return new WorkerStatusService(storage, new FakeTimeProvider(new DateTimeOffset(Now)));
        }

        [TestMethod]
        public void GetStatus_WhenNoServerIsRegistered_IsNotRunning()
        {
            var status = ServiceWith().GetStatus();

            Assert.IsFalse(status.IsRunning);
            Assert.AreEqual(0, status.ActiveServers);
            Assert.IsNull(status.LastHeartbeatAt);
        }

        [TestMethod]
        public void GetStatus_WhenAServerSentARecentHeartbeat_IsRunning()
        {
            var status = ServiceWith(Now.AddSeconds(-5)).GetStatus();

            Assert.IsTrue(status.IsRunning);
            Assert.AreEqual(1, status.ActiveServers);
            Assert.AreEqual(Now.AddSeconds(-5), status.LastHeartbeatAt);
        }

        [TestMethod]
        public void GetStatus_WhenHeartbeatIsExactlyAtTheTimeout_IsStillRunning()
        {
            var status = ServiceWith(Now - WorkerHeartbeat.Timeout).GetStatus();

            Assert.IsTrue(status.IsRunning);
        }

        [TestMethod]
        public void GetStatus_WhenTheLastHeartbeatIsOlderThanTheTimeout_IsNotRunningAndReportsIt()
        {
            var lastSeen = Now - WorkerHeartbeat.Timeout - TimeSpan.FromSeconds(1);

            var status = ServiceWith(lastSeen).GetStatus();

            Assert.IsFalse(status.IsRunning);
            Assert.AreEqual(0, status.ActiveServers);
            Assert.AreEqual(lastSeen, status.LastHeartbeatAt);
        }

        [TestMethod]
        public void GetStatus_CountsOnlyServersWithFreshHeartbeats()
        {
            var status = ServiceWith(Now.AddMinutes(-10), Now.AddSeconds(-2), Now.AddSeconds(-12), null).GetStatus();

            Assert.IsTrue(status.IsRunning);
            Assert.AreEqual(2, status.ActiveServers);
            Assert.AreEqual(Now.AddSeconds(-2), status.LastHeartbeatAt);
        }

        [TestMethod]
        public void GetStatus_TreatsStoredHeartbeatsAsUtc()
        {
            var unspecified = DateTime.SpecifyKind(Now.AddSeconds(-3), DateTimeKind.Unspecified);

            var status = ServiceWith(unspecified).GetStatus();

            Assert.IsTrue(status.IsRunning);
            Assert.AreEqual(DateTimeKind.Utc, status.LastHeartbeatAt!.Value.Kind);
        }

        [TestMethod]
        public void HeartbeatTimeout_LeavesRoomForMissedHeartbeats()
        {
            Assert.IsGreaterThanOrEqualTo(WorkerHeartbeat.Interval * 2, WorkerHeartbeat.Timeout);
        }
    }
}
