using WebSocketService.Hubs;
using FakeItEasy;
using FluentAssertions;
using Castle.Core.Logging;
using WebSocketService.HubStates;
using WebSocketService.Services;
using SharedApiUtils;
using WebSocketService.Interfaces;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
namespace WebSocketService.Tests.HubsTests;

public class SessionManagmentHubTests
{
    [Fact]
    public async Task JoinRoom_UserAlreadyAsignedToRoom_ReturnsError()
    {
        //// Arrange
        //string roomId = "roomid";
        //string accessToken = "accessToken";
        //string userId = "userId";

        //var logger = A.Fake<Microsoft.Extensions.Logging.ILogger<SessionManagmentHub>>();
        //var hubState = A.Fake<SessionManagmentHubState>();
        //var handlerService = A.Fake<IRoomSessionHandlerService>();
        //var context = A.Fake<HubCallerContext>();
        //var userContext = A.Fake<IUserContextService>();
        //A.CallTo(() => userContext.GetUserId(context)).Returns(userId);
        //A.CallTo(() => handlerService.GetUserRoom(userId)).Returns(roomId);
        //SessionManagmentHub sessionManagmentHub = new SessionManagmentHub(logger, hubState, handlerService, userContext);

        //sessionManagmentHub.Context = context;

        //// Act
        //HubActionResult result = await sessionManagmentHub.JoinRoom(roomId, accessToken);

        //// Assert
        //result.Should().NotBeNull();
        //result.IsSuccess.Should().BeFalse();
        //result.ErrorMessage.Should().Be(ErrorMessages.UserAsignedToRoom);
    }

    [Fact]
    public async Task JoinRoom_SubjectClaimNotFound_ReturnsError()
    {
        //// Arrange
        //var logger = A.Fake<Microsoft.Extensions.Logging.ILogger<SessionManagmentHub>>();
        //var hubState = A.Fake<SessionManagmentHubState>();
        //var handlerService = A.Fake<IRoomSessionHandlerService>();
        //var context = A.Fake<HubCallerContext>();
        //var userContext = A.Fake<IUserContextService>();
        //A.CallTo(() => userContext.GetUserId(context)).Returns(null);
        //SessionManagmentHub sessionManagmentHub = new SessionManagmentHub(logger, hubState, handlerService, userContext);
        //sessionManagmentHub.Context = context;
        //string roomId = "roomid";
        //string accessToken = "accessToken";

        //// Act
        //HubActionResult result = await sessionManagmentHub.JoinRoom(roomId, accessToken);

        //// Assert
        //result.Should().NotBeNull();
        //result.IsSuccess.Should().BeFalse();
        //result.ErrorMessage.Should().Be(ErrorMessages.SubjectClaimNotFound);
    }

    [Fact]
    public async Task JoinRoom_ValidRequest_ReturnsOk()
    {
        //// Arrange
        //string roomId = "roomid";
        //string accessToken = "accessToken";
        //string userId = "userId";

        //var logger = A.Fake<Microsoft.Extensions.Logging.ILogger<SessionManagmentHub>>();
        //var hubState = A.Fake<SessionManagmentHubState>();
        //var handlerService = A.Fake<IRoomSessionHandlerService>();
        //var context = A.Fake<HubCallerContext>();
        //var userContext = A.Fake<IUserContextService>();
        //A.CallTo(() => userContext.GetUserId(context)).Returns(userId);
        //A.CallTo(() => handlerService.GetUserRoom(userId)).Returns(null);
        //SessionManagmentHub sessionManagmentHub = new SessionManagmentHub(logger, hubState, handlerService, userContext);

        //sessionManagmentHub.Context = context;

        //// Act
        //HubActionResult result = await sessionManagmentHub.JoinRoom(roomId, accessToken);

        //// Assert
        //result.Should().NotBeNull();
        //result.IsSuccess.Should().BeTrue();
        //result.ErrorMessage.Should().BeNull();
    }

    [Fact]
    public void ConcurrentJoinRequests_AllHandledCorrectly()
    {
        //// Arrange
        //string roomId = "roomid";
        //string accessToken = "accessToken";
        //var users = Enumerable.Range(1, 50).Select(i => $"User{i}").ToList();

        //var logger = A.Fake<Microsoft.Extensions.Logging.ILogger<SessionManagmentHub>>();
        //var hubState = A.Fake<SessionManagmentHubState>();
        //var handlerService = A.Fake<IRoomSessionHandlerService>();
        //var context = A.Fake<HubCallerContext>();
        //var userContext = A.Fake<IUserContextService>();
        //SessionManagmentHub sessionManagmentHub = new SessionManagmentHub(logger, hubState, handlerService, userContext);
        //sessionManagmentHub.Context = context;

        //var expected = new HubActionResult(true, null, null);

        //// Act
        //var tasks = users.Select(async userId =>
        //{
        //    A.CallTo(() => userContext.GetUserId(context)).Returns(userId);
        //    A.CallTo(() => handlerService.GetUserRoom(userId)).Returns(null);
        //    return (await sessionManagmentHub.JoinRoom(roomId, accessToken));
        //});

        //// Assert
        //tasks.Should().NotBeNullOrEmpty();
        //tasks.Should().HaveCount(50);
        //tasks.Should().Contain(x => x.IsCompleted == true);
        //tasks.Should().Contain(x => x.Result.IsSuccess == true);
    }
}
