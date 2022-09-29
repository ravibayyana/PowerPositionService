using Microsoft.Reactive.Testing;
using Moq;
using PowerPositionService.FileUtil;
using PowerPositionService.Logger;
using PowerPositionService.Service;
using PowerPositionService.Setttings;
using PowerPositionService.Utils;
using Services;

namespace PowerPositionServiceTests
{
    public class PowerPositionTests
    {
        private readonly TestScheduler _testScheduler;

        public PowerPositionTests()
        {
            _testScheduler = new TestScheduler();
        }

        [Fact]
        public void Calling_StartCalculatingPowerPositions_Should_Call_Only_Once()
        {
            var currentDate = DateTime.Now;
            var customLoggerMock = new Mock<ICustomLogger>();
            var powerServiceMock = new Mock<IPowerService>();
            var utilityMock = new Mock<IUtility>();
            var writeToCsvFileMock = new Mock<IWriteToCsvFile>();

            var schedulerMock = new Mock<ICustomSchedulerProvider>();
            schedulerMock.Setup(x => x.TaskPool).Returns(_testScheduler);

            var positionService = new PositionService(customLoggerMock.Object, 
                utilityMock.Object, writeToCsvFileMock.Object, 
                powerServiceMock.Object);

            positionService.StartCalculatingPowerPositions();
            positionService.StartCalculatingPowerPositions();
            positionService.StartCalculatingPowerPositions();
            positionService.StartCalculatingPowerPositions();
            positionService.StartCalculatingPowerPositions();

            utilityMock.Verify(x => x.StartTimer(It.IsAny<Action>()), Times.Once());
        }

        [Fact]
        public void TimerCallBack_Should_Call_At_Time_Zero()
        {
            var customLoggerMock = new Mock<ICustomLogger>();
            var powerServiceMock = new Mock<IPowerService>();
            var writeToCsvFileMock = new Mock<IWriteToCsvFile>();
            var appSettingsMock = new Mock<IAppConfigSettings>();
            appSettingsMock.Setup(x => x.ScheduleIntervalInMinutes).Returns(100);

            var schedulerMock = new Mock<ICustomSchedulerProvider>();
            schedulerMock.Setup(x => x.TaskPool).Returns(_testScheduler);

            var utility= new Utility(appSettingsMock.Object, schedulerMock.Object);

            var positionService = new PositionService(customLoggerMock.Object,
                utility, writeToCsvFileMock.Object,
                powerServiceMock.Object);

            positionService.StartCalculatingPowerPositions();
            _testScheduler.AdvanceBy(TimeSpan.FromMilliseconds(1).Ticks);

            powerServiceMock.Verify(x => x.GetTradesAsync(It.IsAny<DateTime>()), Times.Once());
        }

        [Fact]
        public void TimerCallBack_Should_Call_At_Each_Time_Interval()
        {
            var customLoggerMock = new Mock<ICustomLogger>();
            var powerServiceMock = new Mock<IPowerService>();
            var writeToCsvFileMock = new Mock<IWriteToCsvFile>();
            var appSettingsMock = new Mock<IAppConfigSettings>();
            appSettingsMock.Setup(x => x.ScheduleIntervalInMinutes).Returns(1);

            var schedulerMock = new Mock<ICustomSchedulerProvider>();
            schedulerMock.Setup(x => x.TaskPool).Returns(_testScheduler);

            var utility = new Utility(appSettingsMock.Object, schedulerMock.Object);

            var positionService = new PositionService(customLoggerMock.Object,
                utility, writeToCsvFileMock.Object,
                powerServiceMock.Object);

            positionService.StartCalculatingPowerPositions();

            _testScheduler.AdvanceBy(TimeSpan.FromMilliseconds(1).Ticks);
            powerServiceMock.Verify(x => x.GetTradesAsync(It.IsAny<DateTime>()), Times.Once());
            writeToCsvFileMock.Verify(x => x.Write(It.IsAny<PositionVolumes>()),Times.Once);

            _testScheduler.AdvanceTo(TimeSpan.FromMinutes(1.1).Ticks);
            powerServiceMock.Verify(x => x.GetTradesAsync(It.IsAny<DateTime>()), Times.Exactly(2));
            writeToCsvFileMock.Verify(x => x.Write(It.IsAny<PositionVolumes>()), Times.Exactly(2));


            _testScheduler.AdvanceTo(TimeSpan.FromMinutes(2.1).Ticks);
            powerServiceMock.Verify(x => x.GetTradesAsync(It.IsAny<DateTime>()), Times.Exactly(3));
            writeToCsvFileMock.Verify(x => x.Write(It.IsAny<PositionVolumes>()), Times.Exactly(3));
        }

        [Fact]
        public void Check_Data_From_PowerService_GetTrades()
        {
            var customLoggerMock = new Mock<ICustomLogger>();
            var powerServiceMock = new Mock<IPowerService>();
            var writeToCsvFileMock = new Mock<IWriteToCsvFile>();
            var appSettingsMock = new Mock<IAppConfigSettings>();
            appSettingsMock.Setup(x => x.ScheduleIntervalInMinutes).Returns(1);

            var schedulerMock = new Mock<ICustomSchedulerProvider>();
            schedulerMock.Setup(x => x.TaskPool).Returns(_testScheduler);

            var utility = new Utility(appSettingsMock.Object, schedulerMock.Object);

            var listPowerTrades = new List<PowerTrade> { PowerTrade.Create(utility.CurrentDateTime, 2) };
            powerServiceMock.Setup(x => x.GetTradesAsync(It.IsAny<DateTime>())).ReturnsAsync(listPowerTrades);
            PositionVolumes returnData = null;
            writeToCsvFileMock.Setup(x => x.Write(It.IsAny<PositionVolumes>()))
                .Callback((PositionVolumes pv)  => returnData = pv);

            var positionService = new PositionService(customLoggerMock.Object,
                utility, writeToCsvFileMock.Object,
                powerServiceMock.Object);

            positionService.StartCalculatingPowerPositions();

            _testScheduler.AdvanceBy(TimeSpan.FromMilliseconds(1).Ticks);
            powerServiceMock.Verify(x => x.GetTradesAsync(It.IsAny<DateTime>()), Times.Once());
            writeToCsvFileMock.Verify(x => x.Write(It.IsAny<PositionVolumes>()), Times.Once);
            customLoggerMock.Verify(x => x.LogInformation(It.IsAny<string>()), Times.Exactly(2));
            customLoggerMock.Verify(x => x.LogError(It.IsAny<Exception>(), It.IsAny<string>()), Times.Exactly(0));

            Assert.Equal(2, returnData.PowerPositions.Count);
        }

        [Fact]
        public void Check_Data_From_PowerService_GetTrades_ThrowsException()
        {
            var customLoggerMock = new Mock<ICustomLogger>();
            var powerServiceMock = new Mock<IPowerService>();
            var writeToCsvFileMock = new Mock<IWriteToCsvFile>();
            var appSettingsMock = new Mock<IAppConfigSettings>();
            appSettingsMock.Setup(x => x.ScheduleIntervalInMinutes).Returns(1);

            var schedulerMock = new Mock<ICustomSchedulerProvider>();
            schedulerMock.Setup(x => x.TaskPool).Returns(_testScheduler);

            var utility = new Utility(appSettingsMock.Object, schedulerMock.Object);

            powerServiceMock.Setup(x => x.GetTradesAsync(It.IsAny<DateTime>()))
                .ThrowsAsync(new InvalidOperationException("User Generated Exception"));
            PositionVolumes returnData = null;
            writeToCsvFileMock.Setup(x => x.Write(It.IsAny<PositionVolumes>()))
                .Callback((PositionVolumes pv) => returnData = pv);

            Assert.ThrowsAsync<InvalidOperationException>(() => powerServiceMock.Object.GetTradesAsync(DateTime.Now));
        }

        [Fact]
        public void Check_Data_From_PowerService_GetTrades_ThrowsException_WriteFile_Called_Zero_Times()
        {
            var customLoggerMock = new Mock<ICustomLogger>();
            var powerServiceMock = new Mock<IPowerService>();
            var writeToCsvFileMock = new Mock<IWriteToCsvFile>();
            var appSettingsMock = new Mock<IAppConfigSettings>();
            appSettingsMock.Setup(x => x.ScheduleIntervalInMinutes).Returns(1);

            var schedulerMock = new Mock<ICustomSchedulerProvider>();
            schedulerMock.Setup(x => x.TaskPool).Returns(_testScheduler);

            var utility = new Utility(appSettingsMock.Object, schedulerMock.Object);

            powerServiceMock.Setup(x => x.GetTradesAsync(It.IsAny<DateTime>()))
                .ThrowsAsync(new InvalidOperationException("User Generated Exception"));

            var positionService = new PositionService(customLoggerMock.Object,
                utility, writeToCsvFileMock.Object,
                powerServiceMock.Object);

            positionService.StartCalculatingPowerPositions();

            _testScheduler.AdvanceBy(TimeSpan.FromMilliseconds(1).Ticks);
            powerServiceMock.Verify(x => x.GetTradesAsync(It.IsAny<DateTime>()), Times.Once());
            writeToCsvFileMock.Verify(x => x.Write(It.IsAny<PositionVolumes>()), Times.Exactly(0));
            customLoggerMock.Verify(x => x.LogError(It.IsAny<Exception>(), It.IsAny<string>()), Times.Once);
        }



        //var currentDate = DateTime.Now;
        //var customLoggerMock = new Mock<ICustomLogger>();
        //var powerServiceMock = new Mock<IPowerService>();
        //var appSettingsMock = new Mock<IAppConfigSettings>();
        //appSettingsMock.Setup(x => x.ScheduleIntervalInMinutes).Returns(1);
        //var writeToCsvFileMock = new Mock<IWriteToCsvFile>();

        ////var utility = new Utility(appSettingsMock.Object);

        //var positionService = new PositionService(customLoggerMock.Object, utility,
        //    writeToCsvFileMock.Object, powerServiceMock.Object);

        //positionService.StartCalculatingPowerPositions();

        //utilityMock.Setup(x => x.StartTimer(It.IsAny<TimerCallback>())).Callback(() => { });
        //utilityMock.Setup(x => x.StartAt).Returns(TimeSpan.Zero);
        //utilityMock.Setup(x => x.CurrentDateTime).Returns(currentDate);

        //var listPowerTrades = new List<PowerTrade> { PowerTrade.Create(currentDate, 2) };
        //powerServiceMock.Setup(x => x.GetTradesAsync(It.IsAny<DateTime>())).ReturnsAsync(listPowerTrades);

        //var positionService = new PositionService(customLoggerMock.Object, utilityMock.Object,
        //    writeToCsvFileMock.Object, powerServiceMock.Object);

        //utilityMock.Setup(x => x.StartTimer(It.IsAny<TimerCallback>())).Callback(positionService.TimerCallBack);

        //positionService.StartCalculatingPowerPositions();
        //positionService.StartCalculatingPowerPositions();
        //positionService.StartCalculatingPowerPositions();
        //positionService.StartCalculatingPowerPositions();
        //positionService.StartCalculatingPowerPositions();
        //writeToCsvFileMock.Verify(x => x.Write(It.IsAny<PositionVolumes>()),Times.Once);
        //utilityMock.Verify(x => x.StartTimer(It.IsAny<TimerCallback>()), Times.Once);
        //powerServiceMock.Verify(x => x.GetTradesAsync(It.IsAny<DateTime>()), Times.Once());
        //writeToCsvFileMock.Verify(x => x.Write(It.IsAny<PositionVolumes>()), Times.Once);

    }
}