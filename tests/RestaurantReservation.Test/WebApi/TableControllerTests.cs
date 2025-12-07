using FluentResults;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Moq;
using RestaurantReservation.Application.DTOs.Request.Table;
using RestaurantReservation.Application.DTOs.Response.Table;
using RestaurantReservation.Application.Interfaces;
using RestaurantReservation.Domain.Enums;
using RestaurantReservation.WebApi.Controllers;
using RestaurantReservation.WebApi.Localization;

namespace RestaurantReservation.Test.WebApi;

public class TableControllerTests
{
    private readonly Mock<ITableAppService> _tableAppServiceMock;
    private readonly Mock<IStringLocalizer<SharedResource>> _localizerMock;
    private readonly TableController _controller;

    public TableControllerTests()
    {
        _tableAppServiceMock = new Mock<ITableAppService>();

        _localizerMock = new Mock<IStringLocalizer<SharedResource>>();
        _localizerMock
            .Setup(l => l[It.IsAny<string>()])
            .Returns((string key) => new LocalizedString(key, key, resourceNotFound: true));
        _localizerMock
            .Setup(l => l[It.IsAny<string>(), It.IsAny<object[]>()])
            .Returns((string key, object[] args) => new LocalizedString(key, key, resourceNotFound: true));

        _controller = new TableController(
            _tableAppServiceMock.Object,
            _localizerMock.Object);
    }
   
    [Fact]
    public async Task GetAll_ShouldReturnOk_WhenServiceReturnsSuccess()
    {
        // Arrange
        var dtoList = new List<TableResponse>
        {
            new TableResponse
            {
                Id = Guid.NewGuid(),
                Name = "Mesa 1",
                Capacity = 4,
                Status = StatusTable.Disponivel
            },
            new TableResponse
            {
                Id = Guid.NewGuid(),
                Name = "Mesa 2",
                Capacity = 2,
                Status = StatusTable.Reservada
            }
        }.AsReadOnly();

        _tableAppServiceMock
            .Setup(s => s.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok<IReadOnlyCollection<TableResponse>>(dtoList));

        // Act
        var actionResult = await _controller.GetAll(CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(actionResult);
        var value = Assert.IsAssignableFrom<IReadOnlyCollection<TableResponse>>(okResult.Value);

        Assert.Equal(2, value.Count);
        _tableAppServiceMock.Verify(
            s => s.GetAllAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetAll_ShouldReturnProblemDetails_WhenServiceFails()
    {
        // Arrange
        _tableAppServiceMock
            .Setup(s => s.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Fail<IReadOnlyCollection<TableResponse>>("Some error"));

        // Act
        var actionResult = await _controller.GetAll(CancellationToken.None);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(actionResult);
        Assert.Equal(StatusCodes.Status400BadRequest, objectResult.StatusCode);

        var problem = Assert.IsType<ProblemDetails>(objectResult.Value);
        Assert.Contains("Some error", problem.Detail);
    }

    [Fact]
    public async Task GetById_ShouldReturnOk_WhenServiceReturnsSuccess()
    {
        // Arrange
        var id = Guid.NewGuid();
        var dto = new TableResponse
        {
            Id = id,
            Name = "Mesa 1",
            Capacity = 4,
            Status = StatusTable.Disponivel
        };

        _tableAppServiceMock
            .Setup(s => s.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(dto));

        // Act
        var actionResult = await _controller.GetById(id, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var value = Assert.IsType<TableResponse>(okResult.Value);

        Assert.Equal(id, value.Id);
        Assert.Equal("Mesa 1", value.Name);

        _tableAppServiceMock.Verify(
            s => s.GetByIdAsync(id, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetById_ShouldReturnProblemDetails_WhenServiceFails()
    {
        // Arrange
        var id = Guid.NewGuid();

        _tableAppServiceMock
            .Setup(s => s.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Fail<TableResponse>("Table not found."));

        // Act
        var actionResult = await _controller.GetById(id, CancellationToken.None);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(actionResult.Result);
        Assert.Equal(StatusCodes.Status400BadRequest, objectResult.StatusCode);

        var problem = Assert.IsType<ProblemDetails>(objectResult.Value);
        Assert.Contains("Table not found.", problem.Detail);
    }
   
    [Fact]
    public async Task Create_ShouldReturnBadRequest_WhenModelStateInvalid()
    {
        // Arrange
        _controller.ModelState.AddModelError("Name", "Required");

        var request = new CreateTableRequest();

        // Act
        var actionResult = await _controller.Create(request, CancellationToken.None);

        // Assert
        var badRequest = Assert.IsType<BadRequestResult>(actionResult.Result);
        _tableAppServiceMock.Verify(
            s => s.CreateAsync(It.IsAny<CreateTableRequest>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Create_ShouldReturnCreated_WhenServiceReturnsSuccess()
    {
        // Arrange
        var request = new CreateTableRequest
        {
            Name = "Mesa 1",
            Capacity = 4
        };

        var dto = new TableResponse
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Capacity = request.Capacity,
            Status = StatusTable.Disponivel
        };

        _tableAppServiceMock
            .Setup(s => s.CreateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(dto));

        // Act
        var actionResult = await _controller.Create(request, CancellationToken.None);

        // Assert
        var createdResult = Assert.IsType<CreatedResult>(actionResult.Result);
        var value = Assert.IsType<TableResponse>(createdResult.Value);

        Assert.Equal(dto.Id, value.Id);
        Assert.Equal(dto.Name, value.Name);

        _tableAppServiceMock.Verify(
            s => s.CreateAsync(request, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Create_ShouldReturnProblemDetails_WhenServiceFails()
    {
        // Arrange
        var request = new CreateTableRequest
        {
            Name = "Mesa 1",
            Capacity = 4
        };

        _tableAppServiceMock
            .Setup(s => s.CreateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Fail<TableResponse>("Name already exists"));

        // Act
        var actionResult = await _controller.Create(request, CancellationToken.None);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(actionResult.Result);
        Assert.Equal(StatusCodes.Status400BadRequest, objectResult.StatusCode);

        var problem = Assert.IsType<ProblemDetails>(objectResult.Value);
        Assert.Contains("Name already exists", problem.Detail);
    }
  
    [Fact]
    public async Task Update_ShouldReturnBadRequest_WhenModelStateInvalid()
    {
        // Arrange
        _controller.ModelState.AddModelError("Name", "Required");

        var id = Guid.NewGuid();
        var request = new UpdateTableRequest();

        // Act
        var actionResult = await _controller.Update(id, request, CancellationToken.None);

        // Assert
        var badRequest = Assert.IsType<BadRequestResult>(actionResult);
        _tableAppServiceMock.Verify(
            s => s.UpdateAsync(It.IsAny<UpdateTableRequest>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Update_ShouldReturnNoContent_WhenServiceReturnsSuccess()
    {
        // Arrange
        var id = Guid.NewGuid();
        var request = new UpdateTableRequest
        {
            Name = "Mesa Atualizada",
            Capacity = 6
        };

        _tableAppServiceMock
            .Setup(s => s.UpdateAsync(It.IsAny<UpdateTableRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok());

        // Act
        var actionResult = await _controller.Update(id, request, CancellationToken.None);

        // Assert
        var noContentResult = Assert.IsType<NoContentResult>(actionResult);

        _tableAppServiceMock.Verify(
            s => s.UpdateAsync(It.Is<UpdateTableRequest>(r => r.Id == id), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Update_ShouldReturnProblemDetails_WhenServiceFails()
    {
        // Arrange
        var id = Guid.NewGuid();
        var request = new UpdateTableRequest
        {
            Name = "Mesa Atualizada",
            Capacity = 6
        };

        _tableAppServiceMock
            .Setup(s => s.UpdateAsync(It.IsAny<UpdateTableRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Fail("Table not found."));

        // Act
        var actionResult = await _controller.Update(id, request, CancellationToken.None);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(actionResult);
        Assert.Equal(StatusCodes.Status400BadRequest, objectResult.StatusCode);

        var problem = Assert.IsType<ProblemDetails>(objectResult.Value);
        Assert.Contains("Table not found.", problem.Detail);
    }

    [Fact]
    public async Task Delete_ShouldReturnNoContent_WhenServiceReturnsSuccess()
    {
        // Arrange
        var id = Guid.NewGuid();

        _tableAppServiceMock
            .Setup(s => s.DeleteAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok());

        // Act
        var actionResult = await _controller.Delete(id, CancellationToken.None);

        // Assert
        var noContentResult = Assert.IsType<NoContentResult>(actionResult);

        _tableAppServiceMock.Verify(
            s => s.DeleteAsync(id, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Delete_ShouldReturnProblemDetails_WhenServiceFails()
    {
        // Arrange
        var id = Guid.NewGuid();

        _tableAppServiceMock
            .Setup(s => s.DeleteAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Fail("Table not found."));

        // Act
        var actionResult = await _controller.Delete(id, CancellationToken.None);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(actionResult);
        Assert.Equal(StatusCodes.Status400BadRequest, objectResult.StatusCode);

        var problem = Assert.IsType<ProblemDetails>(objectResult.Value);
        Assert.Contains("Table not found.", problem.Detail);
    }
}