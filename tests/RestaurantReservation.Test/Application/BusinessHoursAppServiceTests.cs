using Moq;
using NodaTime;
using RestaurantReservation.Application.DTOs.Request.BusinessHour;
using RestaurantReservation.Application.Services;
using RestaurantReservation.Domain.Enums;
using RestaurantReservation.Domain.Repositories;

namespace RestaurantReservation.Test.Application;

public class BusinessHoursAppServiceTests
{
    private readonly Mock<IBusinessHoursRuleRepository> _repositoryMock;
    private readonly BusinessHoursAppService _service;
    private readonly DateTimeZone _timeZone;

    public BusinessHoursAppServiceTests()
    {
        _repositoryMock = new Mock<IBusinessHoursRuleRepository>();
        _timeZone = DateTimeZoneProviders.Tzdb["America/Sao_Paulo"];
        _service = new BusinessHoursAppService(_repositoryMock.Object, _timeZone);
    }

    private Instant ToInstant(LocalDateTime localDateTime)
    {
        var zoned = localDateTime.InZoneLeniently(_timeZone);
        return zoned.ToInstant();
    }

    [Fact]
    public async Task IsOpenAsync_ShouldReturnFalse_WhenReservationCrossesDifferentDates()
    {
        // Arrange: 23h até 01h do dia seguinte
        var startLocal = new LocalDateTime(2025, 4, 1, 23, 0);
        var endLocal = startLocal.PlusHours(2); // 2025-04-02 01:00

        var startsAt = ToInstant(startLocal);
        var endsAt = ToInstant(endLocal);

        // Act
        var result = await _service.IsOpenAsync(startsAt, endsAt, CancellationToken.None);

        // Assert
        Assert.False(result);
        
        _repositoryMock.Verify(
            r => r.GetRulesForDateAsync(It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnMappedDtos_WhenRulesExist()
    {
        // Arrange
        var rule1 = new BusinessHoursRule(
            new DateOnly(2025, 4, 1),
            new DateOnly(2025, 4, 30),
            null,
            WeekDay.Tuesday,
            new TimeOnly(11, 0),
            new TimeOnly(23, 0),
            false);

        var rule2 = BusinessHoursRule.CreateClosedDate(new DateOnly(2025, 4, 21));

        _repositoryMock
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<BusinessHoursRule> { rule1, rule2 });

        // Act
        var result = await _service.GetAllAsync(CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        var dtos = result.Value;

        Assert.Equal(2, dtos.Count);
        Assert.Contains(dtos, d => d.Id == rule1.Id && d.WeekDay == WeekDay.Tuesday && d.IsClosed == false);
        Assert.Contains(dtos, d => d.Id == rule2.Id && d.SpecificDate == new DateOnly(2025, 4, 21) && d.IsClosed);

        _repositoryMock.Verify(r => r.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
 
    [Fact]
    public async Task GetByIdAsync_ShouldFail_WhenRuleNotFound()
    {
        // Arrange
        var id = Guid.NewGuid();

        _repositoryMock
            .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((BusinessHoursRule?)null);

        // Act
        var result = await _service.GetByIdAsync(id, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailed);
        Assert.Contains(result.Errors, e => e.Message == "Rule not found.");

        _repositoryMock.Verify(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnDto_WhenRuleExists()
    {
        // Arrange
        var id = Guid.NewGuid();

        var rule = new BusinessHoursRule(
            new DateOnly(2025, 4, 1),
            new DateOnly(2025, 4, 30),
            null,
            WeekDay.Friday,
            new TimeOnly(18, 0),
            new TimeOnly(23, 59),
            false);

        _repositoryMock
            .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(rule);

        // Act
        var result = await _service.GetByIdAsync(id, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        var dto = result.Value;

        Assert.Equal(rule.Id, dto.Id);
        Assert.Equal(rule.StartDate, dto.StartDate);
        Assert.Equal(rule.EndDate, dto.EndDate);
        Assert.Equal(rule.WeekDay, dto.WeekDay);
        Assert.Equal(rule.IsClosed, dto.IsClosed);

        _repositoryMock.Verify(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()), Times.Once);
    }
 
    [Fact]
    public async Task CreateAsync_ShouldFail_WhenEndDateBeforeStartDate()
    {
        // Arrange
        var request = new CreateBusinessHoursRuleRequest
        {
            StartDate = new DateOnly(2025, 4, 10),
            EndDate = new DateOnly(2025, 4, 1),
            WeekDay = WeekDay.Monday,
            Open = new TimeOnly(9, 0),
            Close = new TimeOnly(18, 0),
            IsClosed = false
        };

        // Act
        var result = await _service.CreateAsync(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailed);
        Assert.Contains(result.Errors, e => e.Message == "EndDate must be >= StartDate.");

        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<BusinessHoursRule>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_ShouldFail_WhenNoSpecificDateNorWeekDayProvided()
    {
        // Arrange
        var request = new CreateBusinessHoursRuleRequest
        {
            StartDate = new DateOnly(2025, 4, 1),
            EndDate = new DateOnly(2025, 4, 30),
            SpecificDate = null,
            WeekDay = null,
            Open = new TimeOnly(9, 0),
            Close = new TimeOnly(18, 0),
            IsClosed = false
        };

        // Act
        var result = await _service.CreateAsync(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailed);
        Assert.Contains(result.Errors, e => e.Message == "Either SpecificDate or WeekDay must be provided.");

        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<BusinessHoursRule>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateClosedSpecificDateRule_WhenSpecificDateAndIsClosed()
    {
        // Arrange
        var specificDate = new DateOnly(2025, 4, 21);

        var request = new CreateBusinessHoursRuleRequest
        {
            SpecificDate = specificDate,
            IsClosed = true
            // Start/End/WeekDay/Open/Close são irrelevantes para CreateClosedDate
        };

        BusinessHoursRule? savedRule = null;

        _repositoryMock
            .Setup(r => r.AddAsync(It.IsAny<BusinessHoursRule>(), It.IsAny<CancellationToken>()))
            .Callback<BusinessHoursRule, CancellationToken>((rule, _) => savedRule = rule)
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.CreateAsync(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        var dto = result.Value;

        Assert.NotNull(savedRule);
        Assert.True(dto.IsClosed);
        Assert.Equal(specificDate, dto.SpecificDate);
        Assert.Equal(specificDate, savedRule!.StartDate);
        Assert.Equal(specificDate, savedRule.EndDate);

        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<BusinessHoursRule>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateWeeklyRule_WhenWeekDayProvided()
    {
        // Arrange
        var request = new CreateBusinessHoursRuleRequest
        {
            StartDate = new DateOnly(2025, 4, 1),
            EndDate = new DateOnly(2025, 4, 30),
            SpecificDate = null,
            WeekDay = WeekDay.Saturday,
            Open = new TimeOnly(11, 0),
            Close = new TimeOnly(23, 0),
            IsClosed = false
        };

        BusinessHoursRule? savedRule = null;

        _repositoryMock
            .Setup(r => r.AddAsync(It.IsAny<BusinessHoursRule>(), It.IsAny<CancellationToken>()))
            .Callback<BusinessHoursRule, CancellationToken>((rule, _) => savedRule = rule)
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.CreateAsync(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        var dto = result.Value;

        Assert.NotNull(savedRule);
        Assert.Equal(request.WeekDay, dto.WeekDay);
        Assert.False(dto.IsClosed);
        Assert.Equal(request.Open, dto.Open);
        Assert.Equal(request.Close, dto.Close);

        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<BusinessHoursRule>(), It.IsAny<CancellationToken>()), Times.Once);
    }
   
    [Fact]
    public async Task UpdateAsync_ShouldFail_WhenRuleNotFound()
    {
        // Arrange
        var request = new UpdateBusinessHoursRuleRequest
        {
            Id = Guid.NewGuid(),
            StartDate = new DateOnly(2025, 4, 1),
            EndDate = new DateOnly(2025, 4, 30),
            WeekDay = WeekDay.Tuesday,
            Open = new TimeOnly(9, 0),
            Close = new TimeOnly(18, 0),
            IsClosed = false
        };

        _repositoryMock
            .Setup(r => r.GetByIdAsync(request.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((BusinessHoursRule?)null);

        // Act
        var result = await _service.UpdateAsync(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailed);
        Assert.Contains(result.Errors, e => e.Message == "Rule not found.");

        _repositoryMock.Verify(r => r.GetByIdAsync(request.Id, It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<BusinessHoursRule>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateRule_WhenDataIsValid()
    {
        // Arrange
        var id = Guid.NewGuid();

        var existing = new BusinessHoursRule(
            new DateOnly(2025, 4, 1),
            new DateOnly(2025, 4, 30),
            null,
            WeekDay.Monday,
            new TimeOnly(9, 0),
            new TimeOnly(18, 0),
            false);

        var request = new UpdateBusinessHoursRuleRequest
        {
            Id = id,
            StartDate = new DateOnly(2025, 4, 1),
            EndDate = new DateOnly(2025, 4, 30),
            SpecificDate = null,
            WeekDay = WeekDay.Wednesday,
            Open = new TimeOnly(10, 0),
            Close = new TimeOnly(22, 0),
            IsClosed = false
        };

        _repositoryMock
            .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        _repositoryMock
            .Setup(r => r.UpdateAsync(existing, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.UpdateAsync(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);

        _repositoryMock.Verify(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(r => r.UpdateAsync(existing, It.IsAny<CancellationToken>()), Times.Once);
    }
 
    [Fact]
    public async Task DeleteAsync_ShouldFail_WhenRuleNotFound()
    {
        // Arrange
        var id = Guid.NewGuid();

        _repositoryMock
            .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((BusinessHoursRule?)null);

        // Act
        var result = await _service.DeleteAsync(id, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailed);
        Assert.Contains(result.Errors, e => e.Message == "Rule not found.");

        _repositoryMock.Verify(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(r => r.DeleteAsync(It.IsAny<BusinessHoursRule>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_ShouldDeleteRule_WhenRuleExists()
    {
        // Arrange
        var id = Guid.NewGuid();

        var existing = BusinessHoursRule.CreateClosedDate(new DateOnly(2025, 4, 21));

        _repositoryMock
            .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        _repositoryMock
            .Setup(r => r.DeleteAsync(existing, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.DeleteAsync(id, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);

        _repositoryMock.Verify(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(r => r.DeleteAsync(existing, It.IsAny<CancellationToken>()), Times.Once);
    }
}